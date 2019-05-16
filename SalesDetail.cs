using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distro
{
    public class SalesDetail
    {
        public int OrderID { get; set; }
        public string ProductItemCode { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string ProductCategory { get; set; }
        public string Brand { get; set; }
        public decimal SalesValue { get; set; }
        public decimal BV { get; set; }
        public decimal QV { get; set; }
        public decimal Qty { get; set; }

    }
}
