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
    public static class ProcessPayout
    {
        public static DataResponse<List<PayOut>> ProcessPayoutInfo(string lastDateFormatted, string todayDateFormatted, string warehouseID, string fileDate)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Exigo"];
            string filePath = ConfigurationManager.AppSettings["DistroFilePath"];
            string fileName = "Payout_" + fileDate + ".csv";

            try
            {
                List<PayOut> payout = new List<PayOut>();
                String message = "";
                bool headerWritten = false;
                List<string> currencyList;
                DataTable table = new DataTable();
                table.Columns.Add("Column1", typeof(string));
                table.Columns.Add("Column2", typeof(string));

                string sql = "select CurrencyCode " +
                             "    from WarehouseCurrencies " +
                             "    where warehouseId IN(" + @warehouseID + ") " +
                             "    group by CurrencyCode";
                
                using (var connection = new SqlConnection(connectionString.ConnectionString))
                {
                    connection.Open();
                    currencyList = connection.Query<string>(sql).ToList();

                    foreach (var currency in currencyList)
                    {
                        if (!string.IsNullOrWhiteSpace(currency))
                        {
                            // add a single row for each item in the collection.
                            table.Rows.Add(currency, currency);
                        }
                    }

                    connection.Close();
                }

                using (var connection = new SqlConnection(connectionString.ConnectionString))
                {
                    const string sql2 = "dbo.distroPayout";
                    connection.Open();

                    payout = connection.Query<PayOut>(
                        sql2,
                        new
                        {
                            @currency = table,
                            @lastDate = lastDateFormatted,
                            @todayDate = todayDateFormatted
                        },
                        commandTimeout: 480,
                        commandType: CommandType.StoredProcedure
                        ).ToList();

                    connection.Close();
                }
                                
                if (payout.Count == 0)
                {
                    message = "No records found for Payout.";
                    Logging.InsertErrorLogInfo(0, "Distro", "INFO", message, connectionString);
                    return ErrorResponse.CreateErrorResponse<List<PayOut>>(ResponseCode.InternalServerError, message);
                }

                string fileString = filePath + fileName; 

                using (var textWriter = new StreamWriter(@fileString))
                {
                    var writer = new CsvWriter(textWriter);
                    writer.Configuration.Delimiter = ",";

                    foreach (var info in payout)
                    {
                        if (headerWritten == false)
                        {
                            writer.WriteField("MemberID");
                            writer.WriteField("PayoutDate");
                            writer.WriteField("PayoutEarnedDate");
                            writer.WriteField("PayoutType");
                            writer.WriteField("Payout");
                            writer.WriteField("PayoutCurrency");

                            writer.NextRecord();
                            headerWritten = true;
                        }

                        writer.WriteField(info.MemberID);
                        writer.WriteField(info.PayoutDate);
                        writer.WriteField(info.PayoutEarnedDate);
                        writer.WriteField(info.PayoutType);
                        writer.WriteField(info.Payout);
                        writer.WriteField(info.PayoutCurrency);
                        
                        writer.NextRecord();
                    }
                }

                // Using Distro file transfer
                //string result = SentFile.SendFileInfo(fileString, fileName);

                //if (!string.IsNullOrWhiteSpace(result))
                //{
                //    return new DataResponse<List<PayOut>> { Error = new Error { Message = result } };
                //}

                return new DataResponse<List<PayOut>> { Data = payout };

            }
            catch (Exception ex)
            {
                return ErrorResponse.CreateErrorResponse<List<PayOut>>(ResponseCode.InternalServerError, ex.Message);
            }
        }
    }
}
