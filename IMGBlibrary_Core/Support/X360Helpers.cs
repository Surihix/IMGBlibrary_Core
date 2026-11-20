using static IMGBlibrary_Core.Support.IMGBFlags;

namespace IMGBlibrary_Core.Support
{
    internal class X360Helpers
    {
        public static int MipWidth = 0;
        public static int MipHeight = 0;

        public static void ProcessX360ImageData(ref bool doneCopying, GTEX gtex, uint mipSize, FileStream imgbStream, BinaryWriter ddsWriter)
        {
            // Adjust mip data without copying
            // the padding data
            var rawMipData = new byte[mipSize];
            imgbStream.Read(rawMipData, 0, rawMipData.Length);

            var adjustedMipData = AdjustMipData(gtex, rawMipData);

            // Remove this after padding is sorted :(
            if (adjustedMipData.Length == 0)
            {
                return;
            }

            // If the conditions match a swizzled x360 image,
            // then unswizzle the image data, endian swap data,
            // color correct the data (if format is 'r8g8b8a8_mips' or 'r8g8b8a8_no_mips')
            // and copy the data to the final dds file.
            var isSwizzled = false;
            if (gtex.ImgType == ImageTypes.classic_console || gtex.ImgType == ImageTypes.cubemap_console)
            {
                isSwizzled = true;
            }
            if (!doneCopying && isSwizzled)
            {
                var unSwizzledArray = ConvertToLinearTexture(adjustedMipData, gtex);
                unSwizzledArray = XEndian16(unSwizzledArray);

                if (gtex.ImgFormatFlag == 3 || gtex.ImgFormatFlag == 4)
                {
                    var correctedColorArray = ColorAsBGRA(unSwizzledArray);
                    ddsWriter.Write(correctedColorArray);
                }
                else
                {
                    ddsWriter.Write(unSwizzledArray);
                }

                doneCopying = true;
            }

            // If the conditions match pixel format 'r8g8b8a8_mips'
            // or 'r8g8b8a8_no_mips', then endian swap the data,
            // color correct the data and copy the data to
            // the final dds file.
            if (!doneCopying && gtex.ImgFormat == ImageFormats.r8g8b8a8_mips || !doneCopying && gtex.ImgFormat == ImageFormats.r8g8b8a8_no_mips)
            {
                adjustedMipData = XEndian16(adjustedMipData);

                var correctedColorArray = ColorAsBGRA(adjustedMipData);
                ddsWriter.Write(correctedColorArray);

                doneCopying = true;
            }

            // If the condition match pixel format 'dxt1', 'dxt3', or 'dxt5', 
            // "without the swizzle type flag", then endian swap the data,
            // and copy the data to the final dds file.
            if (!doneCopying)
            {
                adjustedMipData = XEndian16(adjustedMipData);
                ddsWriter.Write(adjustedMipData);

                doneCopying = true;
            }

            SetNextMipHeightWidth(gtex);
        }

        private static byte[] AdjustMipData(GTEX gtex, byte[] mipData)
        {
            var adjustedMipData = Array.Empty<byte>();

            var factorPadding = false;

            if (MipWidth % 128 != 0)
            {
                factorPadding = true;
            }

            if (factorPadding)
            {
                using (var mipDataReader = new BinaryReader(new MemoryStream(mipData)))
                {
                    using (var adjustedMipStream = new MemoryStream())
                    {
                        adjustedMipStream.Position = 0;
                        adjustedMipData = adjustedMipStream.ToArray();
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

        private static void SetNextMipHeightWidth(GTEX gtex)
        {
            if (gtex.ImgFormat != ImageFormats.r8g8b8a8_mips)
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

            if (gtex.ImgFormat == ImageFormats.r8g8b8a8_mips)
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
                case ImageFormats.r8g8b8a8_mips:
                case ImageFormats.r8g8b8a8_no_mips:
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

        private static byte[] ColorAsBGRA(byte[] mipData)
        {
            var correctedColors = new byte[mipData.Length];

            using (MemoryStream unSwizzledStream = new MemoryStream())
            {
                unSwizzledStream.Write(mipData, 0, mipData.Length);
                unSwizzledStream.Seek(0, SeekOrigin.Begin);

                using (BinaryReader unSwizzledReader = new BinaryReader(unSwizzledStream))
                {
                    using (MemoryStream adjustedColorStream = new MemoryStream())
                    {
                        adjustedColorStream.Write(mipData, 0, mipData.Length);
                        adjustedColorStream.Seek(0, SeekOrigin.Begin);

                        using (BinaryWriter adjustedColorWriter = new BinaryWriter(adjustedColorStream))
                        {
                            var readPos = 0;
                            var writePos = 0;

                            for (int p = 0; p < mipData.Length; p++)
                            {
                                unSwizzledReader.BaseStream.Position = readPos;
                                var red = unSwizzledReader.ReadByte();
                                var alpha = unSwizzledReader.ReadByte();
                                var blue = unSwizzledReader.ReadByte();
                                var green = unSwizzledReader.ReadByte();

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
                            adjustedColorStream.Read(correctedColors, 0, correctedColors.Length);
                        }
                    }
                }
            }

            return correctedColors;
        }
    }
}