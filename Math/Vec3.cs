using System;
using System.Collections;
using System.Collections.Generic;

public struct Vec3
{
    //------------------------------------------------------------------------
    public float x;
    public float y;
    public float z;

    //------------------------------------------------------------------------
    public static Vec3 zero    => new Vec3( 0,  0,  0);
    public static Vec3 one     => new Vec3( 1,  1,  1);
    public static Vec3 left    => new Vec3(-1,  0,  0);
    public static Vec3 right   => new Vec3( 1,  0,  0);
    public static Vec3 back    => new Vec3( 0,  0, -1);
    public static Vec3 forward => new Vec3( 0,  0,  1);
    public static Vec3 up      => new Vec3( 0,  1,  0);

    //------------------------------------------------------------------------
    public float magnitude => MathF.Sqrt(x*x + y*y + z*z);

    //------------------------------------------------------------------------
    public Vec3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    //------------------------------------------------------------------------
    public static Vec3 operator -(Vec3 obj) => new Vec3(-obj.x, -obj.y, -obj.z);

    //------------------------------------------------------------------------
    public static Vec3 operator +(Vec3 left, Vec3 right) => new Vec3(
        left.x + right.x,
        left.y + right.y,
        left.z + right.z
    );

    //------------------------------------------------------------------------
    public static Vec3 operator -(Vec3 left, Vec3 right) => new Vec3(
        left.x - right.x,
        left.y - right.y,
        left.z - right.z
    );

    //------------------------------------------------------------------------
    public static Vec3 operator *(Vec3 vec, float num) => new Vec3(
        num * vec.x,
        num * vec.y,
        num * vec.z
    );
    public static Vec3 operator *(float num, Vec3 vec) => vec * num;

    //------------------------------------------------------------------------
    public static Vec3 operator /(Vec3 vec, float num) => new Vec3(
        vec.x / num,
        vec.y / num,
        vec.z / num
    );

    //------------------------------------------------------------------------
    public static implicit operator Vec2(Vec3 obj) => new Vec2(obj.x, obj.y);
}
