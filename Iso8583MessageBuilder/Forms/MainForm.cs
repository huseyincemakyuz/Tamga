using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Iso8583MessageBuilder.Controls;
using Iso8583MessageBuilder.Models;
using Iso8583MessageBuilder.Templates;

namespace Iso8583MessageBuilder.Forms
{
    public class MainForm : Form
    {
        #region Variables

        #region Build Tab kontrolleri        
        private ComboBox cmbMessageType;
        private Panel pnlFields;
        private Button btnGenerate;
        private Button btnClear;
        private Button btnAddField;
        private TextBox txtHexOutput;
        private RichTextBox rtbDecodedView;
        private Button btnCopyHex;
        private Button btnSaveToFile;
        private Label lblHexLength;
        #endregion

        #region TabControl
        private TabControl tabControl;
        private TabPage tabBuild;
        private TabPage tabParse;
        #endregion

        #region Parse Tab kontrolleri
        private TextBox txtHexInput;
        private Button btnParse;
        private RichTextBox rtbParseResult;
        #endregion

        private List<FieldControl> fieldControls = new List<FieldControl>();
        private Models.Iso8583MessageBuilder messageBuilder = new Models.Iso8583MessageBuilder();
        #endregion 

        public MainForm()
        {
            InitializeComponent();
            LoadMessageTemplates();
        }

        #region FrontEnd
        private void InitializeComponent()
        {
            this.Text = "ISO 8583 Message Builder";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // TabControl oluştur
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };

            // Tab 1: Build Message
            tabBuild = new TabPage("Build Message");
            InitializeBuildTab();

            // Tab 2: Parse Message
            tabParse = new TabPage("Parse Message");
            InitializeParseTab();

            tabControl.TabPages.Add(tabBuild);
            tabControl.TabPages.Add(tabParse);

            this.Controls.Add(tabControl);
        }

        private void InitializeBuildTab()
        {
            // Top Panel - Message Type Selection
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10)
            };

            var lblMsgType = new Label
            {
                Location = new Point(20, 20),
                Text = "Message Type:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };

            cmbMessageType = new ComboBox
            {
                Location = new Point(140, 18),
                Width = 450,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbMessageType.SelectedIndexChanged += CmbMessageType_SelectedIndexChanged;

            topPanel.Controls.AddRange(new Control[] { lblMsgType, cmbMessageType });

            // Middle Panel - Fields
            var middlePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var lblFields = new Label
            {
                Text = "Message Fields:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(0, 5, 0, 0)
            };

            pnlFields = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45
            };

            btnAddField = new Button
            {
                Location = new Point(10, 8),
                Text = "➕ Add Field",
                Width = 120,
                Height = 32,
                Font = new Font("Segoe UI", 9)
            };
            btnAddField.Click += BtnAddField_Click;

            btnGenerate = new Button
            {
                Location = new Point(140, 8),
                Text = "🔨 Generate Message",
                Width = 160,
                Height = 32,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Click += BtnGenerate_Click;

            btnClear = new Button
            {
                Location = new Point(310, 8),
                Text = "🗑️ Clear",
                Width = 100,
                Height = 32,
                Font = new Font("Segoe UI", 9)
            };
            btnClear.Click += BtnClear_Click;

            buttonPanel.Controls.AddRange(new Control[] { btnAddField, btnGenerate, btnClear });

            middlePanel.Controls.AddRange(new Control[] { pnlFields, buttonPanel, lblFields });

            // Bottom Panel - Output
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 350,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            var lblOutput = new Label
            {
                Text = "Generated Message:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25
            };

            var outputButtonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35
            };

            btnCopyHex = new Button
            {
                Location = new Point(5, 5),
                Text = "📋 Copy Hex",
                Width = 110,
                Height = 25,
                Font = new Font("Segoe UI", 8)
            };
            btnCopyHex.Click += (s, ev) =>
            {
                if (!string.IsNullOrEmpty(txtHexOutput.Text))
                {
                    Clipboard.SetText(txtHexOutput.Text.Replace(Environment.NewLine, ""));
                    MessageBox.Show("Hex copied to clipboard!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            btnSaveToFile = new Button
            {
                Location = new Point(120, 5),
                Text = "💾 Save to File",
                Width = 110,
                Height = 25,
                Font = new Font("Segoe UI", 8)
            };
            btnSaveToFile.Click += BtnSaveToFile_Click;

            lblHexLength = new Label
            {
                Location = new Point(240, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray
            };

            outputButtonPanel.Controls.AddRange(new Control[] { btnCopyHex, btnSaveToFile, lblHexLength });

            txtHexOutput = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 80,
                Multiline = true,
                Font = new Font("Consolas", 9),
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            var lblDecoded = new Label
            {
                Text = "Decoded View:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25,
                Padding = new Padding(0, 5, 0, 0)
            };

            rtbDecodedView = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                BackColor = Color.FromArgb(255, 255, 255),
                BorderStyle = BorderStyle.FixedSingle
            };

            bottomPanel.Controls.Add(rtbDecodedView);
            bottomPanel.Controls.Add(lblDecoded);
            bottomPanel.Controls.Add(txtHexOutput);
            bottomPanel.Controls.Add(outputButtonPanel);
            bottomPanel.Controls.Add(lblOutput);

            // Splitter
            var splitter = new Splitter
            {
                Dock = DockStyle.Bottom,
                Height = 5,
                BackColor = Color.Gray
            };

            // Add all to Build tab
            tabBuild.Controls.Add(middlePanel);
            tabBuild.Controls.Add(splitter);
            tabBuild.Controls.Add(bottomPanel);
            tabBuild.Controls.Add(topPanel);
        }

        private void InitializeParseTab()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Title
            var lblTitle = new Label
            {
                Text = "Paste Hex Message to Parse:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30
            };

            // Hex Input
            txtHexInput = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 120,
                Multiline = true,
                Font = new Font("Consolas", 10),
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Button Panel
            var btnPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(0, 10, 0, 0)
            };

            btnParse = new Button
            {
                Location = new Point(0, 10),
                Text = "🔍 Parse & Decode Message",
                Width = 200,
                Height = 35,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnParse.FlatAppearance.BorderSize = 0;
            btnParse.Click += BtnParse_Click;

            var btnClearParse = new Button
            {
                Location = new Point(210, 10),
                Text = "🗑️ Clear",
                Width = 100,
                Height = 35,
                Font = new Font("Segoe UI", 9)
            };
            btnClearParse.Click += (s, ev) =>
            {
                txtHexInput.Clear();
                rtbParseResult.Clear();
            };

            btnPanel.Controls.AddRange(new Control[] { btnParse, btnClearParse });

            // Result Label
            var lblResult = new Label
            {
                Text = "Parsed Result:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(0, 10, 0, 0)
            };

            // Result Display
            rtbParseResult = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            mainPanel.Controls.Add(rtbParseResult);
            mainPanel.Controls.Add(lblResult);
            mainPanel.Controls.Add(btnPanel);
            mainPanel.Controls.Add(txtHexInput);
            mainPanel.Controls.Add(lblTitle);

            tabParse.Controls.Add(mainPanel);
        }
       
        private void LoadMessageTemplates()
        {
            foreach (var template in MessageTemplates.Templates)
            {
                cmbMessageType.Items.Add(new ComboBoxItem
                {
                    Text = $"{template.MTI} - {template.Name}",
                    Value = template
                });
            }

            if (cmbMessageType.Items.Count > 0)
                cmbMessageType.SelectedIndex = 0;
        }
        
        private void CmbMessageType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = (ComboBoxItem)cmbMessageType.SelectedItem;
            var template = (MessageTemplate)selected.Value;

            LoadFieldsForTemplate(template);
        }
        #endregion

        #region BackEnd
        private void LoadFieldsForTemplate(MessageTemplate template)
        {
            pnlFields.Controls.Clear();
            fieldControls.Clear();

            int yPosition = 10;

            // Required fields
            foreach (var fieldNum in template.RequiredFields)
            {
                if (Iso8583Fields.Fields.ContainsKey(fieldNum))
                {
                    var fieldControl = new FieldControl(Iso8583Fields.Fields[fieldNum], isRequired: true);
                    fieldControl.Location = new Point(10, yPosition);
                    fieldControl.ValueChanged += (s, ev) => UpdatePreview();

                    pnlFields.Controls.Add(fieldControl);
                    fieldControls.Add(fieldControl);

                    yPosition += fieldControl.Height + 5;
                }
            }

            // Optional fields
            foreach (var fieldNum in template.OptionalFields)
            {
                if (Iso8583Fields.Fields.ContainsKey(fieldNum))
                {
                    var fieldControl = new FieldControl(Iso8583Fields.Fields[fieldNum], isRequired: false);
                    fieldControl.Location = new Point(10, yPosition);
                    fieldControl.ValueChanged += (s, ev) => UpdatePreview();

                    pnlFields.Controls.Add(fieldControl);
                    fieldControls.Add(fieldControl);

                    yPosition += fieldControl.Height + 5;
                }
            }
        }

        private void BtnAddField_Click(object sender, EventArgs e)
        {
            using (var dlg = new AddFieldDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK && dlg.SelectedFieldNumber.HasValue)
                {
                    var fieldNum = dlg.SelectedFieldNumber.Value;

                    if (fieldControls.Any(fc => fc.FieldNumber == fieldNum))
                    {
                        MessageBox.Show("Bu alan zaten eklendi!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (Iso8583Fields.Fields.ContainsKey(fieldNum))
                    {
                        var fieldControl = new FieldControl(Iso8583Fields.Fields[fieldNum], isRequired: false);

                        int yPos = fieldControls.Count > 0
                            ? fieldControls.Last().Location.Y + fieldControls.Last().Height + 5
                            : 10;

                        fieldControl.Location = new Point(10, yPos);
                        fieldControl.ValueChanged += (s, ev) => UpdatePreview();

                        pnlFields.Controls.Add(fieldControl);
                        fieldControls.Add(fieldControl);

                        pnlFields.ScrollControlIntoView(fieldControl);
                    }
                }
            }
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                var selected = (ComboBoxItem)cmbMessageType.SelectedItem;
                var template = (MessageTemplate)selected.Value;

                messageBuilder = new Models.Iso8583MessageBuilder();
                messageBuilder.SetMTI(template.MTI);

                foreach (var fieldControl in fieldControls.Where(fc => fc.IsFieldEnabled))
                {
                    if (!string.IsNullOrWhiteSpace(fieldControl.FieldValue))
                    {
                        messageBuilder.SetField(fieldControl.FieldNumber, fieldControl.FieldValue);
                    }
                }

                var hexMessage = messageBuilder.BuildHexString();

                txtHexOutput.Text = FormatHexString(hexMessage);
                lblHexLength.Text = $"Length: {hexMessage.Length / 2} bytes ({hexMessage.Length} hex chars)";

                ShowDecodedView(template.MTI); // alanları parse ediyoruz.

                MessageBox.Show("Mesaj başarıyla oluşturuldu!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Mesaj oluşturulamadı:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnParse_Click(object sender, EventArgs e)
        {
            try
            {
                rtbParseResult.Clear();

                if (string.IsNullOrWhiteSpace(txtHexInput.Text))
                {
                    MessageBox.Show("Lütfen parse edilecek hex mesajını giriniz!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var parser = new Iso8583MessageParser();
                var parsedMessage = parser.Parse(txtHexInput.Text);

                // Display MTI
                AppendColoredToParseResult($"MTI: {parsedMessage.MTI}\n", Color.DarkBlue, true);
                AppendColoredToParseResult($"Message Type: {GetMessageTypeName(parsedMessage.MTI)}\n\n", Color.Gray, false);

                // Display Bitmaps
                AppendColoredToParseResult("Bitmaps:\n", Color.Black, true);
                AppendColoredToParseResult($"  Primary:   {parsedMessage.PrimaryBitmap}\n", Color.DarkGreen, false);

                if (!string.IsNullOrEmpty(parsedMessage.SecondaryBitmap))
                {
                    AppendColoredToParseResult($"  Secondary: {parsedMessage.SecondaryBitmap}\n", Color.DarkGreen, false);
                }

                AppendColoredToParseResult("\n", Color.Black, false);

                // Display Fields
                AppendColoredToParseResult($"Fields: ({parsedMessage.Fields.Count} fields present)\n", Color.Black, true);
                AppendColoredToParseResult("─────────────────────────────────────────────────────────────\n", Color.Gray, false);

                foreach (var field in parsedMessage.Fields.OrderBy(f => f.Key))
                {
                    var fieldDef = Iso8583Fields.Fields[field.Key];

                    AppendColoredToParseResult($"\nF{field.Key:D3} ", Color.DarkRed, true);
                    AppendColoredToParseResult($"- {fieldDef.Name}\n", Color.Gray, false);
                    AppendColoredToParseResult($"     Type: ", Color.Gray, false);
                    AppendColoredToParseResult($"{fieldDef.Type}, {fieldDef.LengthType}\n", Color.DarkCyan, false);
                    AppendColoredToParseResult($"     Value: ", Color.Gray, false);
                    AppendColoredToParseResult($"{field.Value}\n", Color.DarkBlue, false);
                }

                // Display Errors/Warnings
                if (parsedMessage.Errors.Count > 0)
                {
                    AppendColoredToParseResult("\n\n", Color.Black, false);
                    AppendColoredToParseResult("Warnings/Errors:\n", Color.DarkOrange, true);
                    AppendColoredToParseResult("─────────────────────────────────────────────────────────────\n", Color.Gray, false);

                    foreach (var error in parsedMessage.Errors)
                    {
                        AppendColoredToParseResult($"⚠ {error}\n", Color.Red, false);
                    }
                }
                else
                {
                    AppendColoredToParseResult("\n\n", Color.Black, false);
                    AppendColoredToParseResult("✓ Mesaj başarıyla parse edildi!\n", Color.Green, true);
                }
            }
            catch (Exception ex)
            {
                rtbParseResult.Clear();
                AppendColoredToParseResult("Parse Error:\n", Color.Red, true);
                AppendColoredToParseResult($"{ex.Message}\n\n", Color.Red, false);
                AppendColoredToParseResult("Stack Trace:\n", Color.Gray, false);
                AppendColoredToParseResult(ex.StackTrace, Color.Gray, false);
            }
        }

        private void AppendColoredToParseResult(string text, Color color, bool bold = false)
        {
            rtbParseResult.SelectionStart = rtbParseResult.TextLength;
            rtbParseResult.SelectionLength = 0;
            rtbParseResult.SelectionColor = color;
            rtbParseResult.SelectionFont = new Font(rtbParseResult.Font, bold ? FontStyle.Bold : FontStyle.Regular);
            rtbParseResult.AppendText(text);
            rtbParseResult.SelectionColor = rtbParseResult.ForeColor;
        }

        private string GetMessageTypeName(string mti)
        {
            var template = MessageTemplates.Templates.FirstOrDefault(t => t.MTI == mti);
            return template != null ? template.Name : "Unknown";
        }

        private string FormatHexString(string hex) // bizim kullandığımız sekilde stringi bir satır 32 tane değer olacka şekilde ayarlıyoruz.
        {
            var sb = new StringBuilder();
            for (int i = 0; i < hex.Length; i += 32)
            {
                sb.AppendLine(hex.Substring(i, Math.Min(32, hex.Length - i)));
            }
            return sb.ToString();
        }

        private void ShowDecodedView(string mti)
        {
            rtbDecodedView.Clear();

            AppendColored($"MTI: {mti}\n", Color.DarkBlue, true);
            AppendColored($"Bitmap: (Auto-generated based on fields)\n\n", Color.DarkGreen, false);

            AppendColored("Fields:\n", Color.Black, true);
            AppendColored("─────────────────────────────────────────────────────\n", Color.Gray, false);

            foreach (var fieldControl in fieldControls.Where(fc => fc.IsFieldEnabled && !string.IsNullOrWhiteSpace(fc.FieldValue)))
            {
                AppendColored($"F{fieldControl.FieldNumber:D3} ", Color.DarkRed, true);
                AppendColored($"({fieldControl.Definition.Name})\n", Color.Gray, false);
                AppendColored($"     Value: ", Color.Black, false);
                AppendColored($"{fieldControl.FieldValue}\n", Color.DarkBlue, false);
                AppendColored($"     Type: {fieldControl.Definition.Type}, {fieldControl.Definition.LengthType}\n\n", Color.Gray, false);
            }
        }

        private void AppendColored(string text, Color color, bool bold = false)
        {
            rtbDecodedView.SelectionStart = rtbDecodedView.TextLength;
            rtbDecodedView.SelectionLength = 0;
            rtbDecodedView.SelectionColor = color;
            rtbDecodedView.SelectionFont = new Font(rtbDecodedView.Font, bold ? FontStyle.Bold : FontStyle.Regular);
            rtbDecodedView.AppendText(text);
            rtbDecodedView.SelectionColor = rtbDecodedView.ForeColor;
        }

        private void UpdatePreview()
        {
            // Real-time preview - opsiyonel
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            foreach (var fieldControl in fieldControls)
            {
                fieldControl.SetValue(string.Empty);
            }
            txtHexOutput.Clear();
            rtbDecodedView.Clear();
            lblHexLength.Text = "";
        }

        private void BtnSaveToFile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtHexOutput.Text))
            {
                MessageBox.Show("Kaydedilecek mesaj yok. Lütfen mesaj giriniz!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text Files (*.txt)|*.txt|Hex Files (*.hex)|*.hex|All Files (*.*)|*.*";
                sfd.DefaultExt = "txt";
                sfd.FileName = $"ISO8583_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(sfd.FileName, txtHexOutput.Text);
                    MessageBox.Show("Mesaj başarıyla kaydedildi!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
    #endregion

    public class ComboBoxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }
        public override string ToString() => Text;
    }
}