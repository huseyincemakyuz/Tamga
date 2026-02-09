using System;
using System.Drawing;
using System.Windows.Forms;
using Tamga.Forms.Dialogs;
using Tamga.Forms.Tabs;
using Tamga.Models;

namespace Tamga.Forms
{
    public class MainForm : Form
    {
        #region Fields

        private TabControl tabControl;
        private TabPage tabBuild;
        private TabPage tabParse;
        private TabPage tabHistory;

        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblVersion;
        private ToolStripStatusLabel lblCredit;
        private ToolStripStatusLabel lblStatus;

        private MessageStorageManager storageManager;

        // Tab Managers
        private BuildTabManager buildTabManager;
        private ParseTabManager parseTabManager;
        private HistoryTabManager historyTabManager;

        // ═══════════════════════════════════════════════
        // YENİ: Toolbar
        // ═══════════════════════════════════════════════
        private ToolStrip toolStrip;
        private ToolStripLabel lblEnvironment;
        private ToolStripComboBox cmbEnvironment;
        private ToolStripButton btnSettings;
        private ToolStripButton btnSend;                

        #endregion

        #region Constructor

        public MainForm()
        {
            storageManager = new MessageStorageManager();
            InitializeComponent();
            InitializeToolbar();
            InitializeTabManagers();            
        }

        #endregion

        #region Initialization

        private void InitializeComponent()
        {
            this.Text = "TAMGA - ISO 8583 Message Builder";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // TabControl
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };

            // Create Tabs
            tabBuild = new TabPage("Build Message");
            tabParse = new TabPage("Parse Message");
            tabHistory = new TabPage("Message History");

            tabControl.TabPages.Add(tabBuild);
            tabControl.TabPages.Add(tabParse);
            tabControl.TabPages.Add(tabHistory);

            // Status Bar
            InitializeStatusBar();

            // Add to form
            this.Controls.Add(tabControl);
            this.Controls.Add(statusStrip);

            // Tab selection event
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
        }

        private void InitializeTabManagers()
        {
            // Build Tab Manager
            buildTabManager = new BuildTabManager(tabBuild, storageManager);

            // Parse Tab Manager
            parseTabManager = new ParseTabManager(tabParse, storageManager);
           
            parseTabManager.LoadToBuildRequested += ParseTabManager_LoadToBuildRequested;

            // History Tab Manager
            historyTabManager = new HistoryTabManager(tabHistory, storageManager);
            historyTabManager.LoadToBuildRequested += HistoryTabManager_LoadToBuildRequested;
            historyTabManager.LoadToParseRequested += HistoryTabManager_LoadToParseRequested;
        }

        // ═══════════════════════════════════════════════
        // YENİ: TOOLBAR
        // ═══════════════════════════════════════════════
        private void InitializeToolbar()
        {
            toolStrip = new ToolStrip
            {
                GripStyle = ToolStripGripStyle.Hidden,
                Padding = new Padding(0,0,20,0),
                Margin = new Padding(0,0,15,0),
                BackColor = Color.FromArgb(240, 240, 240)
            };
           
            // Environment Label
            lblEnvironment = new ToolStripLabel("Environment:") 
            {
                Alignment = ToolStripItemAlignment.Right
            };

            // Environment ComboBox
            cmbEnvironment = new ToolStripComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 800,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 0),
                Alignment = ToolStripItemAlignment.Right
                
            };
            cmbEnvironment.SelectedIndexChanged += CmbEnvironment_SelectedIndexChanged;

            // Settings Button
            btnSettings = new ToolStripButton
            {
                Text = "⚙️",
                ToolTipText = "Environment Settings",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 12),
                Alignment = ToolStripItemAlignment.Right
            };
            btnSettings.Click += BtnSettings_Click;

            // Separator
            var separator = new ToolStripSeparator() 
            {
                Alignment = ToolStripItemAlignment.Right
            };

            // Send Button
            btnSend = new ToolStripButton
            {
                Text = "📤 Send",
                ToolTipText = "Send Message (Ctrl+Enter)",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(34, 139, 34),
                Padding = new Padding(10, 5, 10, 5),
                 Alignment = ToolStripItemAlignment.Right
            };
            btnSend.Click += BtnSend_Click;

            toolStrip.Items.AddRange(new ToolStripItem[]
            {
                btnSend,
                separator,
                btnSettings,
                cmbEnvironment,
                lblEnvironment,                                             
            });

            this.Controls.Add(toolStrip);

            // Load environments
            LoadEnvironments();
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

            lblVersion = new ToolStripStatusLabel
            {
                Text = "TAMGA v1.0.0",
                ForeColor = Color.FromArgb(100, 100, 100),
                Font = new Font("Segoe UI", 8, FontStyle.Bold)
            };

            var lblSeparator1 = new ToolStripStatusLabel
            {
                Text = "|",
                ForeColor = Color.LightGray,
                Margin = new Padding(5, 0, 5, 0)
            };

            lblCredit = new ToolStripStatusLabel
            {
                Text = "Created by Hüseyin Cem Akyüz",
                ForeColor = Color.FromArgb(100, 100, 100)
            };

            var lblSeparator2 = new ToolStripStatusLabel
            {
                Text = "|",
                ForeColor = Color.LightGray,
                Margin = new Padding(5, 0, 5, 0)
            };

            var lblYear = new ToolStripStatusLabel
            {
                Text = $"© {DateTime.Now.Year}",
                ForeColor = Color.FromArgb(100, 100, 100)
            };

            var spring = new ToolStripStatusLabel
            {
                Spring = true,
                TextAlign = ContentAlignment.MiddleRight
            };

            lblStatus = new ToolStripStatusLabel
            {
                Text = "Ready",
                ForeColor = Color.Green,
                Font = new Font("Segoe UI", 8, FontStyle.Bold)
            };

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

        #endregion

        #region Event Handlers

        ///////////yeni kodlar///////
        private void LoadEnvironments()
        {
            cmbEnvironment.Items.Clear();

            var environments = EnvironmentSettings.Instance.GetEnabled();
            foreach (var env in environments)
            {
                cmbEnvironment.Items.Add(env);
            }

            // Default'u seç
            var defaultEnv = EnvironmentSettings.Instance.GetDefault();
            if (defaultEnv != null)
            {
                cmbEnvironment.SelectedItem = defaultEnv;
            }
            else if (cmbEnvironment.Items.Count > 0)
            {
                cmbEnvironment.SelectedIndex = 0;
            }
        }

        private void CmbEnvironment_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedEnv = cmbEnvironment.SelectedItem as ServerEnvironment;
            if (selectedEnv != null)
            {
                lblStatus.Text = $"Environment: {selectedEnv.DisplayText}";
            }
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            using (var dialog = new EnvironmentSettingsDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadEnvironments();
                }
            }
        }

        // ═══════════════════════════════════════════════
        // SEND BUTONU EVENT
        // ═══════════════════════════════════════════════
        private async void BtnSend_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Aktif tab'dan hex al
                string hexMessage = GetCurrentHexMessage();

                // 2. Hex var mı?
                if (string.IsNullOrWhiteSpace(hexMessage))
                {
                    MessageBox.Show(
                        GetNoMessageWarning(),
                        "No Message",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // 3. Environment var mı?
                var selectedEnv = cmbEnvironment.SelectedItem as Models.ServerEnvironment;
                if (selectedEnv == null)
                {
                    MessageBox.Show(
                        "Please select an environment first!",
                        "No Environment",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // 4. Gönder!
                lblStatus.Text = $"Sending message to {selectedEnv.Name}...";

                await MessageSenderHelper.SendMessageAsync(
                    hexMessage,
                    selectedEnv,
                    this);

                lblStatus.Text = $"Message sent to {selectedEnv.Name}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error sending message";
            }
        }

        /// <summary>
        /// Aktif tab'dan hex mesajı al
        /// </summary>
        private string GetCurrentHexMessage()
        {
            if (tabControl.SelectedTab == tabBuild)
            {
                return buildTabManager.GetGeneratedHex();
            }
            else if (tabControl.SelectedTab == tabParse)
            {
                return parseTabManager.GetInputHex();
            }
            else if (tabControl.SelectedTab == tabHistory)
            {
                return historyTabManager.GetSelectedMessageHex();
            }

            return null;
        }

        /// <summary>
        /// Tab'a göre uyarı mesajı
        /// </summary>
        private string GetNoMessageWarning()
        {
            if (tabControl.SelectedTab == tabBuild)
            {
                return "No message to send!\n\n" +
                       "Please generate a message first.";
            }
            else if (tabControl.SelectedTab == tabParse)
            {
                return "No message to send!\n\n" +
                       "Please parse a message first.";
            }
            else if (tabControl.SelectedTab == tabHistory)
            {
                return "No message selected!\n\n" +
                       "Please select a message from history.";
            }

            return "No message to send!";
        }

        // ═══════════════════════════════════════════════
        // KLAVYE KISAYOLU (Ctrl+Enter)
        // ═══════════════════════════════════════════════
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) // incele!!!!!!
        {
            if (keyData == (Keys.Control | Keys.Enter))
            {
                BtnSend_Click(null, null);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }


        ///////////yeni kodlar///////

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            // History tab seçildiğinde mesajları yükle
            if (tabControl.SelectedTab == tabHistory)
            {
                historyTabManager.LoadSavedMessages();
            }
        }

        private void HistoryTabManager_LoadToBuildRequested(object sender, SavedMessage message)
        {
            tabControl.SelectedTab = tabBuild;
            buildTabManager.LoadMessage(message);
        }

        private void HistoryTabManager_LoadToParseRequested(object sender, SavedMessage message)
        {
            tabControl.SelectedTab = tabParse;
            parseTabManager.LoadMessage(message);
        }
       
        private void ParseTabManager_LoadToBuildRequested(object sender, ParsedMessage parsedMessage)
        {
            // Build tab'a geç
            tabControl.SelectedTab = tabBuild;

            // Parse edilmiş mesajı Build tab'a yükle
            buildTabManager.LoadFromParsedMessage(parsedMessage);

            // Bilgi mesajı
            MessageBox.Show(
                $"Message '{parsedMessage.MTI}' loaded to Build tab!\n\n" +
                $"You can now modify the fields and generate a new hex message.",
                "Loaded Successfully",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        #endregion
    }
}