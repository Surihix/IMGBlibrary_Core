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
        /// <param name="imgbPlatform">Platform of the header block file.</param>
        /// <param name="showLog">Determine whether to show more messages related to this method's process.</param>
        public static byte[] LoadIMGB(byte[] imgHeaderBlockData, string imgHeaderBlockName, string imgbFile, IMGBFlags.Platforms imgbPlatform, bool showLog)
        {
            var ddsData = Array.Empty<byte>();

            var gtex = SharedMethods.GetGTEXInfo(imgHeaderBlockData, Path.GetFileName(imgHeaderBlockName));

            if (!gtex.IsValid)
            {
                SharedMethods.DisplayLogMessage("Unable to find GTEX chunk. skipped image loading.", true);
                return ddsData;
            }

            if (imgbPlatform == IMGBFlags.Platforms.x360)
            {
                SharedMethods.DisplayLogMessage("Platform set to x360. loaded image file(s) will not be unswizzled.", true);
            }

            SharedMethods.DisplayLogMessage($"Image Format Value: {gtex.Format}", showLog);
            SharedMethods.DisplayLogMessage($"Image MipCount: {gtex.MipCount}", showLog);
            SharedMethods.DisplayLogMessage($"Image Type Value: {gtex.Type}", showLog);
            SharedMethods.DisplayLogMessage($"Image Width: {gtex.Width}", showLog);
            SharedMethods.DisplayLogMessage($"Image Height: {gtex.Height}", showLog);

            if (!SharedMethods.CheckGTEXFormatAndType(gtex))
            {
                SharedMethods.DisplayLogMessage("Detected unknown format or type. skipped image loading.", true);
                return ddsData;
            }

            // Open the IMGB file and start loading
            // the images according to the image type
            using (var imgbStream = new FileStream(imgbFile, FileMode.Open, FileAccess.ReadWrite))
            {
                IMGBLoadTypes.Platform = imgbPlatform;

                switch (gtex.Type)
                {
                    // Classic type
                    // Type 4 is for console versions
                    case 0:
                    case 4:
                        ddsData = IMGBLoadTypes.LoadClassic(imgHeaderBlockData, gtex, imgbStream);
                        break;

                    // Cubemap type 
                    // Type 5 is for console versions
                    case 1:
                    case 5:
                        ddsData = IMGBLoadTypes.LoadCubemap(imgHeaderBlockData, gtex, imgbStream);
                        break;

                    // Volumemap type
                    case 2:
                        ddsData = IMGBLoadTypes.LoadVolumemap(imgHeaderBlockData, gtex, imgbStream);
                        break;
                }
            }

            return ddsData;
        }
    }
}