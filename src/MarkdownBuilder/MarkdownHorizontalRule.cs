using System;

namespace BlackDuckReport.GitHubAction.MarkdownBuilder
{
    /// <summary>
    /// Markdown horizontal rule.
    /// </summary>
    public class MarkdownHorizontalRule : IMarkdownBlockElement
    {
        private char @char;

        /// <summary>
        /// Gets or sets the horizontal rule character.
        /// </summary>
        /// <value>Horizontal rule character.</value>
        public char Char
        {
            get => @char;
            set
            {
                //Guard.Argument(value, nameof(value)).In('-', '*', '_');
                if (value != '-' && value != '*' && value != '_')
                {
                    throw new ArgumentException("Horizontal rule character must be either '-', '*', or '_'.", nameof(value));
                }
                @char = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownHorizontalRule" /> class.
        /// </summary>
        public MarkdownHorizontalRule()
        {
            Char = '-';
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownHorizontalRule" /> class.
        /// </summary>
        /// <param name="char">The horizontal rule character.</param>
        public MarkdownHorizontalRule(char @char)
        {
            Char = @char;
        }

        /// <summary>
        /// Returns a string that represents the current markdown horizontal rule.
        /// </summary>
        /// <returns>A string that represents the current markdown horizontal rule.</returns>
        public override string ToString()
        {
            return string.Concat(Char, Char, Char, Environment.NewLine);
        }
    }
}
