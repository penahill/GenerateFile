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
    public static class ProcessMemberProfile
    {
        public static DataResponse<List<MemberProfile>> ProcessMemberProfileInfo(string lastDateFormatted, string todayDateFormatted, string warehouseID, string fileDate)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Exigo"];
            string filePath = ConfigurationManager.AppSettings["DistroFilePath"];
            string fileName = "MemberProfile_" + fileDate + ".csv";

            try
            {
                List<MemberProfile> memberProfiles = new List<MemberProfile>();
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
                    const string sql = "dbo.distroMemberProfile";
                    connection.Open();

                    memberProfiles = connection.Query<MemberProfile>(
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

                if (memberProfiles.Count == 0)
                {
                    message = "No records found for MemberProfile.";
                    Logging.InsertErrorLogInfo(0, "Distro", "INFO", message, connectionString);
                    return ErrorResponse.CreateErrorResponse<List<MemberProfile>>(ResponseCode.InternalServerError, message);
                }

                string fileString = filePath + fileName;

                using (var textWriter = new StreamWriter(@fileString))
                {
                    var writer = new CsvWriter(textWriter);
                    writer.Configuration.Delimiter = ", ";

                    foreach (var info in memberProfiles)
                    {
                        if (string.IsNullOrWhiteSpace(info.City)
                             && string.IsNullOrWhiteSpace(info.Zip)
                             && string.IsNullOrWhiteSpace(info.FirstName)
                             && string.IsNullOrWhiteSpace(info.LastName)
                             && string.IsNullOrWhiteSpace(info.PreferredLanguage)
                           )
                        {
                            // Need to skip these records
                            continue;
                        }

                        if (headerWritten == false)
                        {
                            writer.WriteField("MemberId");
                            writer.WriteField("JoinDate");
                            writer.WriteField("MemberType");
                            writer.WriteField("CountryCode");
                            writer.WriteField("State");
                            writer.WriteField("Zip");
                            writer.WriteField("City");
                            writer.WriteField("FirstName");
                            writer.WriteField("LastName");
                            writer.WriteField("PhoneNumber");
                            writer.WriteField("PhoneNumberFlag");
                            writer.WriteField("Age");
                            writer.WriteField("Gender");
                            writer.WriteField("EducationLevel");
                            writer.WriteField("MaritalStatus");
                            writer.WriteField("NumberOfResidents");
                            writer.WriteField("HouseType");
                            writer.WriteField("Occupation");
                            writer.WriteField("HouseholdIncomeLevel");
                            writer.WriteField("Religion");
                            writer.WriteField("PreferredLanguage");
                            writer.WriteField("PreferredChannelOfcontact");
                            writer.WriteField("MemberStatus");

                            writer.NextRecord();
                            headerWritten = true;
                        }

                        writer.WriteField(info.MemberId);
                        writer.WriteField(info.JoinDate);
                        writer.WriteField(info.MemberType);
                        writer.WriteField(info.CountryCode);
                        writer.WriteField(info.State);
                        writer.WriteField(info.Zip);
                        writer.WriteField(info.City);
                        writer.WriteField(info.FirstName);
                        writer.WriteField(info.LastName);
                        writer.WriteField(info.PhoneNumber);
                        writer.WriteField(info.PhoneNumberFlag);
                        writer.WriteField(info.Age);
                        writer.WriteField(info.Gender);
                        writer.WriteField(info.EducationLevel);
                        writer.WriteField(info.MaritalStatus);
                        writer.WriteField(info.NumberOfResidents);
                        writer.WriteField(info.HouseType);
                        writer.WriteField(info.Occupation);
                        writer.WriteField(info.HouseholdIncomeLevel);
                        writer.WriteField(info.Religion);
                        writer.WriteField(info.PreferredLanguage);
                        writer.WriteField(info.PreferredChannelOfcontact);
                        writer.WriteField(info.MemberStatus);

                        writer.NextRecord();
                    }
                }

                // Using Distro file transfer
                //string result = SentFile.SendFileInfo(fileString, fileName);

                //if (!string.IsNullOrWhiteSpace(result))
                //{
                //    return new DataResponse<List<MemberProfile>> { Error = new Error { Message = result } };
                //}

                return new DataResponse<List<MemberProfile>> { Data = memberProfiles };

            }
            catch (Exception ex)
            {
                return ErrorResponse.CreateErrorResponse<List<MemberProfile>>(ResponseCode.InternalServerError, ex.Message);
            }
        }
    }
}
