using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tamga.Models
{
    public class Iso8583MessageBuilder
    {
        private string _mti;
        private Dictionary<int, string> _fields = new Dictionary<int, string>();
        private EncodingFormat _encoding = EncodingFormat.ASCII;

        public Iso8583MessageBuilder SetMTI(string mti)
        {
            if (mti.Length != 4)
                throw new ArgumentException("MTI must be a 4-digit number.");

            _mti = mti;
            return this;
        }

        public Iso8583MessageBuilder SetEncoding(EncodingFormat encoding)
        {
            _encoding = encoding;
            return this;
        }

        public Iso8583MessageBuilder SetField(int fieldNumber, string value)
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
            if(_encoding == EncodingFormat.BCD)
            {
                // BCD: "0200" -> [0x02, 0x00] (2 byte)
                messageBytes.AddRange(StringToBcd(_mti));
            }
            else
            {
                // ASCII: "0200" -> [0X30, 0X32, 0X30, 0X30] (4 byte)
                messageBytes.AddRange(Encoding.ASCII.GetBytes(_mti));
            }                

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
                    if (IsNumericField(fieldDef))
                    {
                        value = value.PadLeft(fieldDef.MaxLength, '0');
                    }
                    else
                    {
                        value = value.PadRight(fieldDef.MaxLength, ' ');
                    }

                    if(_encoding == EncodingFormat.BCD && IsNumericField(fieldDef))
                    {
                        // BCD: "000010000000" -> [0x00, 0x00, 0x10, 0x00, 0x00] (6 byte)
                        result.AddRange(StringToBcd(value));
                    }
                    else
                    {
                        // ASCII: "000010000000" -> [0x30,0x30,0x30,0x30,0x31,0x30,....] (12 byte)
                        result.AddRange(Encoding.ASCII.GetBytes(value));
                    }
                    break;

                case LengthType.LLVAR:
                    if (_encoding == EncodingFormat.BCD)
                    {
                        // BCD: Length prefix 1 byte
                        // Ornek: Length = 16 -> [0x16] (1 byte)
                        result.AddRange(StringToBcd(value.Length.ToString("D2")));
                    }
                    else
                    {
                        // ASCII: Length prefix 2 byte
                        // Ornek: length = 16 -> [0x30, 0x36] ("16")
                        result.AddRange(Encoding.ASCII.GetBytes(value.Length.ToString("D2")));
                    }

                    // Data Kismi
                    if (_encoding == EncodingFormat.BCD && IsNumericField(fieldDef))
                    {
                        // BCD numeric data: "5528791222789877" -> [0x55, 0x28,...] (8 byte)
                        result.AddRange(StringToBcd(value));
                    }
                    else
                    {
                        // ASCII: data (aplha/AN veya ASCII mode)
                        result.AddRange(Encoding.ASCII.GetBytes(value));
                    }
                    break;
                    
                case LengthType.LLLVAR:
                    if (_encoding == EncodingFormat.BCD)
                    {
                        // BCD: Length prefix 2 byte
                        // Ornek: Length = 99 -> [0x16, 0x99] (2 byte)
                        result.AddRange(StringToBcd(value.Length.ToString("D4")));
                    }
                    else
                    {
                        // ASCII: Length prefix 3 byte
                        // Ornek: length = 99 -> [0x30, 0x39, 0x39] ("099")
                        result.AddRange(Encoding.ASCII.GetBytes(value.Length.ToString("D3")));
                    }

                    // Data Kismi
                    if (_encoding == EncodingFormat.BCD && IsNumericField(fieldDef))
                    {
                        // BCD numeric data
                        result.AddRange(StringToBcd(value));
                    }
                    else
                    {
                        // ASCII data 
                        result.AddRange(Encoding.ASCII.GetBytes(value));
                    }
                    break;
            }

            return result.ToArray();
        }


        /// <summary>
        /// Field'in numeric olup olmadigini kontrol eder
        /// </summary>        
        private bool IsNumericField(FieldDefinition fieldDef)
        {
            return fieldDef.Type == FieldType.Numeric
                 || fieldDef.Type == FieldType.Amount
                 || fieldDef.Type == FieldType.Date
                 || fieldDef.Type == FieldType.Time;
        }

        /// <summary>
        /// Numeric string'i BCD byte array'e cevirir.
        /// Ornek: "0200" -> [0x02, 0x00]
        /// Ornek: "123" -> [0x01, 0x23] (tek ise basa o eklenir)
        /// </summary>        
        private byte[] StringToBcd(string numericString)
        {
            // Tek uzunluksa basina 0 ekle
            if (numericString.Length % 2 != 0)
                numericString = "0" + numericString;

            byte[] bcd = new byte[numericString.Length / 2];

            for (int i = 0; i < bcd.Length; i++)
            {
                byte high = (byte)(CharToNibble(numericString[i * 2]) << 4);
                byte low = CharToNibble(numericString[i * 2 + 1]);
                bcd[i] = (byte)(high | low);
            }

            return bcd;
        }

        /// <summary>
        /// Tek bir rakam karakterini nibble degerine cevirir
        /// '0' -> 0x0, '5' -> 0x5, '9' -> 0x9
        /// </summary>        
        private byte CharToNibble(char c)
        {
            if(c >= '0' && c <= '9')
                return (byte)(c - '0');

            throw new ArgumentException($"Invalid BCD character : '{c}'. Only digits 0-9 are allowed.");
        }

        private string ByteArrayToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", ""); // byte[] bytes = { 0x70, 0x38, 0x20, 0x00 }; => "70-38-20-00" => "70382000";
        }       

    }
}