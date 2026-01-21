using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Tamga.Common;
using Tamga.Models;

namespace Tamga.Controls
{
    public class FieldControl : UserControl
    {
        private CheckBox chkEnabled;
        private Label lblFieldName;
        private TextBox txtValue;
        private Button btnAutoGenerate;
        private Label lblFieldInfo;
        private string lastStan;

        public int FieldNumber { get; private set; }
        public FieldDefinition Definition { get; private set; }

        public bool IsFieldEnabled => chkEnabled.Checked;
        public string FieldValue => txtValue.Text;

        public event EventHandler ValueChanged;

        public FieldControl(FieldDefinition fieldDef, bool isRequired = false)
        {
            Definition = fieldDef;
            FieldNumber = fieldDef.FieldNumber;
            InitializeComponent(isRequired);
        }

        private void InitializeComponent(bool isRequired)
        {
            this.Height = 60;
            this.Width = 600;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Padding = new Padding(5);
            this.BackColor = Color.White;

            chkEnabled = new CheckBox
            {
                Location = new Point(10, 10),
                Width = 30,
                Checked = isRequired,
                Enabled = !isRequired
            };

            lblFieldName = new Label
            {
                Location = new Point(45, 10),
                Width = 150,
                Text = $"F{Definition.FieldNumber} - {Definition.Name}",
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            lblFieldInfo = new Label
            {
                Location = new Point(45, 30),
                Width = 200,
                Text = $"{Definition.Type}, {Definition.LengthType} (Max: {Definition.MaxLength})",
                Font = new Font("Segoe UI", 7),
                ForeColor = Color.Gray
            };

            txtValue = new TextBox
            {
                Location = new Point(250, 10),
                Width = 270,
                Enabled = chkEnabled.Checked
            };

            btnAutoGenerate = new Button
            {
                Location = new Point(530, 10),
                Width = 60,
                Height = 23,
                Text = "Auto",
                Visible = CanAutoGenerate()
            };

            chkEnabled.CheckedChanged += (s, e) =>
            {
                txtValue.Enabled = chkEnabled.Checked;
                ValueChanged?.Invoke(this, EventArgs.Empty);
            };

            txtValue.TextChanged += (s, e) => ValueChanged?.Invoke(this, EventArgs.Empty);
            btnAutoGenerate.Click += BtnAutoGenerate_Click;

            this.Controls.AddRange(new Control[]
            {
                chkEnabled, lblFieldName, lblFieldInfo, txtValue, btnAutoGenerate
            });
        }

        private bool CanAutoGenerate()
        {
            return new[] { 7, 11, 12, 13, 37 }.Contains(Definition.FieldNumber);
        }

        private void BtnAutoGenerate_Click(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            MessageBox.Show(IsoRuntimeState.LastStan ?? "STAN NULL");


            switch (Definition.FieldNumber)
            {
                case 7: // Transmission DateTime MMDDhhmmss
                    txtValue.Text = now.ToString("MMddHHmmss");
                    break;
                case 11: // STAN
                    string stan = new Random().Next(1, 999999).ToString("D6");

                    IsoRuntimeState.LastStan = stan;
                    txtValue.Text = stan;
                    break;
                case 12: // Local Time hhmmss
                    txtValue.Text = now.ToString("HHmmss");
                    break;
                case 13: // Local Date MMDD
                    txtValue.Text = now.ToString("MMdd");
                    break;
                case 37: // RRN şimdilik böyle kasın düzelteceğim

                    if (string.IsNullOrEmpty(IsoRuntimeState.LastStan)) 
                    {
                        MessageBox.Show("Please generate STAN first (Field 11).");
                        return;
                    }

                    int y = now.Year%10;
                    int ddd = now.DayOfYear;
                    int hh = now.Hour;

                    txtValue.Text = $"{y}{ddd:D3}{hh:D2}{IsoRuntimeState.LastStan}";
                    break;
            }

            chkEnabled.Checked = true;
        }

        public void SetValue(string value)
        {
            txtValue.Text = value;
            chkEnabled.Checked = !string.IsNullOrEmpty(value);
        }

        // ═══════════════════════════════════════════════════════════
        // YENİ METOTLAR
        // ═══════════════════════════════════════════════════════════
       
        public void EnableField(bool enable)
        {
            // Checkbox'ı işaretle/kaldır
            chkEnabled.Checked = enable;

            // TextBox'ı enable/disable et
            txtValue.Enabled = enable;

            // TextBox rengini değiştir (opsiyonel - görsel feedback)
            txtValue.BackColor = enable
                ? Color.White
                : Color.FromArgb(240, 240, 240);
        }
        
        public void Clear()
        {
            txtValue.Clear();
            chkEnabled.Checked = false;
        }
        
        public string GetValue()
        {
            return txtValue.Text.Trim();
        }


    }
}