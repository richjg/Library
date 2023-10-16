using System.Text;

namespace Library
{
    public abstract class TextFieldWriter : IDisposable
    {
        private StreamWriter writer;
        private char delimiter;

        protected TextFieldWriter(Stream stream, char delimiter, Encoding encoding)
        {
            this.delimiter = delimiter;
            writer = new StreamWriter(stream, encoding);
        }

        public Encoding Encoding => writer.Encoding;
        public Stream BaseStream => writer.BaseStream;

        public void WriteLine(IEnumerable<string> fields)
        {
            if (fields == null)
                throw new ArgumentNullException("fields");

            StringBuilder stringBuilder = new StringBuilder();
            bool hasData = false;
            foreach (string field in fields)
            {
                hasData = true;
                if (field.IsTrimmedNullOrEmpty())
                    stringBuilder.Append(delimiter);
                else
                    stringBuilder.AppendFormat("\"{0}\"{1}", field.Trim().Replace("\"", "\"\""), delimiter);
            }

            if (hasData)
            {
                // Remove Last delimiter
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }

            writer.WriteLine(stringBuilder.ToString());
        }

        public byte[] GetAllBytes()
        {
            Flush();
            if (BaseStream is MemoryStream ms)
            {
                return ms.ToArray();
            }

            var destination = new MemoryStream();
            writer.BaseStream.CopyTo(destination);
            return destination.ToArray();
        }

        public void Flush()
        {
            writer.Flush();
        }

        public void Close()
        {
            writer.Close();
        }

        public void Dispose()
        {
            writer.Dispose();
        }
    }
}
