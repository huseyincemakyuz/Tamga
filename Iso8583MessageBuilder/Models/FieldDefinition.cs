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
        Fixed,
        LLVAR,
        LLLVAR
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