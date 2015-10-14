using System;
using System.DirectoryServices;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;

namespace TMCafe.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult PlaceOrder(string bread, string fillings, string extras, string total)
        {
            const string smtpServer = "192.168.101.186";
            const string smtpPort = "26";
            const string emailTo = "Kevin.Dix@tmgroup.co.uk";
            var userName = User.Identity.Name.Split('\\')[1];

            var entry = new DirectoryEntry("LDAP://TERAMEDIA");
            var searcher = new DirectorySearcher(entry) { Filter = "(&(objectClass=user)(SAMAccountname=" + userName + "))" };

            var searchResult = searcher.FindOne();

            var emailFrom = searchResult.Properties["mail"][0].ToString();
            var displayName = searchResult.Properties["displayname"][0].ToString();
            
            if (string.IsNullOrWhiteSpace(smtpServer) || string.IsNullOrWhiteSpace(emailFrom) || string.IsNullOrWhiteSpace(emailTo))
            {
                return Json(new { success = false });
            }

            var mail = new MailMessage(emailFrom, emailTo);
            using (var client = new SmtpClient(smtpServer))
            {
                client.Port = int.Parse(smtpPort);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                mail.Subject = string.Format("Order from {0}", displayName);

                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Hi Kevin,");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("I would like to order the following:");
                stringBuilder.AppendLine(string.Format("{0}: {1}", "Bread", bread));
                stringBuilder.AppendLine(string.Format("{0}: {1}", "Fillings", string.IsNullOrWhiteSpace(fillings) ? "None" : fillings));
                stringBuilder.AppendLine(string.Format("{0}: {1}", "Extras", string.IsNullOrWhiteSpace(extras) ? "None" : extras));
                stringBuilder.AppendLine(string.Format("Total: {0:C}", Convert.ToDecimal(total)));
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("Thanks");
                stringBuilder.AppendLine(displayName);
                
                mail.Body = stringBuilder.ToString();
                client.Send(mail);
            }

            return Json(new { success = true });
        }
    }
}
