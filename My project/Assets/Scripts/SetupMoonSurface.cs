using UnityEngine;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 自动创建月球表面场景（优化版）
/// 修复：平滑地形、自然光照、高密度网格
/// </summary>
[ExecuteInEditMode]
public class SetupMoonSurface : MonoBehaviour
{
    [Header("月球纹理（从 Assets/Models/moon 拖入）")]
    [SerializeField] private Texture2D moonDiffuse;
    [SerializeField] private Texture2D moonNormal;
    [SerializeField] private Texture2D moonRoughness;
    [SerializeField] private Texture2D moonDisplacement;

    [Header("地面设置")]
    [SerializeField] private float groundSize = 200f;
    [SerializeField] private Color groundColor = Color.white;
    [SerializeField] [Range(4, 128)] private int groundSubdivisions = 64;

    [Header("地形噪声设置")]
    [SerializeField] private float terrainHeight = 0.3f;
    [SerializeField] private float terrainScale = 8f;
    [SerializeField] private int terrainOctaves = 4;
    [SerializeField] private float terrainPersistence = 0.5f;
    [SerializeField] private float terrainLacunarity = 2f;

    [Header("光照设置")]
    [SerializeField] private float sunIntensity = 1.5f;
    [SerializeField] private Vector3 sunRotation = new Vector3(50f, -30f, 0f);
    [SerializeField] private Color ambientColor = new Color(0.15f, 0.15f, 0.18f);
    [SerializeField] private float ambientIntensity = 0.8f;

    [Header("相机设置")]
    [SerializeField] private Vector3 cameraPosition = new Vector3(0f, 2f, -5f);
    
    [Header("🎯 智能缩放设置（保持视觉质量）")]
    [Tooltip("你满意的视觉效果对应的 Scale（保持纹理清晰度）")]
    [SerializeField] private float visualReferenceScale = 0.1f;
    [Tooltip("你想要的有效覆盖范围（米）")]
    [SerializeField] private float desiredEffectiveSize = 200f;

#if UNITY_EDITOR
    [ContextMenu("创建月球场景")]
    public void CreateMoonScene()
    {
        CreateGround();
        CreateSunlight();
        SetupAmbientLight();
        SetupCamera();
        Debug.Log("✅ 月球场景创建完成！（优化版）");
    }

    [ContextMenu("🌙 智能缩放（正确逻辑）")]
    public void SmartScaleGround_Correct()
    {
        var ground = GameObject.Find("MoonSurface");
        if (ground == null)
        {
            Debug.LogWarning("未找到 MoonSurface，请先创建月球场景");
            return;
        }

        // ========== 核心逻辑（修正版）==========
        // 1. 保持几何 Scale = visualReferenceScale（你满意的视觉效果）
        ground.transform.localScale = new Vector3(visualReferenceScale, 1f, visualReferenceScale);
        
        // 2. 计算当前几何尺寸
        float currentGeometricSize = visualReferenceScale * 10f;  // Unity Plane 默认 10m
        
        // 3. 计算需要的纹理重复倍数
        float tilingMultiplier = desiredEffectiveSize / currentGeometricSize;
        
        // ========== 应用修改 ==========
        var renderer = ground.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            Material mat = renderer.material;
            
            // 设置纹理 Tiling（让纹理重复覆盖更大区域）
            // 从 (1,1) 开始，乘以需要的倍数
            Vector2 newTiling = new Vector2(tilingMultiplier, tilingMultiplier);
            mat.mainTextureScale = newTiling;
            
            Debug.Log($"📐 智能缩放分析（修正逻辑）:");
            Debug.Log($"   保持几何 Scale = {visualReferenceScale} → 实际几何尺寸 = {currentGeometricSize:F1}m");
            Debug.Log($"   目标有效覆盖 = {desiredEffectiveSize:F1}m");
            Debug.Log($"   纹理重复倍数 = {tilingMultiplier:F1}x");
            Debug.Log($"✅ 材质 Tiling 已设置: ({newTiling.x:F1}, {newTiling.y:F1})");
            
            // 如果有 Normal Map，也同步调整
            if (mat.HasProperty("_BumpMap"))
            {
                mat.SetTextureScale("_BumpMap", newTiling);
            }
        }
        
        // ========== 调整地形噪声（让起伏也匹配新范围）==========
        AdjustTerrainForNewScale(tilingMultiplier);
        
        Debug.Log($"✅ 月球表面智能缩放完成！");
        Debug.Log($"   Scale 保持 = {visualReferenceScale} (视觉不变)");
        Debug.Log($"   有效覆盖 = {desiredEffectiveSize}m");
        Debug.Log($"   纹理清晰度 = 不变 (Tiling 自动调整)");
        
        EditorUtility.SetDirty(ground);
    }
    
    private void AdjustTerrainForNewScale(float scaleMultiplier)
    {
        // 当地面"逻辑上"变大时，需要调整地形噪声以保持视觉效果
        // 我们需要增大 terrainScale 和 terrainHeight
        
        terrainScale = terrainScale * Mathf.Sqrt(scaleMultiplier);
        terrainScale = Mathf.Min(terrainScale, 100f);  // 上限保护
        Debug.Log($"   地形 noise scale: {terrainScale:F1}");
        
        terrainHeight = terrainHeight * Mathf.Sqrt(scaleMultiplier);
        terrainHeight = Mathf.Min(terrainHeight, 3f);  // 上限保护
        Debug.Log($"   地形 height: {terrainHeight:F2}m");
        
        // 重新应用地形（需要重新生成 mesh）
        Debug.Log("💡 提示：地形参数已调整。如需看到效果，请点击'清除场景'然后'创建月球场景'重新生成");
    }

    [ContextMenu("📊 显示当前诊断")]
    public void DiagnoseCurrentGround()
    {
        var ground = GameObject.Find("MoonSurface");
        if (ground == null)
        {
            Debug.Log("\n🌙 MoonSurface: ❌ 未找到");
            return;
        }

        Vector3 scale = ground.transform.localScale;
        float actualSizeX = scale.x * 10f;
        float actualSizeZ = scale.z * 10f;
        
        var renderer = ground.GetComponent<Renderer>();
        Vector2 tiling = Vector2.one;
        if (renderer != null && renderer.material != null)
        {
            tiling = renderer.material.mainTextureScale;
        }

        Debug.Log("\n" + "=".PadRight(60, '='));
        Debug.Log("📊 MoonSurface 诊断报告");
        Debug.Log("=".PadRight(60, '='));
        Debug.Log($"   几何 Scale: ({scale.x:F3}, {scale.y:F3}, {scale.z:F3})");
        Debug.Log($"   实际几何尺寸: {actualSizeX:F1}m × {actualSizeZ:F1}m");
        Debug.Log($"   纹理 Tiling: ({tiling.x:F1}, {tiling.y:F1})");
        Debug.Log($"   有效纹理覆盖: {actualSizeX * tiling.x:F1}m × {actualSizeZ * tiling.y:F1}m");
        
        bool isSmall = actualSizeX * tiling.x < 50f;
        bool isTextureStretched = actualSizeX > 20f && tiling.x <= 1f;
        
        if (isSmall)
        {
            Debug.Log($"   ⚠️  问题：有效覆盖太小");
            Debug.Log($"   💡 解决：使用'🌙 智能缩放（正确逻辑）'");
        }
        else if (isTextureStretched)
        {
            Debug.Log($"   ❌ 严重问题：纹理被拉伸！");
            Debug.Log($"   💡 当前状态：{actualSizeX:F0}m 地面只用 {tiling.x:F1} 个纹理实例");
            Debug.Log($"   💡 推荐：使用'🌙 智能缩放（正确逻辑）'自动调整");
        }
        else
        {
            Debug.Log($"   ✅ 状态良好：尺寸与纹理密度协调");
        }
        
        Debug.Log("=".PadRight(60, '='));
    }
#endif

    private void CreateGround()
    {
        var existing = GameObject.Find("MoonSurface");
        if (existing != null)
        {
            Debug.LogWarning("月球表面已存在，跳过创建");
            return;
        }

        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "MoonSurface";
        ground.transform.localScale = new Vector3(groundSize / 10f, 1f, groundSize / 10f);

        SubdivideMesh(ground);
        ApplySmoothTerrain(ground);

        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = "MoonMaterial_Regolith";

        if (moonDiffuse != null)
        {
            material.mainTexture = moonDiffuse;
            material.color = new Color(0.75f, 0.73f, 0.70f);
            Debug.Log($"应用漫反射贴图: {moonDiffuse.name}");
        }
        else
        {
            material.color = new Color(0.45f, 0.43f, 0.40f);
        }

        if (moonNormal != null)
        {
            material.SetTexture("_BumpMap", moonNormal);
            material.EnableKeyword("_NORMALMAP");
            material.SetFloat("_BumpScale", 2.0f);
        }

        if (moonRoughness != null)
        {
            material.SetTexture("_MetallicGlossMap", moonRoughness);
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.SetFloat("_GlossMapScale", 0.0f);
            material.SetFloat("_Smoothness", 0.0f);
        }
        else
        {
            material.SetFloat("_Smoothness", 0.0f);
        }

        if (moonDisplacement != null)
        {
            material.SetTexture("_ParallaxMap", moonDisplacement);
            material.EnableKeyword("_PARALLAXMAP");
            material.SetFloat("_Parallax", 0.005f);
        }

        material.SetFloat("_Metallic", 0.0f);
        material.doubleSidedGI = true;

        var renderer = ground.GetComponent<Renderer>();
        renderer.material = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;

        Debug.Log($"✅ 月球表面创建完成 | 月壤质感模式 | 网格细分: {groundSubdivisions}x{groundSubdivisions} | 顶点数: {(groundSubdivisions+1)*(groundSubdivisions+1)}");
    }

    private void SubdivideMesh(GameObject ground)
    {
        var meshFilter = ground.GetComponent<MeshFilter>();
        var originalMesh = meshFilter.sharedMesh;

        var mesh = new Mesh();
        mesh.name = "MoonSurface_HighPoly";

        int resolution = groundSubdivisions + 1;
        var vertices = new Vector3[resolution * resolution];
        var uvs = new Vector2[resolution * resolution];
        var triangles = new int[groundSubdivisions * groundSubdivisions * 6];

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int index = z * resolution + x;
                float px = (float)x / groundSubdivisions * 10f - 5f;
                float pz = (float)z / groundSubdivisions * 10f - 5f;

                vertices[index] = new Vector3(px, 0, pz);
                uvs[index] = new Vector2((float)x / groundSubdivisions, (float)z / groundSubdivisions);
            }
        }

        int triIndex = 0;
        for (int z = 0; z < groundSubdivisions; z++)
        {
            for (int x = 0; x < groundSubdivisions; x++)
            {
                int topLeft = z * resolution + x;
                int topRight = topLeft + 1;
                int bottomLeft = (z + 1) * resolution + x;
                int bottomRight = bottomLeft + 1;

                triangles[triIndex++] = topLeft;
                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = topRight;

                triangles[triIndex++] = topRight;
                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = bottomRight;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.sharedMesh = mesh;
    }

    private void ApplySmoothTerrain(GameObject ground)
    {
        var meshFilter = ground.GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        var mesh = meshFilter.sharedMesh;
        var vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            float x = vertices[i].x;
            float z = vertices[i].z;

            float height = 0f;
            float amplitude = terrainHeight;
            float frequency = 1f / terrainScale;

            for (int octave = 0; octave < terrainOctaves; octave++)
            {
                height += Mathf.PerlinNoise(x * frequency, z * frequency) * amplitude;
                amplitude *= terrainPersistence;
                frequency *= terrainLacunarity;
            }

            vertices[i].y += height;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        Debug.Log($"✅ 平滑地形已应用 | 噪声八度: {terrainOctaves} | 最大高度: ±{terrainHeight}m");
    }

    private void CreateSunlight()
    {
        var existing = GameObject.Find("SunLight");
        if (existing != null)
        {
            Debug.LogWarning("光源已存在，跳过创建");
            return;
        }

        var lightGO = new GameObject("SunLight");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.99f, 0.98f);
        light.intensity = sunIntensity;
        light.shadows = LightShadows.Soft;
        light.shadowResolution = UnityEngine.Rendering.LightShadowResolution.High;
        light.shadowStrength = 0.9f;

        lightGO.transform.rotation = Quaternion.Euler(sunRotation);

        Debug.Log("✅ 太阳光创建完成（月球高对比度模式）");
    }

    private void SetupAmbientLight()
    {
        RenderSettings.ambientSkyColor = new Color(0.02f, 0.02f, 0.025f);
        RenderSettings.ambientIntensity = 0.3f;

        var probeGO = GameObject.Find("SceneReflectionProbe");
        if (probeGO == null)
        {
            probeGO = new GameObject("SceneReflectionProbe");
            var probe = probeGO.AddComponent<ReflectionProbe>();
            probe.resolution = 256;
            probe.backgroundColor = new Color(0.01f, 0.01f, 0.015f, 1f);
            probe.size = new Vector3(50f, 20f, 50f);
            probe.blendDistance = 5f;
        }

        Debug.Log("✅ 环境光和反射探针已配置（低强度太空模式）");
    }

    private void SetupCamera()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = cameraPosition;
            cam.transform.LookAt(Vector3.zero);

            var camData = cam.GetUniversalAdditionalCameraData();
            if (camData != null)
            {
                camData.renderPostProcessing = true;
            }

            Debug.Log("✅ 相机位置已设置");
        }
        else
        {
            Debug.LogWarning("⚠️ 未找到主相机，请手动调整");
        }
    }

#if UNITY_EDITOR
    [ContextMenu("清除场景")]
    public void ClearScene()
    {
        var ground = GameObject.Find("MoonSurface");
        var light = GameObject.Find("SunLight");
        var probe = GameObject.Find("SceneReflectionProbe");

        if (ground != null) UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(ground);
        if (light != null) UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(light);
        if (probe != null) UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(probe);

        Debug.Log("🗑️ 场景已清除");
    }
    
    [ContextMenu("🔄 重置推荐值")]
    public void ResetRecommendedValues()
    {
        visualReferenceScale = 0.1f;
        desiredEffectiveSize = 200f;
        groundSize = 200f;
        terrainHeight = 0.3f;
        terrainScale = 8f;
        
        Debug.Log("✅ 已重置为推荐值");
    }
#endif
}
