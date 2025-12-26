using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iso8583MessageBuilder.Models
{
    public class ParsedMessage
    {
        public string MTI { get; set; }
        public string PrimaryBitmap { get; set; }
        public string SecondaryBitmap { get; set; }
        public Dictionary<int, string> Fields { get; set; } = new Dictionary<int, string>();
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class Iso8583MessageParser
    {
        public ParsedMessage Parse(string hexString)
        {
            var result = new ParsedMessage();

            try
            {
                // Remove spaces and newlines
                hexString = hexString.Replace(" ", "").Replace("\r", "").Replace("\n", "").Trim();

                // Convert hex to bytes
                byte[] messageBytes = HexStringToByteArray(hexString);
                int position = 0;

                // 1. Parse MTI (4 ASCII characters = 4 bytes)
                if (messageBytes.Length < 4)
                {
                    result.Errors.Add("Message too short - cannot read MTI");
                    return result;
                }

                result.MTI = Encoding.ASCII.GetString(messageBytes, position, 4);
                position += 4;

                // 2. Parse Primary Bitmap (8 bytes = 64 bits)
                if (messageBytes.Length < position + 8)
                {
                    result.Errors.Add("Message too short - cannot read bitmap");
                    return result;
                }

                byte[] primaryBitmap = new byte[8];
                Array.Copy(messageBytes, position, primaryBitmap, 0, 8);
                result.PrimaryBitmap = BitConverter.ToString(primaryBitmap).Replace("-", "");
                position += 8;

                // Check if secondary bitmap exists (bit 1 of primary bitmap)
                bool hasSecondaryBitmap = IsBitSet(primaryBitmap, 1);
                byte[] secondaryBitmap = null;

                if (hasSecondaryBitmap)
                {
                    if (messageBytes.Length < position + 8)
                    {
                        result.Errors.Add("Secondary bitmap indicated but not present");
                        return result;
                    }

                    secondaryBitmap = new byte[8];
                    Array.Copy(messageBytes, position, secondaryBitmap, 0, 8);
                    result.SecondaryBitmap = BitConverter.ToString(secondaryBitmap).Replace("-", "");
                    position += 8;
                }

                // 3. Parse Fields based on bitmap
                for (int fieldNum = 2; fieldNum <= 128; fieldNum++)
                {
                    bool fieldPresent = false;

                    if (fieldNum <= 64)
                    {
                        fieldPresent = IsBitSet(primaryBitmap, fieldNum);
                    }
                    else if (hasSecondaryBitmap && secondaryBitmap != null)
                    {
                        fieldPresent = IsBitSet(secondaryBitmap, fieldNum - 64);
                    }

                    if (fieldPresent)
                    {
                        if (!Iso8583Fields.Fields.ContainsKey(fieldNum))
                        {
                            result.Errors.Add($"Field {fieldNum} is set but not defined in configuration");
                            continue;
                        }

                        var fieldDef = Iso8583Fields.Fields[fieldNum];

                        try
                        {
                            string fieldValue = ParseField(messageBytes, ref position, fieldDef);
                            result.Fields[fieldNum] = fieldValue;
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Error parsing field {fieldNum}: {ex.Message}");
                        }
                    }
                }

                if (position < messageBytes.Length)
                {
                    result.Errors.Add($"Warning: {messageBytes.Length - position} bytes remaining after parsing");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Fatal parsing error: {ex.Message}");
            }

            return result;
        }

        private string ParseField(byte[] messageBytes, ref int position, FieldDefinition fieldDef)
        {
            int fieldLength = 0;
            string value = "";

            switch (fieldDef.LengthType)
            {
                case LengthType.Fixed:
                    fieldLength = fieldDef.MaxLength;
                    if (position + fieldLength > messageBytes.Length)
                    {
                        throw new Exception($"Not enough data for fixed field (need {fieldLength} bytes)");
                    }
                    value = Encoding.ASCII.GetString(messageBytes, position, fieldLength);
                    position += fieldLength;
                    break;

                case LengthType.LLVAR:
                    if (position + 2 > messageBytes.Length)
                    {
                        throw new Exception("Not enough data for LLVAR length prefix");
                    }
                    string lengthStr = Encoding.ASCII.GetString(messageBytes, position, 2);
                    position += 2;

                    if (!int.TryParse(lengthStr, out fieldLength))
                    {
                        throw new Exception($"Invalid LLVAR length: {lengthStr}");
                    }

                    if (position + fieldLength > messageBytes.Length)
                    {
                        throw new Exception($"Not enough data for LLVAR field (need {fieldLength} bytes)");
                    }

                    value = Encoding.ASCII.GetString(messageBytes, position, fieldLength);
                    position += fieldLength;
                    break;

                case LengthType.LLLVAR:
                    if (position + 3 > messageBytes.Length)
                    {
                        throw new Exception("Not enough data for LLLVAR length prefix");
                    }
                    string lengthStr3 = Encoding.ASCII.GetString(messageBytes, position, 3);
                    position += 3;

                    if (!int.TryParse(lengthStr3, out fieldLength))
                    {
                        throw new Exception($"Invalid LLLVAR length: {lengthStr3}");
                    }

                    if (position + fieldLength > messageBytes.Length)
                    {
                        throw new Exception($"Not enough data for LLLVAR field (need {fieldLength} bytes)");
                    }

                    value = Encoding.ASCII.GetString(messageBytes, position, fieldLength);
                    position += fieldLength;
                    break;
            }

            return value.TrimEnd(); // Remove trailing spaces for fixed fields
        }

        private bool IsBitSet(byte[] bitmap, int bitPosition)
        {
            // Bit position is 1-based
            int byteIndex = (bitPosition - 1) / 8;
            int bitIndex = 7 - ((bitPosition - 1) % 8);

            if (byteIndex >= bitmap.Length)
                return false;

            return (bitmap[byteIndex] & (1 << bitIndex)) != 0;
        }

        private byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException("Hex string must have even length");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
    }
}