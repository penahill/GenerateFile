using CsvHelper;
using Dapper;
using Modere.Services.Data;
using Modere.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distro
{
    public static class ProcessSubscription
    {
        public static DataResponse<List<Subscription>> ProcessSubscriptionInfo(string lastDateFormatted, string todayDateFormatted, string warehouseID, string fileDate)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Exigo"];
            string filePath = ConfigurationManager.AppSettings["DistroFilePath"];
            string fileName = "Subscription_" + fileDate + ".csv";

            try
            {
                List<Subscription> subscription = new List<Subscription>();
                String message = "";
                bool headerWritten = false;

                using (var connection = new SqlConnection(connectionString.ConnectionString))
                {
                    const string sql = "dbo.distroSubscriptions";
                    connection.Open();

                    subscription = connection.Query<Subscription>(
                        sql,
                        new
                        {
                            @lastDate = lastDateFormatted,
                            @todayDate = todayDateFormatted
                        },
                        commandTimeout: 480,
                        commandType: CommandType.StoredProcedure
                        ).ToList();

                    connection.Close();
                }
                
                if (subscription.Count == 0)
                {
                    message = "No records found for Subscription.";
                    Logging.InsertErrorLogInfo(0, "Distro", "INFO", message, connectionString);
                    return ErrorResponse.CreateErrorResponse<List<Subscription>>(ResponseCode.InternalServerError, message);
                }

                string fileString = filePath + fileName;

                using (var textWriter = new StreamWriter(@fileString))
                {
                    var writer = new CsvWriter(textWriter);
                    writer.Configuration.Delimiter = ",";

                    foreach (var info in subscription)
                    {
                        if (headerWritten == false)
                        {
                            writer.WriteField("SubscriptionID");
                            writer.WriteField("MemberID");
                            writer.WriteField("SubscriptionStartDate");
                            writer.WriteField("SubscriptionEndDate");
                            writer.WriteField("SubscriptionIntervalType");
                            writer.WriteField("SubscriptionInterval");

                            writer.NextRecord();
                            headerWritten = true;
                        }

                        writer.WriteField(info.SubscriptionID);
                        writer.WriteField(info.MemberID);
                        writer.WriteField(info.SubscriptionStartDate);
                        writer.WriteField(info.SubscriptionEndDate);
                        writer.WriteField(info.SubscriptionIntervalType);
                        writer.WriteField(info.SubscriptionInterval);

                        writer.NextRecord();
                    }
                }

                // Using Distro file transfer
                //string result = SentFile.SendFileInfo(fileString, fileName);

                //if (!string.IsNullOrWhiteSpace(result))
                //{
                //    return new DataResponse<List<Subscription>> { Error = new Error { Message = result } };
                //}

                return new DataResponse<List<Subscription>> { Data = subscription };

            }
            catch (Exception ex)
            {
                return ErrorResponse.CreateErrorResponse<List<Subscription>>(ResponseCode.InternalServerError, ex.Message);
            }
        }
    }
}
