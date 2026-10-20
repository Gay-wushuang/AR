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
    public float gravity = -1.62f;

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

    [Header("Horizontal Inertia")]
    [Min(0f)] public float groundAcceleration = 18f;
    [Min(0f)] public float groundBraking = 24f;
    [Min(0f)] public float airAcceleration = 2f;
    [Min(0f)] public float airBraking = 0.25f;
    private Vector3 horizontalVelocity;
    private float yVelocity;
    private float xRotation = 0f;
    private bool isActing = false;
    private bool isGrounded;
    private float currentCameraDistance;
    private SphereCollider cameraCollider;
    private Vector3 initialCameraLocalPosition;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Quaternion initialCameraPivotRotation;
    private readonly System.Collections.Generic.Dictionary<Renderer, bool> initialRendererStates = new System.Collections.Generic.Dictionary<Renderer, bool>();

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<AstronautAnimationController>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        if (cameraPivot != null)
        {
            initialCameraPivotRotation = cameraPivot.localRotation;
        }

        // 光标锁定由 EmotionPanel 统一管理，这里只做初始化
        // Cursor.lockState = CursorLockMode.Locked;

        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Default");
        }

        if (cameraCollisionLayer == 0)
        {
            cameraCollisionLayer = LayerMask.GetMask("Default");
        }

        int playerLayerIndex = LayerMask.NameToLayer("Player");
        if (playerLayerIndex == -1)
        {
            Debug.LogWarning("[AstronautController] 'Player' layer not found! Using Default layer (0) instead.");
            playerLayerIndex = 0;
        }
        gameObject.layer = playerLayerIndex;

        if (playerModel != null)
        {
            playerModel.gameObject.layer = playerLayerIndex;
            CachePlayerModelRenderers();
        }
        
        Debug.Log($"[AstronautController] Player layer set to: {LayerMask.LayerToName(playerLayerIndex)} ({playerLayerIndex})");

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
        if (!CanControlPlayer())
        {
            return;
        }

        CheckGrounded();
        HandleMouseLook();
        HandleCameraCollision();
        HandleMovement();
        HandleAction();
    }

    private bool CanControlPlayer()
    {
        if (EEGAI.Demo.EnergyLinkPuzzle.Active) return false;
        if (EEGAI.Demo.ConsoleRadarSystem.Instance != null && EEGAI.Demo.ConsoleRadarSystem.Instance.SelectorOpen) return false;
        var demoState = EEGAI.Demo.DemoGameState.Instance;
        return demoState != null &&
            (demoState.Status == EEGAI.Demo.DemoGameState.GameStatus.Playing ||
             demoState.Status == EEGAI.Demo.DemoGameState.GameStatus.ReadyToLaunch);
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
        // 只有在光标锁定时才处理鼠标视角
        if (Cursor.lockState != CursorLockMode.Locked) return;
        
        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mouseDelta = mouse.delta.ReadValue();
        
        // 过滤掉过大的鼠标移动（可能是因为光标锁定状态变化导致的）
        if (Mathf.Abs(mouseDelta.x) > 500f || Mathf.Abs(mouseDelta.y) > 500f)
        {
            return;
        }
        
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

        float sprint = EEGAI.Demo.EnergyLinkPuzzle.Instance != null ? EEGAI.Demo.EnergyLinkPuzzle.Instance.SpeedMultiplier : 1f;
        UpdateHorizontalVelocity(move, moveSpeed * sprint, isGrounded, Time.deltaTime);
        Vector3 velocity = horizontalVelocity + Vector3.up * yVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdateHorizontalVelocity(Vector3 input, float speed, bool grounded, float dt)
    {
        bool moving = input.sqrMagnitude > 0.001f;
        float acceleration = grounded ? (moving ? groundAcceleration : groundBraking)
            : (moving ? airAcceleration : airBraking);
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity,
            Vector3.ClampMagnitude(input, 1f) * speed, acceleration * dt);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Remove momentum into walls without removing motion along the wall.
        if (Mathf.Abs(hit.normal.y) > 0.5f) return;
        Vector3 normal = new Vector3(hit.normal.x, 0f, hit.normal.z).normalized;
        float intoWall = Vector3.Dot(horizontalVelocity, normal);
        if (intoWall < 0f) horizontalVelocity -= normal * intoWall;
    }

    void HandleAction()
    {
        // Third-person wave clips put the forearm through the first-person camera.
        if (useFirstPerson) return;
        var demoState = EEGAI.Demo.DemoGameState.Instance;
        if (demoState != null && demoState.Status == EEGAI.Demo.DemoGameState.GameStatus.ReadyToLaunch)
        {
            return;
        }

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

    public void ResetPose()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        if (controller != null)
        {
            controller.enabled = false;
        }

        transform.position = initialPosition;
        transform.rotation = initialRotation;
        yVelocity = 0f;
        horizontalVelocity = Vector3.zero;
        xRotation = 0f;
        isActing = false;

        if (cameraPivot != null)
        {
            cameraPivot.localRotation = initialCameraPivotRotation;
        }

        if (mainCamera != null)
        {
            mainCamera.transform.localPosition = initialCameraLocalPosition;
            mainCamera.transform.localRotation = Quaternion.identity;
        }

        if (controller != null)
        {
            controller.enabled = true;
        }

        SetModelVisible(true);
    }

    public void SetModelVisible(bool visible)
    {
        if (playerModel == null)
        {
            return;
        }

        if (initialRendererStates.Count == 0)
        {
            CachePlayerModelRenderers();
        }

        foreach (var pair in initialRendererStates)
        {
            if (pair.Key != null)
            {
                pair.Key.enabled = visible && pair.Value;
            }
        }
    }

    private void CachePlayerModelRenderers()
    {
        initialRendererStates.Clear();
        foreach (var renderer in playerModel.GetComponentsInChildren<Renderer>(true))
        {
            initialRendererStates[renderer] = renderer.enabled;
        }
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
