using HtmlAgilityPack;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Summary
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openfiledialog = new OpenFileDialog();
            if (openfiledialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = openfiledialog.FileName;
                textBox1.Text += path;
            }
        }

        public string TextBox1Text
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string text;
            string word = textBox2.Text;
            WebClient web = new WebClient();
            HtmlAgilityPack.HtmlDocument Htmldoc = new HtmlAgilityPack.HtmlDocument();
            Process.Start("https://en.wikipedia.org/wiki/" + word);
            byte[] byteArray = web.DownloadData(new Uri("https://en.wikipedia.org/wiki/" + word));
            Stream stream = new MemoryStream(byteArray);
            Htmldoc.Load(stream);
            FileStream fs = new FileStream("D:\\htmltext.pdf", FileMode.Create, FileAccess.Write, FileShare.None);
            Document pdfDoc = new Document();
            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, fs);
            pdfDoc.Open();
            foreach (HtmlNode node in Htmldoc.DocumentNode.SelectNodes("//text()"))
            {
                text = node.InnerText.Trim();
                pdfDoc.Add(new Paragraph(text));
            }
            pdfDoc.Close();

            if(textBox2.Text != null)
            {
                int sentCount = 1;
                int.TryParse(textBox3.Text, out sentCount);
                SummarizerArguments sumargs = new SummarizerArguments
                {
                    DictionaryLanguage = "en",
                    DisplayLines = sentCount,
                    DisplayPercent = 0,
                    InputFile = @"D:\\htmltext.pdf",
                };
                SummarizedDocument doc = Summarizer.Summarize(sumargs);
                string summary = string.Join("\r\n\r\n", doc.Sentences.ToArray());
                richTextBox1.Text = summary;
            }
            else
            {
                richTextBox1.Text = "Please give the query value!!!";
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(textBox1.Text != null)
            {
                int sentCount = 1;
                int.TryParse(SentenceCountTextBox.Text, out sentCount);
                SummarizerArguments sumargs = new SummarizerArguments
                {
                    DictionaryLanguage = "en",
                    DisplayLines = sentCount,
                    DisplayPercent = 0,
                    InputFile = textBox1.Text
                };
                SummarizedDocument doc = Summarizer.Summarize(sumargs);
                string summary = string.Join("\r\n\r\n", doc.Sentences.ToArray());
                richTextBox1.Text = summary;
            }
            else
            {
                richTextBox1.Text = "Sorry there is no file!!!";
            }
        }
    }
}
