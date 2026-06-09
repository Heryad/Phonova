using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.BarCode;
using DevExpress.XtraPrinting.Shape;
using DevExpress.XtraReports.UI;
using System;
using System.ComponentModel;
using System.IO;
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
 
        private XRPictureBox picLogo;
        private XRPictureBox picPortIcon;
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
            string notes = "",
            string customerName = "",
            bool isSynced = true)
        {
            InitializeComponent();
 
            var s = Services.SettingsManager.Current;

            ((XRControl)this.barcodeIMEI).Text = imei;
            ((XRControl)this.lblProductInfo).Text = product + " - (" + model + ")";
 
            ((XRControl)this.lblSerial).Text = "Serial: " + (serial ?? "-");
            ((XRControl)this.lblColor).Text = s.PrintDeviceColor ? "Color: " + (color ?? "-") : "";
            ((XRControl)this.lblVersion).Text = "Version: " + (version ?? "-");
            ((XRControl)this.lblBattery).Text = "Battery: " + (battery ?? "-");

            // ── Grid Cell Mappings ──
            // iCloud is replaced with merged FMI/iCloud: ON or OFF
            string icloudVal = (icloud ?? "-").Trim().ToUpper();
            ((XRControl)this.lbliCloud).Text = "FMI/iCloud: " + icloudVal;
 
            // FMI is replaced with SIM status
            ((XRControl)this.lblFMI).Text = "SIM: " + (sim ?? "-");
 
            // MDM stays as MDM status
            ((XRControl)this.lblMDM).Text = "MDM: " + (mdm ?? "-");
 
            // SIM cell (free cell) is used for Work status:
            string workStatus = "";
            if (s.MmrMode)
            {
                workStatus = "Pending - MMR MODE";
            }
            else if (!isSynced)
            {
                workStatus = "Pending";
            }
            else if (string.IsNullOrEmpty(notes))
            {
                workStatus = "100% working phone";
            }
            else
            {
                workStatus = "Not Working Phone";
            }
            ((XRControl)this.lblSIM).Text = workStatus;
 
            ((XRControl)this.lblDate).Text = "Date: " + DateTime.Now.ToString("yyyy-MM-dd");
            ((XRControl)this.lblNotes).Text = string.IsNullOrEmpty(notes) ? "" : notes;
            
            int lineCount = 1;
            if (!string.IsNullOrEmpty(notes))
            {
                string[] lines = notes.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                int estimated = 0;
                foreach (var l in lines)
                {
                    // With full width notes (266f), printable width is ~258f.
                    // At 6.5f font, roughly 55 characters fit per line.
                    estimated += Math.Max(1, (int)Math.Ceiling(l.Length / 55.0));
                }
                lineCount = Math.Max(lines.Length, estimated);
            }

            bool isMultiLine = lineCount > 1;
            if (s.MmrMode)
            {
                this.lblNotes.TextAlignment = isMultiLine ? TextAlignment.TopCenter : TextAlignment.MiddleCenter;
            }
            else
            {
                this.lblNotes.TextAlignment = isMultiLine ? TextAlignment.TopLeft : TextAlignment.MiddleLeft;
            }
            
            if (s.MmrMode)
            {
                if (lineCount >= 3)
                {
                    this.lblNotes.Font = new Font("Tahoma", 5.72f, FontStyle.Regular);
                }
                else if (lineCount == 2)
                {
                    this.lblNotes.Font = new Font("Tahoma", 6.6f, FontStyle.Regular);
                }
                else
                {
                    this.lblNotes.Font = new Font("Tahoma", 7.15f, FontStyle.Regular);
                }
            }
            else
            {
                if (lineCount >= 3)
                {
                    this.lblNotes.Font = new Font("Tahoma", 5.2f, FontStyle.Regular);
                }
                else if (lineCount == 2)
                {
                    this.lblNotes.Font = new Font("Tahoma", 6.0f, FontStyle.Regular);
                }
                else
                {
                    this.lblNotes.Font = new Font("Tahoma", 6.5f, FontStyle.Regular);
                }
            }
 
            // Strip leading zeros from the port number
            string cleanPort = (port ?? "").Trim();
            while (cleanPort.StartsWith("0") && cleanPort.Length > 1)
            {
                cleanPort = cleanPort.Substring(1);
            }
            ((XRControl)this.lblPort).Text = s.PrintPortNumber ? cleanPort : "";
            ((XRControl)this.lblCust).Text = s.PrintCustomerName ? (string.IsNullOrEmpty(customerName) ? "" : customerName) : "";
 
            if (!s.PrintPortNumber)
            {
                ((XRControl)this.lblPort).Visible = false;
                ((XRControl)this.picPortIcon).Visible = false;
                this.lblProductInfo.SizeF = new SizeF(266f, 23f);
                this.lblProductInfo.TextAlignment = TextAlignment.MiddleCenter;
                this.lblProductInfo.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
            }
            else
            {
                ((XRControl)this.lblPort).Visible = true;
                ((XRControl)this.picPortIcon).Visible = true;
                this.lblProductInfo.SizeF = new SizeF(205f, 23f);
                this.lblProductInfo.TextAlignment = TextAlignment.MiddleLeft;
                this.lblProductInfo.Padding = new PaddingInfo(10, 0, 0, 0, 100f);
            }
 
            if (!s.PrintCustomerName) ((XRControl)this.lblCust).Visible = false;
            if (!s.PrintPortNumber) ((XRControl)this.lblPort).Visible = false;
            if (!s.PrintPortNumber) ((XRControl)this.picPortIcon).Visible = false;
            if (!s.PrintDeviceColor) ((XRControl)this.lblColor).Visible = false;
            if (!s.PrintLogo) ((XRControl)this.picLogo).Visible = false;
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
 
            this.picLogo = new XRPictureBox();
            this.picPortIcon = new XRPictureBox();
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
                this.picLogo,
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
                this.lblPort,
                this.picPortIcon
            });
            ((XRControl)this.Detail).HeightF = 197f;          // 50mm
            ((XRControl)this.Detail).Name = "Detail";
            ((XRControl)this.Detail).Padding = new PaddingInfo(0, 0, 0, 0, 100f);
            ((XRControl)this.Detail).TextAlignment = (DevExpress.XtraPrinting.TextAlignment)32;
 
            // ── Logo (left) + Barcode (right) — side by side in the same row ──────────
            const float topRowY = 6f;
            const float topRowH = 46f;   // height shared by logo and barcode
            const float logoW = 110f;  // logo panel width
            const float barcodeX = 5f + logoW + 4f;   // 119
            const float barcodeW = 240f - logoW - 4f;  // 152
 
            var sGlobal = Services.SettingsManager.Current;
            bool printLogo = sGlobal.PrintLogo;
 
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "label.png");
            this.picLogo.ImageUrl = logoPath;
            this.picLogo.Sizing = ImageSizeMode.ZoomImage;
            this.picLogo.LocationFloat = new PointFloat(10f, topRowY + 6f);
            this.picLogo.SizeF = new SizeF(logoW - 10, topRowH - 6f);
            this.picLogo.Borders = BorderSide.None;
            this.picLogo.BackColor = Color.Transparent;
            this.picLogo.Name = "picLogo";
 
            // ── Barcode — right of the logo (or full width if logo is hidden) ──────────
            float barcodeXFinal = printLogo ? (barcodeX + 25f) : 5f;
            float barcodeWFinal = printLogo ? barcodeW : 266f;
 
            this.barcodeIMEI.AutoModule = true;
            this.barcodeIMEI.Font = new Font("Tahoma", 7f);
            this.barcodeIMEI.LocationFloat = new PointFloat(barcodeXFinal, topRowY);
            this.barcodeIMEI.Name = "barcodeIMEI";
            this.barcodeIMEI.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
            this.barcodeIMEI.SizeF = new SizeF(barcodeWFinal, topRowH);
            code128.CharacterSet = Code128Charset.CharsetAuto;
            this.barcodeIMEI.Symbology = code128;
            this.barcodeIMEI.TextAlignment = (TextAlignment)32;
            this.barcodeIMEI.ShowText = true;
 
            if (!printLogo)
            {
                this.picLogo.Visible = false;
            }
 
            // ── Product Info Box ─────────────────────────────────────────────────────
            float productY = topRowY + topRowH + 4f;  // below the top row
            this.lblProductInfo.Font = new Font("Tahoma", 6.6f, FontStyle.Bold);
            this.lblProductInfo.LocationFloat = new PointFloat(5f, productY);
            this.lblProductInfo.SizeF = new SizeF(205f, 23f);
            this.lblProductInfo.TextAlignment = TextAlignment.MiddleLeft;
            this.lblProductInfo.Padding = new PaddingInfo(10, 0, 0, 0, 100f);
            this.lblProductInfo.Borders = BorderSide.None;
            this.lblProductInfo.BackColor = Color.Transparent;
            this.lblProductInfo.WordWrap = false;
            this.lblProductInfo.Multiline = false;
 
            this.lblPort.Font = new Font("Tahoma", 6.6f, FontStyle.Bold);
            this.lblPort.LocationFloat = new PointFloat(212f, productY);
            this.lblPort.SizeF = new SizeF(28f, 23f);
            this.lblPort.TextAlignment = TextAlignment.MiddleRight;
            this.lblPort.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
            this.lblPort.Borders = BorderSide.None;
            this.lblPort.BackColor = Color.Transparent;
 
            this.picPortIcon.LocationFloat = new PointFloat(242f, productY + 3.5f);
            this.picPortIcon.SizeF = new SizeF(16f, 16f);
            this.picPortIcon.Sizing = ImageSizeMode.ZoomImage;
            this.picPortIcon.Borders = BorderSide.None;
            this.picPortIcon.BackColor = Color.Transparent;
            
            string usbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "usb.png");
            this.picPortIcon.ImageUrl = usbPath;
 
            XRShape shapeProductBox = new XRShape();
            shapeProductBox.LocationFloat = new PointFloat(5f, productY);
            shapeProductBox.SizeF = new SizeF(266f, 23f);
            shapeProductBox.Shape = new ShapeRectangle() { Fillet = 40 };
            shapeProductBox.FillColor = Color.Transparent;
            shapeProductBox.ForeColor = Color.Black;
            shapeProductBox.LineWidth = 2;
            shapeProductBox.Borders = BorderSide.None;
            shapeProductBox.BackColor = Color.Transparent;
            ((XRControl)this.Detail).Controls.Add(shapeProductBox);
 
            // ── Table geometry — positioned below product info ────────────────────────
            float tableX = 5f;
            float tableY = productY + 27f;   // below product info box
            float col1W = 132.5f;
            float col2W = 132.5f;
            float rowH = 14f;
            int ROWS = 5;
            float tableW = col1W + 1f + col2W;   // 266
            float tableH = 104f;                 // merged height

            // Footer column widths
            float colAW = 90f;
            float colBW = 75f;
            float colCW = tableW - colAW - colBW - 2f;

            float divX = tableX + col1W;
            float divAX = tableX + colAW;
            float divBX = divAX + 1f + colBW;
            float footerY = tableY + rowH * 4;
            float footerRowH = 18f; // Fixed height for footer row
 
            Font cellFont = new Font("Tahoma", 6.5f, FontStyle.Regular);
 
            // ── Outer border — rounded rectangle shape ────────────────────────────────
            XRShape shapeTableBox = new XRShape();
            shapeTableBox.LocationFloat = new PointFloat(tableX, tableY);
            shapeTableBox.SizeF = new SizeF(tableW, tableH);
            shapeTableBox.Shape = new ShapeRectangle() { Fillet = 8 };
            shapeTableBox.FillColor = Color.Transparent;
            shapeTableBox.ForeColor = Color.Black;
            shapeTableBox.LineWidth = 1;
            shapeTableBox.Borders = BorderSide.None;
            shapeTableBox.BackColor = Color.Transparent;
            ((XRControl)this.Detail).Controls.Add(shapeTableBox);
 
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
            vDivA.SizeF = new SizeF(1f, footerRowH);
            vDivA.ForeColor = Color.Black;
            vDivA.LineWidth = 1;
            vDivA.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(vDivA);
 
            XRLine vDivB = new XRLine();
            vDivB.LineDirection = LineDirection.Vertical;
            vDivB.LocationFloat = new PointFloat(divBX, footerY);
            vDivB.SizeF = new SizeF(1f, footerRowH);
            vDivB.ForeColor = Color.Black;
            vDivB.LineWidth = 1;
            vDivB.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(vDivB);

            // ── Divider between Footer and Notes ──────────────────────────────────────
            XRLine hLineNotes = new XRLine();
            hLineNotes.LineDirection = LineDirection.Horizontal;
            hLineNotes.LocationFloat = new PointFloat(tableX, footerY + footerRowH);
            hLineNotes.SizeF = new SizeF(tableW, 1f);
            hLineNotes.ForeColor = Color.Black;
            hLineNotes.LineWidth = 1;
            hLineNotes.Borders = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(hLineNotes);
 
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
            PlaceCell(this.lblSerial, tableX, tableY, col1W, rowH);
            PlaceCell(this.lblColor, divX + 1f, tableY, col2W, rowH);
 
            PlaceCell(this.lblVersion, tableX, tableY + rowH, col1W, rowH);
            PlaceCell(this.lblBattery, divX + 1f, tableY + rowH, col2W, rowH);
 
            PlaceCell(this.lbliCloud, tableX, tableY + rowH * 2, col1W, rowH);
            PlaceCell(this.lblFMI, divX + 1f, tableY + rowH * 2, col2W, rowH);
 
            PlaceCell(this.lblMDM, tableX, tableY + rowH * 3, col1W, rowH);
            PlaceCell(this.lblSIM, divX + 1f, tableY + rowH * 3, col2W, rowH);
 
            // ── Footer row ────────────────────────────────────────────────────────────
            PlaceCell(this.lblDate, tableX, footerY, colAW, footerRowH);
 
            XRLabel lblBrand = new XRLabel();
            lblBrand.Text = "Tester 1";
            lblBrand.Font = new Font("Tahoma", 6.5f, FontStyle.Bold);
            lblBrand.LocationFloat = new PointFloat(divAX + 1f + 3f, footerY + 1f);
            lblBrand.SizeF = new SizeF(colBW - 4f, footerRowH - 2f);
            lblBrand.TextAlignment = (TextAlignment)32;
            lblBrand.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
            lblBrand.Borders = BorderSide.None;
            lblBrand.BackColor = Color.Transparent;
            ((XRControl)this.Detail).Controls.Add(lblBrand);
 
            PlaceCell(this.lblCust, divBX + 1f, footerY, colCW, footerRowH, (TextAlignment)16);
 
            // ── Below-table strip: Notes — now merged ─────────────────────
            float notesY = footerY + footerRowH;
            float notesBoxH = tableH - (notesY - tableY); 
            float leftW = tableW;                   // always full width (266f)
 
            this.lblNotes.Font = new Font("Tahoma", 6.5f, FontStyle.Regular);
            this.lblNotes.LocationFloat = new PointFloat(tableX + 4f, notesY + 1f);
            this.lblNotes.SizeF = new SizeF(leftW - 8f, notesBoxH - 2f);
            this.lblNotes.TextAlignment = (TextAlignment)16;
            this.lblNotes.Padding = new PaddingInfo(2, 2, 1, 1, 100f);
            this.lblNotes.Multiline = true;
            this.lblNotes.WordWrap = true;
            this.lblNotes.CanGrow = false;
            this.lblNotes.Borders = BorderSide.None;
            this.lblNotes.BackColor = Color.Transparent;
 
            // ── Page / Margin Settings ────────────────────────────────────────────────
            this.TopMargin.HeightF = 0f;
            this.BottomMargin.HeightF = 0f;
 
            this.Bands.AddRange(new Band[] { this.Detail, this.TopMargin, this.BottomMargin });
 
            this.Margins = new Margins(0, 0, 0, 0);  // ~2.5mm left offset to correct print alignment
            this.PageHeight = 197;   // 50mm
            this.PageWidth = 276;   // 70mm
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