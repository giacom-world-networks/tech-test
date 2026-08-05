namespace Order.Model
{
    public enum OperationOutcome
    {
        Success,
        NotFound,
        Invalid
    }

    /// <summary>
    /// Lets the service layer report why an operation failed without throwing,
    /// so the controller can map the outcome onto the right HTTP status code.
    /// </summary>
    public class OperationResult<T>
    {
        private static readonly IReadOnlyList<string> NoErrors = [];

        private OperationResult(OperationOutcome outcome, T? value, IReadOnlyList<string> errors)
        {
            Outcome = outcome;
            Value = value;
            Errors = errors;
        }

        public OperationOutcome Outcome { get; }

        /// <summary>Set only when <see cref="IsSuccess"/> is true.</summary>
        public T? Value { get; }

        public IReadOnlyList<string> Errors { get; }

        public bool IsSuccess => Outcome == OperationOutcome.Success;

        public static OperationResult<T> Success(T value)
        {
            return new OperationResult<T>(OperationOutcome.Success, value, NoErrors);
        }

        public static OperationResult<T> NotFound()
        {
            return new OperationResult<T>(OperationOutcome.NotFound, default, NoErrors);
        }

        public static OperationResult<T> Invalid(params string[] errors)
        {
            if (errors == null || errors.Length == 0)
            {
                throw new ArgumentException("An invalid result must describe at least one error.", nameof(errors));
            }

            return new OperationResult<T>(OperationOutcome.Invalid, default, errors);
        }
    }
}
