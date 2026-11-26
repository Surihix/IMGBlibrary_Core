using IMGBlibrary_Core.Load;
using IMGBlibrary_Core.Support;
using static IMGBlibrary_Core.Support.IMGBFlags;

namespace IMGBlibrary_Core.Unpack
{
    /// <summary>
    /// Provides a method for unpacking image files from the FINAL FANTASY XIII trilogy.
    /// </summary>
    public class IMGBUnpack
    {
        /// <summary>
        /// Use for unpacking image files.
        /// </summary>
        /// <param name="imgHeaderBlockFile">Header Block file path. should have the GTEX chunk.</param>
        /// <param name="imgbFile">IMGB file path. the file has to be present.</param>
        /// <param name="extractIMGBdir">Path to the directory where the image files should be unpacked.</param>
        /// <param name="showLog">Determine whether to show more messages related to this method's process.</param>
        public static void UnpackIMGB(string imgHeaderBlockFile, string imgbFile, string extractIMGBdir, bool showLog)
        {
            if (IMGBLoadTypes.IMGBData.Length == 0)
            {
                IMGBLoadTypes.IMGBData = File.ReadAllBytes(imgbFile);
            }

            var gtex = SharedMethods.GetGTEXInfo(File.ReadAllBytes(imgHeaderBlockFile), Path.GetFileName(imgHeaderBlockFile));

            if (!gtex.IsValid)
            {
                SharedMethods.DisplayLogMessage("Unable to find GTEX chunk. skipped image extraction.", true);
                return;
            }

            SharedMethods.DisplayLogMessage($"Image Format Value: {gtex.ImgFormatFlag}", showLog);
            SharedMethods.DisplayLogMessage($"Image MipCount: {gtex.MipCount}", showLog);
            SharedMethods.DisplayLogMessage($"Image Type Value: {gtex.ImgTypeFlag}", showLog);
            SharedMethods.DisplayLogMessage($"Image Width: {gtex.Width}", showLog);
            SharedMethods.DisplayLogMessage($"Image Height: {gtex.Height}", showLog);

            if (!SharedMethods.CheckGTEXFlags(gtex))
            {
                SharedMethods.DisplayLogMessage("Detected unknown platform or image format or image type. skipped image extraction.", true);
                return;
            }

            SharedMethods.LoadGTEXFlags(gtex);

            if (gtex.Platform == Platforms.ps3)
            {
                PS3Helpers.MipWidth = gtex.Width;
                PS3Helpers.MipHeight = gtex.Height;
            }

            if (gtex.Platform == Platforms.x360)
            {
                X360Helpers.MipWidth = gtex.Width;
                X360Helpers.MipHeight = gtex.Height;
            }

            if (gtex.IsMipPackedAfterGTEX)
            {
                using (var mipDataStream = new MemoryStream(File.ReadAllBytes(imgHeaderBlockFile)))
                {
                    _ = mipDataStream.Seek(gtex.GTEXOffset + gtex.MipDataStartOffset, SeekOrigin.Begin);
                    var mipDataSize = mipDataStream.Length - mipDataStream.Position;

                    IMGBLoadTypes.IMGBData = new byte[(int)mipDataSize];
                    _ = mipDataStream.Read(IMGBLoadTypes.IMGBData, 0, IMGBLoadTypes.IMGBData.Length);
                }
            }

            switch (gtex.ImgType)
            {
                case ImageTypes.classic:
                case ImageTypes.classic_swizzled:
                    IMGBUnpackTypes.UnpackClassic(imgHeaderBlockFile, extractIMGBdir, gtex);
                    break;

                case ImageTypes.cubemap:
                case ImageTypes.cubemap_swizzled:
                    IMGBUnpackTypes.UnpackCubemap(imgHeaderBlockFile, extractIMGBdir, gtex);
                    break;

                case ImageTypes.volumemap:
                    IMGBUnpackTypes.UnpackVolumemap(imgHeaderBlockFile, extractIMGBdir, gtex);
                    break;
            }

            if (gtex.IsMipPackedAfterGTEX)
            {
                IMGBLoadTypes.IMGBData = Array.Empty<byte>();
            }
        }
    }
}