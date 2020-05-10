using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summary
{
    public class StringTokenizer
    {
        const char EOF = (char)0;

        int line;
        int column;
        int pos;	// position within data

        string data;

        bool ignoreWhiteSpace;
        char[] symbolChars;

        int saveLine;
        int saveCol;
        int savePos;

        public StringTokenizer(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            data = reader.ReadToEnd();

            Reset();
        }

        public StringTokenizer(string data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            this.data = data;

            Reset();
        }

        /// <summary>
        /// gets or sets which characters are part of TokenKind.Symbol
        /// </summary>
        public char[] SymbolChars
        {
            get { return this.symbolChars; }
            set { this.symbolChars = value; }
        }

        /// <summary>
        /// if set to true, white space characters will be ignored,
        /// but EOL and whitespace inside of string will still be tokenized
        /// </summary>
        public bool IgnoreWhiteSpace
        {
            get { return this.ignoreWhiteSpace; }
            set { this.ignoreWhiteSpace = value; }
        }

        private void Reset()
        {
            this.ignoreWhiteSpace = false;
            this.symbolChars = new char[] { '=', '+', '-', '/', ',', '.', '*', '~', '!', '@', '#', '$', '%', '^', '&', '(', ')', '{', '}', '[', ']', ':', ';', '<', '>', '?', '|', '\\' };

            line = 1;
            column = 1;
            pos = 0;
        }

        protected char LA(int count)
        {
            if (pos + count >= data.Length)
                return EOF;
            else
                return data[pos + count];
        }

        protected char Consume()
        {
            char ret = data[pos];
            pos++;
            column++;

            return ret;
        }

        protected Token CreateToken(TokenKind kind, string value)
        {
            return new Token(kind, value, line, column);
        }

        protected Token CreateToken(TokenKind kind)
        {
            string tokenData = data.Substring(savePos, pos - savePos);
            return new Token(kind, tokenData, saveLine, saveCol);
        }

        public Token Next()
        {
            ReadToken:

            char ch = LA(0);
            switch (ch)
            {
                case EOF:
                    return CreateToken(TokenKind.EOF, string.Empty);

                case ' ':
                case '\t':
                    {
                        if (this.ignoreWhiteSpace)
                        {
                            Consume();
                            goto ReadToken;
                        }
                        else
                            return ReadWhitespace();
                    }
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return ReadNumber();

                case '\r':
                    {
                        StartRead();
                        Consume();
                        if (LA(0) == '\n')
                            Consume();	// on DOS/Windows we have \r\n for new line

                        line++;
                        column = 1;

                        return CreateToken(TokenKind.EOL);
                    }
                case '\n':
                    {
                        StartRead();
                        Consume();
                        line++;
                        column = 1;

                        return CreateToken(TokenKind.EOL);
                    }

                case '"':
                    {
                        return ReadString();
                    }

                default:
                    {
                        if (Char.IsLetter(ch) || ch == '_')
                            return ReadWord();
                        else if (IsSymbol(ch))
                        {
                            StartRead();
                            Consume();
                            return CreateToken(TokenKind.Symbol);
                        }
                        else
                        {
                            StartRead();
                            Consume();
                            return CreateToken(TokenKind.Unknown);
                        }
                    }

            }
        }

        private void StartRead()
        {
            saveLine = line;
            saveCol = column;
            savePos = pos;
        }

        protected Token ReadWhitespace()
        {
            StartRead();

            Consume();

            while (true)
            {
                char ch = LA(0);
                if (ch == '\t' || ch == ' ')
                    Consume();
                else
                    break;
            }

            return CreateToken(TokenKind.WhiteSpace);

        }

        protected Token ReadNumber()
        {
            StartRead();

            bool hadDot = false;

            Consume();

            while (true)
            {
                char ch = LA(0);
                if (Char.IsDigit(ch))
                    Consume();
                else if (ch == '.' && !hadDot)
                {
                    hadDot = true;
                    Consume();
                }
                else
                    break;
            }

            return CreateToken(TokenKind.Number);
        }

        protected Token ReadWord()
        {
            StartRead();

            Consume();

            while (true)
            {
                char ch = LA(0);
                if (Char.IsLetter(ch) || ch == '_')
                    Consume();
                else
                    break;
            }

            return CreateToken(TokenKind.Word);
        }

        protected Token ReadString()
        {
            StartRead();

            Consume();

            while (true)
            {
                char ch = LA(0);
                if (ch == EOF)
                    break;
                else if (ch == '\r')
                {
                    Consume();
                    if (LA(0) == '\n')
                        Consume();

                    line++;
                    column = 1;
                }
                else if (ch == '\n')
                {
                    Consume();

                    line++;
                    column = 1;
                }
                else if (ch == '"')
                {
                    Consume();
                    if (LA(0) != '"')
                        break;
                    else
                        Consume();
                }
                else
                    Consume();
            }

            return CreateToken(TokenKind.QuotedString);
        }

        protected bool IsSymbol(char c)
        {
            for (int i = 0; i < symbolChars.Length; i++)
                if (symbolChars[i] == c)
                    return true;

            return false;
        }
    }
}
