// ********************************************************************************************************
// Product Name: DotSpatial.Symbology.Forms.dll
// Description:  The core assembly for the DotSpatial 6.0 distribution.
// ********************************************************************************************************
// The contents of this file are subject to the MIT License (MIT)
// you may not use this file except in compliance with the License. You may obtain a copy of the License at
// http://dotspatial.codeplex.com/license
//
// Software distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTY OF
// ANY KIND, either expressed or implied. See the License for the specific language governing rights and
// limitations under the License.
//
// The Original Code is DotSpatial.dll
//
// The Initial Developer of this Original Code is Ted Dunsford. Created 5/19/2009 2:56:10 PM
//
// Contributor(s): (Open source contributors should list themselves and their modifications here).
//
// ********************************************************************************************************

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using DotSpatial.Data;

namespace DotSpatial.Symbology.Forms
{
    /// <summary>
    /// DetailedPolygonSymbolDialog
    /// </summary>
    public class DetailedPolygonSymbolControl : UserControl
    {
        #region Events

        /// <summary>
        /// Occurs whenever the apply changes button is clicked, or else when the ok button is clicked.
        /// </summary>
        public event EventHandler ChangesApplied;

        /// <summary>
        /// Occurs when the the Add To Custom Symbols button is clicked
        /// </summary>
        public event EventHandler<PolygonSymbolizerEventArgs> AddToCustomSymbols;

        #endregion

        #region Private Variables

        private bool _disableUnitWarning;
        private bool _ignoreChanges;
        private IPolygonSymbolizer _original;
        private IPolygonSymbolizer _symbolizer;
        private AngleControl angGradientAngle;
        private AngleControl angTileAngle;
        private Button btnAddToCustom;
        private Button btnLoadImage;
        private ColorButton cbColorSimple;
        private PatternCollectionControl ccPatterns;
        private CheckBox chkSmoothing;
        private ComboBox cmbGradientType;
        private ComboBox cmbHatchStyle;
        private ComboBox cmbPatternType;
        private ComboBox cmbScaleMode;
        private ComboBox cmbTileMode;
        private ComboBox cmbUnits;
        private DoubleBox dbxScaleX;
        private DoubleBox dbxScaleY;
        private GroupBox groupBox1;
        private ColorButton hatchBackColor;
        private RampSlider hatchBackOpacity;
        private ColorButton hatchForeColor;
        private RampSlider hatchForeOpacity;
        private HelpProvider helpProvider1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label lblColorSimple;
        private Label lblEndColor;
        private Label lblHatchStyle;
        private Label lblPatternType;
        private Label lblPreview;
        private Label lblScaleMode;
        private Label lblStartColor;
        private Label lblTileMode;
        private Label lblUnits;
        private OutlineControl ocOutline;
        private RampSlider sldOpacitySimple;
        //private Tools.Modeler modeler1;
        private GradientControl sliderGradient;
        private TabPage tabGradient;
        private TabPage tabHatch;
        private TabControl tabPatternProperties;
        private TabPage tabPicture;
        private TabPage tabSimple;
        private TextBox txtImage;

        #endregion

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DetailedPolygonSymbolControl));
            this.btnAddToCustom = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmbUnits = new System.Windows.Forms.ComboBox();
            this.lblUnits = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblPreview = new System.Windows.Forms.Label();
            this.lblScaleMode = new System.Windows.Forms.Label();
            this.cmbScaleMode = new System.Windows.Forms.ComboBox();
            this.chkSmoothing = new System.Windows.Forms.CheckBox();
            this.lblPatternType = new System.Windows.Forms.Label();
            this.cmbPatternType = new System.Windows.Forms.ComboBox();
            this.tabPatternProperties = new System.Windows.Forms.TabControl();
            this.tabSimple = new System.Windows.Forms.TabPage();
            this.lblColorSimple = new System.Windows.Forms.Label();
            this.cbColorSimple = new DotSpatial.Symbology.Forms.ColorButton();
            this.sldOpacitySimple = new DotSpatial.Symbology.Forms.RampSlider();
            this.tabPicture = new System.Windows.Forms.TabPage();
            this.dbxScaleY = new DotSpatial.Symbology.Forms.DoubleBox();
            this.dbxScaleX = new DotSpatial.Symbology.Forms.DoubleBox();
            this.angTileAngle = new DotSpatial.Symbology.Forms.AngleControl();
            this.lblTileMode = new System.Windows.Forms.Label();
            this.cmbTileMode = new System.Windows.Forms.ComboBox();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.txtImage = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabGradient = new System.Windows.Forms.TabPage();
            this.sliderGradient = new DotSpatial.Symbology.Forms.GradientControl();
            this.cmbGradientType = new System.Windows.Forms.ComboBox();
            this.lblEndColor = new System.Windows.Forms.Label();
            this.lblStartColor = new System.Windows.Forms.Label();
            this.angGradientAngle = new DotSpatial.Symbology.Forms.AngleControl();
            this.tabHatch = new System.Windows.Forms.TabPage();
            this.lblHatchStyle = new System.Windows.Forms.Label();
            this.cmbHatchStyle = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.hatchBackOpacity = new DotSpatial.Symbology.Forms.RampSlider();
            this.hatchBackColor = new DotSpatial.Symbology.Forms.ColorButton();
            this.label1 = new System.Windows.Forms.Label();
            this.hatchForeOpacity = new DotSpatial.Symbology.Forms.RampSlider();
            this.hatchForeColor = new DotSpatial.Symbology.Forms.ColorButton();
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.ocOutline = new DotSpatial.Symbology.Forms.OutlineControl();
            this.ccPatterns = new DotSpatial.Symbology.Forms.PatternCollectionControl();
            this.groupBox1.SuspendLayout();
            this.tabPatternProperties.SuspendLayout();
            this.tabSimple.SuspendLayout();
            this.tabPicture.SuspendLayout();
            this.tabGradient.SuspendLayout();
            this.tabHatch.SuspendLayout();
            this.SuspendLayout();
            //
            // btnAddToCustom
            //
            resources.ApplyResources(this.btnAddToCustom, "btnAddToCustom");
            this.btnAddToCustom.Name = "btnAddToCustom";
            this.helpProvider1.SetShowHelp(this.btnAddToCustom, ((bool)(resources.GetObject("btnAddToCustom.ShowHelp"))));
            this.btnAddToCustom.UseVisualStyleBackColor = true;
            this.btnAddToCustom.Click += new System.EventHandler(this.btnAddToCustom_Click);
            //
            // groupBox1
            //
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.cmbUnits);
            this.groupBox1.Controls.Add(this.lblUnits);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.lblPreview);
            this.groupBox1.Controls.Add(this.lblScaleMode);
            this.groupBox1.Controls.Add(this.cmbScaleMode);
            this.groupBox1.Controls.Add(this.chkSmoothing);
            this.groupBox1.Name = "groupBox1";
            this.helpProvider1.SetShowHelp(this.groupBox1, ((bool)(resources.GetObject("groupBox1.ShowHelp"))));
            this.groupBox1.TabStop = false;
            this.groupBox1.UseCompatibleTextRendering = true;
            //
            // cmbUnits
            //
            this.cmbUnits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbUnits.FormattingEnabled = true;
            this.cmbUnits.Items.AddRange(new object[] {
                                                          resources.GetString("cmbUnits.Items"),
                                                          resources.GetString("cmbUnits.Items1"),
                                                          resources.GetString("cmbUnits.Items2"),
                                                          resources.GetString("cmbUnits.Items3"),
                                                          resources.GetString("cmbUnits.Items4"),
                                                          resources.GetString("cmbUnits.Items5"),
                                                          resources.GetString("cmbUnits.Items6")});
            resources.ApplyResources(this.cmbUnits, "cmbUnits");
            this.cmbUnits.Name = "cmbUnits";
            this.helpProvider1.SetShowHelp(this.cmbUnits, ((bool)(resources.GetObject("cmbUnits.ShowHelp"))));
            this.cmbUnits.SelectedIndexChanged += new System.EventHandler(this.cmbUnits_SelectedIndexChanged);
            //
            // lblUnits
            //
            resources.ApplyResources(this.lblUnits, "lblUnits");
            this.lblUnits.Name = "lblUnits";
            this.helpProvider1.SetShowHelp(this.lblUnits, ((bool)(resources.GetObject("lblUnits.ShowHelp"))));
            //
            // label3
            //
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            this.helpProvider1.SetShowHelp(this.label3, ((bool)(resources.GetObject("label3.ShowHelp"))));
            //
            // lblPreview
            //
            this.lblPreview.BackColor = System.Drawing.Color.White;
            this.lblPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.lblPreview, "lblPreview");
            this.lblPreview.Name = "lblPreview";
            this.helpProvider1.SetShowHelp(this.lblPreview, ((bool)(resources.GetObject("lblPreview.ShowHelp"))));
            //
            // lblScaleMode
            //
            resources.ApplyResources(this.lblScaleMode, "lblScaleMode");
            this.lblScaleMode.Name = "lblScaleMode";
            this.helpProvider1.SetShowHelp(this.lblScaleMode, ((bool)(resources.GetObject("lblScaleMode.ShowHelp"))));
            //
            // cmbScaleMode
            //
            this.cmbScaleMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScaleMode.FormattingEnabled = true;
            this.cmbScaleMode.Items.AddRange(new object[] {
                                                              resources.GetString("cmbScaleMode.Items"),
                                                              resources.GetString("cmbScaleMode.Items1"),
                                                              resources.GetString("cmbScaleMode.Items2")});
            resources.ApplyResources(this.cmbScaleMode, "cmbScaleMode");
            this.cmbScaleMode.Name = "cmbScaleMode";
            this.helpProvider1.SetShowHelp(this.cmbScaleMode, ((bool)(resources.GetObject("cmbScaleMode.ShowHelp"))));
            this.cmbScaleMode.SelectedIndexChanged += new System.EventHandler(this.cmbScaleMode_SelectedIndexChanged);
            //
            // chkSmoothing
            //
            resources.ApplyResources(this.chkSmoothing, "chkSmoothing");
            this.chkSmoothing.Name = "chkSmoothing";
            this.helpProvider1.SetShowHelp(this.chkSmoothing, ((bool)(resources.GetObject("chkSmoothing.ShowHelp"))));
            this.chkSmoothing.UseVisualStyleBackColor = true;
            this.chkSmoothing.CheckedChanged += new System.EventHandler(this.chkSmoothing_CheckedChanged);
            //
            // lblPatternType
            //
            resources.ApplyResources(this.lblPatternType, "lblPatternType");
            this.lblPatternType.Name = "lblPatternType";
            this.helpProvider1.SetShowHelp(this.lblPatternType, ((bool)(resources.GetObject("lblPatternType.ShowHelp"))));
            //
            // cmbPatternType
            //
            this.cmbPatternType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPatternType.FormattingEnabled = true;
            this.cmbPatternType.Items.AddRange(new object[] {
                                                                resources.GetString("cmbPatternType.Items"),
                                                                resources.GetString("cmbPatternType.Items1"),
                                                                resources.GetString("cmbPatternType.Items2"),
                                                                resources.GetString("cmbPatternType.Items3")});
            resources.ApplyResources(this.cmbPatternType, "cmbPatternType");
            this.cmbPatternType.Name = "cmbPatternType";
            this.helpProvider1.SetShowHelp(this.cmbPatternType, ((bool)(resources.GetObject("cmbPatternType.ShowHelp"))));
            this.cmbPatternType.SelectedIndexChanged += new System.EventHandler(this.cmbPatternType_SelectedIndexChanged);
            //
            // tabPatternProperties
            //
            resources.ApplyResources(this.tabPatternProperties, "tabPatternProperties");
            this.tabPatternProperties.Controls.Add(this.tabSimple);
            this.tabPatternProperties.Controls.Add(this.tabPicture);
            this.tabPatternProperties.Controls.Add(this.tabGradient);
            this.tabPatternProperties.Controls.Add(this.tabHatch);
            this.tabPatternProperties.Name = "tabPatternProperties";
            this.tabPatternProperties.SelectedIndex = 0;
            this.helpProvider1.SetShowHelp(this.tabPatternProperties, ((bool)(resources.GetObject("tabPatternProperties.ShowHelp"))));
            //
            // tabSimple
            //
            this.tabSimple.Controls.Add(this.lblColorSimple);
            this.tabSimple.Controls.Add(this.cbColorSimple);
            this.tabSimple.Controls.Add(this.sldOpacitySimple);
            resources.ApplyResources(this.tabSimple, "tabSimple");
            this.tabSimple.Name = "tabSimple";
            this.helpProvider1.SetShowHelp(this.tabSimple, ((bool)(resources.GetObject("tabSimple.ShowHelp"))));
            this.tabSimple.UseVisualStyleBackColor = true;
            //
            // lblColorSimple
            //
            resources.ApplyResources(this.lblColorSimple, "lblColorSimple");
            this.lblColorSimple.Name = "lblColorSimple";
            this.helpProvider1.SetShowHelp(this.lblColorSimple, ((bool)(resources.GetObject("lblColorSimple.ShowHelp"))));
            //
            // cbColorSimple
            //
            this.cbColorSimple.BevelRadius = 4;
            this.cbColorSimple.Color = System.Drawing.Color.Blue;
            this.cbColorSimple.LaunchDialogOnClick = true;
            resources.ApplyResources(this.cbColorSimple, "cbColorSimple");
            this.cbColorSimple.Name = "cbColorSimple";
            this.cbColorSimple.RoundingRadius = 10;
            this.helpProvider1.SetShowHelp(this.cbColorSimple, ((bool)(resources.GetObject("cbColorSimple.ShowHelp"))));
            this.cbColorSimple.ColorChanged += new System.EventHandler(this.cbColorSimple_ColorChanged);
            //
            // sldOpacitySimple
            //
            this.sldOpacitySimple.ColorButton = null;
            this.sldOpacitySimple.FlipRamp = false;
            this.sldOpacitySimple.FlipText = false;
            this.sldOpacitySimple.InvertRamp = false;
            resources.ApplyResources(this.sldOpacitySimple, "sldOpacitySimple");
            this.sldOpacitySimple.Maximum = 1D;
            this.sldOpacitySimple.MaximumColor = System.Drawing.Color.CornflowerBlue;
            this.sldOpacitySimple.Minimum = 0D;
            this.sldOpacitySimple.MinimumColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.sldOpacitySimple.Name = "sldOpacitySimple";
            this.sldOpacitySimple.NumberFormat = null;
            this.sldOpacitySimple.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.sldOpacitySimple.RampRadius = 8F;
            this.sldOpacitySimple.RampText = "Opacity";
            this.sldOpacitySimple.RampTextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.sldOpacitySimple.RampTextBehindRamp = true;
            this.sldOpacitySimple.RampTextColor = System.Drawing.Color.Black;
            this.sldOpacitySimple.RampTextFont = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helpProvider1.SetShowHelp(this.sldOpacitySimple, ((bool)(resources.GetObject("sldOpacitySimple.ShowHelp"))));
            this.sldOpacitySimple.ShowMaximum = true;
            this.sldOpacitySimple.ShowMinimum = true;
            this.sldOpacitySimple.ShowTicks = true;
            this.sldOpacitySimple.ShowValue = false;
            this.sldOpacitySimple.SliderColor = System.Drawing.Color.SteelBlue;
            this.sldOpacitySimple.SliderRadius = 4F;
            this.sldOpacitySimple.TickColor = System.Drawing.Color.DarkGray;
            this.sldOpacitySimple.TickSpacing = 5F;
            this.sldOpacitySimple.Value = 0D;
            this.sldOpacitySimple.ValueChanged += new System.EventHandler(this.sldOpacitySimple_ValueChanged);
            //
            // tabPicture
            //
            this.tabPicture.Controls.Add(this.dbxScaleY);
            this.tabPicture.Controls.Add(this.dbxScaleX);
            this.tabPicture.Controls.Add(this.angTileAngle);
            this.tabPicture.Controls.Add(this.lblTileMode);
            this.tabPicture.Controls.Add(this.cmbTileMode);
            this.tabPicture.Controls.Add(this.btnLoadImage);
            this.tabPicture.Controls.Add(this.txtImage);
            this.tabPicture.Controls.Add(this.label4);
            resources.ApplyResources(this.tabPicture, "tabPicture");
            this.tabPicture.Name = "tabPicture";
            this.helpProvider1.SetShowHelp(this.tabPicture, ((bool)(resources.GetObject("tabPicture.ShowHelp"))));
            this.tabPicture.UseVisualStyleBackColor = true;
            //
            // dbxScaleY
            //
            this.dbxScaleY.BackColorInvalid = System.Drawing.Color.Salmon;
            this.dbxScaleY.BackColorRegular = System.Drawing.Color.Empty;
            resources.ApplyResources(this.dbxScaleY, "dbxScaleY");
            this.dbxScaleY.InvalidHelp = "The value entered could not be correctly parsed into a valid double precision flo" +
                                         "ating point value.";
            this.dbxScaleY.IsValid = true;
            this.dbxScaleY.Name = "dbxScaleY";
            this.dbxScaleY.NumberFormat = null;
            this.dbxScaleY.RegularHelp = "Enter a double precision floating point value.";
            this.helpProvider1.SetShowHelp(this.dbxScaleY, ((bool)(resources.GetObject("dbxScaleY.ShowHelp"))));
            this.dbxScaleY.Value = 0D;
            this.dbxScaleY.TextChanged += new System.EventHandler(this.dbxScaleY_TextChanged);
            //
            // dbxScaleX
            //
            this.dbxScaleX.BackColorInvalid = System.Drawing.Color.Salmon;
            this.dbxScaleX.BackColorRegular = System.Drawing.Color.Empty;
            resources.ApplyResources(this.dbxScaleX, "dbxScaleX");
            this.dbxScaleX.InvalidHelp = "The value entered could not be correctly parsed into a valid double precision flo" +
                                         "ating point value.";
            this.dbxScaleX.IsValid = true;
            this.dbxScaleX.Name = "dbxScaleX";
            this.dbxScaleX.NumberFormat = null;
            this.dbxScaleX.RegularHelp = "Enter a double precision floating point value.";
            this.helpProvider1.SetShowHelp(this.dbxScaleX, ((bool)(resources.GetObject("dbxScaleX.ShowHelp"))));
            this.dbxScaleX.Value = 0D;
            this.dbxScaleX.TextChanged += new System.EventHandler(this.dbxScaleX_TextChanged);
            //
            // angTileAngle
            //
            this.angTileAngle.Angle = 0;
            this.angTileAngle.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.angTileAngle, "angTileAngle");
            this.angTileAngle.Clockwise = false;
            this.angTileAngle.KnobColor = System.Drawing.Color.SteelBlue;
            this.angTileAngle.Name = "angTileAngle";
            this.helpProvider1.SetShowHelp(this.angTileAngle, ((bool)(resources.GetObject("angTileAngle.ShowHelp"))));
            this.angTileAngle.StartAngle = 0;
            this.angTileAngle.AngleChanged += new System.EventHandler(this.angTileAngle_AngleChanged);
            //
            // lblTileMode
            //
            resources.ApplyResources(this.lblTileMode, "lblTileMode");
            this.lblTileMode.Name = "lblTileMode";
            this.helpProvider1.SetShowHelp(this.lblTileMode, ((bool)(resources.GetObject("lblTileMode.ShowHelp"))));
            //
            // cmbTileMode
            //
            this.cmbTileMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTileMode.FormattingEnabled = true;
            this.cmbTileMode.Items.AddRange(new object[] {
                                                             resources.GetString("cmbTileMode.Items"),
                                                             resources.GetString("cmbTileMode.Items1"),
                                                             resources.GetString("cmbTileMode.Items2"),
                                                             resources.GetString("cmbTileMode.Items3"),
                                                             resources.GetString("cmbTileMode.Items4")});
            resources.ApplyResources(this.cmbTileMode, "cmbTileMode");
            this.cmbTileMode.Name = "cmbTileMode";
            this.helpProvider1.SetShowHelp(this.cmbTileMode, ((bool)(resources.GetObject("cmbTileMode.ShowHelp"))));
            this.cmbTileMode.SelectedIndexChanged += new System.EventHandler(this.cmbTileMode_SelectedIndexChanged);
            //
            // btnLoadImage
            //
            resources.ApplyResources(this.btnLoadImage, "btnLoadImage");
            this.btnLoadImage.Name = "btnLoadImage";
            this.helpProvider1.SetShowHelp(this.btnLoadImage, ((bool)(resources.GetObject("btnLoadImage.ShowHelp"))));
            this.btnLoadImage.UseVisualStyleBackColor = true;
            this.btnLoadImage.Click += new System.EventHandler(this.btnLoadImage_Click);
            //
            // txtImage
            //
            resources.ApplyResources(this.txtImage, "txtImage");
            this.txtImage.Name = "txtImage";
            this.helpProvider1.SetShowHelp(this.txtImage, ((bool)(resources.GetObject("txtImage.ShowHelp"))));
            //
            // label4
            //
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            this.helpProvider1.SetShowHelp(this.label4, ((bool)(resources.GetObject("label4.ShowHelp"))));
            //
            // tabGradient
            //
            this.tabGradient.Controls.Add(this.sliderGradient);
            this.tabGradient.Controls.Add(this.cmbGradientType);
            this.tabGradient.Controls.Add(this.lblEndColor);
            this.tabGradient.Controls.Add(this.lblStartColor);
            this.tabGradient.Controls.Add(this.angGradientAngle);
            resources.ApplyResources(this.tabGradient, "tabGradient");
            this.tabGradient.Name = "tabGradient";
            this.helpProvider1.SetShowHelp(this.tabGradient, ((bool)(resources.GetObject("tabGradient.ShowHelp"))));
            this.tabGradient.UseVisualStyleBackColor = true;
            //
            // sliderGradient
            //
            this.sliderGradient.EndValue = 0.8F;
            resources.ApplyResources(this.sliderGradient, "sliderGradient");
            this.sliderGradient.MaximumColor = System.Drawing.Color.Blue;
            this.sliderGradient.MinimumColor = System.Drawing.Color.Lime;
            this.sliderGradient.Name = "sliderGradient";
            this.helpProvider1.SetShowHelp(this.sliderGradient, ((bool)(resources.GetObject("sliderGradient.ShowHelp"))));
            this.sliderGradient.SlidersEnabled = true;
            this.sliderGradient.StartValue = 0.2F;
            this.sliderGradient.GradientChanging += new System.EventHandler(this.sliderGradient_GradientChanging);
            //
            // cmbGradientType
            //
            this.cmbGradientType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGradientType.FormattingEnabled = true;
            this.cmbGradientType.Items.AddRange(new object[] {
                                                                 resources.GetString("cmbGradientType.Items"),
                                                                 resources.GetString("cmbGradientType.Items1"),
                                                                 resources.GetString("cmbGradientType.Items2")});
            resources.ApplyResources(this.cmbGradientType, "cmbGradientType");
            this.cmbGradientType.Name = "cmbGradientType";
            this.helpProvider1.SetShowHelp(this.cmbGradientType, ((bool)(resources.GetObject("cmbGradientType.ShowHelp"))));
            this.cmbGradientType.SelectedIndexChanged += new System.EventHandler(this.cmbGradientType_SelectedIndexChanged);
            //
            // lblEndColor
            //
            resources.ApplyResources(this.lblEndColor, "lblEndColor");
            this.lblEndColor.Name = "lblEndColor";
            this.helpProvider1.SetShowHelp(this.lblEndColor, ((bool)(resources.GetObject("lblEndColor.ShowHelp"))));
            //
            // lblStartColor
            //
            resources.ApplyResources(this.lblStartColor, "lblStartColor");
            this.lblStartColor.Name = "lblStartColor";
            this.helpProvider1.SetShowHelp(this.lblStartColor, ((bool)(resources.GetObject("lblStartColor.ShowHelp"))));
            //
            // angGradientAngle
            //
            this.angGradientAngle.Angle = 0;
            this.angGradientAngle.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.angGradientAngle, "angGradientAngle");
            this.angGradientAngle.Clockwise = false;
            this.angGradientAngle.KnobColor = System.Drawing.Color.SteelBlue;
            this.angGradientAngle.Name = "angGradientAngle";
            this.helpProvider1.SetShowHelp(this.angGradientAngle, ((bool)(resources.GetObject("angGradientAngle.ShowHelp"))));
            this.angGradientAngle.StartAngle = 0;
            this.angGradientAngle.AngleChanged += new System.EventHandler(this.angGradientAngle_AngleChanged);
            //
            // tabHatch
            //
            this.tabHatch.Controls.Add(this.lblHatchStyle);
            this.tabHatch.Controls.Add(this.cmbHatchStyle);
            this.tabHatch.Controls.Add(this.label2);
            this.tabHatch.Controls.Add(this.hatchBackOpacity);
            this.tabHatch.Controls.Add(this.hatchBackColor);
            this.tabHatch.Controls.Add(this.label1);
            this.tabHatch.Controls.Add(this.hatchForeOpacity);
            this.tabHatch.Controls.Add(this.hatchForeColor);
            resources.ApplyResources(this.tabHatch, "tabHatch");
            this.tabHatch.Name = "tabHatch";
            this.helpProvider1.SetShowHelp(this.tabHatch, ((bool)(resources.GetObject("tabHatch.ShowHelp"))));
            this.tabHatch.UseVisualStyleBackColor = true;
            //
            // lblHatchStyle
            //
            resources.ApplyResources(this.lblHatchStyle, "lblHatchStyle");
            this.lblHatchStyle.Name = "lblHatchStyle";
            this.helpProvider1.SetShowHelp(this.lblHatchStyle, ((bool)(resources.GetObject("lblHatchStyle.ShowHelp"))));
            //
            // cmbHatchStyle
            //
            this.cmbHatchStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHatchStyle.FormattingEnabled = true;
            resources.ApplyResources(this.cmbHatchStyle, "cmbHatchStyle");
            this.cmbHatchStyle.Name = "cmbHatchStyle";
            this.helpProvider1.SetShowHelp(this.cmbHatchStyle, ((bool)(resources.GetObject("cmbHatchStyle.ShowHelp"))));
            this.cmbHatchStyle.SelectedIndexChanged += new System.EventHandler(this.cmbHatchStyle_SelectedIndexChanged);
            //
            // label2
            //
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            this.helpProvider1.SetShowHelp(this.label2, ((bool)(resources.GetObject("label2.ShowHelp"))));
            //
            // hatchBackOpacity
            //
            this.hatchBackOpacity.ColorButton = null;
            this.hatchBackOpacity.FlipRamp = false;
            this.hatchBackOpacity.FlipText = false;
            this.hatchBackOpacity.InvertRamp = false;
            resources.ApplyResources(this.hatchBackOpacity, "hatchBackOpacity");
            this.hatchBackOpacity.Maximum = 1D;
            this.hatchBackOpacity.MaximumColor = System.Drawing.Color.CornflowerBlue;
            this.hatchBackOpacity.Minimum = 0D;
            this.hatchBackOpacity.MinimumColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.hatchBackOpacity.Name = "hatchBackOpacity";
            this.hatchBackOpacity.NumberFormat = null;
            this.hatchBackOpacity.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.hatchBackOpacity.RampRadius = 8F;
            this.hatchBackOpacity.RampText = "Opacity";
            this.hatchBackOpacity.RampTextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.hatchBackOpacity.RampTextBehindRamp = true;
            this.hatchBackOpacity.RampTextColor = System.Drawing.Color.Black;
            this.hatchBackOpacity.RampTextFont = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helpProvider1.SetShowHelp(this.hatchBackOpacity, ((bool)(resources.GetObject("hatchBackOpacity.ShowHelp"))));
            this.hatchBackOpacity.ShowMaximum = true;
            this.hatchBackOpacity.ShowMinimum = true;
            this.hatchBackOpacity.ShowTicks = true;
            this.hatchBackOpacity.ShowValue = false;
            this.hatchBackOpacity.SliderColor = System.Drawing.Color.Tan;
            this.hatchBackOpacity.SliderRadius = 4F;
            this.hatchBackOpacity.TickColor = System.Drawing.Color.DarkGray;
            this.hatchBackOpacity.TickSpacing = 5F;
            this.hatchBackOpacity.Value = 0D;
            this.hatchBackOpacity.ValueChanged += new System.EventHandler(this.hatchBackOpacity_ValueChanged);
            //
            // hatchBackColor
            //
            this.hatchBackColor.BevelRadius = 4;
            this.hatchBackColor.Color = System.Drawing.Color.Blue;
            this.hatchBackColor.LaunchDialogOnClick = true;
            resources.ApplyResources(this.hatchBackColor, "hatchBackColor");
            this.hatchBackColor.Name = "hatchBackColor";
            this.hatchBackColor.RoundingRadius = 10;
            this.helpProvider1.SetShowHelp(this.hatchBackColor, ((bool)(resources.GetObject("hatchBackColor.ShowHelp"))));
            this.hatchBackColor.ColorChanged += new System.EventHandler(this.hatchBackColor_ColorChanged);
            //
            // label1
            //
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            this.helpProvider1.SetShowHelp(this.label1, ((bool)(resources.GetObject("label1.ShowHelp"))));
            //
            // hatchForeOpacity
            //
            this.hatchForeOpacity.ColorButton = null;
            this.hatchForeOpacity.FlipRamp = false;
            this.hatchForeOpacity.FlipText = false;
            this.hatchForeOpacity.InvertRamp = false;
            resources.ApplyResources(this.hatchForeOpacity, "hatchForeOpacity");
            this.hatchForeOpacity.Maximum = 1D;
            this.hatchForeOpacity.MaximumColor = System.Drawing.Color.CornflowerBlue;
            this.hatchForeOpacity.Minimum = 0D;
            this.hatchForeOpacity.MinimumColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.hatchForeOpacity.Name = "hatchForeOpacity";
            this.hatchForeOpacity.NumberFormat = null;
            this.hatchForeOpacity.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.hatchForeOpacity.RampRadius = 8F;
            this.hatchForeOpacity.RampText = "Opacity";
            this.hatchForeOpacity.RampTextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.hatchForeOpacity.RampTextBehindRamp = true;
            this.hatchForeOpacity.RampTextColor = System.Drawing.Color.Black;
            this.hatchForeOpacity.RampTextFont = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helpProvider1.SetShowHelp(this.hatchForeOpacity, ((bool)(resources.GetObject("hatchForeOpacity.ShowHelp"))));
            this.hatchForeOpacity.ShowMaximum = true;
            this.hatchForeOpacity.ShowMinimum = true;
            this.hatchForeOpacity.ShowTicks = true;
            this.hatchForeOpacity.ShowValue = false;
            this.hatchForeOpacity.SliderColor = System.Drawing.Color.Tan;
            this.hatchForeOpacity.SliderRadius = 4F;
            this.hatchForeOpacity.TickColor = System.Drawing.Color.DarkGray;
            this.hatchForeOpacity.TickSpacing = 5F;
            this.hatchForeOpacity.Value = 0D;
            this.hatchForeOpacity.ValueChanged += new System.EventHandler(this.hatchForeOpacity_ValueChanged);
            //
            // hatchForeColor
            //
            this.hatchForeColor.BevelRadius = 4;
            this.hatchForeColor.Color = System.Drawing.Color.Blue;
            this.hatchForeColor.LaunchDialogOnClick = true;
            resources.ApplyResources(this.hatchForeColor, "hatchForeColor");
            this.hatchForeColor.Name = "hatchForeColor";
            this.hatchForeColor.RoundingRadius = 10;
            this.helpProvider1.SetShowHelp(this.hatchForeColor, ((bool)(resources.GetObject("hatchForeColor.ShowHelp"))));
            this.hatchForeColor.ColorChanged += new System.EventHandler(this.hatchForeColor_ColorChanged);
            //
            // ocOutline
            //
            resources.ApplyResources(this.ocOutline, "ocOutline");
            this.ocOutline.Name = "ocOutline";
            this.ocOutline.Pattern = null;
            this.helpProvider1.SetShowHelp(this.ocOutline, ((bool)(resources.GetObject("ocOutline.ShowHelp"))));
            this.ocOutline.OutlineChanged += new System.EventHandler(this.ocOutline_OutlineChanged);
            //
            // ccPatterns
            //
            resources.ApplyResources(this.ccPatterns, "ccPatterns");
            this.ccPatterns.Name = "ccPatterns";
            this.helpProvider1.SetShowHelp(this.ccPatterns, ((bool)(resources.GetObject("ccPatterns.ShowHelp"))));
            this.ccPatterns.Load += new System.EventHandler(this.ccPatterns_Load);
            //
            // DetailedPolygonSymbolControl
            //
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnAddToCustom);
            this.Controls.Add(this.ocOutline);
            this.Controls.Add(this.tabPatternProperties);
            this.Controls.Add(this.lblPatternType);
            this.Controls.Add(this.cmbPatternType);
            this.Controls.Add(this.ccPatterns);
            this.Name = "DetailedPolygonSymbolControl";
            this.helpProvider1.SetShowHelp(this, ((bool)(resources.GetObject("$this.ShowHelp"))));
            resources.ApplyResources(this, "$this");
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPatternProperties.ResumeLayout(false);
            this.tabSimple.ResumeLayout(false);
            this.tabSimple.PerformLayout();
            this.tabPicture.ResumeLayout(false);
            this.tabPicture.PerformLayout();
            this.tabGradient.ResumeLayout(false);
            this.tabGradient.PerformLayout();
            this.tabHatch.ResumeLayout(false);
            this.tabHatch.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of CollectionPropertyGrid
        /// </summary>
        public DetailedPolygonSymbolControl()
        {
            InitializeComponent();
            Configure();
        }

        /// <summary>
        /// Creates a new dialog where the properties displayed on the form are a duplicate of the original properties,
        /// and those properties will be copied back to the original on an apply changes or an ok click.
        /// </summary>
        /// <param name="original"></param>
        public DetailedPolygonSymbolControl(IPolygonSymbolizer original)
        {
            InitializeComponent();
            Configure();
            Initialize(original);
        }

        private void Configure()
        {
            ccPatterns.SelectedItemChanged += ccPatterns_SelectedItemChanged;
            ccPatterns.AddClicked += ccPatterns_AddClicked;
            lblPreview.Paint += lblPreview_Paint;
            ocOutline.ChangesApplied += ocOutline_ChangesApplied;
        }

        private void ocOutline_ChangesApplied(object sender, EventArgs e)
        {
            _original.CopyProperties(_symbolizer);
        }

        private void lblPreview_Paint(object sender, PaintEventArgs e)
        {
            UpdatePreview(e.Graphics);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Assigns the original symbolizer to this control
        /// </summary>
        /// <param name="original">The polygon symbolizer to modify.</param>
        public void Initialize(IPolygonSymbolizer original)
        {
            _symbolizer = original.Copy();
            _original = original;
            ccPatterns.Patterns = _symbolizer.Patterns;
            ccPatterns.RefreshList();
            if (_symbolizer.Patterns.Count > 0)
            {
                ccPatterns.SelectedPattern = _symbolizer.Patterns[0];
            }
            ocOutline.Pattern = ccPatterns.SelectedPattern;
            UpdatePreview();
            UpdatePatternControls();
        }

        /// <summary>
        /// Forces the current settings to be written back to the original symbolizer
        /// </summary>
        public void ApplyChanges()
        {
            OnApplyChanges();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current (copied) symbolizer or initializes this control to work with the
        /// specified symbolizer as the original.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IPolygonSymbolizer Symbolizer
        {
            get { return _symbolizer; }
            set
            {
                if (value != null) Initialize(value);
            }
        }

        #endregion

        #region Events

        #endregion

        #region Protected Methods

        /// <summary>
        /// Fires the ChangesApplied event
        /// </summary>
        protected virtual void OnApplyChanges()
        {
            _original.CopyProperties(_symbolizer);
            if (ChangesApplied != null) ChangesApplied(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fires the AddToCustomSymbols event
        /// </summary>
        protected virtual void OnAddToCustomSymbols()
        {
            if (AddToCustomSymbols != null) AddToCustomSymbols(this, new PolygonSymbolizerEventArgs(_symbolizer));
        }

        #endregion

        #region Private Functions

        private void UpdatePatternControls()
        {
            _ignoreChanges = true;
            cmbScaleMode.SelectedItem = _symbolizer.ScaleMode.ToString();
            chkSmoothing.Checked = _symbolizer.Smoothing;
            _disableUnitWarning = true;
            cmbUnits.SelectedItem = _symbolizer.Units.ToString();
            _disableUnitWarning = false;
            ocOutline.Pattern = ccPatterns.SelectedPattern;
            ISimplePattern sp = ccPatterns.SelectedPattern as ISimplePattern;
            if (sp != null)
            {
                UpdateSimplePatternControls(sp);
            }
            IPicturePattern pp = ccPatterns.SelectedPattern as IPicturePattern;
            if (pp != null)
            {
                UpdatePicturePatternControls(pp);
            }
            IGradientPattern gp = ccPatterns.SelectedPattern as IGradientPattern;
            if (gp != null)
            {
                UpdateGradientPatternControls(gp);
            }
            IHatchPattern hp = ccPatterns.SelectedPattern as IHatchPattern;
            if (hp != null)
            {
                UpdateHatchPatternControls(hp);
            }
            _ignoreChanges = false;
            UpdatePreview();
        }

        private void UpdateGradientPatternControls(IGradientPattern pattern)
        {
            cmbPatternType.SelectedItem = "Gradient";
            cmbGradientType.SelectedItem = pattern.GradientType.ToString();
            sliderGradient.MinimumColor = pattern.Colors[0];
            sliderGradient.MaximumColor = pattern.Colors[pattern.Colors.Length - 1];
            angGradientAngle.Angle = (int)pattern.Angle;
        }

        private void UpdatePicturePatternControls(IPicturePattern pattern)
        {
            cmbPatternType.SelectedItem = "Picture";
            txtImage.Text = Path.GetFileName(pattern.PictureFilename);
            cmbTileMode.SelectedItem = pattern.WrapMode.ToString();
            angTileAngle.Angle = (int)pattern.Angle;
            dbxScaleX.Value = pattern.Scale.X;
            dbxScaleY.Value = pattern.Scale.Y;
        }

        private void UpdateSimplePatternControls(ISimplePattern pattern)
        {
            cmbPatternType.SelectedItem = "Simple";
            cbColorSimple.Color = pattern.FillColor;
            sldOpacitySimple.Value = pattern.Opacity;
            sldOpacitySimple.MaximumColor = pattern.FillColor.ToOpaque();
        }

        private void UpdateHatchPatternControls(IHatchPattern pattern)
        {
            cmbPatternType.SelectedItem = "Hatch";
            cmbHatchStyle.SelectedItem = pattern.HatchStyle;
            hatchForeColor.Color = pattern.ForeColor;
            hatchForeOpacity.Value = pattern.ForeColorOpacity;
            hatchBackColor.Color = pattern.BackColor;
            hatchBackOpacity.Value = pattern.BackColorOpacity;
        }

        private void UpdatePreview(Graphics g)
        {
            g.Clear(Color.White);

            _symbolizer.Draw(g, new Rectangle(5, 5, lblPreview.Width - 10, lblPreview.Height - 10));
        }

        private void UpdatePreview()
        {
            ccPatterns.Refresh();
            Graphics g = lblPreview.CreateGraphics();
            UpdatePreview(g);
            g.Dispose();
        }

        #endregion

        #region Event Handlers

        private void ccPatterns_AddClicked(object sender, EventArgs e)
        {
            string patternType = (string)cmbPatternType.SelectedItem;
            switch (patternType)
            {
                case "Simple":
                    ccPatterns.Patterns.Insert(0, new SimplePattern());
                    break;
                case "Picture":
                    ccPatterns.Patterns.Insert(0, new PicturePattern());
                    break;
                case "Gradient":
                    ccPatterns.Patterns.Insert(0, new GradientPattern());
                    break;
            }
            ccPatterns.RefreshList();
            ccPatterns.SelectedPattern = ccPatterns.Patterns[0];
            UpdatePreview();
        }

        private void ccPatterns_SelectedItemChanged(object sender, EventArgs e)
        {
            if (ccPatterns.SelectedPattern != null)
            {
                UpdatePatternControls();
            }
            UpdatePreview();
        }

        private void cmbScaleMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            _symbolizer.ScaleMode = Global.ParseEnum<ScaleMode>(cmbScaleMode.SelectedItem.ToString());
        }

        private void chkSmoothing_CheckedChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            _symbolizer.Smoothing = chkSmoothing.Checked;
            UpdatePreview();
        }

        private void cmbUnits_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            if (_disableUnitWarning) return;
            if (cmbUnits.SelectedItem.ToString() == "World" && _symbolizer.ScaleMode != ScaleMode.Geographic)
            {
                if (MessageBox.Show("Chosing this option will force the drawing to use a Geographic ScaleMode.  Do you wish to continue?", "Change Scale Mode?", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    cmbUnits.SelectedItem = _symbolizer.Units.ToString();
                    return;
                }
                _symbolizer.ScaleMode = ScaleMode.Geographic;
            }
            if (cmbUnits.SelectedItem.ToString() != "World" && _symbolizer.ScaleMode == ScaleMode.Geographic)
            {
                if (MessageBox.Show("Chosing this option will force the drawing to use a Symbolic scalemode.  Do you wish to continue?", "Change Scale Mode?", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    cmbUnits.SelectedItem = _symbolizer.Units.ToString();
                    return;
                }
                _symbolizer.ScaleMode = ScaleMode.Symbolic;
            }
            GraphicsUnit destination = (GraphicsUnit)Enum.Parse(typeof(GraphicsUnit), cmbUnits.SelectedItem.ToString());

            GraphicsUnit source = _symbolizer.Units;
            if (source == GraphicsUnit.Inch && destination == GraphicsUnit.Millimeter)
            {
            }
            if (source == GraphicsUnit.Millimeter && destination == GraphicsUnit.Inch)
            {
            }

            UpdatePatternControls();
        }

        private void cbColorSimple_ColorChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            ISimplePattern sp = ccPatterns.SelectedPattern as ISimplePattern;
            if (sp != null)
            {
                sp.FillColor = cbColorSimple.Color;
            }
            sldOpacitySimple.MaximumColor = cbColorSimple.Color.ToOpaque();
            UpdatePreview();
        }

        private void sldOpacitySimple_ValueChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            ISimplePattern sp = ccPatterns.SelectedPattern as ISimplePattern;
            if (sp != null)
            {
                sp.Opacity = (float)sldOpacitySimple.Value;
                _ignoreChanges = true;
                cbColorSimple.Color = sp.FillColor;
                _ignoreChanges = false;
            }
            UpdatePreview();
        }

        private void btnAddToCustom_Click(object sender, EventArgs e)
        {
            OnAddToCustomSymbols();
        }

        private void cmbPatternType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((string)cmbPatternType.SelectedItem == "Simple")
            {
                if (tabPatternProperties.TabPages.Contains(tabPicture))
                {
                    tabPatternProperties.TabPages.Remove(tabPicture);
                }
                if (tabPatternProperties.TabPages.Contains(tabGradient))
                {
                    tabPatternProperties.TabPages.Remove(tabGradient);
                }
                if (tabPatternProperties.TabPages.Contains(tabHatch))
                {
                    tabPatternProperties.TabPages.Remove(tabHatch);
                }
                if (tabPatternProperties.TabPages.Contains(tabSimple) == false)
                {
                    tabPatternProperties.TabPages.Add(tabSimple);
                    tabPatternProperties.SelectedTab = tabSimple;
                }
            }
            if ((string)cmbPatternType.SelectedItem == "Picture")
            {
                if (tabPatternProperties.TabPages.Contains(tabSimple))
                {
                    tabPatternProperties.TabPages.Remove(tabSimple);
                }
                if (tabPatternProperties.TabPages.Contains(tabGradient))
                {
                    tabPatternProperties.TabPages.Remove(tabGradient);
                }
                if (tabPatternProperties.TabPages.Contains(tabHatch))
                {
                    tabPatternProperties.TabPages.Remove(tabHatch);
                }
                if (tabPatternProperties.TabPages.Contains(tabPicture) == false)
                {
                    tabPatternProperties.TabPages.Add(tabPicture);
                    tabPatternProperties.SelectedTab = tabPicture;
                }
            }
            if ((string)cmbPatternType.SelectedItem == "Gradient")
            {
                if (tabPatternProperties.TabPages.Contains(tabSimple))
                {
                    tabPatternProperties.TabPages.Remove(tabSimple);
                }
                if (tabPatternProperties.TabPages.Contains(tabPicture))
                {
                    tabPatternProperties.TabPages.Remove(tabPicture);
                }
                if (tabPatternProperties.TabPages.Contains(tabHatch))
                {
                    tabPatternProperties.TabPages.Remove(tabHatch);
                }
                if (tabPatternProperties.TabPages.Contains(tabGradient) == false)
                {
                    tabPatternProperties.TabPages.Add(tabGradient);
                    tabPatternProperties.SelectedTab = tabGradient;
                }
            }
            if ((string)cmbPatternType.SelectedItem == "Hatch")
            {
                if (tabPatternProperties.TabPages.Contains(tabSimple))
                {
                    tabPatternProperties.TabPages.Remove(tabSimple);
                }
                if (tabPatternProperties.TabPages.Contains(tabPicture))
                {
                    tabPatternProperties.TabPages.Remove(tabPicture);
                }
                if (tabPatternProperties.TabPages.Contains(tabGradient))
                {
                    tabPatternProperties.TabPages.Remove(tabGradient);
                }
                if (tabPatternProperties.TabPages.Contains(tabHatch) == false)
                {
                    tabPatternProperties.TabPages.Add(tabHatch);
                    tabPatternProperties.SelectedTab = tabHatch;
                }
            }
            if (_ignoreChanges) return;
            int index = ccPatterns.Patterns.IndexOf(ccPatterns.SelectedPattern);
            if (index == -1) return;
            IPattern oldPattern = ccPatterns.SelectedPattern;
            if ((string)cmbPatternType.SelectedItem == "Simple")
            {
                SimplePattern sp = new SimplePattern();
                if (oldPattern != null) sp.CopyOutline(oldPattern);
                ccPatterns.Patterns[index] = sp;
                ccPatterns.RefreshList();
                ccPatterns.SelectedPattern = sp;
                UpdateSimplePatternControls(sp);
            }
            if ((string)cmbPatternType.SelectedItem == "Picture")
            {
                PicturePattern pp = new PicturePattern();
                if (oldPattern != null) pp.CopyOutline(oldPattern);
                ccPatterns.Patterns[index] = pp;
                ccPatterns.RefreshList();
                ccPatterns.SelectedPattern = pp;
                UpdatePicturePatternControls(pp);
            }
            if ((string)cmbPatternType.SelectedItem == "Gradient")
            {
                GradientPattern gp = new GradientPattern();
                if (oldPattern != null) gp.CopyOutline(oldPattern);
                ccPatterns.Patterns[index] = gp;
                ccPatterns.RefreshList();
                ccPatterns.SelectedPattern = gp;
                UpdateGradientPatternControls(gp);
            }
            if ((string)cmbPatternType.SelectedItem == "Hatch")
            {
                HatchPattern hp = new HatchPattern();
                if (oldPattern != null) hp.CopyOutline(oldPattern);
                ccPatterns.Patterns[index] = hp;
                ccPatterns.RefreshList();
                ccPatterns.SelectedPattern = hp;
            }
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            IPicturePattern pp = ccPatterns.SelectedPattern as IPicturePattern;
            if (pp != null)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = pp.DialogFilter;
                if (ofd.ShowDialog() != DialogResult.OK) return;
                pp.Open(ofd.FileName);
                txtImage.Text = Path.GetFileName(ofd.FileName);
            }
            UpdatePreview();
        }

        private void cmbTileMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            IPicturePattern pp = ccPatterns.SelectedPattern as IPicturePattern;
            if (pp != null)
            {
                pp.WrapMode = Global.ParseEnum<WrapMode>((string)cmbTileMode.SelectedItem);
            }
            UpdatePreview();
        }

        private void angTileAngle_AngleChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            IPicturePattern pp = ccPatterns.SelectedPattern as IPicturePattern;
            if (pp != null)
            {
                pp.Angle = angTileAngle.Angle;
            }
            UpdatePreview();
        }

        private void dbxScaleX_TextChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            IPicturePattern pp = ccPatterns.SelectedPattern as IPicturePattern;
            if (pp != null)
            {
                pp.Scale.X = dbxScaleX.Value;
            }
            UpdatePreview();
        }

        private void dbxScaleY_TextChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            IPicturePattern pp = ccPatterns.SelectedPattern as IPicturePattern;
            if (pp != null)
            {
                pp.Scale.Y = dbxScaleY.Value;
            }
            UpdatePreview();
        }

        #endregion

        private void ocOutline_OutlineChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void angGradientAngle_AngleChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            IGradientPattern gp = ccPatterns.SelectedPattern as IGradientPattern;
            if (gp != null)
            {
                gp.Angle = angGradientAngle.Angle;
            }
            UpdatePreview();
        }

        private void cmbGradientType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            IGradientPattern gp = ccPatterns.SelectedPattern as IGradientPattern;
            if (gp != null)
            {
                gp.GradientType = Global.ParseEnum<GradientType>((string)cmbGradientType.SelectedItem);
                if (gp.GradientType == GradientType.Linear)
                {
                    lblStartColor.Text = "Start Color";
                    lblEndColor.Text = "End Color";
                }
                else
                {
                    lblStartColor.Text = "Surround Color";
                    lblEndColor.Text = "Center Color";
                }
            }

            UpdatePreview();
        }

        private void sliderGradient_GradientChanging(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            IGradientPattern gp = ccPatterns.SelectedPattern as IGradientPattern;
            if (gp != null)
            {
                gp.Colors = new[] { sliderGradient.MinimumColor, sliderGradient.MinimumColor, sliderGradient.MaximumColor, sliderGradient.MaximumColor };
                gp.Positions = new[] { 0F, sliderGradient.StartValue, sliderGradient.EndValue, 1F };
            }
            UpdatePreview();
        }

        private void hatchForeColor_ColorChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            IHatchPattern hp = ccPatterns.SelectedPattern as IHatchPattern;
            if (hp != null)
                hp.ForeColor = hatchForeColor.Color;
            UpdatePreview();
        }

        private void hatchBackColor_ColorChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            IHatchPattern hp = ccPatterns.SelectedPattern as IHatchPattern;
            if (hp != null)
                hp.BackColor = hatchBackColor.Color;
            UpdatePreview();
        }

        private void hatchForeOpacity_ValueChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            IHatchPattern hp = ccPatterns.SelectedPattern as IHatchPattern;
            if (hp != null)
                hp.ForeColorOpacity = (float)hatchForeOpacity.Value;
            UpdatePreview();
        }

        private void hatchBackOpacity_ValueChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            IHatchPattern hp = ccPatterns.SelectedPattern as IHatchPattern;
            if (hp != null)
                hp.BackColorOpacity = (float)hatchBackOpacity.Value;
            UpdatePreview();
        }

        private void ccPatterns_Load(object sender, EventArgs e)
        {
            Array hatchs = Enum.GetValues(typeof(HatchStyle));
            foreach (object style in hatchs)
                cmbHatchStyle.Items.Add(style);
        }

        private void cmbHatchStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            IHatchPattern hp = ccPatterns.SelectedPattern as IHatchPattern;
            if (hp != null)
                hp.HatchStyle = (HatchStyle)cmbHatchStyle.SelectedItem;
            UpdatePreview();
        }
    }
}