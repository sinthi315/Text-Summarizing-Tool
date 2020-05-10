using System.Collections.Generic;

namespace Summary
{
    public class SummarizedDocument
    {
        public List<string> Concepts { get; set; }
        public List<string> Sentences { get; set; }

        public SummarizedDocument()
        {
            Sentences = new List<string>();
            Concepts = new List<string>();
        }
    }
}