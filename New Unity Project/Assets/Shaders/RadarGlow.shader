Shader "Demo/RadarGlow"
{
 Properties { _Color("Glow",Color)=(0.1,2.5,3,0.8) _Width("World width",Float)=0.04 }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
  Pass
  {
   Cull Front ZWrite Off ZTest LEqual Blend SrcAlpha One
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   float4 _Color; float _Width;
   CBUFFER_END
   struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
   struct Varyings { float4 positionCS:SV_POSITION; };
   Varyings vert(Attributes a) { Varyings o; float3 p=TransformObjectToWorld(a.positionOS.xyz); p+=TransformObjectToWorldNormal(a.normalOS)*_Width; o.positionCS=TransformWorldToHClip(p); return o; }
   half4 frag(Varyings i):SV_Target { return _Color; }
   ENDHLSL
  }
 }
}
