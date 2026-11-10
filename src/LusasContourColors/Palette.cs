using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LusasContourColors
{
    public enum PaletteType
    {
        Qualitative,
        Sequential,
        Diverging
    }

    public sealed class Palette
    {
        private readonly Color[] _colours;

        public Palette(string name, PaletteType type, params Color[] colours)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Palette name is required.", nameof(name));
            if (colours == null || colours.Length == 0) throw new ArgumentException("Palette colours are required.", nameof(colours));

            Name = name;
            Type = type;
            _colours = (Color[])colours.Clone();
        }

        public string Name { get; }

        public PaletteType Type { get; }

        public Color[] Colours => (Color[])_colours.Clone();

        public Color[] Resample(int count)
        {
            return ResampleColours(_colours, count, Type);
        }

        public static Color[] ResampleColours(IReadOnlyList<Color> colours, int count, PaletteType type)
        {
            if (colours == null) throw new ArgumentNullException(nameof(colours));
            if (colours.Count < 2) throw new ArgumentException("At least two colours are required.", nameof(colours));
            if (count < 2) throw new ArgumentOutOfRangeException(nameof(count), "At least two colours are required.");

            if (count == colours.Count)
            {
                var copy = new Color[count];
                for (var i = 0; i < count; i++) copy[i] = colours[i];
                return copy;
            }

            var result = new Color[count];
            if (type == PaletteType.Qualitative)
            {
                for (var i = 0; i < count; i++) result[i] = colours[i % colours.Count];
                return result;
            }

            var segments = colours.Count - 1;
            for (var i = 0; i < count; i++)
            {
                var position = i * segments / (double)(count - 1);
                var lower = (int)Math.Floor(position);
                var upper = Math.Min(lower + 1, segments);
                var fraction = position - lower;
                var start = colours[lower];
                var end = colours[upper];

                result[i] = Color.FromArgb(
                    255,
                    (int)Math.Round(start.R + (end.R - start.R) * fraction),
                    (int)Math.Round(start.G + (end.G - start.G) * fraction),
                    (int)Math.Round(start.B + (end.B - start.B) * fraction));
            }

            return result;
        }

        public bool Matches(Color[] colours)
        {
            if (colours == null || colours.Length != _colours.Length) return false;
            return !_colours.Where((colour, index) => colour.ToArgb() != colours[index].ToArgb()).Any();
        }
    }
}
