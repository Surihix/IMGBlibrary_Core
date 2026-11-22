using IMGBlibrary_Core.Support;
using IMGBlibrary_Core.Unpack;

namespace IMGBlibrary_Core.Load
{
    internal class IMGBLoadTypes
    {
        #region Classic type
        public static byte[] LoadClassic(byte[] imgHeaderBlockData, GTEX gtex, FileStream imgbStream, bool showLoadLog)
        {
            var loadedData = Array.Empty<byte>();

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
                            var mipData = LoadMipData((int)mipSize, imgbStream, mipStart);

                            CopyMipToDDS(gtex, mipData, ddsWriter);
                        }

                        ddsStream.Position = 0;
                        loadedData = ddsStream.ToArray();
                    }
                }
            }

            SharedMethods.DisplayLogMessage($"Loaded {gtex.ImageName}.dds", showLoadLog);
            return loadedData;
        }

        public static byte[] LoadClassicX360(byte[] imgHeaderBlockData, GTEX gtex, FileStream imgbStream, bool showLoadLog)
        {
            var loadedData = Array.Empty<byte>();

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
                            var mipData = LoadMipData((int)mipSize, imgbStream, mipStart);

                            CopyMipToDDS(gtex, mipData, ddsWriter);
                        }

                        ddsStream.Position = 0;
                        loadedData = ddsStream.ToArray();
                    }
                }
            }

            SharedMethods.DisplayLogMessage($"Loaded {gtex.ImageName}.dds", showLoadLog);
            return loadedData;
        }
        #endregion


        #region Cubemap type
        public static byte[] LoadCubemap(byte[] imgHeaderBlockData, GTEX gtex, FileStream imgbStream, bool showLoadLog)
        {
            var loadedData = Array.Empty<byte>();

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

                            var mipData = LoadMipData((int)mipSize, imgbStream, mipStart);

                            CopyMipToDDS(gtex, mipData, ddsWriter);
                        }

                        ddsStream.Position = 0;
                        loadedData = ddsStream.ToArray();
                    }
                }
            }

            SharedMethods.DisplayLogMessage($"Loaded {gtex.ImageName}_cbmap.dds", showLoadLog);
            return loadedData;
        }

        public static byte[] LoadCubemapX360(byte[] imgHeaderBlockData, GTEX gtex, FileStream imgbStream, bool showLoadLog)
        {
            var loadedData = Array.Empty<byte>();

            using (var gtexReader = new BinaryReader(new MemoryStream(imgHeaderBlockData)))
            {
                using (var ddsStream = new MemoryStream())
                {
                    using (var ddsWriter = new BinaryWriter(ddsStream))
                    {
                        gtexReader.BaseStream.Position = gtex.GTEXOffset + gtex.MipInfoTableOffset;
                        var mipStart = gtexReader.ReadBytesUInt32(true);
                        var mipSize = gtexReader.ReadBytesUInt32(true);
                        var mipData = LoadMipData((int)mipSize, imgbStream, mipStart);

                        var hasMips = gtexReader.ReadUInt64() != 0;
                        _ = gtexReader.BaseStream.Position -= 8;

                        if (hasMips)
                        {
                            var mipCounter = X360Helpers.ComputeMipCount(gtex);
                            gtex.MipCount = mipCounter;
                        }

                        ddsWriter.Write(DDSHelpers.GetDDSHeader(gtex));
                        ddsWriter.Write(mipData);

                        ddsStream.Position = 0;
                        loadedData = ddsStream.ToArray();
                    }
                }
            }

            SharedMethods.DisplayLogMessage($"Loaded {gtex.ImageName}_cbmap.dds", showLoadLog);
            return loadedData;
        }
        #endregion


        #region Volumemap type
        public static byte[] LoadVolumemap(byte[] imgHeaderBlockData, GTEX gtex, FileStream imgbStream, bool showLoadLog)
        {
            var loadedData = Array.Empty<byte>();

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
                        var mipData = LoadMipData((int)mipSize, imgbStream, mipStart);

                        CopyMipToDDS(gtex, mipData, ddsWriter);

                        ddsStream.Position = 0;
                        loadedData = ddsStream.ToArray();
                    }
                }
            }

            SharedMethods.DisplayLogMessage($"Loaded {gtex.ImageName}_volume.dds", showLoadLog);
            return loadedData;
        }
        #endregion


        #region Shared
        private static byte[] LoadMipData(int mipSize, FileStream imgbStream, uint mipStart)
        {
            var mipData = new byte[mipSize];

            _ = imgbStream.Position = mipStart;
            _ = imgbStream.Read(mipData, 0, mipSize);

            return mipData;
        }

        private static void CopyMipToDDS(GTEX gtex, byte[] mipData, BinaryWriter ddsWriter)
        {
            var doneCopying = false;

            if (gtex.Platform == IMGBFlags.Platforms.ps3)
            {
                PS3Helpers.ProcessPS3ImageData(ref doneCopying, gtex, mipData, ddsWriter);
            }

            if (gtex.Platform == IMGBFlags.Platforms.x360)
            {
                X360Helpers.ProcessX360ImageData(ref doneCopying, gtex, mipData, ddsWriter);
            }

            // If Platform matches win32 or a image format
            // that doesn't need anything specific
            // to be done, then copy the data directly
            // to the final dds file.
            if (!doneCopying)
            {
                ddsWriter.Write(mipData);
            }
        }
        #endregion
    }
}