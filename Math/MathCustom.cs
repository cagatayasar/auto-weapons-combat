using System;
using System.Collections;
using System.Collections.Generic;

public static class MathCustom
{
    public const float Deg2Rad = 0.017453292f;
    public const float Rad2Deg = 57.29578f;

    // maybe MathF.Round(x, 0) is enough and I am too sceptical
    public static int RoundToInt(float x) => (int)(MathF.Round(x) + 0.1f);
}
