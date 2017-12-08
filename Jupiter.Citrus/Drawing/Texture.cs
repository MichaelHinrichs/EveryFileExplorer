using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using _3DS.GPU;
using CommonCompressors;
using Jupiter.Citrus.Windows.Forms;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;

namespace Jupiter.Citrus.Drawing
{
    /// <summary>
    /// Represents a texture in a game developed by Jupiter Corporation for Nintendo 3DS devices.
    /// </summary>
    public class Texture : FileFormat<Texture.TextureIdentifier>, IViewable, IConvertable
    {
        /// <summary>
        /// Defines the header.
        /// </summary>
        public TextureHeader Header;

        /// <summary>
        /// Contains the decompressed data.
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// Initializes a new instance of <see cref="Texture"/> with some data.
        /// </summary>
        /// <param name="data">Data to use.</param>
        public Texture(byte[] data)
        {
            using (var reader = new EndianBinaryReader(new MemoryStream(data), Endianness.LittleEndian))
            {
                Header = new TextureHeader(reader);
                reader.BaseStream.Seek(Header.DataOffset, SeekOrigin.Begin);
                Data = reader.ReadBytes(Header.Width * Header.Height * 4);
            }
        }

        /// <summary>
        /// Represents the header of a <see cref="Texture"/> instance.
        /// </summary>
        public class TextureHeader
        {
            /// <summary>
            /// Defines the data offset.
            /// </summary>
            public int DataOffset;

            // TODO Match with ImageFormat?
            /// <summary>
            /// Defines the id of the corresponding texture format.
            /// </summary>
            public int Format;

            /// <summary>
            /// Defines the width used ingame. 
            /// </summary>
            public int CroppedWidth;

            /// <summary>
            /// Defines the height used ingame.
            /// </summary>
            public int CroppedHeight;

            /// <summary>
            /// Defines the width of the parent <see cref="Texture"/>.
            /// </summary>
            public int Width;

            /// <summary>
            /// Defines the height of the parent <see cref="Texture"/>.
            /// </summary>
            public int Height;

            /// <summary>
            /// Initializes a new instance of <see cref="TextureHeader"/> with a input stream.
            /// </summary>
            /// <param name="reader">An input stream reader.</param>
            public TextureHeader(EndianBinaryReader reader)
            {
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                DataOffset = reader.ReadInt32();
                Format = reader.ReadInt32();
                CroppedWidth = reader.ReadInt32();
                CroppedHeight = reader.ReadInt32();
                Width = reader.ReadInt32();
                Height = reader.ReadInt32();
            }
        }

        /// <summary>
        /// Returns a viewable representation of the native instance.
        /// </summary>
        /// <returns>An image.</returns>
        public Bitmap GetBitmap()
        {
            return Textures.ToBitmap(Data, Header.Width, Header.Height, Textures.ImageFormat.RGBA8);
        }

        /// <summary>
        /// Returns a viewable representation of the instance used ingame.
        /// </summary>
        /// <returns>An image.</returns>
        public Bitmap GetCroppedBitmap()
        {
            var rectangle = new Rectangle(0, 0, Header.CroppedWidth, Header.CroppedHeight);
            var original = GetBitmap();
            return original.Clone(rectangle, original.PixelFormat);
        }

        /// <summary>
        /// Returns an new instance of <see cref="TextureViewer"/>.
        /// </summary>
        /// <returns>A form.</returns>
        public Form GetDialog()
        {
            return new TextureViewer(this);
        }

        /// <summary>
        /// Returns filters to use within an instance of <see cref="SaveFileDialog"/>.
        /// </summary>
        /// <returns>Some file filters.</returns>
        public string GetConversionFileFilters()
        {
            return "Portable Network Graphics (*.png)|*.png";
        }

        /// <summary>
        /// Converts and saves the instance as an picture.
        /// </summary>
        /// <param name="FilterIndex">Selected filter index choosed by the user.</param>
        /// <param name="Path">Path to the destination file.</param>
        /// <returns><c>true</c> if the conversion succeeds.</returns>
        public bool Convert(int FilterIndex, string Path)
        {
            bool result = false;
            DialogResult dialogResult = MessageBox.Show("This texture has to possible size. The native size you see here and a cropped size to use ingame. Would you like to save the cropped one?", "", MessageBoxButtons.YesNo);
            Bitmap bitmap = dialogResult == DialogResult.Yes ? GetCroppedBitmap() : GetBitmap();
            if (FilterIndex == 0)
            {
                File.Create(Path).Close();
                bitmap.Save(Path, ImageFormat.Png);
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Identifies the file format of an instance of <see cref="Texture"/>.
        /// </summary>
        public class TextureIdentifier : FileFormatIdentifier
        {
            /// <summary>
            /// Returns the category of file corresponding to any instance of <see cref="Texture"/>.
            /// </summary>
            /// <returns>A category label.</returns>
            public override string GetCategory()
            {
                return Category_Textures;
            }

            /// <summary>
            /// Returns the file format description.
            /// </summary>
            /// <returns>A description.</returns>
            public override string GetFileDescription()
            {
                return "Jupiter Texture (JTEX)";
            }

            /// <summary>
            /// Returns filters to use within an instance of <see cref="OpenFileDialog"/>.
            /// </summary>
            /// <returns>Some file filters.</returns>
            public override string GetFileFilter()
            {
                return "Jupiter Texture (*.jtex)|*.jtex";
            }

            /// <summary>
            /// Analyzes the content of the file to see if it matches with an instance of <see cref="Texture"/>.
            /// </summary>
            /// <param name="File">File to analyze.</param>
            /// <returns><c><see cref="FormatMatch.Content"/></c> if the file starts with <c>80 00 00 00</c> and is longer than 0x80 bytes.</returns>
            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data[0] == 0x80 && File.Data[1] == 0x00 && File.Data[2] == 0x00 && File.Data[3] == 0x00 && File.Data.Length > 0x80)
                {
                    return FormatMatch.Content;
                }
                return FormatMatch.No;
            }
        }
    }
}
