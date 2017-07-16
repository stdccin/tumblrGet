using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using tumblrCrawler.MediaParse;

namespace tumblrCrawler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string web = tbInput.Text;
            string pathstr = @"C:\Users\dan\Desktop\tumblr\";
            Parser media = new Parser();
            List<string> allpage = media.GetAllPageFromStartPoint(web);
            media.ParseArchivePageToLinkList(allpage);
            media.FetchMediaLists();
            media.WritelIstsDataToFile(pathstr);
            media.WritelIstsDataToFileCVS(pathstr);

            MessageBox.Show(string.Format("thie link size is {0}", allpage.Count));
        }


    }
}
