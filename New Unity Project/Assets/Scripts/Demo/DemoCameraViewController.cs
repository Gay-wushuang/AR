using UnityEngine;
using UnityEngine.InputSystem;

namespace EEGAI.Demo
{
    public class DemoCameraViewController : MonoBehaviour
    {
        private enum ViewMode
        {
            FirstPerson,
            SecondPerson,
            ThirdPerson
        }

        private Camera renderCamera;
        private Transform firstPersonAnchor;
        private Transform secondPersonAnchor;
        private Transform thirdPersonAnchor;
        private ViewMode currentView = ViewMode.FirstPerson;
        private DemoGameState gameState;
        private float noticeTimer;
        private Vector3 firstPersonLocalPosition;
        private Quaternion firstPersonLocalRotation;
        private bool hasFirstPersonPose;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindObjectOfType<DemoCameraViewController>() != null)
            {
                return;
            }

            var controller = new GameObject("DemoCameraViewController");
            controller.AddComponent<DemoCameraViewController>();
        }

        private void Start()
        {
            CacheCameras();
            gameState = FindAnyObjectByType<DemoGameState>();

            if (gameState != null)
            {
                gameState.OnReset += ResetToDefaultView;
                gameState.OnStarted += ResetToDefaultView;
            }

            ApplyView();
        }

        private void Update()
        {
            if (gameState == null)
            {
                gameState = FindAnyObjectByType<DemoGameState>();
                if (gameState != null)
                {
                    gameState.OnReset += ResetToDefaultView;
                    gameState.OnStarted += ResetToDefaultView;
                }
            }

            if (renderCamera == null || firstPersonAnchor == null || secondPersonAnchor == null || thirdPersonAnchor == null)
            {
                CacheCameras();
            }

            if (Keyboard.current != null &&
                Keyboard.current.f5Key.wasPressedThisFrame &&
                CanSwitchView())
            {
                CycleView();
            }

            if (CanOwnGameplayCameras())
            {
                ApplyView();
            }

            if (noticeTimer > 0f)
            {
                noticeTimer -= Time.unscaledDeltaTime;
            }
        }

        private void LateUpdate()
        {
            if (CanOwnGameplayCameras())
            {
                ApplyView();
            }
        }

        private void CacheCameras()
        {
            Camera firstPersonCamera = FindCameraByName("main Camera");
            Camera secondPersonCamera = FindCameraByName("front Camera");
            Camera thirdPersonCamera = FindCameraByName("back Camera");

            renderCamera = firstPersonCamera != null ? firstPersonCamera : Camera.main;
            firstPersonAnchor = firstPersonCamera != null ? firstPersonCamera.transform : renderCamera != null ? renderCamera.transform : null;
            secondPersonAnchor = secondPersonCamera != null ? secondPersonCamera.transform : null;
            thirdPersonAnchor = thirdPersonCamera != null ? thirdPersonCamera.transform : null;

            DisableAnchorCamera(secondPersonCamera);
            DisableAnchorCamera(thirdPersonCamera);

            if (renderCamera != null)
            {
                if (!hasFirstPersonPose)
                {
                    firstPersonLocalPosition = renderCamera.transform.localPosition;
                    firstPersonLocalRotation = renderCamera.transform.localRotation;
                    hasFirstPersonPose = true;
                }

                renderCamera.enabled = true;
                renderCamera.targetDisplay = 0;
                renderCamera.tag = "MainCamera";

                var listener = renderCamera.GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = true;
                }
            }
        }

        private static Camera FindCameraByName(string cameraName)
        {
            foreach (var camera in FindObjectsOfType<Camera>(true))
            {
                if (camera != null && string.Equals(camera.name, cameraName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return camera;
                }
            }

            return null;
        }

        private bool CanSwitchView()
        {
            return gameState != null &&
                (gameState.Status == DemoGameState.GameStatus.Playing ||
                 gameState.Status == DemoGameState.GameStatus.ReadyToLaunch) &&
                !DemoMainMenu.PauseMenuOpen;
        }

        private bool CanOwnGameplayCameras()
        {
            return gameState == null ||
                gameState.Status == DemoGameState.GameStatus.MainMenu ||
                gameState.Status == DemoGameState.GameStatus.Playing ||
                gameState.Status == DemoGameState.GameStatus.ReadyToLaunch;
        }

        private void CycleView()
        {
            currentView = currentView switch
            {
                ViewMode.FirstPerson => ViewMode.SecondPerson,
                ViewMode.SecondPerson => ViewMode.ThirdPerson,
                _ => ViewMode.FirstPerson
            };

            noticeTimer = 1.6f;
            ApplyView();
            Debug.Log($"[DemoCamera] Switched to {GetViewLabel()}");
        }

        private void ResetToDefaultView()
        {
            currentView = ViewMode.FirstPerson;
            ApplyView();
        }

        private void ApplyView()
        {
            if (renderCamera == null)
            {
                return;
            }

            if (currentView == ViewMode.FirstPerson)
            {
                RestoreFirstPersonPose();
            }
            else
            {
                Transform target = GetActiveAnchor();
                if (target != null)
                {
                    ApplyAnchorPose(target);
                }
            }

            renderCamera.enabled = true;
            renderCamera.targetDisplay = 0;
            renderCamera.tag = "MainCamera";

            var listener = renderCamera.GetComponent<AudioListener>();
            if (listener != null)
            {
                listener.enabled = true;
            }
        }

        private Transform GetActiveAnchor()
        {
            return currentView switch
            {
                ViewMode.SecondPerson => secondPersonAnchor != null ? secondPersonAnchor : firstPersonAnchor,
                ViewMode.ThirdPerson => thirdPersonAnchor != null ? thirdPersonAnchor : firstPersonAnchor,
                _ => firstPersonAnchor
            };
        }

        private void RestoreFirstPersonPose()
        {
            if (!hasFirstPersonPose || renderCamera == null)
            {
                return;
            }

            renderCamera.transform.localPosition = firstPersonLocalPosition;
            renderCamera.transform.localRotation = firstPersonLocalRotation;
        }

        private void ApplyAnchorPose(Transform anchor)
        {
            if (renderCamera == null || anchor == null)
            {
                return;
            }

            if (anchor.parent == renderCamera.transform.parent)
            {
                renderCamera.transform.localPosition = anchor.localPosition;
                renderCamera.transform.localRotation = anchor.localRotation;
            }
            else
            {
                renderCamera.transform.SetPositionAndRotation(anchor.position, anchor.rotation);
            }
        }

        private static void DisableAnchorCamera(Camera camera)
        {
            if (camera == null)
            {
                return;
            }

            camera.enabled = false;
            camera.tag = "Untagged";

            var listener = camera.GetComponent<AudioListener>();
            if (listener != null)
            {
                listener.enabled = false;
            }
        }

        private string GetViewLabel()
        {
            return currentView switch
            {
                ViewMode.SecondPerson => "SECOND PERSON",
                ViewMode.ThirdPerson => "THIRD PERSON",
                _ => "FIRST PERSON"
            };
        }

        private void OnGUI()
        {
            if (!DemoUIVisibilityController.ShowVisualizations || noticeTimer <= 0f)
            {
                return;
            }

            Rect rect = new Rect(Screen.width * 0.5f - 115f, 22f, 230f, 34f);
            GUI.color = new Color(0f, 0f, 0f, 0.68f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(rect, $"VIEW: {GetViewLabel()}  [F5]", new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            });
        }

        private void OnDestroy()
        {
            if (gameState != null)
            {
                gameState.OnReset -= ResetToDefaultView;
                gameState.OnStarted -= ResetToDefaultView;
            }
        }
    }
}
