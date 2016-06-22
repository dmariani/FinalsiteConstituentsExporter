using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
        Boolean bExportWasRunSuccessfully = false;

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
            // Only save this if a successful export was run
            if (bExportWasRunSuccessfully)
            {
                DateTime resultLast = DateTime.Today.AddDays(-1);
                String dateTextLast = resultLast.ToString("MM/dd/yyyy");
                Properties.Settings.Default["startDateLast"] = dateTextLast;

                DateTime result = dateTimePicker1.Value;
                String dateText = result.ToString("MM/dd/yyyy");
                Properties.Settings.Default["startDate"] = dateText;
            }

            Properties.Settings.Default["outputDir"] = textBoxDir.Text;
            Properties.Settings.Default["userName"] = textBoxUsername.Text;
            Properties.Settings.Default["userPassword"] = textBoxPassword.Text;
            Properties.Settings.Default["apiKey"] = textBoxApiKey.Text;
            Properties.Settings.Default.Save(); // Saves settings in application configuration file
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
        }

        private void go_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to begin the export starting at: " + this.Text, "Confirm Export", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                String username = textBoxUsername.Text;
                String password = textBoxPassword.Text;
                String apiKey = textBoxApiKey.Text;
                String outputDir = textBoxDir.Text;

                try
                {
                    Cursor.Current = Cursors.WaitCursor;

                    // Remove the families file
                    System.IO.File.Delete(outputDir + "//" + "families.csv");
                    System.IO.File.Delete(outputDir + "//" + "members.csv");

                    toolStripStatusLabel.Text = "Logging in...";
                    statusStrip.Refresh();

                    String token = GetAuthToken(username, password, apiKey);

                    toolStripStatusLabel.Text = "Requesting data from Finalsite...";
                    statusStrip.Refresh();

                    // Read data
                    DateTime result = dateTimePicker1.Value;
                    String dateText = result.ToString("yyyy-MM-dd");

                    long rowsExported = GetConstituentsData(token, dateText, outputDir);

                    toolStripStatusLabel.Text = "Export succeeded.";
                    statusStrip.Refresh();

                    bExportWasRunSuccessfully = true;

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

        private void buttonLastRunDate_Click(object sender, EventArgs e)
        {
            dateTimePicker1.Value = DateTime.Parse(Properties.Settings.Default["startDateLast"].ToString());
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

                StringBuilder data = new StringBuilder();
                data.Append("ApiEncryptedKey=" + HttpUtility.UrlEncode(apiKey));

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

        private long GetConstituentsData(String token, String dateText, String outputDir)
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
                request.Timeout = Timeout.Infinite;
                request.ReadWriteTimeout = Timeout.Infinite;
                request.ServicePoint.MaxIdleTime = Timeout.Infinite;
                request.ServicePoint.ConnectionLeaseTimeout = -1;

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

                    toolStripStatusLabel.Text = "Writing data to files...";
                    statusStrip.Refresh();

                    string[,] fields = new string[13, 2] { 
                    { "ImportID", "MemberID" },
                    { "BusRoute", "FamilyID" },
                    { "FirstName", "FirstName" }, 
                    { "MiddleName", "MiddleName" },
                    { "LastName", "LastName" },
                    { "NameSuffix", "NameSuffix" },
                    { "Gender", "Gender" },
                    { "Custom1", "EnvelopeID" },
                    { "Custom3", "Relationship" },
                    { "Custom4", "ActiveFlag" },
                    { "DateEnrolled", "DateEnrolled" },
                    { "BirthDate", "BirthDate" },
                    { "MaritalStatus", "MaritalStatus" },
                    };

                    string[,] fieldsAddresses = new string[5, 2] { 
                    { "Address1", "Address1" }, // Addresses/Address/AddressType: Home
                    { "Address2", "Address2" },
                    { "City", "City" },
                    { "State", "State" },
                    { "Zip", "Zip" },
                    };

                    string[,] fieldsPhones = new string[3, 2] { 
                    { "PhoneNumber", "PhoneNumber-Home" }, // Phones/Phone/PhoneType: Home, Mobile, School
                    { "PhoneNumber", "PhoneNumber-Mobile" }, 
                    { "PhoneNumber", "PhoneNumber-School" },
                    };

                    string[,] fieldsEmails = new string[4, 2] { 
                    { "Email", "Email-Primary" }, // Emails/Email/EmailType: Home, Work, School
                    { "Email", "Email-Home" },
                    { "Email", "Email-Work" }, 
                    { "Email", "Email-School" },
                    };

                    StringBuilder bodyFamilies = new StringBuilder();
                    StringBuilder bodyMembers = new StringBuilder();

                    StringBuilder header = new StringBuilder();
                    
                    // Write headers for main info
                    for (int i = 0; i < fields.GetLength(0); i++)
                    {
                        if (header.Length > 0)
                            header.Append(",");

                        header.Append(fields[i, 1]);
                    }

                    // Write headers for addresses
                    for (int i = 0; i < fieldsAddresses.GetLength(0); i++)
                    {
                        if (header.Length > 0)
                            header.Append(",");

                        header.Append(fieldsAddresses[i, 1]);
                    }

                    // Write headers for phones
                    for (int i = 0; i < fieldsPhones.GetLength(0); i++)
                    {
                        if (header.Length > 0)
                            header.Append(",");

                        header.Append(fieldsPhones[i, 1]);
                    }

                    // Write headers for emails
                    for (int i = 0; i < fieldsEmails.GetLength(0); i++)
                    {
                        if (header.Length > 0)
                            header.Append(",");

                        header.Append(fieldsEmails[i, 1]);
                    }

                    header.AppendLine();
                    bodyFamilies.Append(header);
                    bodyMembers.Append(header);

                    // Write the body records
                    XmlNodeList constituents = doc.SelectNodes("//ResultSet/Constituents/Constituent", ns);
                    foreach (XmlNode constituent in constituents)
                    {
                        Boolean bIsHeadofHousehold = false;
                        StringBuilder body = new StringBuilder();
                        String strFamilyID = "";

                        for (int i = 0; i < fields.GetLength(0); i++)
                        {
                            if (body.Length > 0)
                                body.Append(",");

                            String value = "";

                            XmlNode node = constituent[fields[i, 0]];
                            if (node == null)
                            {
                                if (fields[i, 1] == "EnvelopeID")
                                {
                                    // If the envelopeid is empty, fill it with the
                                    // the FamilyID + 1,000,000 if FamilyID 
                                    if (strFamilyID.Length >= 6)
                                        value = strFamilyID;
                                    else
                                        value = "100000" + strFamilyID;
                                }
                                else
                                    continue;
                            }
                            else
                                value = node.InnerText.Replace(",", "");

                            Console.WriteLine(value);

                            // Perform transformations
                            if (fields[i,1] == "Relationship" && value == "Primary Contact")
                            {
                                bIsHeadofHousehold = true;
                                value = "Head of Household";
                            }

                            if (fields[i, 1] == "MemberID")
                            {
                                value = value.Replace("fs_", "");
                            }

                            if (fields[i, 1] == "ActiveFlag")
                            {
                                // Mark this with a special token so we can replace later
                                if (value == "Active")
                                    value = "True";
                                else
                                    value = "False";
                            }

                            // Save FamilyID for later
                            if (fields[i, 1] == "FamilyID")
                            {
                                strFamilyID = value;
                            }

                            body.Append(value);
                        }

                        // Handle Addresses
                        XmlElement addressesElement = constituent["Addresses"];
                        if (addressesElement == null)
                        {
                            // Need to make sure we have placeholders if their is no address
                            for (int i = 0; i < fieldsAddresses.GetLength(0); i++)
                            {
                                if (body.Length > 0)
                                    body.Append(",");
                            }
                        }
                        else
                        {
                            XmlNodeList addresses = addressesElement.ChildNodes;
                            if (addresses != null)
                            {
                                foreach (XmlNode addressNode in addresses)
                                {
                                    String addressTypeStr = "";

                                    XmlNode addressType = addressNode["AddressType"];
                                    if (addressType != null)
                                        addressTypeStr = addressType.InnerText;

                                    // Only pull home addresses
                                    if (addressTypeStr != "Home")
                                        continue;

                                    for (int i = 0; i < fieldsAddresses.GetLength(0); i++)
                                    {
                                        if (body.Length > 0)
                                            body.Append(",");

                                        XmlNode nodeA = addressNode[fieldsAddresses[i, 0]];
                                        if (nodeA == null)
                                            continue;

                                        String value = nodeA.InnerText.Replace(",","");

                                        body.Append(value);
                                    }
                                }
                            }
                        }

                        // Handle Phones
                        Dictionary<String, String> dictionaryPhones = new Dictionary<String, String>();

                        XmlElement phonesElement = constituent["Phones"];
                        if (phonesElement != null)
                        {
                            XmlNodeList phones = phonesElement.ChildNodes;
                            if (phones != null)
                            {
                                foreach (XmlNode phonesNode in phones)
                                {
                                    String phoneTypeStr = "";

                                    XmlNode phoneType = phonesNode["PhoneType"];
                                    if (phoneType != null)
                                        phoneTypeStr = phoneType.InnerText;

                                    XmlNode nodeP = phonesNode["PhoneNumber"];
                                    if (nodeP == null)
                                        continue;

                                    String value = nodeP.InnerText.Replace(",", "");
                                    dictionaryPhones.Add(phoneTypeStr, value);
                                }
                            }
                        }

                        // Now lookup in Home, Mobile, School order
                        body.Append(",");
                        if (dictionaryPhones.ContainsKey("Home"))
                        {
                            String value = dictionaryPhones["Home"];
                            body.Append(value);
                        }
                        
                        body.Append(",");
                        if (dictionaryPhones.ContainsKey("Mobile"))
                        {
                            String value = dictionaryPhones["Mobile"];
                            body.Append(value);
                        }

                        body.Append(",");
                        if (dictionaryPhones.ContainsKey("School"))
                        {
                            String value = dictionaryPhones["School"];
                            body.Append(value);
                        }

                        // Handle Emails
                        Dictionary<String, String> dictionaryEmails = new Dictionary<String, String>();

                        XmlElement emailsElement = constituent["Emails"];
                        if (emailsElement != null)
                        {
                            XmlNodeList emails = emailsElement.ChildNodes;
                            if (emails != null)
                            {
                                foreach (XmlNode emailsNode in emails)
                                {
                                    String emailTypeStr = "";

                                    XmlNode emailType = emailsNode["EmailType"];
                                    if (emailType != null)
                                    {
                                        emailTypeStr = emailType.InnerText;

                                        if (emailTypeStr.Length == 0)
                                            emailTypeStr = "Primary";

                                        XmlNode nodeE = emailsNode["EmailAddress"];
                                        if (nodeE == null)
                                            continue;

                                        String value = nodeE.InnerText.Replace(",", "");
                                        dictionaryEmails.Add(emailTypeStr, value);
                                    }
                                }
                            }
                        }

                        // Now lookup in Home, Mobile, School order
                        body.Append(",");
                        if (dictionaryEmails.ContainsKey("Primary"))
                        {
                            String value = dictionaryEmails["Primary"];
                            body.Append(value);
                        }

                        body.Append(",");
                        if (dictionaryEmails.ContainsKey("Home"))
                        {
                            String value = dictionaryEmails["Home"];
                            body.Append(value);
                        }

                        body.Append(",");
                        if (dictionaryEmails.ContainsKey("Work"))
                        {
                            String value = dictionaryEmails["Work"];
                            body.Append(value);
                        }

                        body.Append(",");
                        if (dictionaryEmails.ContainsKey("School"))
                        {
                            String value = dictionaryEmails["School"];
                            body.Append(value);
                        }

                        body.AppendLine();

                        // Only write the head of household to the Families file
                        if (bIsHeadofHousehold)
                        {
                            bodyFamilies.Append(body);
                        }

                        bodyMembers.Append(body);
                    }

                    // Now write the files
                    System.IO.File.WriteAllText(outputDir + "//" + "families.csv", bodyFamilies.ToString());
                    System.IO.File.WriteAllText(outputDir + "//" + "members.csv", bodyMembers.ToString());
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
