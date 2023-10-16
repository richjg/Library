using Library;
using NUnit.Framework;

namespace LibraryTests
{
    public class CsvWriterTest
    {
        [Test]
        public void WriteLine_NullDataThrowsException()
        {
            MemoryStream stream = new MemoryStream();
            CsvWriter writer = new CsvWriter(stream);

            Assert.That(() => writer.WriteLine(null!), Throws.InstanceOf<ArgumentNullException>());
        }

        [Test]
        public void WriteLine_HandlesNoData()
        {
            MemoryStream stream = new MemoryStream();
            List<string> fields = new List<string>();

            CsvWriter writer = new CsvWriter(stream);

            writer.WriteLine(fields);

            writer.Flush();

            Assert.That(stream.Length, Is.EqualTo(2));
        }

        [Test]
        public void WriteLine_CanWriteSimpleField()
        {
            MemoryStream stream = new MemoryStream();
            List<string> fields = new List<string>();

            CsvWriter writer = new CsvWriter(stream);

            fields.Add("Hello World");

            writer.WriteLine(fields);

            writer.Flush();

            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, writer.Encoding);
            Assert.That(reader.ReadLine(), Is.EqualTo("\"Hello World\""));
        }

        [Test]
        public void WriteLine_CanWriteSimpleList()
        {
            MemoryStream stream = new MemoryStream();
            List<string> fields = new List<string>();

            CsvWriter writer = new CsvWriter(stream);

            fields.Add("Hello World");
            fields.Add("Here is a £");

            writer.WriteLine(fields);

            writer.Flush();

            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, writer.Encoding);
            Assert.That(reader.ReadLine(), Is.EqualTo("\"Hello World\",\"Here is a £\""));
        }

        [Test]
        public void WriteLine_NullFieldsAreWrittenAsEmpty()
        {
            MemoryStream stream = new MemoryStream();
            List<string> fields = new List<string>();

            CsvWriter writer = new CsvWriter(stream);

            fields.Add("Hello");
            fields.Add(null!);
            fields.Add("World");

            writer.WriteLine(fields);

            writer.Flush();

            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, writer.Encoding);
            Assert.That(reader.ReadLine(), Is.EqualTo("\"Hello\",,\"World\""));
        }

        [Test]
        public void WriteLine_DoubleQuotesAreEscaped()
        {
            MemoryStream stream = new MemoryStream();
            List<string> fields = new List<string>();

            CsvWriter writer = new CsvWriter(stream);

            fields.Add("Hello \"My\" World");

            writer.WriteLine(fields);

            writer.Flush();

            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, writer.Encoding);
            Assert.That(reader.ReadLine(), Is.EqualTo("\"Hello \"\"My\"\" World\""));
        }

        [Test]
        public void WriteLine_EscapeCharactersAreNotEscaped()
        {
            MemoryStream stream = new MemoryStream();
            List<string> fields = new List<string>();

            CsvWriter writer = new CsvWriter(stream);

            fields.Add(@"Hello\tWorld");
            fields.Add(@"Hello\nWorld");

            writer.WriteLine(fields);

            writer.Flush();

            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, writer.Encoding);
            Assert.That(reader.ReadLine(), Is.EqualTo("\"Hello\\tWorld\",\"Hello\\nWorld\""));
        }

        [Test]
        public void WriteLine_CarriageReturnsAreMaintained()
        {
            MemoryStream stream = new MemoryStream();
            List<string> fields = new List<string>();

            CsvWriter writer = new CsvWriter(stream);

            fields.Add(@"Hello
World");

            writer.WriteLine(fields);

            writer.Flush();

            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, writer.Encoding);
            Assert.That(reader.ReadLine(), Is.EqualTo("\"Hello"));
            Assert.That(reader.ReadLine(), Is.EqualTo("World\""));
        }
    }
}
