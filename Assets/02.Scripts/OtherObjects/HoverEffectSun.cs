using UnityEngine;

public class HoverEffectSun : MonoBehaviour
{
    public float hoverAmplitude = 0.2f;
    public float hoverSpeed = 2f;
    public float lookSmoothSpeed = 5f;

    private Vector3 initialPosition;
    private Transform targetTransform;
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
        // ⬆️ 호버링
        float offsetY = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        transform.position = initialPosition + new Vector3(0f, offsetY, 0f);

        // 👀 Y축만 회전하게 LookAt
        if (targetTransform != null)
        {
            Vector3 targetPos = targetTransform.position;
            targetPos.y = transform.position.y;

            Vector3 direction = (targetPos - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                // targetRotation *= Quaternion.Euler(0f, 180f, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSmoothSpeed * Time.deltaTime);
            }
        }
    }
}