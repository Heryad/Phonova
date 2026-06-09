using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using Dyagnoz_Latest.Services;

namespace Dyagnoz_Latest.Labels
{
    public class DeviceCertificate : XtraReport
    {
        private DetailBand Detail;
        private TopMarginBand TopMargin;
        private BottomMarginBand BottomMargin;
        private PageHeaderBand PageHeader;
        private PageFooterBand PageFooter;

        public DeviceCertificate(ProcessedDevice device)
        {
            InitializeComponent();
            LoadLogo();
            PopulateData(device);
        }

        private void LoadLogo()
        {
            XRPictureBox logo = (XRPictureBox)this.FindControl("xrLogo", true);
            if (logo == null) return;

            string assetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
            string logoPath = Path.Combine(assetsPath, "certificate_logo.png");

            try
            {
                if (File.Exists(logoPath))
                {
                    logo.Image = Image.FromFile(logoPath);
                }
            }
            catch { }
        }

        private void InitializeComponent()
        {
            this.Detail = new DetailBand();
            this.TopMargin = new TopMarginBand();
            this.BottomMargin = new BottomMarginBand();
            this.PageHeader = new PageHeaderBand();
            this.PageFooter = new PageFooterBand();

            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();

            // Report Settings
            this.PaperKind = PaperKind.A4;
            this.Margins = new Margins(50, 50, 50, 50); // 0.5 inch margins
            this.PageHeader.HeightF = 180f;
            this.PageFooter.HeightF = 100f;
            this.Detail.HeightF = 800f;

            // Page Border
            XRCrossBandBox border = new XRCrossBandBox();
            border.StartBand = this.PageHeader;
            border.EndBand = this.PageFooter;
            border.StartPointF = new PointF(0f, 0f);
            border.EndPointF = new PointF(0f, 95f);
            border.WidthF = 727f;
            border.BorderWidth = 2f;
            border.BorderColor = Color.FromArgb(203, 213, 225);
            this.CrossBandControls.Add(border);

            // Watermark (background text)
            this.Watermark.Text = "OFFICIAL DIAGNOSTIC REPORT";
            this.Watermark.TextDirection = DirectionMode.ForwardDiagonal;
            this.Watermark.Font = new Font("Segoe UI", 40, FontStyle.Bold);
            this.Watermark.ForeColor = Color.FromArgb(241, 245, 249);
            this.Watermark.ShowBehind = true;

            // Header Section
            XRPictureBox logo = new XRPictureBox();
            logo.Name = "xrLogo";
            logo.SizeF = new SizeF(200f, 80f);
            logo.LocationF = new PointF(263f, 0f); 
            logo.Sizing = ImageSizeMode.Squeeze;
            logo.ImageAlignment = ImageAlignment.MiddleCenter;
            
            XRLabel title = new XRLabel();
            title.Text = "DEVICE DIAGNOSTIC CERTIFICATE";
            title.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(30, 41, 59);
            title.SizeF = new SizeF(727f, 40f);
            title.LocationF = new PointF(0f, 85f); // Reduced gap
            title.TextAlignment = TextAlignment.MiddleCenter;

            XRLabel subtitle = new XRLabel();
            subtitle.Text = "Comprehensive Technical Analysis & Verification Report";
            subtitle.Font = new Font("Segoe UI", 11, FontStyle.Italic);
            subtitle.ForeColor = Color.FromArgb(107, 114, 128);
            subtitle.SizeF = new SizeF(727f, 25f);
            subtitle.LocationF = new PointF(0f, 125f);
            subtitle.TextAlignment = TextAlignment.MiddleCenter;

            XRLine headerLine = new XRLine();
            headerLine.SizeF = new SizeF(727f, 2f);
            headerLine.LocationF = new PointF(0f, 170f);
            headerLine.ForeColor = Color.FromArgb(226, 232, 240);

            this.PageHeader.Controls.AddRange(new XRControl[] { logo, title, subtitle, headerLine });

            // Footer Section
            XRLine footerLine = new XRLine();
            footerLine.SizeF = new SizeF(727f, 2f);
            footerLine.LocationF = new PointF(0f, 0f);
            footerLine.ForeColor = Color.FromArgb(226, 232, 240);

            XRLabel footerText = new XRLabel();
            footerText.Text = "This certificate confirms that the device has undergone a full hardware and software diagnostic sweep.\nVerification provided by Phonova Professional Suite.";
            footerText.Font = new Font("Segoe UI", 9);
            footerText.ForeColor = Color.FromArgb(107, 114, 128);
            footerText.SizeF = new SizeF(600f, 40f);
            footerText.LocationF = new PointF(0f, 15f);
            footerText.Multiline = true;

            XRLabel dateLabel = new XRLabel();
            dateLabel.Name = "lblDate";
            dateLabel.Text = "Date: " + DateTime.Now.ToString("MMMM dd, yyyy");
            dateLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dateLabel.SizeF = new SizeF(200f, 20f);
            dateLabel.LocationF = new PointF(527f, 15f);
            dateLabel.TextAlignment = TextAlignment.TopRight;

            // Company Info
            XRLabel contactInfo = new XRLabel();
            contactInfo.Text = "phonova.com | info@phonova.com";
            contactInfo.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            contactInfo.ForeColor = Color.FromArgb(83, 66, 125);
            contactInfo.SizeF = new SizeF(300f, 20f);
            contactInfo.LocationF = new PointF(0f, 60f);
            contactInfo.TextAlignment = TextAlignment.TopLeft;

            XRLabel signatureLabel = new XRLabel();
            signatureLabel.Text = "Authorized Signature (Phonova QC)";
            signatureLabel.Font = new Font("Segoe UI", 8, FontStyle.Italic);
            signatureLabel.SizeF = new SizeF(280f, 20f);
            signatureLabel.LocationF = new PointF(447f, 75f);
            signatureLabel.TextAlignment = TextAlignment.TopRight;

            XRLine signatureLine = new XRLine();
            signatureLine.SizeF = new SizeF(280f, 2f);
            signatureLine.LocationF = new PointF(447f, 70f);
            signatureLine.ForeColor = Color.Black;

            this.PageFooter.Controls.AddRange(new XRControl[] { footerLine, footerText, dateLabel, contactInfo, signatureLabel, signatureLine });

            this.Bands.AddRange(new Band[] { this.TopMargin, this.BottomMargin, this.PageHeader, this.Detail, this.PageFooter });

            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
        }

        private void PopulateData(ProcessedDevice device)
        {
            float currentY = 20f;

            // 1. Device Info Section
            XRLabel section1 = new XRLabel();
            section1.Text = "DEVICE IDENTIFICATION";
            section1.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            section1.ForeColor = Color.FromArgb(83, 66, 125); // Brand Purple
            section1.SizeF = new SizeF(727f, 30f);
            section1.LocationF = new PointF(0f, currentY);
            this.Detail.Controls.Add(section1);
            currentY += 35f;

            XRTable tableInfo = new XRTable();
            tableInfo.SizeF = new SizeF(727f, 0f);
            tableInfo.LocationF = new PointF(0f, currentY);
            tableInfo.Borders = BorderSide.None;

            AddInfoRow(tableInfo, "Model Name", device.DeviceName ?? "-", "Serial Number", device.Serial ?? "-");
            AddInfoRow(tableInfo, "Marketing Model", device.Model ?? "-", "IMEI", device.Imei ?? "-");
            AddInfoRow(tableInfo, "Storage Capacity", device.Storage ?? "-", "iOS Version", device.IosVersion ?? "-");
            AddInfoRow(tableInfo, "Color / Enclosure", device.Color ?? "-", "Region", device.Region ?? "-");
            AddInfoRow(tableInfo, "Battery Health", (device.BatteryHealth ?? "0") + "%", "Battery Cycles", device.BatteryCycles ?? "-");

            this.Detail.Controls.Add(tableInfo);
            currentY += tableInfo.HeightF + 40f;

            // 2. Lock Status Section
            XRLabel section2 = new XRLabel();
            section2.Text = "SECURITY & LOCK STATUS";
            section2.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            section2.ForeColor = Color.FromArgb(83, 66, 125);
            section2.SizeF = new SizeF(727f, 30f);
            section2.LocationF = new PointF(0f, currentY);
            this.Detail.Controls.Add(section2);
            currentY += 35f;

            XRTable tableLock = new XRTable();
            tableLock.SizeF = new SizeF(727f, 0f);
            tableLock.LocationF = new PointF(0f, currentY);
            AddInfoRow(tableLock, "iCloud Status", device.IcloudStatus ?? "-", "FMI Status", device.FmiStatus ?? "-");
            AddInfoRow(tableLock, "MDM Status", device.MdmStatus ?? "-", "SIM Status", device.SimStatus ?? "-");
            this.Detail.Controls.Add(tableLock);
            currentY += tableLock.HeightF + 40f;

            // 3. Test Results Table
            XRLabel section3 = new XRLabel();
            section3.Text = "HARDWARE DIAGNOSTIC RESULTS";
            section3.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            section3.ForeColor = Color.FromArgb(83, 66, 125);
            section3.SizeF = new SizeF(727f, 30f);
            section3.LocationF = new PointF(0f, currentY);
            this.Detail.Controls.Add(section3);
            currentY += 35f;

            XRTable tableTests = new XRTable();
            tableTests.SizeF = new SizeF(727f, 0f);
            tableTests.LocationF = new PointF(0f, currentY);
            tableTests.Borders = BorderSide.All;
            tableTests.BorderColor = Color.FromArgb(226, 232, 240);

            // Header to results table
            XRTableRow headerRow = new XRTableRow();
            headerRow.HeightF = 30f;
            headerRow.BackColor = Color.FromArgb(248, 250, 252);
            headerRow.Cells.AddRange(new XRTableCell[] {
                new XRTableCell { Text = "COMPONENT / TEST", Font = new Font("Segoe UI", 10, FontStyle.Bold), Padding = new PaddingInfo(10,0,0,0) },
                new XRTableCell { Text = "VERDICT", Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlignment = TextAlignment.MiddleCenter },
                new XRTableCell { Text = "COMPONENT / TEST", Font = new Font("Segoe UI", 10, FontStyle.Bold), Padding = new PaddingInfo(10,0,0,0) },
                new XRTableCell { Text = "VERDICT", Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlignment = TextAlignment.MiddleCenter }
            });
            tableTests.Rows.Add(headerRow);

            // Combine all tests
            var allTests = new List<KeyValuePair<string, string>>();
            foreach (var t in device.KernelTests) allTests.Add(t);
            foreach (var t in device.AppTests) allTests.Add(t);

            for (int i = 0; i < allTests.Count; i += 2)
            {
                XRTableRow row = new XRTableRow();
                row.HeightF = 25f;

                var test1 = allTests[i];
                row.Cells.Add(new XRTableCell { Text = test1.Key, Padding = new PaddingInfo(10,0,0,0), Font = new Font("Segoe UI", 9) });
                row.Cells.Add(CreateStatusCell(test1.Value));

                if (i + 1 < allTests.Count)
                {
                    var test2 = allTests[i + 1];
                    row.Cells.Add(new XRTableCell { Text = test2.Key, Padding = new PaddingInfo(10,0,0,0), Font = new Font("Segoe UI", 9) });
                    row.Cells.Add(CreateStatusCell(test2.Value));
                }
                else
                {
                    row.Cells.Add(new XRTableCell());
                    row.Cells.Add(new XRTableCell());
                }
                tableTests.Rows.Add(row);
            }

            this.Detail.Controls.Add(tableTests);
            currentY += tableTests.HeightF + 30f;

            // 4. Overall Pass Badge
            bool allPassed = true;
            foreach (var test in allTests)
            {
                if (test.Value.Equals("Fail", StringComparison.OrdinalIgnoreCase) || test.Value == "1" || test.Value.Equals("No", StringComparison.OrdinalIgnoreCase))
                {
                    allPassed = false;
                    break;
                }
            }

            XRLabel badge = new XRLabel();
            badge.Text = allPassed ? "VERIFIED AUTHENTIC" : "DIAGNOSTIC ALERT";
            badge.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            badge.ForeColor = Color.White;
            badge.BackColor = allPassed ? Color.FromArgb(16, 185, 129) : Color.FromArgb(239, 68, 68);
            badge.SizeF = new SizeF(300f, 40f);
            badge.LocationF = new PointF(213f, currentY);
            badge.TextAlignment = TextAlignment.MiddleCenter;
            this.Detail.Controls.Add(badge);

            // Update height
            this.Detail.HeightF = currentY + 100f;
        }

        private void AddInfoRow(XRTable table, string key1, string val1, string key2, string val2)
        {
            XRTableRow row = new XRTableRow();
            row.HeightF = 25f;

            XRTableCell k1 = new XRTableCell { Text = key1 + ":", Font = new Font("Segoe UI", 9, FontStyle.Bold), WidthF = 120f, Padding = new PaddingInfo(5,0,0,0) };
            XRTableCell v1 = new XRTableCell { Text = val1, Font = new Font("Segoe UI", 9), WidthF = 240f };

            XRTableCell k2 = new XRTableCell { Text = key2 + ":", Font = new Font("Segoe UI", 9, FontStyle.Bold), WidthF = 120f, Padding = new PaddingInfo(5,0,0,0) };
            XRTableCell v2 = new XRTableCell { Text = val2, Font = new Font("Segoe UI", 9), WidthF = 240f };

            row.Cells.AddRange(new XRTableCell[] { k1, v1, k2, v2 });
            table.Rows.Add(row);
        }

        private XRTableCell CreateStatusCell(string val)
        {
            bool passed = val.Equals("Pass", StringComparison.OrdinalIgnoreCase) || 
                          val.Equals("0") || 
                          val.Equals("Original", StringComparison.OrdinalIgnoreCase) ||
                          val.Equals("Yes", StringComparison.OrdinalIgnoreCase);

            var cell = new XRTableCell
            {
                Text = passed ? "PASSED" : "FAILED",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = passed ? Color.FromArgb(16, 185, 129) : Color.FromArgb(239, 68, 68),
                TextAlignment = TextAlignment.MiddleCenter,
                WidthF = 80f
            };
            return cell;
        }
    }
}
