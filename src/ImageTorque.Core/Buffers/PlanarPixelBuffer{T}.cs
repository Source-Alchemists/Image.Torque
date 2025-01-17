using ImageTorque.Pixels;

namespace ImageTorque.Buffers;

/// <summary>
/// Represents a planar pixel buffer.
/// </summary>
public sealed record PlanarPixelBuffer<T> : PixelBuffer<T>
    where T : unmanaged, IPixel
{
    /// <inheritdoc/>
    public override PixelBufferType PixelBufferType => PixelBufferType.Planar;


    /// <summary>
    /// Initializes a new instance of the <see cref="PlanarPixelBuffer{T}"/> class.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public PlanarPixelBuffer(int width, int height) : base(width, height)
    {
        PixelFormat = PixelBufferMarshal.GetPixelFormat(PixelBufferType, PixelType);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanarPixelBuffer{T}"/> class.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="pixels">The pixels.</param>
    public PlanarPixelBuffer(int width, int height, ReadOnlySpan<T> pixels)
        : this(width, height)
    {
        pixels.CopyTo(Pixels);
    }

    /// <inheritdoc/>
    public override Span<T> GetChannel(int channelIndex)
    {
        if (channelIndex < 0 || channelIndex >= NumberOfFrames)
        {
            throw new ArgumentOutOfRangeException(nameof(channelIndex));
        }

        int channelLength = Width * Height;
        return Pixels.Slice(channelIndex * channelLength, channelLength);
    }

    /// <summary>
    /// Gets the row of pixels at the specified <paramref name="rowIndex"/> and <paramref name="channelIndex"/>.
    /// </summary>
    /// <param name="channelIndex">The channel index.</param>
    /// <param name="rowIndex">The row index.</param>
    /// <returns>The row of pixels.</returns>
    public Span<T> GetRow(int channelIndex, int rowIndex)
    {
        if (channelIndex < 0 || channelIndex >= NumberOfFrames)
        {
            throw new ArgumentOutOfRangeException(nameof(channelIndex));
        }

        if (rowIndex < 0 || rowIndex >= Height)
        {
            throw new ArgumentOutOfRangeException(nameof(rowIndex));
        }

        int frame = channelIndex * Width * Height;
        return Pixels.Slice(frame + rowIndex * Width, Width);
    }

    /// <summary>
    /// Gets the pixel buffer as read only.
    /// </summary>
    /// <returns>The pixel buffer as read only.</returns>
    public override IReadOnlyPixelBuffer<T> AsReadOnly() => new ReadOnlyPlanarPixelBuffer<T>(this);
}
