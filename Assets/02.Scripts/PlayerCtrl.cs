using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCtrl : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 10f;
    public float mouseSensitivity = 3f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private Animator anim;
    private float yRotation = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 입력 감지
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S
        // // 마우스 회전 적용
        // float mouseX = Input.GetAxis("Mouse X");
        // yRotation += mouseX * mouseSensitivity;
        // transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        moveDirection = Vector3.zero;

        // 🧭 방향 회전 및 이동 처리
        if (h != 0)
        {
            Vector3 dir = new Vector3(0, 0, h);
            moveDirection = transform.TransformDirection(dir);
            float targetY = h < 0 ? 270f : 90f;
            Quaternion targetRot = Quaternion.Euler(0, targetY, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * Time.deltaTime);
        }
        else if (v != 0)
        {
            // // 마우스 회전 적용
            // float mouseX = Input.GetAxis("Mouse X");
            // yRotation += mouseX * mouseSensitivity;
            // transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

            // 앞/뒤 이동
            Vector3 dir = new Vector3(0, 0, v).normalized;
            moveDirection = transform.TransformDirection(dir);
        }

        // 🎬 애니메이터 처리
        if (anim != null)
        {
            bool isForward = h != 0 || v > 0;
            anim.SetBool("isTraceForward", isForward);
            anim.SetBool("isTraceBackward", v < 0);
            anim.SetBool("isShift", Input.GetKey(KeyCode.LeftShift));
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        Vector3 move = moveDirection * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }
}