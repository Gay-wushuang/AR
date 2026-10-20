#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class FuelCellMaterialPostprocessor : AssetPostprocessor
{
    private const string FuelCellAssetPath = "Assets/Models/sci-fi-fuel-cell-canister/source/Fuel Cell1.blend";
    private const string MaterialFolder = "Assets/Models/sci-fi-fuel-cell-canister/Materials";

    [InitializeOnLoadMethod]
    private static void ScheduleFuelCellMaterialSetup()
    {
        // Material assets are now rebuilt only from the Tools menu.
        // Creating or saving assets from an import callback makes Unity reimport the same
        // materials during refresh, which leaves the editor preview in a broken state.
    }

    private void OnPreprocessModel()
    {
        if (assetPath != FuelCellAssetPath)
        {
            return;
        }

        var importer = (ModelImporter)assetImporter;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
        importer.materialSearch = ModelImporterMaterialSearch.Local;
        importer.materialLocation = ModelImporterMaterialLocation.External;
    }

    private Material OnAssignMaterialModel(Material sourceMaterial, Renderer renderer)
    {
        if (assetPath != FuelCellAssetPath || sourceMaterial == null)
        {
            return null;
        }

        return GetMaterialForName(sourceMaterial.name) ?? sourceMaterial;
    }

    private static Material GetMaterialForName(string sourceName)
    {
        switch (sourceName)
        {
            case "Emission":
                return LoadMaterial("FuelCell_Emission");
            case "Glass":
                return LoadMaterial("FuelCell_Glass");
            case "metal":
                return LoadMaterial("FuelCell_Metal");
            case "metal 2":
                return LoadMaterial("FuelCell_RoughMetal");
            case "Dots Stroke":
                return LoadMaterial("FuelCell_DotsStroke");
            default:
                return null;
        }
    }

    [MenuItem("Tools/Fuel Cell/Rebuild Materials And Reimport")]
    public static void RebuildMaterialsAndReimport()
    {
        EnsureMaterials();
        AssetDatabase.ImportAsset(FuelCellAssetPath, ImportAssetOptions.ForceUpdate);
        Debug.Log("[FuelCell] Materials rebuilt and Fuel Cell1.blend reimported.");
    }

    private static void EnsureMaterials()
    {
        if (EditorApplication.isUpdating || EditorApplication.isCompiling)
        {
            Debug.LogWarning("[FuelCell] Wait until Unity finishes importing/compiling, then run Tools/Fuel Cell/Rebuild Materials And Reimport again.");
            return;
        }

        EnsureFolder(MaterialFolder);

        CreateOrUpdateLit("FuelCell_Emission", new Color(1f, 0.0804f, 0f, 1f), 0f, 0.35f, true, 6f, false);
        CreateOrUpdateLit("FuelCell_Glass", new Color(1f, 1f, 1f, 0.35f), 0f, 0.5f, false, 0f, true);
        CreateOrUpdateLit("FuelCell_Metal", new Color(0.3506f, 0.3506f, 0.3506f, 1f), 1f, 0.71f, false, 0f, false);
        CreateOrUpdateLit("FuelCell_RoughMetal", new Color(0.3588f, 0.3588f, 0.3588f, 1f), 0.77f, 0f, false, 0f, false);
        CreateOrUpdateLit("FuelCell_DotsStroke", new Color(0.8f, 0.8f, 0.8f, 1f), 0f, 0.6f, false, 0f, false);

        AssetDatabase.SaveAssets();
    }

    private static Material CreateOrUpdateLit(string materialName, Color baseColor, float metallic, float smoothness, bool emission, float emissionStrength, bool transparent)
    {
        var material = LoadMaterial(materialName);
        var litShader = FindLitShader();
        if (litShader == null)
        {
            return material;
        }

        if (material == null)
        {
            material = new Material(litShader);
            AssetDatabase.CreateAsset(material, $"{MaterialFolder}/{materialName}.mat");
        }
        else if (material.shader == null || material.shader.name == "Hidden/InternalErrorShader")
        {
            material.shader = litShader;
        }

        material.name = materialName;
        SetColor(material, "_BaseColor", "_Color", baseColor);
        SetFloat(material, "_Metallic", metallic);
        SetFloat(material, "_Smoothness", smoothness);

        if (emission)
        {
            material.EnableKeyword("_EMISSION");
            SetColor(material, "_EmissionColor", "_EmissionColor", baseColor * emissionStrength);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            material.DisableKeyword("_EMISSION");
            SetColor(material, "_EmissionColor", "_EmissionColor", Color.black);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }

        ConfigureTransparency(material, transparent);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void ConfigureTransparency(Material material, bool transparent)
    {
        if (transparent)
        {
            SetFloat(material, "_Surface", 1f);
            SetFloat(material, "_Blend", 0f);
            SetFloat(material, "_ZWrite", 0f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        else
        {
            SetFloat(material, "_Surface", 0f);
            SetFloat(material, "_ZWrite", 1f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = -1;
        }
    }

    private static Shader FindLitShader()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            Debug.LogError("[FuelCell] Universal Render Pipeline/Lit shader was not found. Fuel Cell materials were not rebuilt.");
        }
        return shader;
    }

    private static Material LoadMaterial(string materialName)
    {
        return AssetDatabase.LoadAssetAtPath<Material>($"{MaterialFolder}/{materialName}.mat");
    }

    private static void SetColor(Material material, string primaryProperty, string fallbackProperty, Color color)
    {
        if (material.HasProperty(primaryProperty))
        {
            material.SetColor(primaryProperty, color);
        }
        else if (material.HasProperty(fallbackProperty))
        {
            material.SetColor(fallbackProperty, color);
        }
    }

    private static void SetFloat(Material material, string property, float value)
    {
        if (material.HasProperty(property))
        {
            material.SetFloat(property, value);
        }
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        var parent = Path.GetDirectoryName(folder)?.Replace("\\", "/");
        var name = Path.GetFileName(folder);
        if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(name))
        {
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
