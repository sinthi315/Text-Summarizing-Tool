using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Summary
{
    public class TextBoxStreamWriter : TextWriter
    {
        TextBox _output = null;
        private RichTextBox richTextBox1;

        public TextBoxStreamWriter(TextBox output)
            {
                _output = output;
            }

        public TextBoxStreamWriter(RichTextBox richTextBox1)
        {
            this.richTextBox1 = richTextBox1;
        }

        public override void Write(char value)
            {
                base.Write(value);
                _output.AppendText(value.ToString());

            }

            public override Encoding Encoding
            {
                get { return System.Text.Encoding.UTF8; }
            }
        }
}