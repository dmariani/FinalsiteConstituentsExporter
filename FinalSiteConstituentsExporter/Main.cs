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
using System.Xml;
using System.Xml.XPath;

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
                String username = "david.mariani1";
                String password = "Bully123";

                try
                {
                    GetAuthToken(username, password);

                    long RowsExported = ProcessExport(this.Text, textBoxDir.Text);

                    MessageBox.Show("Success!  Exported " + RowsExported.ToString() + " rows starting at: " + this.Text + " and wrote files to: " + textBoxDir.Text, "Export Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Export Results", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
        
        private String GetAuthToken(String Username, String Password)
        {
            String Token = "";

            try
            {
              // We use the HttpUtility class from the System.Web namespace  
                Uri address = new Uri("https://www.ola.community/api/apilogin/apilogin.cfm");

                // Create the web request  
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;

                // Set type to POST  
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(Username + ":" + Password));

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
//                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    // Console application output  
//                    Console.WriteLine(reader.ReadToEnd());

                    XmlDocument doc = new XmlDocument();
                    doc.Load(response.GetResponseStream());

                    XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
                    ns.AddNamespace("apilogin", "http://www.finalsite.com/apidata/v1.0");

                    XmlNode nodeSuccess = doc.SelectSingleNode("//ResultSet/OperationSucceeded", ns);
                    if (nodeSuccess.InnerText != "True")
                    {
                        XmlNode nodeResult = doc.SelectSingleNode("//ResultSet/OperationResult", ns);
                        throw new Exception("Error logging into API with user=" + Username + ". Error=" + nodeResult.InnerText);
                    }
                }

                return Token;
            }
            catch (WebException ex)
            {
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Console.WriteLine(reader.ReadToEnd());
                    Exception newex = new Exception(reader.ToString());
                    throw newex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private long ProcessExport(String Datestr, String Dir)
        {
            long RowsExported = 0;

            return RowsExported;
        }
    }
}
