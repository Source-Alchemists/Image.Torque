using BenchmarkDotNet.Attributes;
using ImageTorque.Codecs.Png;

namespace ImageTorque.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class PngIdentify
{
    private readonly PngDecoder _decoder = new();
    private readonly Configuration _configuration = ConfigurationFactory.Build([new PngCodec()]);
    private FileStream _imageTorqueStream = null!;
    private FileStream _imageSharpStream = null!;

    [GlobalSetup]
    public void Setup()
    {
        _imageTorqueStream = new FileStream("./lena24.png", FileMode.Open, FileAccess.Read);
        _imageSharpStream = new FileStream("./lena24.png", FileMode.Open, FileAccess.Read);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _imageTorqueStream?.Dispose();
        _imageSharpStream?.Dispose();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _imageTorqueStream.Position = 0;
        _imageSharpStream.Position = 0;
    }

    [Benchmark(Baseline = true)]
    public void ImageTorque()
    {
        _ = _decoder.Identify(_imageTorqueStream, _configuration);
    }

    [Benchmark]
    public void ImageSharp()
    {
        _ = SixLabors.ImageSharp.Image.Identify(_imageSharpStream);
    }
}
