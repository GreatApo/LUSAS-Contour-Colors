using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LusasContourColors
{
    internal sealed class SwatchBar : Control
    {
        private Color[] _colours = new Color[0];
        private int _selectedIndex;

        public SwatchBar()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.Opaque |
                ControlStyles.SupportsTransparentBackColor,
                true);

            BackColor = Color.White;
            TabStop = false;
            Cursor = Cursors.Hand;
            AccessibleName = "Contour colours";
            AccessibleRole = AccessibleRole.Grouping;
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

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        public event EventHandler<SwatchEventArgs> SwatchActivated;

        public Color[] Colours
        {
            get { return (Color[])_colours.Clone(); }
            set
            {
                _colours = value == null ? new Color[0] : (Color[])value.Clone();
                if (_selectedIndex >= _colours.Length) _selectedIndex = Math.Max(0, _colours.Length - 1);
                Invalidate();
            }
        }

        public void SetColour(int index, Color color)
        {
            if (index < 0 || index >= _colours.Length) return;
            if (_colours[index].ToArgb() == color.ToArgb()) return;
            _colours[index] = color;
            InvalidateSwatch(index);
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                var index = _colours.Length == 0 ? -1 : Math.Max(0, Math.Min(_colours.Length - 1, value));
                if (_selectedIndex == index) return;
                _selectedIndex = index;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (var backBrush = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(backBrush, ClientRectangle);
            }
            if (_colours.Length == 0) return;
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            float diameter;
            float step;
            var startX = GetGeometry(out diameter, out step);
            var top = (ClientSize.Height - diameter) / 2f;

            using (var path = Drawing.SwatchGroupOutline(startX, top, diameter, step, _colours.Length, 0.5f))
            using (var pen = new Pen(Color.Black, 2f) { LineJoin = LineJoin.Round })
            {
                e.Graphics.DrawPath(pen, path);
            }

            for (var i = 0; i < _colours.Length; i++)
            {
                var circle = new RectangleF(startX + i * step, top, diameter, diameter);
                using (var brush = new SolidBrush(_colours[i]))
                {
                    e.Graphics.FillEllipse(brush, circle);
                }
            }

            // Focus cue removed to avoid flicker on selection
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;

            var index = IndexAt(e.Location);
            if (index < 0) return;
            _selectedIndex = index;
            Activate(index);
        }



        private float GetGeometry(out float diameter, out float step)
        {
            var count = _colours.Length;
            diameter = Math.Min(60f, ClientSize.Height - 12f);
            if (diameter < 12f) diameter = Math.Max(6f, ClientSize.Height - 4f);

            if (count <= 1)
            {
                step = 0;
                return (ClientSize.Width - diameter) / 2f;
            }

            var maxGroup = ClientSize.Width - 24f;
            step = diameter * 0.76f;
            if (diameter + step * (count - 1) > maxGroup)
            {
                step = (maxGroup - diameter) / (count - 1);
                if (step < diameter * 0.4f)
                {
                    diameter = maxGroup / (1 + (count - 1) * 0.4f);
                    step = diameter * 0.4f;
                }
            }

            return (ClientSize.Width - (diameter + step * (count - 1))) / 2f;
        }

        private int IndexAt(Point point)
        {
            if (_colours.Length == 0) return -1;

            float diameter;
            float step;
            var startX = GetGeometry(out diameter, out step);
            var top = (ClientSize.Height - diameter) / 2f;
            var radius = diameter / 2f;

            for (var i = _colours.Length - 1; i >= 0; i--)
            {
                var centreX = startX + i * step + radius;
                var centreY = top + radius;
                var dx = point.X - centreX;
                var dy = point.Y - centreY;
                if (dx * dx + dy * dy <= radius * radius) return i;
            }

            return -1;
        }

        private void Activate(int index)
        {
            if (index < 0 || index >= _colours.Length) return;
            var handler = SwatchActivated;
            if (handler != null) handler(this, new SwatchEventArgs(index));
        }

        private void InvalidateSwatch(int index)
        {
            if (index < 0 || index >= _colours.Length) return;
            float diameter;
            float step;
            var startX = GetGeometry(out diameter, out step);
            var top = (ClientSize.Height - diameter) / 2f;
            var x = startX + index * step;
            var rect = new Rectangle(
                (int)Math.Floor(x - 2),
                (int)Math.Floor(top - 2),
                (int)Math.Ceiling(diameter + 4),
                (int)Math.Ceiling(diameter + 4));
            Invalidate(rect);
        }
    }

    internal sealed class SwatchEventArgs : EventArgs
    {
        public SwatchEventArgs(int index)
        {
            Index = index;
        }

        public int Index { get; }
    }
}
