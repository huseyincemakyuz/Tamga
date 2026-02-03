using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tamga.Models
{
    /// <summary>
    /// UI ile entegre send helper
    /// </summary>
    public static class MessageSenderHelper
    {
        public static async Task SendMessageAsync(
            string hexMessage,
            ServerEnvironment environment,
            Form parentForm)
        {
            Form loadingForm = null;

            try
            {
                // ═══════════════════════════════════════════════
                // LOADING DIALOG GÖSTER
                // ═══════════════════════════════════════════════
                loadingForm = new Form
                {
                    Text = "Sending Message...",
                    Size = new Size(400, 150),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    ControlBox = false,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                var label = new Label
                {
                    Text = $"Sending message to {environment.Name}...\n\n" +
                           $"Host: {environment.Host}:{environment.Port}\n" +
                           "Please wait...",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9)
                };
                loadingForm.Controls.Add(label);
                loadingForm.Show(parentForm);
                Application.DoEvents();

                // ═══════════════════════════════════════════════
                // MESAJ GÖNDER
                // ═══════════════════════════════════════════════
                var sender = new MessageSender();
                var response = await sender.SendMessageAsync(hexMessage, environment);

                loadingForm.Close();
                loadingForm = null;

                // ═══════════════════════════════════════════════
                // SONUCU GÖSTER
                // ═══════════════════════════════════════════════
                ShowResponseDialog(response, environment, parentForm);
            }
            catch (Exception ex)
            {
                if (loadingForm != null)
                {
                    loadingForm.Close();
                }

                MessageBox.Show(
                    $"Error sending message: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static void ShowResponseDialog(
            MessageResponse response,
            ServerEnvironment environment,
            Form parentForm)
        {
            var resultForm = new Form
            {
                Text = response.Success ? "✅ Message Sent Successfully" : "❌ Send Failed",
                Size = new Size(750, 600),
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false
            };

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.None
            };

            // Header
            AppendColored(rtb, "═══════════════════════════════════════════\n", Color.Gray, false);
            AppendColored(rtb, $"   {environment.Name}\n", Color.DarkBlue, true);
            AppendColored(rtb, "═══════════════════════════════════════════\n\n", Color.Gray, false);

            AppendColored(rtb, "Host: ", Color.Gray, false);
            AppendColored(rtb, $"{environment.Host}:{environment.Port}\n", Color.Black, false);

            AppendColored(rtb, "Timestamp: ", Color.Gray, false);
            AppendColored(rtb, $"{response.Timestamp:yyyy-MM-dd HH:mm:ss}\n", Color.Black, false);

            AppendColored(rtb, "Response Time: ", Color.Gray, false);
            AppendColored(rtb, $"{response.ResponseTime.TotalMilliseconds:F0} ms\n\n",
                response.ResponseTime.TotalSeconds < 1 ? Color.Green : Color.Orange, true);

            if (response.Success)
            {
                AppendColored(rtb, "✅ SUCCESS\n\n", Color.Green, true);

                AppendColored(rtb, "Response (Hex):\n", Color.Black, true);
                AppendColored(rtb, "─────────────────────────────────────\n", Color.Gray, false);
                AppendColored(rtb, response.HexResponse + "\n\n", Color.DarkBlue, false);

                if (response.ParsedResponse != null && response.ParsedResponse.Errors.Count == 0)
                {
                    AppendColored(rtb, "Parsed Response:\n", Color.Black, true);
                    AppendColored(rtb, "─────────────────────────────────────\n", Color.Gray, false);

                    AppendColored(rtb, "MTI: ", Color.Gray, false);
                    AppendColored(rtb, $"{response.ParsedResponse.MTI}\n", Color.DarkGreen, true);

                    AppendColored(rtb, "Fields: ", Color.Gray, false);
                    AppendColored(rtb, $"{response.ParsedResponse.Fields.Count}\n\n", Color.Black, false);

                    foreach (var field in response.ParsedResponse.Fields)
                    {
                        if (!Iso8583Fields.Fields.ContainsKey(field.Key))
                            continue;

                        var fieldDef = Iso8583Fields.Fields[field.Key];

                        AppendColored(rtb, $"F{field.Key:D3} ", Color.DarkRed, true);
                        AppendColored(rtb, $"- {fieldDef.Name}\n", Color.Gray, false);
                        AppendColored(rtb, $"      Value: ", Color.Gray, false);
                        AppendColored(rtb, $"{field.Value}\n", Color.DarkBlue, false);
                    }
                }
                else if (response.ParsedResponse != null && response.ParsedResponse.Errors.Count > 0)
                {
                    AppendColored(rtb, "\n⚠ Warning: Response has parsing errors\n\n", Color.Orange, true);
                }
            }
            else
            {
                AppendColored(rtb, "❌ FAILED\n\n", Color.Red, true);
                AppendColored(rtb, "Error:\n", Color.Black, true);
                AppendColored(rtb, "─────────────────────────────────────\n", Color.Gray, false);
                AppendColored(rtb, response.ErrorMessage + "\n\n", Color.Red, false);

                AppendColored(rtb, "Possible reasons:\n", Color.Gray, false);
                AppendColored(rtb, "• Server is not running\n", Color.Gray, false);
                AppendColored(rtb, "• Wrong host/port configuration\n", Color.Gray, false);
                AppendColored(rtb, "• Network connectivity issues\n", Color.Gray, false);
                AppendColored(rtb, "• Firewall blocking connection\n", Color.Gray, false);
                AppendColored(rtb, "• Connection timeout\n", Color.Gray, false);
            }

            mainPanel.Controls.Add(rtb);

            // Close button
            var btnClose = new Button
            {
                Text = "Close",
                Dock = DockStyle.Bottom,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => resultForm.Close();

            resultForm.Controls.Add(mainPanel);
            resultForm.Controls.Add(btnClose);
            resultForm.ShowDialog(parentForm);
        }

        private static void AppendColored(RichTextBox rtb, string text, Color color, bool bold)
        {
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionLength = 0;
            rtb.SelectionColor = color;
            rtb.SelectionFont = new Font(rtb.Font, bold ? FontStyle.Bold : FontStyle.Regular);
            rtb.AppendText(text);
            rtb.SelectionColor = rtb.ForeColor;
        }
    }
}