using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Threading;

namespace SerialParcer
{
    public partial class Form1 : Form
    {
        ListBox adressList = new ListBox();
        public Form1()
        {
            InitializeComponent();
            Thread parsThread = new Thread(SerialList);
            parsThread.Start();
        }

        public void SerialList()
        {
            string urlSV = "http://seasonvar.ru";
            string htmlSV = string.Empty;
            string patternSerialList = @"href=""(.+?)"" class=""betterT alf-link"">(.+?)</a>";
            HttpWebRequest myRequestSV = (HttpWebRequest)HttpWebRequest.Create(urlSV);
            HttpWebResponse myResponseSV = (HttpWebResponse)myRequestSV.GetResponse();
            StreamReader srSV = new StreamReader(myResponseSV.GetResponseStream());
            while (srSV.Peek() >= 0)
            {
                htmlSV = srSV.ReadLine();
                Regex rgx1 = new Regex(patternSerialList);
                MatchCollection resSV = rgx1.Matches(htmlSV);
                if (resSV.Count > 0)
                {
                    foreach (Match match in resSV)
                    {
                        adressList.Items.Add(match.Groups[1].Value);
                        AddItem(match.Groups[2].Value);
                    }
                }
            }
        }

        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = listBox1.SelectedIndex;
            string serialAdress = adressList.Items[selectedIndex].ToString();
            string urln = "http://seasonvar.ru" + serialAdress;
            linkLabel1.Text = urln;
            string html = string.Empty;
            string pattern = @"<img src=""(.+?)"" alt";
            HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create(urln);
            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
            StreamReader sr = new StreamReader(myResponse.GetResponseStream());
            html = sr.ReadToEnd();
            string desc = getBetween(html, "<p>", "</p>");
            richTextBox1.Text = desc;
            string res;
            res = Regex.Match(html, pattern).Groups[1].ToString();
            
            new WebClient().DownloadFile(res, "DescriptionPicture.jpg");
            byte[] bmpData = new WebClient().DownloadData(res);
            using (MemoryStream ms = new MemoryStream(bmpData))
            {
                Bitmap bmp = (Bitmap)Bitmap.FromStream(ms);
                pictureBox1.Image = bmp;
            }
           
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string findPattern = textBox1.Text;
            int ind = listBox1.FindString(findPattern);
            if (ind > -1)
                listBox1.SelectedIndex = ind;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel1.Text);
        }

        delegate void AddItemCallback(string text);

        private void AddItem(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.listBox1.InvokeRequired)
            {
                AddItemCallback d = new AddItemCallback(AddItem);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.listBox1.Items.Add(text);
            }
        }
    }
}
