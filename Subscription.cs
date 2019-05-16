using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distro
{
    public class Subscription
    {
        public int SubscriptionID { get; set; }
        public int MemberID { get; set; }
        public DateTime SubscriptionStartDate { get; set; }
        public DateTime SubscriptionEndDate { get; set; }
        public string SubscriptionIntervalType { get; set; }
        public int SubscriptionInterval { get; set; }
    }
}
