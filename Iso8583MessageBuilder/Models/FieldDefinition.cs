using System;
using System.Collections.Generic;

namespace Iso8583MessageBuilder.Models
{
    public enum FieldType
    {
        Numeric,
        AlphaNumeric,
        Binary,
        Amount,
        Date,
        Time
    }

    public enum LengthType
    {
        Fixed, // Sabit Uzunluk (Örnek:F3 - Processing Code (Fixed, 6 karakter))
        LLVAR, // LL  = Length-Length (2 haneli uzunluk),        VAR = Variable (değişken uzunluk) => Önce 2 haneli uzunluk bilgisi gönderilir, sonra asıl değer gelir
        LLLVAR // LLL = Length-Length-Length (3 haneli uzunluk), VAR = Variable (değişken uzunluk) => Önce 3 haneli uzunluk bilgisi gönderilir, sonra asıl değer gelir
    }

    public class FieldDefinition
    {
        public int FieldNumber { get; set; }
        public string Name { get; set; }
        public FieldType Type { get; set; }
        public LengthType LengthType { get; set; }
        public int MaxLength { get; set; }
    }
}