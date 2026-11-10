using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace LusasContourColors
{
    internal sealed class CountStepper : Control
    {
        private static readonly Color CanvasColour = Color.FromArgb(247, 248, 250);
        private static readonly Color SurfaceColour = Color.White;
        private static readonly Color BorderColour = Color.FromArgb(219, 223, 230);
        private static readonly Color HoverColour = Color.FromArgb(243, 245, 248);
        private static readonly Color PressedColour = Color.FromArgb(235, 239, 244);
        private static readonly Color Accent = Color.FromArgb(0, 122, 255);
        private static readonly Color TextColour = Color.FromArgb(32, 32, 32);
        private static readonly Color MutedColour = Color.FromArgb(75, 85, 99);
        private static readonly Color DisabledColour = Color.FromArgb(180, 185, 192);
        private const int ButtonWidth = 30;

        private int _minimum = 2;
        private int _maximum = 20;
        private int _value = 11;
        private int _hoverZone;
        private int _pressedZone;

        public CountStepper()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.Selectable,
                true);

            BackColor = CanvasColour;
            Font = new Font("Segoe UI Semibold", 9F);
            Size = new Size(104, 30);
            TabStop = true;
            Cursor = Cursors.Hand;
            AccessibleName = "Number of colours";
            AccessibleRole = AccessibleRole.SpinButton;
        }

        public event EventHandler ValueChanged;

        public int Minimum
        {
            get { return _minimum; }
            set
            {
                _minimum = value;
                if (_maximum < _minimum) _maximum = _minimum;
                if (_value < _minimum) Value = _minimum;
                Invalidate();
            }
        }

        public int Maximum
        {
            get { return _maximum; }
            set
            {
                _maximum = value;
                if (_minimum > _maximum) _minimum = _maximum;
                if (_value > _maximum) Value = _maximum;
                Invalidate();
            }
        }

        public int Value
        {
            get { return _value; }
            set
            {
                var clamped = Math.Max(_minimum, Math.Min(_maximum, value));
                if (_value == clamped) return;
                _value = clamped;
                Invalidate();
                OnValueChanged(EventArgs.Empty);
            }
        }

        private void OnValueChanged(EventArgs e)
        {
            var handler = ValueChanged;
            if (handler != null) handler(this, e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var bounds = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            using (var path = Drawing.RoundedRectangle(bounds, bounds.Height / 2f))
            using (var brush = new SolidBrush(SurfaceColour))
            {
                e.Graphics.FillPath(brush, path);
            }

            var zone = Enabled ? (_pressedZone != 0 ? _pressedZone : _hoverZone) : 0;
            if (zone != 0)
            {
                using (var path = Drawing.RoundedRectangle(bounds, bounds.Height / 2f))
                {
                    var state = e.Graphics.Save();
                    e.Graphics.SetClip(path);
                    var fill = _pressedZone != 0 ? PressedColour : HoverColour;
                    var zoneBounds = zone < 0
                        ? new RectangleF(1f, 1f, ButtonWidth, Height - 2f)
                        : new RectangleF(Width - ButtonWidth - 1f, 1f, ButtonWidth, Height - 2f);
                    using (var brush = new SolidBrush(fill))
                    {
                        e.Graphics.FillRectangle(brush, zoneBounds);
                    }
                    e.Graphics.Restore(state);
                }
            }

            using (var pen = new Pen(BorderColour, 1f))
            {
                e.Graphics.DrawLine(pen, ButtonWidth + 0.5f, 5f, ButtonWidth + 0.5f, Height - 5f);
                e.Graphics.DrawLine(pen, Width - ButtonWidth - 0.5f, 5f, Width - ButtonWidth - 0.5f, Height - 5f);
            }

            using (var path = Drawing.RoundedRectangle(bounds, bounds.Height / 2f))
            using (var pen = new Pen(BorderColour, 1f))
            {
                e.Graphics.DrawPath(pen, path);
            }

            var middleY = Height / 2f;
            var minusX = ButtonWidth / 2f;
            var plusX = Width - ButtonWidth / 2f;
            var arm = 4f;
            var iconColour = Enabled ? MutedColour : DisabledColour;
            using (var pen = new Pen(iconColour, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                e.Graphics.DrawLine(pen, minusX - arm, middleY, minusX + arm, middleY);
                e.Graphics.DrawLine(pen, plusX - arm, middleY, plusX + arm, middleY);
                e.Graphics.DrawLine(pen, plusX, middleY - arm, plusX, middleY + arm);
            }

            var valueBounds = new Rectangle(ButtonWidth, 0, Width - ButtonWidth * 2, Height);
            TextRenderer.DrawText(
                e.Graphics,
                _value.ToString(CultureInfo.InvariantCulture),
                Font,
                valueBounds,
                Enabled ? TextColour : DisabledColour,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

            if (Focused && ShowFocusCues)
            {
                var ring = new RectangleF(2.5f, 2.5f, Width - 5f, Height - 5f);
                using (var path = Drawing.RoundedRectangle(ring, ring.Height / 2f))
                using (var pen = new Pen(Accent, 1.5f))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.PageUp:
                case Keys.PageDown:
                case Keys.Home:
                case Keys.End:
                    return true;
            }

            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!Enabled) return;

            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.Right:
                    Step(1);
                    e.Handled = true;
                    break;
                case Keys.Down:
                case Keys.Left:
                    Step(-1);
                    e.Handled = true;
                    break;
                case Keys.PageUp:
                    Step(5);
                    e.Handled = true;
                    break;
                case Keys.PageDown:
                    Step(-5);
                    e.Handled = true;
                    break;
                case Keys.Home:
                    Value = Minimum;
                    e.Handled = true;
                    break;
                case Keys.End:
                    Value = Maximum;
                    e.Handled = true;
                    break;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var zone = ZoneAt(e.Location);
            if (_hoverZone != zone)
            {
                _hoverZone = zone;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverZone = 0;
            _pressedZone = 0;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;

            Focus();
            var zone = ZoneAt(e.Location);
            if (zone == 0) return;

            _pressedZone = zone;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left) return;

            var zone = ZoneAt(e.Location);
            if (_pressedZone != 0 && _pressedZone == zone) Step(zone);

            _pressedZone = 0;
            Invalidate();
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);
            if (_pressedZone == 0) return;
            _pressedZone = 0;
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

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            _hoverZone = 0;
            _pressedZone = 0;
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }

        private int ZoneAt(Point point)
        {
            if (!Enabled || point.Y < 0 || point.Y >= Height) return 0;
            if (point.X >= 0 && point.X < ButtonWidth) return -1;
            if (point.X >= Width - ButtonWidth && point.X < Width) return 1;
            return 0;
        }

        private void Step(int direction)
        {
            if (!Enabled) return;
            Value = Value + direction;
        }
    }
}
