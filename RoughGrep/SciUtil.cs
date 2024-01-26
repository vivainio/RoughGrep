using ScintillaNET;
using System.Drawing;

namespace RoughGrep
{
    public static class SciUtil
    {
        public static Scintilla CreateScintilla()
        {
            var scintilla = new Scintilla();
            scintilla.Lexer = Lexer.Cpp;

            // Configuring the default style with properties
            // we have common to every lexer style saves time.

            scintilla.StyleResetDefault();
            scintilla.Styles[Style.Default].Font = "Consolas";
            scintilla.Styles[Style.Default].Size = 9;
            scintilla.StyleClearAll();

            // Configure the CPP (C#) lexer styles
            scintilla.Styles[Style.Cpp.Default].ForeColor = Color.Silver;
            scintilla.Styles[Style.Cpp.Comment].ForeColor = Color.FromArgb(0, 128, 0); // Green
            scintilla.Styles[Style.Cpp.CommentLine].ForeColor = Color.FromArgb(0, 128, 0); // Green
            scintilla.Styles[Style.Cpp.CommentLineDoc].ForeColor = Color.FromArgb(128, 128, 128); // Gray
            scintilla.Styles[Style.Cpp.Number].ForeColor = Color.Olive;
            scintilla.Styles[Style.Cpp.Word].ForeColor = Color.Blue;
            scintilla.Styles[Style.Cpp.Word2].ForeColor = Color.Blue;
            scintilla.Styles[Style.Cpp.String].ForeColor = Color.FromArgb(163, 21, 21); // Red
            scintilla.Styles[Style.Cpp.Character].ForeColor = Color.FromArgb(163, 21, 21); // Red
            scintilla.Styles[Style.Cpp.Verbatim].ForeColor = Color.FromArgb(163, 21, 21); // Red
            scintilla.Styles[Style.Cpp.StringEol].BackColor = Color.Pink;
            scintilla.Styles[Style.Cpp.Operator].ForeColor = Color.Purple;
            scintilla.Styles[Style.Cpp.Preprocessor].ForeColor = Color.Maroon;
            scintilla.Lexer = Lexer.Cpp;

            // Set the keywords
            scintilla.SetKeywords(
                0,
                "abstract as base break case catch checked continue default delegate do else event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using virtual while"
            );
            scintilla.SetKeywords(
                1,
                "bool byte char class const decimal double enum float int long sbyte short static string struct uint ulong ushort void"
            );

            scintilla.ScrollWidth = 0;
            scintilla.ScrollWidthTracking = true;

            return scintilla;
        }

        public static int SearchAndMove(
            Scintilla scintilla,
            string searchText,
            bool reverse = false
        )
        {
            if (reverse)
            {
                scintilla.TargetStart = scintilla.CurrentPosition - 1;
                scintilla.TargetEnd = 0;
            }
            else
            {
                scintilla.TargetStart = scintilla.CurrentPosition;
                scintilla.TargetEnd = scintilla.TextLength;
            }
            int pos = scintilla.SearchInTarget(searchText);
            if (pos != -1)
            {
                scintilla.SelectionStart = scintilla.TargetStart;
                scintilla.SelectionEnd = scintilla.TargetEnd;
                scintilla.ScrollCaret();
            }
            return pos;
        }

        public static void TouchAfterTextLoad(Scintilla scintilla)
        {
            scintilla.ScrollWidth = 1;
            scintilla.ScrollWidthTracking = true;
        }

        public static string GetSelectionOrWordOnPosition(Scintilla scintilla)
        {
            var selection = scintilla.SelectedText;
            if (!string.IsNullOrEmpty(selection))
                return selection;
            var pos = scintilla.CurrentPosition;
            int start = scintilla.WordStartPosition(pos, true);
            int end = scintilla.WordEndPosition(pos, true);
            var selected = scintilla.GetTextRange(start, end - start);
            return selected;

        }
        public static void SetAllText(Scintilla scintilla, string text)
        {
            scintilla.ReadOnly = false;
            scintilla.Text = text;
            scintilla.ReadOnly = true;
            TouchAfterTextLoad(scintilla);
        }

        public static void RevealLine(Scintilla scintilla, int line)
        {
            var linesOnScreen = scintilla.LinesOnScreen - 2;

            var start = scintilla.Lines[line - (linesOnScreen / 2)].Position;
            var end = scintilla.Lines[line + (linesOnScreen / 2)].Position;
            scintilla.ScrollRange(end, start);
        }
    }
}
