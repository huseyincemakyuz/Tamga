using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tamga.Models
{
    public class Tamga
    {
        private string _mti;
        private Dictionary<int, string> _fields = new Dictionary<int, string>();

        public Tamga SetMTI(string mti)
        {
            if (mti.Length != 4)
                throw new ArgumentException("MTI must be a 4-digit number.");

            _mti = mti;
            return this;
        }

        public Tamga SetField(int fieldNumber, string value)
        {
            if (!Iso8583Fields.Fields.ContainsKey(fieldNumber))
                throw new ArgumentException($"The field {fieldNumber} is not defined!");

            _fields[fieldNumber] = value;
            return this;
        }

        public string BuildHexString()
        {
            if (string.IsNullOrEmpty(_mti))
                throw new InvalidOperationException("MTI must be defined!");

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

        private void SetBit(byte[] bitmap, int bitPosition) // 1 HEX karakter = 4 bit => primary bitmap 64 bit => 16*4=64 => bitmap 16 hane olmalı
        {
            int byteIndex = (bitPosition - 1) / 8;
            int bitIndex = 7 - ((bitPosition - 1) % 8);

            bitmap[byteIndex] |= (byte)(1 << bitIndex); // | bitwise OR => 1|1=1 , 1|0=1 => byteIndex diyelim 2 oradaki bitlerle set edeceğim bitlere göre karşılaştırma yapıp atama yapıyoruz böylece öneki atamları bozmadan direkt kendi atamalarımızı yapıyoruz.
        }

        // Bitmap nasıl oluşur:
        // 1 hex karakter = 4 bit  => tabloya göre bir banry hexe dönüşümü => 01110000 = 0x70 => bu byte'ın binary gösteriminde hex gösterimibe çevrilmesi, stringe çevirirsek 70 olur. 
        //| Binary | Hex |         Bit:      7   6   5   4   3   2   1   0    (2 üzeri n) => bu tabloyuda binaryd ecimale nasıl dönüştürürüz onu kullanmak için ekledim                             
        //| ------ | --- |         Değer:  128  64  32  16   8   4   2   1    
        //| 0000   | 0   |        
        //| 0001   | 1   |        01110000 => 0*128=0, 1*64=64, 1*32=32, 1*16=16, 0*8=0, 0*4=0, 0*2=0 ,0*1=0 => hespini topla => 112 yani  01110000 (binary) => 0x70 (hex) => 112 (decimal) (1 BYTE GÖSTERİMLERİ)
        //| 0010   | 2   |        0x70 => 7*(16 üzeri 1) + 0*(16 üzeri 0) = 7*16 = 112 (hex 16 tabanlı buradan da decimale çevirebiliriz)
        //| 0011   | 3   |
        //| 0100   | 4   |
        //| 0101   | 5   |
        //| 0110   | 6   |
        //| 0111   | 7   |
        //| 1000   | 8   |
        //| 1001   | 9   |
        //| 1010   | A   |
        //| 1011   | B   |
        //| 1100   | C   |
        //| 1101   | D   |
        //| 1110   | E   |
        //| 1111   | F   |

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
                    string lengthPrefix = value.Length.ToString("D2"); // D2 vs incele
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
            return BitConverter.ToString(bytes).Replace("-", ""); // byte[] bytes = { 0x70, 0x38, 0x20, 0x00 }; => "70-38-20-00" => "70382000";
        }       

    }
}