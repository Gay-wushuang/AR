using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EEGAI.Demo
{
    public class RocketLaunchController : MonoBehaviour
    {
        [Header("Launch Settings")]
        [SerializeField] private float interactionDistance = 8f;
        [SerializeField] private float launchHeight = 38f;
        [SerializeField] private float launchDuration = 5f;
        [SerializeField] private AnimationCurve launchCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Shockwave Settings")]
        [SerializeField] private float shockwaveScaleMultiplier = 4f;
        [SerializeField] private float shockwaveLift = 3f;
        [SerializeField] private Color shockwaveStartColor = new Color(1f, 0.38f, 0.04f, 0.75f);
        [SerializeField] private Color shockwaveEndColor = new Color(1f, 0.82f, 0.22f, 0f);

        private readonly List<Transform> shockwaveParts = new List<Transform>();
        private readonly Dictionary<Transform, Vector3> initialPositions = new Dictionary<Transform, Vector3>();
        private readonly Dictionary<Transform, Vector3> initialScales = new Dictionary<Transform, Vector3>();
        private readonly Dictionary<Renderer, Material> runtimeMaterials = new Dictionary<Renderer, Material>();

        private DemoGameState gameState;
        private Transform player;
        private Camera launchCamera;
        private AudioListener launchAudioListener;
        private readonly Dictionary<Camera, bool> previousCameraStates = new Dictionary<Camera, bool>();
        private readonly Dictionary<AudioListener, bool> previousAudioListenerStates = new Dictionary<AudioListener, bool>();
        private Vector3 initialRocketPosition;
        private Quaternion initialRocketRotation;
        private Vector3 initialRocketScale;
        private bool isLaunching;
        private ParticleSystem exhaust;
        private Material exhaustMaterial;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            var rocket = GameObject.Find("Rocket");
            if (rocket != null && rocket.GetComponent<RocketLaunchController>() == null)
            {
                rocket.AddComponent<RocketLaunchController>();
            }
        }

        private void Awake()
        {
            initialRocketPosition = transform.position;
            initialRocketRotation = transform.rotation;
            initialRocketScale = transform.localScale;
            CacheShockwaveParts();
        }

        private void Start()
        {
            gameState = FindAnyObjectByType<DemoGameState>();
            player = Camera.main != null ? Camera.main.transform : null;
            EnsureLaunchCamera();
            PrepareShockwaveMaterials();
            SetShockwaveAlpha(0f);
        }

        private void Update()
        {
            if (EnergyLinkPuzzle.Active) return;
            if (gameState == null)
            {
                gameState = FindAnyObjectByType<DemoGameState>();
            }

            if (player == null && Camera.main != null)
            {
                player = Camera.main.transform;
            }

            if (gameState == null || gameState.Status != DemoGameState.GameStatus.ReadyToLaunch || isLaunching)
            {
                return;
            }

            if (IsPlayerInRange() && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                StartCoroutine(LaunchSequence());
            }
        }

        private bool IsPlayerInRange()
        {
            return player != null && Vector3.Distance(player.position, transform.position) <= interactionDistance;
        }

        private IEnumerator LaunchSequence()
        {
            isLaunching = true;

            if (gameState == null)
            {
                gameState = FindAnyObjectByType<DemoGameState>();
            }

            if (gameState == null)
            {
                Debug.LogError("[Demo] Cannot launch rocket: DemoGameState was not found.");
                isLaunching = false;
                yield break;
            }

            gameState.BeginLaunch();

            // Visual effects must never be able to abort the launch/victory flow.
            try
            {
                SetAstronautModelsVisible(false);
                ActivateLaunchCamera();
                EnsureExhaust();
                if (exhaust != null) exhaust.Play();
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"[Demo] Rocket effects failed, continuing launch: {exception.Message}");
            }

            Vector3 start = initialRocketPosition;
            Vector3 end = initialRocketPosition + Vector3.up * launchHeight;

            try
            {
                float elapsed = 0f;
                while (elapsed < launchDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / launchDuration);
                    float eased = launchCurve != null ? launchCurve.Evaluate(t) : t;

                    transform.position = Vector3.Lerp(start, end, eased);
                    transform.rotation = initialRocketRotation;

                    yield return null;
                }

                transform.position = end;
            }
            finally
            {
                if (exhaust != null)
                {
                    exhaust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }

                isLaunching = false;

                // If ResetLaunch/ResetGame stopped this coroutine, the state is no longer
                // Launching and must not be converted back into a victory.
                if (gameState != null && gameState.Status == DemoGameState.GameStatus.Launching)
                {
                    gameState.CompleteLaunch();
                }
            }
        }

        public void ResetLaunch()
        {
            StopAllCoroutines();
            if (exhaust != null) exhaust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            isLaunching = false;
            transform.position = initialRocketPosition;
            transform.rotation = initialRocketRotation;
            transform.localScale = initialRocketScale;
            RestorePreviousCameras();
            SetAstronautModelsVisible(true);

            foreach (var part in shockwaveParts)
            {
                if (part == null) continue;
                part.localPosition = initialPositions[part];
                part.localScale = initialScales[part];
            }

            SetShockwaveAlpha(0f);
        }

        private void EnsureExhaust()
        {
            if (exhaust != null) return;
            var nozzle = new GameObject("DownwardRocketExhaust");
            nozzle.transform.SetParent(transform, false);
            var visual = transform.Find("CrewedRocketVisual");
            var renderers = visual != null ? visual.GetComponentsInChildren<Renderer>() : GetComponentsInChildren<Renderer>();
            Bounds bounds = new Bounds(transform.position, Vector3.zero);
            if (renderers.Length > 0)
            {
                bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            }
            nozzle.transform.position = new Vector3(bounds.center.x, bounds.min.y + 0.05f, bounds.center.z);
            // Particle cone emits along local +Z; align that axis with world down.
            nozzle.transform.rotation = Quaternion.LookRotation(Vector3.down, transform.forward);
            exhaust = nozzle.AddComponent<ParticleSystem>();
            exhaust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = exhaust.main;
            main.playOnAwake = false;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(7f, 11f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.24f);
            main.startColor = new Color(1f, 0.65f, 0.16f, 0.85f);
            main.maxParticles = 300;
            var emission = exhaust.emission; emission.rateOverTime = 180f;
            var shape = exhaust.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 7f; shape.radius = 0.12f;
            var size = exhaust.sizeOverLifetime; size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
            var color = exhaust.colorOverLifetime; color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.3f, 0.02f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            color.color = gradient;
            var particleRenderer = exhaust.GetComponent<ParticleSystemRenderer>();

            // Shader.Find only works in a build when the shader is included in the build.
            // Keep a fallback chain and never pass null to the Material constructor.
            Shader exhaustShader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Standard");

            if (exhaustShader == null)
            {
                Debug.LogWarning("[Demo] Rocket exhaust shader was stripped from the build; launching without exhaust material.");
                return;
            }

            exhaustMaterial = new Material(exhaustShader);
            ConfigureTransparentMaterial(exhaustMaterial);
            SetMaterialColor(exhaustMaterial, Color.white);
            particleRenderer.sharedMaterial = exhaustMaterial;
            particleRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            particleRenderer.lengthScale = 2f;
            particleRenderer.velocityScale = 0.03f;
        }

        private void OnDestroy()
        {
            if (exhaustMaterial != null) Destroy(exhaustMaterial);
            foreach (var material in runtimeMaterials.Values) if (material != null) Destroy(material);
        }

        private void EnsureLaunchCamera()
        {
            Transform existing = transform.Find("RocketLaunchCamera");
            if (existing != null)
            {
                launchCamera = existing.GetComponent<Camera>();
                launchAudioListener = existing.GetComponent<AudioListener>();
            }

            if (launchCamera == null)
            {
                var cameraObject = new GameObject("RocketLaunchCamera");
                cameraObject.transform.SetParent(transform, false);
                cameraObject.transform.localPosition = new Vector3(0f, 4.5f, -13f);
                cameraObject.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
                launchCamera = cameraObject.AddComponent<Camera>();
                launchAudioListener = cameraObject.AddComponent<AudioListener>();
            }

            Camera source = Camera.main;
            if (source != null)
            {
                launchCamera.fieldOfView = source.fieldOfView;
                launchCamera.nearClipPlane = source.nearClipPlane;
                launchCamera.farClipPlane = source.farClipPlane;
                launchCamera.clearFlags = source.clearFlags;
                launchCamera.backgroundColor = source.backgroundColor;
                launchCamera.cullingMask = source.cullingMask;
            }

            launchCamera.depth = 100f;
            launchCamera.enabled = false;
            if (launchAudioListener != null)
            {
                launchAudioListener.enabled = false;
            }
        }

        private void ActivateLaunchCamera()
        {
            EnsureLaunchCamera();
            previousCameraStates.Clear();
            previousAudioListenerStates.Clear();

            Camera[] cameras = FindObjectsOfType<Camera>(true);
            AudioListener[] listeners = FindObjectsOfType<AudioListener>(true);

            foreach (var camera in cameras)
            {
                if (camera == null) continue;
                previousCameraStates[camera] = camera.enabled;
                if (camera != launchCamera)
                {
                    camera.enabled = false;
                }
            }

            foreach (var listener in listeners)
            {
                if (listener == null) continue;
                previousAudioListenerStates[listener] = listener.enabled;
                if (listener != launchAudioListener)
                {
                    listener.enabled = false;
                }
            }

            launchCamera.enabled = true;
            if (launchAudioListener != null)
            {
                launchAudioListener.enabled = true;
            }
        }

        private void RestorePreviousCameras()
        {
            if (launchCamera != null)
            {
                launchCamera.enabled = false;
            }

            if (launchAudioListener != null)
            {
                launchAudioListener.enabled = false;
            }

            foreach (var pair in previousCameraStates)
            {
                var camera = pair.Key;
                if (camera != null && camera != launchCamera)
                {
                    camera.enabled = pair.Value;
                }
            }

            foreach (var pair in previousAudioListenerStates)
            {
                var listener = pair.Key;
                if (listener != null && listener != launchAudioListener)
                {
                    listener.enabled = pair.Value;
                }
            }

            EnsureGameplayCameraEnabled();
            previousCameraStates.Clear();
            previousAudioListenerStates.Clear();
        }

        private void EnsureGameplayCameraEnabled()
        {
            Camera fallbackCamera = null;
            bool hasEnabledGameplayCamera = false;

            foreach (var camera in FindObjectsOfType<Camera>(true))
            {
                if (camera == null || camera == launchCamera)
                {
                    continue;
                }

                if (fallbackCamera == null ||
                    (camera.CompareTag("MainCamera") && !fallbackCamera.CompareTag("MainCamera")))
                {
                    fallbackCamera = camera;
                }

                if (camera.enabled && camera.gameObject.activeInHierarchy)
                {
                    hasEnabledGameplayCamera = true;
                }
            }

            if (!hasEnabledGameplayCamera && fallbackCamera != null)
            {
                fallbackCamera.enabled = true;
                var listener = fallbackCamera.GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = true;
                }
            }
        }

        private void CacheShockwaveParts()
        {
            shockwaveParts.Clear();
            initialPositions.Clear();
            initialScales.Clear();

            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child == transform) continue;

                if (child.name.StartsWith("Cube.0"))
                {
                    shockwaveParts.Add(child);
                    initialPositions[child] = child.localPosition;
                    initialScales[child] = child.localScale;
                }
            }
        }

        private void PrepareShockwaveMaterials()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            foreach (var part in shockwaveParts)
            {
                if (part == null) continue;
                var renderer = part.GetComponent<Renderer>();
                if (renderer == null || shader == null) continue;

                var material = new Material(shader) { name = $"{part.name}_ShockwaveRuntime" };
                ConfigureTransparentMaterial(material);
                SetMaterialColor(material, shockwaveStartColor);
                renderer.material = material;
                runtimeMaterials[renderer] = material;
                renderer.enabled = true;
            }
        }

        private void AnimateShockwave(float t)
        {
            foreach (var part in shockwaveParts)
            {
                if (part == null) continue;

                Vector3 origin = initialPositions[part];
                Vector3 radial = new Vector3(origin.x, 0f, origin.z);
                Vector3 direction = radial.sqrMagnitude > 0.001f ? radial.normalized : Vector3.forward;

                part.localPosition = origin + direction * shockwaveScaleMultiplier * t + Vector3.up * shockwaveLift * t;
                part.localScale = Vector3.Lerp(initialScales[part], initialScales[part] * (1.5f + shockwaveScaleMultiplier * 0.3f), t);

                var renderer = part.GetComponent<Renderer>();
                if (renderer != null && runtimeMaterials.TryGetValue(renderer, out var material))
                {
                    SetMaterialColor(material, Color.Lerp(shockwaveStartColor, shockwaveEndColor, t));
                }
            }
        }

        private void SetShockwaveAlpha(float alpha)
        {
            foreach (var part in shockwaveParts)
            {
                if (part == null) continue;
                var renderer = part.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = true;
                    if (runtimeMaterials.TryGetValue(renderer, out var material))
                    {
                        Color color = material.HasProperty("_BaseColor")
                            ? material.GetColor("_BaseColor")
                            : material.HasProperty("_Color")
                                ? material.GetColor("_Color")
                                : shockwaveStartColor;
                        color.a = alpha;
                        SetMaterialColor(material, color);
                    }
                }
            }
        }

        private static void ConfigureTransparentMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }

            if (material.HasProperty("_Blend"))
            {
                material.SetFloat("_Blend", 0f);
            }

            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        private static void SetAstronautModelsVisible(bool visible)
        {
            foreach (var astronaut in FindObjectsOfType<AstronautController>(true))
            {
                astronaut.SetModelVisible(visible);
            }
        }

        private static void SetMaterialColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        private void OnGUI()
        {
            if (EnergyLinkPuzzle.Active) return;
            if (DemoMainMenu.PauseMenuOpen)
            {
                return;
            }

            if (gameState == null || gameState.Status != DemoGameState.GameStatus.ReadyToLaunch || !IsPlayerInRange())
            {
                return;
            }

            Rect rect = new Rect(Screen.width * 0.5f - 150f, Screen.height - 120f, 300f, 54f);
            GUI.color = new Color(0f, 0f, 0f, 0.75f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;
            GUILayout.BeginArea(rect);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Press [E] to launch rocket", new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            });
            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
        }
    }
}
