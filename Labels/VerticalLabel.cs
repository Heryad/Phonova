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
    public class VerticalLabel : XtraReport
    {
        private IContainer components = null;
        private DetailBand Detail;
        private TopMarginBand TopMargin;
        private BottomMarginBand BottomMargin;

        private XRPictureBox picLogo;
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

        public VerticalLabel(
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
            string customerName = "")
        {
            InitializeComponent();

            ((XRControl)this.barcodeIMEI).Text = imei;
            ((XRControl)this.lblProductInfo).Text = product + " - (" + model + ")";

            ((XRControl)this.lblSerial).Text  = serial  ?? "-";
            ((XRControl)this.lblColor).Text   = color   ?? "-";
            ((XRControl)this.lblVersion).Text = version ?? "-";
            ((XRControl)this.lblBattery).Text = battery ?? "-";
            ((XRControl)this.lbliCloud).Text  = icloud  ?? "-";
            ((XRControl)this.lblFMI).Text     = fmi     ?? "-";
            ((XRControl)this.lblMDM).Text     = mdm     ?? "-";
            ((XRControl)this.lblSIM).Text     = "Unlocked";
            ((XRControl)this.lblDate).Text    = DateTime.Now.ToString("yyyy-MM-dd");

            var s = Services.SettingsManager.Current;

            ((XRControl)this.lblNotes).Text = string.IsNullOrEmpty(notes) ? "-" : notes;
            ((XRControl)this.lblPort).Text  = s.PrintPortNumber ? port : "";
            ((XRControl)this.lblCust).Text  = s.PrintCustomerName ? (string.IsNullOrEmpty(customerName) ? "" : customerName) : "";

            if (!s.PrintCustomerName) ((XRControl)this.lblCust).Visible = false;
            if (!s.PrintPortNumber)   ((XRControl)this.lblPort).Visible  = false;
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

            this.Detail       = new DetailBand();
            this.TopMargin    = new TopMarginBand();
            this.BottomMargin = new BottomMarginBand();

            this.picLogo      = new XRPictureBox();
            this.barcodeIMEI  = new XRBarCode();
            this.lblProductInfo = new XRLabel();

            this.lblSerial  = new XRLabel();
            this.lblColor   = new XRLabel();
            this.lblVersion = new XRLabel();
            this.lblBattery = new XRLabel();
            this.lbliCloud  = new XRLabel();
            this.lblFMI     = new XRLabel();
            this.lblMDM     = new XRLabel();
            this.lblSIM     = new XRLabel();
            this.lblDate    = new XRLabel();
            this.lblCust    = new XRLabel();
            this.lblNotes   = new XRLabel();
            this.lblPort    = new XRLabel();

            ((ISupportInitialize)this).BeginInit();

            // ── Layout constants ──────────────────────────────────────────────────────
            // Paper: 50mm wide × 70mm tall  →  PageWidth=197, PageHeight=276 (units: 1/100 inch)
            // Usable width: 197 - 10 (left margin) - 4 (right pad) = 183
            const float X      = 7f;    // left edge
            const float W      = 183f;  // usable width
            const float CENTER = X + W / 2f;

            // ── Detail Band ───────────────────────────────────────────────────────────
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
                this.lblPort
            });
            ((XRControl)this.Detail).HeightF = 276f;
            ((XRControl)this.Detail).Name    = "Detail";
            ((XRControl)this.Detail).Padding = new PaddingInfo(0, 0, 0, 0, 100f);
            ((XRControl)this.Detail).TextAlignment = (DevExpress.XtraPrinting.TextAlignment)32;

            // ── 1. LOGO — full width, top of label ───────────────────────────────────
            const float logoY = 4f;
            const float logoH = 36f;
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "label.png");
            this.picLogo.ImageUrl    = logoPath;
            this.picLogo.Sizing      = ImageSizeMode.ZoomImage;
            this.picLogo.LocationFloat = new PointFloat(X, logoY);
            this.picLogo.SizeF       = new SizeF(W, logoH);
            this.picLogo.Borders     = BorderSide.None;
            this.picLogo.BackColor   = Color.Transparent;
            this.picLogo.Name        = "picLogo";

            // ── Thin separator line under logo ────────────────────────────────────────
            float afterLogo = logoY + logoH + 3f;  // 43
            XRLine sepLogo = new XRLine();
            sepLogo.LineDirection  = LineDirection.Horizontal;
            sepLogo.LocationFloat  = new PointFloat(X, afterLogo);
            sepLogo.SizeF          = new SizeF(W, 1f);
            sepLogo.ForeColor      = Color.FromArgb(200, 200, 200);
            sepLogo.LineWidth      = 1;
            sepLogo.Borders        = BorderSide.None;
            ((XRControl)this.Detail).Controls.Add(sepLogo);

            // ── 2. PRODUCT INFO — bold, centered ──────────────────────────────────────
            float productY = afterLogo + 4f;   // 47
            this.lblProductInfo.Font          = new Font("Tahoma", 7f, FontStyle.Bold);
            this.lblProductInfo.LocationFloat = new PointFloat(X, productY);
            this.lblProductInfo.SizeF         = new SizeF(W, 16f);
            this.lblProductInfo.TextAlignment = (TextAlignment)32;  // MiddleCenter
            this.lblProductInfo.Borders       = BorderSide.None;
            this.lblProductInfo.BackColor     = Color.Transparent;
            this.lblProductInfo.WordWrap      = true;

            // ── 3. PROPERTIES — two-column, no borders ────────────────────────────────
            // Layout: left col = key label (bold, gray), right col = value
            // Columns: col1 key | col1 val | col2 key | col2 val
            // No table borders — just alternating subtle separator lines

            float propsY    = productY + 18f;   // 65
            const float rowH    = 14f;
            const float halfW   = W / 2f;        // 91.5
            const float keyW    = 48f;            // key label width
            const float valW    = halfW - keyW - 2f; // value width ≈ 41.5
            const float col2X   = X + halfW + 2f; // second pair X

            Font keyFont = new Font("Tahoma", 6f, FontStyle.Regular);
            Font valFont = new Font("Tahoma", 6.5f, FontStyle.Bold);
            Color keyColor = Color.FromArgb(110, 110, 110);

            // Helper: place a key+value pair
            void PlacePair(XRLabel valLbl, string keyText,
                           float px, float py, float kw, float vw)
            {
                // Key label (gray, regular)
                var keyLbl = new XRLabel();
                keyLbl.Text          = keyText;
                keyLbl.Font          = keyFont;
                keyLbl.ForeColor     = keyColor;
                keyLbl.LocationFloat = new PointFloat(px, py + 1f);
                keyLbl.SizeF         = new SizeF(kw, rowH - 2f);
                keyLbl.TextAlignment = (TextAlignment)16;  // MiddleLeft
                keyLbl.Padding       = new PaddingInfo(0, 0, 0, 0, 100f);
                keyLbl.Borders       = BorderSide.None;
                keyLbl.BackColor     = Color.Transparent;
                ((XRControl)this.Detail).Controls.Add(keyLbl);

                // Value label (black, bold)
                valLbl.Font          = valFont;
                valLbl.LocationFloat = new PointFloat(px + kw, py + 1f);
                valLbl.SizeF         = new SizeF(vw, rowH - 2f);
                valLbl.TextAlignment = (TextAlignment)16;  // MiddleLeft
                valLbl.Padding       = new PaddingInfo(0, 0, 0, 0, 100f);
                valLbl.Borders       = BorderSide.None;
                valLbl.BackColor     = Color.Transparent;
            }

            // Helper: add a light separator line spanning full width
            void AddSep(float sy)
            {
                var sep = new XRLine();
                sep.LineDirection  = LineDirection.Horizontal;
                sep.LocationFloat  = new PointFloat(X, sy);
                sep.SizeF          = new SizeF(W, 1f);
                sep.ForeColor      = Color.FromArgb(230, 230, 230);
                sep.LineWidth      = 1;
                sep.Borders        = BorderSide.None;
                ((XRControl)this.Detail).Controls.Add(sep);
            }

            // Row 0: Serial | Color
            PlacePair(this.lblSerial, "Serial:",  X,       propsY,          keyW, valW);
            PlacePair(this.lblColor,  "Color:",   col2X,   propsY,          keyW, valW);
            AddSep(propsY + rowH);

            // Row 1: Version | Battery
            PlacePair(this.lblVersion, "Version:", X,       propsY + rowH,   keyW, valW);
            PlacePair(this.lblBattery, "Battery:", col2X,   propsY + rowH,   keyW, valW);
            AddSep(propsY + rowH * 2);

            // Row 2: iCloud | FMI
            PlacePair(this.lbliCloud, "iCloud:", X,       propsY + rowH * 2, keyW, valW);
            PlacePair(this.lblFMI,    "FMI:",    col2X,   propsY + rowH * 2, keyW, valW);
            AddSep(propsY + rowH * 3);

            // Row 3: MDM | SIM
            PlacePair(this.lblMDM, "MDM:", X,       propsY + rowH * 3, keyW, valW);
            PlacePair(this.lblSIM, "SIM:", col2X,   propsY + rowH * 3, keyW, valW);
            AddSep(propsY + rowH * 4);

            // Row 4: Date | Customer
            PlacePair(this.lblDate, "Date:", X,      propsY + rowH * 4, keyW, valW);
            PlacePair(this.lblCust, "Cust:", col2X,  propsY + rowH * 4, keyW, valW);
            AddSep(propsY + rowH * 5);

            // Row 5: Notes | Port  (single-row footer)
            float footerY = propsY + rowH * 5;
            this.lblNotes.Font          = new Font("Tahoma", 5.5f, FontStyle.Italic);
            this.lblNotes.ForeColor     = Color.FromArgb(90, 90, 90);
            this.lblNotes.LocationFloat = new PointFloat(X, footerY + 1f);
            this.lblNotes.SizeF         = new SizeF(W * 0.65f, rowH - 2f);
            this.lblNotes.TextAlignment = (TextAlignment)16;
            this.lblNotes.Padding       = new PaddingInfo(0, 0, 0, 0, 100f);
            this.lblNotes.Multiline     = false;
            this.lblNotes.WordWrap      = false;
            this.lblNotes.CanGrow       = false;
            this.lblNotes.Borders       = BorderSide.None;

            this.lblPort.Font          = new Font("Tahoma", 5.5f, FontStyle.Regular);
            this.lblPort.ForeColor     = Color.FromArgb(90, 90, 90);
            this.lblPort.LocationFloat = new PointFloat(X + W * 0.65f, footerY + 1f);
            this.lblPort.SizeF         = new SizeF(W * 0.35f, rowH - 2f);
            this.lblPort.TextAlignment = (TextAlignment)64; // MiddleRight
            this.lblPort.Borders       = BorderSide.None;

            // ── 4. IMEI BARCODE — full width at bottom ────────────────────────────────
            float barcodeY = footerY + rowH + 4f;
            this.barcodeIMEI.AutoModule    = true;
            this.barcodeIMEI.Font          = new Font("Tahoma", 6.5f);
            this.barcodeIMEI.LocationFloat = new PointFloat(X, barcodeY);
            this.barcodeIMEI.Name          = "barcodeIMEI";
            this.barcodeIMEI.Padding       = new PaddingInfo(2, 2, 0, 0, 100f);
            this.barcodeIMEI.SizeF         = new SizeF(W, 42f);
            code128.CharacterSet           = Code128Charset.CharsetAuto;
            this.barcodeIMEI.Symbology     = code128;
            this.barcodeIMEI.TextAlignment = (TextAlignment)32;
            this.barcodeIMEI.ShowText      = true;

            // ── Page / Margin Settings ────────────────────────────────────────────────
            this.TopMargin.HeightF    = 0f;
            this.BottomMargin.HeightF = 0f;

            this.Bands.AddRange(new Band[] { this.Detail, this.TopMargin, this.BottomMargin });

            this.Margins  = new Margins(0, 0, 0, 0);   // ~2.5mm left offset
            this.PageWidth  = 197;   // 50mm  (portrait: narrow side)
            this.PageHeight = 276;   // 70mm  (portrait: tall side)
            this.Landscape  = false;
            this.PaperKind  = PaperKind.Custom;
            this.PaperName  = "User defined";
            this.ShowPreviewMarginLines  = false;
            this.ShowPrintMarginsWarning = false;
            this.ShowPrintStatusDialog   = false;
            this.Version = "18.1";

            ((ISupportInitialize)this).EndInit();
        }
    }
}
