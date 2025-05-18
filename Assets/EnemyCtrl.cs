using UnityEngine;

public class EnemyCtrl : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        StickToGround();
    }
    void StickToGround()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        float rayLength = 2f;

        if (Physics.Raycast(origin, Vector3.down, out hit, rayLength))
        {
            Vector3 pos = transform.position;
            // 서서히 보정 (즉시 고정하지 않고 Lerp로 부드럽게)
            pos.y = Mathf.Lerp(pos.y, hit.point.y, 0.2f);
            transform.position = pos;
        }
    }
}
