// ********************************************************************************************************
// Product Name: CategoriesViewer.dll Alpha
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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DotSpatial.Data;
using DotSpatial.Topology;
using Point = System.Drawing.Point;

namespace DotSpatial.Symbology.Forms
{
    /// <summary>
    /// Dialog for the 'unique values' feature symbol classification scheme
    /// </summary>
    public partial class FeatureCategoryControl : UserControl, ICategoryControl
    {
        #region Events

        /// <summary>
        /// Occurs when the apply changes option has been triggered.
        /// </summary>
        public event EventHandler ChangesApplied;

        #endregion

        #region Private Variables

        readonly SQLExpressionDialog _expressionDialog = new SQLExpressionDialog();
        private int _activeCategoryIndex;
        private bool _expressionIsExclude;
        private bool _ignoreEnter;
        private bool _ignoreRefresh;
        private bool _ignoreValidation;
        private double _maximum;
        private double _minimum;
        private IFeatureScheme _newScheme;  //the local copy of the scheme
        private IFeatureScheme _original; //the original scheme which is modified only after clicking 'Apply'
        private SymbolizerType _schemeType;  //used to distinquish between a line, point and polygon scheme
        private IFeatureSet _source;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty FeatureCategoryControl without specifying any particular layer to use
        /// </summary>
        public FeatureCategoryControl()
        {
            InitializeComponent();
            Configure();
        }

        /// <summary>
        /// Creates a new instance of the unique values category Table
        /// </summary>
        /// <param name="layer">The original scheme</param>
        public FeatureCategoryControl(IFeatureLayer layer)
        {
            InitializeComponent();
            Configure();
            Initialize(layer);
        }

        /// <summary>
        /// Handles the mouse wheel, allowing the breakSldierGraph to zoom in or out.
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
            _cancelDialog = new ProgressCancelDialog();
            dgvCategories.CellFormatting += DgvCategoriesCellFormatting;
            dgvCategories.CellDoubleClick += DgvCategoriesCellDoubleClick;
            dgvCategories.SelectionChanged += DgvCategoriesSelectionChanged;
            dgvCategories.CellValidated += DgvCategoriesCellValidated;
            dgvCategories.MouseDown += DgvCategoriesMouseDown;
            dgvCategories.Height = 456;
            cmbInterval.SelectedItem = "EqualInterval";
            tabScheme.Visible = false;
            _expressionDialog.ChangesApplied += ExpressionDialogChangesApplied;
            breakSliderGraph1.SliderSelected += BreakSliderGraph1SliderSelected;
            btnDown.Enabled = false;
            btnUp.Enabled = false;
            btnAdd.Enabled = false;
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
        }

        private void DgvCategoriesMouseDown(object sender, MouseEventArgs e)
        {
            if (_ignoreEnter) return;
            _activeCategoryIndex = dgvCategories.HitTest(e.X, e.Y).RowIndex;
        }

        private void CleanupTimerTick(object sender, EventArgs e)
        {
            // When a row validation causes rows above the edit row to be removed,
            // we can't easilly update the Table during the validation event.
            // The timer allows the validation to finish before updating the Table.
            _cleanupTimer.Stop();
            _ignoreValidation = true;
            _cancelDialog.Show(ParentForm);
            UpdateTable(_cancelDialog);
            _cancelDialog.Hide();
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
            if (e.ColumnIndex == 2)
            {
                List<IFeatureCategory> cats = _newScheme.GetCategories().ToList();
                IFeatureCategory fctxt = cats[e.RowIndex];
                fctxt.LegendText = (string)dgvCategories[e.ColumnIndex, e.RowIndex].Value;
                return;
            }

            if (e.ColumnIndex != 1) return;

            List<IFeatureCategory> categories = _newScheme.GetCategories().ToList();
            if (categories.Count < e.RowIndex)
            {
                IFeatureCategory fc = categories[e.RowIndex];

                if ((string)dgvCategories[e.ColumnIndex, e.RowIndex].Value == fc.LegendText) return;
                _ignoreEnter = true;
                string exp = (string)dgvCategories[e.ColumnIndex, e.RowIndex].Value;
                fc.LegendText = exp;
                if (_newScheme.EditorSettings.ClassificationType == ClassificationType.Quantities)
                {
                    fc.Range = new Range(exp);
                    if (fc.Range.Maximum != null && fc.Range.Maximum > _newScheme.Statistics.Maximum)
                    {
                        fc.Range.Maximum = _newScheme.Statistics.Maximum;
                    }
                    if (fc.Range.Minimum != null && fc.Range.Minimum > _newScheme.Statistics.Maximum)
                    {
                        fc.Range.Minimum = _newScheme.Statistics.Maximum;
                    }
                    if (fc.Range.Maximum != null && fc.Range.Minimum < _newScheme.Statistics.Minimum)
                    {
                        fc.Range.Minimum = _newScheme.Statistics.Minimum;
                    }
                    if (fc.Range.Minimum != null && fc.Range.Minimum < _newScheme.Statistics.Minimum)
                    {
                        fc.Range.Minimum = _newScheme.Statistics.Minimum;
                    }
                    fc.ApplyMinMax(_newScheme.EditorSettings);
                    if (fc.Range.Minimum == null && fc.Range.Maximum == null)
                    {
                        _newScheme.ClearCategories();
                        _newScheme.AddCategory(fc);
                    }
                    else if (fc.Range.Maximum == null)
                    {
                        List<IFeatureCategory> removeList = new List<IFeatureCategory>();

                        int iPrev = e.RowIndex - 1;
                        for (int i = 0; i < e.RowIndex; i++)
                        {
                            // If the specified max is below the minima of a lower range, remove the lower range.
                            if (categories[i].Minimum > fc.Minimum)
                            {
                                removeList.Add(categories[i]);
                                iPrev--;
                            }
                            else if (categories[i].Maximum > fc.Minimum || i == iPrev)
                            {
                                // otherwise, if the maximum of a lower range is higher than the value, adjust it.
                                categories[i].Maximum = fc.Minimum;
                                categories[i].ApplyMinMax(_newScheme.EditorSettings);
                            }
                        }
                        for (int i = e.RowIndex + 1; i < categories.Count(); i++)
                        {
                            // Since we have just assigned an absolute maximum, any previous categories
                            // that fell above the edited category should be removed.
                            removeList.Add(categories[i]);
                        }
                        foreach (IFeatureCategory category in removeList)
                        {
                            // Do the actual removal.
                            _newScheme.RemoveCategory(category);
                        }
                    }
                    else if (fc.Range.Minimum == null)
                    {
                        List<IFeatureCategory> removeList = new List<IFeatureCategory>();

                        int iNext = e.RowIndex + 1;
                        for (int i = e.RowIndex + 1; i < categories.Count; i++)
                        {
                            // If the specified max is below the minima of a lower range, remove the lower range.
                            if (categories[i].Maximum < fc.Maximum)
                            {
                                removeList.Add(categories[i]);
                                iNext++;
                            }
                            else if (categories[i].Minimum < fc.Maximum || i == iNext)
                            {
                                // otherwise, if the maximum of a lower range is higher than the value, adjust it.
                                categories[i].Minimum = fc.Maximum;
                                categories[i].ApplyMinMax(_newScheme.EditorSettings);
                            }
                        }
                        for (int i = 0; i < e.RowIndex; i++)
                        {
                            // Since we have just assigned an absolute minimum, any previous categories
                            // that fell above the edited category should be removed.
                            removeList.Add(categories[i]);
                        }
                        foreach (IFeatureCategory category in removeList)
                        {
                            // Do the actual removal.
                            _newScheme.RemoveCategory(category);
                        }
                    }
                    else
                    {
                        // We have two values.  Adjust any above or below that conflict.
                        List<IFeatureCategory> removeList = new List<IFeatureCategory>();
                        int iPrev = e.RowIndex - 1;
                        for (int i = 0; i < e.RowIndex; i++)
                        {
                            // If the specified max is below the minima of a lower range, remove the lower range.
                            if (categories[i].Minimum > fc.Minimum)
                            {
                                removeList.Add(categories[i]);
                                iPrev--;
                            }
                            else if (categories[i].Maximum > fc.Minimum || i == iPrev)
                            {
                                // otherwise, if the maximum of a lower range is higher than the value, adjust it.
                                categories[i].Maximum = fc.Minimum;
                                categories[i].ApplyMinMax(_newScheme.EditorSettings);
                            }
                        }
                        int iNext = e.RowIndex + 1;
                        for (int i = e.RowIndex + 1; i < categories.Count; i++)
                        {
                            // If the specified max is below the minima of a lower range, remove the lower range.
                            if (categories[i].Maximum < fc.Maximum)
                            {
                                removeList.Add(categories[i]);
                                iNext++;
                            }
                            else if (categories[i].Minimum < fc.Maximum || i == iNext)
                            {
                                // otherwise, if the maximum of a lower range is higher than the value, adjust it.
                                categories[i].Minimum = fc.Maximum;
                                categories[i].ApplyMinMax(_newScheme.EditorSettings);
                            }
                        }
                        foreach (IFeatureCategory category in removeList)
                        {
                            // Do the actual removal.
                            _newScheme.RemoveCategory(category);
                        }
                    }
                    _ignoreRefresh = true;
                    cmbInterval.SelectedItem = IntervalMethod.Manual.ToString();
                    _newScheme.EditorSettings.IntervalMethod = IntervalMethod.Manual;
                    _ignoreRefresh = false;
                    UpdateStatistics(false, null);
                    _cleanupTimer.Start();
                }
                else
                {
                    List<IFeatureCategory> cats = _newScheme.GetCategories().ToList();
                    IFeatureCategory fctxt = cats[e.RowIndex];
                    fctxt.FilterExpression = (string)dgvCategories[e.ColumnIndex, e.RowIndex].Value;
                    return;
                }
            }
        }

        private void DgvCategoriesSelectionChanged(object sender, EventArgs e)
        {
            if (breakSliderGraph1 == null) return;
            if (breakSliderGraph1.Breaks == null) return;
            if (_newScheme.EditorSettings.ClassificationType != ClassificationType.Quantities) return;
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
        /// Sets up the Table to work with the specified layer.  This should be the copy,
        /// and not the original.
        /// </summary>
        /// <param name="layer"></param>
        public void Initialize(IFeatureLayer layer)
        {
            _original = layer.Symbology;
            _newScheme = _original.Copy();
            _source = layer.DataSet;
            if (!layer.DataSet.AttributesPopulated)
            {
                if (layer.DataSet.NumRows() < 100000)
                {
                    _source.FillAttributes(); // for small datasets, it is better to just load and cache it.
                }
            }

            if (_source.AttributesPopulated)
            {
                _expressionDialog.Table = _source.DataTable;
            }
            else
            {
                _expressionDialog.AttributeSource = _source;
            }

            _schemeType = GetSchemeType(layer);

            if (_schemeType != SymbolizerType.Polygon)
            {
                chkUseGradients.Visible = false;
                angGradientAngle.Visible = false;
            }
            else
            {
                chkUseGradients.Visible = true;
                angGradientAngle.Visible = true;
            }
            if (_schemeType == SymbolizerType.Point)
            {
                IPointScheme ps = _newScheme as IPointScheme;

                if (ps != null)
                {
                    IPointSymbolizer sym;
                    if (ps.Categories.Count == 0 || ps.Categories[0].Symbolizer == null)
                    {
                        sym = new PointSymbolizer();
                    }
                    else
                    {
                        sym = ps.Categories[0].Symbolizer;
                    }
                    _ignoreRefresh = true;
                    featureSizeRangeControl1.SizeRange = new FeatureSizeRange(sym, _newScheme.EditorSettings.StartSize, _newScheme.EditorSettings.EndSize);
                    featureSizeRangeControl1.Initialize(new SizeRangeEventArgs(_newScheme.EditorSettings.StartSize, _newScheme.EditorSettings.EndSize, sym, _newScheme.EditorSettings.UseSizeRange));
                    featureSizeRangeControl1.Scheme = ps;
                    featureSizeRangeControl1.Visible = true;
                    _ignoreRefresh = false;
                }
            }
            else if (_schemeType == SymbolizerType.Line)
            {
                ILineScheme ls = _newScheme as ILineScheme;
                if (ls != null)
                {
                    ILineSymbolizer sym;
                    if (ls.Categories.Count == 0 || ls.Categories[0].Symbolizer == null)
                    {
                        sym = new LineSymbolizer();
                    }
                    else
                    {
                        sym = ls.Categories[0].Symbolizer;
                    }
                    _ignoreRefresh = true;
                    featureSizeRangeControl1.SizeRange = new FeatureSizeRange(sym, _newScheme.EditorSettings.StartSize, _newScheme.EditorSettings.EndSize);
                    featureSizeRangeControl1.Initialize(new SizeRangeEventArgs(_newScheme.EditorSettings.StartSize, _newScheme.EditorSettings.EndSize, sym, _newScheme.EditorSettings.UseSizeRange));
                    featureSizeRangeControl1.Scheme = ls;
                    featureSizeRangeControl1.Visible = true;
                    _ignoreRefresh = false;
                }
            }
            else
            {
                featureSizeRangeControl1.Visible = false;
            }

            UpdateFields();
            if (_newScheme.EditorSettings.ClassificationType != ClassificationType.Quantities) return;
            nudCategoryCount.Enabled = true;
            tabScheme.Visible = true;
            dgvCategories.Height = 217;
            UpdateStatistics(false, null);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the scheme type based on the type of the layer's underlying dataset
        /// </summary>
        private static SymbolizerType GetSchemeType(IFeatureLayer layer)
        {
            FeatureType fType = layer.DataSet.FeatureType;

            switch (fType)
            {
                case FeatureType.Point:
                    return SymbolizerType.Point;
                case FeatureType.MultiPoint:
                    return SymbolizerType.Point;
                case FeatureType.Line:
                    return SymbolizerType.Line;
                case FeatureType.Polygon:
                    return SymbolizerType.Polygon;
                default:
                    return SymbolizerType.Unknown;
            }
        }

        /// <summary>
        /// Updates the fields in the fields combo box
        /// </summary>
        private void UpdateFields()
        {
            _cmbField.SuspendLayout();
            cmbNormField.SuspendLayout();
            _cmbField.Items.Clear();
            cmbNormField.Items.Clear();
            cmbNormField.Items.Add("(None)");
            DataColumn[] columns = _source.GetColumns();
            foreach (DataColumn dc in columns)
            {
                _cmbField.Items.Add(dc.ColumnName);
                if (dc.DataType == typeof(string)) continue;
                if (dc.DataType == typeof(bool)) continue;
                cmbNormField.Items.Add(dc.ColumnName);
            }
            _cmbField.ResumeLayout();
            cmbNormField.ResumeLayout();
            GetSettings();
        }

        private void UpdateTable()
        {
            UpdateTable(null);
        }

        /// <summary>
        /// Updates the Table using the unique values
        /// </summary>
        private void UpdateTable(ICancelProgressHandler handler)
        {
            dgvCategories.SuspendLayout();
            dgvCategories.Rows.Clear();

            List<IFeatureCategory> categories = _newScheme.GetCategories().ToList();
            int i = 0;
            if (categories.Count > 0)
            {
                dgvCategories.Rows.Add(categories.Count);
                string[] expressions = new string[categories.Count];
                for (int iCat = 0; iCat < categories.Count; iCat++)
                {
                    IFeatureCategory category = categories[iCat];
                    string exp = category.FilterExpression;
                    string excluded = _newScheme.EditorSettings.ExcludeExpression;
                    if (!string.IsNullOrEmpty(excluded))
                    {
                        exp = exp + " AND NOT ( " + _newScheme.EditorSettings.ExcludeExpression + ")";
                    }
                    expressions[iCat] = exp;
                }

                int[] counts = _source.GetCounts(expressions, handler, _newScheme.EditorSettings.MaxSampleCount);

                foreach (IFeatureCategory category in categories)
                {
                    IPointCategory pc = category as IPointCategory;
                    if (pc != null)
                    {
                        Size2D sz = pc.Symbolizer.GetSize();
                        int w = Convert.ToInt32(sz.Width);
                        if (w < 20) w = 20;
                        if (w > 128) w = 128;
                        int h = Convert.ToInt32(sz.Height);
                        if (h < 20) h = 20;
                        if (h > 128) h = 128;
                        dgvCategories[0, i].Value = new Bitmap(w, h);
                        dgvCategories.Rows[i].Height = h;
                    }
                    if (_newScheme.EditorSettings.ClassificationType == ClassificationType.Quantities)
                    {
                        if (category.Range != null)
                        {
                            dgvCategories[1, i].Value = category.Range.ToString();
                        }
                    }
                    else if (_newScheme.EditorSettings.ClassificationType == ClassificationType.UniqueValues)
                    {
                        dgvCategories[1, i].Value = category.LegendText;
                    }
                    else
                    {
                        dgvCategories[1, i].Value = category.FilterExpression;
                    }

                    dgvCategories[2, i].Value = category.LegendText;
                    dgvCategories[3, i].Value = counts[i];
                    i++;
                }
            }
            dgvCategories.ResumeLayout();
            dgvCategories.Invalidate();
        }

        #endregion

        #region Properties

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
            // SetSettings(); When applying a scheme settings are set, so don't bother here.
            _original.CopyProperties(_newScheme);
            _original.ResumeEvents();

            if (ChangesApplied != null) ChangesApplied(_original, EventArgs.Empty);
        }

        /// <summary>
        /// Cancel the action.
        /// </summary>
        public void Cancel()
        {
            //Do nothing;
        }

        /// <summary>
        /// When the user changes the selected attribute field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbField_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshValues();
        }

        private void RefreshValues()
        {
            if (_ignoreRefresh) return;
            SetSettings();

            if (_newScheme.EditorSettings.ClassificationType == ClassificationType.Custom || _cmbField.SelectedItem == null)
            {
                _newScheme.RegenerateColors();
                if (!_source.AttributesPopulated)
                {
                    _cancelDialog.Show(ParentForm);
                    UpdateTable(_cancelDialog);
                    _cancelDialog.Hide();
                }
                else
                {
                    UpdateTable(_cancelDialog);
                }

                return;
            }
            if (_source.AttributesPopulated)
            {
                _newScheme.CreateCategories(_source.DataTable);
                UpdateTable();
                UpdateStatistics(true, null); // if the parameter is true, even on manual, the breaks are reset.
            }
            else
            {
                _cancelDialog.Show(ParentForm);
                _newScheme.CreateCategories(_source, _cancelDialog);
                UpdateTable(_cancelDialog);
                UpdateStatistics(true, _cancelDialog);
                _cancelDialog.Hide();
            }

            breakSliderGraph1.Invalidate();
            Application.DoEvents();
        }

        /// <summary>
        /// When the user double clicks the cell then we should display the detailed
        /// symbology dialog
        /// </summary>
        private void DgvCategoriesCellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int count = _newScheme.GetCategories().Count();
            if (e.ColumnIndex == 0 && e.RowIndex < count)
            {
                if (_schemeType == SymbolizerType.Point)
                {
                    IPointScheme ps = _newScheme as IPointScheme;
                    if (ps != null)
                    {
                        IPointSymbolizer pointSym = ps.Categories[e.RowIndex].Symbolizer;
                        DetailedPointSymbolDialog dlg = new DetailedPointSymbolDialog(pointSym);
                        dlg.ShowDialog();
                        IPointSymbolizer selPoint = pointSym.Copy();
                        selPoint.SetFillColor(Color.Cyan);
                        ps.Categories[e.RowIndex].SelectionSymbolizer = selPoint;
                    }
                }
                else if (_schemeType == SymbolizerType.Line)
                {
                    ILineScheme ls = _newScheme as ILineScheme;
                    if (ls != null)
                    {
                        ILineSymbolizer lineSym = ls.Categories[e.RowIndex].Symbolizer;
                        DetailedLineSymbolDialog dlg = new DetailedLineSymbolDialog(lineSym);
                        dlg.ShowDialog();
                        ILineSymbolizer selLine = lineSym.Copy();
                        selLine.SetFillColor(Color.Cyan);
                        ls.Categories[e.RowIndex].SelectionSymbolizer = selLine;
                    }
                }
                else if (_schemeType == SymbolizerType.Polygon)
                {
                    IPolygonScheme ps = _newScheme as IPolygonScheme;
                    if (ps != null)
                    {
                        IPolygonSymbolizer polySym = ps.Categories[e.RowIndex].Symbolizer;
                        DetailedPolygonSymbolDialog dlg = new DetailedPolygonSymbolDialog(polySym);
                        dlg.ShowDialog();
                        IPolygonSymbolizer selPoly = polySym.Copy();
                        selPoly.SetFillColor(Color.Cyan);
                        selPoly.OutlineSymbolizer.SetFillColor(Color.DarkCyan);
                        ps.Categories[e.RowIndex].SelectionSymbolizer = selPoly;
                    }
                }
            }
            else if (e.ColumnIndex == 1 && e.RowIndex < count)
            {
                if (_newScheme.EditorSettings.ClassificationType != ClassificationType.Custom)
                {
                    MessageBox.Show(SymbologyFormsMessageStrings.FeatureCategoryControl_CustomOnly);
                    return;
                }
                _expressionIsExclude = false;
                List<IFeatureCategory> cats = _newScheme.GetCategories().ToList();
                _expressionDialog.Expression = cats[e.RowIndex].FilterExpression;
                _expressionDialog.ShowDialog(this);
            }
        }

        /// <summary>
        /// When the cell is formatted
        /// </summary>
        private void DgvCategoriesCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_newScheme == null) return;
            int count = _newScheme.GetCategories().Count();
            if (count == 0) return;

            // Replace string values in the column with images.
            if (e.ColumnIndex != 0) return;

            if (e.Value == null)
            {
                e.Value = new Bitmap(16, 16);
            }

            Image img = e.Value as Image;
            if (img == null) return;

            Graphics g = Graphics.FromImage(img);
            g.Clear(Color.White);

            if (count > e.RowIndex)
            {
                IPointScheme ps = _newScheme as IPointScheme;
                if (ps != null)
                {
                    Size2D sz = ps.Categories[e.RowIndex].Symbolizer.GetSize();
                    int w = (int)sz.Width;
                    int h = (int)sz.Height;
                    if (w > 128) w = 128;
                    if (w < 1) w = 1;
                    if (h > 128) h = 128;
                    if (h < 1) h = 1;
                    int x = img.Width / 2 - w / 2;
                    int y = img.Height / 2 - h / 2;
                    Rectangle rect = new Rectangle(x, y, w, h);
                    _newScheme.DrawCategory(e.RowIndex, g, rect);
                }
                else
                {
                    Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);
                    _newScheme.DrawCategory(e.RowIndex, g, rect);
                }
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
            Initialize(layer as IFeatureLayer);
        }

        #endregion

        private void chkUseGradients_CheckedChanged(object sender, EventArgs e)
        {
            angGradientAngle.Enabled = chkUseGradients.Checked;
        }

        private void angGradientAngle_AngleChanged(object sender, EventArgs e)
        {
            _newScheme.EditorSettings.GradientAngle = angGradientAngle.Angle;
        }

        private void cmdRefresh_Click(object sender, EventArgs e)
        {
            _newScheme.EditorSettings.RampColors = false;
            RefreshValues();
        }

        private void btnRamp_Click(object sender, EventArgs e)
        {
            _newScheme.EditorSettings.RampColors = true;
            RefreshValues();
        }

        private void GetSettings()
        {
            _ignoreRefresh = true;
            FeatureEditorSettings settings = _newScheme.EditorSettings;
            switch (settings.ClassificationType)
            {
                case ClassificationType.Custom:
                    radCustom.Checked = true;
                    break;
                case ClassificationType.Quantities:
                    radQuantities.Checked = true;
                    nudCategoryCount.Value = settings.NumBreaks;
                    break;
                case ClassificationType.UniqueValues:
                    radUniqueValues.Checked = true;
                    break;
            }

            _cmbField.SelectedItem = settings.FieldName;
            tccColorRange.Initialize(new ColorRangeEventArgs(settings.StartColor, settings.EndColor, settings.HueShift, settings.HueSatLight, settings.UseColorRange));
            chkUseGradients.Checked = settings.UseGradient;
            angGradientAngle.Angle = settings.GradientAngle;
            if (!_source.AttributesPopulated)
            {
                _cancelDialog.Show(ParentForm);
                UpdateTable(_cancelDialog);
                _cancelDialog.Hide();
            }
            else
            {
                UpdateTable(null);
            }

            cmbInterval.SelectedItem = settings.IntervalMethod.ToString();
            if (settings.ClassificationType == ClassificationType.Quantities) UpdateStatistics(false, null);
            _ignoreRefresh = false;
            cmbIntervalSnapping.SelectedItem = settings.IntervalSnapMethod;
            nudSigFig.Value = settings.IntervalRoundingDigits;

            // featureSizeRangeControl1.Initialize(new SizeRangeEventArgs(settings.StartSize, settings.EndSize, settings.TemplateSymbolizer, settings.UseSizeRange));
        }

        private void SetSettings()
        {
            if (_ignoreRefresh) return;
            FeatureEditorSettings settings = _newScheme.EditorSettings;
            if (radUniqueValues.Checked) settings.ClassificationType = ClassificationType.UniqueValues;
            else if (radQuantities.Checked) settings.ClassificationType = ClassificationType.Quantities;
            else if (radCustom.Checked) settings.ClassificationType = ClassificationType.Custom;
            settings.NumBreaks = (int)nudCategoryCount.Value;
            settings.FieldName = (string)_cmbField.SelectedItem;

            settings.UseGradient = chkUseGradients.Checked;
            settings.GradientAngle = angGradientAngle.Angle;
            settings.IntervalSnapMethod = (IntervalSnapMethod)cmbIntervalSnapping.SelectedItem;
            settings.IntervalRoundingDigits = (int)nudSigFig.Value;

            featureSizeRangeControl1.UpdateControls();
        }

        private void radQuantities_CheckedChanged(object sender, EventArgs e)
        {
            if (radQuantities.Checked) UpdateRadioButtons(); // prevent excess firing by only firing if this is actually the one turned on.
        }

        private void UpdateRadioButtons()
        {
            if (_ignoreRefresh) return;
            string currentName = (string)_cmbField.SelectedItem;
            if (radQuantities.Checked)
            {
                _cmbField.Items.Clear();
                DataColumn[] columns = _source.GetColumns();
                foreach (DataColumn column in columns)
                {
                    if (column.DataType == typeof(string)) continue;
                    if (column.DataType == typeof(bool)) continue;
                    _cmbField.Items.Add(column.ColumnName);
                }
                if (currentName != null && _cmbField.Items.Contains(currentName))
                {
                    _cmbField.SelectedItem = currentName;
                }
                else
                {
                    if (_cmbField.Items.Count > 0) _cmbField.SelectedItem = _cmbField.Items[0];
                }
                nudCategoryCount.Enabled = true;
                tabScheme.Visible = true;
                dgvCategories.Height = 217;
                UpdateStatistics(false, null);
            }
            else
            {
                if (radCustom.Checked)
                {
                    _newScheme.EditorSettings.ClassificationType = ClassificationType.Custom;
                    UpdateTitle();
                    //UpdateTable();
                    btnUp.Enabled = true;
                    btnDown.Enabled = true;
                    _cmbField.Enabled = false;
                }
                else
                {
                    btnUp.Enabled = false;
                    btnDown.Enabled = false;
                    _cmbField.Enabled = true;
                }
                _cmbField.Items.Clear();
                DataColumn[] columns = _source.GetColumns();
                foreach (DataColumn column in columns)
                {
                    _cmbField.Items.Add(column.ColumnName);
                }
                nudCategoryCount.Enabled = false;
                tabScheme.Visible = false;
                dgvCategories.Height = 456;
                if (currentName != null && _cmbField.Items.Contains(currentName))
                {
                    _cmbField.SelectedItem = currentName;
                }
            }
            if (currentName != null) _cmbField.SelectedItem = currentName;
        }

        private void UpdateStatistics(bool clear, ICancelProgressHandler handler)
        {
            if (_newScheme.EditorSettings.ClassificationType != ClassificationType.Quantities) return;
            // Graph
            SetSettings();
            breakSliderGraph1.Scheme = _newScheme;
            breakSliderGraph1.Fieldname = (string)_cmbField.SelectedItem;
            breakSliderGraph1.Title = (string)_cmbField.SelectedItem;
            if (_source.AttributesPopulated)
            {
                breakSliderGraph1.Table = _source.DataTable;
            }
            else
            {
                breakSliderGraph1.AttributeSource = _source;
            }
            breakSliderGraph1.ResetExtents();
            if (_newScheme.EditorSettings.IntervalMethod == IntervalMethod.Manual && !clear)
            {
                breakSliderGraph1.UpdateBreaks();
            }
            else
            {
                breakSliderGraph1.ResetBreaks(handler);
            }
            Statistics stats = breakSliderGraph1.Statistics;

            // Stat list
            dgvStatistics.Rows.Clear();
            dgvStatistics.Rows.Add(7);
            dgvStatistics[0, 0].Value = "Count";
            dgvStatistics[1, 0].Value = stats.Count.ToString("#, ###");
            dgvStatistics[0, 1].Value = "Min";
            dgvStatistics[1, 1].Value = stats.Minimum.ToString("#, ###");
            dgvStatistics[0, 2].Value = "Max";
            dgvStatistics[1, 2].Value = stats.Maximum.ToString("#, ###");
            dgvStatistics[0, 3].Value = "Sum";
            dgvStatistics[1, 3].Value = stats.Sum.ToString("#, ###");
            dgvStatistics[0, 4].Value = "Mean";
            dgvStatistics[1, 4].Value = stats.Mean.ToString("#, ###");
            dgvStatistics[0, 5].Value = "Median";
            dgvStatistics[1, 5].Value = stats.Median.ToString("#, ###");
            dgvStatistics[0, 6].Value = "Std";
            dgvStatistics[1, 6].Value = stats.StandardDeviation.ToString("#, ###");
        }

        private void nudCategoryCount_ValueChanged(object sender, EventArgs e)
        {
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
            if (_newScheme == null) return;
            _newScheme.EditorSettings.IntervalMethod = method;
            RefreshValues();
        }

        private void breakSliderGraph1_SliderMoved(object sender, BreakSliderEventArgs e)
        {
            _ignoreRefresh = true;
            cmbInterval.SelectedItem = "Manual";
            _ignoreRefresh = false;
            _newScheme.EditorSettings.IntervalMethod = IntervalMethod.Manual;
            List<IFeatureCategory> cats = _newScheme.GetCategories().ToList();
            int index = cats.IndexOf(e.Slider.Category as IFeatureCategory);
            if (index == -1) return;
            UpdateTable(null);
            dgvCategories.Rows[index].Selected = true;
        }

        private void nudColumns_ValueChanged(object sender, EventArgs e)
        {
            breakSliderGraph1.NumColumns = (int)nudColumns.Value;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (_newScheme.EditorSettings.ClassificationType != ClassificationType.Quantities)
            {
                _newScheme.AddCategory(_newScheme.CreateRandomCategory(string.Empty));
                if (!_source.AttributesPopulated)
                {
                    _cancelDialog.Show();
                    UpdateTable(_cancelDialog);
                    _cancelDialog.Hide();
                }
                else
                {
                    UpdateTable();
                }
                dgvCategories.ClearSelection();
                dgvCategories.Rows[dgvCategories.Rows.Count - 1].Selected = true;
                _expressionDialog.ShowDialog(this);
            }
            else
            {
                nudCategoryCount.Value += 1;
            }
        }

        private void ExpressionDialogChangesApplied(object sender, EventArgs e)
        {
            if (_expressionIsExclude)
            {
                ApplyExcludeExpression(_expressionDialog.Expression);
                return;
            }
            ApplyCategoryExpression(_expressionDialog.Expression);
        }

        private void ApplyExcludeExpression(string expression)
        {
            _newScheme.EditorSettings.ExcludeExpression = expression;
            RefreshValues();
        }

        private void ApplyCategoryExpression(string expression)
        {
            List<IFeatureCategory> cats = _newScheme.GetCategories().ToList();
            List<int> indices = new List<int>();
            foreach (DataGridViewRow row in dgvCategories.SelectedRows)
            {
                cats[row.Index].FilterExpression = expression;
                cats[row.Index].DisplayExpression();
                indices.Add(row.Index);
            }
            UpdateTable();
            foreach (int index in indices)
            {
                dgvCategories.Rows[index].Selected = true;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCategories.SelectedRows.Count == 0) return;
            List<IFeatureCategory> deleteList = new List<IFeatureCategory>();
            List<IFeatureCategory> categories = _newScheme.GetCategories().ToList();
            int count = 0;
            foreach (DataGridViewRow row in dgvCategories.SelectedRows)
            {
                int index = dgvCategories.Rows.IndexOf(row);
                deleteList.Add(categories[index]);
                count++;
            }
            foreach (IFeatureCategory category in deleteList)
            {
                if (_newScheme.EditorSettings.ClassificationType == ClassificationType.Quantities)
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
                else
                {
                    _newScheme.RemoveCategory(category);
                }
            }
            UpdateTable();
            if (_newScheme.EditorSettings.ClassificationType == ClassificationType.Quantities)
            {
                _newScheme.EditorSettings.IntervalMethod = IntervalMethod.Manual;
                _newScheme.EditorSettings.NumBreaks -= count;
                UpdateStatistics(false, null);
            }
        }

        private void radCustom_CheckedChanged(object sender, EventArgs e)
        {
            if (radCustom.Checked) UpdateRadioButtons();
        }

        private void UpdateTitle()
        {
            if (!radCustom.Checked && _newScheme.EditorSettings.FieldName != null)
            {
                _newScheme.AppearsInLegend = true;
            }
            else
            {
                _newScheme.AppearsInLegend = false;
                foreach (IFeatureCategory category in _newScheme.GetCategories())
                {
                    category.DisplayExpression();
                }
            }
        }

        private void radUniqueValues_CheckedChanged(object sender, EventArgs e)
        {
            btnAdd.Enabled = !radUniqueValues.Checked;
            if (radUniqueValues.Checked) UpdateRadioButtons(); // fire this only if this is the one that was activated
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            if (dgvCategories.IsCurrentCellInEditMode)
            {
                dgvCategories.EndEdit();
            }
            _ignoreValidation = true;
            int index = dgvCategories.SelectedRows[0].Index;
            IFeatureCategory cat = _newScheme.GetCategories().ToList()[index];
            if (!_newScheme.DecreaseCategoryIndex(cat)) return;
            UpdateTable();
            index--;
            dgvCategories.Rows[index].Selected = true;
            _ignoreValidation = false;
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            if (dgvCategories.IsCurrentCellInEditMode)
            {
                dgvCategories.EndEdit();
            }
            _ignoreValidation = true;
            int index = dgvCategories.SelectedRows[0].Index;
            IFeatureCategory cat = _newScheme.GetCategories().ToList()[index];
            if (!_newScheme.IncreaseCategoryIndex(cat)) return;
            index++;
            UpdateTable();
            dgvCategories.Rows[index].Selected = true;
            _ignoreValidation = false;
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

        private void btnExclude_Click(object sender, EventArgs e)
        {
            _expressionIsExclude = true;
            if (_source.AttributesPopulated)
            {
                _expressionDialog.Table = _source.DataTable;
            }
            else
            {
                _expressionDialog.AttributeSource = _source;
            }

            _expressionDialog.Expression = _newScheme.EditorSettings.ExcludeExpression;
            _expressionDialog.ShowDialog();
        }

        private void cmbNormField_SelectedIndexChanged(object sender, EventArgs e)
        {
            string norm = (string)cmbNormField.SelectedItem;
            _newScheme.EditorSettings.NormField = norm == "(None)" ? null : norm;
            RefreshValues();
        }

        private void tccColorRange_ColorChanged(object sender, ColorRangeEventArgs e)
        {
            if (_ignoreRefresh) return;
            FeatureEditorSettings settings = _newScheme.EditorSettings;
            settings.StartColor = e.StartColor;
            settings.EndColor = e.EndColor;
            settings.UseColorRange = e.UseColorRange;
            settings.HueShift = e.HueShift;
            settings.HueSatLight = e.HSL;
            featureSizeRangeControl1.UpdateControls();
            RefreshValues();

        }

        private void pointSizeRangeControl1_SizeRangeChanged(object sender, SizeRangeEventArgs e)
        {
            if (_ignoreRefresh) return;
            FeatureEditorSettings settings = _newScheme.EditorSettings;
            settings.StartSize = e.StartSize;
            settings.EndSize = e.EndSize;
            settings.TemplateSymbolizer = e.Template;
            settings.UseSizeRange = e.UseSizeRange;
        }
    }
}