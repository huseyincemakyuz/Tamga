using System;
using System.Drawing;
using System.Windows.Forms;
using Tamga.Models;

namespace Tamga.Forms.Dialogs
{
    /// <summary>
    /// Ortam ayarları dialog formu
    /// </summary>
    public class EnvironmentSettingsDialog : Form
    {
        private DataGridView dgvEnvironments;
        private Button btnSave;
        private Button btnCancel;
        private Button btnAdd;
        private Button btnDelete;
        private Button btnSetDefault;

        public EnvironmentSettingsDialog()
        {
            InitializeComponent();
            LoadEnvironments();
        }

        private void InitializeComponent()
        {
            this.Text = "Environment Settings";
            this.Size = new Size(800, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // DataGridView
            dgvEnvironments = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(750, 300),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };

            // Columns
            dgvEnvironments.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "IsDefault",
                HeaderText = "Default",
                DataPropertyName = "IsDefault",
                Width = 60,
                ReadOnly = false
            });

            dgvEnvironments.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "IsEnabled",
                HeaderText = "Enabled",
                DataPropertyName = "IsEnabled",
                Width = 60,
                ReadOnly = false
            });

            dgvEnvironments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Name",
                DataPropertyName = "Name",
                Width = 120,
                ReadOnly = false
            });

            dgvEnvironments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Host",
                HeaderText = "Host",
                DataPropertyName = "Host",
                Width = 150,
                ReadOnly = false
            });

            dgvEnvironments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Port",
                HeaderText = "Port",
                DataPropertyName = "Port",
                Width = 60,
                ReadOnly = false
            });

            dgvEnvironments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TimeoutSeconds",
                HeaderText = "Timeout (s)",
                DataPropertyName = "TimeoutSeconds",
                Width = 80,
                ReadOnly = false
            });

            dgvEnvironments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Description",
                HeaderText = "Description",
                DataPropertyName = "Description",
                ReadOnly = false
            });

            // Buttons
            btnAdd = new Button
            {
                Text = "➕ Add New",
                Location = new Point(20, 340),
                Width = 100,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            btnDelete = new Button
            {
                Text = "🗑️ Delete",
                Location = new Point(130, 340),
                Width = 100,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;

            btnSetDefault = new Button
            {
                Text = "⭐ Set Default",
                Location = new Point(240, 340),
                Width = 120,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black
            };
            btnSetDefault.FlatAppearance.BorderSize = 0;
            btnSetDefault.Click += BtnSetDefault_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(570, 340),
                Width = 90,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            btnSave = new Button
            {
                Text = "💾 Save",
                Location = new Point(670, 340),
                Width = 100,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[]
            {
                dgvEnvironments,
                btnAdd, btnDelete, btnSetDefault,
                btnCancel, btnSave
            });

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadEnvironments()
        {
            dgvEnvironments.DataSource = null;
            dgvEnvironments.DataSource = EnvironmentSettings.Instance.Environments;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var newEnv = new ServerEnvironment
            {
                Name = "New Environment",
                Host = "localhost",
                Port = 5000,
                TimeoutSeconds = 30,
                IsDefault = false,
                IsEnabled = true,
                Description = ""
            };

            EnvironmentSettings.Instance.Environments.Add(newEnv);
            LoadEnvironments();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvEnvironments.SelectedRows.Count > 0)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this environment?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    var env = dgvEnvironments.SelectedRows[0].DataBoundItem as ServerEnvironment;
                    EnvironmentSettings.Instance.Environments.Remove(env);
                    LoadEnvironments();
                }
            }
        }

        private void BtnSetDefault_Click(object sender, EventArgs e)
        {
            if (dgvEnvironments.SelectedRows.Count > 0)
            {
                // Tüm default'ları kaldır
                foreach (var env in EnvironmentSettings.Instance.Environments)
                {
                    env.IsDefault = false;
                }

                // Seçiliyi default yap
                var selectedEnv = dgvEnvironments.SelectedRows[0].DataBoundItem as ServerEnvironment;
                selectedEnv.IsDefault = true;

                LoadEnvironments();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validation
                foreach (var env in EnvironmentSettings.Instance.Environments)
                {
                    if (string.IsNullOrWhiteSpace(env.Name))
                    {
                        MessageBox.Show("Environment name cannot be empty!", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(env.Host))
                    {
                        MessageBox.Show("Host cannot be empty!", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (env.Port <= 0 || env.Port > 65535)
                    {
                        MessageBox.Show($"Invalid port number for {env.Name}!", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Kaydet
                EnvironmentSettings.Instance.Save();

                MessageBox.Show("Settings saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}