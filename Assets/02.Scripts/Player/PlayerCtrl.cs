using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;
using Outline = cakeslice.Outline;

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
    private Coroutine playerDelaySetSeed = null;
    private Coroutine playerDelaySetPicked = null;
    private Coroutine enemyDelaySetPicked = null;

    private float lockedYRotation = 0f;
    private bool isRotatingLocked = false;

    private bool isSeedScheduled = false;
    private bool isPickScheduled = false;


    public bool hasVehicleItem = false;//gamemanager
    public bool hasDroneItem = false;//gamemanager
    public int PlayerScore = 0;//gamemanager
    
    
    // string debugMessage = "";
    public GameObject CarItem;
    public GameObject ToxicDroneItem;//for enemy
    public GameObject BoostDroneItem;//for player itself
    public Transform ToxicDroneSpawnPoint;
    public Transform BoostDroneSpawnPoint;
    private bool isTurning;


    private string PlayerWalkSoundID;
    private string PlayerRunSoundID;
    private string PlayerWaterSoundID;
    private string PlayerDigSoundID;
    
    public GameObject PlayerBoostDroneItem;
    public GameObject PlayerToxicDroneItem;
    public GameObject PlayerVehicleItem;

    //for Boost Drone Time Left GUI
    public GameObject DroneBar;
    public Slider DroneBarSlider;
    private Coroutine droneBarCoroutine = null;
    private float targetFOV = 75f;
    public float fovChangeSpeed = 5f; // 속도 조절
    public GameObject instructions;
    void Start()
    {
        ResetCamera();
        cineCameraList[0].SetActive(true);
        sprayer.SetActive(false);
        basket.SetActive(false);
        PlayerVehicleItem.SetActive(false);
        PlayerToxicDroneItem.SetActive(false);
        PlayerBoostDroneItem.SetActive(false);
        DroneBar.SetActive(false);
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        yRotation = transform.eulerAngles.y;
        // DroneBarSlider.value = 30f;
        instructions.SetActive(false);
    }

    void Update()
    {
        ShowInstructions();
        if (!instructions.activeSelf)
        {
            if (!(Input.GetMouseButton(0) && currentTarget != null))
                HandleMovementInput();
            CheckItemUsability();
            HandleLook();
            HandleRaycast();
            ChangeTomatoStatus();
            HandleUI();
        }
    }

    void ShowInstructions()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (instructions.activeSelf)
            {
                SoundManager.Instance.ChangeBgmVolume(1f);
                instructions.SetActive(false);
                Time.timeScale = 1f;
            }
            else
            {
                SoundManager.Instance.StopAllSfx();
                SoundManager.Instance.ChangeBgmVolume(0.3f);
                instructions.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }
    void HandleUI()
    {
        GameManager.Instance.PlayerScore = PlayerScore;
        if (hasVehicleItem)
        {
            PlayerVehicleItem.SetActive(true);
        }
        else
        {
            PlayerVehicleItem.SetActive(false);
        }

        if (hasDroneItem)
        {
            PlayerToxicDroneItem.SetActive(true);
            PlayerBoostDroneItem.SetActive(true);
        }
        else
        {
            PlayerToxicDroneItem.SetActive(false);
            PlayerBoostDroneItem.SetActive(false);
        }
        
        // if (hasVehicleItem && hasDroneItem)
        // {
        //     // ResetCamera();
        //     debugMessage = "has Vehicle Item"+"\nhas Drone Item\n" + PlayerScore;
        // }
        // if (!hasVehicleItem && hasDroneItem)
        // {
        //     // ResetCamera();
        //     debugMessage = "no Vehicle Item"+"\nhas Drone Item\n" + PlayerScore;
        // }
        // if (hasVehicleItem && !hasDroneItem)
        // {
        //     // ResetCamera();
        //     debugMessage = "has Vehicle Item"+"\nno Drone Item\n" + PlayerScore;
        // }
        // if (!hasVehicleItem && !hasDroneItem)
        // {
        //     // ResetCamera();
        //     debugMessage = "no Vehicle Item"+"\nno Drone Item\n" + PlayerScore;
        // }
    }
    // private float targetFOV = 75f;
    // public float fovChangeSpeed = 5f; // 속도 조절
    void FixedUpdate()
    {
        // GameManager.Instance.PlayerScore = PlayerScore;
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

            // 입력 조건: Shift + WASD 중 하나
        bool isRunning = Input.GetKey(KeyCode.LeftShift) &&
                         (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                          Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D));

        // 목표 FOV 설정
        targetFOV = isRunning ? 90f : 75f;

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
    private bool isWalking = false;
    private bool isRunning = false;
    void HandleMovementInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector3(h, 0f, v).normalized;

        bool isMoving = inputDirection.sqrMagnitude > 0.01f;
        bool isForward = v > 0f;
        bool isBackward = v < 0f;
        bool isSide = h != 0f;
        bool isShift = Input.GetKey(KeyCode.LeftShift);

        if (anim != null)
        {
            anim.SetBool("isTraceForward", (isMoving && (isForward || isSide)) || isTurning);
            anim.SetBool("isTraceBackward", isMoving && isBackward);
            anim.SetBool("isShift", isShift);
        }

        // ✅ 사운드 처리
        if (isMoving)
        {
            if (isShift && !isRunning)
            {
                StopWalkSound();
                PlayRunSound();
            }
            else if (!isShift && !isWalking)
            {
                StopRunSound();
                PlayWalkSound();
            }
        }
        else
        {
            StopWalkSound();
            StopRunSound();
        }
    }

    void CheckItemUsability()
    {
        if (hasVehicleItem)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)&&!GameManager.Instance.EnemyUsingItem)
            {
                Instantiate(CarItem, transform.position, Quaternion.identity);
                GameManager.Instance.PLayerUsingItem = true;
                hasVehicleItem = false;
            }
        }

        if (hasDroneItem)
        {
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Instantiate(BoostDroneItem, BoostDroneSpawnPoint.position, Quaternion.identity);
                DroneBar.SetActive(true);
                hasDroneItem = false;
                if(droneBarCoroutine != null)
                    StopCoroutine(droneBarCoroutine);
                droneBarCoroutine = StartCoroutine(DroneBarCountdown(60f));
            } 
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Instantiate(ToxicDroneItem, ToxicDroneSpawnPoint.position, Quaternion.identity);
                hasDroneItem = false;
            }
        }
    }
    IEnumerator DroneBarCountdown(float duration)
    {
        Debug.Log("Drone Bar Countdown");
        // DroneBarSlider.gameObject.SetActive(true);
        DroneBarSlider.value = 60f;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            DroneBarSlider.value = duration - elapsed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        DroneBarSlider.value = 0f;
        DroneBar.SetActive(false);
        // DroneBarSlider.value = 1f;
        droneBarCoroutine = null;
    }
// ✅ 사운드 재생 / 정지 함수 분리
    void PlayWalkSound()
    {
        if (PlayerWalkSoundID == null)
            PlayerWalkSoundID = SoundManager.Instance.PlaySfx(SoundManager.Sfx.WalkOnEarth, true,1f);
        isWalking = true;
        isRunning = false;
    }

    void PlayRunSound()
    {
        if (PlayerRunSoundID == null)
            PlayerRunSoundID = SoundManager.Instance.PlaySfx(SoundManager.Sfx.RunOnEarth, true,1f);
        isRunning = true;
        isWalking = false;
    }

    void StopWalkSound()
    {
        if (PlayerWalkSoundID != null)
        {
            SoundManager.Instance.StopSfx(PlayerWalkSoundID);
            PlayerWalkSoundID = null;
        }
        isWalking = false;
    }

    void StopRunSound()
    {
        if (PlayerRunSoundID != null)
        {
            SoundManager.Instance.StopSfx(PlayerRunSoundID);
            PlayerRunSoundID = null;
        }
        isRunning = false;
    }
    void HandleRaycast()
    {
        if (Input.GetMouseButton(0) && currentTarget != null)
        {
            if (lastOutlinedTarget != null && lastOutlinedTarget.GetComponent<Outline>() is { } outline)
                outline.enabled = false;

            lastOutlinedTarget = null;
            return;
        }

        Ray rayPrefab = new Ray(cameraTransform.position, cameraTransform.forward);
        Ray rayTomato = new Ray(cameraTransform.position, cameraTransform.forward);

        int tomatoLayer = LayerMask.NameToLayer("Tomato");
        int prefabLayer = LayerMask.NameToLayer("TomatoPrefab");

        int tomatoMask = 1 << tomatoLayer;
        int prefabMask = 1 << prefabLayer;

        // ✅ TomatoPrefab용 Ray → Outline 처리
        if (Physics.Raycast(rayPrefab, out RaycastHit prefabHit, 2f, prefabMask))
        {
            GameObject hitObj = prefabHit.collider.gameObject;

            if (hitObj != lastOutlinedTarget)
            {
                if (lastOutlinedTarget != null && lastOutlinedTarget.GetComponent<Outline>() is { } oldOutline)
                    oldOutline.enabled = false;

                if (hitObj.GetComponent<MeshRenderer>() != null || hitObj.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    var newOutline = hitObj.GetComponent<Outline>() ?? hitObj.AddComponent<Outline>();
                    newOutline.enabled = true;
                }

                lastOutlinedTarget = hitObj;
            }
        }
        else
        {
            // ❌ outline 제거
            if (lastOutlinedTarget != null && lastOutlinedTarget.GetComponent<Outline>() is { } outline)
                outline.enabled = false;

            lastOutlinedTarget = null;
        }

        // ✅ Tomato용 Ray → currentTarget만 설정
        if (Physics.Raycast(rayTomato, out RaycastHit tomatoHit, 2f, tomatoMask))
        {
            currentTarget = tomatoHit.collider.gameObject;
        }
        else
        {
            ResetAllTomatoStatus();
            currentTarget = null;
        }
    }
    // void HandleRaycast()
    // {
    //     if (Input.GetMouseButton(0) && currentTarget != null)
    //     {
    //         if (lastOutlinedTarget != null && lastOutlinedTarget.GetComponent<Outline>() is { } outline)
    //             outline.enabled = false;
    //
    //         lastOutlinedTarget = null;
    //         return;
    //     }
    //     Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
    //     int tomatoLayer = LayerMask.NameToLayer("Tomato");
    //     int prefabLayer = LayerMask.NameToLayer("TomatoPrefab");
    //     int mask = (1 << tomatoLayer) | (1 << prefabLayer);
    //
    //     // if (Physics.Raycast(ray, out RaycastHit hit, 2f, mask))
    //     // {
    //     //     GameObject hitObj = hit.collider.gameObject;
    //     //
    //     //     if (hitObj != lastOutlinedTarget)
    //     //     {
    //     //         if (lastOutlinedTarget != null &&lastOutlinedTarget?.GetComponent<Outline>() is { } oldOutline)
    //     //             oldOutline.enabled = false;
    //     //
    //     //         if (hitObj.GetComponent<MeshRenderer>() != null || hitObj.GetComponent<SkinnedMeshRenderer>() != null)
    //     //         {
    //     //             var newOutline = hitObj.GetComponent<Outline>() ?? hitObj.AddComponent<Outline>();
    //     //             newOutline.enabled = true;
    //     //         }
    //     //
    //     //         lastOutlinedTarget = hitObj;
    //     //     }
    //     //
    //     //     if (hitObj.layer == tomatoLayer)
    //     //         currentTarget = hitObj;
    //     // }
    //     // else
    //     // {
    //     //     if (lastOutlinedTarget != null && !lastOutlinedTarget.Equals(null))
    //     //     {
    //     //         var outline = lastOutlinedTarget.GetComponent<Outline>();
    //     //         if (outline != null)
    //     //             outline.enabled = false;
    //     //     }
    //     //
    //     //     ResetAllTomatoStatus();
    //     //     // ResetCamera();
    //     //
    //     //     currentTarget = null;
    //     //     lastOutlinedTarget = null;
    //     // }
    //     if (Physics.Raycast(ray, out RaycastHit hit, 2f, mask))
    //     {
    //         GameObject hitObj = hit.collider.gameObject;
    //         int hitLayer = hitObj.layer;
    //
    //         // 🔁 Outline 초기화
    //         if (hitObj != lastOutlinedTarget)
    //         {
    //             if (lastOutlinedTarget != null && lastOutlinedTarget.GetComponent<Outline>() is { } oldOutline)
    //                 oldOutline.enabled = false;
    //
    //             // ✔️ Outline 적용은 TomatoPrefab만
    //             if (hitLayer == prefabLayer)
    //             {
    //                 if (hitObj.GetComponent<MeshRenderer>() != null || hitObj.GetComponent<SkinnedMeshRenderer>() != null)
    //                 {
    //                     var newOutline = hitObj.GetComponent<Outline>() ?? hitObj.AddComponent<Outline>();
    //                     newOutline.enabled = true;
    //                     lastOutlinedTarget = hitObj; // ✔️ outline된 오브젝트로만 기록
    //                 }
    //             }
    //             else
    //             {
    //                 lastOutlinedTarget = null; // ✔️ Tomato는 outline 없음
    //             }
    //         }
    //
    //         // ✔️ Tomato 레이어만 currentTarget 설정
    //         if (hitLayer == tomatoLayer)
    //         {
    //             currentTarget = hitObj;
    //         }
    //     }
    //     else
    //     {
    //         // 🔁 Outline 제거
    //         if (lastOutlinedTarget != null && lastOutlinedTarget.GetComponent<Outline>() is { } outline)
    //             outline.enabled = false;
    //
    //         ResetAllTomatoStatus();
    //         currentTarget = null;
    //         lastOutlinedTarget = null;
    //     }
    // }

    void ChangeTomatoStatus()
    {
        if (currentTarget == null)
        {
            ResetAllTomatoStatus();
            ResetPlayerAnimation();
            StopCoroutines();
            // StopAllCoroutines();

            isSeedScheduled = false;
            isPickScheduled = false;
            wateringCoroutine = null;
            if (PlayerDigSoundID != null)
            {
                SoundManager.Instance.StopSfx(PlayerDigSoundID);
                PlayerDigSoundID = null;
            }
            if (PlayerWaterSoundID != null)
            {
                SoundManager.Instance.StopSfx(PlayerWaterSoundID);
                PlayerWaterSoundID = null;
            }

            // ResetCamera();
            cineCameraList[0].SetActive(true);
            return;
        }

        if (Input.GetMouseButton(0))
        {
            moveDirection = Vector3.zero;
            inputDirection = Vector3.zero;
            anim.SetBool("isTraceForward", false);
            anim.SetBool("isShift", false);
            if (PlayerWalkSoundID != null)
            {
                SoundManager.Instance.StopSfx(PlayerWalkSoundID);
                PlayerWalkSoundID = null;
            }

            if (PlayerRunSoundID != null)
            {
                SoundManager.Instance.StopSfx(PlayerRunSoundID);
                PlayerRunSoundID = null;
            }
            
            ResetCamera();
           cineCameraList[1].SetActive(true);

            string tag = currentTarget.tag;

            if (currentTarget.TryGetComponent<PlayerTomatoCtrl>(out var playerTomato))
            {
                playerTomato.PlayerUsing = true;

                if (tag == "PlayerisPlantable")// || tag == "PickedPlayerTomato"
                {
                    if (!isSeedScheduled)
                    {
                        ResetAllTomatoStatus();
                        ResetPlayerAnimation();
                        playerTomato.PlayerUsing = true;
                        PlayerDigSoundID = SoundManager.Instance.PlaySfx(SoundManager.Sfx.DigSoil, true,1f);
                        isSeedScheduled = true;
                        // Debug.Log("Player is seeding tomato");
                        anim.SetBool("isTraceForward",false);
                        anim.SetBool("isPlanting", true);
                        // cineCameraList[0].SetActive(false);
                        // cineCameraList[1].SetActive(true);
                        Debug.Log("player set seed for player tomato");
                        playerDelaySetSeed = StartCoroutine(DelaySetSeed(playerTomato));
                    }
                    else
                    {
                        if(anim.GetBool("isTraceForward"))
                            anim.SetBool("isTraceForward", false);
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
                        playerTomato.PlayerUsing = true;
                        if (PlayerDigSoundID != null)
                        {
                            SoundManager.Instance.StopSfx(PlayerDigSoundID);
                            PlayerDigSoundID = null;
                        }

                        wateringCoroutine = StartCoroutine(BackwardStepThenWatering(playerTomato));
                    }
                }
                else if (tag == "PlayerisSunning")
                {
                    ResetCamera();
                    cineCameraList[0].SetActive(true);
                    ResetPlayerAnimation();
                    SoundManager.Instance.StopSfx(PlayerWaterSoundID);

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
                        playerTomato.PlayerUsing = true;

                        anim.SetBool("isPicking", true);

                        isPickScheduled = true;
                        playerDelaySetPicked = StartCoroutine(DelaySetPicked(playerTomato));
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
                        ResetPlayerAnimation();
                        ResetAllTomatoStatus();
                        enemyTomato.PlayerUsing = true;
                        // cineCameraList[0].SetActive(false);
                        //
                        // cineCameraList[1].SetActive(true);


                        anim.SetBool("isPicking", true);
                        isPickScheduled = true;
                        enemyDelaySetPicked = StartCoroutine(DelaySetPicked(enemyTomato));
                    }
                }
                else
                {
                    ResetPlayerAnimation();
                    ResetCamera();
                    cineCameraList[0].SetActive(true);
                }
            }
        }
        else
        {
            StopCoroutines();
            // StopAllCoroutines();
            isSeedScheduled = false;
            isPickScheduled = false;
            wateringCoroutine = null;
            ResetAllTomatoStatus();
            ResetPlayerAnimation();
            ResetCamera();
            SoundManager.Instance.StopSfx(PlayerDigSoundID);
            SoundManager.Instance.StopSfx(PlayerWaterSoundID);
            cineCameraList[0].SetActive(true);
        }
    }

    void StopCoroutines()
    {
        if (playerDelaySetSeed != null)
            StopCoroutine(playerDelaySetSeed);
        if (playerDelaySetPicked != null)
            StopCoroutine(playerDelaySetPicked);
        if(enemyDelaySetPicked != null)
            StopCoroutine(enemyDelaySetPicked);
        if (wateringCoroutine != null)
            StopCoroutine(wateringCoroutine);

    }
    IEnumerator DelaySetSeed(PlayerTomatoCtrl tomatoScript)
    {
        yield return new WaitForSeconds(4f);
        // Debug.Log(tomatoScript);
        // Debug.Log("player set seed for player tomato");
        tomatoScript.isSeeding = true;
        // Debug.Log(tomatoScript.isSeeding);
        yield return new WaitForSeconds(0.5f);
        isSeedScheduled = false;
        playerDelaySetSeed = null;
    }

    IEnumerator DelaySetPicked(PlayerTomatoCtrl tomatoScript)
    {
        yield return new WaitForSeconds(4f);
        tomatoScript.isPicked = true;
        isPickScheduled = false;
        PlayerScore += 6;
        SoundManager.Instance.PlaySfx(SoundManager.Sfx.TomatoPick, false, 1f);
        SoundManager.Instance.PlaySfx(SoundManager.Sfx.PointUp, false, 1f);

        playerDelaySetPicked = null;
    }

    IEnumerator DelaySetPicked(EnemyTomatoCtrl tomatoScript)
    {
        yield return new WaitForSeconds(4f);
        tomatoScript.isPicked = true;
        isPickScheduled = false;
        PlayerScore += 4;
        SoundManager.Instance.PlaySfx(SoundManager.Sfx.TomatoPick, false, 1f);
        SoundManager.Instance.PlaySfx(SoundManager.Sfx.PointUp, false, 1f);

        enemyDelaySetPicked = null;
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
        PlayerWaterSoundID = SoundManager.Instance.PlaySfx(SoundManager.Sfx.WaterTomato, true,1f);
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
        // if (PlayerWaterSoundID != null)
        // {
        //     PlayerWaterSoundID = null;
        // }
        SoundManager.Instance.StopSfx(PlayerWaterSoundID);

        wateringCoroutine = null;
    }

    void ResetAllTomatoStatus()
    {
        if (currentTarget == null) return;
        anim.SetBool("isTraceForward", false);
        anim.SetBool("isTraceBackward", false);
        if (currentTarget.TryGetComponent<PlayerTomatoCtrl>(out var playerTomato))
        {
            // Debug.Log("Player reset all tomato status");
            // playerTomato.isSeeding = false;
            playerTomato.isWatering = false;
            // playerTomato.isPicked = false;
            playerTomato.PlayerUsing = false;
        
        }
        else if (currentTarget.TryGetComponent<EnemyTomatoCtrl>(out var enemyTomato))
        {
            if (enemyTomato.EnemyUsing == false)
            {
                // enemyTomato.isSeeding = false;
                // enemyTomato.isWatering = false;
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

}