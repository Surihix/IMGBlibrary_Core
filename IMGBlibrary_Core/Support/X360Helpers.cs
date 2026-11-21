using static IMGBlibrary_Core.Support.IMGBFlags;

namespace IMGBlibrary_Core.Support
{
    internal class X360Helpers
    {
        public static int MipWidth = 0;
        public static int MipHeight = 0;

        public static void ProcessX360ImageData(ref bool doneCopying, GTEX gtex, uint mipSize, FileStream imgbStream, BinaryWriter ddsWriter)
        {
            var rawMipData = new byte[mipSize];
            _ = imgbStream.Read(rawMipData, 0, rawMipData.Length);

            var adjustedMipData = AdjustMipData(gtex, rawMipData);

            // Remove this after padding is sorted
            if (adjustedMipData.Length == 0)
            {
                return;
            }

            var isSwizzled = gtex.ImgType == ImageTypes.classic_swizzled || gtex.ImgType == ImageTypes.cubemap_swizzled;

            if (isSwizzled)
            {
                adjustedMipData = ConvertToLinearTexture(adjustedMipData, gtex);
            }

            adjustedMipData = XEndian16(adjustedMipData);

            var colorAdjust = gtex.ImgFormat == ImageFormats.r8g8b8a8 || gtex.ImgFormat == ImageFormats.r8g8b8a8_2;

            if (colorAdjust)
            {
                adjustedMipData = AdjustColor(adjustedMipData);
            }

            ddsWriter.Write(adjustedMipData);
            doneCopying = true;

            if (gtex.MipCount > 1)
            {
                SetNextMipHeightWidth(gtex);
            }
        }

        private static byte[] AdjustMipData(GTEX gtex, byte[] mipData)
        {
            var adjustedMipData = Array.Empty<byte>();

            var hasPadding = false;

            if (gtex.ImgType == ImageTypes.classic_swizzled || gtex.ImgType == ImageTypes.cubemap_swizzled)
            {
                if (MipWidth % 128 != 0)
                {
                    hasPadding = true;
                }

                if (MipHeight % 128 != 0)
                {
                    hasPadding = true;
                }
            }
            else
            {
                if (MipWidth % 128 != 0)
                {
                    hasPadding = true;
                }
            }

            if (hasPadding)
            {
                using (var mipDataReader = new BinaryReader(new MemoryStream(mipData)))
                {
                    using (var adjustedMipStream = new MemoryStream())
                    {

                    }
                }
            }
            else
            {
                adjustedMipData = new byte[mipData.Length];
                Array.Copy(mipData, adjustedMipData, mipData.Length);
            }

            return adjustedMipData;
        }

        private static byte[] XEndian16(byte[] unswizzledBufferVar)
        {
            byte[] endianSwappedBufferVar = new byte[unswizzledBufferVar.Length];

            for (int i = 0; i < unswizzledBufferVar.Length; i += 2)
            {
                endianSwappedBufferVar[i] = unswizzledBufferVar[i + 1];
                endianSwappedBufferVar[i + 1] = unswizzledBufferVar[i];
            }

            return endianSwappedBufferVar;
        }

        private static int XGAddress2DTiledX(int Offset, int Width, int TexelPitch)
        {
            int AlignedWidth = (Width + 31) & ~31;

            int LogBpp = (TexelPitch >> 2) + ((TexelPitch >> 1) >> (TexelPitch >> 2));
            int OffsetB = Offset << LogBpp;
            int OffsetT = ((OffsetB & ~4095) >> 3) + ((OffsetB & 1792) >> 2) + (OffsetB & 63);
            int OffsetM = OffsetT >> (7 + LogBpp);

            int MacroX = ((OffsetM % (AlignedWidth >> 5)) << 2);
            int Tile = ((((OffsetT >> (5 + LogBpp)) & 2) + (OffsetB >> 6)) & 3);
            int Macro = (MacroX + Tile) << 3;
            int Micro = ((((OffsetT >> 1) & ~15) + (OffsetT & 15)) & ((TexelPitch << 3) - 1)) >> LogBpp;

            return Macro + Micro;
        }

        private static int XGAddress2DTiledY(int Offset, int Width, int TexelPitch)
        {
            int AlignedWidth = (Width + 31) & ~31;

            int LogBpp = (TexelPitch >> 2) + ((TexelPitch >> 1) >> (TexelPitch >> 2));
            int OffsetB = Offset << LogBpp;
            int OffsetT = ((OffsetB & ~4095) >> 3) + ((OffsetB & 1792) >> 2) + (OffsetB & 63);
            int OffsetM = OffsetT >> (7 + LogBpp);

            int MacroY = ((OffsetM / (AlignedWidth >> 5)) << 2);
            int Tile = ((OffsetT >> (6 + LogBpp)) & 1) + (((OffsetB & 2048) >> 10));
            int Macro = (MacroY + Tile) << 3;
            int Micro = ((((OffsetT & (((TexelPitch << 6) - 1) & ~31)) + ((OffsetT & 15) << 1)) >> (3 + LogBpp)) & ~1);

            return Macro + Micro + ((OffsetT & 16) >> 4);
        }

        private static byte[] ConvertToLinearTexture(byte[] data, GTEX gtex)
        {
            byte[] destData = new byte[data.Length];

            int blockSize = 0;
            int texelPitch = 0;

            switch (gtex.ImgFormat)
            {
                case ImageFormats.dxt1:
                    blockSize = 4;
                    texelPitch = 8;
                    break;
                case ImageFormats.dxt3:
                case ImageFormats.dxt5:
                    blockSize = 4;
                    texelPitch = 16;
                    break;
                case ImageFormats.r8g8b8a8:
                case ImageFormats.r8g8b8a8_2:
                    blockSize = 1;
                    texelPitch = 4;
                    break;
            }

            int blockWidth = MipWidth / blockSize;
            int blockHeight = MipHeight / blockSize;

            for (int j = 0; j < blockHeight; j++)
            {
                for (int i = 0; i < blockWidth; i++)
                {
                    int blockOffset = j * blockWidth + i;

                    int x = XGAddress2DTiledX(blockOffset, blockWidth, texelPitch);
                    int y = XGAddress2DTiledY(blockOffset, blockWidth, texelPitch);

                    int srcOffset = j * blockWidth * texelPitch + i * texelPitch;
                    int destOffset = y * blockWidth * texelPitch + x * texelPitch;

                    if ((destOffset + texelPitch) > destData.Length) continue;
                    Array.Copy(data, srcOffset, destData, destOffset, texelPitch);
                }
            }

            return destData;
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
                            var red = mipDataReader.ReadByte();
                            var alpha = mipDataReader.ReadByte();
                            var blue = mipDataReader.ReadByte();
                            var green = mipDataReader.ReadByte();

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