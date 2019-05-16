using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distro
{
    public static class ProcessDistro
    {
        public static string ProcessDistroAll(string lastDateFormatted, string todayDateFormatted, string warehouseID, string fileDate)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Exigo"];
            
            //Member Profile
            var result = ProcessMemberProfile.ProcessMemberProfileInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);
            string message = null;

            if (result.Error != null)
            {
                Logging.InsertErrorLogInfo(0, "Distro", "Error", result.Error.Message, connectionString);
                SendEmail.SendErrorEmail(result.Error.Message, "Member Profile");
                return result.Error.Message;
            }

            //Subscription  
            var result2 = ProcessSubscription.ProcessSubscriptionInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

            if (result2.Error != null)
            {
                Logging.InsertErrorLogInfo(0, "Distro", "Error", result2.Error.Message, connectionString);
                SendEmail.SendErrorEmail(result2.Error.Message, "Subscription");
                return result2.Error.Message;
            }

            //Enroller Tree
            var result3 = ProcessEnrollerTree.ProcessEnrollerTreeInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

            if (result3.Error != null)
            {
                Logging.InsertErrorLogInfo(0, "Distro", "Error", result3.Error.Message, connectionString);
                SendEmail.SendErrorEmail(result3.Error.Message, "Enroller Tree");
                return result3.Error.Message;
            }

            //Sponsor Tree
            var result4 = ProcessSponsorTree.ProcessSponsorTreeInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

            if (result4.Error != null)
            {
                Logging.InsertErrorLogInfo(0, "Distro", "Error", result4.Error.Message, connectionString);
                SendEmail.SendErrorEmail(result4.Error.Message, "Sponsor Tree");
                return result4.Error.Message;
            }

            //Sales
            var result5 = ProcessSales.ProcessSalesInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

            if (result5.Error != null)
            {
                Logging.InsertErrorLogInfo(0, "Distro", "Error", result5.Error.Message, connectionString);
                SendEmail.SendErrorEmail(result5.Error.Message, "Sales");
                return result5.Error.Message;
            }
                        
            //Sales Detail
            var result6 = ProcessSalesDetail.ProcessSalesDetailInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

            if (result6.Error != null)
            {
                Logging.InsertErrorLogInfo(0, "Distro", "Error", result6.Error.Message, connectionString);
                SendEmail.SendErrorEmail(result6.Error.Message, "Sales Detail");
                return result6.Error.Message;
            }

            //Payout 
            var result7 = ProcessPayout.ProcessPayoutInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

            if (result7.Error != null)
            {
                Logging.InsertErrorLogInfo(0, "Distro", "Error", result7.Error.Message, connectionString);
                SendEmail.SendErrorEmail(result7.Error.Message, "Payout");
                return result7.Error.Message;
            }

            //Member Rank 
            var result8 = ProcessMemberRank.ProcessMemberRankInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

            if (result8.Error != null)
            {
                Logging.InsertErrorLogInfo(0, "Distro", "Error", result8.Error.Message, connectionString);
                SendEmail.SendErrorEmail(result8.Error.Message, "Member Rank");
                return result8.Error.Message;
            }

            //Member Rank Detail 
            var result9 = ProcessMemberRankDetail.ProcessMemberRankDetailInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

            if (result9.Error != null)
            {
                Logging.InsertErrorLogInfo(0, "Distro", "Error", result9.Error.Message, connectionString);
                SendEmail.SendErrorEmail(result9.Error.Message, "Member Rank Detail");
                return result9.Error.Message;
            }

            //Promoter Rank 
            var result10 = ProcessPromoterRank.ProcessPromoterRankInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

            if (result10.Error != null)
            {
                Logging.InsertErrorLogInfo(0, "Distro", "Error", result10.Error.Message, connectionString);
                SendEmail.SendErrorEmail(result10.Error.Message, "Promoter Rank");
                return result10.Error.Message;
            }

            //Promoter Rank Detail 
            var result11 = ProcessPromoterRankDetail.ProcessPromoterRankDetailInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

            if (result11.Error != null)
            {
                Logging.InsertErrorLogInfo(0, "Distro", "Error", result11.Error.Message, connectionString);
                SendEmail.SendErrorEmail(result11.Error.Message, "Promoter Rank Detail");
                return result11.Error.Message;
            }

            return message;
        }
    }
}
