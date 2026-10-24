Shader "Not3A/PlayerInterface/CircularMap"
{
    Properties { _MainTex("Map",2D)="white" {} _SourceRect("Source",Vector)=(0,0,1,1) _CircleMask("Circle",Float)=1 _RemoveDark("Source dark backdrop",Float)=0 _Tint("Tint",Vector)=(1,1,1,1) _EncodeToDisplay("Encode display output",Float)=1 _Desaturate("Desaturate",Float)=0 _Softness("Softness",Float)=0 }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Cull Off ZWrite Off ZTest Always Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float4 _SourceRect, _Tint; float _CircleMask, _RemoveDark, _EncodeToDisplay, _Desaturate, _Softness; float4 _MainTex_TexelSize;
            fixed4 frag(v2f_img i):SV_Target
            {
                float2 local=(i.uv-_SourceRect.xy)/_SourceRect.zw;
                fixed4 c=tex2D(_MainTex,i.uv);
                if(_Softness>0)
                {
                    float2 d=_MainTex_TexelSize.xy*_Softness;
                    c=(c*2+tex2D(_MainTex,i.uv+d)+tex2D(_MainTex,i.uv-d)+tex2D(_MainTex,i.uv+float2(d.x,-d.y))+tex2D(_MainTex,i.uv+float2(-d.x,d.y)))/6;
                }
                c.rgb=lerp(c.rgb,dot(c.rgb,float3(.299,.587,.114)).xxx,_Desaturate);
                float inside=step(0,i.uv.x)*step(i.uv.x,1)*step(0,i.uv.y)*step(i.uv.y,1);
                c=lerp(fixed4(.13,.12,.18,1),c,inside);
                c.a*=lerp(1,1-smoothstep(.485,.5,length(local-.5)),_CircleMask);
                c.a*=lerp(1,smoothstep(.10,.27,max(c.r,max(c.g,c.b))),_RemoveDark);
                c*=_Tint;
                // Windows IMGUI uses display-space output; Metal linear targets encode on write.
                #ifndef UNITY_COLORSPACE_GAMMA
                c.rgb=lerp(c.rgb,LinearToGammaSpace(c.rgb),_EncodeToDisplay);
                #endif
                return c;
            }
            ENDCG
        }
    }
}
