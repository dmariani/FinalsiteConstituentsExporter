using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            return true;
        }
    }
}
