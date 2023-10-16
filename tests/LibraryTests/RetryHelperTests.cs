using System.Text;
using Library;
using NUnit.Framework;

namespace LibraryTests
{
    [TestFixture]
    public class RetryHelperTests
    {
        [Test]
        public async Task RetryMethodWhichThrowsException_WillReturnTheResult_AndNotPerformTheCatchAction_IfTheCallSucceedsFirstTime()
        {
            // Arrange
            var runTranscript = new StringBuilder();
            Func<Task<bool>> runAction = async () =>
            {
                runTranscript.Append("RunAction.");
                await Task.Delay(TimeSpan.Zero);
                return true;
            };

            var catchTranscript = new StringBuilder();
            Func<int, Exception, Task> catchAction = async (i, e) =>
            {
                catchTranscript.Append($"Run {i}, ExceptionMessage {e.Message}.");
                await Task.Delay(TimeSpan.Zero);
            };

            // Act
            var result = await RetryHelper.RetryMethodWhichThrowsException(3, runAction, catchAction);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(runTranscript.ToString(), Is.EqualTo("RunAction."));
            Assert.That(catchTranscript.ToString(), Is.EqualTo(""));
        }

        [Test]
        public void RetryMethodWhichThrowsException_WillRetryTheCorrectNumberOfTimes_AndEventuallyThrow_IfTheRunCallKeepsFailing()
        {
            // Arrange
            var runTranscript = new StringBuilder();
            Func<Task<bool>> runAction = async () =>
            {
                runTranscript.Append("RunAction.");
                await Task.Delay(TimeSpan.Zero);
                throw new Exception("The call keeps throwing");
            };

            var catchTranscript = new StringBuilder();
            Func<int, Exception, Task> catchAction = async (i, e) =>
            {
                catchTranscript.Append($"Run {i}, ExceptionMessage '{e.Message}'.");
                await Task.Delay(TimeSpan.Zero);
            };

            // Act
            var ex = Assert.ThrowsAsync<Exception>(() => RetryHelper.RetryMethodWhichThrowsException(3, runAction, catchAction));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("The call keeps throwing"));
            Assert.That(runTranscript.ToString(), Is.EqualTo("RunAction.RunAction.RunAction."));
            Assert.That(catchTranscript.ToString(), Is.EqualTo("Run 1, ExceptionMessage 'The call keeps throwing'.Run 2, ExceptionMessage 'The call keeps throwing'."));
        }

        [Test]
        public async Task RetryMethodWhichThrowsException_WillRetryAndSucceed_IfTheCallSucceedsInOneOfTheRetries()
        {
            // Arrange
            var runTranscript = new StringBuilder();
            int count = 0;
            Func<Task<bool>> runAction = async () =>
            {
                count++;
                runTranscript.Append("RunAction.");
                await Task.Delay(TimeSpan.Zero);
                if (count >= 3)
                    return true;
                else
                    throw new Exception("The call keeps throwing");
            };

            var catchTranscript = new StringBuilder();
            Func<int, Exception, Task> catchAction = async (i, e) =>
            {
                catchTranscript.Append($"Run {i}, ExceptionMessage '{e.Message}'.");
                await Task.Delay(TimeSpan.Zero);
            };

            // Act
            var result = await RetryHelper.RetryMethodWhichThrowsException(3, runAction, catchAction);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(runTranscript.ToString(), Is.EqualTo("RunAction.RunAction.RunAction."));
            Assert.That(catchTranscript.ToString(), Is.EqualTo("Run 1, ExceptionMessage 'The call keeps throwing'.Run 2, ExceptionMessage 'The call keeps throwing'."));
        }
    }
}