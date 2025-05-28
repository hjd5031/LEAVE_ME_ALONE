using System.Collections;
using UnityEngine;
using cakeslice;
using Unity.Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCtrl : MonoBehaviour
{
    public CinemachineCamera FPCamera;
    public GameObject sprayer;
    public GameObject basket;
    public float walkSpeed = 3f;
    public float runSpeed = 7f;
    public float mouseSensitivity = 3f;
    public Transform cameraTransform;
    public GameObject[] cineCameraList;

    private Rigidbody rb;
    private Animator anim;
    private Vector3 inputDirection;
    private Vector3 moveDirection;
    private float yRotation = 0f;
    private float xRotation = 0f;

    public GameObject currentTarget = null;
    private GameObject lastOutlinedTarget = null;

    private Coroutine wateringCoroutine = null;

    private float lockedYRotation = 0f;
    private bool isRotatingLocked = false;

    private bool isSeedScheduled = false;
    private bool isPickScheduled = false;


    public bool hasVehicleItem = false;//gamemanager
    public bool hasDroneItem = false;//gamemanager
    public int PlayerScore = 0;//gamemanager
    
    
    string debugMessage = "";
    public GameObject CarItem;
    private bool isTurning;

    void Start()
    {
        ResetCamera();
        cineCameraList[0].SetActive(true);
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
            HandleMovementInput();

        HandleLook();
        HandleRaycast();
        ChangeTomatoStatus();
        if (hasVehicleItem)
        {
            // ResetCamera();
            debugMessage = "has Vehicle Item";
        }
        else
        {
            debugMessage = "has no Vehicle Item";
        }
    }
    private float targetFOV = 75f;
    public float fovChangeSpeed = 5f; // 속도 조절
    void FixedUpdate()
    {
        // GameManager.Instance.PlayerScore = PlayerScore;
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

            // 입력 조건: Shift + WASD 중 하나
        bool isRunning = Input.GetKey(KeyCode.LeftShift) &&
                         (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                          Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D));

        // 목표 FOV 설정
        targetFOV = isRunning ? 100f : 75f;

        // 현재 FOV를 목표 FOV로 자연스럽게 보간
        FPCamera.Lens.FieldOfView = Mathf.Lerp(FPCamera.Lens.FieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);

        moveDirection = inputDirection * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + transform.TransformDirection(moveDirection));
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        xRotation -= mouseY * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -30f, 80f);

        isTurning = Mathf.Abs(mouseX) > 0.1f;

        if (Input.GetMouseButton(0) && currentTarget != null)
        {
            if (!isRotatingLocked)
            {
                lockedYRotation = yRotation;
                isRotatingLocked = true;
            }

            yRotation += mouseX * mouseSensitivity;
            yRotation = Mathf.Clamp(yRotation, lockedYRotation - 5f, lockedYRotation + 5f);
        }
        else
        {
            yRotation += mouseX * mouseSensitivity;
            isRotatingLocked = false;
            transform.rotation = Quaternion.Euler(0, yRotation, 0f);
        }

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // // ✅ 좌우 회전 중일 때 애니메이터에 isTraceForward 전이
        // if (anim != null)
        // {
        //     anim.SetBool("isTraceForward", isTurning);
        // }
    }

    void HandleMovementInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector3(h, 0f, v).normalized;

        if (anim != null)
        {
            bool isMoving = inputDirection.sqrMagnitude > 0.01f;
            bool isForward = v > 0f;
            bool isBackward = v < 0f;
            bool isSide = h != 0f;

            // ⬆️ 앞으로 걷기 + ⬅️➡️ 옆으로 걷기 전이도 포함
            anim.SetBool("isTraceForward", (isMoving && (isForward || isSide)) ||isTurning);
            anim.SetBool("isTraceBackward", isMoving && isBackward);
            anim.SetBool("isShift", Input.GetKey(KeyCode.LeftShift));
        }

        if (hasVehicleItem)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Instantiate(CarItem, transform.position, Quaternion.identity);
                hasVehicleItem = false;
            }
        }
    }

    void HandleRaycast()
    {
        if (Input.GetMouseButton(0) && currentTarget != null) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        int tomatoLayer = LayerMask.NameToLayer("Tomato");
        int prefabLayer = LayerMask.NameToLayer("TomatoPrefab");
        int mask = (1 << tomatoLayer) | (1 << prefabLayer);

        if (Physics.Raycast(ray, out RaycastHit hit, 2f, mask))
        {
            GameObject hitObj = hit.collider.gameObject;

            if (hitObj != lastOutlinedTarget)
            {
                if (lastOutlinedTarget != null &&lastOutlinedTarget?.GetComponent<Outline>() is { } oldOutline)
                    oldOutline.enabled = false;

                if (hitObj.GetComponent<MeshRenderer>() != null || hitObj.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    var newOutline = hitObj.GetComponent<Outline>() ?? hitObj.AddComponent<Outline>();
                    newOutline.enabled = true;
                }

                lastOutlinedTarget = hitObj;
            }

            if (hitObj.layer == tomatoLayer)
                currentTarget = hitObj;
        }
        else
        {
            if (lastOutlinedTarget != null && !lastOutlinedTarget.Equals(null))
            {
                var outline = lastOutlinedTarget.GetComponent<Outline>();
                if (outline != null)
                    outline.enabled = false;
            }

            ResetAllTomatoStatus();
            // ResetCamera();

            currentTarget = null;
            lastOutlinedTarget = null;
        }
    }

    void ChangeTomatoStatus()
    {
        if (currentTarget == null)
        {
            ResetAllTomatoStatus();
            ResetPlayerAnimation();
            StopAllCoroutines();
            isSeedScheduled = false;
            isPickScheduled = false;
            wateringCoroutine = null;
            // ResetCamera();
            cineCameraList[0].SetActive(true);
            return;
        }

        if (Input.GetMouseButton(0))
        {
            moveDirection = Vector3.zero;
            inputDirection = Vector3.zero;
            anim.SetBool("isTraceForward", false);
           ResetCamera();
           cineCameraList[1].SetActive(true);

            string tag = currentTarget.tag;

            if (currentTarget.TryGetComponent<PlayerTomatoCtrl>(out var playerTomato))
            {
                playerTomato.PlayerUsing = true;

                if (tag == "PlayerisPlantable" || tag == "PickedPlayerTomato")
                {
                    if (!isSeedScheduled)
                    {
                        ResetAllTomatoStatus();
                        ResetPlayerAnimation();
                        isSeedScheduled = true;
                        Debug.Log("Player is seeding tomato");
                        anim.SetBool("isPlanting", true);
                        // cineCameraList[0].SetActive(false);
                        // cineCameraList[1].SetActive(true);
                        StartCoroutine(DelaySetSeed(playerTomato));
                    }
                }
                else if (tag == "UnripePlayerTomato")
                {
                    if (wateringCoroutine == null)
                    {
                        // cineCameraList[0].SetActive(false);
                        //
                        // cineCameraList[1].SetActive(true);

                        ResetPlayerAnimation();

                        ResetAllTomatoStatus();
                        wateringCoroutine = StartCoroutine(BackwardStepThenWatering(playerTomato));
                    }
                }
                else if (tag == "PlayerisSunning")
                {
                    ResetCamera();
                    cineCameraList[0].SetActive(true);
                    ResetPlayerAnimation();

                    ResetAllTomatoStatus();
                    // ResetCamera();
                }
                else if (tag == "RipePlayerTomato" &&!playerTomato.EnemyUsing)
                {
                    if (!isPickScheduled)
                    {
                        // cineCameraList[0].SetActive(false);
                        //
                        // cineCameraList[1].SetActive(true);
                        ResetPlayerAnimation();
                        ResetAllTomatoStatus();
                        anim.SetBool("isPicking", true);

                        isPickScheduled = true;
                        StartCoroutine(DelaySetPicked(playerTomato));
                    }
                }
                else
                {
                    ResetPlayerAnimation();
                    ResetCamera();
                    cineCameraList[0].SetActive(true);
                }
            }
            else if (currentTarget.TryGetComponent<EnemyTomatoCtrl>(out var enemyTomato))
            {
                if (tag == "RipeEnemyTomato"&&!enemyTomato.EnemyUsing)
                {
                    if (!isPickScheduled)
                    {
                        enemyTomato.PlayerUsing = true;
                        // cineCameraList[0].SetActive(false);
                        //
                        // cineCameraList[1].SetActive(true);

                        ResetPlayerAnimation();
                        ResetAllTomatoStatus();

                        anim.SetBool("isPicking", true);
                        isPickScheduled = true;
                        StartCoroutine(DelaySetPicked(enemyTomato));
                    }
                }
                else
                {
                    ResetPlayerAnimation();
                }
            }
        }
        else
        {
            StopAllCoroutines();
            isSeedScheduled = false;
            isPickScheduled = false;
            wateringCoroutine = null;
            ResetAllTomatoStatus();
            ResetPlayerAnimation();
            ResetCamera();
            cineCameraList[0].SetActive(true);
        }
    }

    IEnumerator DelaySetSeed(PlayerTomatoCtrl tomatoScript)
    {
        yield return new WaitForSeconds(4f);
        Debug.Log(tomatoScript);
        Debug.Log("player set seed for player tomato");
        tomatoScript.isSeeding = true;
        Debug.Log(tomatoScript.isSeeding);
        isSeedScheduled = false;
    }

    IEnumerator DelaySetPicked(PlayerTomatoCtrl tomatoScript)
    {
        yield return new WaitForSeconds(4f);
        tomatoScript.isPicked = true;
        isPickScheduled = false;
    }

    IEnumerator DelaySetPicked(EnemyTomatoCtrl tomatoScript)
    {
        yield return new WaitForSeconds(4f);
        tomatoScript.isPicked = true;
        isPickScheduled = false;
    }

    IEnumerator BackwardStepThenWatering(PlayerTomatoCtrl tomatoScript)
    {
        float moveDuration = 1f;
        float rotateDuration = 0.5f;
        float backwardDistance = 1f;
        float moveElapsed = 0f;
        float rotateElapsed = 0f;

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos - transform.forward * backwardDistance;
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0f, yRotation - 40f, 0f);

        anim.SetBool("isTraceBackward", true);

        while (moveElapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, moveElapsed / moveDuration);
            moveElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        anim.SetBool("isTraceBackward", false);
        anim.SetBool("isWatering", true);
        tomatoScript.isWatering = true;
        sprayer.SetActive(true);

        while (rotateElapsed < rotateDuration)
        {
            transform.rotation = Quaternion.Lerp(startRot, targetRot, rotateElapsed / rotateDuration);
            rotateElapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(10f);
        wateringCoroutine = null;
    }

    void ResetAllTomatoStatus()
    {
        if (currentTarget == null) return;
        anim.SetBool("isTraceForward", false);

        if (currentTarget.TryGetComponent<PlayerTomatoCtrl>(out var playerTomato))
        {
            playerTomato.isSeeding = false;
            playerTomato.isWatering = false;
            // playerTomato.isPicked = false;
            playerTomato.PlayerUsing = false;

        }
        else if (currentTarget.TryGetComponent<EnemyTomatoCtrl>(out var enemyTomato))
        {
            if (enemyTomato.EnemyUsing == false)
            {
                enemyTomato.isSeeding = false;
                enemyTomato.isWatering = false;
                // enemyTomato.isPicked = false;
                enemyTomato.PlayerUsing = false;
            }
        }
    }

    void ResetPlayerAnimation()
    {
        anim.SetBool("isPlanting", false);
        anim.SetBool("isWatering", false);
        anim.SetBool("isPicking", false);
        sprayer.SetActive(false);
        basket.SetActive(false);
    }

    void ResetCamera()
    {
        cineCameraList[0].SetActive(false);
        cineCameraList[1].SetActive(false);
        cineCameraList[2].SetActive(false);
    }

    public void TiltCameraUp()
    {
        StartCoroutine(LerpCameraPitch(-30f));
    }

    IEnumerator LerpCameraPitch(float targetPitch)
    {
        float duration = 1f;
        float elapsed = 0f;
        float startPitch = cameraTransform.localRotation.eulerAngles.x;
        float endPitch = startPitch + targetPitch;

        while (elapsed < duration)
        {
            float currentPitch = Mathf.Lerp(startPitch, endPitch, elapsed / duration);
            cameraTransform.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraTransform.localRotation = Quaternion.Euler(endPitch, 0f, 0f);
    }
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(10, 10, 400, 40), debugMessage, style);
    }
}