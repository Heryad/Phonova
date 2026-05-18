using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.BarCode;
using DevExpress.XtraPrinting.Shape;
using DevExpress.XtraReports.UI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows;
using FontStyle = System.Drawing.FontStyle;
using TextAlignment = DevExpress.XtraPrinting.TextAlignment;

namespace Dyagnoz_Latest
{
    public class HorizontalLabel : XtraReport
    {
        private IContainer components = null;
        private DetailBand Detail;
        private TopMarginBand TopMargin;
        private BottomMarginBand BottomMargin;

        private XRBarCode barcodeIMEI;
        private XRLabel lblProductInfo;

        private XRLabel lblSerial;
        private XRLabel lblColor;
        private XRLabel lblVersion;
        private XRLabel lblBattery;
        private XRLabel lbliCloud;
        private XRLabel lblFMI;
        private XRLabel lblMDM;
        private XRLabel lblSIM;
        private XRLabel lblDate;
        private XRLabel lblCust;

        private XRLabel lblNotes;
        private XRLabel lblPort;

        public HorizontalLabel(
            string imei,
            string serial,
            string model,
            string product,
            string color,
            string version,
            string battery,
            string icloud,
            string fmi,
            string mdm,
            string sim,
            string port,
            string notes = "")
        {
            InitializeComponent();

            ((XRControl)this.barcodeIMEI).Text = imei;
            ((XRControl)this.lblProductInfo).Text = product + " - (" + model + ")";

            ((XRControl)this.lblSerial).Text = "Serial: " + (serial ?? "-");
            ((XRControl)this.lblColor).Text = "Color: " + (color ?? "-");
            ((XRControl)this.lblVersion).Text = "Version: " + (version ?? "-");
            ((XRControl)this.lblBattery).Text = "Battery: " + (battery ?? "-");
            ((XRControl)this.lbliCloud).Text = "iCloud: " + (icloud ?? "-");
            ((XRControl)this.lblFMI).Text = "FMI: " + (fmi ?? "-");
            ((XRControl)this.lblMDM).Text = "MDM: " + (mdm ?? "-");
            ((XRControl)this.lblSIM).Text = "SIM: " + (sim ?? "-");
            ((XRControl)this.lblDate).Text = "Date: " + DateTime.Now.ToString("yyyy-MM-dd");

            var s = Services.SettingsManager.Current;

            ((XRControl)this.lblNotes).Text = string.IsNullOrEmpty(notes) ? "Notes: -" : "Notes: " + notes;
            ((XRControl)this.lblPort).Text = s.PrintPortNumber ? "Port: " + port : "";
            ((XRControl)this.lblCust).Text = s.PrintCustomerName ? "DEMO VER" : "";

            if (!s.PrintCustomerName) ((XRControl)this.lblCust).Visible = false;
            if (!s.PrintPortNumber) ((XRControl)this.lblPort).Visible = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Code128Generator code128 = new Code128Generator();

            this.Detail = new DetailBand();
            this.TopMargin = new TopMarginBand();
            this.BottomMargin = new BottomMarginBand();

            this.barcodeIMEI = new XRBarCode();
            this.lblProductInfo = new XRLabel();

            this.lblSerial = new XRLabel();
            this.lblColor = new XRLabel();
            this.lblVersion = new XRLabel();
            this.lblBattery = new XRLabel();
            this.lbliCloud = new XRLabel();
            this.lblFMI = new XRLabel();
            this.lblMDM = new XRLabel();
            this.lblSIM = new XRLabel();
            this.lblDate = new XRLabel();
            this.lblCust = new XRLabel();

            this.lblNotes = new XRLabel();
            this.lblPort = new XRLabel();

            ((ISupportInitialize)this).BeginInit();

            // ── Detail Band ──────────────────────────────────────────────────────────
            ((XRControl)this.Detail).Controls.AddRange(new XRControl[]
            {
                this.barcodeIMEI,
                this.lblProductInfo,
                this.lblSerial,
                this.lblColor,
                this.lblVersion,
                this.lblBattery,
                this.lbliCloud,
                this.lblFMI,
                this.lblMDM,
                this.lblSIM,
                this.lblDate,
                this.lblCust,
                this.lblNotes,
                this.lblPort
            });
            ((XRControl)this.Detail).HeightF = 177f;          // was 197 × 0.9
            ((XRControl)this.Detail).Name = "Detail";
            ((XRControl)this.Detail).Padding = new PaddingInfo(0, 0, 0, 0, 100f);
            ((XRControl)this.Detail).TextAlignment = (DevExpress.XtraPrinting.TextAlignment)32;

            // ── Barcode — now starts at left edge (no logo) ──────────────────────────
            // Original was 151 × 51 at X=137. With logo gone, centre it in the full width.
            // Full usable width ≈ 265. Barcode scaled: 136 × 46. Centred X = (265-136)/2 ≈ 65.
            this.barcodeIMEI.AutoModule = true;
            this.barcodeIMEI.Font = new Font("Tahoma", 7f);
            this.barcodeIMEI.LocationFloat = new PointFloat(5f, 5f);     // left-aligned, small top gap
            this.barcodeIMEI.Name = "barcodeIMEI";
            this.barcodeIMEI.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
            this.barcodeIMEI.SizeF = new SizeF(255f, 46f);              // wider now — full width minus margins
            code128.CharacterSet = Code128Charset.CharsetAuto;
            this.barcodeIMEI.Symbology = code128;
            this.barcodeIMEI.TextAlignment = (TextAlignment)32;
            this.barcodeIMEI.ShowText = true;

            // ── Product Info Box ─────────────────────────────────────────────────────
            // Y was 62, scaled → 56. Width was 279 → 251.
            this.lblProductInfo.Font = new Font("Tahoma", 7f, FontStyle.Bold);  // font kept readable
            this.lblProductInfo.LocationFloat = new PointFloat(5f, 56f);
            this.lblProductInfo.SizeF = new SizeF(255f, 23f);            // 279×26 × 0.9 ≈ 251×23
            this.lblProductInfo.TextAlignment = (TextAlignment)32;
            this.lblProductInfo.Borders = BorderSide.None;
            this.lblProductInfo.BackColor = Color.Transparent;

            XRShape shapeProductBox = new XRShape();
            shapeProductBox.LocationFloat = new PointFloat(5f, 56f);
            shapeProductBox.SizeF = new SizeF(255f, 23f);
            shapeProductBox.Shape = new ShapeRectangle() { Fillet = 40 };
            shapeProductBox.FillColor = Color.Transparent;
            shapeProductBox.ForeColor = Color.Black;
            shapeProductBox.LineWidth = 1;
            shapeProductBox.Borders = BorderSide.None;
            shapeProductBox.BackColor = Color.Transparent;
            ((XRControl)this.Detail).Controls.Add(shapeProductBox);

            // ── Table geometry — all values scaled × 0.9 from originals ──────────────
            // Original: tableX=5, tableY=92, col1W=138, col2W=136, rowH=16, tableW=275
            const float tableX  = 5f;
            const float tableY  = 83f;      // 92 × 0.9 ≈ 83
            const float col1W   = 124f;     // 138 × 0.9 ≈ 124
            const float col2W   = 122f;     // 136 × 0.9 ≈ 122 (+ 1px divider = 247 total)
            const float rowH    = 14f;      // 16  × 0.9 ≈ 14  (kept integer-ish)
            const int   ROWS    = 5;
            const float tableW  = col1W + 1f + col2W;   // 247f
            const float tableH  = rowH * ROWS;          // 70f

            // Footer column widths — scaled × 0.9, must sum to tableW - 2
            const float colAW   = 85f;      // 95 × 0.9 ≈ 85
            const float colBW   = 68f;      // 75 × 0.9 ≈ 68
            const float colCW   = tableW - colAW - colBW - 2f;  // ≈ 92f

            const float divX    = tableX + col1W;
            const float divAX   = tableX + colAW;
            const float divBX   = divAX + 1f + colBW;
            const float footerY = tableY + rowH * 4;

            Font cellFont = new Font("Tahoma", 6.5f, FontStyle.Regular);

            // ── Outer border — 4 explicit lines (avoids XRShape 1px rendering quirk) ──
            // Top
            XRLine borderTop = new XRLine();
            borderTop.LineDirection = LineDirection.Horizontal;
            borderTop.LocationFloat = new PointFloat(tableX, tableY);
            borderTop.SizeF = new SizeF(tableW, 1f);
            borderTop.ForeColor = Color.Black; borderTop.LineWidth = 1; borderTop.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(borderTop);
            // Bottom
            XRLine borderBottom = new XRLine();
            borderBottom.LineDirection = LineDirection.Horizontal;
            borderBottom.LocationFloat = new PointFloat(tableX, tableY + tableH);
            borderBottom.SizeF = new SizeF(tableW, 1f);
            borderBottom.ForeColor = Color.Black; borderBottom.LineWidth = 1; borderBottom.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(borderBottom);
            // Left
            XRLine borderLeft = new XRLine();
            borderLeft.LineDirection = LineDirection.Vertical;
            borderLeft.LocationFloat = new PointFloat(tableX, tableY);
            borderLeft.SizeF = new SizeF(1f, tableH);
            borderLeft.ForeColor = Color.Black; borderLeft.LineWidth = 1; borderLeft.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(borderLeft);
            // Right
            XRLine borderRight = new XRLine();
            borderRight.LineDirection = LineDirection.Vertical;
            borderRight.LocationFloat = new PointFloat(tableX + tableW, tableY);
            borderRight.SizeF = new SizeF(1f, tableH);
            borderRight.ForeColor = Color.Black; borderRight.LineWidth = 1; borderRight.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(borderRight);

            // ── Main vertical divider — data rows only ────────────────────────────────
            XRLine vDivMain = new XRLine();
            vDivMain.LineDirection = LineDirection.Vertical;
            vDivMain.LocationFloat = new PointFloat(divX, tableY);
            vDivMain.SizeF = new SizeF(1f, rowH * 4f);
            vDivMain.ForeColor = Color.Black;
            vDivMain.LineWidth = 1;
            vDivMain.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(vDivMain);

            // ── Horizontal dividers — rows 1-4 ───────────────────────────────────────
            for (int i = 1; i <= 4; i++)
            {
                XRLine hLine = new XRLine();
                hLine.LineDirection = LineDirection.Horizontal;
                hLine.LocationFloat = new PointFloat(tableX, tableY + rowH * i);
                hLine.SizeF = new SizeF(tableW, 1f);
                hLine.ForeColor = Color.Black;
                hLine.LineWidth = 1;
                hLine.Borders = BorderSide.None;
                ((XRControl)this.Detail).Controls.Add(hLine);
            }

            // ── Footer vertical dividers ──────────────────────────────────────────────
            XRLine vDivA = new XRLine();
            vDivA.LineDirection = LineDirection.Vertical;
            vDivA.LocationFloat = new PointFloat(divAX, footerY);
            vDivA.SizeF = new SizeF(1f, rowH);
            vDivA.ForeColor = Color.Black;
            vDivA.LineWidth = 1;
            vDivA.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(vDivA);

            XRLine vDivB = new XRLine();
            vDivB.LineDirection = LineDirection.Vertical;
            vDivB.LocationFloat = new PointFloat(divBX, footerY);
            vDivB.SizeF = new SizeF(1f, rowH);
            vDivB.ForeColor = Color.Black;
            vDivB.LineWidth = 1;
            vDivB.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(vDivB);

            // ── Cell helper ───────────────────────────────────────────────────────────
            void PlaceCell(XRLabel lbl, float x, float y, float w, float h,
                           TextAlignment align = (TextAlignment)16)
            {
                lbl.Font = cellFont;
                lbl.LocationFloat = new PointFloat(x + 3f, y + 1f);
                lbl.SizeF = new SizeF(w - 4f, h - 2f);
                lbl.TextAlignment = align;
                lbl.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
                lbl.Borders = BorderSide.None;
                lbl.BackColor = Color.Transparent;
            }

            // ── Data rows ─────────────────────────────────────────────────────────────
            PlaceCell(this.lblSerial,  tableX,        tableY,            col1W, rowH);
            PlaceCell(this.lblColor,   divX + 1f,     tableY,            col2W, rowH);

            PlaceCell(this.lblVersion, tableX,        tableY + rowH,     col1W, rowH);
            PlaceCell(this.lblBattery, divX + 1f,     tableY + rowH,     col2W, rowH);

            PlaceCell(this.lbliCloud,  tableX,        tableY + rowH * 2, col1W, rowH);
            PlaceCell(this.lblFMI,     divX + 1f,     tableY + rowH * 2, col2W, rowH);

            PlaceCell(this.lblMDM,     tableX,        tableY + rowH * 3, col1W, rowH);
            PlaceCell(this.lblSIM,     divX + 1f,     tableY + rowH * 3, col2W, rowH);

            // ── Footer row ────────────────────────────────────────────────────────────
            PlaceCell(this.lblDate, tableX, footerY, colAW, rowH);

            XRLabel lblBrand = new XRLabel();
            lblBrand.Text = "mogy.ae";
            lblBrand.Font = new Font("Tahoma", 6.5f, FontStyle.Bold);
            lblBrand.LocationFloat = new PointFloat(divAX + 1f + 3f, footerY + 1f);
            lblBrand.SizeF = new SizeF(colBW - 4f, rowH - 2f);
            lblBrand.TextAlignment = (TextAlignment)32;
            lblBrand.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
            lblBrand.Borders = BorderSide.None;
            lblBrand.BackColor = Color.Transparent;
            ((XRControl)this.Detail).Controls.Add(lblBrand);

            PlaceCell(this.lblCust, divBX + 1f, footerY, colCW, rowH, (TextAlignment)16);

            // ── Below-table strip: Notes | Port ──────────────────────────────────────
            // Original bottomY = tableY(92) + tableH(75) + 4 = 171. Scaled: 83 + 70 + 4 = 157.
            float bottomY = tableY + tableH + 4f;   // 157f
            float lineH   = 14f;                    // was 16 × 0.9
            float rightW  = 81f;                    // was 90 × 0.9
            float rightX  = tableX + tableW - rightW;
            float leftW   = tableW - rightW - 4f;

            this.lblNotes.Font = new Font("Tahoma", 6.5f, FontStyle.Italic);
            this.lblNotes.LocationFloat = new PointFloat(tableX + 3f, bottomY);
            this.lblNotes.SizeF = new SizeF(leftW, lineH);
            this.lblNotes.TextAlignment = (TextAlignment)16;
            this.lblNotes.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
            this.lblNotes.Multiline = false;
            this.lblNotes.WordWrap = false;
            this.lblNotes.CanGrow = false;
            this.lblNotes.Borders = BorderSide.None;

            this.lblPort.Font = new Font("Tahoma", 6.5f, FontStyle.Regular);
            this.lblPort.LocationFloat = new PointFloat(rightX, bottomY);
            this.lblPort.SizeF = new SizeF(rightW, lineH);
            this.lblPort.TextAlignment = (TextAlignment)64;
            this.lblPort.Borders = BorderSide.None;

            // ── Page / Margin Settings ────────────────────────────────────────────────
            this.TopMargin.HeightF = 0f;
            this.BottomMargin.HeightF = 0f;

            this.Bands.AddRange(new Band[] { this.Detail, this.TopMargin, this.BottomMargin });

            this.Margins = new Margins(0, 0, 0, 0);
            this.PageHeight = 177;   // 197 × 0.9 ≈ 177
            this.PageWidth  = 265;   // 295 × 0.9 ≈ 265 (rounded to int)
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
