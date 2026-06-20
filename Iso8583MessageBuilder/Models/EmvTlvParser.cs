using System;
using System.Collections.Generic;
using System.Text;

namespace Tamga.Models
{
    public class EmvTag
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public int Length { get; set; }
        public string Value { get; set; }      // hex gosterim
        public string Decoded { get; set; }    // varsa insan-okur cozumleme
    }

    /// <summary>
    /// EMV BER-TLV cozumleyicisi. F55 (EMV Data) gibi binary alanlarin
    /// icerigini tag-uzunluk-deger ucluleri halinde verir.
    /// </summary>
    public static class EmvTlvParser
    {
        private static readonly Dictionary<string, string> TagNames = new Dictionary<string, string>
        {
            { "4F",   "Application Identifier (AID)" },
            { "50",   "Application Label" },
            { "57",   "Track 2 Equivalent Data" },
            { "5A",   "Application PAN" },
            { "5F20", "Cardholder Name" },
            { "5F24", "Application Expiration Date" },
            { "5F25", "Application Effective Date" },
            { "5F28", "Issuer Country Code" },
            { "5F2A", "Transaction Currency Code" },
            { "5F2D", "Language Preference" },
            { "5F30", "Service Code" },
            { "5F34", "Application PAN Sequence Number" },
            { "82",   "Application Interchange Profile (AIP)" },
            { "84",   "Dedicated File (DF) Name" },
            { "87",   "Application Priority Indicator" },
            { "88",   "Short File Identifier (SFI)" },
            { "8A",   "Authorisation Response Code" },
            { "8C",   "CDOL1" },
            { "8D",   "CDOL2" },
            { "8E",   "CVM List" },
            { "8F",   "CA Public Key Index" },
            { "90",   "Issuer Public Key Certificate" },
            { "91",   "Issuer Authentication Data" },
            { "92",   "Issuer Public Key Remainder" },
            { "93",   "Signed Static Application Data" },
            { "94",   "Application File Locator (AFL)" },
            { "95",   "Terminal Verification Results (TVR)" },
            { "97",   "TDOL" },
            { "98",   "Transaction Certificate Hash Value" },
            { "99",   "Transaction PIN Data" },
            { "9A",   "Transaction Date" },
            { "9B",   "Transaction Status Information (TSI)" },
            { "9C",   "Transaction Type" },
            { "9D",   "Directory Definition File Name" },
            { "9F01", "Acquirer Identifier" },
            { "9F02", "Amount, Authorised" },
            { "9F03", "Amount, Other" },
            { "9F04", "Amount, Other (Binary)" },
            { "9F05", "Application Discretionary Data" },
            { "9F06", "AID (Terminal)" },
            { "9F07", "Application Usage Control" },
            { "9F08", "Application Version Number (ICC)" },
            { "9F09", "Application Version Number (Terminal)" },
            { "9F0D", "Issuer Action Code - Default" },
            { "9F0E", "Issuer Action Code - Denial" },
            { "9F0F", "Issuer Action Code - Online" },
            { "9F10", "Issuer Application Data" },
            { "9F11", "Issuer Code Table Index" },
            { "9F12", "Application Preferred Name" },
            { "9F13", "Last Online ATC Register" },
            { "9F14", "Lower Consecutive Offline Limit" },
            { "9F15", "Merchant Category Code" },
            { "9F16", "Merchant Identifier" },
            { "9F17", "PIN Try Counter" },
            { "9F18", "Issuer Script Identifier" },
            { "9F1A", "Terminal Country Code" },
            { "9F1B", "Terminal Floor Limit" },
            { "9F1C", "Terminal Identification" },
            { "9F1D", "Terminal Risk Management Data" },
            { "9F1E", "IFD Serial Number" },
            { "9F1F", "Track 1 Discretionary Data" },
            { "9F21", "Transaction Time" },
            { "9F22", "CA Public Key Index (Terminal)" },
            { "9F26", "Application Cryptogram" },
            { "9F27", "Cryptogram Information Data" },
            { "9F2D", "ICC PIN Encipherment Public Key Certificate" },
            { "9F2E", "ICC PIN Encipherment Public Key Exponent" },
            { "9F2F", "ICC PIN Encipherment Public Key Remainder" },
            { "9F32", "Issuer Public Key Exponent" },
            { "9F33", "Terminal Capabilities" },
            { "9F34", "CVM Results" },
            { "9F35", "Terminal Type" },
            { "9F36", "Application Transaction Counter (ATC)" },
            { "9F37", "Unpredictable Number" },
            { "9F38", "PDOL" },
            { "9F39", "POS Entry Mode" },
            { "9F3A", "Amount, Reference Currency" },
            { "9F3B", "Application Reference Currency" },
            { "9F3C", "Transaction Reference Currency Code" },
            { "9F40", "Additional Terminal Capabilities" },
            { "9F41", "Transaction Sequence Counter" },
            { "9F42", "Application Currency Code" },
            { "9F44", "Application Currency Exponent" },
            { "9F45", "Data Authentication Code" },
            { "9F4C", "ICC Dynamic Number" },
            { "9F4D", "Log Entry" },
            { "9F4E", "Merchant Name and Location" },
        };

        public static List<EmvTag> Parse(string hex)
        {
            var result = new List<EmvTag>();
            if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0)
                return result;

            byte[] bytes;
            try
            {
                bytes = HexToBytes(hex);
            }
            catch
            {
                return result;
            }

            int pos = 0;
            while (pos < bytes.Length)
            {
                // Padding byte'larini atla (0x00)
                if (bytes[pos] == 0x00)
                {
                    pos++;
                    continue;
                }

                // Tag oku (1 veya cok byte)
                int tagStart = pos;
                byte first = bytes[pos++];
                if ((first & 0x1F) == 0x1F)
                {
                    while (pos < bytes.Length && (bytes[pos] & 0x80) != 0)
                        pos++;
                    if (pos < bytes.Length) pos++;
                }
                if (pos > bytes.Length) break;
                string tag = BytesToHex(bytes, tagStart, pos - tagStart);

                if (pos >= bytes.Length) break;

                // Length oku (BER-TLV)
                int len;
                byte lenFirst = bytes[pos++];
                if ((lenFirst & 0x80) == 0)
                {
                    len = lenFirst;
                }
                else
                {
                    int lenBytes = lenFirst & 0x7F;
                    if (lenBytes == 0 || pos + lenBytes > bytes.Length)
                        break;
                    len = 0;
                    for (int i = 0; i < lenBytes; i++)
                        len = (len << 8) | bytes[pos++];
                }

                // Value oku
                string value;
                if (pos + len <= bytes.Length)
                {
                    value = BytesToHex(bytes, pos, len);
                    pos += len;
                }
                else
                {
                    value = BytesToHex(bytes, pos, bytes.Length - pos);
                    pos = bytes.Length;
                }

                result.Add(new EmvTag
                {
                    Tag = tag,
                    Name = TagNames.TryGetValue(tag, out var n) ? n : "Unknown",
                    Length = len,
                    Value = value,
                    Decoded = DecodeValue(tag, value)
                });
            }

            return result;
        }

        private static string DecodeValue(string tag, string hexValue)
        {
            switch (tag)
            {
                case "9A": // Date YYMMDD
                    if (hexValue.Length == 6)
                        return $"20{hexValue.Substring(0, 2)}-{hexValue.Substring(2, 2)}-{hexValue.Substring(4, 2)}";
                    break;
                case "9F21": // Time HHMMSS
                    if (hexValue.Length == 6)
                        return $"{hexValue.Substring(0, 2)}:{hexValue.Substring(2, 2)}:{hexValue.Substring(4, 2)}";
                    break;
                case "9C":
                    switch (hexValue)
                    {
                        case "00": return "Purchase";
                        case "01": return "Cash Advance";
                        case "09": return "Purchase with Cashback";
                        case "20": return "Refund";
                    }
                    break;
                case "5F2A":
                case "9F1A":
                case "5F28":
                    return CurrencyOrCountry(hexValue);
                case "9F02":
                case "9F03":
                    if (long.TryParse(hexValue, out long amount))
                        return (amount / 100m).ToString("F2");
                    break;
            }
            return null;
        }

        private static string CurrencyOrCountry(string hex)
        {
            switch (hex)
            {
                case "0949": return "TRY (Turkey)";
                case "0978": return "EUR";
                case "0840": return "USD";
                case "0826": return "GBP";
                case "0792": return "TR";
                case "0276": return "DE";
                case "0840": return "US";
            }
            return null;
        }

        private static byte[] HexToBytes(string hex)
        {
            byte[] b = new byte[hex.Length / 2];
            for (int i = 0; i < b.Length; i++)
                b[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return b;
        }

        private static string BytesToHex(byte[] data, int offset, int length)
        {
            var sb = new StringBuilder(length * 2);
            for (int i = 0; i < length; i++)
                sb.Append(data[offset + i].ToString("X2"));
            return sb.ToString();
        }
    }
}
