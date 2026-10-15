Shader "Not3A/Soul Flame"
{
    Properties { _BaseColor ("Flame color", Color) = (0.04,0.8,2.0,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-20" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha One
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS=TransformObjectToWorld(input.positionOS.xyz);
                // Tilt the volumetric flame toward the fixed tactical viewing plane.
                // The surrounding sphere and all solid masonry stay upright.
                float3 origin=TransformObjectToWorld(float3(0,0,0))-float3(0,0,.83);
                float3 delta=output.positionWS-origin;
                output.positionWS.y=origin.y+delta.y*.81915-delta.z*.57358;
                output.positionWS.z=origin.z+delta.y*.57358+delta.z*.81915;
                float h=saturate(-(output.positionWS.z-TransformObjectToWorld(float3(0,0,0)).z)-.6);
                output.positionWS.x+=sin(_Time.y*2.8+h*13)*h*.03;
                output.positionCS=TransformWorldToHClip(output.positionWS);
                half3 n=TransformObjectToWorldNormal(input.normalOS);
                output.normalWS=half3(n.x,n.y*.81915-n.z*.57358,n.y*.57358+n.z*.81915);
                return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                half facing=saturate(dot(normalize(input.normalWS),GetWorldSpaceNormalizeViewDir(input.positionWS)));
                half core=pow(facing,5);
                half pulse=.95+.05*sin(_Time.y*3.2);
                return half4((_BaseColor.rgb+core*half3(1.1,1.3,1.3))*pulse,
                    pow(facing,.55)*_BaseColor.a);
            }
            ENDHLSL
        }
    }
}
