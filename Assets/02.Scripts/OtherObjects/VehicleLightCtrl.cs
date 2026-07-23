using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class VehicleLightCtrl : MonoBehaviour
{
    public Material material;
    public GameObject[] lights;
    public GameObject mudParticle;
    public Color emissionColor = Color.white; // 기본 발광 색상
    public int flickerCount = 6;              // 총 깜빡임 횟수 (짝수면 false로 끝나므로 5나 7 추천)
    public GameObject introCamera;
    public GameObject followCamera;
    private GameObject crossHair;

    [Header("Picture in Picture")]
    [SerializeField] private Vector2 pipSize = new Vector2(400f, 225f);
    [SerializeField] private Vector2 pipTopRightMargin = new Vector2(24f, 24f);
    [SerializeField, Range(0f, 16f)] private float pipBorderWidth = 4f;
    [SerializeField] private Color pipBorderColor = new Color(0.05f, 0.05f, 0.05f, 0.95f);

    private VehiclePipCamera pipCamera;
    
    
    private String CarEngineSoundID;
    private String CarAccelerationSoundID;
    private String CarHornID;
    // public AudioSource CarHorn;

    void Awake()
    {
        SetLegacyCamerasActive(false);
    }

    void Start()
    {
        SetEmission(false);
        crossHair = GameObject.FindWithTag("crossHair");
        CarEngineSoundID = SoundManager.Instance.Play3DSfx(SoundManager.Sfx.EngineStart,transform,1f);

        pipCamera = GetComponent<VehiclePipCamera>();
        if (pipCamera == null)
        {
            pipCamera = gameObject.AddComponent<VehiclePipCamera>();
        }

        pipCamera.Configure(
            introCamera,
            followCamera,
            pipSize,
            pipTopRightMargin,
            pipBorderWidth,
            pipBorderColor);
        pipCamera.ShowIntro();

        mudParticle.SetActive(false);
        StartCoroutine(FlickerSequence());
        // Invoke(nameof(TurnOffCameras),15f);
    }

    IEnumerator FlickerSequence()
    {
        yield return new WaitForSeconds(1f); // ⏳ 3초 대기
        CarHornID = SoundManager.Instance.Play3DSfx(SoundManager.Sfx.CarHorn,transform,1f);
        for (int i = 0; i < flickerCount; i++)
        {
            bool on = i % 2 == 0;

            SetLightsActive(on);
            SetEmission(on);
            // if (on) SoundManager.Instance.Play3DSfx(SoundManager.Sfx.CarHorn,transform,1f);
            yield return new WaitForSeconds(0.4f);
        }
        CarAccelerationSoundID = SoundManager.Instance.Play3DSfx(SoundManager.Sfx.CarAcceleration,transform,1f);
        // SoundManager.Instance.Play3DSfx(SoundManager.Sfx.CarHorn,transform,1f);
        // 마지막은 항상 true로 고정
        
        pipCamera.ShowFollow();
        mudParticle.SetActive(true);
        SetLightsActive(true);
        SetEmission(true);
        
        yield return new WaitForSeconds(1f);
        SoundManager.Instance.StopSfx(CarHornID);
    }

    void TurnOffCameras()
    {
        SetLegacyCamerasActive(false);
        if (pipCamera != null)
        {
            pipCamera.Hide();
        }

        SoundManager.Instance.StopSfx(CarAccelerationSoundID);
        if (GameManager.Instance.PLayerUsingItem)
        {
            GameManager.Instance.PLayerUsingItem = false;
            return;
        }

        GameManager.Instance.EnemyUsingItem = false;
    }

    private void SetLegacyCamerasActive(bool isActive)
    {
        if (introCamera != null)
        {
            introCamera.SetActive(isActive);
        }

        if (followCamera != null)
        {
            followCamera.SetActive(isActive);
        }
    }
    void SetLightsActive(bool isOn)
    {
        foreach (GameObject obj in lights)
        {
            if (obj != null)
                obj.SetActive(isOn);
        }
    }

    void SetEmission(bool isOn)
    {
        if (material != null)
        {
            if (isOn)
            {
                material.EnableKeyword("_EMISSION");
            }
            else
            {
                material.DisableKeyword("_EMISSION");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnterVehicleLightCtrl");
        Invoke(nameof(TurnOffCameras),2f);
    }
}

[DisallowMultipleComponent]
public sealed class VehiclePipCamera : MonoBehaviour
{
    private const string LogTag = "[VehiclePIP]";
    private const int TextureWidth = 640;
    private const int TextureHeight = 360;
    private const int CanvasSortingOrder = 30000;
    private const OutputChannels PipChannel = OutputChannels.Channel15;

    private readonly Dictionary<CinemachineBrain, OutputChannels> originalBrainMasks = new();

    private GameObject introCamera;
    private GameObject followCamera;
    private GameObject activeVirtualCamera;
    private GameObject pipCameraObject;
    private GameObject pipCanvasObject;
    private Camera pipCamera;
    private CinemachineBrain pipBrain;
    private RenderTexture renderTexture;
    private bool isConfigured;

    public void Configure(
        GameObject intro,
        GameObject follow,
        Vector2 displaySize,
        Vector2 topRightMargin,
        float borderWidth,
        Color borderColor)
    {
#if UNITY_SERVER
        return;
#else
        introCamera = intro;
        followCamera = follow;

        PrepareVirtualCamera(introCamera);
        PrepareVirtualCamera(followCamera);

        if (!CreateRenderCamera())
        {
            return;
        }

        CreateOverlay(displaySize, topRightMargin, borderWidth, borderColor);
        isConfigured = true;
#endif
    }

    public void ShowIntro()
    {
        Show(introCamera);
    }

    public void ShowFollow()
    {
        Show(followCamera);
    }

    public void Hide()
    {
        SetVirtualCameraActive(null);

        if (pipCamera != null)
        {
            pipCamera.enabled = false;
        }

        if (pipCanvasObject != null)
        {
            pipCanvasObject.SetActive(false);
        }

        RestoreMainBrainMasks();
    }

    private void Show(GameObject virtualCamera)
    {
        if (!isConfigured || virtualCamera == null)
        {
            return;
        }

        ExcludePipChannelFromMainBrains();
        SetVirtualCameraActive(virtualCamera);

        pipCamera.enabled = true;
        pipCanvasObject.SetActive(true);
    }

    private static void PrepareVirtualCamera(GameObject virtualCameraObject)
    {
        if (virtualCameraObject == null)
        {
            return;
        }

        CinemachineCamera virtualCamera = virtualCameraObject.GetComponent<CinemachineCamera>();
        if (virtualCamera != null)
        {
            virtualCamera.OutputChannel = PipChannel;
        }

        virtualCameraObject.SetActive(false);
    }

    private bool CreateRenderCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (cameras.Length > 0)
            {
                mainCamera = cameras[0];
            }
        }

        if (mainCamera == null)
        {
            Debug.LogWarning($"{LogTag} Cannot create PIP because no gameplay camera was found.");
            return false;
        }

        renderTexture = new RenderTexture(TextureWidth, TextureHeight, 24, RenderTextureFormat.ARGB32)
        {
            name = $"VehiclePIP_{GetInstanceID()}",
            antiAliasing = 1,
            useMipMap = false,
            autoGenerateMips = false
        };
        renderTexture.Create();

        pipCameraObject = new GameObject($"Vehicle PIP Camera ({name})");
        pipCamera = pipCameraObject.AddComponent<Camera>();
        pipCamera.CopyFrom(mainCamera);
        pipCamera.targetTexture = renderTexture;
        pipCamera.rect = new Rect(0f, 0f, 1f, 1f);
        pipCamera.depth = mainCamera.depth + 1f;
        pipCamera.allowHDR = false;
        pipCamera.allowMSAA = false;

        UniversalAdditionalCameraData cameraData = pipCamera.GetUniversalAdditionalCameraData();
        cameraData.renderType = CameraRenderType.Base;
        cameraData.renderPostProcessing = false;
        cameraData.antialiasing = AntialiasingMode.None;

        pipBrain = pipCameraObject.AddComponent<CinemachineBrain>();
        pipBrain.ChannelMask = PipChannel;
        pipCamera.enabled = false;
        return true;
    }

    private void CreateOverlay(
        Vector2 displaySize,
        Vector2 topRightMargin,
        float borderWidth,
        Color borderColor)
    {
        pipCanvasObject = new GameObject($"Vehicle PIP Canvas ({name})", typeof(RectTransform));

        Canvas canvas = pipCanvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = CanvasSortingOrder;

        CanvasScaler scaler = pipCanvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject frameObject = new GameObject("Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        frameObject.transform.SetParent(pipCanvasObject.transform, false);

        RectTransform frameRect = (RectTransform)frameObject.transform;
        frameRect.anchorMin = Vector2.one;
        frameRect.anchorMax = Vector2.one;
        frameRect.pivot = Vector2.one;
        frameRect.anchoredPosition = new Vector2(-topRightMargin.x, -topRightMargin.y);
        frameRect.sizeDelta = displaySize + Vector2.one * (borderWidth * 2f);

        Image frameImage = frameObject.GetComponent<Image>();
        frameImage.color = borderColor;
        frameImage.raycastTarget = false;

        GameObject imageObject = new GameObject("Feed", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        imageObject.transform.SetParent(frameObject.transform, false);

        RectTransform imageRect = (RectTransform)imageObject.transform;
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.one * borderWidth;
        imageRect.offsetMax = -Vector2.one * borderWidth;

        RawImage rawImage = imageObject.GetComponent<RawImage>();
        rawImage.texture = renderTexture;
        rawImage.raycastTarget = false;

        pipCanvasObject.SetActive(false);
    }

    private void SetVirtualCameraActive(GameObject virtualCamera)
    {
        if (activeVirtualCamera != null)
        {
            activeVirtualCamera.SetActive(false);
        }

        activeVirtualCamera = virtualCamera;
        if (activeVirtualCamera != null)
        {
            activeVirtualCamera.SetActive(true);
        }
    }

    private void ExcludePipChannelFromMainBrains()
    {
        CinemachineBrain[] brains =
            FindObjectsByType<CinemachineBrain>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (CinemachineBrain brain in brains)
        {
            if (brain == null || brain == pipBrain || originalBrainMasks.ContainsKey(brain))
            {
                continue;
            }

            originalBrainMasks.Add(brain, brain.ChannelMask);
            brain.ChannelMask &= ~PipChannel;

            if (brain.ChannelMask == 0)
            {
                brain.ChannelMask = OutputChannels.Default;
            }
        }
    }

    private void RestoreMainBrainMasks()
    {
        foreach (KeyValuePair<CinemachineBrain, OutputChannels> entry in originalBrainMasks)
        {
            if (entry.Key != null)
            {
                entry.Key.ChannelMask = entry.Value;
            }
        }

        originalBrainMasks.Clear();
    }

    private void OnDisable()
    {
        Hide();
    }

    private void OnDestroy()
    {
        RestoreMainBrainMasks();

        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }

        if (pipCameraObject != null)
        {
            Destroy(pipCameraObject);
        }

        if (pipCanvasObject != null)
        {
            Destroy(pipCanvasObject);
        }
    }
}
