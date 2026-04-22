using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tamga.Controls;
using Tamga.Models;
using Tamga.Templates;

namespace Tamga.Forms.Tabs
{
    public class BuildTabManager
    {
        #region Fields

        private TabPage tabPage;
        private ComboBox cmbMessageType;
        private ComboBox cmbEncoding;
        private Panel pnlFields;
        private Button btnGenerate;
        private Button btnClear;
        private Button btnAddField;
        private Button btnSaveMessage;
        private TextBox txtHexOutput;
        private RichTextBox rtbDecodedView;
        private Label lblHexLength;

        private List<FieldControl> fieldControls = new List<FieldControl>();
        private Iso8583MessageBuilder messageBuilder = new Iso8583MessageBuilder();
        private MessageStorageManager storageManager;

        #endregion

        #region Constructor

        public BuildTabManager(TabPage tab, MessageStorageManager storage)
        {
            tabPage = tab;
            storageManager = storage;
            InitializeUI();
            LoadMessageTemplates();
        }

        #endregion

        #region UI Initialization

        private void InitializeUI()
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

            var lblEncoding = new Label
            {
                Location = new Point(610, 20),
                Text = "Encoding:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };

            cmbEncoding = new ComboBox
            {
                Location = new Point(700, 18),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbEncoding.Items.Add("ASCII");
            cmbEncoding.Items.Add("BCD");
            cmbEncoding.SelectedIndex = 0;

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

            var buttonPanel = CreateButtonPanel();
            middlePanel.Controls.AddRange(new Control[] { pnlFields, buttonPanel, lblFields });

            // Bottom Panel - Output
            var bottomPanel = CreateOutputPanel();

            // Splitter
            var splitter = new Splitter
            {
                Dock = DockStyle.Bottom,
                Height = 5,
                BackColor = Color.Gray
            };

            // Add to tab
            tabPage.Controls.Add(middlePanel);
            tabPage.Controls.Add(splitter);
            tabPage.Controls.Add(bottomPanel);
            tabPage.Controls.Add(topPanel);
        }

        private Panel CreateButtonPanel()
        {
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

            btnSaveMessage = new Button
            {
                Location = new Point(420, 8),
                Text = "💾 Save Message",
                Width = 130,
                Height = 32,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSaveMessage.FlatAppearance.BorderSize = 0;
            btnSaveMessage.Click += BtnSaveMessage_Click;

            buttonPanel.Controls.AddRange(new Control[] { btnAddField, btnGenerate, btnClear, btnSaveMessage });
            return buttonPanel;
        }

        private Panel CreateOutputPanel()
        {
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

            var btnCopyHex = new Button
            {
                Location = new Point(5, 5),
                Text = "📋 Copy Hex",
                Width = 110,
                Height = 25,
                Font = new Font("Segoe UI", 8)
            };
            btnCopyHex.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(txtHexOutput.Text))
                {
                    Clipboard.SetText(txtHexOutput.Text.Replace(System.Environment.NewLine, ""));
                    MessageBox.Show("Hex copied to clipboard!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            var btnSaveToFile = new Button
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
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            bottomPanel.Controls.Add(rtbDecodedView);
            bottomPanel.Controls.Add(lblDecoded);
            bottomPanel.Controls.Add(txtHexOutput);
            bottomPanel.Controls.Add(outputButtonPanel);
            bottomPanel.Controls.Add(lblOutput);

            return bottomPanel;
        }

        #endregion

        #region Template Management
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
                    pnlFields.Controls.Add(fieldControl);
                    fieldControls.Add(fieldControl);
                    yPosition += fieldControl.Height + 5;
                }
            }
        }

        #endregion

        #region Event Handlers
        private void CmbMessageType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = (ComboBoxItem)cmbMessageType.SelectedItem;
            var template = (MessageTemplate)selected.Value;
            LoadFieldsForTemplate(template);
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
                        MessageBox.Show("This field already added!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (Iso8583Fields.Fields.ContainsKey(fieldNum))
                    {
                        var fieldControl = new FieldControl(Iso8583Fields.Fields[fieldNum], isRequired: false);
                        int yPos = fieldControls.Count > 0
                            ? fieldControls.Last().Location.Y + fieldControls.Last().Height + 5
                            : 10;

                        fieldControl.Location = new Point(10, yPos);
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

                var selectedEncoding = cmbEncoding.SelectedIndex == 1 ? EncodingFormat.BCD : EncodingFormat.ASCII;

                messageBuilder = new Iso8583MessageBuilder();
                messageBuilder.SetMTI(template.MTI);
                messageBuilder.SetEncoding(selectedEncoding);

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
                MessageBox.Show("Message created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void BtnSaveMessage_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtHexOutput.Text))
                {
                    MessageBox.Show("Please generate a message first!", "No Message",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var selected = (ComboBoxItem)cmbMessageType.SelectedItem;
                var template = (MessageTemplate)selected.Value;
                string defaultName = $"{template.Name} - {DateTime.Now:yyyyMMdd_HHmmss}";

                using (var dialog = new SaveMessageDialog(defaultName))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        var fields = new Dictionary<int, string>();
                        foreach (var fieldControl in fieldControls.Where(fc => fc.IsFieldEnabled))
                        {
                            if (!string.IsNullOrWhiteSpace(fieldControl.FieldValue))
                            {
                                fields[fieldControl.FieldNumber] = fieldControl.FieldValue;
                            }
                        }

                        var savedMessage = new SavedMessage
                        {
                            Name = dialog.MessageName,
                            MTI = template.MTI,
                            HexMessage = txtHexOutput.Text.Replace(System.Environment.NewLine, "").Replace(" ", ""),
                            Fields = fields,
                            Tags = dialog.Tags?.ToList() ?? new List<string>(),
                            Notes = dialog.Notes
                        };

                        storageManager.AddMessage(savedMessage);
                        MessageBox.Show($"Message '{savedMessage.Name}' saved successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving message: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                    System.IO.File.WriteAllText(sfd.FileName, txtHexOutput.Text);
                    MessageBox.Show("Message saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        #endregion

        #region Display Methods

        private void ShowDecodedView(string mti)
        {
            rtbDecodedView.Clear();
            AppendColored($"Encoding: {encoding}\n", Color.Purple, true);
            AppendColored($"MTI: {mti}\n", Color.DarkBlue, true);
            AppendColored($"Bitmap: (Auto-generated based on fields)\n\n", Color.DarkGreen, false);
            AppendColored("Fields:\n", Color.Black, true);
            AppendColored("─────────────────────────────────────────────────────\n", Color.Gray, false);

            foreach (var fieldControl in fieldControls.Where(fc => fc.IsFieldEnabled && !string.IsNullOrWhiteSpace(fc.FieldValue)))
            {
                AppendColored($"F{fieldControl.FieldNumber:D3} ", Color.DarkRed, true);
                AppendColored($"({fieldControl.Definition.Name})\n", Color.Gray, false);

                string fieldValue = fieldControl.FieldValue;

                if (ContainsNonPrintable(fieldValue))
                {
                    AppendColored($"     ASCII: ", Color.Black, false);
                    AppendColored($"{SanitizeForDisplay(fieldValue)}", Color.DarkBlue, false);
                }
                else
                {
                    AppendColored($"     Value: ", Color.Black, false);
                    AppendColored($"{fieldControl.FieldValue}\n", Color.DarkBlue, false);
                }
                    
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

        private string FormatHexString(string hex)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < hex.Length; i += 32)
            {
                sb.AppendLine(hex.Substring(i, Math.Min(32, hex.Length - i)));
            }
            return sb.ToString();
        }

        private bool ContainsNonPrintable(string text)
        {
            foreach (char c in text)
            {
                if (c < 0x20 || c > 0x7E)
                    return true;
            }

            return false;
        }

        private string SanitizeForDisplay(string text)
        {
            var sb = new System.Text.StringBuilder(text.Length);
            foreach (char c in text)
            {
                sb.Append(c >= 0x20 && c <= 0x7E ? c : '.');
            }
            return sb.ToString();
        }


        #endregion

        #region Public Methods

        public void LoadMessage(SavedMessage message)
        {
            // Find and select matching template
            var template = MessageTemplates.Templates.FirstOrDefault(t => t.MTI == message.MTI);
            if (template != null)
            {
                for (int i = 0; i < cmbMessageType.Items.Count; i++)
                {
                    var item = (ComboBoxItem)cmbMessageType.Items[i];
                    if (((MessageTemplate)item.Value).MTI == message.MTI)
                    {
                        cmbMessageType.SelectedIndex = i;
                        break;
                    }
                }
            }

            // Load field values
            if (message.Fields != null)
            {
                foreach (var field in message.Fields)
                {
                    var fieldControl = fieldControls.FirstOrDefault(fc => fc.FieldNumber == field.Key);
                    if (fieldControl != null)
                    {
                        fieldControl.SetValue(field.Value);
                    }
                    else
                    {
                        if (Iso8583Fields.Fields.ContainsKey(field.Key))
                        {
                            var newFieldControl = new FieldControl(Iso8583Fields.Fields[field.Key], isRequired: false);
                            int yPos = fieldControls.Count > 0
                                ? fieldControls.Last().Location.Y + fieldControls.Last().Height + 5
                                : 10;

                            newFieldControl.Location = new Point(10, yPos);
                            newFieldControl.SetValue(field.Value);
                            pnlFields.Controls.Add(newFieldControl);
                            fieldControls.Add(newFieldControl);
                        }
                    }
                }
            }
        }
       
        public void LoadFromParsedMessage(ParsedMessage parsedMessage)
        {
            // Find template by MTI
            var template = MessageTemplates.Templates.FirstOrDefault(t => t.MTI == parsedMessage.MTI);

            if (template != null)
            {
                // Select template
                for (int i = 0; i < cmbMessageType.Items.Count; i++)
                {
                    var item = (ComboBoxItem)cmbMessageType.Items[i];
                    if (((MessageTemplate)item.Value).MTI == parsedMessage.MTI)
                    {
                        cmbMessageType.SelectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                // Template yoksa ilk template'i seç
                if (cmbMessageType.Items.Count > 0)
                    cmbMessageType.SelectedIndex = 0;
            }

            // Encoding formatini otomatik ayarla
            cmbEncoding.SelectedIndex = parsedMessage.Encoding == EncodingFormat.BCD ? 1 : 0;

            // Load fields from parsed message
            if (parsedMessage.Fields != null)
            {
                foreach (var field in parsedMessage.Fields)
                {
                    var fieldControl = fieldControls.FirstOrDefault(fc => fc.FieldNumber == field.Key);

                    if (fieldControl != null)
                    {
                        // Field zaten var, değeri güncelle
                        fieldControl.SetValue(field.Value);
                        fieldControl.EnableField(true);  // Aktif et
                    }
                    else
                    {
                        // Field yok, yeni ekle
                        if (Iso8583Fields.Fields.ContainsKey(field.Key))
                        {
                            var fieldDef = Iso8583Fields.Fields[field.Key];
                            var newFieldControl = new FieldControl(fieldDef, isRequired: false);

                            int yPos = fieldControls.Count > 0
                                ? fieldControls.Last().Location.Y + fieldControls.Last().Height + 5
                                : 10;

                            newFieldControl.Location = new Point(10, yPos);
                            newFieldControl.SetValue(field.Value);
                            newFieldControl.EnableField(true);

                            pnlFields.Controls.Add(newFieldControl);
                            fieldControls.Add(newFieldControl);
                        }
                    }
                }
            }

            // Scroll to top
            pnlFields.ScrollControlIntoView(fieldControls.FirstOrDefault());
        }

        /// <summary>
        /// Generate edilmiş hex mesajı döndür
        /// </summary>
        public string GetGeneratedHex()
        {
            if (txtHexOutput == null)
                return string.Empty;

            return txtHexOutput.Text.Trim();
        }

        #endregion
    }   
}