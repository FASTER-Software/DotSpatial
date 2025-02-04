// ********************************************************************************************************
// Product Name: Components.CategoriesViewer.dll Alpha
// Description:  The basic module for CategoriesViewer version 6.0
// ********************************************************************************************************
// The contents of this file are subject to the MIT License (MIT)
// you may not use this file except in compliance with the License. You may obtain a copy of the License at
// http://dotspatial.codeplex.com/license
//
// Software distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTY OF
// ANY KIND, either expressed or implied. See the License for the specific language governing rights and
// limitations under the License.
//
// The Original Code is from CategoriesViewer.dll version 6.0
//
// The Initial Developer of this Original Code is Jiri Kadlec. Created 5/14/2009 4:13:28 PM
//
// Contributor(s): (Open source contributors should list themselves and their modifications here).
//
// ********************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DotSpatial.Data;

namespace DotSpatial.Symbology.Forms
{
    /// <summary>
    /// Dialog for the 'unique values' feature symbol classification scheme
    /// </summary>
    public partial class RasterCategoryControl : UserControl, ICategoryControl
    {
        #region Events

        /// <summary>
        /// Occurs when the apply changes option has been triggered.
        /// </summary>
        public event EventHandler ChangesApplied;

        #endregion

        #region Private Variables

        //the original scheme which is modified only after clicking 'Apply'
        private int _activeCategoryIndex;
        private Timer _cleanupTimer;
        private int _dblClickEditIndex;
        private bool _ignoreEnter;

        //the attribute data Table
        private bool _ignoreRefresh;
        private bool _ignoreValidation;
        private double _maximum;
        private double _minimum;
        private IRasterLayer _newLayer;
        private IColorScheme _newScheme;
        private IColorScheme _originalScheme;
        private IRasterLayer _originalLayer;
        private ContextMenu _quickSchemes;
        private IRaster _raster;
        private IRasterSymbolizer _symbolizer;
        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty FeatureCategoryControl without specifying any particular layer to use
        /// </summary>
        public RasterCategoryControl()
        {
            InitializeComponent();
            Configure();
        }

        /// <summary>
        /// Creates a new instance of the unique values category Table
        /// </summary>
        /// <param name="layer">The feature set that is used</param>
        public RasterCategoryControl(IRasterLayer layer)
        {
            InitializeComponent();
            Configure();
            Initialize(layer);
        }

        /// <summary>
        /// Handles the mouse wheel, allowing the breakSliderGraph to zoom in or out.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            Point screenLoc = PointToScreen(e.Location);
            Point bsPoint = breakSliderGraph1.PointToClient(screenLoc);
            if (breakSliderGraph1.ClientRectangle.Contains(bsPoint))
            {
                breakSliderGraph1.DoMouseWheel(e.Delta, bsPoint.X);
                return;
            }
            base.OnMouseWheel(e);
        }

        private void Configure()
        {
            _elevationQuickPick = new ContextMenu();

            _elevationQuickPick.MenuItems.Add("Z Feet     | XY Lat Long", SetElevationFeetLatLong);
            _elevationQuickPick.MenuItems.Add("Z Feet     | XY Meters", SetElevationFeetMeters);
            _elevationQuickPick.MenuItems.Add("Z Feet     | XY Feet", SetElevationSameUnits);
            _elevationQuickPick.MenuItems.Add("Z Meters | XY Lat Long", SetElevationMetersLatLong);
            _elevationQuickPick.MenuItems.Add("Z Meters | XY Meters", SetElevationSameUnits);
            _elevationQuickPick.MenuItems.Add("Z Meters | XY Feet", SetElevationMetersFeet);
            dgvCategories.CellFormatting += DgvCategoriesCellFormatting;
            dgvCategories.CellDoubleClick += DgvCategoriesCellDoubleClick;
            dgvCategories.SelectionChanged += DgvCategoriesSelectionChanged;
            dgvCategories.CellValidated += DgvCategoriesCellValidated;
            dgvCategories.MouseDown += DgvCategoriesMouseDown;
            cmbInterval.SelectedItem = "EqualInterval";
            breakSliderGraph1.SliderSelected += BreakSliderGraph1SliderSelected;
            _quickSchemes = new ContextMenu();
            string[] names = Enum.GetNames(typeof(ColorSchemeType));
            foreach (string name in names)
            {
                MenuItem mi = new MenuItem(name, QuickSchemeClicked);
                _quickSchemes.MenuItems.Add(mi);
            }
            cmbIntervalSnapping.Items.Clear();
            var result = Enum.GetValues(typeof(IntervalSnapMethod));
            foreach (var item in result)
            {
                cmbIntervalSnapping.Items.Add(item);
            }
            cmbIntervalSnapping.SelectedItem = IntervalSnapMethod.Rounding;
            _cleanupTimer = new Timer();
            _cleanupTimer.Interval = 10;
            _cleanupTimer.Tick += CleanupTimerTick;

            // Allows shaded Relief to be edited
            _shadedReliefDialog = new PropertyDialog();
            _shadedReliefDialog.ChangesApplied += PropertyDialogChangesApplied;
        }

        private void SetElevationFeetMeters(object sender, EventArgs e)
        {
            dbxElevationFactor.Value = .3048;
        }

        private void SetElevationSameUnits(object sender, EventArgs e)
        {
            dbxElevationFactor.Value = 1;
        }

        private void SetElevationMetersFeet(object sender, EventArgs e)
        {
            dbxElevationFactor.Value = 3.2808399;
        }

        private void SetElevationFeetLatLong(object sender, EventArgs e)
        {
            dbxElevationFactor.Value = 1 / (111319.9 * 3.2808399);
        }

        private void SetElevationMetersLatLong(object sender, EventArgs e)
        {
            dbxElevationFactor.Value = 1 / 111319.9;
        }

        private void DgvCategoriesMouseDown(object sender, MouseEventArgs e)
        {
            if (_ignoreEnter) return;
            _activeCategoryIndex = dgvCategories.HitTest(e.X, e.Y).RowIndex;
        }

        private void CleanupTimerTick(object sender, EventArgs e)
        {
            // When a row validation causes rows above the edit row to be removed,
            // we can't easily update the Table during the validation event.
            // The timer allows the validation to finish before updating the Table.
            _cleanupTimer.Stop();
            _ignoreValidation = true;
            UpdateTable();
            if (_activeCategoryIndex >= 0 && _activeCategoryIndex < dgvCategories.Rows.Count)
            {
                dgvCategories.Rows[_activeCategoryIndex].Selected = true;
            }

            _ignoreValidation = false;
            _ignoreEnter = false;
        }

        private void BreakSliderGraph1SliderSelected(object sender, BreakSliderEventArgs e)
        {
            int index = breakSliderGraph1.Breaks.IndexOf(e.Slider);
            dgvCategories.Rows[index].Selected = true;
        }

        private void DgvCategoriesCellValidated(object sender, DataGridViewCellEventArgs e)
        {
            if (_ignoreValidation) return;
            if (_newScheme.Categories.Count <= e.RowIndex) return;

            if (e.ColumnIndex == 2)
            {
                IColorCategory fctxt = _newScheme.Categories[e.RowIndex];
                fctxt.LegendText = (string)dgvCategories[e.ColumnIndex, e.RowIndex].Value;
                return;
            }

            if (e.ColumnIndex != 1) return;

            IColorCategory cb = _newScheme.Categories[e.RowIndex];
            if ((string)dgvCategories[e.ColumnIndex, e.RowIndex].Value == cb.LegendText) return;
            _ignoreEnter = true;
            string exp = (string)dgvCategories[e.ColumnIndex, e.RowIndex].Value;
            cb.LegendText = exp;

            cb.Range = new Range(exp);
            if (cb.Range.Maximum != null && cb.Range.Maximum > _raster.Maximum)
            {
                cb.Range.Maximum = _raster.Maximum;
            }
            if (cb.Range.Minimum != null && cb.Range.Minimum > _raster.Maximum)
            {
                cb.Range.Minimum = _raster.Maximum;
            }
            if (cb.Range.Maximum != null && cb.Range.Minimum < _raster.Minimum)
            {
                cb.Range.Minimum = _raster.Minimum;
            }
            if (cb.Range.Minimum != null && cb.Range.Minimum < _raster.Minimum)
            {
                cb.Range.Minimum = _raster.Minimum;
            }
            cb.ApplyMinMax(_newScheme.EditorSettings);
            ColorCategoryCollection breaks = _newScheme.Categories;
            breaks.SuspendEvents();
            if (cb.Range.Minimum == null && cb.Range.Maximum == null)
            {
                breaks.Clear();
                breaks.Add(cb);
            }
            else if (cb.Range.Maximum == null)
            {
                List<IColorCategory> removeList = new List<IColorCategory>();

                int iPrev = e.RowIndex - 1;
                for (int i = 0; i < e.RowIndex; i++)
                {
                    // If the specified max is below the minima of a lower range, remove the lower range.
                    if (breaks[i].Minimum > cb.Minimum)
                    {
                        removeList.Add(breaks[i]);
                        iPrev--;
                    }
                    else if (breaks[i].Maximum > cb.Minimum || i == iPrev)
                    {
                        // otherwise, if the maximum of a lower range is higher than the value, adjust it.
                        breaks[i].Maximum = cb.Minimum;
                        breaks[i].ApplyMinMax(_symbolizer.EditorSettings);
                    }
                }
                for (int i = e.RowIndex + 1; i < breaks.Count(); i++)
                {
                    // Since we have just assigned an absolute maximum, any previous categories
                    // that fell above the edited category should be removed.
                    removeList.Add(breaks[i]);
                }
                foreach (IColorCategory brk in removeList)
                {
                    // Do the actual removal.
                    breaks.Remove(brk);
                }
            }
            else if (cb.Range.Minimum == null)
            {
                List<IColorCategory> removeList = new List<IColorCategory>();

                int iNext = e.RowIndex + 1;
                for (int i = e.RowIndex + 1; i < breaks.Count; i++)
                {
                    // If the specified max is below the minima of a lower range, remove the lower range.
                    if (breaks[i].Maximum < cb.Maximum)
                    {
                        removeList.Add(breaks[i]);
                        iNext++;
                    }
                    else if (breaks[i].Minimum < cb.Maximum || i == iNext)
                    {
                        // otherwise, if the maximum of a lower range is higher than the value, adjust it.
                        breaks[i].Minimum = cb.Maximum;
                        breaks[i].ApplyMinMax(_symbolizer.EditorSettings);
                    }
                }
                for (int i = 0; i < e.RowIndex; i++)
                {
                    // Since we have just assigned an absolute minimum, any previous categories
                    // that fell above the edited category should be removed.
                    removeList.Add(breaks[i]);
                }

                foreach (IColorCategory brk in removeList)
                {
                    // Do the actual removal.
                    breaks.Remove(brk);
                }
            }
            else
            {
                // We have two values.  Adjust any above or below that conflict.
                List<IColorCategory> removeList = new List<IColorCategory>();
                int iPrev = e.RowIndex - 1;
                for (int i = 0; i < e.RowIndex; i++)
                {
                    // If the specified max is below the minima of a lower range, remove the lower range.
                    if (breaks[i].Minimum > cb.Minimum)
                    {
                        removeList.Add(breaks[i]);
                        iPrev--;
                    }
                    else if (breaks[i].Maximum > cb.Minimum || i == iPrev)
                    {
                        // otherwise, if the maximum of a lower range is higher than the value, adjust it.
                        breaks[i].Maximum = cb.Minimum;
                        breaks[i].ApplyMinMax(_symbolizer.EditorSettings);
                    }
                }
                int iNext = e.RowIndex + 1;
                for (int i = e.RowIndex + 1; i < breaks.Count; i++)
                {
                    // If the specified max is below the minima of a lower range, remove the lower range.
                    if (breaks[i].Maximum < cb.Maximum)
                    {
                        removeList.Add(breaks[i]);
                        iNext++;
                    }
                    else if (breaks[i].Minimum < cb.Maximum || i == iNext)
                    {
                        // otherwise, if the maximum of a lower range is higher than the value, adjust it.
                        breaks[i].Minimum = cb.Maximum;
                        breaks[i].ApplyMinMax(_symbolizer.EditorSettings);
                    }
                }
                foreach (IColorCategory brk in removeList)
                {
                    // Do the actual removal.
                    breaks.Remove(brk);
                }
            }
            breaks.ResumeEvents();
            _ignoreRefresh = true;
            cmbInterval.SelectedItem = IntervalMethod.Manual.ToString();
            _symbolizer.EditorSettings.IntervalMethod = IntervalMethod.Manual;
            _ignoreRefresh = false;
            UpdateStatistics(false);
            _cleanupTimer.Start();
        }

        private void DgvCategoriesSelectionChanged(object sender, EventArgs e)
        {
            if (breakSliderGraph1 == null) return;
            if (breakSliderGraph1.Breaks == null) return;
            if (dgvCategories.SelectedRows.Count > 0)
            {
                int index = dgvCategories.Rows.IndexOf(dgvCategories.SelectedRows[0]);
                if (breakSliderGraph1.Breaks.Count == 0 || index >= breakSliderGraph1.Breaks.Count) return;
                breakSliderGraph1.SelectBreak(breakSliderGraph1.Breaks[index]);
            }
            else
            {
                breakSliderGraph1.SelectBreak(null);
            }
            breakSliderGraph1.Invalidate();
        }

        /// <summary>
        /// Sets up the Table to work with the specified layer
        /// </summary>
        /// <param name="layer"></param>
        public void Initialize(IRasterLayer layer)
        {
            _originalLayer = layer;
            _newLayer = layer.Copy();
            _symbolizer = layer.Symbolizer;
            _newScheme = _symbolizer.Scheme;
            _originalScheme = (IColorScheme)_symbolizer.Scheme.Clone();
            _raster = layer.DataSet;
            GetSettings();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the Table using the unique values
        /// </summary>
        private void UpdateTable()
        {
            dgvCategories.SuspendLayout();
            dgvCategories.Rows.Clear();

            ColorCategoryCollection breaks = _newScheme.Categories;
            int i = 0;
            if (breaks.Count > 0)
            {
                dgvCategories.Rows.Add(breaks.Count);
                foreach (IColorCategory brk in breaks)
                {
                    dgvCategories[1, i].Value = brk.Range.ToString(_symbolizer.EditorSettings.IntervalSnapMethod,
                                                                   _symbolizer.EditorSettings.IntervalRoundingDigits);
                    dgvCategories[2, i].Value = brk.LegendText;
                    i++;
                }
            }
            dgvCategories.ResumeLayout();
            dgvCategories.Invalidate();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current progress bar.
        /// </summary>
        public SymbologyProgressBar ProgressBar
        {
            get { return mwProgressBar1; }
        }

        /// <summary>
        /// Gets or sets the Maximum value currently displayed in the graph.
        /// </summary>
        public double Maximum
        {
            get { return _maximum; }
            set { _maximum = value; }
        }

        /// <summary>
        /// Gets or sets the Minimum value currently displayed in the graph.
        /// </summary>
        public double Minimum
        {
            get { return _minimum; }
            set { _minimum = value; }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Fires the apply changes situation externally, forcing the Table to
        /// write its values to the original layer.
        /// </summary>
        public void ApplyChanges()
        {
            OnApplyChanges();
        }

        /// <summary>
        /// Applies the changes that have been specified in this control
        /// </summary>
        protected virtual void OnApplyChanges()
        {
            _originalLayer.Symbolizer = _newLayer.Symbolizer.Copy();
            _originalScheme = _newLayer.Symbolizer.Scheme.Copy();
            if (_originalLayer.Symbolizer.ShadedRelief.IsUsed)
            {
                if (_originalLayer.Symbolizer.ShadedRelief.HasChanged || _originalLayer.Symbolizer.HillShade == null)
                    _originalLayer.Symbolizer.CreateHillShade(mwProgressBar1);
            }
            _originalLayer.WriteBitmap(mwProgressBar1);
            if (ChangesApplied != null) ChangesApplied(_originalLayer, EventArgs.Empty);
        }

        /// <summary>
        /// Cancel the action.
        /// </summary>
        public void Cancel()
        {
            OnCancel();
        }

        /// <summary>
        /// Event that fires when the action is canceled.
        /// </summary>
        protected virtual void OnCancel()
        {
            _originalLayer.Symbolizer.Scheme = _originalScheme;
        }

        private void RefreshValues()
        {
            if (_ignoreRefresh) return;
            SetSettings();
            _newScheme.CreateCategories(_raster);
            UpdateTable();
            UpdateStatistics(false); // if the parameter is true, even on manual, the breaks are reset.
            breakSliderGraph1.Invalidate();
        }

        /// <summary>
        /// When the user double clicks the cell then we should display the detailed
        /// symbology dialog
        /// </summary>
        private void DgvCategoriesCellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int count = _newScheme.Categories.Count;
            if (e.ColumnIndex == 0 && e.RowIndex < count)
            {
                _dblClickEditIndex = e.RowIndex;
                _tabColorDialog = new TabColorDialog();
                _tabColorDialog.ChangesApplied += TabColorDialogChangesApplied;
                _tabColorDialog.StartColor = _newScheme.Categories[_dblClickEditIndex].LowColor;
                _tabColorDialog.EndColor = _newScheme.Categories[_dblClickEditIndex].HighColor;
                _tabColorDialog.Show(ParentForm);
            }
        }

        private void TabColorDialogChangesApplied(object sender, EventArgs e)
        {
            if (_newScheme == null) return;
            if (_newScheme.Categories == null) return;
            if (_dblClickEditIndex < 0 || _dblClickEditIndex > _newScheme.Categories.Count) return;
            _newScheme.Categories[_dblClickEditIndex].LowColor = _tabColorDialog.StartColor;
            _newScheme.Categories[_dblClickEditIndex].HighColor = _tabColorDialog.EndColor;
            UpdateTable();
        }

        /// <summary>
        /// When the cell is formatted
        /// </summary>
        private void DgvCategoriesCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_newScheme == null) return;
            int count = _newScheme.Categories.Count;
            if (count == 0) return;

            // Replace string values in the column with images.
            if (e.ColumnIndex != 0) return;
            Image img = e.Value as Image;
            if (img == null)
            {
                img = SymbologyFormsImages.info;
                e.Value = img;
            }
            Graphics g = Graphics.FromImage(img);
            g.Clear(Color.White);

            if (count > e.RowIndex)
            {
                Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);
                _newScheme.DrawCategory(e.RowIndex, g, rect);
            }
            g.Dispose();
        }

        #endregion

        #region ICategoryControl Members

        /// <summary>
        /// Initializes the specified layer.
        /// </summary>
        /// <param name="layer">The layer.</param>
        public void Initialize(ILayer layer)
        {
            Initialize(layer as IRasterLayer);
        }

        #endregion

        private void cmdRefresh_Click(object sender, EventArgs e)
        {
            _symbolizer.EditorSettings.RampColors = false;
            RefreshValues();
        }

        private void btnRamp_Click(object sender, EventArgs e)
        {
            _symbolizer.EditorSettings.RampColors = true;
            RefreshValues();
        }

        private void GetSettings()
        {
            _ignoreRefresh = true;
            EditorSettings settings = _symbolizer.EditorSettings;
            tccColorRange.Initialize(new ColorRangeEventArgs(settings.StartColor, settings.EndColor, settings.HueShift, settings.HueSatLight, settings.UseColorRange));
            UpdateTable();
            cmbInterval.SelectedItem = settings.IntervalMethod.ToString();
            UpdateStatistics(false);
            nudCategoryCount.Value = _newScheme.EditorSettings.NumBreaks;
            cmbIntervalSnapping.SelectedItem = settings.IntervalSnapMethod;
            nudSigFig.Value = settings.IntervalRoundingDigits;
            angLightDirection.Angle = (int)_symbolizer.ShadedRelief.LightDirection;
            dbxElevationFactor.Value = _symbolizer.ShadedRelief.ElevationFactor;
            chkHillshade.Checked = _symbolizer.ShadedRelief.IsUsed;
            colorNoData.Color = _symbolizer.NoDataColor;
            opacityNoData.Value = _symbolizer.NoDataColor.GetOpacity();
            sldSchemeOpacity.Value = _symbolizer.Opacity;
            _ignoreRefresh = false;
        }

        private void SetSettings()
        {
            if (_ignoreRefresh) return;
            EditorSettings settings = _symbolizer.EditorSettings;
            settings.NumBreaks = (int)nudCategoryCount.Value;
            settings.IntervalSnapMethod = (IntervalSnapMethod)cmbIntervalSnapping.SelectedItem;
            settings.IntervalRoundingDigits = (int)nudSigFig.Value;
        }

        private void UpdateStatistics(bool clear)
        {
            // Graph
            SetSettings();
            breakSliderGraph1.RasterLayer = _newLayer;
            breakSliderGraph1.Title = _newLayer.LegendText;
            breakSliderGraph1.ResetExtents();
            if (_symbolizer.EditorSettings.IntervalMethod == IntervalMethod.Manual && !clear)
            {
                breakSliderGraph1.UpdateBreaks();
            }
            else
            {
                breakSliderGraph1.ResetBreaks(null);
            }
            Statistics stats = breakSliderGraph1.Statistics;

            // Stat list
            dgvStatistics.Rows.Clear();
            dgvStatistics.Rows.Add(7);
            dgvStatistics[0, 0].Value = "Count";
            dgvStatistics[1, 0].Value = _raster.NumValueCells.ToString("#, ###");
            dgvStatistics[0, 1].Value = "Min";
            dgvStatistics[1, 1].Value = _raster.Minimum.ToString("#, ###");
            dgvStatistics[0, 2].Value = "Max";
            dgvStatistics[1, 2].Value = _raster.Maximum.ToString("#, ###");
            dgvStatistics[0, 3].Value = "Sum";
            dgvStatistics[1, 3].Value = (_raster.Mean * _raster.NumValueCells).ToString("#, ###");
            dgvStatistics[0, 4].Value = "Mean";
            dgvStatistics[1, 4].Value = _raster.Mean.ToString("#, ###");
            dgvStatistics[0, 5].Value = "Median";
            dgvStatistics[1, 5].Value = stats.Median.ToString("#, ###");
            dgvStatistics[0, 6].Value = "Std";
            dgvStatistics[1, 6].Value = stats.StandardDeviation.ToString("#, ###");
        }

        private void nudCategoryCount_ValueChanged(object sender, EventArgs e)
        {
            if (_ignoreRefresh) return;
            _ignoreRefresh = true;
            cmbInterval.SelectedItem = IntervalMethod.EqualInterval.ToString();
            _ignoreRefresh = false;
            RefreshValues();
        }

        private void chkLog_CheckedChanged(object sender, EventArgs e)
        {
            breakSliderGraph1.LogY = chkLog.Checked;
        }

        private void chkShowMean_CheckedChanged(object sender, EventArgs e)
        {
            breakSliderGraph1.ShowMean = chkShowMean.Checked;
        }

        private void chkShowStd_CheckedChanged(object sender, EventArgs e)
        {
            breakSliderGraph1.ShowStandardDeviation = chkShowStd.Checked;
        }

        private void cmbInterval_SelectedIndexChanged(object sender, EventArgs e)
        {
            IntervalMethod method = (IntervalMethod)Enum.Parse(typeof(IntervalMethod), cmbInterval.SelectedItem.ToString());
            if (_symbolizer == null) return;
            _symbolizer.EditorSettings.IntervalMethod = method;
            RefreshValues();
        }

        private void breakSliderGraph1_SliderMoved(object sender, BreakSliderEventArgs e)
        {
            _ignoreRefresh = true;
            cmbInterval.SelectedItem = "Manual";
            _ignoreRefresh = false;
            _symbolizer.EditorSettings.IntervalMethod = IntervalMethod.Manual;
            int index = _newScheme.Categories.IndexOf(e.Slider.Category as IColorCategory);
            if (index == -1) return;
            UpdateTable();
            dgvCategories.Rows[index].Selected = true;
        }

        private void nudColumns_ValueChanged(object sender, EventArgs e)
        {
            breakSliderGraph1.NumColumns = (int)nudColumns.Value;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            nudCategoryCount.Value += 1;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCategories.SelectedRows.Count == 0) return;
            List<IColorCategory> deleteList = new List<IColorCategory>();
            ColorCategoryCollection categories = _newScheme.Categories;
            int count = 0;
            foreach (DataGridViewRow row in dgvCategories.SelectedRows)
            {
                int index = dgvCategories.Rows.IndexOf(row);
                deleteList.Add(categories[index]);
                count++;
            }
            foreach (IColorCategory category in deleteList)
            {
                int index = categories.IndexOf(category);
                if (index > 0 && index < categories.Count - 1)
                {
                    categories[index - 1].Maximum = categories[index + 1].Minimum;
                    categories[index - 1].ApplyMinMax(_newScheme.EditorSettings);
                }
                _newScheme.RemoveCategory(category);
                breakSliderGraph1.UpdateBreaks();
            }
            UpdateTable();
            _newScheme.EditorSettings.IntervalMethod = IntervalMethod.Manual;
            _newScheme.EditorSettings.NumBreaks -= count;
            UpdateStatistics(false);
        }

        private void nudSigFig_ValueChanged(object sender, EventArgs e)
        {
            if (_newScheme == null) return;
            _newScheme.EditorSettings.IntervalRoundingDigits = (int)nudSigFig.Value;

            RefreshValues();
        }

        private void cmbIntervalSnapping_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_newScheme == null) return;
            IntervalSnapMethod method = (IntervalSnapMethod)cmbIntervalSnapping.SelectedItem;
            _newScheme.EditorSettings.IntervalSnapMethod = method;
            switch (method)
            {
                case IntervalSnapMethod.SignificantFigures:
                    lblSigFig.Visible = true;
                    nudSigFig.Visible = true;
                    nudSigFig.Minimum = 1;
                    lblSigFig.Text = "Significant Figures:";
                    break;
                case IntervalSnapMethod.Rounding:
                    nudSigFig.Visible = true;
                    lblSigFig.Visible = true;
                    nudSigFig.Minimum = 0;
                    lblSigFig.Text = "Rounding Digits:";
                    break;
                case IntervalSnapMethod.None:
                    lblSigFig.Visible = false;
                    nudSigFig.Visible = false;
                    break;
                case IntervalSnapMethod.DataValue:
                    lblSigFig.Visible = false;
                    nudSigFig.Visible = false;
                    break;
            }

            RefreshValues();
        }

        private void tccColorRange_ColorChanged(object sender, ColorRangeEventArgs e)
        {
            if (_ignoreRefresh) return;
            RasterEditorSettings settings = _newScheme.EditorSettings;
            settings.StartColor = e.StartColor;
            settings.EndColor = e.EndColor;
            settings.UseColorRange = e.UseColorRange;
            settings.HueShift = e.HueShift;
            settings.HueSatLight = e.HSL;
            RefreshValues();
        }

        private void btnQuick_Click(object sender, EventArgs e)
        {
            _quickSchemes.Show(btnQuick, new Point(0, 0));
        }

        private void QuickSchemeClicked(object sender, EventArgs e)
        {
            _ignoreRefresh = true;
            _newScheme.EditorSettings.NumBreaks = 2;
            nudCategoryCount.Value = 2;
            _ignoreRefresh = false;
            MenuItem mi = sender as MenuItem;
            if (mi == null) return;
            ColorSchemeType cs = (ColorSchemeType)Enum.Parse(typeof(ColorSchemeType), mi.Text);
            _newScheme.ApplyScheme(cs, _raster);
            UpdateTable();
            UpdateStatistics(true); // if the parameter is true, even on manual, the breaks are reset.
            breakSliderGraph1.Invalidate();
        }

        private void chkHillshade_CheckedChanged(object sender, EventArgs e)
        {
            _symbolizer.ShadedRelief.IsUsed = chkHillshade.Checked;
        }

        private void btnShadedRelief_Click(object sender, EventArgs e)
        {
            _shadedReliefDialog.PropertyGrid.SelectedObject = _symbolizer.ShadedRelief.Copy();
            _shadedReliefDialog.ShowDialog();
        }

        private void PropertyDialogChangesApplied(object sender, EventArgs e)
        {
            _symbolizer.ShadedRelief = (_shadedReliefDialog.PropertyGrid.SelectedObject as IShadedRelief).Copy();
            angLightDirection.Angle = (int)_symbolizer.ShadedRelief.LightDirection;
            dbxElevationFactor.Value = _symbolizer.ShadedRelief.ElevationFactor;
        }

        private void angLightDirection_AngleChanged(object sender, EventArgs e)
        {
            _symbolizer.ShadedRelief.LightDirection = angLightDirection.Angle;
        }

        private void dbxElevationFactor_TextChanged(object sender, EventArgs e)
        {
            _symbolizer.ShadedRelief.ElevationFactor = (float)dbxElevationFactor.Value;
        }

        private void dbxMin_TextChanged(object sender, EventArgs e)
        {
            _symbolizer.EditorSettings.Min = dbxMin.Value;
            _symbolizer.Scheme.CreateCategories(_raster);
            UpdateStatistics(true); // if the parameter is true, even on manual, the breaks are reset.
            UpdateTable();
        }

        private void dbxMax_TextChanged(object sender, EventArgs e)
        {
            _symbolizer.EditorSettings.Max = dbxMax.Value;
            _symbolizer.Scheme.CreateCategories(_raster);
            UpdateStatistics(true); // if the parameter is true, even on manual, the breaks are reset.
            UpdateTable();
        }

        private void btnElevation_Click(object sender, EventArgs e)
        {
            _elevationQuickPick.Show(grpHillshade, new Point(dbxElevationFactor.Left, btnElevation.Bottom));
        }

        private void sldSchemeOpacity_ValueChanged(object sender, EventArgs e)
        {
            if (_ignoreRefresh) return;
            _symbolizer.Opacity = Convert.ToSingle(sldSchemeOpacity.Value);
            foreach (var cat in _symbolizer.Scheme.Categories)
            {
                cat.HighColor = cat.HighColor.ToTransparent(_symbolizer.Opacity);
                cat.LowColor = cat.LowColor.ToTransparent(_symbolizer.Opacity);
            }
            dgvCategories.Invalidate();
        }

        private void colorNoData_ColorChanged(object sender, EventArgs e)
        {
            _symbolizer.NoDataColor = colorNoData.Color;
        }
    }
}