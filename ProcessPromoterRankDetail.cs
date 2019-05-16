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
    public static class ProcessPromoterRankDetail
    {
        public static DataResponse<List<PromoterRankDetail>> ProcessPromoterRankDetailInfo(string lastDateFormatted, string todayDateFormatted, string warehouseID, string fileDate)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Exigo"];
            string filePath = ConfigurationManager.AppSettings["DistroFilePath"];
            string fileName = "PromoterRankDetail_" + fileDate + ".csv";

            try
            {
                List<PromoterRankDetail> promoterRankDetail = new List<PromoterRankDetail>();
                String message = "";
                bool headerWritten = false;

                using (var connection = new SqlConnection(connectionString.ConnectionString))
                {
                    const string sql = "dbo.distroPromoterRankDetail";
                    connection.Open();

                    promoterRankDetail = connection.Query<PromoterRankDetail>(
                        sql,
                        commandTimeout: 480,
                        commandType: CommandType.StoredProcedure
                        ).ToList();

                    connection.Close();
                }

                if (promoterRankDetail.Count == 0)
                {
                    message = "No records found for Promoter Rank Detail.";
                    Logging.InsertErrorLogInfo(0, "Distro", "INFO", message, connectionString);
                    return ErrorResponse.CreateErrorResponse<List<PromoterRankDetail>>(ResponseCode.InternalServerError, message);
                }

               string fileString = filePath + fileName;

                using (var textWriter = new StreamWriter(@fileString))
                {
                    var writer = new CsvWriter(textWriter);
                    writer.Configuration.Delimiter = ",";

                    foreach (var info in promoterRankDetail)
                    {
                        if (headerWritten == false)
                        {
                            writer.WriteField("PromoterRankID");
                            writer.WriteField("PromoterRankShort");
                            writer.WriteField("PromoterRankName");
                            writer.WriteField("PromoterRankOrder");

                            writer.NextRecord();
                            headerWritten = true;
                        }

                        writer.WriteField(info.PromoterRankID);
                        writer.WriteField(info.PromoterRankShort);
                        writer.WriteField(info.PromoterRankName);
                        writer.WriteField(info.PromoterRankOrder);

                        writer.NextRecord();
                    }
                }

                // Using Distro file transfer
                //string result = SentFile.SendFileInfo(fileString, fileName);

                //if (!string.IsNullOrWhiteSpace(result))
                //{
                //    return new DataResponse<List<PromoterRankDetail>> { Error = new Error { Message = result } };
                //}

                return new DataResponse<List<PromoterRankDetail>> { Data = promoterRankDetail };

            }
            catch (Exception ex)
            {
                return ErrorResponse.CreateErrorResponse<List<PromoterRankDetail>>(ResponseCode.InternalServerError, ex.Message);
            }
        }
    }
}
