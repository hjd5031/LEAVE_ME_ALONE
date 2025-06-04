using TMPro;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light directionalLight; // Directional Light 참조
    public float dayDuration = 300f; // 총 5분
    public float stableDuration = 180f; // 밝기 유지 구간 (3분)
    private float elapsedTime = 0f;

    private float startIntensity;
    public float nightIntensity = 0.05f;
    public TextMeshProUGUI timeText;

    void Start()
    {
        if (directionalLight == null)
            directionalLight = GetComponent<Light>();

        startIntensity = directionalLight.intensity;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime <= stableDuration)
        {
            // 초기 밝기 유지
            directionalLight.intensity = startIntensity;
        }
        else
        {
            // 점차 밤으로 전환 (3분 이후 ~ 5분까지)
            float fadeTime = elapsedTime - stableDuration;
            float fadeDuration = dayDuration - stableDuration;

            float t = Mathf.Clamp01(fadeTime / fadeDuration); // 0~1로 전환 비율
            directionalLight.intensity = Mathf.Lerp(startIntensity, nightIntensity, t);
        }
    }

    void OnGUI()
    {
        // // 경과 시간 MM:SS로 표시
        // int currentTime = Mathf.Clamp((int)elapsedTime, 0, (int)dayDuration);
        // int minutes = currentTime / 60;
        // int seconds = currentTime % 60;
        //
        // string timeStr = $"{minutes:D2}:{seconds:D2}";
        //
        // GUIStyle style = new GUIStyle(GUI.skin.label);
        // style.fontSize = 32;
        // style.alignment = TextAnchor.UpperCenter;
        // style.normal.textColor = Color.white;
        //
        // GUI.Label(new Rect(0, 10, Screen.width, 50), $"Time: {timeStr}", style);
    }
}