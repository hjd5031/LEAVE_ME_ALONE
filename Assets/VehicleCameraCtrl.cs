using UnityEngine;
using Unity.Cinemachine;

public class VehicleCameraController : MonoBehaviour
{
    public GameObject introCamera;   // 정면 카메라
    public GameObject followCamera;  // 추적 카메라

    public float startDelay = 2f;           // 전환까지 대기 시간
    public Rigidbody carRigidbody;          // 차량 Rigidbody (속도 감지용)

    private bool hasStarted = false;

    void Start()
    {
        // 처음엔 정면 카메라 활성화, 따라가기 비활성화
        introCamera.SetActive(true);
        followCamera.SetActive(false);

        // 일정 시간 후 Follow로 전환 (혹은 움직임 감지로 대체 가능)
        Invoke(nameof(SwitchToFollowCamera), startDelay);
    }

    void SwitchToFollowCamera()
    {
        if (carRigidbody != null && carRigidbody.linearVelocity.magnitude > 0.1f)
        {
            introCamera.SetActive(false);
            followCamera.SetActive(true);
            hasStarted = true;
        }
        else
        {
            // 아직 멈춰있으면 계속 감시
            Invoke(nameof(SwitchToFollowCamera), 0.1f);
        }
    }
}