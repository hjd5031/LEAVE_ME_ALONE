using UnityEngine;

public class HoverEffectCheck : MonoBehaviour
{
    public float hoverAmplitude = 0.2f;      // 호버링 높이
    public float hoverSpeed = 2f;            // 호버링 속도
    public float lookSmoothSpeed = 5f;       // 회전 부드럽게 만드는 속도

    private Vector3 initialPosition;         // 시작 위치 저장
    private Transform targetTransform;       // 바라볼 대상 (플레이어)
    private GameObject player;

    void Start()
    {
        initialPosition = transform.position;

        // "Player" 태그가 붙은 오브젝트 찾기
        player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            // Debug.LogWarning("Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
            targetTransform = player.transform;
        }
        else
        {
            // Debug.LogWarning("Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }
    }

    void Update()
    {
        if (player != null)
        {
            // Debug.LogWarning("Player 태그를 가진 오브젝트를 찾았습니다.");
            player = GameObject.FindWithTag("Player");
            targetTransform = player.transform;

        }
        // ⬆️ 호버링 (y값을 사인 함수로 위아래 움직임)
        float offsetY = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        transform.position = initialPosition + new Vector3(0f, offsetY, 0f);

        // 👀 Y축 회전으로만 대상 바라보기
        if (targetTransform != null)
        {
            // 대상 위치의 y를 동일하게 해서 수평 회전만 하도록 함
            Vector3 targetPos = targetTransform.position;
            targetPos.y = transform.position.y;

            // 방향 벡터 계산
            Vector3 direction = (targetPos - transform.position).normalized;

            // 방향이 유효할 때만 회전
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                targetRotation *= Quaternion.Euler(0, 180f, 0); // Y축 180도 회전 보정
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSmoothSpeed * Time.deltaTime);
            }
        }
    }
}