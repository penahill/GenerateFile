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
    public static class ProcessSponsorTree
    {
        public static DataResponse<List<SponsorTree>> ProcessSponsorTreeInfo(string lastDateFormatted, string todayDateFormatted, string warehouseID, string fileDate)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Exigo"];
            string filePath = ConfigurationManager.AppSettings["DistroFilePath"];
            string fileName = "SponsorTree_" + fileDate + ".csv";

            try
            {
                List<SponsorTree> sponsorTree = new List<SponsorTree>();
                String message = "";
                bool headerWritten = false;
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
                    const string sql = "dbo.distroSponsorTree";
                    connection.Open();

                    sponsorTree = connection.Query<SponsorTree>(
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
                
                if (sponsorTree.Count == 0)
                {
                    message = "No records found for Sponsor Tree.";
                    Logging.InsertErrorLogInfo(0, "Distro", "INFO", message, connectionString);
                    return ErrorResponse.CreateErrorResponse<List<SponsorTree>>(ResponseCode.InternalServerError, message);
                }

                //string fileString = @"C:\Program Files\Modere\SponsorTree_" + fileDate + ".csv";
                string fileString = filePath + fileName;


                using (var textWriter = new StreamWriter(@fileString))
                {
                    var writer = new CsvWriter(textWriter);
                    writer.Configuration.Delimiter = ",";

                    foreach (var info in sponsorTree)
                    {
                        if (headerWritten == false)
                        {
                            writer.WriteField("MemberID");
                            writer.WriteField("SponsorID");
                            writer.WriteField("AssociatedDate");

                            writer.NextRecord();
                            headerWritten = true;
                        }

                        writer.WriteField(info.MemberID);
                        writer.WriteField(info.SponsorID);
                        writer.WriteField(info.AssociatedDate);

                        writer.NextRecord();
                    }
                }

                // Using Distro file transfer
                //string result = SentFile.SendFileInfo(fileString, fileName);

                //if (!string.IsNullOrWhiteSpace(result))
                //{
                //    return new DataResponse<List<SponsorTree>> { Error = new Error { Message = result } };
                //}

                return new DataResponse<List<SponsorTree>> { Data = sponsorTree };

            }
            catch (Exception ex)
            {
                return ErrorResponse.CreateErrorResponse<List<SponsorTree>>(ResponseCode.InternalServerError, ex.Message);
            }
        }

    }
}
