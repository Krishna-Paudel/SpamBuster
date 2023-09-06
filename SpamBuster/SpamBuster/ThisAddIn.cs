using Outlook = Microsoft.Office.Interop.Outlook;
using log4net;
using log4net.Config;
using System.IO;

using System.Threading.Tasks;
using System;
using System.Reflection;

namespace SpamBuster
{
    public partial class ThisAddIn
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ThisAddIn));
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string logFolderPath = Path.Combine(folderPath, "SpamBusterLogs");

            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }

            log4net.Config.XmlConfigurator.Configure();
            log.Info("************************************************************************************************");
            log.Info("************************************************************************************************");
            log.Info("Outlook and the SpamBuster Add in has started.");

            this.Application.NewMailEx += new Outlook.ApplicationEvents_11_NewMailExEventHandler(Application_NewMailEx);

            Outlook.NameSpace outlookNS = this.Application.GetNamespace("MAPI");
            Outlook.MAPIFolder rootFolder = outlookNS.Folders[1];
            Outlook.Items mailItems = rootFolder.Items;
            mailItems.Restrict("[Unread]=true");

            foreach (Outlook.MailItem mailItem in mailItems)
            {
                Application_NewMailEx(mailItem.EntryID);
            }

        }

        private void Application_NewMailEx(string entryID)
        {
            Outlook.MailItem mailItem = null;
            try
            {
                mailItem = this.Application.Session.GetItemFromID(entryID) as Outlook.MailItem;
            }
            catch (Exception ex)
            {
                // Handle any errors
                // ...
            }

            if (mailItem != null)
            {
                if (mailItem is Outlook.MailItem)
                {

                    log.Info("A new email has arrived.");
                    log.Info("Started processing the email: " + mailItem.EntryID);

                    Outlook.MAPIFolder spamFolder = mailItem.Session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderJunk);

                    Task<int> result = SpamBusterModel.getPrediction(mailItem);

                    int output = result.Result;

                    if (output < 0)
                    {
                        log.Error("Error running the ML Model. Check SpamBuster function in Google Cloud Functions.");
                        throw new System.Exception("Error running the ML Model. Check logs.");
                    }

                    if (output == 1)
                    {
                        log.Info("The mail has been identified as spam and moved to Spam Folder.");
                        mailItem.Move(spamFolder);
                    }
                    else
                    {
                        log.Info("The mail is identified as Ham.");
                    }

                }
            }
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            // Note: Outlook no longer raises this event. If you have code that 
            //    must run when Outlook shuts down, see https://go.microsoft.com/fwlink/?LinkId=506785
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
