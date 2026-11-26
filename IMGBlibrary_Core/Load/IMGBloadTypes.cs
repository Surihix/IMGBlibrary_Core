using IMGBlibrary_Core.Support;
using IMGBlibrary_Core.Unpack;
using static IMGBlibrary_Core.Support.IMGBFlags;

namespace IMGBlibrary_Core.Load
{
    internal class IMGBLoadTypes
    {
        public static byte[] IMGBData = Array.Empty<byte>();

        #region Classic type
        public static byte[] LoadClassic(byte[] imgHeaderBlockData, GTEX gtex, bool showLoadLog)
        {
            var loadedData = Array.Empty<byte>();

            using (var imgbStream = new MemoryStream(IMGBData))
            {
                using (var gtexReader = new BinaryReader(new MemoryStream(imgHeaderBlockData)))
                {
                    using (var ddsStream = new MemoryStream())
                    {
                        using (var ddsWriter = new BinaryWriter(ddsStream))
                        {
                            ddsWriter.Write(DDSHelpers.GetDDSHeader(gtex));

                            gtexReader.BaseStream.Position = gtex.GTEXOffset + gtex.MipInfoTableOffset;

                            for (int i = 0; i < gtex.MipCount; i++)
                            {
                                var mipStart = gtexReader.ReadBytesUInt32(true);
                                var mipSize = gtexReader.ReadBytesUInt32(true);
                                var mipData = GetMipData((int)mipSize, imgbStream, mipStart, gtex);

                                ddsWriter.Write(mipData);
                            }

                            ddsStream.Position = 0;
                            loadedData = ddsStream.ToArray();
                        }
                    }
                }
            }

            SharedMethods.DisplayLogMessage($"Loaded {gtex.ImageName}.dds", showLoadLog);
            return loadedData;
        }
        #endregion


        #region Cubemap type
        public static byte[] LoadCubemap(byte[] imgHeaderBlockData, GTEX gtex, bool showLoadLog)
        {
            var loadedData = Array.Empty<byte>();

            using (var imgbStream = new MemoryStream(IMGBData))
            {
                using (var gtexReader = new BinaryReader(new MemoryStream(imgHeaderBlockData)))
                {
                    using (var ddsStream = new MemoryStream())
                    {
                        using (var ddsWriter = new BinaryWriter(ddsStream))
                        {
                            ddsWriter.Write(DDSHelpers.GetDDSHeader(gtex));

                            gtexReader.BaseStream.Position = gtex.GTEXOffset + gtex.MipInfoTableOffset;

                            for (int i = 0; i < gtex.MipCount * 6; i++)
                            {
                                var mipStart = gtexReader.ReadBytesUInt32(true);
                                var mipSize = gtexReader.ReadBytesUInt32(true);
                                var mipData = GetMipData((int)mipSize, imgbStream, mipStart, gtex);

                                ddsWriter.Write(mipData);
                            }

                            ddsStream.Position = 0;
                            loadedData = ddsStream.ToArray();
                        }
                    }
                }
            }

            SharedMethods.DisplayLogMessage($"Loaded {gtex.ImageName}_cbmap.dds", showLoadLog);
            return loadedData;
        }

        public static byte[] LoadCubemapX360(byte[] imgHeaderBlockData, GTEX gtex, bool showLoadLog)
        {
            var loadedData = Array.Empty<byte>();

            using (var imgbStream = new MemoryStream(IMGBData))
            {
                using (var gtexReader = new BinaryReader(new MemoryStream(imgHeaderBlockData)))
                {
                    using (var ddsStream = new MemoryStream())
                    {
                        using (var ddsWriter = new BinaryWriter(ddsStream))
                        {
                            gtexReader.BaseStream.Position = gtex.GTEXOffset + gtex.MipInfoTableOffset;
                            var mipStart = gtexReader.ReadBytesUInt32(true);
                            var mipSize = gtexReader.ReadBytesUInt32(true);

                            var cubemapBaseBlock = new byte[mipSize];
                            _ = imgbStream.Position = mipStart;
                            _ = imgbStream.Read(cubemapBaseBlock, 0, (int)mipSize);

                            // fix later
                            ddsWriter.Write(DDSHelpers.GetDDSHeader(gtex));
                            ddsWriter.Write(cubemapBaseBlock);

                            ddsStream.Position = 0;
                            loadedData = ddsStream.ToArray();
                        }
                    }
                }
            }

            SharedMethods.DisplayLogMessage($"Loaded {gtex.ImageName}_cbmap.dds", showLoadLog);
            return loadedData;
        }
        #endregion


        #region Volumemap type
        public static byte[] LoadVolumemap(byte[] imgHeaderBlockData, GTEX gtex, bool showLoadLog)
        {
            var loadedData = Array.Empty<byte>();

            using (var imgbStream = new MemoryStream(IMGBData))
            {
                using (var gtexReader = new BinaryReader(new MemoryStream(imgHeaderBlockData)))
                {
                    using (var ddsStream = new MemoryStream())
                    {
                        using (var ddsWriter = new BinaryWriter(ddsStream))
                        {
                            ddsWriter.Write(DDSHelpers.GetDDSHeader(gtex));

                            gtexReader.BaseStream.Position = gtex.GTEXOffset + gtex.MipInfoTableOffset;
                            var mipStart = gtexReader.ReadBytesUInt32(true);
                            var mipSize = gtexReader.ReadBytesUInt32(true);
                            var mipData = GetMipData((int)mipSize, imgbStream, mipStart, gtex);

                            ddsWriter.Write(mipData);

                            ddsStream.Position = 0;
                            loadedData = ddsStream.ToArray();
                        }
                    }
                }
            }

            SharedMethods.DisplayLogMessage($"Loaded {gtex.ImageName}_volume.dds", showLoadLog);
            return loadedData;
        }
        #endregion


        #region Shared
        private static byte[] GetMipData(int mipSize, MemoryStream imgbStream, uint mipStart, GTEX gtex)
        {
            var rawMipData = new byte[mipSize];

            _ = imgbStream.Position = mipStart;
            _ = imgbStream.Read(rawMipData, 0, mipSize);

            if (gtex.Platform == Platforms.ps3)
            {
                rawMipData = PS3Helpers.ProcessPS3ImageData(gtex, rawMipData);
            }

            if (gtex.Platform == Platforms.x360)
            {
                rawMipData = X360Helpers.ProcessX360ImageData(gtex, rawMipData);
            }

            return rawMipData;
        }
        #endregion
    }
}