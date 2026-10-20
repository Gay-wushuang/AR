using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace EEGAI.Demo
{
    [DefaultExecutionOrder(-1000)]
    public class FuelCellRuntimeMaterialFixer : MonoBehaviour
    {
        private static Material emissionMaterial;
        private static Material glassMaterial;
        private static Material metalMaterial;
        private static Material roughMetalMaterial;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            ApplyToScene();
            SceneManager.sceneLoaded += (_, __) => ApplyToScene();
        }

        private static void ApplyToScene()
        {
            EnsureMaterials();
            if (emissionMaterial == null || glassMaterial == null || metalMaterial == null || roughMetalMaterial == null)
            {
                Debug.LogWarning("[FuelCell] Runtime material fixer skipped because compatible shaders were not found.");
                return;
            }

            foreach (Renderer renderer in Object.FindObjectsOfType<Renderer>(true))
            {
                if (!IsFuelCellRenderer(renderer))
                {
                    continue;
                }

                var materials = renderer.sharedMaterials;
                if (materials == null || materials.Length < 4)
                {
                    continue;
                }

                // Imported submesh order can change. Never overwrite valid authored materials.
                for (int i = 0; i < materials.Length; i++)
                {
                    var source = materials[i];
                    if (source == null || (source.shader != null && source.shader.isSupported &&
                        source.shader.name.StartsWith("Universal Render Pipeline/"))) continue;
                    string name = source.name.ToLowerInvariant();
                    if (name.Contains("emission")) materials[i] = emissionMaterial;
                    else if (name.Contains("glass")) materials[i] = glassMaterial;
                    else if (name.Contains("rough")) materials[i] = roughMetalMaterial;
                    else if (name.Contains("metal")) materials[i] = metalMaterial;
                }
                renderer.sharedMaterials = materials;
            }
        }

        private static bool IsFuelCellRenderer(Renderer renderer)
        {
            Transform current = renderer.transform;
            while (current != null)
            {
                if (current.name.ToLowerInvariant().Contains("fuel cell"))
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static void EnsureMaterials()
        {
            if (emissionMaterial != null)
            {
                return;
            }

            bool usingScriptablePipeline = GraphicsSettings.currentRenderPipeline != null;
            Shader litShader = usingScriptablePipeline
                ? Shader.Find("Universal Render Pipeline/Lit")
                : Shader.Find("Standard");
            Shader unlitShader = usingScriptablePipeline
                ? Shader.Find("Universal Render Pipeline/Unlit")
                : Shader.Find("Unlit/Color");

            if (litShader == null)
            {
                litShader = Shader.Find("Standard");
            }

            if (unlitShader == null)
            {
                unlitShader = litShader;
            }

            if (litShader == null || unlitShader == null)
            {
                return;
            }

            emissionMaterial = CreateMaterial("FuelCell_Runtime_Emission", unlitShader, new Color(1f, 0.08f, 0f, 1f), 0f, 0.35f, true);
            glassMaterial = CreateMaterial("FuelCell_Runtime_Glass", litShader, new Color(1f, 1f, 1f, 0.35f), 0f, 0.5f, false);
            metalMaterial = CreateMaterial("FuelCell_Runtime_Metal", litShader, new Color(0.35f, 0.35f, 0.35f, 1f), 1f, 0.71f, false);
            roughMetalMaterial = CreateMaterial("FuelCell_Runtime_RoughMetal", litShader, new Color(0.36f, 0.36f, 0.36f, 1f), 0.77f, 0f, false);

            ConfigureTransparent(glassMaterial);
        }

        private static Material CreateMaterial(string name, Shader shader, Color color, float metallic, float smoothness, bool emission)
        {
            var material = new Material(shader)
            {
                name = name,
                color = color
            };

            SetColor(material, "_BaseColor", color);
            SetColor(material, "_Color", color);
            SetFloat(material, "_Metallic", metallic);
            SetFloat(material, "_Smoothness", smoothness);

            if (emission)
            {
                material.EnableKeyword("_EMISSION");
                SetColor(material, "_EmissionColor", color * 5f);
            }

            return material;
        }

        private static void ConfigureTransparent(Material material)
        {
            SetFloat(material, "_Surface", 1f);
            SetFloat(material, "_ZWrite", 0f);
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        private static void SetColor(Material material, string property, Color value)
        {
            if (material.HasProperty(property))
            {
                material.SetColor(property, value);
            }
        }

        private static void SetFloat(Material material, string property, float value)
        {
            if (material.HasProperty(property))
            {
                material.SetFloat(property, value);
            }
        }
    }
}
