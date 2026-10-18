Shader "Not3A/District Ground Blend"
{
    Properties { _GrassTex ("Existing grass pigment",2D)="white"{} }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-30" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Off ZWrite Off ZTest LEqual Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/Stage3/DayNight/Shaders/DayNightColor.hlsl"
            TEXTURE2D(_GrassTex);SAMPLER(sampler_GrassTex);
            CBUFFER_START(UnityPerMaterial)
            float4 _GrassTex_ST;
            CBUFFER_END
            struct Attributes { float3 positionOS:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR; };
            Varyings Vert(Attributes v){Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS);o.uv=v.uv*_GrassTex_ST.xy+_GrassTex_ST.zw;o.color=v.color;return o;}
            half4 Frag(Varyings i):SV_Target
            {
                half3 grass=SAMPLE_TEXTURE2D(_GrassTex,sampler_GrassTex,i.uv).rgb;
                return half4(Not3ADayNight(lerp(grass,grass*.45h+i.color.rgb*.55h,.65h),0),i.color.a);
            }
            ENDHLSL
        }
    }
}
