using System.Collections;
using UnityEngine;
using cakeslice;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCtrl : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 7f;
    public float mouseSensitivity = 3f;
    public Transform PlayerHead;
    public Transform cameraTransform;
    public Transform pointerTargetEmpty;
    public GameObject[] cineCameraList;
    
    private Rigidbody rb;
    private Animator anim;
    private Vector3 inputDirection;
    private Vector3 moveDirection;
    private float yRotation = 0f;
    private float xRotation = 0f;

    private GameObject currentTarget = null;
    private GameObject lastOutlinedTarget = null;
    private GameObject focusTarget = null;//현재 ray가 보고 있는 target

    private float holdTimer = 0f;
    private float seedDuration = 4f;
    public bool isPlanting = false;
    public bool hasPlanted = false;

    void Start()
    {
        cineCameraList[0].SetActive(true);
        cineCameraList[1].SetActive(false);
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        yRotation = transform.eulerAngles.y;
    }

    void Update()
    {
        HandleMovementInput();
        // PlayerMove();
        HandleLook();
        HandleRaycast();
        HandleFocusInteraction();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yRotation += mouseX * mouseSensitivity;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        xRotation -= mouseY * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -30f, 60f);

        if (PlayerHead != null)
        {
            Quaternion targetRotation = (anim != null && anim.GetBool("isPlanting"))
                ? Quaternion.Euler(50f, 0f, 0f)
                : Quaternion.Euler(xRotation, 0f, 0f);

            PlayerHead.localRotation = Quaternion.Lerp(PlayerHead.localRotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    void HandleMovementInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector3(h, 0f, v).normalized;

        if (anim != null)
        {
            bool isMoving = inputDirection.sqrMagnitude > 0.01f;
            anim.SetBool("isTraceForward", isMoving);
            anim.SetBool("isShift", Input.GetKey(KeyCode.LeftShift));
        }
    }

    void HandleRaycast()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);//카메라 기준으로 Ray 발사
        if (Physics.Raycast(ray, out RaycastHit hit, 5f))//최대 탐지 거리 5f
        {
            GameObject hitObj = hit.collider.gameObject;
            currentTarget = hitObj;//Ray에 탐지된 개체

            if (pointerTargetEmpty != null)
            {
                pointerTargetEmpty.position = hit.point;
                pointerTargetEmpty.rotation = Quaternion.LookRotation(hit.normal);
            }

            // ✅ Outline 적용은 focusTarget이 없을 때만
            if (focusTarget == null && hitObj != lastOutlinedTarget)
            {
                if (lastOutlinedTarget != null)
                {
                    var prev = lastOutlinedTarget.GetComponent<Outline>();
                    if (prev != null) prev.enabled = false;
                }

                var outline = hitObj.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = true;
                    lastOutlinedTarget = hitObj;
                }
            }

            // ✅ focusTarget 지정
            bool validTag = hitObj.CompareTag("PlayerisPlantable") || hitObj.CompareTag("PickedPlayerTomato");
            if (validTag && Input.GetMouseButtonDown(0))
            {
                focusTarget = hitObj;
                isPlanting = true;
                holdTimer = 0f;
                anim.SetBool("isPlanting", true);
            }
        }
        else if (focusTarget == null)
        {
            // Ray가 아무것도 안 가리킬 때만 Outline 해제
            if (lastOutlinedTarget != null)
            {
                var outline = lastOutlinedTarget.GetComponent<Outline>();
                if (outline != null) outline.enabled = false;
                lastOutlinedTarget = null;
            }
        }
    }

    void HandleFocusInteraction()
    {
        if (focusTarget != null)
        {
            if (Input.GetMouseButton(0))
            {
                holdTimer += Time.deltaTime;

                if (holdTimer >= seedDuration && !hasPlanted)
                {
                    hasPlanted = true;
                    anim.SetBool("isPlanting", false);
                    anim.SetBool("isWatering", true);

                    var tomato = focusTarget.GetComponent<PlayerTomatoCtrl>();
                    if (tomato != null)
                    {
                        tomato.isSeeding = true;
                        tomato.isWatering = true;
                        // focusTarget.tag = "UnripePlayerTomato";
                    }
                }
            }
            else
            {
                ResetPlantState(); // 내부에서 focusTarget 초기화
            }
        }
    }

    void ResetPlantState()
    {
        if (isPlanting || hasPlanted)
        {
            isPlanting = false;
            hasPlanted = false;
            holdTimer = 0f;

            if (anim != null)
            {
                anim.SetBool("isPlanting", false);
                anim.SetBool("isWatering", false);
            }

            if (focusTarget != null)
            {
                var tomato = focusTarget.GetComponent<PlayerTomatoCtrl>();
                if (tomato != null)
                {
                    tomato.isWatering = false;
                }

                focusTarget = null; // ✅ 여기서만 null 처리
            }
        }
    }

    void FixedUpdate()
    {
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        moveDirection = inputDirection * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + transform.TransformDirection(moveDirection));
    }
}