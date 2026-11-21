using static IMGBlibrary_Core.Support.IMGBFlags;

namespace IMGBlibrary_Core.Support
{
    internal class PS3Helpers
    {
        public static int MipWidth = 0;
        public static int MipHeight = 0;

        public static byte[] ProcessPS3ImageData(ref bool doneCopying, GTEX gtex, uint mipSize, FileStream imgbStream, BinaryWriter ddsWriter)
        {
            var rawMipData = new byte[mipSize];
            _ = imgbStream.Read(rawMipData, 0, (int)mipSize);

            var hasProcessed = false;
            var isSwizzled = false;

            if (gtex.ImgType == ImageTypes.classic_swizzled)
            {
                if (gtex.ImgFormat == ImageFormats.r8g8b8a8 || gtex.ImgFormat == ImageFormats.r8g8b8a8_2)
                {
                    isSwizzled = true;
                }
            }

            if (isSwizzled)
            {
                rawMipData = MortonUnswizzle(rawMipData);
                hasProcessed = true;
            }

            var colorAdjust = gtex.ImgFormat == ImageFormats.r8g8b8a8 || gtex.ImgFormat == ImageFormats.r8g8b8a8_2;

            if (colorAdjust)
            {
                rawMipData = AdjustColor(rawMipData);
                hasProcessed = true;
            }

            if (hasProcessed)
            {
                ddsWriter.Write(rawMipData);
                doneCopying = true;

                if (gtex.MipCount > 1)
                {
                    SetNextMipHeightWidth(gtex);
                }
            }

            return rawMipData;
        }

        private static byte[] MortonUnswizzle(byte[] mipData)
        {
            int width = MipWidth;
            int height = MipHeight;

            var unswizzledData = new byte[width * height * 4];
            var processBuffer = new byte[4];

            var readPos = 0;
            for (int i = 0; i < width * height; i++)
            {
                Array.Copy(mipData, readPos, processBuffer, 0, 4);

                int val1 = 0;
                int val2 = 0;
                int val3;
                int val4 = (val3 = 1);
                int val5 = i;
                int val6 = width;
                int val7 = height;

                while (val6 > 1 || val7 > 1)
                {
                    if (val6 > 1)
                    {
                        val1 += val4 * (val5 & 1);
                        val5 >>= 1;
                        val4 *= 2;
                        val6 >>= 1;
                    }
                    if (val7 > 1)
                    {
                        val2 += val3 * (val5 & 1);
                        val5 >>= 1;
                        val3 *= 2;
                        val7 >>= 1;
                    }
                }

                var processedPixel = val2 * width + val1;
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
            if (gtex.ImgFormat == ImageFormats.dxt1 || gtex.ImgFormat == ImageFormats.dxt3 || gtex.ImgFormat == ImageFormats.dxt5)
            {
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
            }

            if (gtex.ImgFormat == ImageFormats.r8g8b8a8 || gtex.ImgFormat == ImageFormats.r8g8b8a8_2)
            {
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
            }
        }
    }
}