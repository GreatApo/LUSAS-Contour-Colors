using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LusasContourColors
{
    internal sealed class CardPanel : Panel
    {
        public CardPanel()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            BackColor = Color.Transparent;
            Padding = new Padding(8);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var shadowPath = Drawing.RoundedRectangle(new RectangleF(1, 3, Width - 3, Height - 4), 14))
            using (var shadow = new SolidBrush(Color.FromArgb(14, 17, 24, 39)))
            {
                e.Graphics.FillPath(shadow, shadowPath);
            }

            var bounds = new RectangleF(1, 1, Width - 3, Height - 3);
            using (var path = Drawing.RoundedRectangle(bounds, 14))
            using (var fill = new SolidBrush(Color.White))
            {
                e.Graphics.FillPath(fill, path);
            }

            using (var path = Drawing.RoundedRectangle(bounds, 14))
            using (var pen = new Pen(Color.FromArgb(232, 234, 238)))
            {
                e.Graphics.DrawPath(pen, path);
            }
        }
    }
}
