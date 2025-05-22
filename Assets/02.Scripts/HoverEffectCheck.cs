using UnityEngine;

public class HoverEffectCheck : MonoBehaviour
{
    public float hoverAmplitude = 0.2f;
    public float hoverSpeed = 2f;
    public float lookSmoothSpeed = 5f;

    private Vector3 initialPosition;
    private Transform targetTransform;

    void Start()
    {
        initialPosition = transform.position;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            targetTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }
    }

    void Update()
    {
        // ⬆️ 호버링
        float offsetY = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        transform.position = initialPosition + new Vector3(0f, offsetY, 0f);

        // 👀 Y축만 회전하게 LookAt
        if (targetTransform != null)
        {
            Vector3 targetPos = targetTransform.position;
            targetPos.y = transform.position.y+180;

            Vector3 direction = (targetPos - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSmoothSpeed * Time.deltaTime);
            }
        }
    }
}