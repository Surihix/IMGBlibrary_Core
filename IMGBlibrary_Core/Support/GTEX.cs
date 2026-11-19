using static IMGBlibrary_Core.Support.IMGBFlags;

namespace IMGBlibrary_Core.Support
{
    internal class GTEX
    {
        public string ImageName;
        public bool IsValid;
        public uint GTEXOffset;
        public byte Version;
        public byte PlatformFlag;
        public byte ImgFormatFlag;
        public byte MipCount;
        public byte UnkFlag;
        public byte ImgTypeFlag;
        public ushort Width;
        public ushort Height;
        public ushort Depth;
        public uint MipInfoTableOffset;
        public Platforms Platform;
        public ImageFormats ImgFormat;
        public ImageTypes ImgType;
    }
}