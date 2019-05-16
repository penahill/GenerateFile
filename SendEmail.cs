using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distro
{
    public static class SendEmail
    {
        public static void SendErrorEmail(string errorMessage, string programName)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Exigo"];
            string emails = ConfigurationManager.AppSettings["Emailaddresses"];

            emailspace.email em = new emailspace.email();
            List<string> mess = new List<string>();
            mess.Add("Process Distro: " + programName + " has thrown an exception");
            mess.Add(errorMessage);
            em.SendMail("Distro has failed - " + Environment.MachineName, mess, "BatchError@modere.com", emails);
            
            return;
        }
    }
}
