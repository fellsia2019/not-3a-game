Shader "Not3A/Flowing Soul Wave"
{
    Properties { _BaseColor ("Wave tint", Color) = (0.02,0.65,1,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+30" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
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
                half lengthFade=smoothstep(0,.35,input.uv.x)*(1-smoothstep(.92,1,input.uv.x));
                half centre=abs(input.uv.y-.5)*2;
                half softEdge=pow(saturate(1-centre),1.4);
                half flowing=.8+.2*sin(input.uv.x*15-_Time.y*2.1);
                half core=pow(saturate(1-centre),5);
                half head=exp(-pow((input.uv.x-.86)*9,2));
                half3 color=_BaseColor.rgb*(1.7+head)+core*head*half3(.6,1.2,1.4);
                half alpha=lengthFade*softEdge*flowing*_BaseColor.a;
                return half4(color*alpha,alpha);
            }
            ENDHLSL
        }
    }
}
