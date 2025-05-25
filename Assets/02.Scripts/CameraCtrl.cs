using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2f, -5f);
    public float followSpeed = 10f;

    void LateUpdate()
    {
        Vector3 desiredPos = target.position + target.rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
        transform.LookAt(target); // 또는 target.forward 방향
    }
}