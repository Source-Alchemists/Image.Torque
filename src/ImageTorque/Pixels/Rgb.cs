using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ImageTorque.Pixels;

[StructLayout(LayoutKind.Explicit, Size = 12)]
public record struct Rgb : IL3Pixel<float>
{
    [FieldOffset(0)]
    public float Red;

    [FieldOffset(4)]
    public float Green;

    [FieldOffset(8)]
    public float Blue;

    public float R
    {
        get { return Red; }
        set { Red = value; }
    }

    public float G
    {
        get { return Green; }
        set { Green = value; }
    }

    public float B
    {
        get { return Blue; }
        set { Blue = value; }
    }

    public PixelType PixelType => PixelType.Rgb;

    public Rgb(float red, float green, float blue)
    {
        Red = red;
        Green = green;
        Blue = blue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Rgb(ValueTuple<float, float, float> value)
    {
        return new Rgb(value.Item1, value.Item2, value.Item3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rgb24 ToRgb24()
    {
        return new Rgb24(Convert.ToByte(Red), Convert.ToByte(Green), Convert.ToByte(Blue));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rgb48 ToRgb48()
    {
        return new Rgb48(Convert.ToUInt16(Red), Convert.ToUInt16(Green), Convert.ToUInt16(Blue));
    }
}
