using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartModeMenu : MonoBehaviour
{
    private const string LogTag = "[GameStartModeMenu]";
    private const string GameStartSceneName = "GameStartScene";

    private Canvas canvas;
    private GameObject mainPanel;
    private GameObject coopPanel;
    private InputField roomCodeInput;
    private Text statusText;
    private UgsCoopClient coopClient;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
#if !UNITY_SERVER
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        TryCreateForScene(SceneManager.GetActiveScene());
#endif
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryCreateForScene(scene);
    }

    private static void TryCreateForScene(Scene scene)
    {
        if (scene.name != GameStartSceneName)
        {
            return;
        }

        if (FindFirstObjectByType<GameStartModeMenu>() != null)
        {
            return;
        }

        GameObject menuObject = new GameObject(nameof(GameStartModeMenu));
        menuObject.AddComponent<GameStartModeMenu>();
    }

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name != GameStartSceneName)
        {
            Destroy(gameObject);
            return;
        }

        coopClient = UgsCoopClient.EnsureInstance();
        coopClient.OnStatusChanged += HandleCoopStatusChanged;
        BuildUi();
    }

    private void OnDestroy()
    {
        if (coopClient != null)
        {
            coopClient.OnStatusChanged -= HandleCoopStatusChanged;
        }
    }

    private void BuildUi()
    {
        EnsureEventSystem();

        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

        RectTransform canvasTransform = gameObject.GetComponent<RectTransform>();
        canvasTransform.anchorMin = Vector2.zero;
        canvasTransform.anchorMax = Vector2.one;
        canvasTransform.offsetMin = Vector2.zero;
        canvasTransform.offsetMax = Vector2.zero;

        GameObject backdrop = CreateUiObject("Backdrop", transform);
        Image backdropImage = backdrop.AddComponent<Image>();
        backdropImage.color = new Color(0f, 0f, 0f, 0.35f);
        Stretch(backdrop.GetComponent<RectTransform>());

        mainPanel = CreatePanel("ModePanel", transform);
        CreateTitle(mainPanel.transform, "Tomato Farming");
        CreateButton(mainPanel.transform, "Single Player", OnSinglePlayerClicked);
        CreateButton(mainPanel.transform, "Co-op Mode", OnCoopClicked);

        coopPanel = CreatePanel("CoopPanel", transform);
        coopPanel.SetActive(false);
        CreateTitle(coopPanel.transform, "Co-op Mode");
        roomCodeInput = CreateInput(coopPanel.transform, "Room Code");
        CreateButton(coopPanel.transform, "Join Code", OnJoinCodeClicked);
        CreateButton(coopPanel.transform, "Auto Match", OnAutoMatchClicked);
        CreateButton(coopPanel.transform, "Back", OnBackClicked);
        statusText = CreateLabel(coopPanel.transform, "Start your dedicated server first, then join by code or auto match.", 18, TextAnchor.MiddleCenter);
        statusText.color = Color.white;

        Debug.Log($"{LogTag} Runtime mode selection UI created in GameStartScene.");
    }

    private void OnSinglePlayerClicked()
    {
        mainPanel.SetActive(false);
        coopPanel.SetActive(false);

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.BeginSinglePlayerMode();
            Debug.Log($"{LogTag} Single Player selected. Existing GameManager flow enabled.");
            return;
        }

        Debug.LogWarning($"{LogTag} GameManager was not found. Loading MainGameScene directly as fallback.");
        SceneManager.LoadScene("MainGameScene");
    }

    private void OnCoopClicked()
    {
        mainPanel.SetActive(false);
        coopPanel.SetActive(true);
        SetStatus("Co-op selected. Connect to an existing dedicated server session.");
    }

    private void OnJoinCodeClicked()
    {
        coopClient.JoinByCode(roomCodeInput != null ? roomCodeInput.text : string.Empty);
    }

    private void OnAutoMatchClicked()
    {
        coopClient.AutoMatchDedicatedServer();
    }

    private void OnBackClicked()
    {
        coopPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    private void HandleCoopStatusChanged(string message)
    {
        SetStatus(message);
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private static GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = CreateUiObject(name, parent);
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(420f, 360f);
        rectTransform.anchoredPosition = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.08f, 0.1f, 0.12f, 0.92f);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(28, 28, 28, 28);
        layout.spacing = 12f;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        return panel;
    }

    private static void CreateTitle(Transform parent, string text)
    {
        Text title = CreateLabel(parent, text, 32, TextAnchor.MiddleCenter);
        title.fontStyle = FontStyle.Bold;
        title.color = Color.white;
        LayoutElement layoutElement = title.gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 52f;
    }

    private static Text CreateLabel(Transform parent, string text, int fontSize, TextAnchor alignment)
    {
        GameObject labelObject = CreateUiObject("Label", parent);
        Text label = labelObject.AddComponent<Text>();
        label.text = text;
        label.font = GetDefaultFont();
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        return label;
    }

    private static Button CreateButton(Transform parent, string label, UnityAction onClick)
    {
        GameObject buttonObject = CreateUiObject(label.Replace(" ", string.Empty) + "Button", parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.95f, 0.35f, 0.18f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 52f;

        Text buttonText = CreateLabel(buttonObject.transform, label, 22, TextAnchor.MiddleCenter);
        buttonText.color = Color.white;
        Stretch(buttonText.GetComponent<RectTransform>());

        return button;
    }

    private static InputField CreateInput(Transform parent, string placeholderText)
    {
        GameObject inputObject = CreateUiObject("RoomCodeInput", parent);
        Image image = inputObject.AddComponent<Image>();
        image.color = Color.white;

        LayoutElement layoutElement = inputObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 46f;

        InputField inputField = inputObject.AddComponent<InputField>();
        inputField.characterLimit = 24;
        inputField.lineType = InputField.LineType.SingleLine;

        Text text = CreateLabel(inputObject.transform, string.Empty, 20, TextAnchor.MiddleLeft);
        text.color = Color.black;
        RectTransform textRect = text.GetComponent<RectTransform>();
        Stretch(textRect);
        textRect.offsetMin = new Vector2(12f, 0f);
        textRect.offsetMax = new Vector2(-12f, 0f);

        Text placeholder = CreateLabel(inputObject.transform, placeholderText, 20, TextAnchor.MiddleLeft);
        placeholder.color = new Color(0.45f, 0.45f, 0.45f, 1f);
        RectTransform placeholderRect = placeholder.GetComponent<RectTransform>();
        Stretch(placeholderRect);
        placeholderRect.offsetMin = new Vector2(12f, 0f);
        placeholderRect.offsetMax = new Vector2(-12f, 0f);

        inputField.textComponent = text;
        inputField.placeholder = placeholder;
        return inputField;
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private static Font GetDefaultFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return font;
    }
}
