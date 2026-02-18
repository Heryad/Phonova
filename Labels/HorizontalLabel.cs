using System;
using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.BarCode;
using DevExpress.XtraReports.UI;
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

        private XRPictureBox xrLogo;
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
        private XRLabel lblTester;
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

            // Set the label data
            ((XRControl)this.barcodeIMEI).Text = imei;
            ((XRControl)this.lblProductInfo).Text = product;

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
            
            ((XRControl)this.lblTester).Text = s.PrintTesterName ? "Tester: Dyagnoz" : "";
            ((XRControl)this.lblPort).Text = s.PrintPortNumber ? "Port: " + port : "";
            ((XRControl)this.lblCust).Text = s.PrintCustomerName ? "Cust: Phoenix" : "";
            
            if (!s.PrintCustomerName) ((XRControl)this.lblCust).Visible = false;
            if (!s.PrintTesterName) ((XRControl)this.lblTester).Visible = false;
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

            this.xrLogo = new XRPictureBox();
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
            this.lblTester = new XRLabel();
            this.lblPort = new XRLabel();

            ((ISupportInitialize)this).BeginInit();

            // Detail Band
            ((XRControl)this.Detail).Controls.AddRange(new XRControl[]
            {
                this.xrLogo,
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
                this.lblTester,
                this.lblPort
            });
            ((XRControl)this.Detail).HeightF = 197f;
            ((XRControl)this.Detail).Name = "Detail";
            ((XRControl)this.Detail).Padding = new PaddingInfo(0, 0, 0, 0, 100f);
            ((XRControl)this.Detail).TextAlignment = (DevExpress.XtraPrinting.TextAlignment)32;

            // --- TOP BAR ---
            this.xrLogo.LocationFloat = new PointFloat(5f, 5f);
            this.xrLogo.SizeF = new SizeF(110f, 35f);
            this.xrLogo.Sizing = DevExpress.XtraPrinting.ImageSizeMode.Squeeze;
            
            try 
            {
                string pngPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Assets", "label_logo.png");
                if (System.IO.File.Exists(pngPath))
                {
                    this.xrLogo.Image = System.Drawing.Image.FromFile(pngPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to load logo PNG: " + ex.Message);
            }

            this.xrLogo.ImageAlignment = DevExpress.XtraPrinting.ImageAlignment.TopCenter;
            this.xrLogo.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.xrLogo.BorderWidth = 0f;

            this.barcodeIMEI.AutoModule = true;
            this.barcodeIMEI.Font = new Font("Tahoma", 7f);
            this.barcodeIMEI.LocationFloat = new PointFloat(125f, 2f);
            this.barcodeIMEI.Name = "barcodeIMEI";
            this.barcodeIMEI.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
            this.barcodeIMEI.SizeF = new SizeF(255f, 38f);
            code128.CharacterSet = Code128Charset.CharsetAuto;
            this.barcodeIMEI.Symbology = code128;
            this.barcodeIMEI.TextAlignment = (TextAlignment)32;
            this.barcodeIMEI.ShowText = true;

            // Separator Line 1
            XRLine line1 = new XRLine();
            line1.LocationFloat = new PointFloat(5f, 42f);
            line1.SizeF = new SizeF(375f, 2f);
            this.Detail.Controls.Add(line1);

            // --- PRODUCT INFO BOX ---
            this.lblProductInfo.Font = new Font("Tahoma", 10f, FontStyle.Bold);
            this.lblProductInfo.LocationFloat = new PointFloat(5f, 46f);
            this.lblProductInfo.SizeF = new SizeF(375f, 24f);
            this.lblProductInfo.TextAlignment = (TextAlignment)32;
            this.lblProductInfo.Borders = (BorderSide)15;
            this.lblProductInfo.BorderWidth = 1f;

            // --- CENTER GRID ---
            float col1X = 10f;
            float col2X = 200f;
            float rowHeight = 15f;
            float startY = 74f;
            Font gridFont = new Font("Tahoma", 7f, FontStyle.Regular);

            this.SetupGridLabel(this.lblSerial, col1X, startY, 185f, rowHeight, gridFont);
            this.SetupGridLabel(this.lblColor, col2X, startY, 175f, rowHeight, gridFont);

            this.SetupGridLabel(this.lblVersion, col1X, startY + rowHeight, 185f, rowHeight, gridFont);
            this.SetupGridLabel(this.lblBattery, col2X, startY + rowHeight, 175f, rowHeight, gridFont);

            this.SetupGridLabel(this.lbliCloud, col1X, startY + (rowHeight * 2), 185f, rowHeight, gridFont);
            this.SetupGridLabel(this.lblFMI, col2X, startY + (rowHeight * 2), 175f, rowHeight, gridFont);

            this.SetupGridLabel(this.lblMDM, col1X, startY + (rowHeight * 3), 185f, rowHeight, gridFont);
            this.SetupGridLabel(this.lblSIM, col2X, startY + (rowHeight * 3), 175f, rowHeight, gridFont);

            this.SetupGridLabel(this.lblDate, col1X, startY + (rowHeight * 4), 185f, rowHeight, gridFont);
            this.SetupGridLabel(this.lblCust, col2X, startY + (rowHeight * 4), 175f, rowHeight, gridFont);

            // Separator Line 2
            XRLine line2 = new XRLine();
            line2.LocationFloat = new PointFloat(5f, startY + (rowHeight * 5) + 3f);
            line2.SizeF = new SizeF(375f, 2f);
            this.Detail.Controls.Add(line2);

            // --- BOTTOM SECTION ---
            this.lblNotes.Font = new Font("Tahoma", 7f, FontStyle.Italic);
            this.lblNotes.LocationFloat = new PointFloat(10f, 155f);
            this.lblNotes.SizeF = new SizeF(250f, 38f);
            this.lblNotes.TextAlignment = (TextAlignment)16;
            this.lblNotes.Padding = new PaddingInfo(2, 2, 2, 2, 100f);
            this.lblNotes.Multiline = true;
            this.lblNotes.WordWrap = true;
            this.lblNotes.CanGrow = true;

            this.lblTester.Font = new Font("Tahoma", 7f, FontStyle.Regular);
            this.lblTester.LocationFloat = new PointFloat(270f, 155f);
            this.lblTester.SizeF = new SizeF(105f, 16f);
            this.lblTester.TextAlignment = (TextAlignment)64;

            this.lblPort.Font = new Font("Tahoma", 7f, FontStyle.Regular);
            this.lblPort.LocationFloat = new PointFloat(270f, 171f);
            this.lblPort.SizeF = new SizeF(105f, 16f);
            this.lblPort.TextAlignment = (TextAlignment)64;

            // Margins & Page Settings
            this.TopMargin.HeightF = 0f;
            this.BottomMargin.HeightF = 0f;

            this.Bands.AddRange(new Band[] { this.Detail, this.TopMargin, this.BottomMargin });

            this.Margins = new Margins(12, 0, 0, 0);
            this.PageHeight = 197;
            this.PageWidth = 394;
            this.Landscape = true;
            this.PaperKind = PaperKind.Custom;
            this.PaperName = "User defined";
            this.ShowPreviewMarginLines = false;
            this.ShowPrintMarginsWarning = false;
            this.ShowPrintStatusDialog = false;
            this.Version = "18.1";

            ((ISupportInitialize)this).EndInit();
        }

        private void SetupGridLabel(XRLabel label, float x, float y, float w, float h, Font font)
        {
            label.Font = font;
            label.LocationFloat = new PointFloat(x, y);
            label.SizeF = new SizeF(w, h);
            label.TextAlignment = (TextAlignment)16;
            label.Padding = new PaddingInfo(2, 0, 0, 0, 100f);
        }
    }
}
