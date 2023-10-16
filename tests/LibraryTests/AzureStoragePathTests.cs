using Library;
using NUnit.Framework;

namespace LibraryTests
{
    [TestFixture]
    public class AzureStoragePathTests
    {
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "\".txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvJTIyLnR4dA2")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "$.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvJC50eHQ1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "&.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvJi50eHQ1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "'.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvJy50eHQ1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "(.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvKC50eHQ1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", ").txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvKS50eHQ1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "*.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvKi50eHQ1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "+.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvKy50eHQ1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", ",.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvLC50eHQ1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", ":.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvOi50eHQ1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", ";.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvOy50eHQ1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "<.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvJTNDLnR4dA2")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "=.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvPS50eHQ1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", ">.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvJTNFLnR4dA2")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "@.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvQC50eHQ1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "Imported Test File 1", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvSW1wb3J0ZWQlMjBUZXN0JTIwRmlsZSUyMDE1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "Imported Test File 2", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvSW1wb3J0ZWQlMjBUZXN0JTIwRmlsZSUyMDI1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "[.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvWy50eHQ1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "].txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvXS50eHQ1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "^.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvJTVFLnR4dA2")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "_.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvXy50eHQ1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "`.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvJTYwLnR4dA2")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "bbbbbbbbbbb.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvYmJiYmJiYmJiYmIudHh00")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "ccccccccccc.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvY2NjY2NjY2NjY2MudHh00")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "empty file", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvZW1wdHklMjBmaWxl0")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "space test.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvc3BhY2UlMjB0ZXN0LnR4dA2")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "testfilename.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvdGVzdGZpbGVuYW1lLnR4dA2")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "{.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvJTdCLnR4dA2")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "|.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvJTdDLnR4dA2")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "}.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvJTdELnR4dA2")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "~.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvfi50eHQ1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "£.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvJUMyJUEzLnR4dA2")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "Â-×™×—×¦.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvJUMzJTgyLSVDMyU5NyVFMiU4NCVBMiVDMyU5NyVFMiU4MCU5NCVDMyU5NyVDMiVBNi50eHQ1")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "éçö.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvJUMzJUE5JUMzJUE3JUMzJUI2LnR4dA2")]
        [TestCase("BottKittBIOMMAT", "BottKittBIOMMAT-129", "😃 emoji test.txt", "aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvJUYwJTlGJTk4JTgzJTIwZW1vamklMjB0ZXN0LnR4dA2")]
        public void GetStoragePathEncoded_ShouldReturnTheSameAsAzure(string resourceGroupName, string assistantName, string filename, string expectedStoragePathEncoded)
        {
            // Act
            var storagePathEncoded = AzureStoragePath.Encode(resourceGroupName, assistantName, filename);

            // Assert
            Assert.That(storagePathEncoded, Is.EqualTo(expectedStoragePathEncoded), () => filename);
        }

        [Test]
        public void TestThatUrlUnsafeBasae64Characters_PlusAndForwardSlash_AreCorrectlyConvertedTo_MinusAndUnderbar()
        {
            // By default base 64 encoding uses the problematic characters + and /
            // + and / are problematic are not ok in urls, so there is a "url safe" version of base64 where 
            // + is changed to -
            // / is changed to _
            // see https://stackoverflow.com/questions/50731397/httpserverutility-urltokenencode-replacement-for-netstandard
            // and https://stackoverflow.com/a/26354677
            //
            // This test is designed to show that this change works correctly.
            //
            // In base 64 encoding, the bit array is encoded in chunks of 6-bits at a time https://en.wikipedia.org/wiki/Base64 
            // The following bit sequences give the problematic base 64 characters
            // 111110 = +
            // 111111 = /
            //
            // From the ascii table these bit sequences are contained in the following characters http://web.alfredstate.edu/faculty/weimandn/miscellaneous/ascii/ascii_index.html
            // ~ = 01111110
            // ? = 00111111
            //
            // Neither ~ or ? are url encoded, by the encoder we are using at the moment.
            //
            // The upshot of all this is is if we encode a string containing ~ and ?, we should see - and _ in the base64
            var storagePathEncoded = AzureStoragePath.Encode("BottKittBIOMMAT", "BottKittBIOMMAT-129", "00~00?00");
            Assert.That(storagePathEncoded, Is.EqualTo("aHR0cHM6Ly9ib3R0a2l0dGJpb21tYXQuYmxvYi5jb3JlLndpbmRvd3MubmV0L2JvdHRraXR0YmlvbW1hdC0xMjktZG9jdW1lbnRzZWFyY2gvMDB-MDA_MDA1"));

            var url = AzureStoragePath.Decode(storagePathEncoded);
            Assert.That(url, Does.EndWith("00~00?00"));
        }
    }
}