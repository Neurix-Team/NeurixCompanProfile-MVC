namespace Neurix.BLL.Common
{
    /// <summary>
    /// Outcome of an operation that can fail for a business reason (for example,
    /// deleting a company that still has contacts). Input validation stays in
    /// ModelState — this is only for rules the service layer owns.
    /// </summary>
    public record ServiceResult(bool Success, string? ErrorMessage = null)
    {
        public static ServiceResult Ok() => new(true);
        public static ServiceResult Fail(string message) => new(false, message);
    }

    /// <summary>
    /// As <see cref="ServiceResult"/>, but carries a value back on success —
    /// used where the caller needs the id of whatever the operation produced.
    /// </summary>
    public record ServiceResult<T>(bool Success, T? Value, string? ErrorMessage = null)
    {
        public static ServiceResult<T> Ok(T value) => new(true, value);
        public static ServiceResult<T> Fail(string message) => new(false, default, message);
    }
}
