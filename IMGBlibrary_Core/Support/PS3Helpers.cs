using static IMGBlibrary_Core.Support.IMGBFlags;

namespace IMGBlibrary_Core.Support
{
    internal class PS3Helpers
    {
        public static int MipWidth = 0;
        public static int MipHeight = 0;

        public static byte[] ProcessPS3ImageData(GTEX gtex, byte[] rawMipData)
        {
            var hasProcessed = false;
            var isSwizzled = false;

            if (gtex.ImgType == ImageTypes.classic_swizzled)
            {
                if (gtex.ImgFormat == ImageFormats.r8g8b8a8_type3 || gtex.ImgFormat == ImageFormats.r8g8b8a8)
                {
                    isSwizzled = true;
                }
            }

            if (isSwizzled)
            {
                rawMipData = MortonUnswizzle(rawMipData);
                hasProcessed = true;
            }

            var colorAdjust = gtex.ImgFormat == ImageFormats.r8g8b8a8_type3 || gtex.ImgFormat == ImageFormats.r8g8b8a8;

            if (colorAdjust)
            {
                rawMipData = AdjustColor(rawMipData);
                hasProcessed = true;
            }

            if (hasProcessed)
            {
                if (gtex.MipCount > 1)
                {
                    SetNextMipHeightWidth(gtex);
                }
            }

            return rawMipData;
        }

        // Swizzle algorithm adapted from Xentax
        private static byte[] MortonUnswizzle(byte[] mipData)
        {
            var unswizzledData = new byte[MipWidth * MipHeight * 4];
            var processBuffer = new byte[4];

            var readPos = 0;
            for (int i = 0; i < MipWidth * MipHeight; i++)
            {
                Array.Copy(mipData, readPos, processBuffer, 0, 4);

                int x = 0, y = 0;
                int xw = 1, yw = 1;
                int tnumt = i;
                int xbit = MipWidth;
                int ybit = MipHeight;

                while (xbit > 1 || ybit > 1)
                {
                    if (xbit > 1)
                    {
                        x += xw * (tnumt & 1);
                        tnumt >>= 1;
                        xw *= 2;
                        xbit >>= 1;
                    }
                    if (ybit > 1)
                    {
                        y += yw * (tnumt & 1);
                        tnumt >>= 1;
                        yw *= 2;
                        ybit >>= 1;
                    }
                }

                var processedPixel = y * MipWidth + x;
                int pixelOffset = processedPixel * 4;

                Array.Copy(processBuffer, 0, unswizzledData, pixelOffset, 4);

                readPos += 4;
            }

            return unswizzledData;
        }

        private static byte[] AdjustColor(byte[] mipData)
        {
            var adjustedColorsData = new byte[mipData.Length];

            using (var mipDataReader = new BinaryReader(new MemoryStream(mipData)))
            {
                using (var adjustedColorStream = new MemoryStream())
                {
                    adjustedColorStream.Write(mipData, 0, mipData.Length);
                    adjustedColorStream.Seek(0, SeekOrigin.Begin);

                    using (var adjustedColorWriter = new BinaryWriter(adjustedColorStream))
                    {
                        var readPos = 0;
                        var writePos = 0;

                        for (int i = 0; i < mipData.Length; i++)
                        {
                            mipDataReader.BaseStream.Position = readPos;
                            var alpha = mipDataReader.ReadByte();
                            var red = mipDataReader.ReadByte();
                            var green = mipDataReader.ReadByte();
                            var blue = mipDataReader.ReadByte();

                            adjustedColorWriter.BaseStream.Position = writePos;
                            adjustedColorWriter.Write(blue);
                            adjustedColorWriter.Write(green);
                            adjustedColorWriter.Write(red);
                            adjustedColorWriter.Write(alpha);

                            if (readPos < (mipData.Length - 4))
                            {
                                readPos += 4;
                                writePos += 4;
                            }
                        }

                        adjustedColorStream.Seek(0, SeekOrigin.Begin);
                        adjustedColorStream.Read(adjustedColorsData, 0, adjustedColorsData.Length);
                    }
                }
            }

            return adjustedColorsData;
        }

        private static void SetNextMipHeightWidth(GTEX gtex)
        {
            switch (gtex.ImgFormat)
            {
                case ImageFormats.dxt1:
                case ImageFormats.dxt3:
                case ImageFormats.dxt5:
                    MipHeight /= 2;

                    if (MipHeight < 4)
                    {
                        MipHeight = 4;
                    }

                    MipWidth /= 2;

                    if (MipWidth < 4)
                    {
                        MipWidth = 4;
                    }
                    break;

                case ImageFormats.r8g8b8a8:
                case ImageFormats.r8g8b8a8_type3:
                case ImageFormats.r8:
                    if (MipHeight == 1)
                    {
                        MipHeight = 1;
                    }
                    else
                    {
                        MipHeight /= 2;
                    }

                    if (MipWidth == 1)
                    {
                        MipWidth = 1;
                    }
                    else
                    {
                        MipWidth /= 2;
                    }
                    break;
            }
        }
    }
}