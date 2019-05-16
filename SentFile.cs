using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Modere.Services.Utilities;

namespace Distro
{
    public static class SentFile
    {
        // Keeping code in case we use Azcopy. Currently Distro wants us to use their utility located 
        // in the C:\Program Files\Modere\Distro\Distro Scheduler directory on ExigoProd1. 

        public static string SendFileInfo(string fileToSend, string fileName)
        {
            // Sends file to Azure

            var connectionString = ConfigurationManager.ConnectionStrings["Exigo"];
            string filePath = ConfigurationManager.AppSettings["DistroFilePath"];
            string fileDonePath = ConfigurationManager.AppSettings["DistroFileDonePath"];

            Process azCopycmd = new Process();
            DataReceivedEventHandler errorDataReceived = null;   // Error Data
            DataReceivedEventHandler outputDataReceived = null;  // OutPut Data

            string errorLine = null;
            
            string source = ConfigurationManager.AppSettings["AzCopySource"].ToString();
            string dest = ConfigurationManager.AppSettings["AzCopyDest"].ToString();
            string destKey = ConfigurationManager.AppSettings["AzcopyKey"].ToString();
            string path = ConfigurationManager.AppSettings["AzcopyPath"].ToString();
            string azCopyCommand = "AzCopy /Source:" + "'" + source + "'" + " /Dest:" + "'" + dest + "'" + " /DestKey:" + "'" + destKey + "'" + " /Pattern:" + "'" + fileToSend + "'" + " /Y";

            try
            {
                var startInfo = new ProcessStartInfo()
                {
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    FileName = path,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Arguments = azCopyCommand
                };

                azCopycmd.OutputDataReceived += outputDataReceived;
                azCopycmd.ErrorDataReceived += errorDataReceived;
                azCopycmd.StartInfo = startInfo;
                azCopycmd.Start();
                //azCopycmd.BeginOutputReadLine();
                //azCopycmd.BeginErrorReadLine();

                errorLine = azCopycmd.StandardError.ReadToEnd();

                azCopycmd.StandardInput.Flush();
                azCopycmd.StandardInput.Close();
                azCopycmd.WaitForExit();
                azCopycmd.Close();

                if (!string.IsNullOrWhiteSpace(errorLine))
                {
                    string errorMessage = "SendFile Error: " + errorLine;
                    return errorMessage;
                }

                // Move file once it is sent
                string sourceFile = System.IO.Path.Combine(filePath, fileName);
                string destFile = System.IO.Path.Combine(fileDonePath, fileName);

                if (System.IO.Directory.Exists(filePath))
                {
                    if (System.IO.Directory.Exists(fileDonePath))
                    {
                        // Need to delete file because move command can not replace the file.
                        if (System.IO.File.Exists(destFile))
                        {
                            System.IO.File.Delete(destFile);
                        }

                        System.IO.File.Move(sourceFile, destFile);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "SendFile Exception Error: " + ex.Message;
                return errorMessage;
            }

            return errorLine;
        }
    }
}
