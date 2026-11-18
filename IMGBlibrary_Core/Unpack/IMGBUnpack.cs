using IMGBlibrary_Core.Support;

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
            var gtex = SharedMethods.GetGTEXInfo(File.ReadAllBytes(imgHeaderBlockFile), Path.GetFileName(imgHeaderBlockFile));

            if (!gtex.IsValid)
            {
                SharedMethods.DisplayLogMessage("Unable to find GTEX chunk. skipped image extraction.", true);
                return;
            }

            SharedMethods.DisplayLogMessage($"Image Format Value: {gtex.Format}", showLog);
            SharedMethods.DisplayLogMessage($"Image MipCount: {gtex.MipCount}", showLog);
            SharedMethods.DisplayLogMessage($"Image Type Value: {gtex.Type}", showLog);
            SharedMethods.DisplayLogMessage($"Image Width: {gtex.Width}", showLog);
            SharedMethods.DisplayLogMessage($"Image Height: {gtex.Height}", showLog);

            if (!SharedMethods.CheckGTEXValues(gtex))
            {
                SharedMethods.DisplayLogMessage("Detected unknown platform or image format or image type. skipped image extraction.", true);
                return;
            }

            var platform = (IMGBFlags.Platforms)gtex.Platform;
            var type = (IMGBFlags.Types)gtex.Type;

            // Open the IMGB file and start extracting
            // the images according to the image type
            using (var imgbStream = new FileStream(imgbFile, FileMode.Open, FileAccess.ReadWrite))
            {
                IMGBUnpackTypes.Platform = platform;

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
                            IMGBUnpackTypes.UnpackClassicX360(imgHeaderBlockFile, extractIMGBdir, gtex, imgbStream);
                        }
                        else
                        {
                            IMGBUnpackTypes.UnpackClassic(imgHeaderBlockFile, extractIMGBdir, gtex, imgbStream);
                        }
                        break;

                    case IMGBFlags.Types.cubemap:
                    case IMGBFlags.Types.cubemap_console:
                        if (platform == IMGBFlags.Platforms.x360)
                        {
                            IMGBUnpackTypes.UnpackCubemapX360(imgHeaderBlockFile, extractIMGBdir, gtex, imgbStream);
                        }
                        else
                        {
                            IMGBUnpackTypes.UnpackCubemap(imgHeaderBlockFile, extractIMGBdir, gtex, imgbStream);
                        }
                        break;

                    case IMGBFlags.Types.volumemap:
                        IMGBUnpackTypes.UnpackVolumemap(imgHeaderBlockFile, extractIMGBdir, gtex, imgbStream);
                        break;
                }
            }
        }
    }
}