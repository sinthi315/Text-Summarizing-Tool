using HtmlAgilityPack;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
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
    public partial class Form2 : Form
    {
        static string mModelPath = @"C:\Users\SAIMA\Documents\Visual Studio 2015\Projects\Summary\Summary\bin\Models\";
        static OpenNLP.Tools.SentenceDetect.EnglishMaximumEntropySentenceDetector mSentenceDetector = new OpenNLP.Tools.SentenceDetect.EnglishMaximumEntropySentenceDetector(mModelPath + "EnglishSD.nbin");
        static OpenNLP.Tools.Tokenize.EnglishMaximumEntropyTokenizer mTokenizer = new OpenNLP.Tools.Tokenize.EnglishMaximumEntropyTokenizer(mModelPath + "EnglishTok.nbin");
        static OpenNLP.Tools.PosTagger.EnglishMaximumEntropyPosTagger mPosTagger = new OpenNLP.Tools.PosTagger.EnglishMaximumEntropyPosTagger(mModelPath + "EnglishPOS.nbin");
        static OpenNLP.Tools.NameFind.EnglishNameFinder mNameFinder = new OpenNLP.Tools.NameFind.EnglishNameFinder(mModelPath + "NameFind\\");
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openfiledialog = new OpenFileDialog();
            if (openfiledialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string fileName = openfiledialog.FileName;
                textBox1.Text += fileName;
            }
        }

        static string ReadPdfFile(string fileName)
        {
            StringBuilder text = new StringBuilder();
            if (File.Exists(fileName))
            {
                PdfReader pdfReader = new PdfReader(fileName);

                for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                    currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                    text.Append(currentText);
                }
                pdfReader.Close();
            }
            return text.ToString();
        }

        static string[] SplitSentences(string text)
        {
            return mSentenceDetector.SentenceDetect(text);
        }

        static string[] TokenSentence(string text)
        {
            return mTokenizer.Tokenize(text);
        }

        static string[] PosTagging(string[] text)
        {
            return mPosTagger.Tag(text);
        }

        static string NameFind(string[] models, string sentence)
        {
            return mNameFinder.GetNames(models, sentence);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string text = ReadPdfFile(textBox1.Text);
            string[] sentences = SplitSentences(text);

            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            summary summ = new summary();
            List<string> tokenSentence = new List<string>();
            List<string> nonStopWords = new List<string>();
            List<string> posTag = new List<string>();
            List<string> names = new List<string>();
            string[] models = new string[] { "date", "location", "money", "organization", "percentage", "person", "time" };
            List<TFIDFFrequency> tFrequency = new List<TFIDFFrequency>();

            foreach(string sentence in sentences)
            {
                tokenSentence = mTokenizer.Tokenize(sentence).ToList();
                names.Add(mNameFinder.GetNames(models, sentence));
            }

            foreach(string word in StopwordTool._stops.Keys)
            {
                while(tokenSentence.Contains(word))
                {
                    tokenSentence.Remove(word);
                }
            }

            posTag = mPosTagger.Tag(tokenSentence.ToArray()).ToList();

            double[][] inputs = TFIDF.Transform(sentences, 0);
            inputs = TFIDF.Normalize(inputs);

            // Display the output.
            for (int index = 0; index < inputs.Length; index++)
            {
                foreach (double value in inputs[index])
                {
                    tFrequency.Add(new TFIDFFrequency { Sentence = sentences[index], TFValue = value });
                }
            }

            foreach (string word in tokenSentence)
            {
                if(word.Length >= 3)
                {
                    if(dictionary.ContainsKey(word))
                    {
                        dictionary[word]++;
                    }
                    else
                    {
                        dictionary[word] = 1;
                    }
                }
            }

            foreach(string name in names)
            {
                if (name.Length >= 3)
                {
                    if (dictionary.ContainsKey(name))
                    {
                        dictionary[name]++;
                    }
                    else
                    {
                        dictionary[name] = 1;
                    }
                }
            }

            var sortedDict = (from entry in dictionary orderby entry.Value descending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);

            int count2 = 1;
            int result2 = 1;
            int.TryParse(textBox3.Text, out result2);

            foreach (KeyValuePair<string, int> pair in sortedDict)
            {
                summ.Sentences.Add(pair.Key);
                count2++;
                if (count2.Equals(result2))
                {
                    break;
                }
            }

            int count1 = 1;
            int result1 = 1;
            int.TryParse(textBox3.Text, out result1);

            var sortedtFIDF = from data in tFrequency orderby data.TFValue descending select data;

            foreach (var vr in sortedtFIDF)
            {
                summ.Sentences.Add(vr.Sentence);
                count1++;
                if (count1.Equals(result1))
                {
                    break;
                }
            }

            string summary = string.Join("\r\n", summ.Sentences.ToArray());
            richTextBox1.Text = summary;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string text;
            string word = textBox2.Text;
            WebClient web = new WebClient();
            HtmlAgilityPack.HtmlDocument Htmldoc = new HtmlAgilityPack.HtmlDocument();
            Process.Start("https://en.wikipedia.org/wiki/" + word);
            byte[] byteArray = web.DownloadData(new Uri("https://en.wikipedia.org/wiki/" + word));
            Stream stream = new MemoryStream(byteArray);
            Htmldoc.Load(stream);
            FileStream fs = new FileStream("D:\\htmltext2.pdf", FileMode.Create, FileAccess.Write);
            Document pdfDoc = new Document();
            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, fs);
            pdfDoc.Open();
            foreach (HtmlNode node in Htmldoc.DocumentNode.SelectNodes("//p"))
            {
                text = node.InnerText.Trim();
                pdfDoc.Add(new Paragraph(text));
            }
            pdfDoc.Close();

            string htmltext = ReadPdfFile("D:\\htmltext2.pdf");
            string[] sentences = SplitSentences(htmltext);

            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            summary summ = new summary();
            List<string> tokenSentence = new List<string>();
            List<string> nonStopWords = new List<string>();
            List<string> posTag = new List<string>();
            List<string> names = new List<string>();
            string[] models = new string[] { "date", "location", "money", "organization", "percentage", "person", "time" };
            List<TFIDFFrequency> tFrequency = new List<TFIDFFrequency>();

            foreach (string sentence in sentences)
            {
                tokenSentence = mTokenizer.Tokenize(sentence).ToList();
                names.Add(mNameFinder.GetNames(models, sentence));
            }

            foreach (string stopWord in StopwordTool._stops.Keys)
            {
                while (tokenSentence.Contains(stopWord))
                {
                    tokenSentence.Remove(stopWord);
                }
            }

            posTag = mPosTagger.Tag(tokenSentence.ToArray()).ToList();

            double[][] inputs = TFIDF.Transform(sentences, 0);
            inputs = TFIDF.Normalize(inputs);

            // Display the output.
            for (int index = 0; index < inputs.Length; index++)
            {
                foreach (double value in inputs[index])
                {
                    tFrequency.Add(new TFIDFFrequency { Sentence = sentences[index], TFValue = value });
                }
            }

            foreach (string tokenWord in tokenSentence)
            {
                if (tokenWord.Length >= 3)
                {
                    if (dictionary.ContainsKey(tokenWord))
                    {
                        dictionary[tokenWord]++;
                    }
                    else
                    {
                        dictionary[tokenWord] = 1;
                    }
                }
            }

            foreach (string name in names)
            {
                if (name.Length >= 3)
                {
                    if (dictionary.ContainsKey(name))
                    {
                        dictionary[name]++;
                    }
                    else
                    {
                        dictionary[name] = 1;
                    }
                }
            }

            var sortedDict = (from entry in dictionary orderby entry.Value descending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);

            int count2 = 1;
            int result2 = 1;
            int.TryParse(textBox4.Text, out result2);

            foreach (KeyValuePair<string, int> pair in sortedDict)
            {
                summ.Sentences.Add(pair.Key);
                count2++;
                if (count2.Equals(result2))
                {
                    break;
                }
            }

            int count1 = 1;
            int result1 = 1;
            int.TryParse(textBox4.Text, out result1);

            var sortedtFIDF = from data in tFrequency orderby data.TFValue descending select data;

            foreach (var vr in sortedtFIDF)
            {
                summ.Sentences.Add(vr.Sentence);
                count1++;
                if (count1.Equals(result1))
                {
                    break;
                }
            }

            string summary = string.Join("\r\n", summ.Sentences.ToArray());
            richTextBox1.Text = summary;
        }
    }
}
