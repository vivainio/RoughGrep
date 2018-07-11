using System.Windows.Forms;

namespace RoughGrep
{
    public static class FormsUtil
    {
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
