using System.Windows.Controls;
using System.Windows.Documents;

namespace Laba1.Core
{
    public static class TextPositionHelper
    {
        public static TextPointer? GetTextPointerAt(RichTextBox richTextBox, int targetLine, int targetColumn)
        {
            string text = new TextRange(
                richTextBox.Document.ContentStart,
                richTextBox.Document.ContentEnd).Text;

            int currentLine = 1;
            int currentColumn = 1;
            int charIndex = 0;

            while (charIndex < text.Length)
            {
                if (currentLine == targetLine && currentColumn == targetColumn)
                    break;

                if (text[charIndex] == '\n')
                {
                    currentLine++;
                    currentColumn = 1;
                }
                else if (text[charIndex] != '\r')
                {
                    currentColumn++;
                }

                charIndex++;
            }

            TextPointer navigator = richTextBox.Document.ContentStart;
            int count = 0;

            while (navigator != null && count < charIndex)
            {
                if (navigator.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string run = navigator.GetTextInRun(LogicalDirection.Forward);
                    int remaining = charIndex - count;

                    if (run.Length >= remaining)
                        return navigator.GetPositionAtOffset(remaining);

                    count += run.Length;
                    navigator = navigator.GetPositionAtOffset(run.Length);
                }
                else
                {
                    navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
                }
            }

            return navigator;
        }
    }
}