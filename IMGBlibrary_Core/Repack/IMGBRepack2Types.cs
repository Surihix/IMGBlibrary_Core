using IMGBlibrary_Core.Support;

namespace IMGBlibrary_Core.Repack
{
    internal class IMGBRepack2Types
    {
        #region Shared
        public static void ProcessImage(string extractedIMGBdir, string imgHeaderBlockFile, GTEX gtex, FileStream imgbStream)
        {
            var currentDDSfile = string.Empty;

            if (!LocateImageFile(extractedIMGBdir, gtex.ImageName, ref currentDDSfile))
            {
                SharedMethods.DisplayLogMessage("Unable to locate image file. skipped image repacking.", true);
                return;
            }

            SharedMethods.DisplayLogMessage($"Image found {currentDDSfile}", true);

            var imageInfo = GetImageInfo(currentDDSfile, gtex);

            if (!imageInfo.IsValid)
            {
                return;
            }

            gtex.Width = imageInfo.Width;
            gtex.Height = imageInfo.Height;
            gtex.MipCount = imageInfo.MipCount;
            gtex.ImgFormatFlag = imageInfo.PixelFormatFlag;

            switch (imageInfo.ImgTypeFlag)
            {
                case 0:
                    gtex.ImgTypeFlag = 0;
                    gtex.Depth = 1;
                    RepackClassicType(imgHeaderBlockFile, currentDDSfile, gtex, imgbStream);
                    break;

                case 1:
                    gtex.ImgTypeFlag = 1;
                    gtex.Depth = 1;
                    RepackCubemapType(imgHeaderBlockFile, currentDDSfile, gtex, imgbStream);
                    break;

                case 2:
                    gtex.ImgTypeFlag = 2;
                    gtex.Depth = imageInfo.Depth;
                    RepackVolumemap(imgHeaderBlockFile, currentDDSfile, gtex, imgbStream);
                    break;
            }
        }

        private static bool LocateImageFile(string extractedIMGBdir, string imageName, ref string currentDDSfile)
        {
            if (File.Exists(Path.Combine(extractedIMGBdir, imageName + ".dds")))
            {
                currentDDSfile = Path.Combine(extractedIMGBdir, imageName + ".dds");
                return true;
            }

            if (File.Exists(Path.Combine(extractedIMGBdir, imageName + "_cbmap.dds")))
            {
                currentDDSfile = Path.Combine(extractedIMGBdir, imageName + "_cbmap.dds");
                return true;
            }

            if (File.Exists(Path.Combine(extractedIMGBdir, imageName + "_volume.dds")))
            {
                currentDDSfile = Path.Combine(extractedIMGBdir, imageName + "_volume.dds");
                return true;
            }

            return false;
        }

        private static ImageInfo GetImageInfo(string currentDDSfile, GTEX gtex)
        {
            var imageInfo = new ImageInfo();

            if (new FileInfo(currentDDSfile).Length < 128)
            {
                SharedMethods.DisplayLogMessage("DDS size is invalid. skipped repacking image.", true);
                return imageInfo;
            }

            if (currentDDSfile.EndsWith("_cbmap.dds"))
            {
                imageInfo.ImgTypeFlag = 1;
            }
            else if (currentDDSfile.EndsWith("_volume.dds"))
            {
                imageInfo.ImgTypeFlag = 2;
            }
            else
            {
                imageInfo.ImgTypeFlag = 0;
            }

            using (var ddsReader = new BinaryReader(File.Open(currentDDSfile, FileMode.Open, FileAccess.Read)))
            {
                _ = ddsReader.BaseStream.Position = 12;
                imageInfo.Height = (ushort)ddsReader.ReadUInt32();
                imageInfo.Width = (ushort)ddsReader.ReadUInt32();

                _ = ddsReader.BaseStream.Position += 4;
                imageInfo.Depth = (byte)ddsReader.ReadUInt32();
                imageInfo.MipCount = (byte)ddsReader.ReadUInt32();

                _ = ddsReader.BaseStream.Position += 52;
                var ddsFourCC = ddsReader.ReadBytesString(4, false);

                switch (ddsFourCC)
                {
                    case "":
                        if (ddsReader.ReadByte() == 8)  // R8
                        {
                            imageInfo.PixelFormatFlag = 5;
                        }
                        else if (ddsReader.ReadByte() == 32)    // R8G8B8A8 and R8G8B8A8_Type3
                        {
                            if (imageInfo.PixelFormatFlag == 3 || imageInfo.PixelFormatFlag == 4)
                            {
                                imageInfo.PixelFormatFlag = gtex.ImgFormatFlag;
                            }
                            else
                            {
                                imageInfo.PixelFormatFlag = 4;
                            }
                        }
                        else
                        {
                            SharedMethods.DisplayLogMessage("Pixel format is not supported. skipped repacking image", true);
                            imageInfo.IsValid = false;
                            return imageInfo;
                        }
                        break;

                    case "DXT1":
                        imageInfo.PixelFormatFlag = 24;
                        break;

                    case "DXT3":
                        imageInfo.PixelFormatFlag = 25;
                        break;

                    case "DXT5":
                        imageInfo.PixelFormatFlag = 26;
                        break;

                    default:
                        SharedMethods.DisplayLogMessage("Pixel format is not supported. skipped repacking image", true);
                        imageInfo.IsValid = false;
                        return imageInfo;
                }

                imageInfo.IsValid = true;
            }

            return imageInfo;
        }

        private static void UpdateHeader(string imgHeaderBlockFile, GTEX gtex)
        {
            var headerBlockFileData = File.ReadAllBytes(imgHeaderBlockFile);

            var mipInfoTableSize = gtex.ImgTypeFlag == 1 ? (gtex.MipCount * 8) * 6 : gtex.MipCount * 8;
            var mipInfoTableData = new byte[mipInfoTableSize];

            File.Delete(imgHeaderBlockFile);

            using (var newHeaderBlockStream = new FileStream(imgHeaderBlockFile, FileMode.Append, FileAccess.Write))
            {
                newHeaderBlockStream.Write(headerBlockFileData, 0, (int)gtex.GTEXOffset + 24);
                newHeaderBlockStream.Write(mipInfoTableData, 0, mipInfoTableSize);
                newHeaderBlockStream.PadStream(16);
            }

            var headerBlockFileSize = (uint)new FileInfo(imgHeaderBlockFile).Length;

            using (var headerBlockUpdater = new BinaryWriter(File.Open(imgHeaderBlockFile, FileMode.Open, FileAccess.Write)))
            {
                _ = headerBlockUpdater.BaseStream.Position = 16;
                headerBlockUpdater.Write(headerBlockFileSize);

                _ = headerBlockUpdater.BaseStream.Position = gtex.GTEXOffset + 6;
                headerBlockUpdater.Write(gtex.ImgFormatFlag);

                _ = headerBlockUpdater.BaseStream.Position = gtex.GTEXOffset + 7;
                headerBlockUpdater.Write(gtex.MipCount);

                _ = headerBlockUpdater.BaseStream.Position = gtex.GTEXOffset + 9;
                headerBlockUpdater.Write(gtex.ImgTypeFlag);
                headerBlockUpdater.WriteBytesUInt16(gtex.Width, true);
                headerBlockUpdater.WriteBytesUInt16(gtex.Height, true);
                headerBlockUpdater.WriteBytesUInt16(gtex.Depth, true);
            }
        }

        private static uint ComputeMipSize(GTEX gtex)
        {
            uint mipSize = 0;

            switch (gtex.ImgFormatFlag)
            {
                case 3:
                case 4:
                    mipSize = (uint)gtex.Height * gtex.Width * 4;
                    break;

                case 5:
                    mipSize = (uint)gtex.Height * gtex.Width;
                    break;

                case 24:
                    gtex.Height += (ushort)((4 - gtex.Height % 4) % 4);
                    gtex.Width += (ushort)((4 - gtex.Width % 4) % 4);
                    mipSize = (uint)gtex.Height * gtex.Width * 4 / 8;
                    break;

                case 25:
                case 26:
                    gtex.Height += (ushort)((4 - gtex.Height % 4) % 4);
                    gtex.Width += (ushort)((4 - gtex.Width % 4) % 4);
                    mipSize = (uint)gtex.Height * gtex.Width * 4 / 4;
                    break;
            }

            return mipSize;
        }

        private static void NextMipHeightWidth(GTEX gtex, ref ushort nextMipHeight, ref ushort nextMipWidth)
        {
            if (gtex.ImgFormatFlag == 24 || gtex.ImgFormatFlag == 25 || gtex.ImgFormatFlag == 26)
            {
                nextMipHeight = (ushort)(gtex.Height / 2);
                gtex.Height = nextMipHeight;

                if (gtex.Height < 4)
                {
                    gtex.Height = 4;
                }

                nextMipWidth = (ushort)(gtex.Width / 2);
                gtex.Width = nextMipWidth;

                if (gtex.Width < 4)
                {
                    nextMipWidth = 4;
                }
            }

            if (gtex.ImgFormatFlag == 3 || gtex.ImgFormatFlag == 4 || gtex.ImgFormatFlag == 5)
            {
                if (gtex.Height == 1)
                {
                    nextMipHeight = 1;
                }
                else
                {
                    nextMipHeight = (ushort)(gtex.Height / 2);
                }

                gtex.Height = nextMipHeight;

                if (gtex.Width == 1)
                {
                    nextMipWidth = 1;
                }
                else
                {
                    nextMipWidth = (ushort)(gtex.Width / 2);
                }

                gtex.Width = nextMipWidth;
            }
        }

        private static void PadNullsForLastMips(FileStream imgbStream, uint mipSize)
        {
            if (mipSize < 16)
            {
                var padBuffer = new byte[16 - mipSize];
                imgbStream.Write(padBuffer, 0, padBuffer.Length);
            }
        }
        #endregion


        #region Classic type
        private static void RepackClassicType(string imgHeaderBlockFile, string currentDDSfile, GTEX gtex, FileStream imgbStream)
        {
            UpdateHeader(imgHeaderBlockFile, gtex);

            using (BinaryReader ddsReader = new BinaryReader(File.Open(currentDDSfile, FileMode.Open, FileAccess.Read)))
            {
                using (var tempDDSstream = new MemoryStream())
                {
                    ddsReader.BaseStream.Seek(128, SeekOrigin.Begin);
                    ddsReader.BaseStream.CopyTo(tempDDSstream);

                    using (var headerBlockWriter = new BinaryWriter(File.Open(imgHeaderBlockFile, FileMode.Open, FileAccess.Write)))
                    {
                        ushort nextMipHeight = 0;
                        ushort nextMipWidth = 0;
                        long dataReadStart = 0;

                        headerBlockWriter.BaseStream.Position = gtex.GTEXOffset + 24;

                        for (int i = 0; i < gtex.MipCount; i++)
                        {
                            var mipSize = ComputeMipSize(gtex);
                            var mipStart = (uint)imgbStream.Length;

                            headerBlockWriter.WriteBytesUInt32(mipStart, true);
                            headerBlockWriter.WriteBytesUInt32(mipSize, true);

                            tempDDSstream.Seek(dataReadStart, SeekOrigin.Begin);
                            tempDDSstream.CopyStreamTo(imgbStream, mipSize, false);

                            NextMipHeightWidth(gtex, ref nextMipHeight, ref nextMipWidth);

                            PadNullsForLastMips(imgbStream, mipSize);

                            dataReadStart = tempDDSstream.Position;
                        }
                    }
                }
            }

            SharedMethods.DisplayLogMessage("Repacked " + currentDDSfile, true);
        }
        #endregion


        #region Cubemap type
        private static void RepackCubemapType(string imgHeaderBlockFile, string currentDDSfile, GTEX gtex, FileStream imgbStream)
        {
            UpdateHeader(imgHeaderBlockFile, gtex);

            using (BinaryReader ddsReader = new BinaryReader(File.Open(currentDDSfile, FileMode.Open, FileAccess.Read)))
            {
                using (var tempDDSstream = new MemoryStream())
                {
                    ddsReader.BaseStream.Seek(128, SeekOrigin.Begin);
                    ddsReader.BaseStream.CopyTo(tempDDSstream);

                    using (var headerBlockWriter = new BinaryWriter(File.Open(imgHeaderBlockFile, FileMode.Open, FileAccess.Write)))
                    {
                        ushort nextMipHeight = 0;
                        ushort nextMipWidth = 0;
                        var mipSizeTable = new uint[gtex.MipCount];

                        headerBlockWriter.BaseStream.Position = gtex.GTEXOffset + 24;

                        for (int i = 0; i < gtex.MipCount; i++)
                        {
                            var mipSize = ComputeMipSize(gtex);
                            mipSizeTable[i] = mipSize;
                            NextMipHeightWidth(gtex, ref nextMipHeight, ref nextMipWidth);
                        }

                        long dataReadStart = 0;

                        for (int i = 0; i < 6; i++)
                        {
                            for (int j = 0; j < gtex.MipCount; j++)
                            {
                                var mipStart = (uint)imgbStream.Length;
                                var mipSize = mipSizeTable[j];

                                headerBlockWriter.WriteBytesUInt32(mipStart, true);
                                headerBlockWriter.WriteBytesUInt32(mipSize, true);

                                tempDDSstream.Seek(dataReadStart, SeekOrigin.Begin);
                                tempDDSstream.CopyStreamTo(imgbStream, mipSize, false);

                                PadNullsForLastMips(imgbStream, mipSize);

                                dataReadStart = tempDDSstream.Position;
                            }
                        }
                    }
                }
            }

            SharedMethods.DisplayLogMessage("Repacked " + currentDDSfile, true);
        }
        #endregion


        #region Volumemap type
        private static void RepackVolumemap(string imgHeaderBlockFile, string currentDDSfile, GTEX gtex, FileStream imgbStream)
        {
            if (gtex.MipCount > 1)
            {
                SharedMethods.DisplayLogMessage("Detected more than one mip. mip 0 alone would be repacked", true);
            }

            UpdateHeader(imgHeaderBlockFile, gtex);

            using (BinaryReader ddsReader = new BinaryReader(File.Open(currentDDSfile, FileMode.Open, FileAccess.Read)))
            {
                using (var tempDDSstream = new MemoryStream())
                {
                    ddsReader.BaseStream.Seek(128, SeekOrigin.Begin);
                    ddsReader.BaseStream.CopyTo(tempDDSstream);

                    using (var headerBlockWriter = new BinaryWriter(File.Open(imgHeaderBlockFile, FileMode.Open, FileAccess.Write)))
                    {
                        headerBlockWriter.BaseStream.Position = gtex.GTEXOffset + 24;
                        var mipStart = (uint)imgbStream.Length;
                        var mipSize = (uint)tempDDSstream.Length;

                        headerBlockWriter.WriteBytesUInt32(mipStart, true);
                        headerBlockWriter.WriteBytesUInt32(mipSize, true);

                        tempDDSstream.Seek(0, SeekOrigin.Begin);
                        tempDDSstream.CopyStreamTo(imgbStream, mipSize, false);
                    }
                }
            }

            SharedMethods.DisplayLogMessage("Repacked " + currentDDSfile, true);
        }
        #endregion
    }
}