/*
 * Copyright 2024 Source Alchemists
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using ImageTorque.Buffers;
using ImageTorque.Pixels;
using ImageTorque.Processing;

namespace ImageTorque;

/// <summary>
/// Image extensions.
/// </summary>
public static partial class ImageExtensions
{
    private static readonly Encoder s_encoder = new();
    private static readonly GrayscaleFilter s_grayscaleFilter = new();
    private static readonly Mirror s_mirror = new();
    private static readonly Binarizer s_binarizer = new();

    /// <summary>
    /// Saves the image to the specified stream.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="stream">The stream.</param>
    /// <param name="encodeType">Type of the encode.</param>
    /// <param name="quality">The quality.</param>
    public static void Save(this IImage image, Stream stream, string encodeType, int quality = 80) => Save(image, stream, encodeType, ConfigurationFactory.GetOrCreateDefault(), quality);

    /// <summary>
    /// Saves the image to the specified stream using the specified encoder type, configuration, and quality.
    /// </summary>
    /// <param name="image">The image to save.</param>
    /// <param name="stream">The stream to save the image to.</param>
    /// <param name="encodeType">The type of encoder to use.</param>
    /// <param name="configuration">The configuration for the encoder.</param>
    /// <param name="quality">The quality of the saved image (default is 80).</param>
    /// <exception cref="NotSupportedException">Thrown when the pixel format of the image is not supported.</exception>
    public static void Save(this IImage image, Stream stream, string encodeType, IConfiguration configuration, int quality = 80)
    {
        IReadOnlyPixelBuffer pixelBuffer = image.PixelFormat switch
        {
            PixelFormat.Mono8 => image.AsPacked<L8>(),
            PixelFormat.Mono16 => image.AsPacked<L16>(),
            PixelFormat.Rgb24Packed => image.AsPacked<Rgb24>(),
            PixelFormat.Rgb48Packed => image.AsPacked<Rgb48>(),
            PixelFormat.Rgb888Planar => image.AsPacked<Rgb24>(),
            PixelFormat.Rgb161616Planar => image.AsPacked<Rgb48>(),
            PixelFormat.Mono => image.AsPacked<L8>(),
            PixelFormat.RgbPacked => image.AsPacked<Rgb24>(),
            PixelFormat.RgbPlanar => image.AsPacked<Rgb24>(),
            _ => throw new NotSupportedException($"The pixel format {image.PixelFormat} is not supported."),
        };
        s_encoder.Execute(new EncoderParameters
        {
            Input = pixelBuffer,
            Stream = stream,
            EncoderType = encodeType,
            Quality = quality,
            Configuration = configuration
        });
    }

    /// <summary>
    /// Saves the image as file in the specified format.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="path">The path.</param>
    /// <param name="quality">The quality.</param>
    public static void Save(this IImage image, string path, int quality = 80) => Save(image, path, ConfigurationFactory.GetOrCreateDefault(), quality);


    /// <summary>
    /// Saves the image to the specified file path using the specified encoder type, configuration, and quality.
    /// </summary>
    /// <param name="image">The image to save.</param>
    /// <param name="path">The file path to save the image to.</param>
    /// <param name="configuration">The configuration to use for saving the image.</param>
    /// <param name="quality">The quality of the saved image (default is 80).</param>
    public static void Save(this IImage image, string path, IConfiguration configuration, int quality = 80)
    {
        string extension = Path.GetExtension(path)[1..];
        using FileStream fileStream = File.OpenWrite(path);
        Save(image, fileStream, extension, configuration, quality);
    }

    /// <summary>
    /// Calculates the image clamp size. Keeping the aspect ratio.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="maxSize">The max size.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public static void CalculateClampSize(this IImage image, int maxSize, out int width, out int height)
    {
        if (image.Width > image.Height)
        {
            double per = (double)maxSize / image.Width;
            width = (int)(image.Width * per);
            height = (int)(image.Height * per);
        }
        else if (image.Height > image.Width)
        {
            double per = (double)maxSize / image.Height;
            width = (int)(image.Width * per);
            height = (int)(image.Height * per);
        }
        else
        {
            width = maxSize;
            height = maxSize;
        }
    }

    /// <summary>
    /// Converts the image to grayscale.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <returns>The grayscale image.</returns>
    public static Image ToGrayscale(this Image image)
    {
        if (!image.IsColor)
        {
            return new Image(image);
        }

        IReadOnlyPixelBuffer sourceBuffer = null!;
        Type targetType = null!;
        switch (image.PixelFormat)
        {
            case PixelFormat.RgbPacked:
                sourceBuffer = image.AsPacked<Rgb>();
                targetType = typeof(PixelBuffer<LS>);
                break;
            case PixelFormat.Rgb24Packed:
                sourceBuffer = image.AsPacked<Rgb24>();
                targetType = typeof(PixelBuffer<L8>);
                break;
            case PixelFormat.Rgb48Packed:
                sourceBuffer = image.AsPacked<Rgb48>();
                targetType = typeof(PixelBuffer<L16>);
                break;
            case PixelFormat.RgbPlanar:
                sourceBuffer = image.AsPlanar<LS>();
                targetType = typeof(PixelBuffer<LS>);
                break;
            case PixelFormat.Rgb888Planar:
                sourceBuffer = image.AsPlanar<L8>();
                targetType = typeof(PixelBuffer<L8>);
                break;
            case PixelFormat.Rgb161616Planar:
                sourceBuffer = image.AsPlanar<L16>();
                targetType = typeof(PixelBuffer<L16>);
                break;
        }
        IPixelBuffer grayscaleBuffer = s_grayscaleFilter.Execute(new GrayscaleFilterParameters
        {
            Input = sourceBuffer,
            OutputType = targetType
        });

        return new Image(grayscaleBuffer);
    }

    /// <summary>
    /// Mirror the image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="mirrorMode">The mirror mode.</param>
    /// <returns>The mirrored image.</returns>
    public static Image Mirror(this Image image, MirrorMode mirrorMode)
    {
        IReadOnlyPixelBuffer sourceBuffer = image.GetPixelBuffer();
        IPixelBuffer mirroredBuffer = s_mirror.Execute(new MirrorParameters
        {
            Input = sourceBuffer,
            MirrorMode = mirrorMode
        });

        return new Image(mirroredBuffer);
    }

    /// <summary>
    /// Binary threshold the image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="threshold">The threshold.</param>
    /// <param name="mode">The threshold mode.</param>
    /// <returns>The binary thresholded image.</returns>
    public static Image Binarize(this Image image, float threshold = 0.5f, BinaryThresholdMode mode = BinaryThresholdMode.Lumincance)
    {
        IReadOnlyPixelBuffer sourceBuffer = image.PixelFormat switch
        {
            PixelFormat.RgbPlanar => image.AsPacked<Rgb>(),
            PixelFormat.Rgb888Planar => image.AsPacked<Rgb24>(),
            PixelFormat.Rgb161616Planar => image.AsPacked<Rgb48>(),
            _ => image.GetPixelBuffer(),
        };
        IPixelBuffer binarizedBuffer = s_binarizer.Execute(new BinarizerParameters
        {
            Input = sourceBuffer,
            Threshold = threshold,
            Mode = mode
        });

        return new Image(binarizedBuffer);
    }

    internal static IReadOnlyPixelBuffer GetPixelBuffer(this Image image) => image.GetPixelBuffer(image.PixelFormat);

    internal static IReadOnlyPixelBuffer GetPixelBuffer(this Image image, PixelFormat pixelFormat)
    {
        return pixelFormat switch
        {
            PixelFormat.Mono => image.AsPacked<LS>(),
            PixelFormat.Mono8 => image.AsPacked<L8>(),
            PixelFormat.Mono16 => image.AsPacked<L16>(),
            PixelFormat.RgbPacked => image.AsPacked<Rgb>(),
            PixelFormat.Rgb24Packed => image.AsPacked<Rgb24>(),
            PixelFormat.Rgb48Packed => image.AsPacked<Rgb48>(),
            PixelFormat.RgbPlanar => image.AsPlanar<LS>(),
            PixelFormat.Rgb888Planar => image.AsPlanar<L8>(),
            PixelFormat.Rgb161616Planar => image.AsPlanar<L16>(),
            _ => throw new NotSupportedException($"The pixel format {image.PixelFormat} is not supported."),
        };
    }
}
