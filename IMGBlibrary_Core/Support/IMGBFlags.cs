namespace IMGBlibrary_Core.Support
{
    /// <summary>
    /// Provides enums for accessing functions from this library.
    /// </summary>
    public class IMGBFlags
    {
        /// <summary>
        /// Use for determing the extension of the image header block file.
        /// </summary>
        public enum FileExtensions
        {
            /// <summary>
            /// Present mainly in trb files
            /// </summary>
            txb,

            /// <summary>
            /// Present manily in xgr files
            /// </summary>
            txbh,

            /// <summary>
            /// Present mainly in xfv files
            /// </summary>
            vtex
        }

        /// <summary>
        /// Use for setting the platform from the image header block file.
        /// </summary>
        public enum Platforms
        {
            /// <summary>
            /// PC version
            /// </summary>
            win32 = 1,

            /// <summary>
            /// PS3 version
            /// </summary>
            ps3 = 2,

            /// <summary>
            /// Xbox 360 version
            /// </summary>
            x360 = 3
        }

        /// <summary>
        /// Use for setting the image format from the header block file.
        /// </summary>
        public enum ImageFormats
        {
            /// <summary>
            /// Uncompressed DDS (can have mipmaps)
            /// </summary>
            r8g8b8a8_mips = 3,

            /// <summary>
            /// Uncompressed DDS (cannot have mipmaps)
            /// </summary>
            r8g8b8a8_no_mips = 4,

            /// <summary>
            /// BC1 compression
            /// </summary>
            dxt1 = 24,

            /// <summary>
            /// BC2 compression
            /// </summary>
            dxt3 = 25,

            /// <summary>
            /// BC3 compression
            /// </summary>
            dxt5 = 26
        };

        /// <summary>
        /// Use for setting the image type from the header block file.
        /// </summary>
        public enum ImageTypes
        {
            /// <summary>
            /// Normal image
            /// </summary>
            classic = 0,

            /// <summary>
            /// DDS cubemap image (type present only on win32 version, x360 version indicates swizzled data)
            /// </summary>
            cubemap = 1,

            /// <summary>
            /// DDS volumemap image
            /// </summary>
            volumemap = 2,

            /// <summary>
            /// Normal image (type present only on ps3, x360. x360 version indicates swizzled data)
            /// </summary>
            classic_console = 4,

            /// <summary>
            /// DDS cubemap image (type present only on consoles, x360 version indicates swizzled data)
            /// </summary>
            cubemap_console = 5
        }
    }
}