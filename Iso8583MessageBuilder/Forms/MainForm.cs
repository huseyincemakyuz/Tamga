using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tamga.Controls;
using Tamga.Models;
using Tamga.Templates;

namespace Tamga.Forms
{
    public class MainForm : Form
    {
        #region Fiedls & Properties

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
        private TabPage tabHistory;
        #endregion

        #region Parse Tab kontrolleri
        private TextBox txtHexInput;
        private Button btnParse;
        private RichTextBox rtbParseResult;
        #endregion

        #region History Tab Kontrolleri
        private DataGridView dgvMessages;
        private TextBox txtSearch;
        private RichTextBox rtbMessageDetail;
        private Button btnLoadToBuild;
        private Button btnLoadToParse;
        private Button btnDeleteMessage;
        private Button btnExportMessage;
        #endregion

        #region Storage
        private MessageStorageManager storageManager;
        private List<SavedMessage> currentMessages;
        #endregion

        #region Status Bar
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblVersion;
        private ToolStripStatusLabel lblCredit;
        private ToolStripStatusLabel lblStatus;
        #endregion

        private List<FieldControl> fieldControls = new List<FieldControl>();
        private Models.Tamga messageBuilder = new Models.Tamga();
        #endregion 

        #region Constructor & Initialization
        public MainForm()
        {
            storageManager = new MessageStorageManager(); 
            currentMessages = new List<SavedMessage>();

            InitializeComponent();
            LoadMessageTemplates();
        }
        
        private void InitializeComponent()
        {
            this.Text = "TAMGA - ISO 8583 Message Builder";
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

            // Tab 3: Message History
            tabHistory = new TabPage("Message History");
            InitializeHistoryTab();

            // Status Bar
            InitializeStatusBar();

            tabControl.TabPages.Add(tabBuild);
            tabControl.TabPages.Add(tabParse);
            tabControl.TabPages.Add(tabHistory);

            this.Controls.Add(tabControl);
            this.Controls.Add(statusStrip);
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

            // YENİ: Save Message butonu
            var btnSaveMessage = new Button
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

            // YENİ: Save Parsed Message butonu
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

            btnPanel.Controls.AddRange(new Control[] { btnParse, btnClearParse, btnSaveParsed });

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

        private void InitializeHistoryTab()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // Top Panel - Search
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(5)
            };

            var lblSearch = new Label
            {
                Location = new Point(5, 12),
                Text = "🔍 Search:",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            txtSearch = new TextBox
            {
                Location = new Point(85, 10),
                Width = 300,
                Font = new Font("Segoe UI", 10)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            var btnRefresh = new Button
            {
                Location = new Point(395, 8),
                Text = "🔄 Refresh",
                Width = 100,
                Height = 30,
                Font = new Font("Segoe UI", 9)
            };
            btnRefresh.Click += (s, e) => LoadSavedMessages();

            topPanel.Controls.AddRange(new Control[] { lblSearch, txtSearch, btnRefresh });

            // Middle Panel - DataGridView
            var middlePanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            dgvMessages = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                Font = new Font("Segoe UI", 9),
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(245, 245, 245)
                }
            };

            // Columns
            dgvMessages.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                DataPropertyName = "Id",
                Visible = false
            });

            dgvMessages.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Message Name",
                DataPropertyName = "Name",
                Width = 250,
                DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 9, FontStyle.Bold) }
            });

            dgvMessages.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MTI",
                HeaderText = "MTI",
                DataPropertyName = "MTI",
                Width = 60
            });

            dgvMessages.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Timestamp",
                HeaderText = "Date/Time",
                DataPropertyName = "Timestamp",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm:ss" }
            });

            dgvMessages.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Tags",
                HeaderText = "Tags",
                DataPropertyName = "Tags",
                Width = 200
            });

            dgvMessages.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FieldCount",
                HeaderText = "Fields",
                DataPropertyName = "FieldCount",
                Width = 60
            });

            dgvMessages.SelectionChanged += DgvMessages_SelectionChanged;
            dgvMessages.CellDoubleClick += DgvMessages_CellDoubleClick;

            middlePanel.Controls.Add(dgvMessages);

            // Bottom Panel - Details & Actions
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 300,
                Padding = new Padding(5),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            var lblDetail = new Label
            {
                Text = "Message Details:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25
            };

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40
            };

            btnLoadToBuild = new Button
            {
                Location = new Point(5, 5),
                Text = "📤 Load to Build",
                Width = 130,
                Height = 30,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLoadToBuild.FlatAppearance.BorderSize = 0;
            btnLoadToBuild.Click += BtnLoadToBuild_Click;

            btnLoadToParse = new Button
            {
                Location = new Point(145, 5),
                Text = "🔍 Load to Parse",
                Width = 130,
                Height = 30,
                Font = new Font("Segoe UI", 9)
            };
            btnLoadToParse.Click += BtnLoadToParse_Click;

            btnDeleteMessage = new Button
            {
                Location = new Point(285, 5),
                Text = "🗑️ Delete",
                Width = 100,
                Height = 30,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDeleteMessage.FlatAppearance.BorderSize = 0;
            btnDeleteMessage.Click += BtnDeleteMessage_Click;

            btnExportMessage = new Button
            {
                Location = new Point(395, 5),
                Text = "📋 Copy Hex",
                Width = 110,
                Height = 30,
                Font = new Font("Segoe UI", 9)
            };
            btnExportMessage.Click += BtnExportMessage_Click;

            buttonPanel.Controls.AddRange(new Control[]
            {
        btnLoadToBuild, btnLoadToParse, btnDeleteMessage, btnExportMessage
            });

            rtbMessageDetail = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            bottomPanel.Controls.Add(rtbMessageDetail);
            bottomPanel.Controls.Add(buttonPanel);
            bottomPanel.Controls.Add(lblDetail);

            // Splitter
            var splitter = new Splitter
            {
                Dock = DockStyle.Bottom,
                Height = 5,
                BackColor = Color.Gray
            };

            mainPanel.Controls.Add(middlePanel);
            mainPanel.Controls.Add(splitter);
            mainPanel.Controls.Add(bottomPanel);
            mainPanel.Controls.Add(topPanel);

            tabHistory.Controls.Add(mainPanel);

            // Load messages when tab is selected
            tabControl.SelectedIndexChanged += (s, e) =>
            {
                if (tabControl.SelectedTab == tabHistory)
                {
                    LoadSavedMessages();
                }
            };
        }

        private void InitializeStatusBar()
        {
            statusStrip = new StatusStrip
            {
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(240, 240, 240),
                Font = new Font("Segoe UI", 8),
                Padding = new Padding(5, 0, 5, 0)
            };

            // Version
            lblVersion = new ToolStripStatusLabel
            {
                Text = "TAMGA v1.0.0",
                ForeColor = Color.FromArgb(100, 100, 100),
                Font = new Font("Segoe UI", 8, FontStyle.Bold)
            };

            // Separator 1
            var lblSeparator1 = new ToolStripStatusLabel
            {
                Text = "|",
                ForeColor = Color.LightGray,
                Margin = new Padding(5, 0, 5, 0)
            };

            // Credit
            lblCredit = new ToolStripStatusLabel
            {
                Text = "Created by Hüseyin Cem Akyüz",
                ForeColor = Color.FromArgb(100, 100, 100)
            };

            // Separator 2
            var lblSeparator2 = new ToolStripStatusLabel
            {
                Text = "|",
                ForeColor = Color.LightGray,
                Margin = new Padding(5, 0, 5, 0)
            };

            // Year
            var lblYear = new ToolStripStatusLabel
            {
                Text = $"© {DateTime.Now.Year}",
                ForeColor = Color.FromArgb(100, 100, 100)
            };

            // Spring (boşluk - sağa yaslamak için)
            var spring = new ToolStripStatusLabel
            {
                Spring = true,
                TextAlign = ContentAlignment.MiddleRight
            };

            // Status (sağda)
            lblStatus = new ToolStripStatusLabel
            {
                Text = "Ready",
                ForeColor = Color.Green,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Image = null
            };

            // Tümünü ekle
            statusStrip.Items.AddRange(new ToolStripItem[]
            {
                lblVersion,
                lblSeparator1,
                lblCredit,
                lblSeparator2,
                lblYear,
                spring,
                lblStatus
            });
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

        #endregion
       
        #region Build Tab - Field Management
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

        private void UpdatePreview()
        {
            // Real-time preview - opsiyonel
        }
        #endregion

        #region Build Tab - Event Handlers

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

                messageBuilder = new Models.Tamga();
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

                MessageBox.Show("Message created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error, message could not be created:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void BtnSaveToFile_Click(object sender, EventArgs e) // Hex string mesajı direkt bir klaösre kaydediyorum.
        {
            if (string.IsNullOrEmpty(txtHexOutput.Text))
            {
                MessageBox.Show("There are no messages to save. Please enter a message!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void BtnSaveMessage_Click(object sender, EventArgs e)
        {
            try
            {
                // Hex mesaj var mı kontrol et
                if (string.IsNullOrWhiteSpace(txtHexOutput.Text))
                {
                    MessageBox.Show("Please generate a message first!", "No Message",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Default isim oluştur
                var selected = (ComboBoxItem)cmbMessageType.SelectedItem;
                var template = (MessageTemplate)selected.Value;
                string defaultName = $"{template.Name} - {DateTime.Now:yyyyMMdd_HHmmss}";

                // Save dialog aç
                using (var dialog = new SaveMessageDialog(defaultName))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // Field'ları topla
                        var fields = new Dictionary<int, string>();
                        foreach (var fieldControl in fieldControls.Where(fc => fc.IsFieldEnabled))
                        {
                            if (!string.IsNullOrWhiteSpace(fieldControl.FieldValue))
                            {
                                fields[fieldControl.FieldNumber] = fieldControl.FieldValue;
                            }
                        }

                        // SavedMessage oluştur
                        var savedMessage = new SavedMessage
                        {
                            Name = dialog.MessageName,
                            MTI = template.MTI,
                            HexMessage = txtHexOutput.Text.Replace(Environment.NewLine, "").Replace(" ", ""),
                            Fields = fields,
                            Tags = dialog.Tags?.ToList() ?? new List<string>(),
                            Notes = dialog.Notes
                        };

                        // Kaydet
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
        } // json formatında mesajları kaydediyorum

        #endregion

        #region Build Tab - Display Methods

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

        #endregion

        #region Parse Tab - Event Handlers
        private void BtnParse_Click(object sender, EventArgs e)
        {
            try
            {
                rtbParseResult.Clear();

                if (string.IsNullOrWhiteSpace(txtHexInput.Text))
                {
                    MessageBox.Show("Please enter the hex message to be parsed!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    AppendColoredToParseResult("✓ The message has been successfully parsed!\n", Color.Green, true);
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

        private void BtnSaveParsedMessage_Click(object sender, EventArgs e)
        {
            try
            {
                // Parse edilmiş mesaj var mı kontrol et
                if (string.IsNullOrWhiteSpace(txtHexInput.Text))
                {
                    MessageBox.Show("Please parse a message first!", "No Message",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Mesajı parse et
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

                // Default isim oluştur
                string defaultName = $"Parsed {parsedMessage.MTI} - {DateTime.Now:yyyyMMdd_HHmmss}";

                // Save dialog aç
                using (var dialog = new SaveMessageDialog(defaultName))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // SavedMessage oluştur
                        var savedMessage = new SavedMessage
                        {
                            Name = dialog.MessageName,
                            MTI = parsedMessage.MTI,
                            HexMessage = txtHexInput.Text.Replace(Environment.NewLine, "").Replace(" ", ""),
                            Fields = parsedMessage.Fields,
                            Tags = dialog.Tags?.ToList() ?? new List<string>(),
                            Notes = dialog.Notes
                        };

                        // Kaydet
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

        #region Parse Tab - Display Methods

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

        #endregion

        #region History Tab - Data Management
        private void LoadSavedMessages()
        {
            try
            {
                currentMessages = storageManager.LoadMessages();

                // DataGridView'e bind et
                var displayList = currentMessages.Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.MTI,
                    m.Timestamp,
                    Tags = m.Tags != null && m.Tags.Count > 0 ? string.Join(", ", m.Tags) : "",
                    FieldCount = m.Fields?.Count ?? 0
                }).ToList();

                dgvMessages.DataSource = displayList;

                // İlk satırı seç
                if (dgvMessages.Rows.Count > 0)
                {
                    dgvMessages.Rows[0].Selected = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading messages: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                LoadSavedMessages();
                return;
            }

            try
            {
                var filtered = storageManager.SearchMessages(txtSearch.Text);

                var displayList = filtered.Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.MTI,
                    m.Timestamp,
                    Tags = m.Tags != null && m.Tags.Count > 0 ? string.Join(", ", m.Tags) : "",
                    FieldCount = m.Fields?.Count ?? 0
                }).ToList();

                dgvMessages.DataSource = displayList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region History Tab - Event Handlers

        private void DgvMessages_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvMessages.SelectedRows.Count == 0)
            {
                rtbMessageDetail.Clear();
                return;
            }

            try
            {
                var row = dgvMessages.SelectedRows[0];
                var messageId = row.Cells["Id"].Value?.ToString();
                var message = currentMessages.FirstOrDefault(m => m.Id == messageId);

                if (message == null) return;

                // Display message details
                rtbMessageDetail.Clear();

                AppendColoredHistory($"📋 {message.Name}\n\n", Color.DarkBlue, true);
                AppendColoredHistory($"MTI: {message.MTI}\n", Color.Black, false);
                AppendColoredHistory($"Saved: {message.Timestamp:yyyy-MM-dd HH:mm:ss}\n", Color.Gray, false);

                if (message.Tags != null && message.Tags.Count > 0)
                {
                    AppendColoredHistory($"Tags: {string.Join(", ", message.Tags)}\n", Color.DarkCyan, false);
                }

                if (!string.IsNullOrEmpty(message.Notes))
                {
                    AppendColoredHistory($"Notes: {message.Notes}\n", Color.Gray, false);
                }

                AppendColoredHistory("\nHex Message:\n", Color.Black, true);
                AppendColoredHistory($"{FormatHexString(message.HexMessage)}\n", Color.DarkGreen, false);

                if (message.Fields != null && message.Fields.Count > 0)
                {
                    AppendColoredHistory($"\nFields ({message.Fields.Count}):\n", Color.Black, true);
                    AppendColoredHistory("─────────────────────────────────────────────────────\n", Color.Gray, false);

                    foreach (var field in message.Fields.OrderBy(f => f.Key))
                    {
                        var fieldDef = Iso8583Fields.Fields.ContainsKey(field.Key)
                            ? Iso8583Fields.Fields[field.Key]
                            : null;

                        AppendColoredHistory($"F{field.Key:D3} ", Color.DarkRed, true);

                        if (fieldDef != null)
                        {
                            AppendColoredHistory($"- {fieldDef.Name}\n", Color.Gray, false);
                            AppendColoredHistory($"     Type: {fieldDef.Type}, {fieldDef.LengthType}\n", Color.DarkCyan, false);
                        }
                        else
                        {
                            AppendColoredHistory($"\n", Color.Gray, false);
                        }

                        AppendColoredHistory($"     Value: ", Color.Gray, false);
                        AppendColoredHistory($"{field.Value}\n\n", Color.DarkBlue, false);
                    }
                }
            }
            catch (Exception ex)
            {
                rtbMessageDetail.Clear();
                AppendColoredHistory($"Error displaying message: {ex.Message}", Color.Red, false);
            }
        }

        private void DgvMessages_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Çift tıklayınca Build tab'a yükle
            if (e.RowIndex >= 0)
            {
                BtnLoadToBuild_Click(sender, e);
            }
        }

        private void BtnLoadToBuild_Click(object sender, EventArgs e)
        {
            if (dgvMessages.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a message first!", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var row = dgvMessages.SelectedRows[0];
                var messageId = row.Cells["Id"].Value?.ToString();
                var message = currentMessages.FirstOrDefault(m => m.Id == messageId);

                if (message == null) return;

                // Switch to Build tab
                tabControl.SelectedTab = tabBuild;

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
                            // Field mevcut değilse ekle
                            if (Iso8583Fields.Fields.ContainsKey(field.Key))
                            {
                                var newFieldControl = new FieldControl(Iso8583Fields.Fields[field.Key], isRequired: false);

                                int yPos = fieldControls.Count > 0
                                    ? fieldControls.Last().Location.Y + fieldControls.Last().Height + 5
                                    : 10;

                                newFieldControl.Location = new Point(10, yPos);
                                newFieldControl.ValueChanged += (s, ev) => UpdatePreview();
                                newFieldControl.SetValue(field.Value);

                                pnlFields.Controls.Add(newFieldControl);
                                fieldControls.Add(newFieldControl);
                            }
                        }
                    }
                }

                MessageBox.Show($"Message '{message.Name}' loaded to Build tab!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading message: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLoadToParse_Click(object sender, EventArgs e)
        {
            if (dgvMessages.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a message first!", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var row = dgvMessages.SelectedRows[0];
                var messageId = row.Cells["Id"].Value?.ToString();
                var message = currentMessages.FirstOrDefault(m => m.Id == messageId);

                if (message == null) return;

                // Switch to Parse tab
                tabControl.SelectedTab = tabParse;

                // Load hex to input
                txtHexInput.Text = message.HexMessage;

                // Auto-parse
                BtnParse_Click(sender, e);

                MessageBox.Show($"Message '{message.Name}' loaded to Parse tab!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading message: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeleteMessage_Click(object sender, EventArgs e)
        {
            if (dgvMessages.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a message first!", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var row = dgvMessages.SelectedRows[0];
                var messageId = row.Cells["Id"].Value?.ToString();
                var message = currentMessages.FirstOrDefault(m => m.Id == messageId);

                if (message == null) return;

                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{message.Name}'?\n\nThis action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    storageManager.DeleteMessage(message.Id);
                    LoadSavedMessages();
                    MessageBox.Show("Message deleted successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting message: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportMessage_Click(object sender, EventArgs e)
        {
            if (dgvMessages.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a message first!", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var row = dgvMessages.SelectedRows[0];
                var messageId = row.Cells["Id"].Value?.ToString();
                var message = currentMessages.FirstOrDefault(m => m.Id == messageId);

                if (message == null) return;

                Clipboard.SetText(message.HexMessage);
                MessageBox.Show("Hex message copied to clipboard!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region History Tab - Display Methods
        private void AppendColoredHistory(string text, Color color, bool bold = false)
        {
            rtbMessageDetail.SelectionStart = rtbMessageDetail.TextLength;
            rtbMessageDetail.SelectionLength = 0;
            rtbMessageDetail.SelectionColor = color;
            rtbMessageDetail.SelectionFont = new Font(rtbMessageDetail.Font, bold ? FontStyle.Bold : FontStyle.Regular);
            rtbMessageDetail.AppendText(text);
            rtbMessageDetail.SelectionColor = rtbMessageDetail.ForeColor;
        }
        #endregion

        #region Helper Methods
        private string FormatHexString(string hex) // bizim kullandığımız sekilde stringi bir satır 32 tane değer olacka şekilde ayarlıyoruz.
        {
            var sb = new StringBuilder();
            for (int i = 0; i < hex.Length; i += 32)
            {
                sb.AppendLine(hex.Substring(i, Math.Min(32, hex.Length - i)));
            }
            return sb.ToString();
        }

        #endregion

    }

    public class ComboBoxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }
        public override string ToString() => Text;
    }
}