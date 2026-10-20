using UnityEngine;
using UnityEngine.InputSystem;

namespace EEGAI.Demo
{
    public class DemoPlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float baseMoveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Emotion Speed Modifiers")]
        [SerializeField] private float happySpeedBonus = 1.3f;
        [SerializeField] private float sadSpeedPenalty = 0.7f;
        [SerializeField] private float normalSpeed = 1f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.4f;
        [SerializeField] private LayerMask groundMask;

        public bool IsMoving { get; private set; } = false;

        private CharacterController controller;
        private Vector3 velocity;
        private DemoEmotionSimulator emotionSimulator;
        private DemoOxygenSystem oxygenSystem;
        private DemoGameState gameState;
        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        private void Start()
        {
            emotionSimulator = FindAnyObjectByType<DemoEmotionSimulator>();
            oxygenSystem = FindAnyObjectByType<DemoOxygenSystem>();
            gameState = FindAnyObjectByType<DemoGameState>();
        }

        private void Update()
        {
            Move();
        }

        private void Move()
        {
            if (gameState == null)
            {
                gameState = FindAnyObjectByType<DemoGameState>();
            }

            bool canMove = gameState != null &&
                (gameState.Status == DemoGameState.GameStatus.Playing ||
                gameState.Status == DemoGameState.GameStatus.ReadyToLaunch);

            if (!canMove)
            {
                IsMoving = false;
                if (oxygenSystem != null)
                {
                    oxygenSystem.SetMoving(false);
                }
                return;
            }

            bool isGrounded = groundCheck == null ||
                Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            Vector2 moveInput = Vector2.zero;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput.x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput.y -= 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput.y += 1f;
            }

            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
            move = Vector3.ClampMagnitude(move, 1f);

            IsMoving = move.magnitude > 0.1f;

            if (oxygenSystem != null)
            {
                oxygenSystem.SetMoving(IsMoving);
            }

            float emotionSpeedModifier = normalSpeed;

            if (emotionSimulator != null)
            {
                switch (emotionSimulator.CurrentEmotion)
                {
                    case DemoEmotionSimulator.EmotionType.Happy:
                        emotionSpeedModifier = happySpeedBonus;
                        break;
                    case DemoEmotionSimulator.EmotionType.Sad:
                        emotionSpeedModifier = sadSpeedPenalty;
                        break;
                }
            }

            controller.Move(move * baseMoveSpeed * emotionSpeedModifier * Time.deltaTime);

            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                float mouseX = Mouse.current.delta.ReadValue().x;
                transform.Rotate(Vector3.up * mouseX * rotationSpeed * Time.deltaTime);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
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
            velocity = Vector3.zero;
            IsMoving = false;

            if (controller != null)
            {
                controller.enabled = true;
            }
        }

        private void OnDrawGizmos()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
            }
        }
    }
}
