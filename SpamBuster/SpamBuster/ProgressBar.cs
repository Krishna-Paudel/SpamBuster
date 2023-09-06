using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpamBuster
{
    public class ProgressForm : Form, IDisposable
    {
       
        private static HashSet<ProgressForm> instances = new HashSet<ProgressForm>();
        private ProgressBar progressBar;
        private Label progressLabel;
        private int itemCount;
        private bool _canceled = false;
        private Button cancelButton;

        public ProgressForm(int itemCount, string folderName)
        {
            instances.Add(this);


            // Set the static variable to true to indicate that a progress bar is being displayed


            this.itemCount = itemCount;

            // Initialize the form            
            this.Text = "Processing Mail Items in: " + folderName;
            this.ClientSize = new Size(400, 130);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.ControlBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Add the progress bar
            progressBar = new ProgressBar();
            progressBar.Maximum = itemCount;
            progressBar.Minimum = 0;
            progressBar.Value = 0;
            progressBar.Step = 1;
            progressBar.Dock = DockStyle.Top;
            this.Controls.Add(progressBar);

            // Add the progress label
            progressLabel = new Label();
            progressLabel.Text = "Processing item 1 of " + itemCount.ToString();
            progressLabel.Dock = DockStyle.Top;
            progressLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(progressLabel);

            // Add the cancel button
            cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Size = new Size(80, 30);
            cancelButton.Location = new Point(160, 70);
            cancelButton.Click += new EventHandler(CancelButton_Click);
            this.Controls.Add(cancelButton);
        }

        public static bool IsAnyInstanceAlive()
        {
            return instances.Any(instance => instance != null);
        }

        public new void Dispose()
        {
            instances.Remove(this);
            // Dispose any other resources used by the form
            // ...
        }

        // Update the progress bar and label with the current item number
        public int UpdateProgress(int currentItem)
        {

                progressBar.PerformStep();
                progressLabel.Text = "Processing item " + currentItem.ToString() + " of " + itemCount.ToString();
                Application.DoEvents();
                if (_canceled)
                {
                    return 1;
                }
            return 0;

        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            this.Dispose();
        }

        // Event handler for the cancel button
        private void CancelButton_Click(object sender, EventArgs e)
        {
            _canceled = true;
            //cancelButton.Enabled = false;
            if (cancelButton.InvokeRequired)
            {
                cancelButton.Invoke(new Action(() => cancelButton.Enabled = false));
            }
            else
            {
                cancelButton.Enabled = false;
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ProgressForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "ProgressForm";
            this.ResumeLayout(false);

        }
    }
    public class OperationCancelledException : Exception
    {
        public ProgressForm ProgressFormObj { get; private set; }
        public OperationCancelledException() : base("The operation has been cancelled.") { }
        public OperationCancelledException(ProgressForm form) : base("The operation has been cancelled.") {
            ProgressFormObj = form;
        }
    }

}
