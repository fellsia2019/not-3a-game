Shader "Not3A/Ground Teleport Seal"
{
    Properties
    {
        _MainTex ("Approved seal source", 2D) = "black" {}
        _MaskLow ("Background cutoff (linear)", Range(0,1)) = .12
        _MaskHigh ("Full line opacity (linear)", Range(0,1)) = .5
        _OutlinePixels ("Contact outline pixels", Range(0,4)) = 1.5
        _Opacity ("Opacity", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-20" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_TexelSize;
                half _MaskLow, _MaskHigh, _OutlinePixels, _Opacity;
            CBUFFER_END
            struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output; output.positionCS=TransformObjectToHClip(input.positionOS); output.uv=input.uv; return output;
            }
            half Coverage(half3 rgb) { return smoothstep(_MaskLow,_MaskHigh,max(rgb.r,max(rgb.g,rgb.b))); }
            half4 Frag(Varyings input):SV_Target
            {
                half3 art=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv).rgb;
                half stroke=Coverage(art);
                float2 step=_MainTex_TexelSize.xy*_OutlinePixels;
                half edge=max(max(Coverage(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv+float2(step.x,0)).rgb),
                    Coverage(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv-float2(step.x,0)).rgb)),
                    max(Coverage(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv+float2(0,step.y)).rgb),
                    Coverage(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv-float2(0,step.y)).rgb)));
                half alpha=max(stroke,edge*.65h)*_Opacity;
                clip(alpha-.005h);
                half3 color=lerp(half3(.015h,.025h,.025h),art,saturate(stroke/max(.001h,alpha)));
                return half4(color,alpha);
            }
            ENDHLSL
        }
    }
}
