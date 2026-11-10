using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace LusasContourColors
{
    internal static class Drawing
    {
        public static GraphicsPath RoundedRectangle(RectangleF bounds, float radius)
        {
            var path = new GraphicsPath();
            var diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static GraphicsPath SwatchGroupOutline(float startX, float top, float diameter, float step, int count, float padding)
        {
            var path = new GraphicsPath();
            if (count <= 0) return path;

            var radius = diameter / 2f + padding;
            var size = radius * 2f;
            var centerY = top + diameter / 2f;

            if (count == 1)
            {
                path.AddEllipse(startX - padding, top - padding, diameter + padding * 2f, diameter + padding * 2f);
                return path;
            }

            var half = step / 2f;
            if (half >= radius)
            {
                for (var i = 0; i < count; i++)
                {
                    var centerX = startX + i * step + diameter / 2f;
                    path.AddEllipse(centerX - radius, centerY - radius, size, size);
                }

                return path;
            }

            var height = (float)Math.Sqrt(radius * radius - half * half);
            var nextTop = Angle(half, -height);
            var previousTop = Angle(-half, -height);
            var nextBottom = Angle(half, height);
            var previousBottom = Angle(-half, height);

            for (var i = 0; i < count; i++)
            {
                var centerX = startX + i * step + diameter / 2f;
                var start = i == 0 ? 180f : previousTop;
                var end = i == count - 1 ? 360f : nextTop;
                path.AddArc(centerX - radius, centerY - radius, size, size, start, end - start);
            }

            for (var i = count - 1; i >= 0; i--)
            {
                var centerX = startX + i * step + diameter / 2f;
                var start = i == count - 1 ? 0f : nextBottom;
                var end = i == 0 ? 180f : previousBottom;
                path.AddArc(centerX - radius, centerY - radius, size, size, start, end - start);
            }

            path.CloseFigure();
            return path;
        }

        private static float Angle(float dx, float dy)
        {
            var degrees = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
            return degrees < 0f ? degrees + 360f : degrees;
        }
    }
}
