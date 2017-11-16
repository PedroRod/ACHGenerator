using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACHGenerator.CustomAttributes
{
    public class ACHField : Attribute
    {
        public int Position { get; set; }
        public int Length { get; set; }
        public string Format { get; set; }

    }
}
