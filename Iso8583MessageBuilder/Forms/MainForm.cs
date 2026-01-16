using System;
using System.Drawing;
using System.Windows.Forms;
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

        #endregion

        #region Constructor

        public MainForm()
        {
            storageManager = new MessageStorageManager();
            InitializeComponent();
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

            // ═══════════════════════════════════════════════
            // YENİ EVENT: Parse'dan Build'e yükleme
            // ═══════════════════════════════════════════════
            parseTabManager.LoadToBuildRequested += ParseTabManager_LoadToBuildRequested;

            // History Tab Manager
            historyTabManager = new HistoryTabManager(tabHistory, storageManager);
            historyTabManager.LoadToBuildRequested += HistoryTabManager_LoadToBuildRequested;
            historyTabManager.LoadToParseRequested += HistoryTabManager_LoadToParseRequested;
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

        // ═══════════════════════════════════════════════
        // YENİ EVENT HANDLER: Parse → Build
        // ═══════════════════════════════════════════════
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