Shader "Not3A/City Sprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Service contour", Color) = (.48,.90,.87,.95)
        _OutlinePixels ("Contour width", Float) = 2.5
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
            #include "Assets/Stage3/DayNight/Shaders/DayNightColor.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            CBUFFER_START(UnityPerMaterial)
            half4 _Color, _OutlineColor;
            float _OutlinePixels;
            CBUFFER_END
            struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            Varyings Vert(Attributes input)
            {
                Varyings o; o.positionCS=TransformObjectToHClip(input.positionOS);
                o.uv=input.uv; o.color=input.color*_Color; return o;
            }
            half4 Frag(Varyings input):SV_Target
            {
                half4 ink=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv)*input.color;
                float2 d=_MainTex_TexelSize.xy*_OutlinePixels;
                half edge=0;
                edge=max(edge,SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv+float2(d.x,0)).a);
                edge=max(edge,SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv-float2(d.x,0)).a);
                edge=max(edge,SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv+float2(0,d.y)).a);
                edge=max(edge,SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv-float2(0,d.y)).a);
                edge=max(edge,SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv+d*.7071).a);
                edge=max(edge,SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv-d*.7071).a);
                edge=max(edge,SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv+float2(d.x,-d.y)*.7071).a);
                edge=max(edge,SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv+float2(-d.x,d.y)*.7071).a);
                half alpha=max(ink.a,edge*_OutlineColor.a);
                half3 color=lerp(_OutlineColor.rgb,Not3ADayNight(ink.rgb,0),saturate(ink.a));
                clip(alpha-.005h); return half4(color,alpha);
            }
            ENDHLSL
        }
    }
}
