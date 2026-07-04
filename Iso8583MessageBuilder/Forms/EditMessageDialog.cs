using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tamga.Models;

namespace Tamga.Forms
{
    public class EditMessageDialog : Form
    {
        public TextBox txtName;
        public TextBox txtTags;
        public TextBox txtNotes;
        public Button btnSave;
        public Button btnCancel;
        public Label lblTagsPlaceHolder;

        public string MessageName {  get; private set; }
        public List<string> Tags { get; private set; }
        public string Notes { get; private set; }

        public EditMessageDialog(SavedMessage message)
        {
            IntializeComponent(message);
        }

        private void IntializeComponent(SavedMessage message)
        {
            this.Text = "Edit Message";
            this.Size = new Size(450, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblName = new Label 
            {
                Location= new Point(20, 20),
                Text = "Message Name:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            txtName = new TextBox
            {
                Location = new Point(20, 45),
                Width = 390,                
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Text = message.Name
            };

            var lblTags = new Label
            {
                Location = new Point(20, 80),
                Text = "Tags (comma separeted)",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            string existingTags = message.Tags != null && message.Tags.Count > 0 ? string.Join(", ", message.Tags) : "";

            txtTags = new TextBox
            {
                Location = new Point(20, 105),
                Width = 390,
                Font = new Font("Segoe UI", 10),
                Text = existingTags
            };

            lblTagsPlaceHolder = new Label
            {
                Location = new Point(23, 108),
                Text = "e.g., test, purchase, refund",                
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true,
                BackColor = Color.White,
                Visible = string.IsNullOrEmpty(txtTags.Text)
            };

            txtTags.TextChanged += (s, e) =>
            {
                lblTagsPlaceHolder.Visible = string.IsNullOrEmpty(txtTags.Text);
            };

            lblTagsPlaceHolder.Click += (s, e) =>
            {
                txtTags.Focus();
            };

            var lblNotes = new Label
            {
                Location = new Point(20, 140),
                Text = "TNotes (optional):",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            txtNotes = new TextBox
            {
                Location = new Point(20, 165),
                Width = 390,
                Height = 60,
                Font = new Font("Segoe UI", 9),
                ScrollBars = ScrollBars.Vertical,
                Text = message.Notes ?? ""
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
                    MessageBox.Show("Please enter your message name!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.OK;
                    return;
                }

                MessageName = txtName.Text.Trim();
                Tags = txtTags.Text.Split(',')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();
                Notes = txtNotes.Text.Trim();
            };

            this.Controls.AddRange(new Control[]
            {
                lblName, txtName,
                lblTags, txtTags, lblTagsPlaceHolder,
                lblNotes, txtNotes,
                btnSave, btnCancel
            });

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

    }
}
