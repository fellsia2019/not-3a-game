Shader "Not3A/Landmark Sprite Flow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Original sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Kind ("Portal 0 / soul 1", Float) = 0
        _FlowTime ("Presentation clock", Float) = 0
        _FlowCenter ("Flow center", Vector) = (.51,.56,0,0)
        _FlowRadii ("Flow radii", Vector) = (.18,.22,0,0)
        _Speed ("Flow speed", Float) = .75
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _Kind, _FlowTime, _Speed;
                float4 _FlowCenter, _FlowRadii;
            CBUFFER_END
            struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                half4 art = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float2 p = (input.uv - _FlowCenter.xy) / _FlowRadii.xy;
                float radius = length(p);
                float flow = _FlowTime * _Speed;
                if (_Kind < .5)
                {
                    // Confine circulation to the interior. Masonry and the three inlays stay still.
                    float mask = (1-smoothstep(.60,.94,radius)) * saturate((art.r-max(art.g,art.b)) * 9);
                    float angle = -flow + .10*sin(radius*11+flow*2);
                    float c = cos(angle), s = sin(angle);
                    float2 q = float2(c*p.x-s*p.y,s*p.x+c*p.y);
                    half4 moving = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,_FlowCenter.xy+q*_FlowRadii.xy);
                    art.rgb = lerp(art.rgb,moving.rgb,mask);
                }
                else
                {
                    // Travelling light follows the baked horizontal ellipse, never tipping the image.
                    float cyan = saturate((min(art.g,art.b)-art.r)*5);
                    float outer = smoothstep(.94,1.12,radius)*cyan;
                    float angle = atan2(p.y,p.x);
                    float travelling = .72 + .28*sin(angle*3-flow*3);
                    art.rgb *= lerp(1,travelling,outer);
                    float flame = (1-smoothstep(.05,.14,length((input.uv-float2(.5,.53))*float2(1,1.2))))*cyan;
                    art.rgb *= 1 + flame*.13*sin(flow*7+input.uv.y*24);
                }
                art *= input.color;
                clip(art.a - .005h);
                return art;
            }
            ENDHLSL
        }
    }
}
