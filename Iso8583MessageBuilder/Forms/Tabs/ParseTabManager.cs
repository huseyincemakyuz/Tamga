using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Tamga.Models;
using Tamga.Templates;

namespace Tamga.Forms.Tabs
{
    public class ParseTabManager
    {
        #region Fields

        private TabPage tabPage;
        private TextBox txtHexInput;
        private Button btnParse;
        private Button btnLoadToBuild;  
        private RichTextBox rtbParseResult;
        private MessageStorageManager storageManager;

        private ParsedMessage lastParsedMessage;  
       
        public event EventHandler<ParsedMessage> LoadToBuildRequested;

        #endregion

        #region Constructor

        public ParseTabManager(TabPage tab, MessageStorageManager storage)
        {
            tabPage = tab;
            storageManager = storage;
            InitializeUI();
        }

        #endregion

        #region UI Initialization

        private void InitializeUI()
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
            var btnPanel = CreateButtonPanel();

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

            tabPage.Controls.Add(mainPanel);
        }

        private Panel CreateButtonPanel()
        {
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
            btnClearParse.Click += (s, e) =>
            {
                txtHexInput.Clear();
                rtbParseResult.Clear();
            };

            var btnSaveParsed = new Button
            {
                Location = new Point(320, 10),
                Text = "💾 Save",
                Width = 100,
                Height = 35,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSaveParsed.FlatAppearance.BorderSize = 0;
            btnSaveParsed.Click += BtnSaveParsedMessage_Click;
          
            btnLoadToBuild = new Button
            {
                Location = new Point(430, 10),
                Text = "📤 Load to Build",
                Width = 140,
                Height = 35,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false  // Başlangıçta devre dışı
            };
            btnLoadToBuild.FlatAppearance.BorderSize = 0;
            btnLoadToBuild.Click += BtnLoadToBuild_Click;

            btnPanel.Controls.AddRange(new Control[] { btnParse, btnClearParse, btnSaveParsed, btnLoadToBuild });
            return btnPanel;
        }

        #endregion

        #region Event Handlers

        private void BtnParse_Click(object sender, EventArgs e)
        {
            try
            {
                rtbParseResult.Clear();

                if (string.IsNullOrWhiteSpace(txtHexInput.Text))
                {
                    MessageBox.Show("Please enter the hex message to be parsed!", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var parser = new Iso8583MessageParser();
                var parsedMessage = parser.Parse(txtHexInput.Text);

                DisplayParsedMessage(parsedMessage);

                lastParsedMessage = parser.Parse(txtHexInput.Text); // YENİ
                
                btnLoadToBuild.Enabled = lastParsedMessage != null &&
                                         lastParsedMessage.Errors.Count == 0;

                // Debug için
                Console.WriteLine($"Parse completed. Errors: {lastParsedMessage.Errors.Count}");
                Console.WriteLine($"Load to Build button enabled: {btnLoadToBuild.Enabled}");
            }
            catch (Exception ex)
            {
                rtbParseResult.Clear();
                AppendColored("Parse Error:\n", Color.Red, true);
                AppendColored($"{ex.Message}\n\n", Color.Red, false);
                AppendColored("Stack Trace:\n", Color.Gray, false);
                AppendColored(ex.StackTrace, Color.Gray, false);
            }
        }
       
        private void BtnLoadToBuild_Click(object sender, EventArgs e)
        {
            if (lastParsedMessage == null)
            {
                MessageBox.Show("Please parse a message first!", "No Message",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (lastParsedMessage.Errors.Count > 0)
            {
                var result = MessageBox.Show(
                    "The parsed message has errors. Do you still want to load it to Build tab?",
                    "Parsing Errors",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                    return;
            }

            // Event tetikle - MainForm dinliyor
            LoadToBuildRequested?.Invoke(this, lastParsedMessage);
        }

        private void BtnSaveParsedMessage_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtHexInput.Text))
                {
                    MessageBox.Show("Please parse a message first!", "No Message",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var parser = new Iso8583MessageParser();
                var parsedMessage = parser.Parse(txtHexInput.Text);

                if (parsedMessage.Errors.Count > 0)
                {
                    var result = MessageBox.Show(
                        "The message has parsing errors. Do you still want to save it?",
                        "Parsing Errors",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.No)
                        return;
                }

                string defaultName = $"Parsed {parsedMessage.MTI} - {DateTime.Now:yyyyMMdd_HHmmss}";

                using (var dialog = new SaveMessageDialog(defaultName))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        var savedMessage = new SavedMessage
                        {
                            Name = dialog.MessageName,
                            MTI = parsedMessage.MTI,
                            HexMessage = txtHexInput.Text.Replace(Environment.NewLine, "").Replace(" ", ""),
                            Fields = parsedMessage.Fields,
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

        #endregion

        #region Display Methods

        private void DisplayParsedMessage(ParsedMessage parsedMessage)
        {
            // Display MTI
            AppendColored($"MTI: {parsedMessage.MTI}\n", Color.DarkBlue, true);
            AppendColored($"Message Type: {GetMessageTypeName(parsedMessage.MTI)}\n\n", Color.Gray, false);

            // Display Bitmaps
            AppendColored("Bitmaps:\n", Color.Black, true);
            AppendColored($"  Primary:   {parsedMessage.PrimaryBitmap}\n", Color.DarkGreen, false);

            if (!string.IsNullOrEmpty(parsedMessage.SecondaryBitmap))
            {
                AppendColored($"  Secondary: {parsedMessage.SecondaryBitmap}\n", Color.DarkGreen, false);
            }

            AppendColored("\n", Color.Black, false);

            // Display Fields
            AppendColored($"Fields: ({parsedMessage.Fields.Count} fields present)\n", Color.Black, true);
            AppendColored("─────────────────────────────────────────────────────────────\n", Color.Gray, false);

            foreach (var field in parsedMessage.Fields.OrderBy(f => f.Key))
            {
                if (!Iso8583Fields.Fields.ContainsKey(field.Key))
                    continue;

                var fieldDef = Iso8583Fields.Fields[field.Key];

                AppendColored($"\nF{field.Key:D3} ", Color.DarkRed, true);
                AppendColored($"- {fieldDef.Name}\n", Color.Gray, false);
                AppendColored($"     Type: ", Color.Gray, false);
                AppendColored($"{fieldDef.Type}, {fieldDef.LengthType}\n", Color.DarkCyan, false);
                AppendColored($"     Value: ", Color.Gray, false);
                AppendColored($"{field.Value}\n", Color.DarkBlue, false);
            }

            // Display Errors/Warnings
            if (parsedMessage.Errors.Count > 0)
            {
                AppendColored("\n\n", Color.Black, false);
                AppendColored("Warnings/Errors:\n", Color.DarkOrange, true);
                AppendColored("─────────────────────────────────────────────────────────────\n", Color.Gray, false);

                foreach (var error in parsedMessage.Errors)
                {
                    AppendColored($"⚠ {error}\n", Color.Red, false);
                }
            }
            else
            {
                AppendColored("\n\n", Color.Black, false);
                AppendColored("✓ The message has been successfully parsed!\n", Color.Green, true);
            }
        }

        private void AppendColored(string text, Color color, bool bold = false)
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

        #endregion

        #region Public Methods

        public void LoadMessage(SavedMessage message)
        {
            txtHexInput.Text = message.HexMessage;
            BtnParse_Click(null, null);
        }

        #endregion
    }
}