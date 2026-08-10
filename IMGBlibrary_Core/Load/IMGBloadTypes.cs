using IMGBlibrary_Core.Support;

namespace IMGBlibrary_Core.Load
{
    internal class IMGBLoadTypes
    {
        public static IMGBFlags.Platforms Platform { get; set; }

        #region Classic type
        public static byte[] LoadClassic(byte[] imgHeaderBlockData, GTEX gtex, FileStream imgbStream)
        {
            var loadedData = Array.Empty<byte>();

            using (var gtexReader = new BinaryReader(new MemoryStream(imgHeaderBlockData)))
            {
                using (var ddsStream = new MemoryStream())
                {
                    using (var ddsWriter = new BinaryWriter(ddsStream))
                    {
                        gtexReader.BaseStream.Position = gtex.GTEXOffset + gtex.MipInfoTableOffset;

                        for (int i = 0; i < gtex.MipCount; i++)
                        {
                            var mipStart = gtexReader.ReadBytesUInt32(true);
                            var mipSize = gtexReader.ReadBytesUInt32(true);

                            CopyMipToDDS(gtex, mipStart, mipSize, imgbStream, ddsWriter);
                        }

                        ddsStream.Position = 0;
                        loadedData = ddsStream.ToArray();
                    }
                }
            }

            SharedMethods.DisplayLogMessage($"Loaded {gtex.ImageName}.dds", true);
            return loadedData;
        }
        #endregion


        #region Cubemap type
        public static byte[] LoadCubemap(byte[] imgHeaderBlockData, GTEX gtex, FileStream imgbStream)
        {
            var loadedData = Array.Empty<byte>();

            using (var gtexReader = new BinaryReader(new MemoryStream(imgHeaderBlockData)))
            {
                using (var ddsStream = new MemoryStream())
                {
                    using (var ddsWriter = new BinaryWriter(ddsStream))
                    {
                        gtexReader.BaseStream.Position = gtex.GTEXOffset + gtex.MipInfoTableOffset;

                        for (int i = 0; i < gtex.MipCount * 6; i++)
                        {
                            var mipStart = gtexReader.ReadBytesUInt32(true);
                            var mipSize = gtexReader.ReadBytesUInt32(true);

                            CopyMipToDDS(gtex, mipStart, mipSize, imgbStream, ddsWriter);
                        }

                        ddsStream.Position = 0;
                        loadedData = ddsStream.ToArray();
                    }
                }
            }

            SharedMethods.DisplayLogMessage($"Loaded {gtex.ImageName}_cbmap.dds", true);
            return loadedData;
        }
        #endregion


        #region Volumemap type
        public static byte[] LoadVolumemap(byte[] imgHeaderBlockData, GTEX gtex, FileStream imgbStream)
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

                        CopyMipToDDS(gtex, mipStart, mipSize, imgbStream, ddsWriter);

                        ddsStream.Position = 0;
                        loadedData = ddsStream.ToArray();
                    }
                }
            }

            SharedMethods.DisplayLogMessage($"Loaded {gtex.ImageName}_volume.dds", true);
            return loadedData;
        }
        #endregion


        #region Shared
        private static void CopyMipToDDS(GTEX gtex, uint mipStart, uint mipSize, FileStream imgbStream, BinaryWriter ddsWriter)
        {
            var doneCopying = false;

            if (Platform == IMGBFlags.Platforms.ps3)
            {
                imgbStream.Position = mipStart;
                PS3Helpers.ProcessPS3ImageData(ref doneCopying, gtex, mipSize, imgbStream, ddsWriter);
            }

            // If the condition matches a win32 image file or a pixel format
            // that does not need anything specific done, then copy the data
            // directly to the final dds file.
            if (!doneCopying)
            {
                var currentMip = new byte[(int)mipSize];

                imgbStream.Seek(mipStart, SeekOrigin.Begin);
                _ = imgbStream.Read(currentMip, 0, (int)mipSize);
                ddsWriter.Write(currentMip);
            }
        }
        #endregion
    }
}