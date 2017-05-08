using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFAdam.models
{
    public class Tijdslot
    {
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }

        public override string ToString()
        {
            return this.Start.ToShortTimeString() + " -- " + this.Stop.ToShortTimeString();
        }
    }
}
