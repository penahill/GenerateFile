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
    public static class ProcessSalesDetail
    {
        public static DataResponse<List<SalesDetail>> ProcessSalesDetailInfo(string lastDateFormatted, string todayDateFormatted, string warehouseID, string fileDate)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Exigo"];
            string filePath = ConfigurationManager.AppSettings["DistroFilePath"];
            string fileName = "SalesDetail_" + fileDate + ".csv";

            try
            {
                List<SalesDetail> salesDetail = new List<SalesDetail>();
                String message = "";
                bool headerWritten = false;
                List<int> warehouseIdList = new List<int>();
                DataTable table = new DataTable();
                table.Columns.Add("Column1", typeof(string));
                table.Columns.Add("Column2", typeof(string));

                switch (warehouseID)
                {
                    case "1":
                        // Canada
                        table.Rows.Add("1", "1");
                        table.Rows.Add("15", "15");
                        table.Rows.Add("18", "18");
                        break;
                    case "2":
                        // US
                        table.Rows.Add("2", "2");
                        table.Rows.Add("16", "16");
                        break;
                    case "1,2":
                    case "1,2,18":
                        // North America
                        table.Rows.Add("1", "1");
                        table.Rows.Add("2", "2");
                        table.Rows.Add("15", "15");
                        table.Rows.Add("16", "16");
                        table.Rows.Add("18", "18");
                        break;
                    default:
                        table.Rows.Add(warehouseID, warehouseID);
                        break;
                }

                using (var connection = new SqlConnection(connectionString.ConnectionString))
                {
                    const string sql = "dbo.distroSalesDetail";
                    connection.Open();

                    salesDetail = connection.Query<SalesDetail>(
                        sql,
                        new
                        {
                            @lastDate = lastDateFormatted,
                            @todayDate = todayDateFormatted,
                            @orderWarehouseID = table
                        },
                        commandTimeout: 480,
                        commandType: CommandType.StoredProcedure
                        ).ToList();

                    connection.Close();
                }

                if (salesDetail.Count == 0)
                {
                    message = "No records found for Sales Detail.";
                    Logging.InsertErrorLogInfo(0, "Distro", "INFO", message, connectionString);
                    return ErrorResponse.CreateErrorResponse<List<SalesDetail>>(ResponseCode.InternalServerError, message);
                }

                string fileString = filePath + fileName;
                
                using (var textWriter = new StreamWriter(@fileString))
                {
                    var writer = new CsvWriter(textWriter);
                    writer.Configuration.Delimiter = ",";

                    foreach (var info in salesDetail)
                    {
                        if (headerWritten == false)
                        {
                            writer.WriteField("OrderID");
                            writer.WriteField("ProductItemCode");
                            writer.WriteField("ProductName");
                            writer.WriteField("ProductCategory");
                            writer.WriteField("Brand");
                            writer.WriteField("SalesValue");
                            writer.WriteField("BV");
                            writer.WriteField("QV");
                            writer.WriteField("Qty");

                            writer.NextRecord();
                            headerWritten = true;
                        }

                        writer.WriteField(info.OrderID);
                        writer.WriteField(info.ProductItemCode);
                        writer.WriteField(info.ProductName);
                        writer.WriteField(info.ProductCategory);
                        writer.WriteField(info.Brand);
                        writer.WriteField(info.SalesValue);
                        writer.WriteField(info.BV);
                        writer.WriteField(info.QV);
                        writer.WriteField(info.Qty);

                        writer.NextRecord();
                    }
                }

                // Using Distro file transfer
                //string result = SentFile.SendFileInfo(fileString, fileName);

                //if (!string.IsNullOrWhiteSpace(result))
                //{
                //    return new DataResponse<List<SalesDetail>> { Error = new Error { Message = result } };
                //}

                return new DataResponse<List<SalesDetail>> { Data = salesDetail };

            }
            catch (Exception ex)
            {
                return ErrorResponse.CreateErrorResponse<List<SalesDetail>>(ResponseCode.InternalServerError, ex.Message);
            }
        }
    }
}
