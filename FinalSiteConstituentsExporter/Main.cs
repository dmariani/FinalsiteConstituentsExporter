using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using System.Net.Http;
using System.Net;
using System.IO;

namespace FinalSiteConstituentsExporter
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            // Initialize picker to yesterday.
            DateTime result = DateTime.Today.Subtract(TimeSpan.FromDays(7));
            dateTimePicker1.Value = result;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            // Set title bar to selected date.
            DateTime result = dateTimePicker1.Value;
            this.Text = result.ToString("MM/dd/yyyy");
        }

        private void go_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to begin the export starting at: " + this.Text, "Confirm Export", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                if (ProcessExport(this.Text, textBoxDir.Text) == true)
                {
                    MessageBox.Show("Success!  Exported data starting at: " + this.Text + " and written to: " + textBoxDir.Text, "Export Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Error!  Failed to export data starting at: " + this.Text + " and written to: " + textBoxDir.Text, "Export Results", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (dialogResult == DialogResult.No)
            {
                //do something else
            }
        }

        private void close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void chooseDir_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxDir.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private Boolean ProcessExport(String Datestr, String Dir)
        {
            // We use the HttpUtility class from the System.Web namespace  
            Uri address = new Uri("https://www.ola.community/api/apilogin/apilogin.cfm");

            // Create the web request  
            HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;

            // Set type to POST  
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            string username = "david.mariani";
            string password = "Bully123";

            request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(username + ":" + password));

            // Create the data we want to send  
            string key = "0357D83A88AA320BE9194A00D753B5686CBC9547AFD0B6A7E54F9C84B064FBAD";

            StringBuilder data = new StringBuilder();
            data.Append("ApiEncryptedKey=" + HttpUtility.UrlEncode(key));

            // Create a byte array of the data we want to send  
            byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

            // Set the content length in the request headers  
            request.ContentLength = byteData.Length;

            // Write data  
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(byteData, 0, byteData.Length);
            }

            // Get response  
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                // Get the response stream  
                StreamReader reader = new StreamReader(response.GetResponseStream());

                // Console application output  
                Console.WriteLine(reader.ReadToEnd());
            }  

            return true;
        }
    }
}
