Shader "Not3A/Hero Blink Marks"
{
    SubShader
    {
        // Target guidance must remain readable over raised grass and the rejecting obstacle.
        // It is still a world-camera pass, below the opaque HUD and fullscreen pages.
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+50" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Off ZWrite Off ZTest Always Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float3 positionOS:POSITION; half4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; half4 color:COLOR; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS=TransformObjectToHClip(input.positionOS);
                output.color=input.color;
                return output;
            }
            half4 Frag(Varyings input):SV_Target { return input.color; }
            ENDHLSL
        }
    }
}
