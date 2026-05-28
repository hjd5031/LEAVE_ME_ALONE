using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(CharacterController))]
public class NetworkPlayerMovement : NetworkBehaviour
{
    private const string LogTag = "[NetworkPlayerMovement]";

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 180f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private Camera playerCamera;

    private CharacterController characterController;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>(true);
        }

        SetCameraEnabled(false);
    }

    public override void OnNetworkSpawn()
    {
        SetCameraEnabled(IsOwner && IsClient);
        Debug.Log($"{LogTag} Spawned player. OwnerClientId={OwnerClientId} IsOwner={IsOwner}");
    }

    private void Update()
    {
        if (!IsOwner || characterController == null)
        {
            return;
        }

        MoveOwner();
    }

    private void MoveOwner()
    {
        // Prototype movement for first connection testing only.
        // It intentionally avoids advanced prediction/reconciliation for now.
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(horizontal, 0f, vertical);
        input = Vector3.ClampMagnitude(input, 1f);

        Vector3 move = transform.TransformDirection(input) * moveSpeed;
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -1f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        characterController.Move(move * Time.deltaTime);

        float yaw = Input.GetAxisRaw("Mouse X") * turnSpeed * Time.deltaTime;
        if (Mathf.Abs(yaw) > 0.001f)
        {
            transform.Rotate(0f, yaw, 0f);
        }

        if (!IsServer)
        {
            SubmitTransformServerRpc(transform.position, transform.rotation);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void SubmitTransformServerRpc(Vector3 position, Quaternion rotation, RpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
        {
            Debug.LogWarning($"{LogTag} Ignored transform update from non-owner client {rpcParams.Receive.SenderClientId}.");
            return;
        }

        transform.SetPositionAndRotation(position, rotation);
    }

    private void SetCameraEnabled(bool isEnabled)
    {
        if (playerCamera == null)
        {
            return;
        }

        playerCamera.gameObject.SetActive(isEnabled);

        AudioListener audioListener = playerCamera.GetComponent<AudioListener>();
        if (audioListener != null)
        {
            audioListener.enabled = isEnabled;
        }
    }
}
