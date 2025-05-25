using System.Collections;
using UnityEngine;
using cakeslice;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCtrl : MonoBehaviour
{
    public GameObject sprayer; // 손에 붙은 분무통 오브젝트
    public GameObject basket;  // 손에 붙은 바구니 오브젝트
    
    
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

    public GameObject currentTarget = null;
    private GameObject lastOutlinedTarget = null;
    private GameObject focusTarget = null;//현재 ray가 보고 있는 target

    private float holdTimer = 0f;
    private float seedDuration = 4f;
    public bool isPlanting = false;
    public bool hasPlanted = false;
    private bool isClicking = false;

    void Start()
    {
        cineCameraList[0].SetActive(true);
        cineCameraList[1].SetActive(false);
        sprayer.SetActive(false);
        basket.SetActive(false);
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        yRotation = transform.eulerAngles.y;
    }

    void Update()
    {
        if (!(Input.GetMouseButton(0) && currentTarget != null))
        {
            HandleMovementInput();
            // PlayerMove();
        }
        HandleLook();
        // else
        // {
        //     // 🎯 입력을 막는 상황에서 플레이어 회전값 고정
        //     yRotation = transform.eulerAngles.y;
        //     xRotation = PlayerHead != null ? PlayerHead.localEulerAngles.x : xRotation;
        // }

        HandleRaycast();
        // HandleFocusInteraction();
        ChangeTomatoStatus();
    }
    void FixedUpdate()
    {
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        moveDirection = inputDirection * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + transform.TransformDirection(moveDirection));
    }

    void HandleLook()
    {
        if (!(Input.GetMouseButton(0) && currentTarget != null))
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

                PlayerHead.localRotation =
                    Quaternion.Lerp(PlayerHead.localRotation, targetRotation, Time.deltaTime * 5f);
            }
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
        if (Input.GetMouseButton(0) && currentTarget != null) return;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        // 🍅 "Tomato"와 "TomatoPrefab" 레이어를 포함한 마스크 생성
        int tomatoLayer = LayerMask.NameToLayer("Tomato");
        int prefabLayer = LayerMask.NameToLayer("TomatoPrefab");
        int mask = (1 << tomatoLayer) | (1 << prefabLayer);  // OR 연산으로 결합

        if (Physics.Raycast(ray, out RaycastHit hit, 5f, mask))
        {
            GameObject hitObj = hit.collider.gameObject;
            Debug.Log("🎯 Ray Hit: " + hitObj.name);

            // ✅ Outline 처리 (생략 없이 기존 코드 유지)
            if (hitObj != lastOutlinedTarget)
            {
                if (lastOutlinedTarget != null)
                {
                    var oldOutline = lastOutlinedTarget.GetComponent<cakeslice.Outline>();
                    if (oldOutline != null)
                        oldOutline.enabled = false;
                }

                if (hitObj.GetComponent<MeshRenderer>() != null || hitObj.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    var newOutline = hitObj.GetComponent<cakeslice.Outline>();
                    if (newOutline == null)
                        newOutline = hitObj.AddComponent<cakeslice.Outline>();
                    if (newOutline != null)
                        newOutline.enabled = true;
                }

                lastOutlinedTarget = hitObj;
            }

            // ✅ currentTarget은 "Tomato" 레이어일 때만 저장
            if (hitObj.layer == tomatoLayer)
            {
                currentTarget = hitObj;
            }

            // // ✅ focusTarget 지정
            // bool validTag = hitObj.CompareTag("PlayerisPlantable") || hitObj.CompareTag("PickedPlayerTomato");
            // if (validTag && Input.GetMouseButtonDown(0))
            // {
            //     focusTarget = hitObj;
            //     isPlanting = true;
            //     holdTimer = 0f;
            //     anim.SetBool("isPlanting", true);
            // }
        }
        else
        {
            // 🎯 감지 실패 시 Outline 제거
            if (lastOutlinedTarget != null)
            {
                var outline = lastOutlinedTarget.GetComponent<Outline>();
                if (outline != null) outline.enabled = false;
                lastOutlinedTarget = null;
            }

            currentTarget = null;
        }
    }

    // void HandleFocusInteraction()
    // {
    //     if (focusTarget != null)
    //     {
    //         if (Input.GetMouseButton(0))
    //         {
    //             holdTimer += Time.deltaTime;
    //
    //             if (holdTimer >= seedDuration && !hasPlanted)
    //             {
    //                 hasPlanted = true;
    //                 anim.SetBool("isPlanting", false);
    //                 anim.SetBool("isWatering", true);
    //
    //                 var tomato = focusTarget.GetComponent<PlayerTomatoCtrl>();
    //                 if (tomato != null)
    //                 {
    //                     tomato.isSeeding = true;
    //                     tomato.isWatering = true;
    //                     // focusTarget.tag = "UnripePlayerTomato";
    //                 }
    //             }
    //         }
    //         else
    //         {
    //             ResetPlantState(); // 내부에서 focusTarget 초기화
    //         }
    //     }
    // }

    void ChangeTomatoStatus()
    {
        if (currentTarget != null)
        {
            var tomatoScript = currentTarget.GetComponent<PlayerTomatoCtrl>();
            if (Input.GetMouseButton(0))
            {
                cineCameraList[0].SetActive(false);
                cineCameraList[1].SetActive(true);
                // tomatoScript.isClicking = true;
                if (currentTarget.tag == "PlayerisPlantable" || currentTarget.tag == "PickedPlayerTomato")
                {
                    //플레이어 심기 동작
                    anim.SetBool("isPlanting", true);
                    anim.SetBool("isWatering", false);
                    anim.SetBool("isPicking", false);
                    Invoke("ChangeIsSeed",4f);
                    tomatoScript.isWatering = false;
                    tomatoScript.isPicked = false;
                    // tomatoScript.isSeeding = true;
                    sprayer.SetActive(false);
                    basket.SetActive(false);
                }

                if (currentTarget.tag == "UnripePlayerTomato")
                {
                    //플레이어 물주기 동작
                    anim.SetBool("isPlanting", false);
                    anim.SetBool("isWatering", true);
                    anim.SetBool("isPicking", false);
                    tomatoScript.isSeeding = false;
                    tomatoScript.isWatering = true;
                    tomatoScript.isPicked = false;
                    sprayer.SetActive(true);
                    basket.SetActive(false);
                    

                }

                if (currentTarget.tag == "PlayerisSunning")
                {
                    //아무것도 안하기
                    anim.SetBool("isPlanting", false);
                    anim.SetBool("isWatering", false);
                    anim.SetBool("isPicking", false);
                    tomatoScript.isSeeding = false;
                    tomatoScript.isWatering = false;
                    tomatoScript.isPicked = false;
                    sprayer.SetActive(false);
                    basket.SetActive(false);
                }

                if (currentTarget.tag == "RipePlayerTomato")
                {
                    //플레이어 따기 모션
                    anim.SetBool("isPlanting", false);
                    anim.SetBool("isWatering", false);
                    anim.SetBool("isPicking", true);
                    tomatoScript.isSeeding = false;
                    tomatoScript.isWatering = false;
                    Invoke("ChangeIsPicked", 7f);
                    sprayer.SetActive(false);
                    basket.SetActive(true);
                }
            }
            else
            {
                cineCameraList[0].SetActive(true);
                cineCameraList[1].SetActive(false);
                anim.SetBool("isPlanting", false);
                anim.SetBool("isWatering", false);
                anim.SetBool("isPicking", false);
                tomatoScript.isWatering = false;
                tomatoScript.isPicked = false;
                tomatoScript.isSeeding = false;
                sprayer.SetActive(false);
                basket.SetActive(false);
                CancelInvoke("ChangeIsSeed");
                CancelInvoke("ChangeIsPicked");
                //모두 종료
            }
        }
        else
        {
            CancelInvoke("ChangeIsSeed");
            CancelInvoke("ChangeIsPicked");
            cineCameraList[0].SetActive(true);
            cineCameraList[1].SetActive(false);
            anim.SetBool("isPlanting", false);
            anim.SetBool("isWatering", false);
            anim.SetBool("isPicking", false);
            // tomatoScript.isWatering = false;
            // tomatoScript.isPicked = false;
            // tomatoScript.isSeeding = false;
            sprayer.SetActive(false);
            basket.SetActive(false);
        }
    }

    void ChangeIsSeed()
    {
        var tomatoScript = currentTarget.GetComponent<PlayerTomatoCtrl>();

        tomatoScript.isSeeding = true;
    }
    void ChangeIsPicked(PlayerTomatoCtrl ptc)
    {
        var tomatoScript = currentTarget.GetComponent<PlayerTomatoCtrl>();

        tomatoScript.isPicked = true;
    }
    // void ResetPlantState()
    // {
    //     
    //     if (isPlanting || hasPlanted)
    //     {
    //         isPlanting = false;
    //         hasPlanted = false;
    //         holdTimer = 0f;
    //
    //         if (anim != null)
    //         {
    //             anim.SetBool("isPlanting", false);
    //             anim.SetBool("isWatering", false);
    //         }
    //
    //         if (focusTarget != null)
    //         {
    //             var tomato = focusTarget.GetComponent<PlayerTomatoCtrl>();
    //             if (tomato != null)
    //             {
    //                 tomato.isWatering = false;
    //             }
    //
    //             focusTarget = null; // ✅ 여기서만 null 처리
    //         }
    //     }
    // }

    
}