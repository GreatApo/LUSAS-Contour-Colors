using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LusasContourColors
{
    internal sealed class IconButton : Button
    {
        private static readonly Color Accent = Color.FromArgb(0, 122, 255);
        private static readonly Color SurfaceColour = Color.FromArgb(247, 248, 250);
        private static readonly Color HoverColour = Color.FromArgb(241, 243, 246);
        private static readonly Color PressedColour = Color.FromArgb(233, 237, 242);
        private static readonly Color BorderColour = Color.FromArgb(232, 234, 238);
        private static readonly Color IconColour = Color.FromArgb(75, 85, 99);
        private bool _hover;
        private bool _pressed;

        public IconButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            BackColor = Color.White;
            Cursor = Cursors.Hand;
            TabStop = true;

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var circle = new RectangleF(0, 0, Width, Height);
            var fill = _pressed
                ? PressedColour
                : (_hover ? HoverColour : SurfaceColour);

            using (var path = Drawing.RoundedRectangle(circle, circle.Height / 2f))
            using (var brush = new SolidBrush(fill))
            {
                e.Graphics.FillPath(brush, path);
            }

            var border = new RectangleF(1f, 1f, Width - 2f, Height - 2f);
            using (var path = Drawing.RoundedRectangle(border, border.Height / 2f))
            using (var pen = new Pen(BorderColour, 1f))
            {
                e.Graphics.DrawPath(pen, path);
            }

            if (Focused)
            {
                var ring = new RectangleF(2.5f, 2.5f, Width - 5f, Height - 5f);
                using (var path = Drawing.RoundedRectangle(ring, ring.Height / 2f))
                using (var pen = new Pen(Accent, 1.5f))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }

            DrawSwapIcon(e.Graphics);
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

        private void DrawSwapIcon(Graphics graphics)
        {
            var left = Width * 0.28f;
            var right = Width * 0.72f;
            var top = Height / 2f - 4f;
            var bottom = Height / 2f + 4f;

            using (var pen = new Pen(IconColour, 2f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            })
            {
                graphics.DrawLine(pen, left, top, right, top);
                graphics.DrawLine(pen, right, top, right - 4f, top - 3f);
                graphics.DrawLine(pen, right, top, right - 4f, top + 3f);

                graphics.DrawLine(pen, right, bottom, left, bottom);
                graphics.DrawLine(pen, left, bottom, left + 4f, bottom - 3f);
                graphics.DrawLine(pen, left, bottom, left + 4f, bottom + 3f);
            }
        }
    }
}
