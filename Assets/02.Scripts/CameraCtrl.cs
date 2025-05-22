using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    public Transform target;        // 따라갈 대상
    public Vector3 offset = new Vector3(0f, 10f, -5f); // 위에서 내려다보는 오프셋
    public float smoothSpeed = 5f;  // 카메라 부드럽게 따라오기

    void LateUpdate()
    {
        if (target == null) return;

        // 목표 위치 계산
        Vector3 desiredPosition = target.position + offset;

        // 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 항상 타겟을 바라보게 회전
        transform.LookAt(target);
    }
}