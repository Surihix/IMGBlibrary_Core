namespace IMGBlibrary_Core.Repack
{
    internal class ImageInfo
    {
        public byte ImgTypeFlag;
        public ushort Height;
        public ushort Width;
        public byte Depth;
        public byte MipCount;
        public byte PixelFormatFlag;
        public bool IsValid;
    }
}