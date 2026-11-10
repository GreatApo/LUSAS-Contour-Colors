using Lusas.LPI;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LusasContourColors
{
    internal sealed class ContourColorsDialog : Form
    {
        private static readonly Color MutedColour = Color.FromArgb(107, 114, 128);
        private static readonly Color CanvasColour = Color.FromArgb(247, 248, 250);

        private readonly ContourColorService _service;
        private readonly SwatchBar _swatches;
        private readonly PaletteListBox _sequential;
        private readonly PaletteListBox _diverging;
        private readonly PaletteListBox _qualitative;
        private readonly CountStepper _count;
        private readonly ToolTip _tooltip = new ToolTip();
        private readonly int[] _customColours = new int[16];
        private Color[] _colours;
        private Color[] _baseColours;
        private Palette _selectedPalette;
        private PaletteType _lastPaletteType = PaletteType.Diverging;
        private bool _countIsCustom;
        private bool _suppressCountChange;
        private bool _suppressPaletteSelection;

        public ContourColorsDialog(IFModeller modeller)
        {
            _service = new ContourColorService(modeller);

            Text = "Contour Colours";
            Font = new Font("Segoe UI", 9F);
            BackColor = CanvasColour;
            ClientSize = new Size(920, 580);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Font;
            AutoScaleDimensions = new SizeF(96F, 96F);
            DoubleBuffered = true;

            _swatches = new SwatchBar { Dock = DockStyle.Fill, Margin = new Padding(0) };
            _swatches.SwatchActivated += OnSwatchActivated;

            _sequential = CreatePaletteList();
            _diverging = CreatePaletteList();
            _qualitative = CreatePaletteList();

            _count = new CountStepper
            {
                Minimum = 2,
                Maximum = 20,
                Value = PaletteCatalog.ClassCount,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0)
            };
            _count.ValueChanged += OnColourCountChanged;
            _tooltip.SetToolTip(_count, "Number of contour colours (2-20)");

            var reverse = new IconButton
            {
                Size = new Size(38, 38),
                Anchor = AnchorStyles.Right,
                Margin = new Padding(0, 0, 6, 0),
                AccessibleName = "Reverse colours"
            };
            reverse.Click += (sender, args) => ReverseColours();
            _tooltip.SetToolTip(reverse, "Reverse colours");

            var heroLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
            heroLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            heroLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 46));
            heroLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            heroLayout.Controls.Add(_swatches, 0, 0);
            heroLayout.Controls.Add(reverse, 1, 0);

            var hero = new CardPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 14) };
            hero.Controls.Add(heroLayout);

            var columns = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 14),
                BackColor = Color.Transparent
            };
            columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3f));
            columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3f));
            columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3f));
            columns.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            columns.Controls.Add(CreatePaletteColumn("Sequential", _sequential, new Padding(0, 0, 6, 0)), 0, 0);
            columns.Controls.Add(CreatePaletteColumn("Diverging", _diverging, new Padding(6, 0, 6, 0)), 1, 0);
            columns.Controls.Add(CreatePaletteColumn("Qualitative", _qualitative, new Padding(6, 0, 0, 0)), 2, 0);

            var done = new PillButton
            {
                Text = "Done",
                Primary = true,
                DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI Semibold", 9F),
                Size = new Size(96, 34),
                Anchor = AnchorStyles.Right,
                Margin = new Padding(0)
            };
            done.Click += (sender, args) => Close();

            var countLabel = new Label
            {
                Text = "Colours",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = MutedColour,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 0, 8, 0)
            };

            var countLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
            countLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            countLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104));
            countLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            countLayout.Controls.Add(countLabel, 0, 0);
            countLayout.Controls.Add(_count, 1, 0);

            var footer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96));
            footer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            footer.Controls.Add(countLayout, 0, 0);
            footer.Controls.Add(done, 1, 0);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(20),
                BackColor = Color.Transparent
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 114));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.Controls.Add(hero, 0, 0);
            layout.Controls.Add(columns, 0, 1);
            layout.Controls.Add(footer, 0, 2);

            Controls.Add(layout);
            AcceptButton = done;
            CancelButton = done;

            LoadPalettes();
            LoadColours();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ActiveControl = _sequential;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _tooltip.Dispose();
            base.Dispose(disposing);
        }

        private PaletteListBox CreatePaletteList()
        {
            var list = new PaletteListBox { Dock = DockStyle.Fill, Margin = new Padding(0) };
            list.SelectedIndexChanged += OnPaletteSelected;
            return list;
        }

        private static Control CreatePaletteColumn(string title, PaletteListBox list, Padding margin)
        {
            var header = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 9F),
                ForeColor = MutedColour,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(22, 0, 0, 4),
                BackColor = Color.Transparent
            };

            var card = new CardPanel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            card.Controls.Add(list);

            var column = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = margin,
                BackColor = Color.Transparent
            };
            column.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            column.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            column.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            column.Controls.Add(header, 0, 0);
            column.Controls.Add(card, 0, 1);
            return column;
        }

        private void LoadPalettes()
        {
            _sequential.BeginUpdate();
            _diverging.BeginUpdate();
            _qualitative.BeginUpdate();
            try
            {
                _sequential.Items.Clear();
                _diverging.Items.Clear();
                _qualitative.Items.Clear();
                _sequential.Items.AddRange(PaletteCatalog.All.Where(palette => palette.Type == PaletteType.Sequential).ToArray());
                _diverging.Items.AddRange(PaletteCatalog.All.Where(palette => palette.Type == PaletteType.Diverging).ToArray());
                _qualitative.Items.AddRange(PaletteCatalog.All.Where(palette => palette.Type == PaletteType.Qualitative).ToArray());
            }
            finally
            {
                _sequential.EndUpdate();
                _diverging.EndUpdate();
                _qualitative.EndUpdate();
            }
        }

        private void LoadColours()
        {
            var palette = PaletteCatalog.All.First(item => item.Name == "Jedi");
            _selectedPalette = palette;
            _lastPaletteType = palette.Type;
            _colours = palette.Colours;
            _baseColours = palette.Colours;
            _countIsCustom = false;
            SetColourCount(_colours.Length);
            _swatches.Colours = _colours;
            SelectPalette(palette);
        }

        private void OnPaletteSelected(object sender, EventArgs e)
        {
            if (_suppressPaletteSelection) return;

            var list = sender as PaletteListBox;
            if (list == null) return;

            var palette = list.SelectedItem as Palette;
            if (palette == null) return;

            _suppressPaletteSelection = true;
            try
            {
                _sequential.BeginUpdate();
                _diverging.BeginUpdate();
                _qualitative.BeginUpdate();
                try
                {
                    if (!ReferenceEquals(list, _sequential)) _sequential.SelectedIndex = -1;
                    if (!ReferenceEquals(list, _diverging)) _diverging.SelectedIndex = -1;
                    if (!ReferenceEquals(list, _qualitative)) _qualitative.SelectedIndex = -1;
                }
                finally
                {
                    _qualitative.EndUpdate();
                    _diverging.EndUpdate();
                    _sequential.EndUpdate();
                }
            }
            finally
            {
                _suppressPaletteSelection = false;
            }

            _selectedPalette = palette;
            _lastPaletteType = palette.Type;
            _baseColours = palette.Colours;

            if (!_countIsCustom)
            {
                SetColourCount(palette.Colours.Length);
            }

            _colours = palette.Resample(_count.Value);
            SuspendLayout();
            try
            {
                _swatches.Colours = _colours;
            }
            finally
            {
                ResumeLayout(true);
            }
            Apply();
        }

        private void OnColourCountChanged(object sender, EventArgs e)
        {
            if (_suppressCountChange || _baseColours == null) return;

            _countIsCustom = true;
            _colours = _selectedPalette != null
                ? _selectedPalette.Resample(_count.Value)
                : Palette.ResampleColours(_baseColours, _count.Value, _lastPaletteType);

            SuspendLayout();
            try
            {
                _swatches.Colours = _colours;
            }
            finally
            {
                ResumeLayout(true);
            }
            Apply();
        }

        private void OnSwatchActivated(object sender, SwatchEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _colours.Length) return;

            Color newColor = _colours[e.Index];
            using (var dialog = new ColorDialog
            {
                Color = newColor,
                FullOpen = true,
                AnyColor = true,
                CustomColors = _customColours
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                if (dialog.CustomColors != null)
                {
                    Array.Copy(dialog.CustomColors, _customColours, Math.Min(dialog.CustomColors.Length, _customColours.Length));
                }

                newColor = dialog.Color;
                _colours[e.Index] = newColor;
                _baseColours = (Color[])_colours.Clone();
                _selectedPalette = null;
            }

            SuspendLayout();
            try
            {
                _swatches.SetColour(e.Index, newColor);
            }
            finally
            {
                ResumeLayout(true);
            }
            _selectedPalette = null;
            Apply();
        }

        private void ReverseColours()
        {
            Array.Reverse(_colours);
            _baseColours = (Color[])_colours.Clone();
            _selectedPalette = null;
            SuspendLayout();
            try
            {
                _swatches.Colours = _colours;
                ClearPaletteSelection();
            }
            finally
            {
                ResumeLayout(true);
            }
            Apply();
        }

        private void Apply()
        {
            try
            {
                _service.Apply(_colours);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetColourCount(int count)
        {
            var clamped = Math.Max(_count.Minimum, Math.Min(_count.Maximum, count));
            _suppressCountChange = true;
            try
            {
                _count.Value = clamped;
            }
            finally
            {
                _suppressCountChange = false;
            }
        }

        private void SelectPalette(Palette palette)
        {
            _suppressPaletteSelection = true;
            try
            {
                _sequential.BeginUpdate();
                _diverging.BeginUpdate();
                _qualitative.BeginUpdate();
                try
                {
                    _sequential.SelectedIndex = -1;
                    _diverging.SelectedIndex = -1;
                    _qualitative.SelectedIndex = -1;

                    PaletteListBox list;
                    switch (palette.Type)
                    {
                        case PaletteType.Sequential:
                            list = _sequential;
                            break;
                        case PaletteType.Diverging:
                            list = _diverging;
                            break;
                        default:
                            list = _qualitative;
                            break;
                    }

                    var index = FindPalette(list, palette.Colours);
                    if (index >= 0) list.SelectedIndex = index;
                }
                finally
                {
                    _qualitative.EndUpdate();
                    _diverging.EndUpdate();
                    _sequential.EndUpdate();
                }
            }
            finally
            {
                _suppressPaletteSelection = false;
            }
        }

        private static bool ColoursEqual(Color[] left, Color[] right)
        {
            if (left == null || right == null || left.Length != right.Length) return false;
            for (var i = 0; i < left.Length; i++)
            {
                if (left[i].ToArgb() != right[i].ToArgb()) return false;
            }

            return true;
        }

        private static int FindPalette(PaletteListBox list, Color[] colours)
        {
            for (var i = 0; i < list.Items.Count; i++)
            {
                var palette = (Palette)list.Items[i];
                if (palette.Matches(colours)) return i;
            }

            return -1;
        }

        private void ClearPaletteSelection()
        {
            _suppressPaletteSelection = true;
            try
            {
                _sequential.BeginUpdate();
                _diverging.BeginUpdate();
                _qualitative.BeginUpdate();
                try
                {
                    _sequential.SelectedIndex = -1;
                    _diverging.SelectedIndex = -1;
                    _qualitative.SelectedIndex = -1;
                }
                finally
                {
                    _qualitative.EndUpdate();
                    _diverging.EndUpdate();
                    _sequential.EndUpdate();
                }
            }
            finally
            {
                _suppressPaletteSelection = false;
            }
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
    }
}
