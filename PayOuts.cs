using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distro
{
    public class PayOut
    {
        public int MemberID { get; set; }
        public DateTime PayoutDate { get; set; }
        public DateTime PayoutEarnedDate { get; set; }
        public string PayoutType { get; set; }
        public decimal Payout { get; set; }
        public string PayoutCurrency { get; set; }

    }
}
