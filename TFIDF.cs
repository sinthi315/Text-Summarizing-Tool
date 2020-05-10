using LemmaSharp.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Summary
{
    public class TFIDF
    {
        private int _numTerms = 0;
        /// <summary>
        /// Document vocabulary, containing each word's IDF value.
        /// </summary>
        public static Dictionary<string, double> _vocabularyIDF = new Dictionary<string, double>();

        /// <summary>
        /// Transforms a list of documents into their associated TF*IDF values.
        /// If a vocabulary does not yet exist, one will be created, based upon the documents' words.
        /// </summary>
        /// <param name="documents">string[]</param>
        /// <param name="vocabularyThreshold">Minimum number of occurences of the term within all documents</param>
        /// <returns>double[][]</returns>
        public static double[][] Transform(string[] documents, int vocabularyThreshold = 3)
        {
            List<List<string>> stemmedDocs;
            List<string> vocabulary;

            // Get the vocabulary and stem the documents at the same time.
            vocabulary = GetVocabulary(documents, out stemmedDocs, vocabularyThreshold);

            if (_vocabularyIDF.Count == 0)
            {
                // Calculate the IDF for each vocabulary term.
                foreach (var term in vocabulary)
                {
                    double numberOfDocsContainingTerm = stemmedDocs.Where(d => d.Contains(term)).Count();
                    _vocabularyIDF[term] = Math.Log((double)stemmedDocs.Count / ((double)1 + numberOfDocsContainingTerm));
                }
            }

            // Transform each document into a vector of tfidf values.
            return TransformToTFIDFVectors(stemmedDocs, _vocabularyIDF);
        }

        /// <summary>
        /// Converts a list of stemmed documents (lists of stemmed words) and their associated vocabulary + idf values, into an array of TF*IDF values.
        /// </summary>
        /// <param name="stemmedDocs">List of List of string</param>
        /// <param name="vocabularyIDF">Dictionary of string, double (term, IDF)</param>
        /// <returns>double[][]</returns>
        private static double[][] TransformToTFIDFVectors(List<List<string>> stemmedDocs, Dictionary<string, double> vocabularyIDF)
        {
            // Transform each document into a vector of tfidf values.
            List<List<double>> vectors = new List<List<double>>();
            foreach (var doc in stemmedDocs)
            {
                List<double> vector = new List<double>();

                foreach (var vocab in vocabularyIDF)
                {
                    // Term frequency = count how many times the term appears in this document.
                    double tf = doc.Where(d => d == vocab.Key).Count();
                    double tfidf = tf * vocab.Value;

                    vector.Add(tfidf);
                }

                vectors.Add(vector);
            }

            return vectors.Select(v => v.ToArray()).ToArray();
        }

        /// <summary>
        /// Normalizes a TF*IDF array of vectors using L2-Norm.
        /// Xi = Xi / Sqrt(X0^2 + X1^2 + .. + Xn^2)
        /// </summary>
        /// <param name="vectors">double[][]</param>
        /// <returns>double[][]</returns>
        public static double[][] Normalize(double[][] vectors)
        {
            // Normalize the vectors using L2-Norm.
            List<double[]> normalizedVectors = new List<double[]>();
            foreach (var vector in vectors)
            {
                var normalized = Normalize(vector);
                normalizedVectors.Add(normalized);
            }

            return normalizedVectors.ToArray();
        }

        /// <summary>
        /// Normalizes a TF*IDF vector using L2-Norm.
        /// Xi = Xi / Sqrt(X0^2 + X1^2 + .. + Xn^2)
        /// </summary>
        /// <param name="vectors">double[][]</param>
        /// <returns>double[][]</returns>
        public static double[] Normalize(double[] vector)
        {
            List<double> result = new List<double>();

            double sumSquared = 0;
            foreach (var value in vector)
            {
                sumSquared += value * value;
            }

            double SqrtSumSquared = Math.Sqrt(sumSquared);

            foreach (var value in vector)
            {
                // L2-norm: Xi = Xi / Sqrt(X0^2 + X1^2 + .. + Xn^2)
                result.Add(value / SqrtSumSquared);
            }

            return result.ToArray();
        }

      public static double ComputeCosineSimilarity(double[] vector1, double[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new Exception("DIFER LENGTH");

            float denom = (VectorLength(vector1) * VectorLength(vector2));
            if (denom == 0F)
                return 0F;
            else
                return (InnerProduct(vector1, vector2) / denom);
        }

        public static double InnerProduct(double[] vector1, double[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new Exception("DIFFER LENGTH ARE NOT ALLOWED");
            double result = 0F;
            for (int i = 0; i < vector1.Length; i++)
                result += vector1[i] * vector2[i];
            return result;
        }

        public static float VectorLength(double[] vector)
        {
            double sum = 0.0F;
            for (int i = 0; i<vector.Length; i++)
                sum =sum + (vector[i] * vector[i]);
            return (float)Math.Sqrt(sum);
        }

        private double[] GetTermVector(int doc)
        {
            _numTerms = _vocabularyIDF.Count;
            double[][] _termWeight = new double[_numTerms][];
            double[] w = new double[_numTerms];
            for (int i = 0; i<_numTerms; i++)
                w[i]=_termWeight[i][doc];
            return w;
        }

    public double GetSimilarity(int doc_i, int doc_j)
        {
            double[] vector1 = GetTermVector(doc_i);
            double[] vector2 = GetTermVector(doc_j);

            return TFIDF.ComputeCosineSimilarity(vector1, vector2) ;
        }


    /// <summary>
    /// Saves the TFIDF vocabulary to disk.
    /// </summary>
    /// <param name="filePath">File path</param>
    public static void Save(string filePath = "vocabulary.dat")
        {
            // Save result to disk.
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, _vocabularyIDF);
            }
        }

        /// <summary>
        /// Loads the TFIDF vocabulary from disk.
        /// </summary>
        /// <param name="filePath">File path</param>
        public static void Load(string filePath = "vocabulary.dat")
        {
            // Load from disk.
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                _vocabularyIDF = (Dictionary<string, double>)formatter.Deserialize(fs);
            }
        }

        #region Private Helpers

        /// <summary>
        /// Parses and tokenizes a list of documents, returning a vocabulary of words.
        /// </summary>
        /// <param name="docs">string[]</param>
        /// <param name="stemmedDocs">List of List of string</param>
        /// <returns>Vocabulary (list of strings)</returns>
        static string path = @"C:\Users\SAIMA\Documents\Visual Studio 2015\Projects\Summary\Summary\bin\Models\full7z-mlteast-en.lem";
        private static List<string> GetVocabulary(string[] sentences, out List<List<string>> lemmalizeWords, int vocabularyThreshold)
        {
            string filteredLine;
            List<string> filterLine = new List<string>();
            List<string> tokenizedWords = new List<string>();
            List<string> vocabulary = new List<string>();
            lemmalizeWords = new List<List<string>>();
            Dictionary<string, int> tFrequency = new Dictionary<string, int>();
            var stream = File.OpenRead(path);
            var lemmatizer = new Lemmatizer(stream);

            int docIndex = 0;

            foreach (var doc in sentences)
            {
                List<string> stemmedDoc = new List<string>();
                docIndex++;

                tokenizedWords = Tokenize(doc);

                List<string> lemmalizeWord = new List<string>();
                foreach (string part in tokenizedWords)
                {
                    // Strip non-alphanumeric characters.
                    string stripped = Regex.Replace(part, "[^a-zA-Z0-9]", "");
                    filteredLine = StopwordTool.RemoveStopwords(stripped);
                    string stem = lemmatizer.Lemmatize(filteredLine);
                    lemmalizeWord.Add(stem);

                    if (stem.Length > 0)
                    {
                        if (tFrequency.ContainsKey(stem))
                        {
                            tFrequency[stem]++;
                        }
                        else
                        {
                            tFrequency.Add(stem, 0);
                        }

                        stemmedDoc.Add(stem);
                    }
                }
                lemmalizeWords.Add(lemmalizeWord);
            }

            var vocabList = tFrequency.Where(w => w.Value >= vocabularyThreshold);
            foreach (var item in vocabList)
            {
                vocabulary.Add(item.Key);
            }

            return vocabulary;
        }

        private static List<string> Tokenize(string text)
        {
            List<string> tokenizedWords = new List<string>();
            StringTokenizer tokenize = new StringTokenizer(text);
            tokenize.IgnoreWhiteSpace = true;
            tokenize.SymbolChars = new char[] { '=', '+', '-', '/', ',', '.', '*', '~', '!', '@', '#', '$', '%', '^', '&', '(', ')', '{', '}', '[', ']', ':', ';', '<', '>', '?', '|', '\\' };
            Token token;
            do
            {
                token = tokenize.Next();
                tokenizedWords.Add(token.Value);
            } while (token.Kind != TokenKind.EOF);

            return tokenizedWords;
        }

        #endregion
    }
}
