#ifndef NOT3A_DAY_NIGHT_COLOR_INCLUDED
#define NOT3A_DAY_NIGHT_COLOR_INCLUDED
float _Not3ANightAmount;
half3 Not3ADayNight(half3 color, half lightMask)
{
    half luminance = dot(color, half3(.2126h,.7152h,.0722h));
    half3 moonlit = lerp(color*.42h, luminance*half3(.17h,.30h,.58h), .70h);
    moonlit *= half3(.42h,.66h,1.08h);
    return lerp(color, moonlit, saturate(_Not3ANightAmount)*(1-saturate(lightMask)));
}
#endif
