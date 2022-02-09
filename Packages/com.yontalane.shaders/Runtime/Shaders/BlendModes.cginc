#include "UnityCG.cginc"

// Conversions

// Conversions from https://chilliant.com/ by Ian Taylor.
// Based on work by Sam Hocevar and Emil Persson.

float Epsilon = 1e-10;

half3 RGBtoHCV(half3 RGB)
{
    half4 P = (RGB.g < RGB.b) ? half4(RGB.bg, -1.0, 2.0/3.0) : half4(RGB.gb, 0.0, -1.0/3.0);
    half4 Q = (RGB.r < P.x) ? half4(P.xyw, RGB.r) : half4(RGB.r, P.yzx);
    half C = Q.x - min(Q.w, Q.y);
    half H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
    return half3(H, C, Q.x);
}

half3 HUEtoRGB(in half H)
{
    half R = abs(H * 6 - 3) - 1;
    half G = 2 - abs(H * 6 - 2);
    half B = 2 - abs(H * 6 - 4);
    return saturate(half3(R,G,B));
}

half3 RGBtoHSV(half3 RGB)
{
    half3 HCV = RGBtoHCV(RGB);
    half S = HCV.y / (HCV.z + Epsilon);
    return half3(HCV.x, S, HCV.z);
}

half3 RGBtoHSL(half3 RGB)
{
    if (RGB.r == 1.0 && RGB.g == 1.0 && RGB.b == 1.0)
    {
        return half3(0.0, 0.0, 1.0);
    }
    else if (RGB.r == 0.0 && RGB.g == 0.0 && RGB.b == 0.0)
    {
        return half3(0.0, 0.0, 0.0);
    }

    half3 HCV = RGBtoHCV(RGB);
    half L = HCV.z - HCV.y * 0.5;
    half S = HCV.y / (1 - abs(L * 2 - 1) + Epsilon);
    return half3(HCV.x, S, L);
}

half3 HSVtoRGB(half3 HSV)
{
    half3 RGB = HUEtoRGB(HSV.x);
    return ((RGB - 1) * HSV.y + 1) * HSV.z;
}

half3 HSLtoRGB(half3 HSL)
{
    if (HSL.z == 1.0)
    {
        return half3(1.0, 1.0, 1.0);
    }
    else if (HSL.z == 0.0)
    {
        return half3(0.0, 0.0, 0.0);
    }

    half3 RGB = HUEtoRGB(HSL.x);
    half C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
    return (RGB - 0.5) * C + HSL.z;
}

// a is the active layer. In Photoshop, this would be the top layer or the brush.
// b is the base layer. In Photoshop, this would be the bottom layer.

// Normal

half3 BlendNormal(half3 a, half3 b, half amount)
{
    return lerp(b, a, amount);
}

// Darken

half Darken(half a, half b)
{
    return min(a, b);
}
half3 BlendDarken(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    c.r = Darken(a.r, b.r);
    c.g = Darken(a.g, b.g);
    c.b = Darken(a.b, b.b);
    return lerp(b, c, amount);
}

half3 BlendMultiply(half3 a, half3 b, half amount)
{
    half3 c = a * b;
    return lerp(b, c, amount);
}

half ColorBurn(half a, half b)
{
    return 1.0 - (1.0 - b) / a;
}
half3 BlendColorBurn(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    c.r = ColorBurn(a.r, b.r);
    c.g = ColorBurn(a.g, b.g);
    c.b = ColorBurn(a.b, b.b);
    return lerp(b, c, amount);
}

half LinearBurn(half a, half b)
{
    return clamp(a + b - 1.0, 0.0, 1.0);
}
half3 BlendLinearBurn(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    c.r = LinearBurn(a.r, b.r);
    c.g = LinearBurn(a.g, b.g);
    c.b = LinearBurn(a.b, b.b);
    return lerp(b, c, amount);
}

half3 BlendDarkerColor(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    if (a.r + a.g + a.b < b.r + b.g + b.b)
    {
        c = a;
    }
    else
    {
        c = b;
    }
    return lerp(b, c, amount);
}

// Lighten

half Lighten(half a, half b)
{
    return max(a, b);
}
half3 BlendLighten(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    c.r = Lighten(a.r, b.r);
    c.g = Lighten(a.g, b.g);
    c.b = Lighten(a.b, b.b);
    return lerp(b, c, amount);
}

half Screen(half a, half b)
{
    return 1.0 - (1.0 - a) * (1.0 - b);
}
half3 BlendScreen(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    c.r = Screen(a.r, b.r);
    c.g = Screen(a.g, b.g);
    c.b = Screen(a.b, b.b);
    return lerp(b, c, amount);
}

half ColorDodge(half a, half b)
{
    return b / (1.0 - a);
}
half3 BlendColorDodge(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    c.r = ColorDodge(a.r, b.r);
    c.g = ColorDodge(a.g, b.g);
    c.b = ColorDodge(a.b, b.b);
    return lerp(b, c, amount);
}

half LinearDodge(half a, half b)
{
    return clamp(a + b, 0.0, 1.0);
}
half3 BlendLinearDodge(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    c.r = LinearDodge(a.r, b.r);
    c.g = LinearDodge(a.g, b.g);
    c.b = LinearDodge(a.b, b.b);
    return lerp(b, c, amount);
}

half3 BlendLighterColor(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    if (a.r + a.g + a.b > b.r + b.g + b.b)
    {
        c = a;
    }
    else
    {
        c = b;
    }
    return lerp(b, c, amount);
}

// Contrast

half Overlay(half a, half b)
{
    if (b < 0.5)
        return 2.0 * a * b;
    else
        return 1.0 - 2.0 * (1.0 - a) * (1.0 - b);
}
half3 BlendOverlay(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    c.r = Overlay(a.r, b.r);
    c.g = Overlay(a.g, b.g);
    c.b = Overlay(a.b, b.b);
    return lerp(b, c, amount);
}

half SoftLight(half a, half b)
{
    if (a < 0.5)
        return (2.0 * a * b) + ((b * b) * (1.0 - (2.0 * a)));
    else
        return ((2.0 * b) * (1.0 - a)) + (sqrt(b) * ((2.0 * a) - 1.0));
}
half3 BlendSoftLight(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    c.r = SoftLight(a.r, b.r);
    c.g = SoftLight(a.g, b.g);
    c.b = SoftLight(a.b, b.b);
    return lerp(b, c, amount);
}

half HardLight(half a, half b)
{
    if (a < 0.5)
        return 2.0 * a * b;
    else
        return 1.0 - 2.0 * (1.0 - a) * (1.0 - b);
}
half3 BlendHardLight(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    c.r = HardLight(a.r, b.r);
    c.g = HardLight(a.g, b.g);
    c.b = HardLight(a.b, b.b);
    return lerp(b, c, amount);
}

half VividLight(half a, half b)
{
    if (a < 0.5)
        return ColorBurn(a, lerp(b, 1.0, 0.5));
    else
        return ColorDodge(a, lerp(b, 0.0, 0.5));
}
half3 BlendVividLight(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    c.r = VividLight(a.r, b.r);
    c.g = VividLight(a.g, b.g);
    c.b = VividLight(a.b, b.b);
    return lerp(b, c, amount);
}

half LinearLight(half a, half b)
{
    if (a < 0.5)
        return LinearBurn(a, lerp(b, 1.0, 0.5));
    else
        return LinearDodge(a, lerp(b, 0.0, 0.5));
}
half3 BlendLinearLight(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    c.r = LinearLight(a.r, b.r);
    c.g = LinearLight(a.g, b.g);
    c.b = LinearLight(a.b, b.b);
    return lerp(b, c, amount);
}

half PinLight(half a, half b)
{
    if (a < 0.5)
        return Darken(a, b);
    else
        return Lighten(a, b);
}
half3 BlendPinLight(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    c.r = PinLight(a.r, b.r);
    c.g = PinLight(a.g, b.g);
    c.b = PinLight(a.b, b.b);
    return lerp(b, c, amount);
}

half HardMix(half a, half b)
{
    a = a < 0.5 ? 0.0 : 1.0;
    b = b < 0.5 ? 0.0 : 1.0;
    return lerp(a, b, 0.5);
}
half3 BlendHardMix(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    c.r = HardMix(a.r, b.r);
    c.g = HardMix(a.g, b.g);
    c.b = HardMix(a.b, b.b);
    return lerp(b, c, amount);
}

// Inversion

half Difference(half a, half b)
{
    half dif1 = a - b;
    half dif2 = b - a;
    return max(dif1, dif2);
}
half3 BlendDifference(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    c.r = Difference(a.r, b.r);
    c.g = Difference(a.g, b.g);
    c.b = Difference(a.b, b.b);
    return lerp(b, c, amount);
}

half Exclusion(half a, half b)
{
    if (a < 0.5)
    {
        return lerp(b, 0.5, a * 2.0);
    }
    else
    {
        return lerp(0.5, 1.0 - b, (a - 0.5) * 2.0);
    }
}
half3 BlendExclusion(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    c.r = Exclusion(a.r, b.r);
    c.g = Exclusion(a.g, b.g);
    c.b = Exclusion(a.b, b.b);
    return lerp(b, c, amount);
}

// Cancelation

half Subtract(half a, half b)
{
    return max(b - a, 0.0);
}
half3 BlendSubtract(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    c.r = Subtract(a.r, b.r);
    c.g = Subtract(a.g, b.g);
    c.b = Subtract(a.b, b.b);
    return lerp(b, c, amount);
}

half Divide(half a, half b)
{
    return b / a;
}
half3 BlendDivide(half3 a, half3 b, half amount)
{
    half3 c = half3(1,1,1);
    c.r = Divide(a.r, b.r);
    c.g = Divide(a.g, b.g);
    c.b = Divide(a.b, b.b);
    return lerp(b, c, amount);
}

// Component

half3 BlendHue(half3 a, half3 b, half amount)
{
    half3 aHSL = RGBtoHSL(a);
    half3 bHSL = RGBtoHSL(b);
    
    half3 c = half3(1,1,1);
    c.x = aHSL.x;
    c.y = bHSL.y;
    c.z = bHSL.z;

    return lerp(b, HSLtoRGB(c), amount);
}

half3 BlendSaturation(half3 a, half3 b, half amount)
{
    half3 aHSL = RGBtoHSL(a);
    half3 bHSL = RGBtoHSL(b);
    
    half3 c = half3(1,1,1);
    c.x = bHSL.x;
    c.y = aHSL.y;
    c.z = bHSL.z;

    return lerp(b, HSLtoRGB(c), amount);
}

half3 BlendColor(half3 a, half3 b, half amount)
{
    half3 aHSL = RGBtoHSL(a);
    half3 bHSL = RGBtoHSL(b);
    
    half3 c = half3(1,1,1);
    c.x = aHSL.x;
    c.y = aHSL.y;
    c.z = bHSL.z;

    return lerp(b, HSLtoRGB(c), amount);
}

half3 BlendLuminosity(half3 a, half3 b, half amount)
{
    half3 aHSL = RGBtoHSL(a);
    half3 bHSL = RGBtoHSL(b);
    
    half3 c = half3(1,1,1);
    c.x = bHSL.x;
    c.y = bHSL.y;
    c.z = aHSL.z;

    return lerp(b, HSLtoRGB(c), amount);
}
