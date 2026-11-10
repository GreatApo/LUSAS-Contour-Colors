using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LusasContourColors
{
    internal sealed class PaletteListBox : ListBox
    {
        private static readonly Color SelectedBackground = Color.FromArgb(235, 242, 255);
        private static readonly Color TextColour = Color.FromArgb(32, 32, 32);
        private readonly Font _font;

        public PaletteListBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            BorderStyle = BorderStyle.None;
            IntegralHeight = false;
            BackColor = Color.White;
            this.TabStop = false;
            _font = new Font("Segoe UI", 9.5f);
            Font = _font;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint, true);
            DoubleBuffered = true;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _font?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            ItemHeight = Math.Max(32, Font.Height + 16);
            Invalidate();
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count) return;

            var palette = Items[e.Index] as Palette;
            if (palette == null) return;

            using (var backBrush = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
            }

            var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            if (selected)
            {
                var pill = new RectangleF(e.Bounds.Left + 4, e.Bounds.Top + 4, e.Bounds.Width - 8, e.Bounds.Height - 8);
                using (var path = Drawing.RoundedRectangle(pill, 10f))
                using (var brush = new SolidBrush(SelectedBackground))
                {
                    e.Graphics.FillPath(brush, path);
                }
            }

            var colours = palette.Colours;
            var diameter = Math.Min(Font.Height + 2f, e.Bounds.Height - 14f);
            if (diameter < 8f) return;

            var step = diameter * 0.6f;
            var groupWidth = diameter + step * (colours.Length - 1);
            var startX = e.Bounds.Right - 14f - groupWidth;
            var textWidth = Math.Max(0, (int)(startX - 10f) - (e.Bounds.Left + 14));

            if (textWidth > 0)
            {
                var textBounds = new Rectangle(e.Bounds.Left + 14, e.Bounds.Top, textWidth, e.Bounds.Height);
                TextRenderer.DrawText(
                    e.Graphics,
                    palette.Name,
                    Font,
                    textBounds,
                    TextColour,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
            }

            var top = e.Bounds.Top + (e.Bounds.Height - diameter) / 2f;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var path = Drawing.SwatchGroupOutline(startX, top, diameter, step, colours.Length, 0.5f))
            using (var pen = new Pen(Color.Black, 2f) { LineJoin = LineJoin.Round })
            {
                e.Graphics.DrawPath(pen, path);
            }

            for (var i = 0; i < colours.Length; i++)
            {
                var circle = new RectangleF(startX + i * step, top, diameter, diameter);
                using (var brush = new SolidBrush(colours[i]))
                {
                    e.Graphics.FillEllipse(brush, circle);
                }
            }

        }

    }
}
