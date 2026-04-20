using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class AstronautController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float mouseSensitivity = 2f;
    public float floatForce = 5f;
    public float gravity = -9.8f;

    [Header("Ground Settings")]
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;

    [Header("Camera Settings")]
    public Transform cameraPivot;
    public Camera mainCamera;
    public bool useFirstPerson = true;
    public Vector3 firstPersonOffset = new Vector3(0, 0, -0.14f);
    public float cameraCollisionRadius = 0.3f;
    public float minCameraDistance = 0.3f;
    public float maxCameraDistance = 1.5f;
    public LayerMask cameraCollisionLayer;

    [Header("References")]
    public Transform playerModel;

    private CharacterController controller;
    private AstronautAnimationController anim;

    private float yVelocity;
    private float xRotation = 0f;
    private bool isActing = false;
    private bool isGrounded;
    private float currentCameraDistance;
    private SphereCollider cameraCollider;
    private Vector3 initialCameraLocalPosition;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<AstronautAnimationController>();

        Cursor.lockState = CursorLockMode.Locked;

        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Default");
        }

        if (cameraCollisionLayer == 0)
        {
            cameraCollisionLayer = LayerMask.GetMask("Default");
        }

        gameObject.layer = LayerMask.NameToLayer("Player");

        if (playerModel != null)
        {
            playerModel.gameObject.layer = LayerMask.NameToLayer("Player");
        }

        SetupCameraCollision();

        if (mainCamera != null)
        {
            initialCameraLocalPosition = mainCamera.transform.localPosition;
        }

        currentCameraDistance = maxCameraDistance;

        Debug.Log($"Ground Layer set to: {LayerMask.LayerToName((int)Mathf.Log(groundLayer.value, 2))}");
    }

    void SetupCameraCollision()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            cameraCollider = mainCamera.gameObject.GetComponent<SphereCollider>();
            if (cameraCollider == null)
            {
                cameraCollider = mainCamera.gameObject.AddComponent<SphereCollider>();
            }

            cameraCollider.radius = cameraCollisionRadius;
            cameraCollider.isTrigger = true;

            Rigidbody rb = mainCamera.gameObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = mainCamera.gameObject.AddComponent<Rigidbody>();
            }
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        CheckGrounded();
        HandleMouseLook();
        HandleCameraCollision();
        HandleMovement();
        HandleAction();
    }

    void CheckGrounded()
    {
        isGrounded = controller.isGrounded;

        if (!isGrounded)
        {
            RaycastHit hit;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, groundCheckDistance + 0.1f, groundLayer))
            {
                isGrounded = true;
            }
        }
    }

    void HandleMouseLook()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mouseDelta = mouse.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        if (cameraPivot != null)
        {
            cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleCameraCollision()
    {
        if (cameraPivot == null || mainCamera == null) return;

        if (useFirstPerson)
        {
            mainCamera.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Vector3 targetCameraPosition = cameraPivot.position - cameraPivot.forward * maxCameraDistance;
            Vector3 direction = targetCameraPosition - cameraPivot.position;
            float distance = direction.magnitude;

            RaycastHit hit;
            int playerLayer = LayerMask.GetMask("Player");
            int collisionLayer = cameraCollisionLayer & ~playerLayer;

            if (Physics.SphereCast(cameraPivot.position, cameraCollisionRadius, direction.normalized, out hit, maxCameraDistance, collisionLayer))
            {
                currentCameraDistance = Mathf.Clamp(hit.distance - cameraCollisionRadius, minCameraDistance, maxCameraDistance);
            }
            else
            {
                currentCameraDistance = Mathf.Lerp(currentCameraDistance, maxCameraDistance, Time.deltaTime * 10f);
            }

            mainCamera.transform.position = cameraPivot.position - cameraPivot.forward * currentCameraDistance;
            mainCamera.transform.LookAt(cameraPivot.position);
        }
    }

    void HandleMovement()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        float h = 0f;
        float v = 0f;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) h = 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) h = -1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) v = 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) v = -1f;

        Vector3 move = transform.right * h + transform.forward * v;

        if (!isActing)
        {
            if (move.magnitude > 0.1f)
            {
                anim.SetWalk();
            }
            else
            {
                anim.SetIdle();
            }
        }

        if (isGrounded)
        {
            if (yVelocity < 0) yVelocity = -2f;

            if (keyboard.spaceKey.wasPressedThisFrame && !isActing)
            {
                yVelocity = floatForce;
                anim.SetFloat();
            }
        }

        yVelocity += gravity * Time.deltaTime;

        Vector3 velocity = moveSpeed * move + Vector3.up * yVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleAction()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.eKey.wasPressedThisFrame && !isActing)
        {
            StartCoroutine(PlayWave());
        }
    }

    IEnumerator PlayWave()
    {
        isActing = true;
        anim.SetWave();

        yield return new WaitForSeconds(2f);

        isActing = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * (groundCheckDistance + 0.1f));

        if (cameraPivot != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(cameraPivot.position, cameraCollisionRadius);
        }
    }
}
