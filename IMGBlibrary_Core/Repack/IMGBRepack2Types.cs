﻿using IMGBlibrary_Core.Support;

namespace IMGBlibrary_Core.Repack
{
    internal class IMGBRepack2Types
    {
        #region Classic type
        public static void RepackClassicType(string imgHeaderBlockFile, string extractedIMGBdir, IMGBVariables imgbVars, FileStream imgbStream)
        {
            var currentDDSfile = Path.Combine(extractedIMGBdir, imgbVars.ImgHeaderBlockFileName + ".dds");

            if (!File.Exists(currentDDSfile))
            {
                Console.WriteLine("Missing image file. skipped to next file.");
                return;
            }

            using (var gtexStream = new FileStream(imgHeaderBlockFile, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var gtexReader = new BinaryReader(gtexStream))
                {
                    gtexReader.BaseStream.Position = imgbVars.GtexStartVal + 16;
                    var mipOffsetsStartPos = gtexReader.ReadBytesUInt32(true);

                    using (FileStream ddsStream = new FileStream(currentDDSfile, FileMode.Open, FileAccess.Read))
                    {
                        using (BinaryReader ddsReader = new BinaryReader(ddsStream))
                        {
                            SharedMethods.GetExtImgInfo(ddsReader, imgbVars);

                            using (var tempDDSstream = new MemoryStream())
                            {

                                ddsStream.Seek(128, SeekOrigin.Begin);
                                ddsStream.CopyTo(tempDDSstream);

                                using (var gtexWriter = new BinaryWriter(gtexStream))
                                {
                                    if (imgbVars.GtexImgMipCount < imgbVars.OutImgMipCount)
                                    {
                                        ExtraMipsOffsets(imgbVars, gtexStream, gtexWriter);
                                    }

                                    gtexWriter.BaseStream.Position = imgbVars.GtexStartVal + 6;
                                    gtexWriter.Write(imgbVars.OutImgFormatValue);

                                    gtexWriter.BaseStream.Position = imgbVars.GtexStartVal + 7;
                                    gtexWriter.Write((byte)imgbVars.OutImgMipCount);

                                    gtexWriter.BaseStream.Position = imgbVars.GtexStartVal + 10;
                                    gtexWriter.WriteBytesUInt16((ushort)imgbVars.OutImgWidth, true);

                                    gtexWriter.BaseStream.Position = imgbVars.GtexStartVal + 12;
                                    gtexWriter.WriteBytesUInt16((ushort)imgbVars.OutImgHeight, true);


                                    uint nextMipHeight = 0;
                                    uint nextMipWidth = 0;
                                    uint mipStart = 0;
                                    uint dataReadStart = 0;
                                    uint mipSize = 0;
                                    uint totalMipSize = 0;
                                    uint mipWritingStart = imgbVars.GtexStartVal + 24;

                                    for (int m = 0; m < imgbVars.OutImgMipCount; m++)
                                    {
                                        ComputeMipSizes(imgbVars, ref mipSize, ref mipStart, imgbStream);

                                        gtexWriter.BaseStream.Position = mipWritingStart;
                                        gtexWriter.WriteBytesUInt32(mipStart, true);

                                        gtexWriter.BaseStream.Position = mipWritingStart + 4;
                                        gtexWriter.WriteBytesUInt32(mipSize, true);

                                        tempDDSstream.ExCopyTo(imgbStream, dataReadStart, mipSize);

                                        NextMipHeightWidth(imgbVars, ref nextMipHeight, ref nextMipWidth);

                                        PadNullsForLastMips(imgbStream, mipSize);

                                        uint nextDataReadStart = mipSize + totalMipSize;
                                        dataReadStart = nextDataReadStart;
                                        totalMipSize = nextDataReadStart;

                                        gtexWriter.BaseStream.Position = mipWritingStart + 8;
                                        mipWritingStart = (uint)gtexWriter.BaseStream.Position;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Repacked " + currentDDSfile + " data to IMGB.");
        }
        #endregion


        #region Cubemap type
        public static void RepackCubemapType(string imgHeaderBlockFile, string extractedIMGBdir, IMGBVariables imgbVars, FileStream imgbStream)
        {
            var isMissingAnImg = SharedMethods.CheckImgFilesBatch(6, extractedIMGBdir, imgbVars.ImgHeaderBlockFileName, imgbVars);
            if (isMissingAnImg)
            {
                Console.WriteLine("Missing one or more cubemap type image files. skipped to next file.");
                return;
            }

            var isAllValidImg = CheckExtImgInfoType2(6, extractedIMGBdir, imgbVars.ImgHeaderBlockFileName, imgbVars);
            if (!isAllValidImg)
            {
                Console.WriteLine("One or more image file info does not match with the first image file's info. skipped to next file.");
                return;
            }

            using (var gtexStream = new FileStream(imgHeaderBlockFile, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var gtexWriter = new BinaryWriter(gtexStream))
                {

                    var cubeMapCount = 1;
                    uint mipWritingStart = 0;
                    var file1 = true;

                    for (int c = 0; c < 6; c++)
                    {
                        var currentDDSfile = Path.Combine(extractedIMGBdir, imgbVars.ImgHeaderBlockFileName + imgbVars.GtexImgType + cubeMapCount + ".dds");

                        using (var ddsStream = new FileStream(currentDDSfile, FileMode.Open, FileAccess.Read))
                        {
                            using (var ddsReader = new BinaryReader(ddsStream))
                            {
                                SharedMethods.GetExtImgInfo(ddsReader, imgbVars);

                                using (var tempDDSstream = new MemoryStream())
                                {

                                    ddsStream.Seek(128, SeekOrigin.Begin);
                                    ddsStream.CopyTo(tempDDSstream);

                                    if (file1)
                                    {
                                        if (imgbVars.GtexImgMipCount < imgbVars.OutImgMipCount)
                                        {
                                            ExtraMipsOffsets(imgbVars, gtexStream, gtexWriter);
                                        }

                                        gtexWriter.BaseStream.Position = imgbVars.GtexStartVal + 6;
                                        gtexWriter.Write(imgbVars.OutImgFormatValue);

                                        gtexWriter.BaseStream.Position = imgbVars.GtexStartVal + 7;
                                        gtexWriter.Write((byte)imgbVars.OutImgMipCount);

                                        gtexWriter.BaseStream.Position = imgbVars.GtexStartVal + 10;
                                        gtexWriter.WriteBytesUInt16((ushort)imgbVars.OutImgWidth, true);

                                        gtexWriter.BaseStream.Position = imgbVars.GtexStartVal + 12;
                                        gtexWriter.WriteBytesUInt16((ushort)imgbVars.OutImgHeight, true);
                                    }


                                    uint nextMipHeight = 0;
                                    uint nextMipWidth = 0;
                                    uint mipStart = 0;
                                    uint dataReadStart = 0;
                                    uint mipSize = 0;
                                    uint totalMipSize = 0;

                                    if (file1)
                                    {
                                        mipWritingStart = imgbVars.GtexStartVal + 24;
                                    }

                                    for (int m = 0; m < imgbVars.OutImgMipCount; m++)
                                    {
                                        ComputeMipSizes(imgbVars, ref mipSize, ref mipStart, imgbStream);

                                        gtexWriter.BaseStream.Position = mipWritingStart;
                                        gtexWriter.WriteBytesUInt32(mipStart, true);

                                        gtexWriter.BaseStream.Position = mipWritingStart + 4;
                                        gtexWriter.WriteBytesUInt32(mipSize, true);

                                        tempDDSstream.ExCopyTo(imgbStream, dataReadStart, mipSize);

                                        NextMipHeightWidth(imgbVars, ref nextMipHeight, ref nextMipWidth);

                                        PadNullsForLastMips(imgbStream, mipSize);

                                        uint nextDataReadStart = mipSize + totalMipSize;
                                        dataReadStart = nextDataReadStart;
                                        totalMipSize = nextDataReadStart;

                                        gtexWriter.BaseStream.Position = mipWritingStart + 8;
                                        mipWritingStart = (uint)gtexWriter.BaseStream.Position;
                                    }
                                }
                            }
                        }

                        file1 = false;

                        Console.WriteLine("Repacked " + currentDDSfile + " data to IMGB.");

                        cubeMapCount++;
                    }
                }
            }
        }
        #endregion


        #region Stack type
        public static void RepackStackType(string imgHeaderBlockFile, string extractedIMGBdir, IMGBVariables imgbVars, FileStream imgbStream)
        {
            var isMissingAnImg = SharedMethods.CheckImgFilesBatch(imgbVars.GtexImgDepth, extractedIMGBdir, imgbVars.ImgHeaderBlockFileName, imgbVars);
            if (isMissingAnImg)
            {
                Console.WriteLine("Missing one or more stack type image files. skipped to next file.");
                return;
            }

            var isAllValidImg = CheckExtImgInfoType2(imgbVars.GtexImgDepth, extractedIMGBdir, imgbVars.ImgHeaderBlockFileName, imgbVars);
            if (!isAllValidImg)
            {
                Console.WriteLine("One or more image file info does not match with the first image file's info. skipped to next file.");
                return;
            }

            using (var gtexStream = new FileStream(imgHeaderBlockFile, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var gtexWriter = new BinaryWriter(gtexStream))
                {

                    var stackCount = 1;
                    var file1 = true;

                    for (int s = 0; s < imgbVars.GtexImgDepth; s++)
                    {
                        var currentDDSfile = Path.Combine(extractedIMGBdir, imgbVars.ImgHeaderBlockFileName + imgbVars.GtexImgType + stackCount + ".dds");

                        using (var ddsStream = new FileStream(currentDDSfile, FileMode.Open, FileAccess.Read))
                        {
                            using (var ddsReader = new BinaryReader(ddsStream))
                            {
                                SharedMethods.GetExtImgInfo(ddsReader, imgbVars);

                                using (var tempDDSstream = new MemoryStream())
                                {
                                    ddsStream.Seek(128, SeekOrigin.Begin);
                                    ddsStream.CopyTo(tempDDSstream);

                                    var currentImgSize = (uint)tempDDSstream.Length;

                                    if (file1)
                                    {
                                        var stackStart = (uint)imgbStream.Length;
                                        var totalStackSize = currentImgSize * imgbVars.GtexImgDepth;

                                        gtexWriter.BaseStream.Position = imgbVars.GtexStartVal + 10;
                                        gtexWriter.WriteBytesUInt16((ushort)imgbVars.OutImgWidth, true);

                                        gtexWriter.BaseStream.Position = imgbVars.GtexStartVal + 12;
                                        gtexWriter.WriteBytesUInt16((ushort)imgbVars.OutImgHeight, true);

                                        gtexWriter.BaseStream.Position = imgbVars.GtexStartVal + 24;
                                        gtexWriter.WriteBytesUInt32(stackStart, true);

                                        gtexWriter.BaseStream.Position = imgbVars.GtexStartVal + 28;
                                        gtexWriter.WriteBytesUInt32(totalStackSize, true);
                                    }

                                    tempDDSstream.ExCopyTo(imgbStream, 0, currentImgSize);
                                }
                            }
                        }

                        file1 = false;

                        Console.WriteLine("Repacked " + currentDDSfile + " data to IMGB.");

                        stackCount++;
                    }
                }
            }
        }
        #endregion


        #region Common methods
        private static void ExtraMipsOffsets(IMGBVariables imgbVars, FileStream gtexStream, BinaryWriter gtexWriter)
        {
            var extraMipsVar = imgbVars.OutImgMipCount - imgbVars.GtexImgMipCount;

            for (int exm = 0; exm < extraMipsVar; exm++)
            {
                long addBytesAt = gtexStream.Length;
                gtexWriter.BaseStream.Position = addBytesAt;
                for (int b = 0; b < 8; b++)
                {
                    byte[] mipOffset = { 00 };
                    gtexWriter.Write(mipOffset);
                }
            }

            var updSize = (uint)gtexStream.Length;

            gtexWriter.BaseStream.Position = 16;
            gtexWriter.WriteBytesUInt32(updSize, false);

            Console.WriteLine("Detected more mips in image file. added extra offsets.");
        }


        private static void ComputeMipSizes(IMGBVariables imgbVars, ref uint mipSizeVar, ref uint mipStartVar, FileStream imgbStream)
        {
            switch (imgbVars.OutImgFormatValue)
            {
                case 3:    // R8G8B8A8
                case 4:    // R8G8B8A8 with Mips
                    mipSizeVar = imgbVars.OutImgHeight * imgbVars.OutImgWidth * 4;
                    mipStartVar = (uint)imgbStream.Length;
                    break;

                case 24:   // DXT1
                    imgbVars.OutImgHeight += ((4 - imgbVars.OutImgHeight % 4) % 4);
                    imgbVars.OutImgWidth += ((4 - imgbVars.OutImgWidth % 4) % 4);
                    mipSizeVar = imgbVars.OutImgHeight * imgbVars.OutImgWidth * 4 / 8;
                    mipStartVar = (uint)imgbStream.Length;
                    break;

                case 25:   // DXT 3 or
                case 26:   // DXT 5
                    imgbVars.OutImgHeight += ((4 - imgbVars.OutImgHeight % 4) % 4);
                    imgbVars.OutImgWidth += ((4 - imgbVars.OutImgWidth % 4) % 4);
                    mipSizeVar = imgbVars.OutImgHeight * imgbVars.OutImgWidth * 4 / 4;
                    mipStartVar = (uint)imgbStream.Length;
                    break;
            }
        }


        private static void NextMipHeightWidth(IMGBVariables imgbVars, ref uint nextMipHeight, ref uint nextMipWidth)
        {
            if (imgbVars.OutImgFormatValue != 3)
            {
                nextMipHeight = imgbVars.OutImgHeight / 2;
                imgbVars.OutImgHeight = nextMipHeight;
                if (imgbVars.OutImgHeight < 4)
                {
                    imgbVars.OutImgHeight = 4;
                }

                nextMipWidth = imgbVars.OutImgWidth / 2;
                imgbVars.OutImgWidth = nextMipWidth;
                if (imgbVars.OutImgWidth < 4)
                {
                    nextMipWidth = 4;
                }
            }

            if (imgbVars.OutImgFormatValue == 3)
            {
                if (imgbVars.OutImgHeight == 1)
                {
                    nextMipHeight = 1;
                }
                else
                {
                    nextMipHeight = imgbVars.OutImgHeight / 2;
                }
                imgbVars.OutImgHeight = nextMipHeight;

                if (imgbVars.OutImgWidth == 1)
                {
                    nextMipWidth = 1;
                }
                else
                {
                    nextMipWidth = imgbVars.OutImgWidth / 2;
                }
                imgbVars.OutImgWidth = nextMipWidth;
            }
        }


        private static bool CheckExtImgInfoType2(int fileAmount, string extractImgbDir, string imgHeaderBlockFileName, IMGBVariables imgbVars)
        {
            var isAllValidImg = true;
            var imgFileCount = 1;
            var file1 = true;
            uint firstImgHeight = 0;
            uint firstImgWidth = 0;
            uint firstImgMipCount = 0;
            byte firstImgFormatValue = 0;
            for (int i = 0; i < fileAmount; i++)
            {
                var fileToCheck = Path.Combine(extractImgbDir, imgHeaderBlockFileName + imgbVars.GtexImgType + imgFileCount + ".dds");

                using (var ddsFileToCheck = new FileStream(fileToCheck, FileMode.Open, FileAccess.Read))
                {
                    using (var ddsFileReader = new BinaryReader(ddsFileToCheck))
                    {
                        SharedMethods.GetExtImgInfo(ddsFileReader, imgbVars);

                        if (imgbVars.OutImgFormatValue == 0)
                        {
                            Console.WriteLine("One or more DDS files are in unsupported pixel format. skip to next file.");
                            isAllValidImg = false;
                        }

                        if (file1)
                        {
                            firstImgHeight = imgbVars.OutImgHeight;
                            firstImgWidth = imgbVars.OutImgWidth;
                            firstImgMipCount = imgbVars.OutImgMipCount;
                            firstImgFormatValue = imgbVars.OutImgFormatValue;
                        }

                        if (!file1)
                        {
                            if (imgbVars.OutImgHeight != firstImgHeight)
                            {
                                isAllValidImg = false;
                            }
                            if (imgbVars.OutImgWidth != firstImgWidth)
                            {
                                isAllValidImg = false;
                            }
                            if (imgbVars.OutImgMipCount != firstImgMipCount)
                            {
                                isAllValidImg = false;
                            }
                            if (imgbVars.OutImgFormatValue != firstImgFormatValue)
                            {
                                isAllValidImg = false;
                            }

                        }

                        file1 = false;
                    }
                }

                imgFileCount++;
            }

            return isAllValidImg;
        }


        private static void PadNullsForLastMips(FileStream imgbStream, uint mipSize)
        {
            if (mipSize < 16)
            {
                var padAmount = 16 - mipSize;
                for (int b = 0; b < padAmount; b++)
                {
                    imgbStream.WriteByte(0);
                }
            }
        }
        #endregion
    }
}