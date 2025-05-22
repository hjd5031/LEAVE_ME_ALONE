using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCtrl : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 7f;
    public float mouseSensitivity = 3f;
    public Transform PlayerHead;

    private Rigidbody rb;
    private Animator anim;
    private Vector3 inputDirection;
    private Vector3 moveDirection;
    private float yRotation = 0f; // 좌우 회전
    private float xRotation = 0f; // 상하 회전 (PlayerHead에 적용)

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yRotation = transform.eulerAngles.y;
    }

    void Update()
    {
        // 📌 마우스 회전 입력
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // 좌우 회전: 본체 기준 Y축
        yRotation += mouseX * mouseSensitivity;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        // 상하 회전: PlayerHead 기준 X축 (pitch)
        xRotation -= mouseY * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -30f, 30f);
        if (PlayerHead != null)
        {
            PlayerHead.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        // 🎮 입력
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector3(h, 0f, v).normalized;

        // 🎬 애니메이션
        if (anim != null)
        {
            bool isMoving = inputDirection.sqrMagnitude > 0.01f;
            anim.SetBool("isTraceForward", isMoving);
            anim.SetBool("isShift", Input.GetKey(KeyCode.LeftShift));
        }
    }

    void FixedUpdate()
    {
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        moveDirection = inputDirection * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + transform.TransformDirection(moveDirection));
    }
}