Shader "Not3A/City Paving"
{
    Properties { _BaseMap ("Surface", 2D) = "white" {} _BaseColor ("Tint", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="AlphaTest" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            // This owned ground overlay follows the terrain but must not intersect
            // its coarser triangles. City sprites draw later in the transparent queue.
            Cull Off ZWrite Off ZTest Always Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/Stage3/DayNight/Shaders/DayNightColor.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST; half4 _BaseColor;
            CBUFFER_END
            struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half alpha:TEXCOORD1; };
            Varyings Vert(Attributes input)
            {
                Varyings o; o.positionCS=TransformObjectToHClip(input.positionOS);
                o.uv=TRANSFORM_TEX(input.uv,_BaseMap);o.alpha=input.color.a;return o;
            }
            half4 Frag(Varyings input):SV_Target
            {
                half4 ink=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv)*_BaseColor;
                ink.rgb=Not3ADayNight(ink.rgb,0);ink.a*=input.alpha;clip(ink.a-.005h);return ink;
            }
            ENDHLSL
        }
    }
}
