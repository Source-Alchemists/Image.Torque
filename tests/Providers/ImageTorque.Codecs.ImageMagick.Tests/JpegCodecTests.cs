using System.Runtime.InteropServices;

using ImageTorque.Pixels;

namespace ImageTorque.Codecs.ImageMagick.Tests;

public class JpegCodecTests
{
    [Fact]
    public void Test_DecodeAndEncode_L8()
    {
        // Arrange
        var codec = new PngCodec();
        IImageDecoder decoder = codec.Decoder;
        IImageEncoder encoder = codec.Encoder;

        using FileStream stream = File.OpenRead("./lena8.jpg");

        // Act
        using Buffers.IPixelBuffer<L8>? pixelBuffer = decoder.Decode(stream) as Buffers.IPixelBuffer<L8>;

        // Assert
        string hash = TestHelper.CreateHash(MemoryMarshal.Cast<L8, byte>(pixelBuffer!.Pixels));
        Assert.Equal("578adbfede7e9a2633f749ae7686333b7e658fb84634d52ded1348403efa631e", hash);
        Assert.Equal(512, pixelBuffer.Width);
        Assert.Equal(512, pixelBuffer.Height);

        // Act
        using var outStream = new MemoryStream();
        var image = new Image(pixelBuffer!);
        encoder.Encode(outStream, image.AsPacked<L8>(), "jpg");

        // Assert
        Assert.True(outStream.Length > 0);
        outStream.Position = 0;
        string outHash = TestHelper.CreateHash(outStream.ToArray());
        Assert.Equal("c96b67587c8e65fbd085c273c5f2549a264fabbea68527e6b653796db9568b50", outHash);
    }

    [Fact]
    public void Test_DecodeAndEncode_Rgb24()
    {
        // Arrange
        var codec = new PngCodec();
        IImageDecoder decoder = codec.Decoder;
        IImageEncoder encoder = codec.Encoder;

        using FileStream stream = File.OpenRead("./lena24.jpg");

        // Act
        using Buffers.IPixelBuffer<Rgb24>? pixelBuffer = decoder.Decode(stream) as Buffers.IPixelBuffer<Rgb24>;

        // Assert
        string hash = TestHelper.CreateHash(MemoryMarshal.Cast<Rgb24, byte>(pixelBuffer!.Pixels));
        Assert.Equal("bca650f5c8f32950bf6bea5b0006f987a4169dc766dd61e2d81d4d90d594f703", hash);
        Assert.Equal(512, pixelBuffer.Width);
        Assert.Equal(512, pixelBuffer.Height);

        // Act
        using var outStream = new MemoryStream();
        var image = new Image(pixelBuffer!);
        encoder.Encode(outStream, image.AsPacked<Rgb24>(), "jpg");

        // Assert
        Assert.True(outStream.Length > 0);
        outStream.Position = 0;
        string outHash = TestHelper.CreateHash(outStream.ToArray());
        Assert.Equal("38d92154dca08582847f5fed913009eadf5c93496a66720a0489ce3ecaa347ac", outHash);
    }
}
