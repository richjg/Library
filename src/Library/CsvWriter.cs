using System.Text;

namespace Library
{
    public class CsvWriter : TextFieldWriter
    {
        public CsvWriter(Stream stream) : this(stream, new UTF8Encoding(false)) { }
        public CsvWriter(Stream stream, Encoding encoding) : base(stream, ',', encoding) { }
        public static CsvWriter CreateInMemory() => new CsvWriter(new MemoryStream());
    }
}
