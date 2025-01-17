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
using ImageTorque.Codecs;

namespace ImageTorque.Processing;

internal class Decoder : IProcessor<DecoderParameters, IPixelBuffer>
{
    public IPixelBuffer Execute(DecoderParameters parameters)
    {
        Stream? stream = parameters.Input;

        if (!stream!.CanRead)
        {
            throw new NotSupportedException("Cannot read from the stream.");
        }

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using BufferedStream bs = new(stream);
        ICodec format = DetectCodec(bs, parameters.Configuration!);
        return format.Decoder.Decode(bs, parameters.Configuration!);
    }

    private static ICodec DetectCodec(Stream stream, IConfiguration configuration)
    {
        int headerSize = (int)Math.Min(configuration.MaxHeaderSize, stream.Length);
        if (headerSize <= 0)
        {
            throw new InvalidDataException("Header size could not be estimated!");
        }

        Span<byte> headersBuffer = headerSize > 512 ? new byte[headerSize] : stackalloc byte[headerSize];
        long startPosition = stream.Position;

        int n = 0;
        int i;
        do
        {
            i = stream.Read(headersBuffer[n..headerSize]);
            n += i;
        }
        while (n < headerSize && i > 0);

        stream.Position = startPosition;

        ReadOnlySpan<byte> targetHeadersBuffer = headersBuffer[..n];

        ICodec? codec = null;
        foreach (ICodec confCodec in configuration.Codecs)
        {
            if (confCodec.HeaderSize <= targetHeadersBuffer.Length && confCodec.IsSupportedDecoderFormat(targetHeadersBuffer))
            {
                codec = confCodec;
                break;
            }
        }

        if (codec is null)
        {
            throw new InvalidDataException("Format could not be detected!");
        }

        return codec;
    }
}
