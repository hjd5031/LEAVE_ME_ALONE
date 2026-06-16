using UnityEngine;
using UnityEngine.UI;

public sealed class PipBroadcastView : MonoBehaviour
{
    public enum ScreenCorner
    {
        TopRight,
        TopLeft,
        BottomRight,
        BottomLeft
    }

    public static PipBroadcastView Instance { get; private set; }

    [SerializeField] private int textureWidth = 640;
    [SerializeField] private int textureHeight = 360;
    [SerializeField] private Vector2 panelSize = new Vector2(360f, 203f);
    [SerializeField] private Vector2 screenMargin = new Vector2(24f, 24f);

    private Transform source;
    private Camera pipCamera;
    private RenderTexture renderTexture;
    private GameObject canvasObject;
    private RectTransform panelRect;
    private RawImage rawImage;

    public static void Show(Transform cameraSource, ScreenCorner corner = ScreenCorner.TopRight)
    {
        if (cameraSource == null)
        {
            Debug.LogWarning("[PipBroadcastView] Cannot show PIP without a camera source.");
            return;
        }

        EnsureInstance().ShowInternal(cameraSource, corner);
    }

    public static void Hide(Transform expectedSource = null)
    {
        if (Instance == null)
            return;

        Instance.HideInternal(expectedSource);
    }

    private static PipBroadcastView EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        Instance = FindFirstObjectByType<PipBroadcastView>();
        if (Instance != null)
            return Instance;

        GameObject viewObject = new GameObject("PIP Broadcast View");
        Instance = viewObject.AddComponent<PipBroadcastView>();
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void LateUpdate()
    {
        if (source == null || pipCamera == null || !pipCamera.enabled)
            return;

        pipCamera.transform.SetPositionAndRotation(source.position, source.rotation);
    }

    private void ShowInternal(Transform cameraSource, ScreenCorner corner)
    {
        EnsureResources();

        source = cameraSource;
        ApplyCorner(corner);

        canvasObject.SetActive(true);
        panelRect.gameObject.SetActive(true);
        pipCamera.enabled = true;
        pipCamera.transform.SetPositionAndRotation(source.position, source.rotation);

        Debug.Log("[PipBroadcastView] Showing broadcast PIP.");
    }

    private void HideInternal(Transform expectedSource)
    {
        if (expectedSource != null && source != expectedSource)
            return;

        source = null;

        if (pipCamera != null)
            pipCamera.enabled = false;

        if (panelRect != null)
            panelRect.gameObject.SetActive(false);

        Debug.Log("[PipBroadcastView] Hiding broadcast PIP.");
    }

    private void EnsureResources()
    {
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(textureWidth, textureHeight, 16, RenderTextureFormat.ARGB32)
            {
                name = "PIP Broadcast RenderTexture",
                antiAliasing = 1
            };
            renderTexture.Create();
        }

        if (pipCamera == null)
        {
            GameObject cameraObject = new GameObject("PIP Broadcast Camera");
            cameraObject.transform.SetParent(transform, false);

            pipCamera = cameraObject.AddComponent<Camera>();
            pipCamera.enabled = false;
            pipCamera.targetTexture = renderTexture;
            pipCamera.fieldOfView = 55f;
            pipCamera.nearClipPlane = 0.1f;
            pipCamera.farClipPlane = 1000f;
            pipCamera.clearFlags = CameraClearFlags.Skybox;
            pipCamera.allowHDR = true;
            pipCamera.allowMSAA = false;
            pipCamera.depth = -100f;
        }

        if (canvasObject == null)
        {
            canvasObject = new GameObject("PIP Broadcast Canvas");
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject panelObject = new GameObject("PIP Panel");
            panelObject.transform.SetParent(canvasObject.transform, false);
            panelRect = panelObject.AddComponent<RectTransform>();
            panelRect.sizeDelta = panelSize;

            Image panelBackground = panelObject.AddComponent<Image>();
            panelBackground.color = new Color(0f, 0f, 0f, 0.75f);

            GameObject imageObject = new GameObject("PIP Image");
            imageObject.transform.SetParent(panelObject.transform, false);
            RectTransform imageRect = imageObject.AddComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.offsetMin = new Vector2(4f, 4f);
            imageRect.offsetMax = new Vector2(-4f, -4f);

            rawImage = imageObject.AddComponent<RawImage>();
            rawImage.texture = renderTexture;
            rawImage.color = Color.white;

            panelObject.SetActive(false);
        }
        else if (rawImage != null && rawImage.texture == null)
        {
            rawImage.texture = renderTexture;
        }
    }

    private void ApplyCorner(ScreenCorner corner)
    {
        if (panelRect == null)
            return;

        switch (corner)
        {
            case ScreenCorner.TopLeft:
                panelRect.anchorMin = new Vector2(0f, 1f);
                panelRect.anchorMax = new Vector2(0f, 1f);
                panelRect.pivot = new Vector2(0f, 1f);
                panelRect.anchoredPosition = new Vector2(screenMargin.x, -screenMargin.y);
                break;
            case ScreenCorner.BottomRight:
                panelRect.anchorMin = new Vector2(1f, 0f);
                panelRect.anchorMax = new Vector2(1f, 0f);
                panelRect.pivot = new Vector2(1f, 0f);
                panelRect.anchoredPosition = new Vector2(-screenMargin.x, screenMargin.y);
                break;
            case ScreenCorner.BottomLeft:
                panelRect.anchorMin = new Vector2(0f, 0f);
                panelRect.anchorMax = new Vector2(0f, 0f);
                panelRect.pivot = new Vector2(0f, 0f);
                panelRect.anchoredPosition = screenMargin;
                break;
            default:
                panelRect.anchorMin = new Vector2(1f, 1f);
                panelRect.anchorMax = new Vector2(1f, 1f);
                panelRect.pivot = new Vector2(1f, 1f);
                panelRect.anchoredPosition = new Vector2(-screenMargin.x, -screenMargin.y);
                break;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (renderTexture == null)
            return;

        renderTexture.Release();
        Destroy(renderTexture);
    }
}
