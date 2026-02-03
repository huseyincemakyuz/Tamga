using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Tamga.Models
{
    /// <summary>
    /// TCP/IP ile ISO 8583 mesaj gönderici
    /// </summary>
    public class MessageSender
    {
        /// <summary>
        /// Mesaj gönder ve yanıt al
        /// </summary>
        public async Task<MessageResponse> SendMessageAsync(string hexMessage, ServerEnvironment environment)
        {
            var response = new MessageResponse();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // ═══════════════════════════════════════════════
                // HEX STRING HAZIRLA
                // ═══════════════════════════════════════════════
                hexMessage = CleanHexString(hexMessage);

                // ═══════════════════════════════════════════════
                // HEX STRING → BINARY BYTE ARRAY
                // ═══════════════════════════════════════════════
                byte[] binaryMessage = HexStringToByteArray(hexMessage);

                // ═══════════════════════════════════════════════
                // TCP BAĞLANTISI
                // ═══════════════════════════════════════════════
                using (var client = new TcpClient())
                {
                    client.ReceiveTimeout = environment.TimeoutSeconds * 1000;
                    client.SendTimeout = environment.TimeoutSeconds * 1000;

                    await client.ConnectAsync(environment.Host, environment.Port);

                    using (var stream = client.GetStream())
                    {
                        // ═══════════════════════════════════════════════
                        // BURAYA C++ KODUNA GÖRE SEND YAZACAĞIZ
                        // ═══════════════════════════════════════════════
                        // TODO: C++ gateway formatına göre gönderim yapılacak
                        //
                        // Örnek:
                        // await SendLengthAndMessage(stream, binaryMessage);
                        //
                        // ═══════════════════════════════════════════════

                        // ═══════════════════════════════════════════════
                        // YANIT AL
                        // ═══════════════════════════════════════════════
                        var responseData = await ReceiveResponse(stream);

                        stopwatch.Stop();

                        if (responseData != null && responseData.Length > 0)
                        {
                            // Binary → Hex String
                            response.HexResponse = ByteArrayToHexString(responseData);
                            response.Success = true;
                            response.ResponseTime = stopwatch.Elapsed;

                            // Parse et
                            try
                            {
                                var parser = new Iso8583MessageParser();
                                response.ParsedResponse = parser.Parse(response.HexResponse);
                            }
                            catch (Exception parseEx)
                            {
                                response.ErrorMessage = $"Response received but parsing failed: {parseEx.Message}";
                            }
                        }
                        else
                        {
                            response.Success = false;
                            response.ErrorMessage = "No response from server";
                            response.ResponseTime = stopwatch.Elapsed;
                        }
                    }
                }
            }
            catch (SocketException ex)
            {
                stopwatch.Stop();
                response.Success = false;
                response.ErrorMessage = $"Connection error: {ex.Message}";
                response.ResponseTime = stopwatch.Elapsed;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                response.Success = false;
                response.ErrorMessage = $"Error: {ex.Message}";
                response.ResponseTime = stopwatch.Elapsed;
            }

            return response;
        }

        /// <summary>
        /// Yanıt al (Gateway'den gelen response)
        /// TODO: C++ koduna göre düzenlenecek
        /// </summary>
        private async Task<byte[]> ReceiveResponse(NetworkStream stream)
        {
            // ═══════════════════════════════════════════════
            // BURAYA C++ KODUNA GÖRE RECEIVE YAZACAĞIZ
            // ═══════════════════════════════════════════════
            // TODO: Gateway'in response formatına göre okuma yapılacak
            //
            // Örnek senaryo 1: Length + Binary
            // 1. Length oku (ASCII veya binary)
            // 2. Binary message oku
            //
            // Örnek senaryo 2: Sadece binary
            // 1. Buffer'a oku
            //
            // ═══════════════════════════════════════════════

            // Geçici implementasyon (buffer'a oku)
            byte[] buffer = new byte[8192];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead > 0)
            {
                byte[] result = new byte[bytesRead];
                Array.Copy(buffer, result, bytesRead);
                return result;
            }

            return null;
        }

        /// <summary>
        /// Hex string'i temizle (boşluk, satır sonu, vb.)
        /// </summary>
        private string CleanHexString(string hex)
        {
            return hex.Replace(" ", "")
                      .Replace("\n", "")
                      .Replace("\r", "")
                      .Replace("\t", "")
                      .ToUpper();
        }

        /// <summary>
        /// Hex string'i binary byte array'e çevir
        /// C++'daki hex_to_bin() fonksiyonunun karşılığı
        /// </summary>
        private byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException("Hex string length must be even");
            }

            int length = hex.Length / 2;
            byte[] bytes = new byte[length];

            for (int i = 0; i < length; i++)
            {
                string byteStr = hex.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(byteStr, 16);
            }

            return bytes;
        }

        /// <summary>
        /// Binary byte array'i hex string'e çevir
        /// </summary>
        private string ByteArrayToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                sb.AppendFormat("{0:X2}", b);
            }
            return sb.ToString();
        }
    }
}