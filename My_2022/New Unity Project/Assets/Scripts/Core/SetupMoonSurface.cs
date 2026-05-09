using UnityEngine;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 自动创建月球表面场景（最佳方案：直接在地面上挖坑！）
/// 100% 完美：没有极点，没有 z-fighting，法线完美
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
    [Tooltip("地面网格细分（越多越平滑）")]
    [SerializeField] [Range(32, 128)] private int groundSubdivisions = 64;

    [Header("地形起伏（绝对能看到！）")]
    [Tooltip("起伏高度，越大越明显（推荐0.5-2）")]
    [SerializeField] private float terrainHeight = 1.0f;
    [Tooltip("起伏细密程度，越多越密（推荐2-6）")]
    [SerializeField] [Range(1, 8)] private int terrainOctaves = 4;
    [SerializeField] private float terrainScale = 20f;
    [SerializeField] private float terrainPersistence = 0.45f;
    [SerializeField] private float terrainLacunarity = 1.8f;

    [Header("🌑 陨石坑设置（直接刻在地面上！）")]
    [Tooltip("陨石坑数量")]
    [SerializeField] private int craterCount = 8;
    [Tooltip("陨石坑半径范围（米）")]
    [SerializeField] private Vector2 craterRadiusRange = new Vector2(1f, 8f);
    [Tooltip("陨石坑深度范围（米）")]
    [SerializeField] private Vector2 craterDepthRange = new Vector2(0.3f, 1.5f);
    [Tooltip("陨石坑分布半径（相对于地面中心）")]
    [SerializeField] private float craterScatterRadius = 40f;
    [Tooltip("随机种子（保证每次生成一样）")]
    [SerializeField] private int craterRandomSeed = 42;

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
        Debug.Log("✅ 月球场景创建完成！（陨石坑直接刻在地面上！）");
    }

    [ContextMenu("🗑️ 清除场景")]
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
        
        float scale = groundSize / 10f;
        ground.transform.localScale = new Vector3(scale, 1f, scale);

        // 细分网格 + 应用地形起伏 + 直接在上面挖陨石坑！
        var meshFilter = ground.GetComponent<MeshFilter>();
        var highPolyMesh = CreateHighPolyMesh(groundSize, scale);
        meshFilter.sharedMesh = highPolyMesh;

        // 更新碰撞器，强制 1:1 匹配视觉网格！
        var meshCollider = ground.GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            meshCollider = ground.AddComponent<MeshCollider>();
        }
        meshCollider.sharedMesh = highPolyMesh;
        meshCollider.convex = false;  // 对于大型地形，不使用凸包更准确
        meshCollider.cookingOptions = MeshColliderCookingOptions.None;  // 不简化

        // 创建材质
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = "MoonMaterial_Regolith";

        if (moon_02_diff != null)
        {
            ApplyTextureSet(material, moon_02_diff, moon_02_nor, moon_02_rough, moon_02_disp, 
                new Vector2(scale, scale), 2.0f, 0.005f);
            Debug.Log($"应用月壤纹理: {moon_02_diff.name}");
        }
        else
        {
            material.color = new Color(0.45f, 0.43f, 0.40f);
        }

        material.SetFloat("_Metallic", 0.0f);
        material.SetFloat("_Smoothness", 0.0f);
        material.doubleSidedGI = true;

        var renderer = ground.GetComponent<Renderer>();
        renderer.material = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;
    }

    private Mesh CreateHighPolyMesh(float groundSize, float scale)
    {
        var mesh = new Mesh();
        mesh.name = "MoonSurface_WithCraters";

        int resolution = groundSubdivisions + 1;
        var vertices = new Vector3[resolution * resolution];
        var uvs = new Vector2[resolution * resolution];
        var triangles = new int[groundSubdivisions * groundSubdivisions * 6];

        // 步骤 1：生成陨石坑位置（先算好，后面挖坑用）
        var craters = GenerateCraterPositions();
        Debug.Log($"🗺️  预计算 {craters.Length} 个陨石坑位置...");

        // 步骤 2：生成顶点
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int index = z * resolution + x;
                
                // 原始平面坐标（-5 到 5，因为 Unity Plane 默认 10m）
                float px = (float)x / groundSubdivisions * 10f - 5f;
                float pz = (float)z / groundSubdivisions * 10f - 5f;
                
                // 世界坐标（考虑 scale）
                float worldX = px * scale;
                float worldZ = pz * scale;
                
                // 基础 y = 0
                float y = 0f;
                
                // 步骤 3：应用地形起伏
                y += CalculateTerrainHeight(worldX, worldZ, scale);
                
                // 步骤 4：直接在上面挖陨石坑！
                y += CalculateCratersDepth(worldX, worldZ, craters);
                
                vertices[index] = new Vector3(px, y, pz);
                uvs[index] = new Vector2((float)x / groundSubdivisions, (float)z / groundSubdivisions);
            }
        }

        // 生成三角形
        int triIndex = 0;
        for (int z = 0; z < groundSubdivisions; z++)
        {
            for (int x = 0; x < groundSubdivisions; x++)
            {
                int a = z * resolution + x;
                int b = a + 1;
                int c = (z + 1) * resolution + x;
                int d = c + 1;

                triangles[triIndex++] = a;
                triangles[triIndex++] = c;
                triangles[triIndex++] = b;

                triangles[triIndex++] = b;
                triangles[triIndex++] = c;
                triangles[triIndex++] = d;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        Debug.Log($"✅ 地面网格创建完成！顶点数: {vertices.Length}，已包含 {craters.Length} 个陨石坑！");
        return mesh;
    }

    private (Vector2 center, float radius, float depth)[] GenerateCraterPositions()
    {
        var rng = new System.Random(craterRandomSeed);
        var craters = new (Vector2, float, float)[craterCount];

        for (int i = 0; i < craterCount; i++)
        {
            // 圆形分布
            float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
            float dist = Mathf.Sqrt((float)rng.NextDouble()) * craterScatterRadius;
            
            float x = Mathf.Cos(angle) * dist;
            float z = Mathf.Sin(angle) * dist;
            
            float radius = Mathf.Lerp(craterRadiusRange.x, craterRadiusRange.y, (float)rng.NextDouble());
            float depth = Mathf.Lerp(craterDepthRange.x, craterDepthRange.y, (float)rng.NextDouble());
            
            craters[i] = (new Vector2(x, z), radius, depth);
        }

        return craters;
    }

    private float CalculateTerrainHeight(float worldX, float worldZ, float scale)
    {
        float height = 0f;
        float amplitude = terrainHeight;
        float frequency = (1f / terrainScale) * scale;

        // 避免在 (0,0) 边界的问题
        float offsetX = 1000f;
        float offsetZ = 2000f;

        for (int octave = 0; octave < terrainOctaves; octave++)
        {
            height += Mathf.PerlinNoise((worldX + offsetX) * frequency, (worldZ + offsetZ) * frequency) * amplitude;
            amplitude *= terrainPersistence;
            frequency *= terrainLacunarity;
        }

        return height;
    }

    private float CalculateCratersDepth(float worldX, float worldZ, (Vector2 center, float radius, float depth)[] craters)
    {
        float totalDepth = 0f;
        Vector2 pos = new Vector2(worldX, worldZ);

        foreach (var crater in craters)
        {
            float dist = Vector2.Distance(pos, crater.center);
            
            if (dist < crater.radius)
            {
                float t = dist / crater.radius;
                // 平滑的下凹函数（余弦函数，边缘平滑过渡）
                float depthFactor = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
                totalDepth -= crater.depth * depthFactor;
            }
        }

        return totalDepth;
    }

    private void ApplyTextureSet(Material material, Texture2D diff, Texture2D nor, Texture2D rough, Texture2D disp, Vector2 tiling, float bumpScale, float parallax)
    {
        material.mainTexture = diff;
        material.mainTextureScale = tiling;
        material.color = new Color(0.75f, 0.73f, 0.70f);

        if (nor != null)
        {
            material.SetTexture("_BumpMap", nor);
            material.SetTextureScale("_BumpMap", tiling);
            material.EnableKeyword("_NORMALMAP");
            material.SetFloat("_BumpScale", bumpScale);
        }

        if (rough != null)
        {
            material.SetTexture("_MetallicGlossMap", rough);
            material.SetTextureScale("_MetallicGlossMap", tiling);
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.SetFloat("_GlossMapScale", 0.0f);
            material.SetFloat("_Smoothness", 0.0f);
        }

        if (disp != null)
        {
            material.SetTexture("_ParallaxMap", disp);
            material.SetTextureScale("_ParallaxMap", tiling);
            material.EnableKeyword("_PARALLAXMAP");
            material.SetFloat("_Parallax", parallax);
        }
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
            Debug.LogWarning("⚠️ 未找到主相机，请手动调整");
        }
    }
#endif
}
