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
                workStatus = "100% WORKING";
            else
                workStatus = "NOT WORKING";

            // Header Row: Checkmark + VERIFIED DEVICE
            XRShape shapeCircle = new XRShape();
            shapeCircle.LocationFloat = new PointFloat(10f, 10f);
            shapeCircle.SizeF = new SizeF(20f, 20f);
            shapeCircle.Shape = new ShapeEllipse();
            shapeCircle.FillColor = Color.Transparent;
            shapeCircle.ForeColor = Color.Black;
            shapeCircle.LineWidth = 2;
            shapeCircle.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(shapeCircle);

            XRLabel lblCheck = new XRLabel();
            lblCheck.Text = "✔";
            lblCheck.Font = new Font("Tahoma", 10f, FontStyle.Bold);
            lblCheck.LocationFloat = new PointFloat(10f, 10f);
            lblCheck.SizeF = new SizeF(20f, 20f);
            lblCheck.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            lblCheck.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(lblCheck);

            XRLabel lblVerified = new XRLabel();
            lblVerified.Text = "VERIFIED DEVICE";
            lblVerified.Font = new Font("Tahoma", 12f, FontStyle.Bold);
            lblVerified.LocationFloat = new PointFloat(36f, 10f);
            lblVerified.SizeF = new SizeF(230f, 20f);
            lblVerified.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            lblVerified.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(lblVerified);

            // Row 2: Phone Name
            XRLabel lblPhone = new XRLabel();
            lblPhone.Text = product;
            lblPhone.Font = new Font("Tahoma", 14f, FontStyle.Bold);
            lblPhone.LocationFloat = new PointFloat(10f, 38f);
            lblPhone.SizeF = new SizeF(256f, 24f);
            lblPhone.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            lblPhone.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(lblPhone);

            // Row 3: Status
            XRLabel lblStatus = new XRLabel();
            lblStatus.Text = "Tested • " + workStatus;
            lblStatus.Font = new Font("Tahoma", 9f, FontStyle.Regular);
            lblStatus.LocationFloat = new PointFloat(10f, 66f);
            lblStatus.SizeF = new SizeF(256f, 18f);
            lblStatus.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            lblStatus.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(lblStatus);

            // Separator Line
            XRLine sepLine = new XRLine();
            sepLine.LocationFloat = new PointFloat(10f, 92f);
            sepLine.SizeF = new SizeF(256f, 2f);
            sepLine.LineWidth = 2;
            ((XRControl)this.Detail).Controls.Add(sepLine);

            // Bottom Grid (3 Columns)
            float gridY = 100f;
            float colW = 256f / 3f;
            
            // Col 1: IMEI
            XRLabel lblImeiTitle = new XRLabel { Text = "IMEI:", Font = new Font("Tahoma", 6.5f), LocationFloat = new PointFloat(10f, gridY), SizeF = new SizeF(colW, 14f), TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft, Borders = BorderSide.None };
            XRLabel lblImeiVal = new XRLabel { Text = imei, Font = new Font("Tahoma", 8f, FontStyle.Bold), LocationFloat = new PointFloat(10f, gridY + 14f), SizeF = new SizeF(colW, 18f), TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft, Borders = BorderSide.None };
            
            // Col 2: Battery
            XRLabel lblBatTitle = new XRLabel { Text = "BATTERY:", Font = new Font("Tahoma", 6.5f), LocationFloat = new PointFloat(10f + colW, gridY), SizeF = new SizeF(colW, 14f), TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft, Borders = BorderSide.None };
            XRLabel lblBatVal = new XRLabel { Text = battery ?? "-", Font = new Font("Tahoma", 8f, FontStyle.Bold), LocationFloat = new PointFloat(10f + colW, gridY + 14f), SizeF = new SizeF(colW, 18f), TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft, Borders = BorderSide.None };
            
            // Col 3: Warranty
            XRLabel lblWarTitle = new XRLabel { Text = "WARRANTY:", Font = new Font("Tahoma", 6.5f), LocationFloat = new PointFloat(10f + colW * 2, gridY), SizeF = new SizeF(colW, 14f), TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft, Borders = BorderSide.None };
            XRLabel lblWarVal = new XRLabel { Text = s.WarrantyText, Font = new Font("Tahoma", 8f, FontStyle.Bold), LocationFloat = new PointFloat(10f + colW * 2, gridY + 14f), SizeF = new SizeF(colW, 18f), TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft, Borders = BorderSide.None };

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
