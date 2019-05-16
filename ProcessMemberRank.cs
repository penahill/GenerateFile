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
    public static class ProcessMemberRank
    {
        public static DataResponse<List<MemberRank>> ProcessMemberRankInfo(string lastDateFormatted, string todayDateFormatted, string warehouseID, string fileDate)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Exigo"];
            string filePath = ConfigurationManager.AppSettings["DistroFilePath"];
            string fileName = "MemberRank_" + fileDate + ".csv";

            try
            {
                List<MemberRank> memberRank = new List<MemberRank>();
                List<int> warehouseIdList = new List<int>();
                DataTable table = new DataTable();
                table.Columns.Add("Column1", typeof(string));
                table.Columns.Add("Column2", typeof(string));

                string sql2 = "select WarehouseID " +
                            "    from Warehouses " +
                            "    where warehouseId IN(" + @warehouseID + ")";

                using (var connection = new SqlConnection(connectionString.ConnectionString))
                {
                    connection.Open();
                    warehouseIdList = connection.Query<int>(sql2).ToList();

                    foreach (var warehouseId in warehouseIdList)
                    {
                        // add a single row for each item in the collection.
                        table.Rows.Add(warehouseId, warehouseId);
                    }

                    connection.Close();
                }

                using (var connection = new SqlConnection(connectionString.ConnectionString))
                {
                    const string sql = "dbo.distroMemberRank";
                    connection.Open();

                    memberRank = connection.Query<MemberRank>(
                        sql,
                        new
                        {
                            @lastDate = lastDateFormatted,
                            @todayDate = todayDateFormatted,
                            @warehouseID = table
                        },
                        commandTimeout: 480,
                        commandType: CommandType.StoredProcedure
                        ).ToList();

                    connection.Close();
                }

                string fileString = filePath + fileName;

                using (var textWriter = new StreamWriter(@fileString))
                {
                    var writer = new CsvWriter(textWriter);
                    writer.Configuration.Delimiter = ",";
                    {
                        // Need to write Headers regardless if data was found. 
                        writer.WriteField("MemberID");
                        writer.WriteField("RankDate");
                        writer.WriteField("RankID");
                        writer.NextRecord();
                        
                        if (memberRank.Count > 0)
                        {
                            foreach (var info in memberRank)
                            {
                                writer.WriteField(info.MemberID);
                                writer.WriteField(info.RankDate);
                                writer.WriteField(info.RankID);

                                writer.NextRecord();
                            }
                        }
                    }
                }

                // Using Distro file transfer
                //string result = SentFile.SendFileInfo(fileString, fileName);

                //if (!string.IsNullOrWhiteSpace(result))
                //{
                //    return new DataResponse<List<MemberRank>> { Error = new Error { Message = result } };
                //}

                return new DataResponse<List<MemberRank>> { Data = memberRank };
            }
            catch (Exception ex)
            {
                return ErrorResponse.CreateErrorResponse<List<MemberRank>>(ResponseCode.InternalServerError, ex.Message);
            }
        }
    }
}
