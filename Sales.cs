using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distro
{
    public class Sales
    {
        public int MemberID { get; set; }
        public DateTime SalesDate { get; set; }
        public int OrderID { get; set; }
        public decimal SalesValue { get; set; }
        public decimal BV { get; set; }
        public decimal QV { get; set; }
        public int Qty { get; set; }
        public string SalesValueCurrency { get; set; }
    }
}
