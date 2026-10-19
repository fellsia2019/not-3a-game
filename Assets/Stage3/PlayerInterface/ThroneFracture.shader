Shader "Not3A/PlayerInterface/ThroneFracture"
{
    Properties { _MainTex("White",2D)="white" {} }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Cull Off ZWrite Off ZTest Always Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            float4 _Hits[8]; float _Seeds[8];
            float2 _Origin; float _Fatal, _Whiteout, _Aspect, _Pixel;
            float hash(float n) { return frac(sin(n * 127.1 + 311.7) * 43758.5453); }
            float fracture(float2 uv, float2 origin, float strength, float reveal, float seed)
            {
                float2 p = (uv-origin)*float2(_Aspect,1);
                float radius = length(p);
                float angle = atan2(p.y,p.x) + 3.14159265;
                float sectorCount = 16;
                float sector = floor(angle / 6.2831853 * sectorCount);
                float local = frac(angle / 6.2831853 * sectorCount);
                float rnd = hash(sector + seed*19);
                // Unequal long radial fissures with angular branches, rather than a regular star.
                float bend = sin(radius*10 + rnd*12) * .06 + abs(frac(radius*3+rnd)-.5)*.12;
                float d = abs(local - (.28 + rnd*.4) - bend) * radius * 6.2831853 / sectorCount;
                float width = _Pixel * lerp(.8,2.3,strength);
                float ray = 1-smoothstep(width, width+_Pixel*1.2,d);
                float glow = exp(-d/max(_Pixel*3,width*4))*.22;
                float selected = step(rnd, .22+strength*.7);
                float branchPhase = frac(radius*2.6 + rnd);
                float branch = abs(local - (.28+rnd*.4) - (branchPhase-.5)*.65);
                float hairline = (1-smoothstep(_Pixel*.7,_Pixel*1.5,branch*max(radius,.1)*.39)) *
                    step(.55,strength) * step(.12,branchPhase) * step(branchPhase,.74);
                float revealMask = 1-smoothstep(reveal*2.5,reveal*2.5+.12,radius);
                float facets = step(.83,hash(sector+floor(radius*3)*37+seed)) *
                    (1-smoothstep(.05,.2,d)) * strength*strength*.25;
                return saturate((ray+glow+hairline*.65+facets)*selected*revealMask);
            }
            fixed4 frag(v2f_img i):SV_Target
            {
                float alpha = 0;
                for (int h=0;h<8;h++)
                {
                    float4 hit=_Hits[h];
                    if(hit.w<=0) continue;
                    // The first actual frame must contain light, including after a CPU hitch.
                    // This only advances the initial reveal; CPU lifetime still expires at .65s.
                    float sampleAge=max(.08,hit.z);
                    float envelope=smoothstep(0,.07,sampleAge)*(1-smoothstep(.16,1,sampleAge));
                    alpha=max(alpha,fracture(i.uv,hit.xy,hit.w,saturate(sampleAge*6),_Seeds[h])*envelope*lerp(.5,.9,hit.w));
                }
                if(_Fatal>0)
                {
                    float shattered=fracture(i.uv,_Origin,1,saturate(_Fatal*4),53);
                    float2 p=(i.uv-_Origin)*float2(_Aspect,1);
                    float wedge=floor((atan2(p.y,p.x)+3.14159)*7.6394);
                    float band=floor(length(p)*5);
                    float facet=smoothstep(hash(wedge+band*39)*.75+.08,hash(wedge+band*39)*.75+.22,_Fatal);
                    alpha=max(alpha,saturate(shattered*saturate(_Fatal*4)+facet*_Fatal*.85));
                }
                alpha=lerp(alpha,1,_Whiteout);
                return fixed4(1,1,1,alpha);
            }
            ENDCG
        }
    }
}
