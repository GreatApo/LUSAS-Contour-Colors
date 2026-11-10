using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LusasContourColors
{
    internal sealed class PillButton : Button
    {
        private static readonly Color Accent = Color.FromArgb(0, 122, 255);
        private static readonly Color TextColour = Color.FromArgb(32, 32, 32);
        private bool _hover;
        private bool _pressed;

        public PillButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            BackColor = Color.FromArgb(247, 248, 250);
            ForeColor = TextColour;
            Font = new Font("Segoe UI", 9F);
            Cursor = Cursors.Hand;

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
        }

        public bool Primary { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var bounds = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            var fill = Primary
                ? (_pressed ? Color.FromArgb(0, 100, 220) : (_hover ? Color.FromArgb(10, 132, 255) : Accent))
                : (_pressed ? Color.FromArgb(238, 242, 248) : (_hover ? Color.FromArgb(248, 250, 253) : Color.White));

            using (var path = Drawing.RoundedRectangle(bounds, bounds.Height / 2f))
            using (var brush = new SolidBrush(fill))
            {
                e.Graphics.FillPath(brush, path);
            }

            if (!Primary)
            {
                using (var path = Drawing.RoundedRectangle(bounds, bounds.Height / 2f))
                using (var pen = new Pen(Color.FromArgb(219, 223, 230)))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }

            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                ClientRectangle,
                Primary ? Color.White : TextColour,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

            if (Focused)
            {
                var ring = new RectangleF(2.5f, 2.5f, Width - 5f, Height - 5f);
                using (var path = Drawing.RoundedRectangle(ring, (Height - 5f) / 2f))
                using (var pen = new Pen(Primary ? Color.FromArgb(170, 255, 255, 255) : Accent, 1.5f))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            _pressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            if (mevent.Button != MouseButtons.Left) return;
            _pressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            _pressed = false;
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }
    }
}
