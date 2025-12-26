using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Iso8583MessageBuilder.Models;

namespace Iso8583MessageBuilder.Forms
{
    public class AddFieldDialog : Form
    {
        private ComboBox cmbFields;
        private Button btnOK;
        private Button btnCancel;

        public int? SelectedFieldNumber { get; private set; }

        public AddFieldDialog()
        {
            InitializeComponent();
            LoadAvailableFields();
        }

        private void InitializeComponent()
        {
            this.Text = "Add Field";
            this.Size = new Size(400, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblSelect = new Label
            {
                Location = new Point(20, 20),
                Text = "Select Field:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            cmbFields = new ComboBox
            {
                Location = new Point(20, 45),
                Width = 340,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };

            btnOK = new Button
            {
                Location = new Point(200, 80),
                Width = 75,
                Text = "OK",
                DialogResult = DialogResult.OK
            };

            btnCancel = new Button
            {
                Location = new Point(285, 80),
                Width = 75,
                Text = "Cancel",
                DialogResult = DialogResult.Cancel
            };

            btnOK.Click += (s, e) =>
            {
                if (cmbFields.SelectedItem is ComboBoxItem item)
                {
                    SelectedFieldNumber = (int)item.Value;
                }
            };

            this.Controls.AddRange(new Control[] { lblSelect, cmbFields, btnOK, btnCancel });
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void LoadAvailableFields()
        {
            foreach (var field in Iso8583Fields.Fields.OrderBy(f => f.Key))
            {
                cmbFields.Items.Add(new ComboBoxItem
                {
                    Text = $"F{field.Key:D3} - {field.Value.Name}",
                    Value = field.Key
                });
            }

            if (cmbFields.Items.Count > 0)
                cmbFields.SelectedIndex = 0;
        }
    }   
}