using Library;
using NUnit.Framework;

namespace LibraryTests
{
    public class HttpRequestHeaderValidatorTests
    {
        [TestCase("contains quotes but no token", "\"test\"")]
        [TestCase("contains EN DASH UTF-8 (hex)	0xE2 0x80 0x93 (e28093) but no token", "test–enDash")]
        public void TryAddIsValid_ReturnsFalse_WhenAuthorizationValueIs_(string testName, string headerValue)
        {
            var validator = new HttpRequestHeaderValidator();

            var result = validator.TryAddIsValid("Authorization", headerValue);

            //Assert
            Assert.That(result, Is.False);
        }

        [TestCase("contains quotes and token", "BASIC \"test\"")]
        [TestCase("contains EN DASH UTF-8 (hex)	0xE2 0x80 0x93 (e28093) and token", "bearer test–enDash")] //this will error on send HttpConnection.WriteStringAsync Request headers must contain only ASCII characters net5/6 might change this
        public void TryAddIsValid_ReturnsTrue_WhenAuthorizationValueIs_(string testName, string headerValue)
        {
            var validator = new HttpRequestHeaderValidator();

            var result = validator.TryAddIsValid("Authorization", headerValue);

            //Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void TryAddIsValid_ReturnsFalse_WhenMultipleAuthorizationHeaders()
        {
            var validator = new HttpRequestHeaderValidator();

            var result1 = validator.TryAddIsValid("Authorization", "basic ok");
            var result2 = validator.TryAddIsValid("Authorization", "basic again", out var e);

            //Assert
            Assert.That(result1, Is.True);
            Assert.That(result2, Is.False);
            Assert.That(e!.Message, Contains.Substring("Authorization' does not support multiple values"));
        }

        [Test]
        public void TryAddIsValid_ReturnsTrue_WhenMultipleProxyAuthenticateHeaders()
        {
            var validator = new HttpRequestHeaderValidator();

            var result1 = validator.TryAddIsValid("Proxy-Authenticate", "basic ok");
            var result2 = validator.TryAddIsValid("Proxy-Authenticate", "basic again", out var e);

            //Assert
            Assert.That(result1, Is.True);
            Assert.That(result2, Is.True);
        }

        [Test]
        public void TryAddIsValid_ReturnsTrue_WhenMultipleCustomHeaders()
        {
            var validator = new HttpRequestHeaderValidator();

            var result1 = validator.TryAddIsValid("Custom1", "value1");
            var result2 = validator.TryAddIsValid("Custom1", "value2");

            //Assert
            Assert.That(result1, Is.True);
            Assert.That(result2, Is.True);
        }

        [TestCase("json", "application/json")]
        [TestCase("test", "text/plain")]
        public void TryAddIsValid_ReturnsTrue_WhenContentTypeIs(string testName, string headerValue)
        {
            var validator = new HttpRequestHeaderValidator();

            var result = validator.TryAddIsValid("Content-Type", headerValue);

            //Assert
            Assert.That(result, Is.True);
        }

        [TestCase("blank", "")]
        [TestCase("null", null)]
        [TestCase("invalid mime type", "garbage")]
        public void TryAddIsValid_ReturnsFalse_WhenContentTypeIs(string testName, string headerValue)
        {
            var validator = new HttpRequestHeaderValidator();

            var result = validator.TryAddIsValid("Content-Type", headerValue);

            //Assert
            Assert.That(result, Is.False);
        }
    }
}
