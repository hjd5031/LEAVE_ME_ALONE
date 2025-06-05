using UnityEngine;

public class HoverDrone : MonoBehaviour
{
    public float hoverAmplitude = 0.2f;      // 호버링 높이
    public float hoverSpeed = 2f;            // 호버링 속도
    public float lookSmoothSpeed = 5f;       // 회전 부드럽게 만드는 속도

    private Vector3 initialPosition;         // 시작 위치 저장
    private Transform targetTransform;       // 바라볼 대상 (플레이어)

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        //호버링 (y값을 사인 함수로 위아래 움직임)
        float offsetY = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        transform.position = initialPosition + new Vector3(0f, offsetY, 0f);

    }
}