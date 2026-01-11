using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tamga.Models
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
                // Boşlukları ve satır sonlarını kaldırıyoruz. Tek atrı haline gitiriyoruz
                hexString = hexString.Replace(" ", "").Replace("\r", "").Replace("\n", "").Trim();

                // Hex stringi byte'a çeviriyoruz
                byte[] messageBytes = HexStringToByteArray(hexString);
                int position = 0;

                // 1. Parse MTI (4 ASCII karakter = 4 bytes)
                if (messageBytes.Length < 4)
                {
                    result.Errors.Add("The message is too short - the MTI is unreadable!");
                    return result;
                }

                result.MTI = Encoding.ASCII.GetString(messageBytes, position, 4);
                position += 4;

                // 2. Parse Primary Bitmap (8 bytes = 64 bits)
                if (messageBytes.Length < position + 8)
                {
                    result.Errors.Add("The message is too short - the bitmap is unreadable!");
                    return result;
                }

                byte[] primaryBitmap = new byte[8];
                Array.Copy(messageBytes, position, primaryBitmap, 0, 8);
                result.PrimaryBitmap = BitConverter.ToString(primaryBitmap).Replace("-", "");
                position += 8;

                // Secondary bitmap'in olup olmadığını kontrol ediyoruz exists (primary bitmap'in birinci biti)
                bool hasSecondaryBitmap = IsBitSet(primaryBitmap, 1);
                byte[] secondaryBitmap = null;

                if (hasSecondaryBitmap)
                {
                    if (messageBytes.Length < position + 8)
                    {
                        result.Errors.Add("A secondary bitmap has been specified but is not available!");
                        return result;
                    }

                    secondaryBitmap = new byte[8];
                    Array.Copy(messageBytes, position, secondaryBitmap, 0, 8);
                    result.SecondaryBitmap = BitConverter.ToString(secondaryBitmap).Replace("-", "");
                    position += 8;
                }

                // 3. Bitmap'e dayalı alanları parse ediyoruz
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
                            result.Errors.Add($"The field {fieldNum} is set but not defined.");
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
                            result.Errors.Add($"Field could not be parsed  {fieldNum}: {ex.Message}");
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
                        throw new Exception($"Not enough data for the fixed field ({fieldLength} bytes required)");
                    }
                    value = Encoding.ASCII.GetString(messageBytes, position, fieldLength);
                    position += fieldLength;
                    break;

                case LengthType.LLVAR:
                    if (position + 2 > messageBytes.Length)
                    {
                        throw new Exception("Not enough data is available to read the LLVAR length prefix.");
                    }
                    string lengthStr = Encoding.ASCII.GetString(messageBytes, position, 2);
                    position += 2;

                    if (!int.TryParse(lengthStr, out fieldLength))
                    {
                        throw new Exception($"Invalid LLVAR length: {lengthStr}");
                    }

                    if (position + fieldLength > messageBytes.Length)
                    {
                        throw new Exception($"Not enough data is available to read the LLVAR length prefix. ({fieldLength} bytes required)");
                    }

                    value = Encoding.ASCII.GetString(messageBytes, position, fieldLength);
                    position += fieldLength;
                    break;

                case LengthType.LLLVAR:
                    if (position + 3 > messageBytes.Length)
                    {
                        throw new Exception("Not enough data is available to read the LLLVAR length prefix.");
                    }
                    string lengthStr3 = Encoding.ASCII.GetString(messageBytes, position, 3);
                    position += 3;

                    if (!int.TryParse(lengthStr3, out fieldLength))
                    {
                        throw new Exception($"Invalid LLLVAR length: {lengthStr3}");
                    }

                    if (position + fieldLength > messageBytes.Length)
                    {
                        throw new Exception($"Not enough data is available to read the LLLVAR length prefix. ({fieldLength} bytes required)");
                    }

                    value = Encoding.ASCII.GetString(messageBytes, position, fieldLength);
                    position += fieldLength;
                    break;
            }

            return value.TrimEnd(); // Remove trailing spaces for fixed fields
        }

        // Alan kontrolü
        private bool IsBitSet(byte[] bitmap, int bitPosition)  // bitPosition => örneğin F10 alanı 10.bit oluyor. 1'den 64 kadar olan ilk bitmap içinde pozisyonu 10.
        {
            //Bit index:   7 6 5 4 3 2 1 0
            //Binary:     MSB           LSB

            // Bit pozisyonu 1 tabanlıdır.
            int byteIndex = (bitPosition - 1) / 8;  // F10'dan ilerleyelim => 1 byte 8 bit o halde F10 hangi byte içinde olduğunu nasıl buluruz pozisyonunu 8 bölerek (10-1/8=1).  -1 olayı ilk bit secondary bitmap var mı onun değeri o sebeple onu çıkardık.
            int bitIndex = 7 - ((bitPosition - 1) % 8); // F10 byte indexi 1 yani 2.byte'da (0,1,2,...). peki bit indexi nedir => bu byte'ın içindeki konumu, byte içinde bitler soldan sağa yerleşiyor 7,6,5,4,3,2,1,0 => 7=>f9,6=>f10 formülde bu şekilde çıkıyor.

            if (byteIndex >= bitmap.Length)
                return false;

            return (bitmap[byteIndex] & (1 << bitIndex)) != 0; // << ifadesi 1 değerini biIndexdeki değer kadar sola kaydırır. & => (bitwiseand tek tek bit üzerinde (1&1=1,1&0=0) çalışıyor)            

        }

        private byte[] HexStringToByteArray(string hex) 
        {
            if (hex.Length % 2 != 0) // Hex formatta 1 byte = 2 hex karakter
            {
                throw new ArgumentException("The hex string length must be even!");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16); // 16 -> hex tabanı
            }
            return bytes;
        }
    }
}