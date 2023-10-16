namespace Library
{
    public class RetryHelper
    {
        public static async Task<T> RetryMethodWhichThrowsException<T>(int numberRetries, Func<Task<T>> runAction, Func<int, Exception, Task> catchAction)
        {
            int retryCount = 1;
            while (true)
            {
                try
                {
                    return await runAction();
                }
                catch (Exception e)
                {
                    if (retryCount >= numberRetries)
                        throw;
                    await catchAction(retryCount, e);
                    retryCount++;
                }
            }
        }
    }
}