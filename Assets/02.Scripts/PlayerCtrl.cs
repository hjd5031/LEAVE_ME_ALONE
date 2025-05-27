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
    }

    void FixedUpdate()
    {
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // cineCameraList[0].SetActive(!Input.GetKey(KeyCode.LeftShift));
        // // cineCameraList[1].SetActive(false);
        // cineCameraList[2].SetActive(Input.GetKey(KeyCode.LeftShift));

        moveDirection = inputDirection * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + transform.TransformDirection(moveDirection));
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        xRotation -= mouseY * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -30f, 80f);

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
        int tomatoLayer = LayerMask.NameToLayer("Tomato");
        int prefabLayer = LayerMask.NameToLayer("TomatoPrefab");
        int mask = (1 << tomatoLayer) | (1 << prefabLayer);

        if (Physics.Raycast(ray, out RaycastHit hit, 2f, mask))
        {
            GameObject hitObj = hit.collider.gameObject;

            if (hitObj != lastOutlinedTarget)
            {
                if (lastOutlinedTarget?.GetComponent<Outline>() is { } oldOutline)
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
            ResetCamera();
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
                else if (tag == "RipePlayerTomato")
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
                }
            }
            else if (currentTarget.TryGetComponent<EnemyTomatoCtrl>(out var enemyTomato))
            {
                if (tag == "RipeEnemyTomato")
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
        tomatoScript.isSeeding = true;
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

    // void ResetAllTomatoStatus(PlayerTomatoCtrl tomatoScript)
    // {
    //     anim.SetBool("isPlanting", false);
    //     anim.SetBool("isWatering", false);
    //     anim.SetBool("isPicking", false);
    //     tomatoScript.isSeeding = false;
    //     tomatoScript.isWatering = false;
    //     tomatoScript.isPicked = false;
    //     sprayer.SetActive(false);
    //     basket.SetActive(false);
    // }
    //
    // void ResetAllTomatoStatus(EnemyTomatoCtrl tomatoScript)
    // {
    //     anim.SetBool("isPlanting", false);
    //     anim.SetBool("isWatering", false);
    //     anim.SetBool("isPicking", false);
    //     tomatoScript.isSeeding = false;
    //     tomatoScript.isWatering = false;
    //     tomatoScript.isPicked = false;
    //     sprayer.SetActive(false);
    //     basket.SetActive(false);
    // }

    void ResetAllTomatoStatus()
    {
        if (currentTarget == null) return;
        anim.SetBool("isTraceForward", false);

        if (currentTarget.TryGetComponent<PlayerTomatoCtrl>(out var playerTomato))
        {
            playerTomato.isSeeding = false;
            playerTomato.isWatering = false;
            playerTomato.isPicked = false;
            playerTomato.PlayerUsing = false;

        }
        else if (currentTarget.TryGetComponent<EnemyTomatoCtrl>(out var enemyTomato))
        {
            if (enemyTomato.EnemyUsing == false)
            {
                enemyTomato.isSeeding = false;
                enemyTomato.isWatering = false;
                enemyTomato.isPicked = false;
                enemyTomato.PlayerUsing = false;
            }
        }
        //
        // sprayer.SetActive(false);
        // basket.SetActive(false);
        // sprayer.SetActive(false);
        // basket.SetActive(false);
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
}