Shader "Not3A/Resource District Sprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
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
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _DistrictHeroWorld;
            float4 _DistrictRevealParameters;
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            CBUFFER_END
            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                float4 screenPos : TEXCOORD1;
                float3 heroView : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            Varyings Vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                Varyings output;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.screenPos = ComputeScreenPos(output.positionCS);
                float4 heroClip = TransformWorldToHClip(_DistrictHeroWorld.xyz);
                float4 heroScreen = ComputeScreenPos(heroClip);
                output.heroView = float3(heroScreen.xy / heroScreen.w,
                    -TransformWorldToView(_DistrictHeroWorld.xyz).z);
                output.uv = input.uv;
                output.color = input.color * _Color * unity_SpriteColor;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float2 delta=(input.screenPos.xy/input.screenPos.w-input.heroView.xy)*_ScreenParams.xy;
                float radius=_DistrictRevealParameters.x*_ScreenParams.y/max(.01,unity_OrthoParams.y);
                float window=1-smoothstep(radius*(1-_DistrictRevealParameters.y),radius,length(delta));
                // Vertex/per-renderer alpha is the local opacity target, not a whole-object erase.
                color.rgb*=input.color.rgb;
                color.a*=lerp(1,max(_DistrictRevealParameters.w,input.color.a),window*_DistrictHeroWorld.w);
                clip(color.a - .005h);
                return color;
            }
            ENDHLSL
        }
    }
}
