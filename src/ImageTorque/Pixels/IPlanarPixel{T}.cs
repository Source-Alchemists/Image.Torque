using System.Numerics;

namespace ImageTorque.Pixels;

public interface IPlanarPixel<T> : IPixel<T> where T : unmanaged, INumber<T>
{
}
