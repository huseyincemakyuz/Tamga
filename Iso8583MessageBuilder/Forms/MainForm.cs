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

        private List<FieldControl> fieldControls = new List<FieldControl>();
        private Models.Iso8583MessageBuilder messageBuilder = new Models.Iso8583MessageBuilder();

        public MainForm()
        {
            InitializeComponent();
            LoadMessageTemplates();
        }

        private void InitializeComponent()
        {
            this.Text = "ISO 8583 Message Builder";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

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

            // Add all to form
            this.Controls.Add(middlePanel);
            this.Controls.Add(splitter);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(topPanel);
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

        private void LoadFieldsForTemplate(MessageTemplate template)
        {
            pnlFields.Controls.Clear();
            fieldControls.Clear();

            int yPos = 10;

            // Required fields
            foreach (var fieldNum in template.RequiredFields)
            {
                if (Iso8583Fields.Fields.ContainsKey(fieldNum))
                {
                    var fieldControl = new FieldControl(Iso8583Fields.Fields[fieldNum], isRequired: true);
                    fieldControl.Location = new Point(10, yPos);
                    fieldControl.ValueChanged += (s, ev) => UpdatePreview();

                    pnlFields.Controls.Add(fieldControl);
                    fieldControls.Add(fieldControl);

                    yPos += fieldControl.Height + 5;
                }
            }

            // Optional fields
            foreach (var fieldNum in template.OptionalFields)
            {
                if (Iso8583Fields.Fields.ContainsKey(fieldNum))
                {
                    var fieldControl = new FieldControl(Iso8583Fields.Fields[fieldNum], isRequired: false);
                    fieldControl.Location = new Point(10, yPos);
                    fieldControl.ValueChanged += (s, ev) => UpdatePreview();

                    pnlFields.Controls.Add(fieldControl);
                    fieldControls.Add(fieldControl);

                    yPos += fieldControl.Height + 5;
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
                        MessageBox.Show("This field is already added!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                ShowDecodedView(template.MTI);

                MessageBox.Show("Message generated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating message:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FormatHexString(string hex)
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
                MessageBox.Show("No message to save!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show("Message saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }  
}

public class ComboBoxItem
{
    public string Text { get; set; }
    public object Value { get; set; }
    public override string ToString() => Text;
}