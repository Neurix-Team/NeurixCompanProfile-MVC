namespace Neurix.Helpers
{
    /// <summary>
    /// Picks the Tailwind column classes for a CMS-driven card grid from the number of
    /// cards, so a section with two published items fills the row instead of leaving a
    /// gap, and anything past three wraps onto the next row.
    /// </summary>
    /// <remarks>
    /// These class strings are only reachable to Tailwind because <c>tailwind.config.js</c>
    /// lists <c>./Helpers/**/*.cs</c> under <c>content</c>. Moving this file outside that
    /// glob would silently stop the classes being generated.
    /// </remarks>
    public static class CardGridCss
    {
        public static string ForItemCount(int count) => count switch
        {
            <= 1 => "grid grid-cols-1 gap-8",
            2 => "grid grid-cols-1 md:grid-cols-2 gap-8",
            _ => "grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8",
        };
    }
}
