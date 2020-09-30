using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.IO;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using PixelFormat = SharpDX.WIC.PixelFormat;
using RenderTarget = SharpDX.Direct2D1.RenderTarget;

namespace SRTPluginUIMGUDirectXOverlay.Utilities
{
    // Ref: https://github.com/michel-pi/GameOverlay.Net/
    // Adjustments to the image loading mechanism in GameOverlay.net due to an issue I encountered. I am not certain it is a bug so this is here more for testing...?
    public static class ImageLoader
    {
        private static readonly ImagingFactory imageFactory = new ImagingFactory();

        public static Bitmap LoadBitmap(RenderTarget device, byte[] bytes)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(bytes));

            using (MemoryStream stream = new MemoryStream(bytes))
            using (BitmapDecoder decoder = new BitmapDecoder(imageFactory, stream, DecodeOptions.CacheOnDemand))
                return Decode(device, decoder);
        }

        private static Bitmap Decode(RenderTarget device, BitmapDecoder decoder)
        {
            using (BitmapFrameDecode frame = decoder.GetFrame(0))
            {
                FormatConverter converter = new FormatConverter(imageFactory); // Converter get clobbered on failure so we're not wrapping this in a using.
                foreach (Guid format in _pixelFormatEnumerator)
                {
                    try
                    {
                        converter.Initialize(frame, format);
                        return Bitmap.FromWicBitmap(device, converter);
                    }
                    catch // Ignore error here, just try another format. We'll throw an error below if we exhaust all options.
                    {
                        converter?.Dispose();
                        converter = new FormatConverter(imageFactory);
                    }
                }
                converter?.Dispose();
            }

            throw new Exception("Unsupported Image Format!");
        }

        private static readonly Guid[] _floatingPointFormats = new Guid[]
        {
            PixelFormat.Format128bppRGBAFloat,
            PixelFormat.Format128bppRGBAFixedPoint,
            PixelFormat.Format128bppPRGBAFloat,
            PixelFormat.Format128bppRGBFloat,
            PixelFormat.Format128bppRGBFixedPoint,
            PixelFormat.Format96bppRGBFixedPoint,
            PixelFormat.Format96bppRGBFloat,
            PixelFormat.Format64bppBGRAFixedPoint,
            PixelFormat.Format64bppRGBAFixedPoint,
            PixelFormat.Format64bppRGBFixedPoint,
            PixelFormat.Format48bppRGBFixedPoint,
            PixelFormat.Format48bppBGRFixedPoint,
            PixelFormat.Format32bppGrayFixedPoint,
            PixelFormat.Format32bppGrayFloat,
            PixelFormat.Format16bppGrayFixedPoint
        };

        private static readonly Guid[] _standardPixelFormats = new Guid[]
        {
            PixelFormat.Format144bpp8ChannelsAlpha,
            PixelFormat.Format128bpp8Channels,
            PixelFormat.Format128bpp7ChannelsAlpha,
            PixelFormat.Format112bpp7Channels,
            PixelFormat.Format112bpp6ChannelsAlpha,
            PixelFormat.Format96bpp6Channels,
            PixelFormat.Format96bpp5ChannelsAlpha,
            PixelFormat.Format80bpp5Channels,
            PixelFormat.Format80bppCMYKAlpha,
            PixelFormat.Format80bpp4ChannelsAlpha,
            PixelFormat.Format72bpp8ChannelsAlpha,
            PixelFormat.Format64bppBGRA,
            PixelFormat.Format64bppRGBA,
            PixelFormat.Format64bppPBGRA,
            PixelFormat.Format64bppPRGBA,
            PixelFormat.Format64bpp8Channels,
            PixelFormat.Format64bpp4Channels,
            PixelFormat.Format64bppRGBAHalf,
            PixelFormat.Format64bppPRGBAHalf,
            PixelFormat.Format64bpp7ChannelsAlpha,
            PixelFormat.Format64bpp3ChannelsAlpha,
            PixelFormat.Format64bppRGB,
            PixelFormat.Format64bppCMYK,
            PixelFormat.Format64bppRGBHalf,
            PixelFormat.Format56bpp7Channels,
            PixelFormat.Format56bpp6ChannelsAlpha,
            PixelFormat.Format48bpp6Channels,
            PixelFormat.Format48bppRGB,
            PixelFormat.Format48bppBGR,
            PixelFormat.Format48bpp3Channels,
            PixelFormat.Format48bppRGBHalf,
            PixelFormat.Format48bpp5ChannelsAlpha,
            PixelFormat.Format40bpp5Channels,
            PixelFormat.Format40bppCMYKAlpha,
            PixelFormat.Format40bpp4ChannelsAlpha,
            PixelFormat.Format32bppBGRA,
            PixelFormat.Format32bppRGBA,
            PixelFormat.Format32bppPBGRA,
            PixelFormat.Format32bppPRGBA,
            PixelFormat.Format32bppRGBA1010102,
            PixelFormat.Format32bppRGBA1010102XR,
            PixelFormat.Format32bppCMYK,
            PixelFormat.Format32bpp4Channels,
            PixelFormat.Format32bpp3ChannelsAlpha,
            PixelFormat.Format32bppBGR,
            PixelFormat.Format32bppRGB,
            PixelFormat.Format32bppRGBE,
            PixelFormat.Format32bppBGR101010,
            PixelFormat.Format24bppBGR,
            PixelFormat.Format24bppRGB,
            PixelFormat.Format24bpp3Channels,
            PixelFormat.Format16bppBGR555,
            PixelFormat.Format16bppBGR565,
            PixelFormat.Format16bppBGRA5551,
            PixelFormat.Format16bppGray,
            PixelFormat.Format16bppGrayHalf,
            PixelFormat.Format16bppCbCr,
            PixelFormat.Format16bppYQuantizedDctCoefficients,
            PixelFormat.Format16bppCbQuantizedDctCoefficients,
            PixelFormat.Format16bppCrQuantizedDctCoefficients,
            PixelFormat.Format8bppIndexed,
            PixelFormat.Format8bppAlpha,
            PixelFormat.Format8bppY,
            PixelFormat.Format8bppCb,
            PixelFormat.Format8bppCr,
            PixelFormat.Format8bppGray
        };

        private static readonly Guid[] _uncommonFormats = new Guid[]
        {
            PixelFormat.Format4bppIndexed,
            PixelFormat.Format2bppIndexed,
            PixelFormat.Format1bppIndexed,
            PixelFormat.Format4bppGray,
            PixelFormat.Format2bppGray,
            PixelFormat.FormatDontCare,
            PixelFormat.FormatBlackWhite
        };

        private static IEnumerable<Guid> _pixelFormatEnumerator
        {
            get
            {
                foreach (var format in _standardPixelFormats)
                {
                    yield return format;
                }

                foreach (var format in _floatingPointFormats)
                {
                    yield return format;
                }

                foreach (var format in _uncommonFormats)
                {
                    yield return format;
                }
            }
        }
    }
}