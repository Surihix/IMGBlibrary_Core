using IMGBlibrary_Core.Load;
using IMGBlibrary_Core.Support;

namespace IMGBlibrary_Core.Unpack
{
    internal class IMGBUnpackTypes
    {
        #region Classic type
        public static void UnpackClassic(string imgHeaderBlockFile, string extractIMGBdir, GTEX gtex, FileStream imgbStream)
        {
            var ddsData = IMGBLoadTypes.LoadClassic(File.ReadAllBytes(imgHeaderBlockFile), gtex, imgbStream, false);

            var ddsFile = Path.Combine(extractIMGBdir, gtex.ImageName + ".dds");
            File.WriteAllBytes(ddsFile, ddsData);

            SharedMethods.DisplayLogMessage("Unpacked " + ddsFile, true);
        }

        public static void UnpackClassicX360(string imgHeaderBlockFile, string extractIMGBdir, GTEX gtex, FileStream imgbStream)
        {
            var ddsData = IMGBLoadTypes.LoadClassicX360(File.ReadAllBytes(imgHeaderBlockFile), gtex, imgbStream, false);

            var ddsFile = Path.Combine(extractIMGBdir, gtex.ImageName + ".dds");
            File.WriteAllBytes(ddsFile, ddsData);

            SharedMethods.DisplayLogMessage("Unpacked " + ddsFile, true);
        }
        #endregion


        #region Cubemap type
        public static void UnpackCubemap(string imgHeaderBlockFile, string extractIMGBdir, GTEX gtex, FileStream imgbStream)
        {
            var ddsData = IMGBLoadTypes.LoadCubemap(File.ReadAllBytes(imgHeaderBlockFile), gtex, imgbStream, false);

            var ddsFile = Path.Combine(extractIMGBdir, gtex.ImageName + "_cbmap.dds");
            File.WriteAllBytes(ddsFile, ddsData);

            SharedMethods.DisplayLogMessage("Unpacked " + ddsFile, true);
        }

        public static void UnpackCubemapX360(string imgHeaderBlockFile, string extractIMGBdir, GTEX gtex, FileStream imgbStream)
        {
            var ddsData = IMGBLoadTypes.LoadCubemapX360(File.ReadAllBytes(imgHeaderBlockFile), gtex, imgbStream, false);

            var ddsFile = Path.Combine(extractIMGBdir, gtex.ImageName + "_cbmap.dds");
            File.WriteAllBytes(ddsFile, ddsData);

            SharedMethods.DisplayLogMessage("Unpacked " + ddsFile, true);
        }
        #endregion


        #region Volumemap type
        public static void UnpackVolumemap(string imgHeaderBlockFile, string extractIMGBdir, GTEX gtex, FileStream imgbStream)
        {
            if (gtex.MipCount > 1)
            {
                SharedMethods.DisplayLogMessage("Detected more than one mip. mip 0 alone would be unpacked", true);
            }

            var ddsData = IMGBLoadTypes.LoadVolumemap(File.ReadAllBytes(imgHeaderBlockFile), gtex, imgbStream, false);

            var ddsFile = Path.Combine(extractIMGBdir, gtex.ImageName + "_volume.dds");
            File.WriteAllBytes(ddsFile, ddsData);

            SharedMethods.DisplayLogMessage("Unpacked " + ddsFile, true);
        }
        #endregion
    }
}