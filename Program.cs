using Dapper;
using Modere.Services.Data;
using Modere.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distro
{
    public class Program
    {
        static void Main(string[] args)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Exigo"];
            string warehouseID = "";
            string programToRun = "";
            DateTime lastdate = DateTime.MinValue;
            DateTime todaydate = DateTime.Now;
            string lastDateFormatted;
            string todayDateFormatted = todaydate.ToString("yyyy-MM-dd 00:00:00");
            DateTime filedateutc = DateTime.UtcNow;
            string fileDate = filedateutc.ToString("MMddyyyy");

            try
            {
                if (args.Length == 0)
                {
                    Logging.InsertErrorLogInfo(0, "Distro", "INFO", "Need to pass in what process to run. (no argument)", connectionString);
                    return;
                }
                else if (args.Length == 2)
                {
                    // Both Warehouse and Program was passed in
                    warehouseID = args[0];
                    programToRun = args[1];
                }
                else if (args.Length == 1)
                {
                    // Only warehouse was passed in
                    warehouseID = args[0];
                }

                if (String.IsNullOrWhiteSpace(warehouseID))
                {
                    // Should never happen seeing warehouse id(s) are required to be passed in.
                    warehouseID = "1,2";
                }
                
                if (string.IsNullOrWhiteSpace(programToRun))
                {
                    programToRun = "All";
                }

                // Get last run date
                string sql = @"select BatchDateTime 
                                 from BizAppExtension.BatchUpdateTime
                                 where BatchIdentifier = 'Distro'";

                using (var connection = new SqlConnection(connectionString.ConnectionString))
                {
                    connection.Open();
                    lastdate = connection.Query<DateTime>(sql).FirstOrDefault();
                    connection.Close();
                }

                if (lastdate == DateTime.MinValue)
                {
                    lastdate = DateTime.Now.AddDays(-1);
                }

                lastDateFormatted = lastdate.ToString("yyyy-MM-dd 00:00:00");

                switch (programToRun.ToUpper())
                {
                    case "ALL":
                        var resultAll = ProcessDistro.ProcessDistroAll(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

                        if (!string.IsNullOrWhiteSpace(resultAll))
                        {
                            //Error emails has been sent.
                            return;
                        }

                        // If here everything ran right and we need to update the last run date value
                        string sql2 = @"update BizAppExtension.BatchUpdateTime set 
                                       BatchDateTime = @insertdate
                                   where BatchIdentifier = 'Distro'";

                        DateTime insertdate = Convert.ToDateTime(todayDateFormatted);

                        using (var connection = new SqlConnection(connectionString.ConnectionString))
                        {
                            connection.Open();
                            var resultUpdate = connection.Execute(sql2, new
                            {
                                insertdate
                            });
                            connection.Close();
                        }
                        break;
                    case "MEMBERPROFILE":
                        var result = ProcessMemberProfile.ProcessMemberProfileInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

                        if (result.Error != null)
                        {
                            Logging.InsertErrorLogInfo(0, "Distro", "Error", result.Error.Message, connectionString);
                            SendEmail.SendErrorEmail(result.Error.Message, programToRun);
                            return;
                        }
                        break;
                    case "SUBSCRIPTION":
                        var result2 = ProcessSubscription.ProcessSubscriptionInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

                        if (result2.Error != null)
                        {
                            Logging.InsertErrorLogInfo(0, "Distro", "Error", result2.Error.Message, connectionString);
                            SendEmail.SendErrorEmail(result2.Error.Message, programToRun);
                            return;
                        }
                        break;
                    case "ENROLLERTREE":
                        var result3 = ProcessEnrollerTree.ProcessEnrollerTreeInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

                        if (result3.Error != null)
                        {
                            Logging.InsertErrorLogInfo(0, "Distro", "Error", result3.Error.Message, connectionString);
                            SendEmail.SendErrorEmail(result3.Error.Message, programToRun);
                            return;
                        }
                        break;
                    case "SPONSORTREE":
                        var result4 = ProcessSponsorTree.ProcessSponsorTreeInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

                        if (result4.Error != null)
                        {
                            Logging.InsertErrorLogInfo(0, "Distro", "Error", result4.Error.Message, connectionString);
                            SendEmail.SendErrorEmail(result4.Error.Message, programToRun);
                            return;
                        }
                        break;
                    case "SALES":
                        var result5 = ProcessSales.ProcessSalesInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

                        if (result5.Error != null)
                        {
                            Logging.InsertErrorLogInfo(0, "Distro", "Error", result5.Error.Message, connectionString);
                            SendEmail.SendErrorEmail(result5.Error.Message, programToRun);
                            return;
                        }
                        break;
                    case "SALESDETAIL":
                        var result6 = ProcessSalesDetail.ProcessSalesDetailInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

                        if (result6.Error != null)
                        {
                            Logging.InsertErrorLogInfo(0, "Distro", "Error", result6.Error.Message, connectionString);
                            SendEmail.SendErrorEmail(result6.Error.Message, programToRun);
                            return;
                        }
                        break;
                    case "PAYOUT":
                        var result7 = ProcessPayout.ProcessPayoutInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

                        if (result7.Error != null)
                        {
                            Logging.InsertErrorLogInfo(0, "Distro", "Error", result7.Error.Message, connectionString);
                            SendEmail.SendErrorEmail(result7.Error.Message, programToRun);
                            return;
                        }
                        break;
                    case "MEMBERRANK":
                        var result8 = ProcessMemberRank.ProcessMemberRankInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

                        if (result8.Error != null)
                        {
                            Logging.InsertErrorLogInfo(0, "Distro", "Error", result8.Error.Message, connectionString);
                            SendEmail.SendErrorEmail(result8.Error.Message, programToRun);
                            return;
                        }
                        break;
                    case "MEMBERRANKDETAIL":
                        var result9 = ProcessMemberRankDetail.ProcessMemberRankDetailInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

                        if (result9.Error != null)
                        {
                            Logging.InsertErrorLogInfo(0, "Distro", "Error", result9.Error.Message, connectionString);
                            SendEmail.SendErrorEmail(result9.Error.Message, programToRun);
                            return;
                        }
                        break;
                    case "PROMOTERRANK":
                        var result10 = ProcessPromoterRank.ProcessPromoterRankInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

                        if (result10.Error != null)
                        {
                            Logging.InsertErrorLogInfo(0, "Distro", "Error", result10.Error.Message, connectionString);
                            SendEmail.SendErrorEmail(result10.Error.Message, programToRun);
                            return;
                        }
                        break;
                    case "PROMOTERRANKDETAIL":
                        var result11 = ProcessPromoterRankDetail.ProcessPromoterRankDetailInfo(lastDateFormatted, todayDateFormatted, warehouseID, fileDate);

                        if (result11.Error != null)
                        {
                            Logging.InsertErrorLogInfo(0, "Distro", "Error", result11.Error.Message, connectionString);
                            SendEmail.SendErrorEmail(result11.Error.Message, programToRun);
                            return;
                        }
                        break;
                    default:
                        Logging.InsertErrorLogInfo(0, "Distro", "INFO", "Invalid or no program value found", connectionString);
                        return;
                }
            }
            catch (Exception ex)
            {
                emailspace.email em = new emailspace.email();
                List<string> mess = new List<string>();
                mess.Add("Distro has thrown an exception");
                string errorMsg = ex.ToString();
                mess.Add(errorMsg);
                if (ex.InnerException != null)
                {
                    mess.Add(ex.InnerException.ToString());
                }
                string emails = ConfigurationManager.AppSettings["Emailaddresses"];
                em.SendMail("Disto Program has failed - " + Environment.MachineName, mess, "BatchError@modere.com", emails);
                Logging.InsertErrorLogInfo(0, "Distro", "Error", ex.Message, connectionString);
            }
        }
    }
}
