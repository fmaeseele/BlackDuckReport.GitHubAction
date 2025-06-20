using System;

namespace BlackDuckReport.GitHubAction.MarkdownBuilder
{
    /// <summary>
    /// Markdown text element.
    /// </summary>
    public abstract class MarkdownTextElement
    {
        private MarkdownInlineElement? inlineElement;

        /// <summary>
        /// Gets or sets the markdown inline element.
        /// </summary>
        /// <value>The markdown inline element.</value>
        protected MarkdownInlineElement? InlineElement
        {
            get => inlineElement;
            set
            {
                if (value != null)
                {
                    Text = null;
                }

                inlineElement = value;
            }
        }

        private string? text;

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text or a string that represents the markdown inline element.</value>
        public string? Text
        {
            get
            {
                if (text == null && InlineElement != null)
                {
                    return InlineElement.ToString();
                }

                return text;
            }
            set
            {
                if (value != null)
                {
                    InlineElement = null;
                }

                text = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownTextElement" /> class.
        /// </summary>
        /// <param name="text">The text.</param>
        public MarkdownTextElement(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownTextElement" /> class.
        /// </summary>
        /// <param name="inlineElement">The text as markdown inline element.</param>
        public MarkdownTextElement(MarkdownInlineElement inlineElement)
        {
            //Guard.Argument(inlineElement, nameof(inlineElement)).NotNull();
            ArgumentNullException.ThrowIfNull(inlineElement);

            InlineElement = inlineElement;
        }
    }
}
