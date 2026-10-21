Shader "Not3A/PlayerInterface/CycleHand"
{
    Properties { _MainTex("Hand",2D)="white"{} _SourceRect("Source",Vector)=(0,0,1,1) _Pivot("Pivot",Vector)=(.5,.1,0,0) _HandSize("Size",Vector)=(.1,.46,0,0) _Angle("Clockwise radians from up",Float)=0 _EncodeToDisplay("Encode display output",Float)=1 _Brightness("Hand brightness",Float)=1 }
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
            sampler2D _MainTex;float4 _SourceRect,_Pivot,_HandSize;float _Angle,_EncodeToDisplay,_Brightness;
            fixed4 frag(v2f_img i):SV_Target
            {
                float2 p=i.uv-.5;float s=sin(_Angle),c=cos(_Angle);
                float2 hand=float2(c*p.x-s*p.y,s*p.x+c*p.y)/_HandSize.xy+_Pivot.xy;
                clip(min(min(hand.x,hand.y),min(1-hand.x,1-hand.y)));
                fixed4 color=tex2D(_MainTex,_SourceRect.xy+hand*_SourceRect.zw);
                color.rgb=saturate(color.rgb*_Brightness);
                #ifndef UNITY_COLORSPACE_GAMMA
                color.rgb=lerp(color.rgb,LinearToGammaSpace(color.rgb),_EncodeToDisplay);
                #endif
                return color;
            }
            ENDCG
        }
    }
}
