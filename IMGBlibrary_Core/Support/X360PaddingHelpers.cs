using static IMGBlibrary_Core.Support.IMGBFlags;

namespace IMGBlibrary_Core.Support
{
    internal class X360PaddingHelpers
    {
        public static byte[] RemovePadding(GTEX gtex, byte[] mipData)
        {
            var nonPaddedMipData = Array.Empty<byte>();

            // remove later
            if (X360Helpers.MipHeight < 128 && X360Helpers.MipWidth < 128)
            {
                return nonPaddedMipData;
            }

            var bpp = 0;
            var blockReadSize = 0;

            var paddedY = X360Helpers.MipHeight % 128;
            var paddedX = X360Helpers.MipWidth % 128;

            var isPaddedY = paddedY != 0;
            var isPaddedX = paddedX != 0;

            int paddedWidth = 0;
            if (isPaddedX)
            {
                paddedWidth = (128 - paddedX);
            }

            var padType1 = isPaddedX && isPaddedY && X360Helpers.MipWidth > 128 && X360Helpers.MipHeight < 128;
            var padType2 = isPaddedX && isPaddedY && X360Helpers.MipHeight > 128 && X360Helpers.MipWidth > 128;
            var padType3 = isPaddedX && isPaddedY && X360Helpers.MipHeight < 128 && X360Helpers.MipWidth < 128;
            var padType4 = !isPaddedX && isPaddedY;

            switch (gtex.ImgFormat)
            {
                case ImageFormats.r8g8b8a8_type3:
                case ImageFormats.r8g8b8a8:
                    if (padType1)
                    {
                        nonPaddedMipData = PaddingType1R8G8B8A8(mipData, paddedWidth);
                    }

                    if (padType2)
                    {
                        nonPaddedMipData = PaddingType2R8G8B8A8(mipData, paddedWidth);  // not available
                    }
                    break;

                case ImageFormats.dxt1:
                    bpp = 2;
                    blockReadSize = 8;
                    break;

                case ImageFormats.dxt3:
                case ImageFormats.dxt5:
                    bpp = 4;
                    blockReadSize = 16;
                    break;
            }

            var isBCFormat = gtex.ImgFormat == ImageFormats.dxt1 || gtex.ImgFormat == ImageFormats.dxt3 || gtex.ImgFormat == ImageFormats.dxt5;

            if (isBCFormat)
            {
                if (padType1)
                {
                    nonPaddedMipData = PaddingType1BC(mipData, blockReadSize, bpp, paddedWidth); //not available
                }

                if (padType2)
                {
                    nonPaddedMipData = PaddingType2BC(mipData, blockReadSize, bpp, paddedWidth);
                }
            }

            return nonPaddedMipData;
        }

        private static byte[] PaddingType1R8G8B8A8(byte[] mipData, int paddedWidth)
        {
            var nonPaddedMipData = Array.Empty<byte>();

            using (var mipDataReader = new BinaryReader(new MemoryStream(mipData)))
            {
                using (var nonPaddedDataStream = new MemoryStream())
                {
                    for (int y = 0; y < X360Helpers.MipHeight; y++)
                    {
                        for (int x = 0; x < X360Helpers.MipWidth; x++)
                        {
                            var dataToWrite = mipDataReader.ReadBytes(4);
                            nonPaddedDataStream.Write(dataToWrite, 0, 4);
                        }

                        _ = mipDataReader.ReadBytes(4 * paddedWidth);
                    }

                    nonPaddedDataStream.Seek(0, SeekOrigin.Begin);
                    nonPaddedMipData = nonPaddedDataStream.ToArray();
                }
            }

            return nonPaddedMipData;
        }

        private static byte[] PaddingType2R8G8B8A8(byte[] mipData, int paddedWidth)
        {
            var nonPaddedMipData = Array.Empty<byte>();

            return nonPaddedMipData;
        }

        private static byte[] PaddingType1BC(byte[] mipData, int blockReadSize, int bpp, int paddedWidth)
        {
            var nonPaddedMipData = Array.Empty<byte>();

            return nonPaddedMipData;
        }

        private static byte[] PaddingType2BC(byte[] mipData, int blockReadSize, int bpp, int paddedWidth)
        {
            var nonPaddedMipData = Array.Empty<byte>();

            using (var mipDataReader = new BinaryReader(new MemoryStream(mipData)))
            {
                using (var nonPaddedDataStream = new MemoryStream())
                {
                    for (int y = 0; y < X360Helpers.MipHeight; y += bpp)
                    {
                        for (int x = 0; x < X360Helpers.MipWidth; x += bpp)
                        {
                            var dataToWrite = mipDataReader.ReadBytes(blockReadSize);

                            var partialBlock = y + bpp > X360Helpers.MipHeight || x + bpp > X360Helpers.MipWidth;

                            nonPaddedDataStream.Write(dataToWrite, 0, blockReadSize);

                            for (int i = 0; i < bpp; i++)
                            {
                                if (partialBlock && y + i >= X360Helpers.MipHeight)
                                {
                                    break;
                                }

                                for (int j = 0; j < bpp; j++)
                                {
                                    if (partialBlock && x + j >= X360Helpers.MipWidth)
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        for (int x = 0; x < paddedWidth; x += bpp)
                        {
                            _ = mipDataReader.ReadBytes(blockReadSize);
                        }
                    }

                    nonPaddedDataStream.Seek(0, SeekOrigin.Begin);
                    nonPaddedMipData = nonPaddedDataStream.ToArray();
                }
            }

            return nonPaddedMipData;
        }
    }
}