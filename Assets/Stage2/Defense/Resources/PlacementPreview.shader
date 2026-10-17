Shader "Not3A/Placement Preview"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Sprite ("Sprite renderer color", Float) = 1
        _PreserveRGB ("Keep source pigment", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+20" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Off ZWrite Off ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half _Sprite, _PreserveRGB;
            CBUFFER_END
            struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; UNITY_VERTEX_OUTPUT_STEREO };
            Varyings Vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                SetUpSpriteInstanceProperties();
                Varyings output;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS=TransformObjectToHClip(input.positionOS);
                output.uv=input.uv;
                output.color=input.color*_Color*lerp(half4(1,1,1,1),unity_SpriteColor,_Sprite);
                return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                half4 source=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv);
                half pigment=.45h+.55h*dot(source.rgb,half3(.2126,.7152,.0722));
                return half4(lerp(pigment.xxx,source.rgb,_PreserveRGB)*input.color.rgb,source.a*input.color.a);
            }
            ENDHLSL
        }
    }
}
