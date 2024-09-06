namespace NRTools
{
    public class AssertionResult
    {
        public bool Passed { get; }
        public string Message { get; }
        public string StackTrace { get; }

        public AssertionResult(bool passed, string message, string stackTrace = null)
        {
            Passed = passed;
            Message = message;
            StackTrace = stackTrace;
        }

        public static AssertionResult Success(string message = "Test Passed!")
        {
            return new AssertionResult(true, message);
        }

        public static AssertionResult Fail(string message, string stackTrace = null)
        {
            return new AssertionResult(false, message, stackTrace);
        }
    }

}