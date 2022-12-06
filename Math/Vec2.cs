using System;
using System.Collections;
using System.Collections.Generic;

public struct Vec2
{
    //------------------------------------------------------------------------
    public float x;
    public float y;

    //------------------------------------------------------------------------
    public static Vec2 zero  => new Vec2( 0,  0);
    public static Vec2 one   => new Vec2( 1,  1);
    public static Vec2 left  => new Vec2(-1,  0);
    public static Vec2 right => new Vec2( 1,  0);
    public static Vec2 up    => new Vec2( 0,  1);
    public static Vec2 down  => new Vec2( 0, -1);

    //------------------------------------------------------------------------
    public float magnitude => MathF.Sqrt(x*x + y*y);

    //------------------------------------------------------------------------
    public Vec2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    //------------------------------------------------------------------------
    public static Vec2 operator -(Vec2 val) => new Vec2(-val.x, -val.y);

    //------------------------------------------------------------------------
    public static Vec2 operator +(Vec2 left, Vec2 right) => new Vec2(
        left.x + right.x,
        left.y + right.y
    );

    //------------------------------------------------------------------------
    public static Vec2 operator -(Vec2 left, Vec2 right) => new Vec2(
        left.x - right.x,
        left.y - right.y
    );

    //------------------------------------------------------------------------
    public static Vec2 operator *(Vec2 vec, float num) => new Vec2(
        num * vec.x,
        num * vec.y
    );
    public static Vec2 operator *(float num, Vec2 vec) => vec * num;

    //------------------------------------------------------------------------
    public static Vec2 operator /(Vec2 vec, float num) => new Vec2(
        vec.x / num,
        vec.y / num
    );

    //------------------------------------------------------------------------
    public static implicit operator Vec3(Vec2 val) => new Vec3(val.x, val.y, 0f);
}
