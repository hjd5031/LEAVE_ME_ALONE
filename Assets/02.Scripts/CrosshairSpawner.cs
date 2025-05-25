using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairSpawner : MonoBehaviour
{
    void Start()
    {
        // 1. Canvas 생성
        GameObject canvasGO = new GameObject("CrosshairCanvas");
        canvasGO.layer = LayerMask.NameToLayer("UI");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // 2. TextMeshProUGUI 생성
        GameObject textGO = new GameObject("CrosshairText");
        textGO.transform.SetParent(canvasGO.transform);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "+";
        tmp.fontSize = 36;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        // 3. 위치 중앙 정렬
        RectTransform rectTransform = tmp.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(50, 50);
    }
}