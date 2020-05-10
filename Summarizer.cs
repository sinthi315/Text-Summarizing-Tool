using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.IO;
using System.Security.Permissions;
using System.Text;

namespace Summary
{
    public class Summarizer
    {
        public Summarizer()
        {
        }

        public static SummarizedDocument Summarize(SummarizerArguments args)
        {
            if (args == null) return null;
            Article article = null;
            if (args.InputString.Length > 0 && args.InputFile.Length == 0)
            {
                article = ParseDocument(args.InputString, args);
            }
            else
            {
                article = ParseFile(args.InputFile, args);
            }
            Grader.Grade(article);
            Highlighter.Highlight(article, args);
            SummarizedDocument sumdoc = CreateSummarizedDocument(article, args);
            return sumdoc;

        }

        private static SummarizedDocument CreateSummarizedDocument(Article article, SummarizerArguments args)
        {
            SummarizedDocument sumDoc = new SummarizedDocument();
            sumDoc.Concepts = article.Concepts;
            foreach (Sentence sentence in article.Sentences)
            {
                if (sentence.Selected)
                {
                    sumDoc.Sentences.Add(sentence.OriginalSentence);
                }
            }
            return sumDoc;
        }

        private static Article ParseFile(string fileName, SummarizerArguments args)
        {
            string text = LoadFile(fileName);
            return ParseDocument(text, args);
        }

        private static Article ParseDocument(string text, SummarizerArguments args)
        {
            Dictionary rules = Dictionary.LoadFromFile(args.DictionaryLanguage);
            Article article = new Article(rules);
            article.ParseText(text);
            return article;
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

        [FileIOPermission(SecurityAction.Demand)]
        internal static string LoadFile(string fileName)
        {
            if (fileName != string.Empty)
                return ReadPdfFile(fileName);
            return string.Empty;
        }
    }
}
