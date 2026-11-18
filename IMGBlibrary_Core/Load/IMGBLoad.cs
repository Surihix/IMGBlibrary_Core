using IMGBlibrary_Core.Support;

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
        /// <param name="imgbFile">IMGB file path. the file has to be present.</param>
        /// <param name="showLog">Determine whether to show more messages related to this method's process.</param>
        public static byte[] LoadIMGB(byte[] imgHeaderBlockData, string imgHeaderBlockName, string imgbFile, bool showLog)
        {
            var ddsData = Array.Empty<byte>();

            var gtex = SharedMethods.GetGTEXInfo(imgHeaderBlockData, Path.GetFileName(imgHeaderBlockName));

            if (!gtex.IsValid)
            {
                SharedMethods.DisplayLogMessage("Unable to find GTEX chunk. skipped image loading.", true);
                return ddsData;
            }

            SharedMethods.DisplayLogMessage($"Image Format Value: {gtex.Format}", showLog);
            SharedMethods.DisplayLogMessage($"Image MipCount: {gtex.MipCount}", showLog);
            SharedMethods.DisplayLogMessage($"Image Type Value: {gtex.Type}", showLog);
            SharedMethods.DisplayLogMessage($"Image Width: {gtex.Width}", showLog);
            SharedMethods.DisplayLogMessage($"Image Height: {gtex.Height}", showLog);

            if (!SharedMethods.CheckGTEXValues(gtex))
            {
                SharedMethods.DisplayLogMessage("Detected unknown platform or image format or image type. skipped image loading.", true);
                return ddsData;
            }

            var platform = (IMGBFlags.Platforms)gtex.Platform;
            var type = (IMGBFlags.Types)gtex.Type;

            // Open the IMGB file and start loading
            // the images according to the image type
            using (var imgbStream = new FileStream(imgbFile, FileMode.Open, FileAccess.ReadWrite))
            {
                IMGBLoadTypes.Platform = platform;

                if (platform == IMGBFlags.Platforms.x360)
                {
                    X360Helpers.MipWidth = gtex.Width;
                    X360Helpers.MipHeight = gtex.Height;
                }

                switch (type)
                {
                    case IMGBFlags.Types.classic:
                    case IMGBFlags.Types.classic_console:
                        if (platform == IMGBFlags.Platforms.x360)
                        {
                            ddsData = IMGBLoadTypes.LoadClassicX360(imgHeaderBlockData, gtex, imgbStream);
                        }
                        else
                        {
                            ddsData = IMGBLoadTypes.LoadClassic(imgHeaderBlockData, gtex, imgbStream);
                        }
                        break;

                    case IMGBFlags.Types.cubemap:
                    case IMGBFlags.Types.cubemap_console:
                        if (platform == IMGBFlags.Platforms.x360)
                        {
                            ddsData = IMGBLoadTypes.LoadCubemapX360(imgHeaderBlockData, gtex, imgbStream);
                        }
                        else
                        {
                            ddsData = IMGBLoadTypes.LoadCubemap(imgHeaderBlockData, gtex, imgbStream);
                        }
                        break;

                    case IMGBFlags.Types.volumemap:
                        ddsData = IMGBLoadTypes.LoadVolumemap(imgHeaderBlockData, gtex, imgbStream);
                        break;
                }
            }

            return ddsData;
        }
    }
}