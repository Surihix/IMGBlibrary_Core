using IMGBlibrary_Core.Support;
using static IMGBlibrary_Core.Support.IMGBFlags;

namespace IMGBlibrary_Core.Load
{
    /// <summary>
    /// Provides a method for loading image files from the FINAL FANTASY XIII trilogy.
    /// </summary>
    public class IMGBLoad
    {
        /// <summary>
        /// Use for unpacking image files.
        /// </summary>
        /// <param name="imgHeaderBlockData">Header Block data. should have the GTEX chunk.</param>
        /// <param name="imgHeaderBlockName">Header Block name. should have a valid header block extension.</param>
        /// <param name="imgbFileData">IMGB file data.</param>
        /// <param name="showLog">Determine whether to show more messages related to this method's process.</param>
        public static byte[] LoadIMGB(byte[] imgHeaderBlockData, string imgHeaderBlockName, byte[] imgbFileData, bool showLog)
        {
            if (IMGBLoadTypes.IMGBData.Length == 0)
            {
                IMGBLoadTypes.IMGBData = imgbFileData;
            }

            var ddsData = Array.Empty<byte>();

            var gtex = SharedMethods.GetGTEXInfo(imgHeaderBlockData, Path.GetFileName(imgHeaderBlockName));

            if (!gtex.IsValid)
            {
                SharedMethods.DisplayLogMessage("Unable to find GTEX chunk. skipped image loading.", true);
                return ddsData;
            }

            SharedMethods.DisplayLogMessage($"Image Format Value: {gtex.ImgFormatFlag}", showLog);
            SharedMethods.DisplayLogMessage($"Image MipCount: {gtex.MipCount}", showLog);
            SharedMethods.DisplayLogMessage($"Image Type Value: {gtex.ImgTypeFlag}", showLog);
            SharedMethods.DisplayLogMessage($"Image Width: {gtex.Width}", showLog);
            SharedMethods.DisplayLogMessage($"Image Height: {gtex.Height}", showLog);

            if (!SharedMethods.CheckGTEXFlags(gtex))
            {
                SharedMethods.DisplayLogMessage("Detected unknown platform or image format or image type. skipped image loading.", true);
                return ddsData;
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

            switch (gtex.ImgType)
            {
                case ImageTypes.classic:
                case ImageTypes.classic_swizzled:
                    ddsData = IMGBLoadTypes.LoadClassic(imgHeaderBlockData, gtex, true);
                    break;

                case ImageTypes.cubemap:
                case ImageTypes.cubemap_swizzled:
                    if (gtex.Platform == Platforms.win32 || gtex.Platform == Platforms.ps3)
                    {
                        ddsData = IMGBLoadTypes.LoadCubemap(imgHeaderBlockData, gtex, true);
                    }

                    if (gtex.Platform == Platforms.x360)
                    {
                        ddsData = IMGBLoadTypes.LoadCubemapX360(imgHeaderBlockData, gtex, true);
                    }
                    break;

                case ImageTypes.volumemap:
                    ddsData = IMGBLoadTypes.LoadVolumemap(imgHeaderBlockData, gtex, true);
                    break;
            }

            return ddsData;
        }
    }
}