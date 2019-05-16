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
    public static class ProcessMemberRankDetail
    {
        public static DataResponse<List<MemberRankDetail>> ProcessMemberRankDetailInfo(string lastDateFormatted, string todayDateFormatted, string warehouseID, string fileDate)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Exigo"];
            string filePath = ConfigurationManager.AppSettings["DistroFilePath"];
            string fileName = "MemberRankDetail_" + fileDate + ".csv";

            try
            {
                List<MemberRankDetail> memberRankDetail = new List<MemberRankDetail>();
                String message = "";
                bool headerWritten = false;
                
                using (var connection = new SqlConnection(connectionString.ConnectionString))
                {
                    const string sql = "dbo.distroMemberRankDetail";
                    connection.Open();

                    memberRankDetail = connection.Query<MemberRankDetail>(
                        sql,
                        commandTimeout: 480,
                        commandType: CommandType.StoredProcedure
                        ).ToList();

                    connection.Close();
                }

                if (memberRankDetail.Count == 0)
                {
                    message = "No records found for Memeber Rank Detail.";
                    Logging.InsertErrorLogInfo(0, "Distro", "INFO", message, connectionString);
                    return ErrorResponse.CreateErrorResponse<List<MemberRankDetail>>(ResponseCode.InternalServerError, message);
                }

                string fileString = filePath + fileName;

                using (var textWriter = new StreamWriter(@fileString))
                {
                    var writer = new CsvWriter(textWriter);
                    writer.Configuration.Delimiter = ",";

                    foreach (var info in memberRankDetail)
                    {
                        if (headerWritten == false)
                        {
                            writer.WriteField("RankID");
                            writer.WriteField("RankShort");
                            writer.WriteField("RankName");
                            writer.WriteField("RankOrder");

                            writer.NextRecord();
                            headerWritten = true;
                        }

                        writer.WriteField(info.RankID);
                        writer.WriteField(info.RankShort);
                        writer.WriteField(info.RankName);
                        writer.WriteField(info.RankOrder);

                        writer.NextRecord();
                    }
                }

                // Using Distro file transfer
                //string result = SentFile.SendFileInfo(fileString, fileName);

                //if (!string.IsNullOrWhiteSpace(result))
                //{
                //    return new DataResponse<List<MemberRankDetail>> { Error = new Error { Message = result } };
                //}

                return new DataResponse<List<MemberRankDetail>> { Data = memberRankDetail };

            }
            catch (Exception ex)
            {
                return ErrorResponse.CreateErrorResponse<List<MemberRankDetail>>(ResponseCode.InternalServerError, ex.Message);
            }
        }
    }
}
