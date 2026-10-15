Shader "Not3A/Cartoon Grass Verge"
{
    Properties
    {
        _BaseMap ("Grass", 2D) = "white" {}
        _BaseColor ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Geometry+10" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            CBUFFER_END
            struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half alpha:TEXCOORD1; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS=TransformObjectToHClip(input.positionOS);
                output.uv=TRANSFORM_TEX(input.uv,_BaseMap);
                output.alpha=input.color.a;
                return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                half4 color=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv)*_BaseColor;
                // Broken soil/turf edge, not a translucent blurred green ribbon.
                float2 p=input.uv*140;
                float noise=.5+.20*sin(p.x*1.31+p.y*1.87)+.15*sin(p.x*3.71-p.y*2.11);
                float threshold=.18+noise*.55;
                color.a*=smoothstep(threshold-.045,threshold+.045,input.alpha);
                return color;
            }
            ENDHLSL
        }
    }
}
