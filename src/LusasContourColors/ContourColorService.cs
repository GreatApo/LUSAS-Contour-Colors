using Lusas.LPI;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace LusasContourColors
{
    public sealed class ContourColorService
    {
        private const int ContourSet = 1;
        private readonly IFModeller _modeller;

        public ContourColorService(IFModeller modeller)
        {
            _modeller = modeller ?? throw new ArgumentNullException(nameof(modeller));
        }

        public void Apply(IReadOnlyList<Color> colours)
        {
            if (colours == null) throw new ArgumentNullException(nameof(colours));
            if (colours.Count < 2)
                throw new ArgumentException("At least two colours are required.", nameof(colours));

            var view = _modeller.getCurrentView();
            if (view == null) throw new InvalidOperationException("No active view.");

            if (!view.existsContoursLayer()) view.insertContoursLayer();

            var values = new object[colours.Count];
            for (var i = 0; i < colours.Count; i++)
            {
                values[i] = ToLusasColour(colours[i]);
            }

            var contours = view.contours();
            contours.setRangeAutoNumber(colours.Count - 1, ContourSet);
            contours.setSeedColours(values, ContourSet);
            view.update(true);
        }

        public static int ToLusasColour(Color colour)
        {
            return colour.R | (colour.G << 8) | (colour.B << 16);
        }
    }
}
