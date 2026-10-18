Shader "Not3A/Dwelling Sprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _VisibleRect ("Visible source UV bounds", Vector) = (0,0,1,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Off ZWrite Off ZTest LEqual Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "DayNightColor.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            float4 _VisibleRect;
            CBUFFER_END
            struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            Varyings Vert(Attributes input)
            {
                Varyings output; output.positionCS=TransformObjectToHClip(input.positionOS);
                output.uv=input.uv; output.color=input.color*_Color; return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                half4 color=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv)*input.color;
                float2 p=(input.uv-_VisibleRect.xy)/_VisibleRect.zw;
                float window=step(.245,p.x)*step(p.x,.34)*step(.245,p.y)*step(p.y,.335);
                float lantern=step(.745,p.x)*step(p.x,.792)*step(.24,p.y)*step(p.y,.30);
                half warm=saturate((color.r-max(color.g,color.b)) * 4)*saturate((color.g-color.b)*4);
                color.rgb=Not3ADayNight(color.rgb,saturate(window+lantern)*warm);
                clip(color.a-.005h); return color;
            }
            ENDHLSL
        }
    }
}
