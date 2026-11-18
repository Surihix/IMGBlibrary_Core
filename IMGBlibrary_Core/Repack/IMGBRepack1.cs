using IMGBlibrary_Core.Support;

namespace IMGBlibrary_Core.Repack
{
    /// <summary>
    /// Provides a type of method for repacking image files from the FINAL FANTASY XIII trilogy.
    /// </summary>
    public class IMGBRepack1
    {
        /// <summary>
        /// Use for repacking images that require the pixel format, 
        /// mipcount and dimensions to be same as the original.
        /// </summary>
        /// <param name="imgHeaderBlockFile">Header Block file path. should have the GTEX chunk.</param>
        /// <param name="outImgbFile">IMGB file path. the file has to be present.</param>
        /// <param name="extractedIMGBdir">Path to the directory where the image files are present.</param>
        public static void RepackIMGBType1(string imgHeaderBlockFile, string outImgbFile, string extractedIMGBdir)
        {
            var gtex = SharedMethods.GetGTEXInfo(File.ReadAllBytes(imgHeaderBlockFile), Path.GetFileName(imgHeaderBlockFile));

            if (!gtex.IsValid)
            {
                SharedMethods.DisplayLogMessage("Unable to find GTEX chunk. skipped image repacking.", true);
                return;
            }

            if (!SharedMethods.CheckGTEXValues(gtex))
            {
                SharedMethods.DisplayLogMessage("Detected unknown platform or image format or image type. skipped image repacking.", true);
                return;
            }

            var platform = (IMGBFlags.Platforms)gtex.Platform;

            if (platform == IMGBFlags.Platforms.ps3)
            {
                SharedMethods.DisplayLogMessage("Detected ps3 version image file. image repacking is not supported.", true);
                return;
            }

            if (platform == IMGBFlags.Platforms.x360)
            {
                SharedMethods.DisplayLogMessage("Detected xbox 360 version image file. image repacking is not supported.", true);
                return;
            }

            var type = (IMGBFlags.Types)gtex.Type;

            // Open the IMGB file and start repacking
            // the images according to the image type
            using (var imgbStream = new FileStream(outImgbFile, FileMode.Open, FileAccess.Write))
            {
                switch (type)
                {
                    case IMGBFlags.Types.classic:
                        IMGBRepack1Types.RepackClassic(imgHeaderBlockFile, extractedIMGBdir, gtex, imgbStream);
                        break;

                    case IMGBFlags.Types.cubemap:
                        IMGBRepack1Types.RepackCubemap(imgHeaderBlockFile, extractedIMGBdir, gtex, imgbStream);
                        break;

                    case IMGBFlags.Types.volumemap:
                        IMGBRepack1Types.RepackVolumemap(imgHeaderBlockFile, extractedIMGBdir, gtex, imgbStream);
                        break;
                }
            }
        }
    }
}