using System.Buffers;
using System.Runtime.InteropServices;
using ImageTorque.Memory;
using ImageTorque.Pixels;

namespace ImageTorque.Buffers;

/// <summary>
/// Represents a pixel buffer.
/// </summary>
public record PixelBuffer<T> : IPixelBuffer<T>
    where T : unmanaged, IPixel
{
    private readonly Memory<T> _memory;
    private readonly IMemoryOwner<T> _memoryOwner;
    private bool _isDisposed = false;

    /// <summary>
    /// Gets the pixel at the specified index.
    /// </summary>
    public T this[int index] { get => Pixels[index]; set => Pixels[index] = value; }

    /// <summary>
    /// Gets the pixel at the specified coordinates.
    /// </summary>
    public T this[int x, int y]
    {
        get
        {
            int index = y * Width + x;
            return _memory.Span[index];
        }
        set
        {
            int index = y * Width + x;
            _memory.Span[index] = value;
        }
    }

    /// <summary>
    /// Gets the width.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the number of frames.
    /// </summary>
    public int NumberOfFrames { get; protected set; } = 1;

    /// <summary>
    /// Gets the size.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is color.
    /// </summary>
    public bool IsColor { get; }

    /// <summary>
    /// Gets the pixels.
    /// </summary>
    public Span<T> Pixels => _memory.Span;

    /// <summary>
    /// Gets the buffer.
    /// </summary>
    public Span<byte> Buffer => MemoryMarshal.AsBytes(_memory.Span);

    /// <summary>
    /// Gets the pixel type.
    /// </summary>
    public PixelType PixelType { get; }

    /// <summary>
    /// Gets the pixel format.
    /// </summary>
    public PixelFormat PixelFormat { get; protected set; } = PixelFormat.Unknown;

    /// <summary>
    /// Gets the pixel buffer type.
    /// </summary>
    public virtual PixelBufferType PixelBufferType => PixelBufferType.Packed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PixelBuffer{T}"/> class.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public PixelBuffer(int width, int height)
    {
        Width = width;
        Height = height;
        T pixel = Activator.CreateInstance<T>();
        PixelType = pixel.PixelType;
        PixelFormat = PixelBufferMarshal.GetPixelFormat(PixelBufferType, PixelType);
        if (PixelBufferType == PixelBufferType.Planar && (PixelType == PixelType.LS || PixelType == PixelType.L8 || PixelType == PixelType.L16))
        {
            NumberOfFrames *= 3;
        }

        Size = width * height * NumberOfFrames;

        _memoryOwner = OptimizedMemoryPool<T>.Shared.Rent(Width * Height * NumberOfFrames);
        _memory = _memoryOwner.Memory;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PixelBuffer{T}"/> class.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="pixels">The pixels.</param>
    public PixelBuffer(int width, int height, ReadOnlySpan<T> pixels)
        : this(width, height)
    {
        pixels.CopyTo(Pixels);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PixelBuffer{T}"/> class.
    /// </summary>
    /// <param name="other">The pixel buffer to copy.</param>
    public PixelBuffer(PixelBuffer<T> other)
    {
        Width = other.Width;
        Height = other.Height;
        NumberOfFrames = other.NumberOfFrames;
        Size = other.Size;
        IsColor = other.IsColor;
        PixelType = other.PixelType;
        PixelFormat = other.PixelFormat;

        _memoryOwner = OptimizedMemoryPool<T>.Shared.Rent(Width * Height * NumberOfFrames);
        _memory = _memoryOwner.Memory;
        other.Pixels.CopyTo(_memory.Span);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets the channel.
    /// </summary>
    /// <param name="channelIndex">Index of the channel.</param>
    /// <returns>Span of the channel.</returns>
    public virtual Span<T> GetChannel(int channelIndex)
    {
        if (channelIndex != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(channelIndex));
        }

        return Pixels;
    }

    /// <summary>
    /// Gets the row.
    /// </summary>
    /// <remarks>Instead of throwing ArgumentOutOfRange it will provide a empty/partial row.</remarks>
    /// <param name="rowIndex">Index of the row.</param>
    /// <returns>Span of the row.</returns>
    public Span<T> GetRow(int rowIndex)
    {
        int startPos = rowIndex * Width;
        int count = Width;
        int length = _memory.Span.Length;
        if (startPos > length || length - startPos < 1)
        {
            var fallback = new Span<T>(new T[count]);
            if (startPos < length)
            {
                _memory.Span.Slice(startPos, (int)(uint)length - startPos);
            }

            return fallback;
        }

        return _memory.Span.Slice(startPos, count);
    }

    /// <summary>
    /// Gets the pixel buffer as read only.
    /// </summary>
    /// <returns>The pixel buffer as read only.</returns>
    public virtual IReadOnlyPixelBuffer<T> AsReadOnly() => new ReadOnlyPackedPixelBuffer<T>(this);

    IReadOnlyPixelBuffer IPixelBuffer.AsReadOnly() => AsReadOnly();

    /// <summary>
    /// Indicates whether the specified <see cref="PixelBuffer{T}"/>, is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="PixelBuffer{T}"/> to compare with this instance.</param>
    /// <returns><c>true</c> if the specified <see cref="PixelBuffer{T}"/> is equal to this instance; otherwise, <c>false</c>.</returns>
    public virtual bool Equals(PixelBuffer<T>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (Width != other.Width || Height != other.Height)
        {
            return false;
        }

        return Pixels.SequenceEqual(other.Pixels);
    }

    /// <summary>
    /// Gets a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height, PixelType);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">if set to <c>true</c> [disposing].</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _memoryOwner.Dispose();
            }

            _isDisposed = true;
        }
    }
}
