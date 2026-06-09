using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Shape;
using DevExpress.XtraReports.UI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;

namespace Dyagnoz_Latest
{
    public class SimpleLabel : XtraReport
    {
        private IContainer components = null;
        private DetailBand Detail;
        private TopMarginBand TopMargin;
        private BottomMarginBand BottomMargin;

        public SimpleLabel(
            string imei,
            string product,
            string battery,
            string notes = "",
            bool isSynced = true)
        {
            InitializeComponent();
            var s = Services.SettingsManager.Current;

            // Compute status
            string workStatus = "";
            if (s.MmrMode)
                workStatus = "Pending - MMR MODE";
            else if (!isSynced)
                workStatus = "Pending";
            else if (string.IsNullOrEmpty(notes))
                workStatus = "Fully Functional";
            else
                workStatus = "Needs Attention";

            // Header Row: Checkmark
            XRShape shapeCircle = new XRShape();
            shapeCircle.LocationFloat = new PointFloat(128f, 8f);
            shapeCircle.SizeF = new SizeF(20f, 20f);
            shapeCircle.Shape = new ShapeEllipse();
            shapeCircle.FillColor = Color.Transparent;
            shapeCircle.ForeColor = Color.Black;
            shapeCircle.LineWidth = 1;
            shapeCircle.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(shapeCircle);

            XRLabel lblCheck = new XRLabel();
            lblCheck.Text = "✔";
            lblCheck.Font = new Font("Tahoma", 9f, FontStyle.Regular);
            lblCheck.LocationFloat = new PointFloat(128f, 8f);
            lblCheck.SizeF = new SizeF(20f, 20f);
            lblCheck.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            lblCheck.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(lblCheck);

            // VERIFIED DEVICE
            XRLabel lblVerified = new XRLabel();
            lblVerified.Text = "V E R I F I E D   D E V I C E";
            lblVerified.Font = new Font("Tahoma", 6.5f, FontStyle.Bold);
            lblVerified.LocationFloat = new PointFloat(0f, 30f);
            lblVerified.SizeF = new SizeF(276f, 12f);
            lblVerified.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            lblVerified.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(lblVerified);

            // Row 2: Phone Name (Smaller)
            XRLabel lblPhone = new XRLabel();
            lblPhone.Text = product;
            lblPhone.Font = new Font("Tahoma", 14f, FontStyle.Bold);
            lblPhone.LocationFloat = new PointFloat(0f, 44f);
            lblPhone.SizeF = new SizeF(276f, 20f);
            lblPhone.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            lblPhone.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(lblPhone);

            // Barcode (IMEI)
            DevExpress.XtraPrinting.BarCode.Code128Generator code128 = new DevExpress.XtraPrinting.BarCode.Code128Generator();
            code128.CharacterSet = DevExpress.XtraPrinting.BarCode.Code128Charset.CharsetAuto;
            XRBarCode barcode = new XRBarCode();
            barcode.AutoModule = true;
            barcode.Font = new Font("Tahoma", 6f);
            barcode.LocationFloat = new PointFloat(38f, 66f);
            barcode.SizeF = new SizeF(200f, 26f);
            barcode.Symbology = code128;
            barcode.TextAlignment = DevExpress.XtraPrinting.TextAlignment.BottomCenter;
            barcode.ShowText = false; // Hidden since IMEI is shown clearly at the bottom
            barcode.Text = imei;
            barcode.Borders = BorderSide.None;
            barcode.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
            ((XRControl)this.Detail).Controls.Add(barcode);

            // Row 3: Status
            XRLabel lblStatus = new XRLabel();
            lblStatus.Text = "Tested  •  " + workStatus;
            lblStatus.Font = new Font("Tahoma", 8f, FontStyle.Regular);
            lblStatus.LocationFloat = new PointFloat(0f, 96f);
            lblStatus.SizeF = new SizeF(276f, 14f);
            lblStatus.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            lblStatus.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(lblStatus);

            // Separator Line
            XRLine sepLine = new XRLine();
            sepLine.LocationFloat = new PointFloat(24f, 116f);
            sepLine.SizeF = new SizeF(228f, 2f);
            sepLine.LineWidth = 1;
            ((XRControl)this.Detail).Controls.Add(sepLine);

            // Vertical Lines
            XRLine vLine1 = new XRLine();
            vLine1.LineDirection = LineDirection.Vertical;
            vLine1.LocationFloat = new PointFloat(100f, 126f);
            vLine1.SizeF = new SizeF(2f, 24f);
            vLine1.LineWidth = 1;
            ((XRControl)this.Detail).Controls.Add(vLine1);

            XRLine vLine2 = new XRLine();
            vLine2.LineDirection = LineDirection.Vertical;
            vLine2.LocationFloat = new PointFloat(176f, 126f);
            vLine2.SizeF = new SizeF(2f, 24f);
            vLine2.LineWidth = 1;
            ((XRControl)this.Detail).Controls.Add(vLine2);

            // Bottom Grid (3 Columns)
            // Col 1: IMEI
            XRLabel lblImeiTitle = new XRLabel { Text = "IMEI:", Font = new Font("Tahoma", 5.5f), LocationFloat = new PointFloat(24f, 126f), SizeF = new SizeF(72f, 12f), TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft, Borders = BorderSide.None };
            XRLabel lblImeiVal = new XRLabel { Text = imei, Font = new Font("Tahoma", 7.5f), LocationFloat = new PointFloat(24f, 138f), SizeF = new SizeF(74f, 14f), TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft, Borders = BorderSide.None };
            
            // Col 2: Battery
            XRLabel lblBatTitle = new XRLabel { Text = "BATTERY:", Font = new Font("Tahoma", 5.5f), LocationFloat = new PointFloat(104f, 126f), SizeF = new SizeF(68f, 12f), TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter, Borders = BorderSide.None };
            XRLabel lblBatVal = new XRLabel { Text = battery ?? "-", Font = new Font("Tahoma", 7.5f), LocationFloat = new PointFloat(104f, 138f), SizeF = new SizeF(68f, 14f), TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter, Borders = BorderSide.None };
            
            // Col 3: Warranty
            XRLabel lblWarTitle = new XRLabel { Text = "WARRANTY:", Font = new Font("Tahoma", 5.5f), LocationFloat = new PointFloat(180f, 126f), SizeF = new SizeF(72f, 12f), TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter, Borders = BorderSide.None };
            string warText = string.IsNullOrEmpty(s.WarrantyText) ? "30 DAYS" : s.WarrantyText.ToUpper();
            XRLabel lblWarVal = new XRLabel { Text = warText, Font = new Font("Tahoma", 7.5f), LocationFloat = new PointFloat(180f, 138f), SizeF = new SizeF(72f, 14f), TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter, Borders = BorderSide.None };

            ((XRControl)this.Detail).Controls.AddRange(new XRControl[] { lblImeiTitle, lblImeiVal, lblBatTitle, lblBatVal, lblWarTitle, lblWarVal });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Detail = new DetailBand();
            this.TopMargin = new TopMarginBand();
            this.BottomMargin = new BottomMarginBand();

            ((ISupportInitialize)this).BeginInit();

            this.Detail.HeightF = 197f;          // 50mm
            this.Detail.Name = "Detail";
            this.Detail.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
            this.Detail.TextAlignment = (DevExpress.XtraPrinting.TextAlignment)32;

            this.TopMargin.HeightF = 0f;
            this.BottomMargin.HeightF = 0f;

            this.Bands.AddRange(new Band[] { this.Detail, this.TopMargin, this.BottomMargin });

            this.Margins = new Margins(0, 0, 0, 0);
            this.PageHeight = 197;   // 50mm
            this.PageWidth = 276;    // 70mm
            this.Landscape = false;
            this.PaperKind = PaperKind.Custom;
            this.PaperName = "User defined";
            this.ShowPreviewMarginLines = false;
            this.ShowPrintMarginsWarning = false;
            this.ShowPrintStatusDialog = false;
            this.Version = "18.1";

            ((ISupportInitialize)this).EndInit();
        }
    }
}
