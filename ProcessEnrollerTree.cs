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
    public static class ProcessEnrollerTree
    {
        public static DataResponse<List<EnrollerTree>> ProcessEnrollerTreeInfo(string lastDateFormatted, string todayDateFormatted, string warehouseID, string fileDate)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Exigo"];
            string filePath = ConfigurationManager.AppSettings["DistroFilePath"];
            string fileName = "EnrollerTree_" + fileDate + ".csv";

            try
            {
                List<EnrollerTree> enrollerTree = new List<EnrollerTree>();
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
                    const string sql = "dbo.distroEnrollerTree";
                    connection.Open();

                    enrollerTree = connection.Query<EnrollerTree>(
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

                if (enrollerTree.Count == 0)
                {
                    message = "No records found for Enroller Tree.";
                    Logging.InsertErrorLogInfo(0, "Distro", "INFO", message, connectionString);
                    return ErrorResponse.CreateErrorResponse<List<EnrollerTree>>(ResponseCode.InternalServerError, message);
                }

                //string fileString = @"C:\Program Files\Modere\EnrollerTree_" + fileDate + ".csv";
                string fileString = filePath + fileName;


                using (var textWriter = new StreamWriter(@fileString))
                {
                    var writer = new CsvWriter(textWriter);
                    writer.Configuration.Delimiter = ",";

                    foreach (var info in enrollerTree)
                    {
                        if (headerWritten == false)
                        {
                            writer.WriteField("MemberID");
                            writer.WriteField("EnrollerID");
                            writer.WriteField("AssociatedDate");

                            writer.NextRecord();
                            headerWritten = true;
                        }

                        writer.WriteField(info.MemberID);
                        writer.WriteField(info.EnrollerID);
                        writer.WriteField(info.AssociatedDate);
                       
                        writer.NextRecord();
                    }
                }

                // Using Distro file transfer
                //string result = SentFile.SendFileInfo(fileString, fileName);

                //if (!string.IsNullOrWhiteSpace(result))
                //{
                //    return new DataResponse<List<EnrollerTree>> { Error = new Error { Message = result } };
                //}

                return new DataResponse<List<EnrollerTree>> { Data = enrollerTree };

            }
            catch (Exception ex)
            {
                return ErrorResponse.CreateErrorResponse<List<EnrollerTree>>(ResponseCode.InternalServerError, ex.Message);
            }
        }
    }
}
