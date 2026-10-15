Shader "Not3A/Red Entrance Vortex"
{
    Properties
    {
        _BaseMap ("Original vortex texture", 2D) = "black" {}
        _FlowTime ("Presentation clock", Float) = 0
        _CirculationSpeed ("Circulation speed (radians/sec)", Range(0,2)) = .9
        _Intensity ("Red light", Range(0,4)) = 1.8
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry+1" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float _FlowTime;
                float _CirculationSpeed;
                half _Intensity;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS=TransformObjectToHClip(input.positionOS.xyz);
                output.uv=input.uv;
                return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                float2 p=input.uv-.5;
                float radius=length(p)*2;
                // A bounded inward-moving distortion bends the circulating strands
                // without accumulating shear during long sessions.
                float flow=_FlowTime*_CirculationSpeed;
                float angle=-flow + .14*(1-saturate(radius))*sin(radius*13+flow*2);
                float c=cos(angle), s=sin(angle);
                float2 uv=float2(c*p.x-s*p.y,s*p.x+c*p.y)+.5;
                half3 smoke=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv).rgb;
                half rim=pow(saturate((radius-.91)/.09),2);
                return half4(smoke*_Intensity + half3(.9,.009,.003)*rim,1);
            }
            ENDHLSL
        }
    }
}
