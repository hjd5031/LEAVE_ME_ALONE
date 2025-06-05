using TMPro;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light directionalLight; // Directional Light 참조
    
    public float dayDuration = 300f; // 총 5분
    public float stableDuration = 180f; // 밝기 유지 구간 (3분)
    public float nightIntensity = 0.05f;
    
    private float _elapsedTime = 0f;
    private float _startIntensity;
    

    void Start()
    {
        if (directionalLight == null)
            directionalLight = GetComponent<Light>();

        _startIntensity = directionalLight.intensity;
    }

    void Update()
    {
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime <= stableDuration)
        {
            // 초기 밝기 유지
            directionalLight.intensity = _startIntensity;
        }
        else
        {
            // 점차 밤으로 전환 (3분 이후 ~ 5분까지)
            float fadeTime = _elapsedTime - stableDuration;
            float fadeDuration = dayDuration - stableDuration;

            float t = Mathf.Clamp01(fadeTime / fadeDuration); // 0~1로 전환 비율
            directionalLight.intensity = Mathf.Lerp(_startIntensity, nightIntensity, t);
        }
    }
}