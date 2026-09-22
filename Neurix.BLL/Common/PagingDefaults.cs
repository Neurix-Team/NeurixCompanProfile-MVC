namespace Neurix.BLL.Common
{
    /// <summary>
    /// Server-side clamping so a hand-edited query string cannot request
    /// page 0, a negative page, or an unbounded page size.
    /// </summary>
    public static class PagingDefaults
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;

        public static int ClampPage(int page) => page < 1 ? 1 : page;

        public static int ClampPageSize(int pageSize) => pageSize switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => pageSize
        };
    }
}
