using IMGBlibrary_Core.Load;

namespace IMGBlibrary_Core.Support
{
    /// <summary>
    /// Library handling functions
    /// </summary>
    public class IMGBLibraryHandler
    {
        /// <summary>
        /// Use for ending current library session. Very Important for unpacking!
        /// </summary>
        public static void EndCurrentLibrarySession()
        {
            IMGBLoadTypes.IMGBData = Array.Empty<byte>();
        }
    }
}