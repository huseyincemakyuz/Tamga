using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iso8583MessageBuilder.Models
{
    public class Iso8583MessageBuilder
    {
        private string _mti;
        private Dictionary<int, string> _fields = new Dictionary<int, string>();

        public Iso8583MessageBuilder SetMTI(string mti)
        {
            if (mti.Length != 4)
                throw new ArgumentException("MTI must be 4 digits");

            _mti = mti;
            return this;
        }

        public Iso8583MessageBuilder SetField(int fieldNumber, string value)
        {
            if (!Iso8583Fields.Fields.ContainsKey(fieldNumber))
                throw new ArgumentException($"Field {fieldNumber} is not defined");

            _fields[fieldNumber] = value;
            return this;
        }

        public string BuildHexString()
        {
            if (string.IsNullOrEmpty(_mti))
                throw new InvalidOperationException("MTI must be set");

            var messageBytes = new List<byte>();

            // 1. MTI ekle
            messageBytes.AddRange(Encoding.ASCII.GetBytes(_mti));

            // 2. Bitmap oluştur ve ekle
            var bitmap = GenerateBitmap();
            messageBytes.AddRange(bitmap);

            // 3. Field'ları ekle (sırayla)
            foreach (var fieldNumber in _fields.Keys.OrderBy(k => k))
            {
                var fieldDef = Iso8583Fields.Fields[fieldNumber];
                var fieldValue = _fields[fieldNumber];
                var fieldBytes = FormatField(fieldDef, fieldValue);
                messageBytes.AddRange(fieldBytes);
            }

            // 4. Hex string'e çevir
            return ByteArrayToHexString(messageBytes.ToArray());
        }

        private byte[] GenerateBitmap()
        {
            int maxField = _fields.Keys.Max();
            bool needSecondary = maxField > 64;

            byte[] primaryBitmap = new byte[8];

            if (needSecondary)
            {
                SetBit(primaryBitmap, 1);
            }

            foreach (var fieldNumber in _fields.Keys)
            {
                if (fieldNumber <= 64)
                {
                    SetBit(primaryBitmap, fieldNumber);
                }
            }

            if (needSecondary)
            {
                byte[] secondaryBitmap = new byte[8];
                foreach (var fieldNumber in _fields.Keys)
                {
                    if (fieldNumber > 64 && fieldNumber <= 128)
                    {
                        SetBit(secondaryBitmap, fieldNumber - 64);
                    }
                }
                return primaryBitmap.Concat(secondaryBitmap).ToArray();
            }

            return primaryBitmap;
        }

        private void SetBit(byte[] bitmap, int bitPosition)
        {
            int byteIndex = (bitPosition - 1) / 8;
            int bitIndex = 7 - ((bitPosition - 1) % 8);

            bitmap[byteIndex] |= (byte)(1 << bitIndex);
        }

        private byte[] FormatField(FieldDefinition fieldDef, string value)
        {
            var result = new List<byte>();

            switch (fieldDef.LengthType)
            {
                case LengthType.Fixed:
                    if (fieldDef.Type == FieldType.Numeric || fieldDef.Type == FieldType.Amount)
                    {
                        value = value.PadLeft(fieldDef.MaxLength, '0');
                    }
                    else
                    {
                        value = value.PadRight(fieldDef.MaxLength, ' ');
                    }
                    result.AddRange(Encoding.ASCII.GetBytes(value));
                    break;

                case LengthType.LLVAR:
                    string lengthPrefix = value.Length.ToString("D2");
                    result.AddRange(Encoding.ASCII.GetBytes(lengthPrefix));
                    result.AddRange(Encoding.ASCII.GetBytes(value));
                    break;

                case LengthType.LLLVAR:
                    string lengthPrefix3 = value.Length.ToString("D3");
                    result.AddRange(Encoding.ASCII.GetBytes(lengthPrefix3));
                    result.AddRange(Encoding.ASCII.GetBytes(value));
                    break;
            }

            return result.ToArray();
        }

        private string ByteArrayToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }
    }
}