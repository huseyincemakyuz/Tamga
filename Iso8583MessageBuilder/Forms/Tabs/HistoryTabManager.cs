using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Tamga.Models;

namespace Tamga.Forms.Tabs
{
    public class HistoryTabManager
    {
        #region Fields

        private TabPage tabPage;
        private DataGridView dgvMessages;
        private TextBox txtSearch;
        private RichTextBox rtbMessageDetail;
        private Button btnLoadToBuild;
        private Button btnLoadToParse;
        private Button btnDeleteMessage;
        private Button btnExportMessage;

        private MessageStorageManager storageManager;
        private List<SavedMessage> currentMessages;

        // Events
        public event EventHandler<SavedMessage> LoadToBuildRequested;
        public event EventHandler<SavedMessage> LoadToParseRequested;

        #endregion

        #region Constructor

        public HistoryTabManager(TabPage tab, MessageStorageManager storage)
        {
            tabPage = tab;
            storageManager = storage;
            currentMessages = new List<SavedMessage>();
            InitializeUI();
        }

        #endregion

        #region UI Initialization

        private void InitializeUI()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // Top Panel - Search
            var topPanel = CreateSearchPanel();

            // Middle Panel - DataGridView
            var middlePanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            dgvMessages = CreateDataGridView();
            middlePanel.Controls.Add(dgvMessages);

            // Bottom Panel - Details & Actions
            var bottomPanel = CreateDetailsPanel();

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

            tabPage.Controls.Add(mainPanel);
        }

        private Panel CreateSearchPanel()
        {
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
            return topPanel;
        }

        private DataGridView CreateDataGridView()
        {
            var dgv = new DataGridView
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
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                DataPropertyName = "Id",
                Visible = false
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Message Name",
                DataPropertyName = "Name",
                Width = 250,
                DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 9, FontStyle.Bold) }
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MTI",
                HeaderText = "MTI",
                DataPropertyName = "MTI",
                Width = 60
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Timestamp",
                HeaderText = "Date/Time",
                DataPropertyName = "Timestamp",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm:ss" }
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Tags",
                HeaderText = "Tags",
                DataPropertyName = "Tags",
                Width = 200
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FieldCount",
                HeaderText = "Fields",
                DataPropertyName = "FieldCount",
                Width = 60
            });

            dgv.SelectionChanged += DgvMessages_SelectionChanged;
            dgv.CellDoubleClick += DgvMessages_CellDoubleClick;

            return dgv;
        }

        private Panel CreateDetailsPanel()
        {
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

            var buttonPanel = CreateActionButtonPanel();

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

            return bottomPanel;
        }

        private Panel CreateActionButtonPanel()
        {
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

            return buttonPanel;
        }

        #endregion

        #region Data Management

        public void LoadSavedMessages()
        {
            try
            {
                currentMessages = storageManager.LoadMessages();

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

        #region Event Handlers

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

                DisplayMessageDetails(message);
            }
            catch (Exception ex)
            {
                rtbMessageDetail.Clear();
                AppendColored($"Error displaying message: {ex.Message}", Color.Red, false);
            }
        }

        private void DgvMessages_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                BtnLoadToBuild_Click(sender, e);
            }
        }

        private void BtnLoadToBuild_Click(object sender, EventArgs e)
        {
            var message = GetSelectedMessage();
            if (message == null) return;

            LoadToBuildRequested?.Invoke(this, message);
            MessageBox.Show($"Message '{message.Name}' loaded to Build tab!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnLoadToParse_Click(object sender, EventArgs e)
        {
            var message = GetSelectedMessage();
            if (message == null) return;

            LoadToParseRequested?.Invoke(this, message);
            MessageBox.Show($"Message '{message.Name}' loaded to Parse tab!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDeleteMessage_Click(object sender, EventArgs e)
        {
            var message = GetSelectedMessage();
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

        private void BtnExportMessage_Click(object sender, EventArgs e)
        {
            var message = GetSelectedMessage();
            if (message == null) return;

            Clipboard.SetText(message.HexMessage);
            MessageBox.Show("Hex message copied to clipboard!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Display Methods

        private void DisplayMessageDetails(SavedMessage message)
        {
            rtbMessageDetail.Clear();

            AppendColored($"📋 {message.Name}\n\n", Color.DarkBlue, true);
            AppendColored($"MTI: {message.MTI}\n", Color.Black, false);
            AppendColored($"Saved: {message.Timestamp:yyyy-MM-dd HH:mm:ss}\n", Color.Gray, false);

            if (message.Tags != null && message.Tags.Count > 0)
            {
                AppendColored($"Tags: {string.Join(", ", message.Tags)}\n", Color.DarkCyan, false);
            }

            if (!string.IsNullOrEmpty(message.Notes))
            {
                AppendColored($"Notes: {message.Notes}\n", Color.Gray, false);
            }

            AppendColored("\nHex Message:\n", Color.Black, true);
            AppendColored($"{FormatHexString(message.HexMessage)}\n", Color.DarkGreen, false);

            if (message.Fields != null && message.Fields.Count > 0)
            {
                AppendColored($"\nFields ({message.Fields.Count}):\n", Color.Black, true);
                AppendColored("─────────────────────────────────────────────────────\n", Color.Gray, false);

                foreach (var field in message.Fields.OrderBy(f => f.Key))
                {
                    var fieldDef = Iso8583Fields.Fields.ContainsKey(field.Key)
                        ? Iso8583Fields.Fields[field.Key]
                        : null;

                    AppendColored($"F{field.Key:D3} ", Color.DarkRed, true);

                    if (fieldDef != null)
                    {
                        AppendColored($"- {fieldDef.Name}\n", Color.Gray, false);
                        AppendColored($"     Type: {fieldDef.Type}, {fieldDef.LengthType}\n", Color.DarkCyan, false);
                    }
                    else
                    {
                        AppendColored($"\n", Color.Gray, false);
                    }

                    AppendColored($"     Value: ", Color.Gray, false);
                    AppendColored($"{field.Value}\n\n", Color.DarkBlue, false);
                }
            }
        }

        private void AppendColored(string text, Color color, bool bold = false)
        {
            rtbMessageDetail.SelectionStart = rtbMessageDetail.TextLength;
            rtbMessageDetail.SelectionLength = 0;
            rtbMessageDetail.SelectionColor = color;
            rtbMessageDetail.SelectionFont = new Font(rtbMessageDetail.Font, bold ? FontStyle.Bold : FontStyle.Regular);
            rtbMessageDetail.AppendText(text);
            rtbMessageDetail.SelectionColor = rtbMessageDetail.ForeColor;
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

        #endregion

        #region Helper Methods

        private SavedMessage GetSelectedMessage()
        {
            if (dgvMessages.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a message first!", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            var row = dgvMessages.SelectedRows[0];
            var messageId = row.Cells["Id"].Value?.ToString();
            return currentMessages.FirstOrDefault(m => m.Id == messageId);
        }

        #endregion
    }
}