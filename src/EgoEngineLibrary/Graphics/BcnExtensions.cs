using BCnEncoder.Decoder;
using BCnEncoder.Shared.ImageFiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace EgoEngineLibrary.Graphics
{
    public static class BcnExtensions
    {
        public static void DecodeDdsToPixels<TPixel>(this BcDecoder decoder, DdsFile dds, Span<TPixel> pixels)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var width = (int)dds.header.dwWidth;
            var height = (int)dds.header.dwHeight;
            if (decoder.IsHdrFormat(dds))
            {
                var source = decoder.DecodeHdr(dds).AsSpan();
                for (var r = 0; r < height; ++r)
                {
                    var start = r * width;
                    var destRow = pixels.Slice(start, width);
                    var sorcRow = source.Slice(start, width);
                    for (var c = 0; c < destRow.Length; ++c)
                    {
                        ref var destPixel = ref destRow[c];
                        ref var sorcPixel = ref sorcRow[c];

                        var rgbVal = sorcPixel.ToVector3();
                        destPixel.FromScaledVector4(new Vector4(rgbVal, 1));
                    }
                }
            }
            else
            {
                var source = MemoryMarshal.Cast<BCnEncoder.Shared.ColorRgba32, Rgba32>(decoder.Decode(dds));
                for (var r = 0; r < height; r++)
                {
                    var start = r * width;
                    var destRow = pixels.Slice(start, width);
                    var sorcRow = source.Slice(start, width);
                    PixelOperations<Rgba32>.Instance.To(Configuration.Default, sorcRow, destRow);
                }
            }
        }
    }
}
