Shader "Not3A/District Geology"
{
    Properties
    {
        _StoneTex ("Painted project stone", 2D) = "white" {}
        _GrassTex ("Existing grass edge pigment",2D)="white"{}
        [HideInInspector] _ZWrite ("Depth write",Float)=1
        [HideInInspector] _SrcBlend ("Source",Float)=1
        [HideInInspector] _DstBlend ("Destination",Float)=0
        [HideInInspector] _RevealPass ("Local reveal pass",Float)=0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Off ZWrite [_ZWrite] ZTest LEqual Blend [_SrcBlend] [_DstBlend]
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Assets/Stage3/DayNight/Shaders/DayNightColor.hlsl"
            TEXTURE2D(_StoneTex); SAMPLER(sampler_StoneTex);
            TEXTURE2D(_GrassTex); SAMPLER(sampler_GrassTex);
            float4 _DistrictHeroWorld;
            float4 _DistrictRevealParameters;
            CBUFFER_START(UnityPerMaterial)
            float _ZWrite,_SrcBlend,_DstBlend,_RevealPass;
            float4 _GrassTex_ST;
            CBUFFER_END
            struct Attributes { float3 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 normalWS:TEXCOORD0; float2 uv:TEXCOORD1; half4 color:COLOR; float4 screenPos:TEXCOORD2; float4 hero:TEXCOORD3;float depth:TEXCOORD4;float2 ground:TEXCOORD5;float3 world:TEXCOORD6; };
            Varyings Vert(Attributes v)
            {
                Varyings o; o.positionCS=TransformObjectToHClip(v.positionOS);
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);o.uv=v.uv;o.color=v.color;
                o.screenPos=ComputeScreenPos(o.positionCS);
                float4 heroScreen=ComputeScreenPos(TransformWorldToHClip(_DistrictHeroWorld.xyz));
                o.hero=float4(heroScreen.xy/heroScreen.w,-TransformWorldToView(_DistrictHeroWorld.xyz).z,1);
                float3 world=TransformObjectToWorld(v.positionOS);
                o.world=world;
                o.depth=-TransformWorldToView(world).z;
                o.ground=((world.xy+.5)/4)*_GrassTex_ST.xy+_GrassTex_ST.zw;return o;
            }
            float2 Hash(float2 p) { return frac(sin(float2(dot(p,float2(127.1,311.7)),dot(p,float2(269.5,183.3))))*43758.5453); }
            half3 Pigment(float2 uv)
            {
                // This is a model atlas, not a tileable material. Stay inside one clean
                // face and mirror continuously; never repeat baked silhouette/shadow edges.
                float2 mirrored=1-abs(frac(uv*.17)*2-1);
                return SAMPLE_TEXTURE2D(_StoneTex,sampler_StoneTex,float2(.075,.14)+mirrored*.035).rgb;
            }
            float Crack(float2 uv)
            {
                float2 cell=floor(uv*.95);float2 f=frac(uv*.95);float a=10,b=10;
                for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++)
                {
                    float2 d=float2(x,y)+Hash(cell+float2(x,y))-f;float dist=dot(d,d);
                    if(dist<a){b=a;a=dist;}else b=min(b,dist);
                }
                return 1-smoothstep(.008,.032,sqrt(b)-sqrt(a));
            }
            half4 Frag(Varyings i):SV_Target
            {
                float radius=_DistrictRevealParameters.x*_ScreenParams.y/max(.01,unity_OrthoParams.y);
                float window=1-smoothstep(radius*(1-_DistrictRevealParameters.y),radius,
                    length((i.screenPos.xy/i.screenPos.w-i.hero.xy)*_ScreenParams.xy));
                float foreground=1-smoothstep(i.hero.z-.20,i.hero.z+.20,i.depth);
                float reveal=window*foreground*_DistrictHeroWorld.w;
                if(_RevealPass<.5)clip(.001-reveal);else clip(reveal-.001);
                float3 normal=normalize(i.normalWS);
                float3 weights=pow(abs(normal),4);weights/=max(.001,weights.x+weights.y+weights.z);
                // World-space triplanar pigment avoids per-quad UV switches at cliff lips.
                half3 paint=Pigment(i.world.yz)*weights.x+Pigment(i.world.xz)*weights.y+Pigment(i.world.xy)*weights.z;
                float seam=Crack(i.world.yz)*weights.x+Crack(i.world.xz)*weights.y+Crack(i.world.xy)*weights.z;
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));
                half light=.36h+.64h*saturate(dot(normal,sun.direction))*sun.shadowAttenuation;
                half3 color=paint*i.color.rgb*light*lerp(1,.79h,seam);
                half3 grass=SAMPLE_TEXTURE2D(_GrassTex,sampler_GrassTex,i.ground).rgb;
                color=lerp(grass,color,i.color.a);
                color=Not3ADayNight(color,0);
                return half4(color,_RevealPass<.5?1:lerp(1,_DistrictRevealParameters.w,reveal));
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull Back
            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _ZWrite,_SrcBlend,_DstBlend,_RevealPass;
            float4 _GrassTex_ST;
            CBUFFER_END
            float3 _LightDirection;
            float3 _LightPosition;
            struct Attributes { float3 positionOS:POSITION;float3 normalOS:NORMAL; };
            float4 ShadowVert(Attributes v):SV_POSITION
            {
                float3 world=TransformObjectToWorld(v.positionOS);
                float3 normal=TransformObjectToWorldNormal(v.normalOS);
                #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                float3 direction=normalize(_LightPosition-world);
                #else
                float3 direction=_LightDirection;
                #endif
                float4 position=TransformWorldToHClip(ApplyShadowBias(world,normal,direction));
                #if UNITY_REVERSED_Z
                position.z=min(position.z,UNITY_NEAR_CLIP_VALUE*position.w);
                #else
                position.z=max(position.z,UNITY_NEAR_CLIP_VALUE*position.w);
                #endif
                return position;
            }
            half4 ShadowFrag():SV_Target { return 0; }
            ENDHLSL
        }
    }
}
