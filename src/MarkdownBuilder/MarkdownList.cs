using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackDuckReport.GitHubAction.MarkdownBuilder
{
    /// <summary>
    /// Markdown list.
    /// </summary>
    public class MarkdownList : IMarkdownListItem, IMarkdownBlockElement
    {
        private char @char;

        /// <summary>
        /// Gets or sets the bullet point character.
        /// </summary>
        /// <value>The bullet point character.</value>
        public char Char
        {
            get => @char;
            set
            {
                //Guard.Argument(value, nameof(value)).In('-', '*');
                if (value != '-' && value != '*' && value != '+')
                {
                    throw new ArgumentException("Bullet point character must be either '-', '*', or '+'.", nameof(value));
                }

                @char = value;
            }
        }

        /// <summary>
        /// Gets the list items.
        /// </summary>
        /// <value>List items.</value>
        public List<IMarkdownListItem> ListItems { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownList" /> class.
        /// </summary>
        public MarkdownList() : this('-', new List<IMarkdownListItem>()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownList" /> class.
        /// </summary>
        /// <param name="char">The bullet point character.</param>
        public MarkdownList(char @char) : this(@char, new List<IMarkdownListItem>()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownList" /> class.
        /// </summary>
        /// <param name="listItems">The list items.</param>
        public MarkdownList(params IMarkdownListItem[] listItems) : this('-', new List<IMarkdownListItem>(listItems)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownList" /> class.
        /// </summary>
        /// <param name="listItems">The list items.</param>
        public MarkdownList(IEnumerable<IMarkdownListItem> listItems) : this('-', new List<IMarkdownListItem>(listItems)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownList" /> class.
        /// </summary>
        /// <param name="listItems">The list items.</param>
        public MarkdownList(params string[] listItems) : this('-', listItems) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownList" /> class.
        /// </summary>
        /// <param name="char">The bullet point character.</param>
        /// <param name="listItems">The list items.</param>
        public MarkdownList(char @char, params IMarkdownListItem[] listItems)
        {
            Char = @char;
            ListItems = new List<IMarkdownListItem>(listItems);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownList" /> class.
        /// </summary>
        /// <param name="char">The bullet point character.</param>
        /// <param name="listItems">The list items.</param>
        public MarkdownList(char @char, IEnumerable<IMarkdownListItem> listItems)
        {
            Char = @char;
            ListItems = new List<IMarkdownListItem>(listItems);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownList" /> class.
        /// </summary>
        /// <param name="char">The bullet point character.</param>
        /// <param name="listItems">The list items.</param>
        public MarkdownList(char @char, params string[] listItems) : this(@char, listItems.Select(li => new MarkdownTextListItem(li))) { }

        /// <summary>
        /// Creates an item with the specified string value and adds this at the end of the items list.
        /// </summary>
        /// <param name="item">The item as string.</param>
        public void AddItem(string item)
        {
            ListItems.Add(new MarkdownTextListItem(item));
        }

        /// <summary>
        /// Creates an item with the specified markdown inline element and adds this at the end of the items list.
        /// </summary>
        /// <param name="item">The item as markdown inline element.</param>
        public void AddItem(MarkdownInlineElement item)
        {
            ListItems.Add(new MarkdownTextListItem(item));
        }

        /// <summary>
        /// Returns a string that represents the current markdown list.
        /// </summary>
        /// <returns>A string that represents the current markdown list.</returns>
        public override string ToString()
        {
            return Print(0);
        }

        /// <summary>
        /// Prints the bullet point.
        /// </summary>
        /// <param name="index">The index of the bullet point.</param>
        /// <returns>The string represent the bullet point.</returns>
        protected virtual string PrintBulletPoint(int index) => Char.ToString();

        private string Print(int level)
        {
            //Guard.Argument(level, nameof(level)).GreaterThan(-1);
            if (level < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(level), "Level must be greater than or equal to 0.");
            }

            var sb = new StringBuilder();
            for (int i = 0; i < ListItems.Count; i++)
            {
                if (ListItems[i] is MarkdownList list)
                {
                    sb.Append(list.Print(level + 1));
                }
                else
                {
                    sb.Append(new string(' ', level * 2));
                    sb.Append(PrintBulletPoint(i));
                    sb.Append(" ");
                    sb.AppendLine(ListItems[i].ToString());
                }
            }

            return sb.ToString();
        }
    }
}
