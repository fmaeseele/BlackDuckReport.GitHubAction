using System;
using System.Collections.Generic;
using System.Linq;

namespace BlackDuckReport.GitHubAction.MarkdownBuilder
{
    /// <summary>
    /// Markdown table row.
    /// </summary>
    public class MarkdownTableRow
    {
        /// <summary>Gets the cells.</summary>
        /// <value>The cells.</value>
        public MarkdownInlineElement[] Cells { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownTableRow"/> class.
        /// </summary>
        /// <param name="cells">The cells.</param>
        public MarkdownTableRow(IEnumerable<MarkdownInlineElement> cells)
        {
            //Guard.Argument(cells, nameof(cells)).NotNull();
            ArgumentNullException.ThrowIfNull(cells);

            Cells = [.. cells];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownTableRow"/> class.
        /// </summary>
        /// <param name="cells">The cells.</param>
        public MarkdownTableRow(params MarkdownInlineElement[] cells)
        {
            //Guard.Argument(cells, nameof(cells)).NotNull();
            ArgumentNullException.ThrowIfNull(cells);

            Cells = (MarkdownInlineElement[])cells.Clone();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownTableRow"/> class.
        /// </summary>
        /// <param name="cells">The cells.</param>
        public MarkdownTableRow(IEnumerable<string> cells)
        {
            //Guard.Argument(cells, nameof(cells)).NotNull();
            ArgumentNullException.ThrowIfNull(cells);

            Cells = [.. cells.Select(cell => new MarkdownText(cell))];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownTableRow"/> class.
        /// </summary>
        /// <param name="cells">The cells.</param>
        public MarkdownTableRow(params string[] cells)
        {
            //Guard.Argument(cells, nameof(cells)).NotNull();
            ArgumentNullException.ThrowIfNull(cells);

            Cells = [.. cells.Select(cell => new MarkdownText(cell))];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownTableRow"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public MarkdownTableRow(int capacity)
        {
            //Guard.Argument(capacity, nameof(capacity)).Positive();
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Table row cells capacity must be greater than 0.");
            }

            Cells = new MarkdownInlineElement[capacity];
        }

        /// <summary>
        /// Returns a string that represents the current markdown table row.
        /// </summary>
        /// <returns>A string that represents the current markdown table row.</returns>
        public override string ToString()
        {
            return $"{string.Concat(Cells.Select(c => $"| {c} "))}|";
        }
    }
}
