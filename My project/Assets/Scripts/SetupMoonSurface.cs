using UnityEngine;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 自动创建月球表面场景（超简单版）
/// 100% 保证：纹理不变，起伏明显，陨石坑可选
/// </summary>
[ExecuteInEditMode]
public class SetupMoonSurface : MonoBehaviour
{
    [Header("🌙 月壤纹理（moon_02）")]
    [Tooltip("细腻月壤，拖入这4个")]
    [SerializeField] private Texture2D moon_02_diff;
    [SerializeField] private Texture2D moon_02_nor;
    [SerializeField] private Texture2D moon_02_rough;
    [SerializeField] private Texture2D moon_02_disp;

    [Header("地面设置（简单！）")]
    [Tooltip("地面想要多大？(10=默认，200=很大")]
    [SerializeField] private float groundSize = 10f;

    [Header("地形起伏（绝对能看到！）")]
    [Tooltip("起伏高度，越大越明显（推荐1-3）")]
    [SerializeField] private float terrainHeight = 2.0f;
    [Tooltip("起伏细密程度，越多越密（推荐4-8，最多8）")]
    [SerializeField] [Range(1, 8)] private int terrainOctaves = 6;

    [Header("🌑 陨石坑纹理")]
    [Tooltip("陨石坑漫反射（moon_macro_01_diff.png）")]
    [SerializeField] private Texture2D craterDiffuse;
    [Tooltip("陨石坑法线（moon_macro_01_nor_gl.png）")]
    [SerializeField] private Texture2D craterNormal;
    [Tooltip("陨石坑粗糙度（moon_macro_01_rough.png）")]
    [SerializeField] private Texture2D craterRoughness;
    [Tooltip("陨石坑位移（moon_macro_01_disp.png）")]
    [SerializeField] private Texture2D craterDisplacement;

    [Header("光照设置")]
    [SerializeField] private float sunIntensity = 1.5f;
    [SerializeField] private Vector3 sunRotation = new Vector3(50f, -30f, 0f);

    [Header("相机设置")]
    [SerializeField] private Vector3 cameraPosition = new Vector3(0f, 2f, -5f);

#if UNITY_EDITOR
    [ContextMenu("创建月球场景")]
    public void CreateMoonScene()
    {
        CreateGround();
        CreateSunlight();
        SetupAmbientLight();
        SetupCamera();
        Debug.Log("✅ 月球场景创建完成！");
    }

    [ContextMenu("🗑️ 清除场景")]
    public void ClearScene()
    {
        var ground = GameObject.Find("MoonSurface");
        var crater = GameObject.Find("MoonCraters");
        var light = GameObject.Find("SunLight");
        var probe = GameObject.Find("SceneReflectionProbe");

        if (ground != null) UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(ground);
        if (crater != null) UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(crater);
        if (light != null) UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(light);
        if (probe != null) UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(probe);

        Debug.Log("🗑️ 场景已清除");
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

        // ========== 1. 创建地面 ==========
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "MoonSurface";
        
        // 计算缩放：Unity Plane 默认 10m
        float scale = groundSize / 10f;

        Debug.Log($"📐 地面尺寸: {groundSize}m (Scale={scale:F2})");

        // ========== 2. 细分网格 + 地形起伏 ==========
        SubdivideMesh(ground, 64);  // 固定 64 细分，够密了
        ApplyTerrainNoise(ground, terrainHeight, terrainOctaves, scale);

        // ========== 3. 现在缩放对象 ==========
        ground.transform.localScale = new Vector3(scale, 1f, scale);

        // ========== 4. 创建材质 ==========
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = "MoonMaterial";

        // ========== 5. 应用纹理（只用于月壤！陨石坑用单独对象） ==========
        // 地面总是用月壤，陨石坑用单独对象
        Vector2 tiling = new Vector2(scale, scale);
        
        if (moon_02_diff != null)
        {
            Debug.Log($"   ├─ 纹理方案: 月壤 (moon_02)");
            Debug.Log($"   └─ 纹理 Tiling: ({tiling.x:F2}, {tiling.y:F2})");
            Debug.Log($"   💡 月壤方案：Tiling={tiling.x:F0} 是正确的（保持每米清晰度不变）");
            ApplyTextureSet(material, moon_02_diff, moon_02_nor, moon_02_rough, moon_02_disp, tiling, 2.0f, 0.005f);
        }
        else
        {
            Debug.LogWarning("⚠️  没有拖入月壤纹理（moon_02），使用纯色");
            material.color = new Color(0.45f, 0.43f, 0.40f);
        }

        // ========== 6. 应用材质 ==========
        material.SetFloat("_Metallic", 0.0f);
        material.SetFloat("_Smoothness", 0.1f);
        material.doubleSidedGI = true;

        var renderer = ground.GetComponent<Renderer>();
        renderer.material = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;

        Debug.Log($"✅ 月球表面创建完成！");
    }

    [ContextMenu("🌑 添加陨石坑")]
    public void AddCraters()
    {
        ClearCraters();
        
        var ground = GameObject.Find("MoonSurface");
        if (ground == null)
        {
            Debug.LogWarning("未找到 MoonSurface，请先创建月球场景");
            return;
        }

        if (craterDiffuse == null)
        {
            Debug.LogWarning("请先拖入陨石坑纹理（craterDiffuse）");
            return;
        }

        // 创建陨石坑对象
        var crater = GameObject.CreatePrimitive(PrimitiveType.Plane);
        crater.name = "MoonCraters";
        crater.transform.parent = ground.transform;
        
        // 设置陨石坑尺寸和位置（正好匹配地面，稍微向上避免 z-fighting）
        crater.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        crater.transform.localRotation = Quaternion.identity;
        crater.transform.localScale = Vector3.one;  // 和地面一样大

        // 创建陨石坑材质
        var craterMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        craterMaterial.name = "MoonMaterial_Craters";

        // 应用陨石坑纹理（Tiling=1，正好覆盖整个地面）
        ApplyTextureSet(craterMaterial, craterDiffuse, craterNormal, craterRoughness, craterDisplacement, Vector2.one, 3.0f, 0.02f);

        // 设置不透明材质（陨石坑纹理没有 Alpha，直接覆盖）
        craterMaterial.doubleSidedGI = true;

        var craterRenderer = crater.GetComponent<Renderer>();
        craterRenderer.material = craterMaterial;
        craterRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        craterRenderer.receiveShadows = false;

        Debug.Log($"🌑 陨石坑对象已创建！");
    }

    [ContextMenu("🗑️ 清除陨石坑")]
    public void ClearCraters()
    {
        var crater = GameObject.Find("MoonCraters");
        if (crater != null)
        {
            UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(crater);
            Debug.Log("🗑️ 陨石坑已清除");
        }
        else
        {
            Debug.Log("没有找到陨石坑对象");
        }
    }

    private void ApplyTextureSet(Material material, Texture2D diff, Texture2D nor, Texture2D rough, Texture2D disp, Vector2 tiling, float bumpScale, float parallax)
    {
        // 漫反射
        if (diff != null)
        {
            material.mainTexture = diff;
            material.mainTextureScale = tiling;
        }
        
        // 法线
        if (nor != null)
        {
            material.SetTexture("_BumpMap", nor);
            material.SetTextureScale("_BumpMap", tiling);
            material.EnableKeyword("_NORMALMAP");
            material.SetFloat("_BumpScale", bumpScale);
        }
        
        // 粗糙度
        if (rough != null)
        {
            material.SetTexture("_MetallicGlossMap", rough);
            material.SetTextureScale("_MetallicGlossMap", tiling);
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
        }
        
        // 位移
        if (disp != null)
        {
            material.SetTexture("_ParallaxMap", disp);
            material.SetTextureScale("_ParallaxMap", tiling);
            material.EnableKeyword("_PARALLAXMAP");
            material.SetFloat("_Parallax", parallax);
        }
    }

    private void SubdivideMesh(GameObject ground, int subdivisions)
    {
        var meshFilter = ground.GetComponent<MeshFilter>();
        var mesh = new Mesh();
        mesh.name = "MoonSurface_HighPoly";

        int resolution = subdivisions + 1;
        var vertices = new Vector3[resolution * resolution];
        var uvs = new Vector2[resolution * resolution];
        var triangles = new int[subdivisions * subdivisions * 6];

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int index = z * resolution + x;
                float px = (float)x / subdivisions * 10f - 5f;
                float pz = (float)z / subdivisions * 10f - 5f;

                vertices[index] = new Vector3(px, 0, pz);
                uvs[index] = new Vector2((float)x / subdivisions, (float)z / subdivisions);
            }
        }

        int triIndex = 0;
        for (int z = 0; z < subdivisions; z++)
        {
            for (int x = 0; x < subdivisions; x++)
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

    private void ApplyTerrainNoise(GameObject ground, float height, int octaves, float scale)
    {
        var meshFilter = ground.GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        var mesh = meshFilter.sharedMesh;
        var vertices = mesh.vertices;

        Debug.Log($"⛰️  应用地形起伏: 高度={height}m, 八度={octaves}, scale补偿={scale:F2}");

        // 添加随机偏移，避免 Perlin Noise 在 (0,0) 边界的问题
        float offsetX = 1000f;
        float offsetZ = 2000f;

        for (int i = 0; i < vertices.Length; i++)
        {
            float x = vertices[i].x;
            float z = vertices[i].z;

            float h = 0f;
            float amp = height;
            // 关键！调整频率：当地面放大 N 倍时，频率也要乘以 N，
            // 这样同样尺寸的噪声在更大的地面上保持视觉一致！
            float freq = (1f / 8f) * scale;

            for (int o = 0; o < octaves; o++)
            {
                // 添加偏移，避免边缘撕裂
                h += Mathf.PerlinNoise((x + offsetX) * freq, (z + offsetZ) * freq) * amp;
                amp *= 0.55f;
                freq *= 2.2f;
            }

            vertices[i].y += h;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        Debug.Log($"✅ 地形起伏应用完成！");
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

        Debug.Log("✅ 太阳光创建完成");
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

        Debug.Log("✅ 环境光配置完成");
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
            Debug.LogWarning("⚠️ 未找到主相机");
        }
    }
}
