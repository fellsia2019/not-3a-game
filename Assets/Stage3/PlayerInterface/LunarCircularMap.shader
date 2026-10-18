Shader "Not3A/PlayerInterface/CircularMap"
{
    Properties { _MainTex("Map",2D)="white" {} _SourceRect("Source",Vector)=(0,0,1,1) _CircleMask("Circle",Float)=1 _RemoveDark("Source dark backdrop",Float)=0 _Tint("Tint",Vector)=(1,1,1,1) }
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
            sampler2D _MainTex; float4 _SourceRect, _Tint; float _CircleMask, _RemoveDark;
            fixed4 frag(v2f_img i):SV_Target
            {
                float2 local=(i.uv-_SourceRect.xy)/_SourceRect.zw;
                fixed4 c=tex2D(_MainTex,i.uv);
                float inside=step(0,i.uv.x)*step(i.uv.x,1)*step(0,i.uv.y)*step(i.uv.y,1);
                c=lerp(fixed4(.13,.12,.18,1),c,inside);
                c.a*=lerp(1,1-smoothstep(.485,.5,length(local-.5)),_CircleMask);
                c.a*=lerp(1,smoothstep(.10,.27,max(c.r,max(c.g,c.b))),_RemoveDark);
                c*=_Tint;
                // IMGUI draws into its display-space target; match GUI.DrawTexture in linear projects.
                #ifndef UNITY_COLORSPACE_GAMMA
                c.rgb=LinearToGammaSpace(c.rgb);
                #endif
                return c;
            }
            ENDCG
        }
    }
}
