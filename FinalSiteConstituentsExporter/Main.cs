﻿using System;
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
            LoadOptions();
        }

        private void LoadOptions()
        {
            Properties.Settings.Default.Reload(); // Loads settings in application configuration file

            dateTimePicker1.Value = DateTime.Parse(Properties.Settings.Default["startDate"].ToString());
            textBoxDir.Text = Properties.Settings.Default["outputDir"].ToString();
            textBoxUsername.Text = Properties.Settings.Default["userName"].ToString();
            textBoxPassword.Text = Properties.Settings.Default["userPassword"].ToString();
            textBoxApiKey.Text = Properties.Settings.Default["apiKey"].ToString();
        }

        private void SaveOptions()
        {
            DateTime result = dateTimePicker1.Value;
            String dateText = result.ToString("MM/dd/yyyy");

            Properties.Settings.Default["startDate"] = dateText;
            Properties.Settings.Default["outputDir"] = textBoxDir.Text;
            Properties.Settings.Default["userName"] = textBoxUsername.Text;
            Properties.Settings.Default["userPassword"] = textBoxPassword.Text;
            Properties.Settings.Default["apiKey"] = textBoxApiKey.Text;
            Properties.Settings.Default.Save(); // Saves settings in application configuration file
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
                String username = textBoxUsername.Text;
                String password = textBoxPassword.Text;
                String apiKey = textBoxApiKey.Text;

                try
                {
                    Cursor.Current = Cursors.WaitCursor;

                    toolStripStatusLabel.Text = "Logging in...";
                    statusStrip.Refresh();

                    String token = GetAuthToken(username, password, apiKey);

                    toolStripStatusLabel.Text = "Fetching data from Finalsite...";
                    statusStrip.Refresh();

                    // Read data
                    DateTime result = dateTimePicker1.Value;
                    String dateText = result.ToString("yyyy-MM-dd");

                    long rowsExported = GetConstituentsData(token, dateText);

                    toolStripStatusLabel.Text = "Writing data to files...";
                    statusStrip.Refresh();

                    // Write data

                    toolStripStatusLabel.Text = "Export succeeded.";
                    statusStrip.Refresh();

                    MessageBox.Show("Success!  Exported " + rowsExported.ToString() + " rows starting at: " + this.Text + " and wrote files to: " + textBoxDir.Text, "Export Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    toolStripStatusLabel.Text = "Export failed.";
                    statusStrip.Refresh();

                    MessageBox.Show("Error: " + ex.Message, "Export Results", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                Cursor.Current = Cursors.Default;
            }
        }

        private void close_Click(object sender, EventArgs e)
        {
            SaveOptions();
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
        
        private String GetAuthToken(String Username, String Password, String apiKey)
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
                    XmlDocument doc = new XmlDocument();
                    doc.Load(response.GetResponseStream());

                    XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
                    ns.AddNamespace("apidata", "http://www.finalsite.com/apidata/v1.0");

                    XmlNode nodeSuccess = doc.SelectSingleNode("//ResultSet/OperationSucceeded", ns);
                    if (nodeSuccess.InnerText != "True")
                    {
                        XmlNode nodeResultError = doc.SelectSingleNode("//ResultSet/OperationResult", ns);
                        throw new Exception("Error logging into API with user=" + Username + ". Error=" + nodeResultError.InnerText);
                    }

                    XmlNode nodeResultSuccess = doc.SelectSingleNode("//ResultSet/OperationResult", ns);
                    Token = nodeResultSuccess.InnerText;
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

        private long GetConstituentsData(String token, String dateText)
        {
            long rowsExported = 0;

            try
            {
                // We use the HttpUtility class from the System.Web namespace  
                String uri = "https://www.ola.community/api/constituents/constituents.cfm";
                uri += "?ModifiedDate=" + dateText;

                Uri address = new Uri(uri);

                // Create the web request  
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;

                // Set type to POST  
                request.Headers["Authorization"] = Convert.ToBase64String(Encoding.Default.GetBytes("apitoken:" + token));

                // Get response  
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(response.GetResponseStream());

                    XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
                    ns.AddNamespace("apidata", "http://www.finalsite.com/apidata/v1.0");

                    XmlNode nodeSuccess = doc.SelectSingleNode("//ResultSet/OperationSucceeded", ns);
                    if (nodeSuccess.InnerText != "True")
                    {
                        XmlNode nodeResultError = doc.SelectSingleNode("//ResultSet/OperationResult", ns);
                        throw new Exception("Error reading data from Finalsite. Error=" + nodeResultError.InnerText);
                    }

                    XmlNode nodeResultSuccess = doc.SelectSingleNode("//ResultSet/OperationKey", ns);
                    rowsExported = Convert.ToInt64(nodeResultSuccess.InnerText);
                }

                return rowsExported;
            }
            catch (WebException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
