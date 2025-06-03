using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HoveringText : MonoBehaviour
{
    public float amplitude = 10f;        // 움직이는 높이
    public float frequency = 1f;         // 움직이는 속도
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float newY = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.localPosition = startPos + new Vector3(0f, newY, 0f);
    }
}