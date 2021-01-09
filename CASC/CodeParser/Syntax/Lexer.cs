using System.Diagnostics;
using System;
using CASC.CodeParser.Utils;
using System.Collections.Generic;
using CASC.CodeParser.Text;

namespace CASC.CodeParser.Syntax
{
    internal sealed class Lexer
    {
        private static readonly List<char> _exceptionChineseChar = new List<char> {
            // Common Operators
            '加', // Add                加
            '減', // Minus              減
            '乘', // Multiply           乘
            '除', // Divide             除
            '點', // Point              點
            '開', // OpenParenthesis    開
            '閉', // CloseParenthesis   閉
            '正', // Idnetity           正
            '負', // Negation           負
            '且', // Logical AND        且
            '或', // Logical OR         或
            '反', // Logical Negation   反
            '是', // Eqauls             是
            '不', // Not Equals         不是
            '賦'  // Assign             賦
        };

        private readonly SourceText _text;
        private readonly DiagnosticPack _diagnostics = new DiagnosticPack();

        private int _position;

        private int _start;
        private SyntaxKind _kind;
        private object _value;

        public Lexer(SourceText text)
        {
            _text = text;
        }

        public DiagnosticPack Diagnostics => _diagnostics;

        private char Current => Peek(0);
        private char LookAhead => Peek(1);

        private char Peek(int offset)
        {
            var index = _position + offset;

            if (index >= _text.Length)
                return '\0';

            return _text[index];
        }

        public SyntaxToken Lex()
        {
            _start = _position;
            _kind = SyntaxKind.BadToken;
            _value = null;

            switch (Current)
            {
                case '\0':
                    _kind = SyntaxKind.EndOfFileToken;
                    break;

                case '加':
                case '正':
                case '+':
                    _kind = SyntaxKind.PlusToken;
                    _position++;
                    break;
                case '減':
                case '負':
                case '-':
                    _kind = SyntaxKind.MinusToken;
                    _position++;
                    break;
                case '乘':
                case '*':
                    _kind = SyntaxKind.StarToken;
                    _position++;
                    break;
                case '除':
                case '/':
                    _kind = SyntaxKind.SlashToken;
                    _position++;
                    break;
                case '點':
                case '.':
                    _kind = SyntaxKind.PointToken;
                    _position++;
                    break;
                case '開':
                case '(':
                    _kind = SyntaxKind.OpenParenthesesToken;
                    _position++;
                    break;
                case '閉':
                case ')':
                    _kind = SyntaxKind.CloseParenthesesToken;
                    _position++;
                    break;
                case '且':
                    _kind = SyntaxKind.AmpersandAmpersandToken;
                    _position++;
                    break;
                case '&':
                    if (LookAhead == '&')
                    {
                        _kind = SyntaxKind.AmpersandAmpersandToken;
                        _position += 2;
                        break;
                    }
                    break;
                case '或':
                    _kind = SyntaxKind.PipePipeToken;
                    _position++;
                    break;
                case '|':
                    if (LookAhead == '|')
                    {
                        _kind = SyntaxKind.PipePipeToken;
                        _position += 2;
                        break;
                    }
                    break;
                case '反':
                    _kind = SyntaxKind.BangToken;
                    _position++;
                    break;
                case '!':
                    if (LookAhead == '=')
                    {
                        _kind = SyntaxKind.BangEqualsToken;
                        _position += 2;
                        break;
                    }
                    _kind = SyntaxKind.BangToken;
                    _position++;
                    break;
                case '不':
                    if (LookAhead == '是')
                    {
                        _kind = SyntaxKind.BangEqualsToken;
                        _position += 2;
                        break;
                    }
                    break;
                case '是':
                    _kind = SyntaxKind.EqualsEqualsToken;
                    _position++;
                    break;
                case '=':
                    if (LookAhead != '=')
                        goto case '賦';
                    _kind = SyntaxKind.EqualsEqualsToken;
                    _position += 2;
                    break;
                case '賦':
                    _kind = SyntaxKind.EqualsToken;
                    _position++;
                    break;

                case '0': case '1': case '2': case '3': case '4':
                case '5': case '6': case '7': case '8': case '9':
                case '零': case '一': case '二': case '三': case '四':
                case '五': case '六': case '七': case '八': case '九':
                case '壹': case '貳': case '參': case '肆': case '伍':
                case '陆': case '柒': case '捌': case '玖': case '拾':
                case '十': case '百': case '千': case '萬': case '億':
                    ReadNumberToken();
                    break;

                case ' ':
                case '\t':
                case '\n':
                case '\r':
                    ReadWhiteSpaceToken();
                    break;
                    
                default:
                    if (char.IsLetter(Current))
                        ReadIdentifierOrKeyword();
                    else if (char.IsWhiteSpace(Current))
                        ReadWhiteSpaceToken();
                    else
                    {
                        _diagnostics.ReportBadCharacter(_position, Current);
                        _position++;
                    }
                    break;
            }

            var length = _position - _start;
            var text = SyntaxFacts.GetText(_kind);
            if (text == null)
                text = _text.ToString(_start, length);

            return new SyntaxToken(_kind, _start, text, _value);
        }

        private void ReadNumberToken()
        {
            while (ChineseParser.isDigit(Current))
                _position++;

            var length = _position - _start;
            var text = _text.ToString(_start, length);
            if (!ChineseParser.tryParseDigits(text, out var value))
                _diagnostics.ReportInvalidNumber(new TextSpan(_start, length), text, typeof(int));

            _value = value;
            _kind = SyntaxKind.NumberToken;
        }

        private void ReadWhiteSpaceToken()
        {
            while (char.IsWhiteSpace(Current))
                _position++;

            _kind = SyntaxKind.WhiteSpaceToken;
        }

        private string ReadIdentifierOrKeyword()
        {
            while (char.IsLetter(Current))
                _position++;

            var length = _position - _start;
            var text = _text.ToString(_start, length);
            _kind = SyntaxFacts.GetKeywordKind(text);
            return text;
        }
    }

}