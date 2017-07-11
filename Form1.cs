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
            string web = "http://berrygirls.tumblr.com/archive";
            string allstr = string.Empty;
            Parser media = new Parser();
            List<string> allpage = media.GetAllPageFromStartPoint(web);


            string link = string.Empty;
            foreach ( string a in allpage)
            {
                link += string.Format("{0}\r\n", a);
            }
            System.IO.File.WriteAllText(@"C:\Users\dan\Desktop\var.text", link);
            MessageBox.Show(string.Format("thie link size is {0}", allpage.Count));
        }


    }
    }
