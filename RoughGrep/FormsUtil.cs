using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RoughGrep
{
    public static class FormsUtil
    {
        // select one rectangle next to the originalForm and places now form there
        public static void FindVisiblePlaceForNewForm(Form originalForm, Form newForm)
        {
            newForm.StartPosition = FormStartPosition.Manual;
            var workarea = Screen.FromControl(originalForm).WorkingArea;
            newForm.Location = new Point(originalForm.Right, 5);
            newForm.Size = originalForm.Size;
            var left = Rectangle.FromLTRB(0, 0, originalForm.Left, workarea.Bottom);
            var right = Rectangle.FromLTRB(originalForm.Right, 0, workarea.Right, workarea.Bottom);
            var top = Rectangle.FromLTRB(0, 0, workarea.Right, originalForm.Top);
            var bottom = Rectangle.FromLTRB(
                0,
                originalForm.Bottom,
                workarea.Right,
                workarea.Bottom
            );
            var all = new[] { left, right, top, bottom }.Where(r =>
                r.Height > 200 && r.Width > 200
            );
            var best = all.OrderByDescending(r => r.Width * r.Height).FirstOrDefault();
            if (best == default(Rectangle))
            {
                best = Rectangle.FromLTRB(workarea.Right / 2, 0, workarea.Right, workarea.Bottom);
            }

            newForm.Location = best.Location;
            newForm.Size = best.Size;
        }

        public static void BringFormToFront(Form form, Form returnFocusTo = null)
        {
            form.TopMost = true;
            form.TopMost = false;
            if (returnFocusTo != null)
            {
                returnFocusTo.Activate();
            }
        }
    }
}
