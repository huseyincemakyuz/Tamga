using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Iso8583MessageBuilder.Forms
{
    public class SaveMessageDialog : Form
    {
        private TextBox txtName;
        private TextBox txtTags;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;
        private Label lblTagsPlaceholder;

        public string MessageName { get; private set; }
        public string[] Tags { get; private set; }
        public string Notes { get; private set; }

        public SaveMessageDialog(string defaultName = "")
        {
            InitializeComponent(defaultName);
        }

        private void InitializeComponent(string defaultName)
        {
            this.Text = "Save Message";
            this.Size = new Size(450, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblName = new Label
            {
                Location = new Point(20, 20),
                Text = "Message Name:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            txtName = new TextBox
            {
                Location = new Point(20, 45),
                Width = 390,
                Font = new Font("Segoe UI", 10),
                Text = defaultName
            };

            var lblTags = new Label
            {
                Location = new Point(20, 80),
                Text = "Tags (comma separated):",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            txtTags = new TextBox
            {
                Location = new Point(20, 105),
                Width = 390,
                Font = new Font("Segoe UI", 10)
            };

            // Placeholder effect için label (TextBox boşken görünür)
            lblTagsPlaceholder = new Label
            {
                Location = new Point(23, 108),
                Text = "e.g., test, purchase, refund",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true,
                BackColor = Color.White,
                Visible = string.IsNullOrEmpty(txtTags.Text)
            };

            // TextBox'a yazı yazılınca placeholder gizle
            txtTags.TextChanged += (s, e) =>
            {
                lblTagsPlaceholder.Visible = string.IsNullOrEmpty(txtTags.Text);
            };

            // Placeholder'a tıklayınca TextBox'a focus
            lblTagsPlaceholder.Click += (s, e) =>
            {
                txtTags.Focus();
            };

            var lblNotes = new Label
            {
                Location = new Point(20, 140),
                Text = "Notes (optional):",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            txtNotes = new TextBox
            {
                Location = new Point(20, 165),
                Width = 390,
                Height = 60,
                Multiline = true,
                Font = new Font("Segoe UI", 9),
                ScrollBars = ScrollBars.Vertical
            };

            btnSave = new Button
            {
                Location = new Point(250, 240),
                Width = 75,
                Text = "Save",
                DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 0;

            btnCancel = new Button
            {
                Location = new Point(335, 240),
                Width = 75,
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Font = new Font("Segoe UI", 9)
            };

            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Lütfen mesaj adını giriniz!", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }

                MessageName = txtName.Text.Trim();
                Tags = txtTags.Text.Split(',')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToArray();
                Notes = txtNotes.Text.Trim();
            };

            this.Controls.AddRange(new Control[]
            {
                lblName, txtName,
                lblTags, txtTags, lblTagsPlaceholder, // lblTagsPlaceholder eklendi
                lblNotes, txtNotes,
                btnSave, btnCancel
            });

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }
    }
}