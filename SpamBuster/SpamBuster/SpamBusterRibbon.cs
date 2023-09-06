using Microsoft.Office.Tools.Ribbon;
using System;

using System.Text.RegularExpressions;
using System.Windows.Forms;

using Outlook = Microsoft.Office.Interop.Outlook;

using System.IO;
using System.Threading.Tasks;
using log4net;


namespace SpamBuster
{
    public partial class SpamBusterRibbon
    {
        
        ProgressForm progressForm;
        private static readonly ILog log = LogManager.GetLogger(typeof(SpamBusterRibbon));

        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {
            
        }

        private void button1_Click(object sender, RibbonControlEventArgs e)
        {
            
            log.Info("Spam Buster Scan All Button has been clicked");
            Outlook.Application outlookApp = Globals.ThisAddIn.Application;
            //Outlook.MAPIFolder spamFolder = outlookApp.Session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderJunk);

            if (ProgressForm.IsAnyInstanceAlive() == true)
            {
                MessageBox.Show("Another Operation is in progres.");
                return;
            }

            Outlook.MAPIFolder selectedFolder = outlookApp.Session.PickFolder();

            Outlook.NameSpace ns = outlookApp.Session;

            // Get the store with the specified name
            Outlook.Store newDefaultStore = null;
            foreach (Outlook.Store store in ns.Stores)
            {
                if (store.DisplayName == selectedFolder.Store.DisplayName)
                {
                    newDefaultStore = store;
                    break;
                }
            }

            Outlook.MAPIFolder spamFolder = newDefaultStore.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderJunk);
            //Outlook.MAPIFolder spamFolder = outlookApp.Session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderJunk);

            if (selectedFolder != null)
            {
                // The user has selected a folder
                // Do something with the selected folder, such as displaying its name
                //MessageBox.Show($"You have selected the folder {selectedFolder.Name}");
                IterateFolders(selectedFolder as Outlook.Folder, spamFolder);
                log.Info("The following folder has been selected for scanning: " + spamFolder);
            }
            else
            {
                // The user has canceled the folder picker dialog
                MessageBox.Show("You have canceled the operation!!!");
                log.Warn("Operation has been cancelled without selecting the folder to scan");
            }


             
             


        }

        private async void IterateFolders(Outlook.Folder folder, Outlook.MAPIFolder spamFolder)
        {
            try
            {
                log.Info("Started iterating through the subfolders and mail items in the folder: " + folder);
                Outlook.Application outlookApp = Globals.ThisAddIn.Application;
                
                // Do something with the current folder, such as displaying its name
                //MessageBox.Show($"Folder: {folder.Name}");

                int itemCount = folder.Items.Restrict("[MessageClass]='IPM.Note'").Count;
                int i = 1;
                // Create and display a modal dialog box to show progress
                
                progressForm = new ProgressForm(itemCount, folder.Name);

                progressForm.Show();

                foreach (Outlook.MailItem mailItem in folder.Items.Restrict("[MessageClass]='IPM.Note'"))
                {
                    log.Info("Started processing the email: " + mailItem.EntryID);

                    // Do something with the email item, such as displaying its subject
                    int output = await SpamBusterModel.getPrediction(mailItem);

                    //int output = result.Result;

                    if (output < 0)
                    {
                        throw new System.Exception("Error running the ML Model. Check logs.");
                    }

                    if (output == 1)
                    {
                        mailItem.Move(spamFolder);                        
                        log.Info("The mail has been identified as spam and moved to Spam Folder.");                        
                    }
                    else
                    {
                        log.Info("The mail is identified as Ham.");
                    }


                    int isCancelled = 0;

                    progressForm.Invoke(new System.Action(() => { 
                        isCancelled = progressForm.UpdateProgress(i++); 
                    }));

                    if (isCancelled == 1)
                    {
                        log.Warn("The operation in progress has been cancelled by the User.");
                        throw new System.Exception("Operation is cancelled!!!");
                        //progressForm.Invoke(new System.Action(() => progressForm.Close()));
                        //progressForm.Dispose();
                        //break;
                        
                    }
                    // Write a log message to the file
                    


                    await Task.Delay(TimeSpan.FromTicks(1000));


                }

                progressForm.Invoke(new System.Action(() => progressForm.Close()));
                progressForm.Dispose();
                // Iterate through all subfolders
                if (folder.Folders.Count > 0)
                {
                    foreach (Outlook.Folder subfolder in folder.Folders)
                    {

                        IterateFolders(subfolder, spamFolder); // Call the function recursively for each subfolder
                    }
                }
            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message);
                MessageBox.Show(ex.Message);
                progressForm.Invoke(new System.Action(() => progressForm.Close()));
                progressForm.Dispose();
            }

        }
        public static string getEmailHeaders(string transportHeaders, string flagPattern)
        {
           
            string flagvalue = "unknown";

            Match match = Regex.Match(transportHeaders, flagPattern, RegexOptions.IgnoreCase);

            // if a match is found, extract the SPF value
            if (match.Success)
            {
                flagvalue = match.Groups[1].Value;
            }

            return flagvalue;
        }

    }
}
