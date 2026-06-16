using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartModeMenu : MonoBehaviour
{
    private const string LogTag = "[GameStartModeMenu]";
    private const string GameStartSceneName = "GameStartScene";

    private const float CanvasMatchWidthOrHeight = 0.5f;
    private const float MainMenuSpacing = 14f;
    private const float MainButtonHeight = 50f;
    private const int MainButtonFontSize = 22;
    private const float CoopMenuSpacing = 9f;
    private const float CoopButtonHeight = 42f;
    private const int CoopButtonFontSize = 19;
    private const float CoopStatusLabelHeight = 56.67f;
    private const int CoopStatusLabelFontSize = 19;

    private static readonly Vector2 CanvasReferenceResolution = new Vector2(1500f, 600f);
    private static readonly Vector2 MainMenuPosition = new Vector2(0f, -120f);
    private static readonly Vector2 MainMenuSize = new Vector2(320f, 130f);
    private static readonly Vector2 CoopMenuPosition = Vector2.zero;
    private static readonly Vector2 CoopMenuSize = new Vector2(460f, 570f);

    [Header("Editable Scene UI")]
    [SerializeField] private GameObject mainPanelObject;
    [SerializeField] private GameObject coopPanelObject;
    [SerializeField] private Button singlePlayerButton;
    [SerializeField] private Button coopModeButton;
    [SerializeField] private Button joinCodeButton;
    [SerializeField] private Button autoMatchButton;
    [SerializeField] private Button backButton;
    [SerializeField] private InputField roomCodeInput;
    [SerializeField] private Text statusText;
    
    [Header("Crate Button for Game Mode")]
    [SerializeField] private GameObject singleCrateButton;
    [SerializeField] private GameObject coopCrateButton;
    

    [Header("Optional Style Override")]
    [SerializeField] private Font fontOverride;

    [Header("Runtime Canvas Camera")]
    [SerializeField] private Camera uiCamera;

    private Canvas _canvas;
    private GameObject _mainPanel;
    private GameObject _coopPanel;
    private InputField _roomCodeInput;
    private Text _statusText;
    private UgsCoopClient _coopClient;
    private CanvasScaler _canvasScaler;

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

        _coopClient = UgsCoopClient.EnsureInstance();
        _coopClient.OnStatusChanged += HandleCoopStatusChanged;

        if (HasEditableSceneUi())
        {
            BindEditableSceneUi();
        }
        else
        {
            BuildRuntimeFallbackUi();
        }
        _mainPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_coopClient != null)
        {
            _coopClient.OnStatusChanged -= HandleCoopStatusChanged;
        }
    }

    private bool HasEditableSceneUi()
    {
        return mainPanelObject != null
            && coopPanelObject != null
            && singlePlayerButton != null
            && coopModeButton != null
            && joinCodeButton != null
            && autoMatchButton != null
            && backButton != null;
    }

    private void BindEditableSceneUi()
    {
        EnsureEventSystem();

        _mainPanel = mainPanelObject;
        _coopPanel = coopPanelObject;
        _roomCodeInput = roomCodeInput;
        _statusText = statusText;

        BindButton(singlePlayerButton, OnSinglePlayerClicked);
        BindButton(coopModeButton, OnCoopClicked);
        BindButton(joinCodeButton, OnJoinCodeClicked);
        BindButton(autoMatchButton, OnAutoMatchClicked);
        BindButton(backButton, OnBackClicked);

        _mainPanel.SetActive(false);
        _coopPanel.SetActive(false);
        ToggleCrateButtons(true);
        
        ApplyFontOverride();
        SetStatus("Start your dedicated server first, then join by code or auto match.");

        Debug.Log($"{LogTag} Editable scene UI bound in GameStartScene.");
    }

    private static void BindButton(Button button, UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private void BuildRuntimeFallbackUi()
    {
        EnsureEventSystem();

        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceCamera;
        _canvas.worldCamera = uiCamera != null ? uiCamera : Camera.main;
        if (_canvas.worldCamera == null)
        {
            Debug.LogWarning($"{LogTag} Runtime Canvas is Screen Space - Camera, but no UI Camera was assigned and no MainCamera was found.");
        }

        _canvas.sortingOrder = 5000;

        _canvasScaler = gameObject.AddComponent<CanvasScaler>();
        _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        _canvasScaler.referenceResolution = CanvasReferenceResolution;
        _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        _canvasScaler.matchWidthOrHeight = CanvasMatchWidthOrHeight;

        gameObject.AddComponent<GraphicRaycaster>();
        
        RectTransform canvasTransform = gameObject.GetComponent<RectTransform>();
        canvasTransform.anchorMin = Vector2.zero;
        canvasTransform.anchorMax = Vector2.one;
        canvasTransform.offsetMin = Vector2.zero;
        canvasTransform.offsetMax = Vector2.zero;
        

        GameObject backdrop = CreateUiObject("Backdrop", transform);
        Image backdropImage = backdrop.AddComponent<Image>();
        backdropImage.color = new Color(1f, 1f, 1f, 0f);
        Stretch(backdrop.GetComponent<RectTransform>());

        _mainPanel = CreatePanel("ModePanel", transform, MainMenuSize, MainMenuPosition, new RectOffset(0, 0, 0, 0), MainMenuSpacing, false);
        CreateButton(_mainPanel.transform, "Single Player", OnSinglePlayerClicked, MainButtonHeight, MainButtonFontSize);
        CreateButton(_mainPanel.transform, "Co-op Mode", OnCoopClicked, MainButtonHeight, MainButtonFontSize);

        _coopPanel = CreatePanel("CoopPanel", transform, CoopMenuSize, CoopMenuPosition, new RectOffset(26, 26, 24, 24), CoopMenuSpacing, true);
        _coopPanel.SetActive(false);
        CreateTitle(_coopPanel.transform, "Co-op Mode", 28, 42f);
        _roomCodeInput = CreateInput(_coopPanel.transform, "Room Code", 42f, 19);
        CreateButton(_coopPanel.transform, "Join Code", OnJoinCodeClicked, CoopButtonHeight, CoopButtonFontSize);
        CreateButton(_coopPanel.transform, "Auto Match", OnAutoMatchClicked, CoopButtonHeight, CoopButtonFontSize);
        CreateButton(_coopPanel.transform, "Back", OnBackClicked, 40f, 18, new Color(0.24f, 0.27f, 0.3f, 1f));
        _statusText = CreateLabel(_coopPanel.transform, "Start your dedicated server first, then join by code or auto match.", CoopStatusLabelFontSize, TextAnchor.MiddleCenter);
        _statusText.color = new Color(0.92f, 0.97f, 1f, 1f);
        LayoutElement statusLayout = _statusText.gameObject.AddComponent<LayoutElement>();
        statusLayout.preferredHeight = CoopStatusLabelHeight;
        statusLayout.flexibleHeight = 0f;
        ApplyFontOverride();

        Debug.Log($"{LogTag} Runtime mode selection UI created in GameStartScene.");
    }

    private void ApplyFontOverride()
    {
        if (fontOverride == null)
        {
            return;
        }

        ApplyFontOverride(_mainPanel);
        ApplyFontOverride(_coopPanel);

        if (_statusText != null)
        {
            _statusText.font = fontOverride;
        }

        if (_roomCodeInput != null)
        {
            if (_roomCodeInput.textComponent != null)
            {
                _roomCodeInput.textComponent.font = fontOverride;
            }

            if (_roomCodeInput.placeholder is Text placeholderText)
            {
                placeholderText.font = fontOverride;
            }
        }
    }

    private void ApplyFontOverride(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        Text[] texts = root.GetComponentsInChildren<Text>(true);
        foreach (Text text in texts)
        {
            text.font = fontOverride;
        }
    }

    private void OnSinglePlayerClicked()
    {
        // _mainPanel.SetActive(false);
        ToggleCrateButtons(false);
        _coopPanel.SetActive(false);

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

    public void SelectSinglePlayer()
    {
        OnSinglePlayerClicked();
    }

    private void OnCoopClicked()
    {
        ToggleCrateButtons(false);
        
        // _mainPanel.SetActive(false);
        _coopPanel.SetActive(true);
        SetStatus("Co-op selected. Connect to an existing dedicated server session.");
    }

    public void OpenCoopMenu()
    {
        OnCoopClicked();
    }

    private void OnJoinCodeClicked()
    {
        _coopClient.JoinByCode(_roomCodeInput != null ? _roomCodeInput.text : string.Empty);
    }

    public void JoinByCode()
    {
        OnJoinCodeClicked();
    }

    private void OnAutoMatchClicked()
    {
        _coopClient.AutoMatchDedicatedServer();
    }

    public void AutoMatch()
    {
        OnAutoMatchClicked();
    }

    private void OnBackClicked()
    {
        _coopPanel.SetActive(false);
        // _mainPanel.SetActive(true);
        ToggleCrateButtons(true);
    }

    public void BackToMainMenu()
    {
        OnBackClicked();
    }

    private void HandleCoopStatusChanged(string message)
    {
        SetStatus(message);
    }

    private void SetStatus(string message)
    {
        if (_statusText != null)
        {
            _statusText.text = message;
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

    private static GameObject CreatePanel(string name, Transform parent, Vector2 size, Vector2 position, RectOffset padding, float spacing, bool showBackground)
    {
        GameObject panel = CreateUiObject(name, parent);
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = position;

        if (showBackground)
        {
            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.08f, 0.1f, 0.12f, 0.92f);
        }

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = padding;
        layout.spacing = spacing;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        return panel;
    }

    private static void CreateTitle(Transform parent, string text, int fontSize, float preferredHeight)
    {
        Text title = CreateLabel(parent, text, fontSize, TextAnchor.MiddleCenter);
        title.fontStyle = FontStyle.Bold;
        title.color = Color.white;
        LayoutElement layoutElement = title.gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = preferredHeight;
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

    private static Button CreateButton(Transform parent, string label, UnityAction onClick, float preferredHeight, int fontSize, Color? color = null)
    {
        GameObject buttonObject = CreateUiObject(label.Replace(" ", string.Empty) + "Button", parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = color ?? new Color(0.95f, 0.35f, 0.18f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = preferredHeight;

        Text buttonText = CreateLabel(buttonObject.transform, label, fontSize, TextAnchor.MiddleCenter);
        buttonText.color = Color.white;
        Stretch(buttonText.GetComponent<RectTransform>());

        return button;
    }

    private static InputField CreateInput(Transform parent, string placeholderText, float preferredHeight, int fontSize)
    {
        GameObject inputObject = CreateUiObject("RoomCodeInput", parent);
        Image image = inputObject.AddComponent<Image>();
        image.color = Color.white;

        LayoutElement layoutElement = inputObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = preferredHeight;

        InputField inputField = inputObject.AddComponent<InputField>();
        inputField.characterLimit = 24;
        inputField.lineType = InputField.LineType.SingleLine;

        Text text = CreateLabel(inputObject.transform, string.Empty, fontSize, TextAnchor.MiddleLeft);
        text.color = Color.black;
        RectTransform textRect = text.GetComponent<RectTransform>();
        Stretch(textRect);
        textRect.offsetMin = new Vector2(12f, 0f);
        textRect.offsetMax = new Vector2(-12f, 0f);

        Text placeholder = CreateLabel(inputObject.transform, placeholderText, fontSize, TextAnchor.MiddleLeft);
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

    private void ToggleCrateButtons(bool toggle)
    {
        ToggleGameObject(singleCrateButton, toggle);
        ToggleGameObject(coopCrateButton, toggle);
    }

    private static void ToggleGameObject(GameObject target, bool toggle)
    {
        if (target == null)
        {
            return;
        }

        target.SetActive(toggle);
    }
}
