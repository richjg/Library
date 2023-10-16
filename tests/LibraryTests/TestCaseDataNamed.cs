namespace LibraryTests
{
    public class TestCaseDataNamed : TestCaseData
    {
        public TestCaseDataNamed(string testCaseName, params object[] args) : base(args)
        {
            SetArgDisplayNames(testCaseName);
        }
    }
}
