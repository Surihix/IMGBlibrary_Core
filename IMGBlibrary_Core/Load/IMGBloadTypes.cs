using IMGBlibrary_Core.Support;

namespace IMGBlibrary_Core.Load
{
    internal class IMGBLoadTypes
    {
        public static IMGBFlags.Platforms Platform { get; set; }

        #region Classic type
        public static byte[] UnpackClassic(string imgHeaderBlockFile, GTEX gtex, FileStream imgbStream)
        {
            return null;
        }
        #endregion


        #region Cubemap type
        public static byte[] UnpackCubemap(string imgHeaderBlockFile, GTEX gtex, FileStream imgbStream)
        {
            return null;
        }
        #endregion


        #region Volumemap type
        public static byte[] UnpackVolumemap(string imgHeaderBlockFile, GTEX gtex, FileStream imgbStream)
        {
            return null;
        }
        #endregion
    }
}