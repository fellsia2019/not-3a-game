Shader "Not3A/Smooth Soul Ice"
{
    Properties
    {
        _BaseColor ("Ice tint", Color) = (0.08,0.52,0.95,1)
        _RimColor ("Luminous edge", Color) = (0.30,0.90,1.3,1)
        _CenterOpacity ("Clear centre", Range(0,1)) = 0.07
        _EdgeOpacity ("Ice edge", Range(0,1)) = 0.7
        _RimPower ("Edge width", Float) = 4
        _Sheen ("Smooth surface reflection", Range(0,1)) = 0.15
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _RimColor;
                half _CenterOpacity;
                half _EdgeOpacity;
                half _RimPower;
                half _Sheen;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; half3 normalWS : TEXCOORD1; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half3 n = normalize(input.normalWS);
                half3 v = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half facing = saturate(dot(n,v));
                half fresnel = pow(1-facing,_RimPower);
                half highlight = pow(saturate(dot(n,normalize(half3(-.4,.55,-.75)+v))),180);
                half secondHighlight = pow(saturate(dot(n,normalize(half3(.7,.25,-.8)+v))),260)*.45;
                // Broad, faint flowing reflections. No opaque fill, faceting or crack lines.
                half cloud=sin(n.x*23+sin(n.y*17)*1.8+n.z*11)
                    * sin(n.y*21+sin(n.z*19)*1.3-n.x*8)*.5+.5;
                half sheen = smoothstep(.55,.86,cloud)*pow(1-facing,.7)*_Sheen;
                half3 tint = lerp(_BaseColor.rgb, _RimColor.rgb, fresnel);
                tint += (highlight+secondHighlight) * half3(2,2.4,2.6) + sheen*half3(.4,.8,1.1);
                half opacity = saturate(lerp(_CenterOpacity,_EdgeOpacity,fresnel)
                    + (highlight+secondHighlight)*.72 + sheen*.35);
                return half4(tint,opacity);
            }
            ENDHLSL
        }
    }
}
