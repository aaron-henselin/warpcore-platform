using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarpCore.Platform.DataAnnotations.Expressions
{
    public class BooleanExpressionParser
    {
        private class Reader
        {
            public int Position { get; set; } = -1;
            public string Expression { get; set; }

            public char Peek()
            {
                if (Position == Expression.Length - 1)
                    return default(char);

                return Expression[Position+1];
            }

            public void MoveToNext()
            {
                
                while (Peek() == ' ' && !IsEof)
                    TryReadCharacter(out _);

            }

            static Dictionary<char,char>_numericWordCharacters = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.' }.ToDictionary(x => x);
            static Dictionary<char, char> _variableWordCharacters = "_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.".ToArray().ToDictionary(x => x);
            static Dictionary<char, char> _comparisonSymbology = "=<>!".ToArray().ToDictionary(x => x);
            static Dictionary<char, char> _booleanBinarySymbology = "|&".ToArray().ToDictionary(x => x);
            static Dictionary<char, char> _parenSymbology = "()".ToArray().ToDictionary(x => x);

            internal bool IsParenCharacter(char character)
            {
                return _parenSymbology.ContainsKey(character);
            }

            internal bool IsBooleanBinaryCharacter(char character)
            {
                return _booleanBinarySymbology.ContainsKey(character);
            }

            internal bool IsBooleanComparisonCharacter(char character)
            {
                return _comparisonSymbology.ContainsKey(character);
            }

            internal BooleanExpressionToken ReadIdentifier()
            {
                var entireWord = ReadWord(_variableWordCharacters);
                return new VariableReference { Name = entireWord };
            }

            internal BooleanExpressionToken ReadBooleanBinary()
            {
                var entireWord = ReadWord(_booleanBinarySymbology);
                if (entireWord == "||")
                    return new BooleanBinaryToken { GateType = LogicGateType.Or };

                if (entireWord == "&&")
                    return new BooleanBinaryToken { GateType = LogicGateType.And };

                throw new ArgumentException("Invalid boolean binary operator " + entireWord);
            }

            internal BooleanExpressionToken ReadNumericLiteral()
            {
                var entireWord = ReadWord(_numericWordCharacters);
                var isInt = !entireWord.Contains(".");
                if (isInt)
                    return new IntegerLiteral { Value = Int32.Parse(entireWord) };
                else
                    return new DecimalLiteral { Value = decimal.Parse(entireWord) };
            }

            public BooleanExpressionToken ReadStringLiteral()
            {
                TryReadCharacter(out var _); //skips the initial quote.

                char? previousCharacter = null;
                var pendingString = new StringBuilder();
                while (TryReadCharacter(out var newChar))
                {
                    bool isQuote = newChar == '"';
                    bool isInEscapeSequence = '\\' == previousCharacter;
                    bool isEscapeCharacter = '\\' == newChar;

                    if (!isInEscapeSequence && isQuote)
                    {
                        TryReadCharacter(out var _); //skips the last quote.
                        return new StringLiteral(pendingString);
                    }
                    
                    if (!isEscapeCharacter || isInEscapeSequence)
                        pendingString.Append(newChar);

                    previousCharacter = newChar;
                }

                throw new Exception("Unclosed string literal");
            }

            public string ReadWord(IDictionary<char,char> wordCharacters)
            {
                var pendingString = new StringBuilder();

                while (!IsEof)
                {
                    var peeked = Peek();
                    var isPartOfWord = peeked != ' ' && wordCharacters.ContainsKey(peeked);
                    if (isPartOfWord)
                    {
                        TryReadCharacter(out var append);
                        pendingString.Append(append);
                    }
                    else
                        return pendingString.ToString();
                }

                return pendingString.ToString();
            }

            public bool IsEof => Position >= Expression.Length-1;

            public bool TryReadCharacter(out char outChar)
            {
                if (!IsEof)
                {
                    Position++;
                    outChar = Expression[Position];
                    
                    return true;
                }
                else
                {
                    outChar = default(char);
                    return false;
                }
            }

            internal BooleanExpressionToken ReadBooleanParenSymbology()
            {
                TryReadCharacter(out var character);

                if (character == ')')
                    return new CloseParenToken();

                if (character == '(')
                    return new OpenParenToken();


                throw new ArgumentException("Invalid paren character: " + character);

            }



            internal BooleanExpressionToken ReadBooleanComparisonSymbology()
            {
                var entireWord = ReadWord(_comparisonSymbology);
                if (entireWord == "!")
                    return new NegationToken();

                if (entireWord == "!=")
                    return new ComparisonOperatorToken { ComparisonType = ComparisonType.NotEqual };

                if (entireWord == "==")
                    return new ComparisonOperatorToken { ComparisonType = ComparisonType.Equals };

                if (entireWord == ">")
                    return new ComparisonOperatorToken { ComparisonType = ComparisonType.GreaterThan };

                if (entireWord == ">=")
                    return new ComparisonOperatorToken { ComparisonType = ComparisonType.GreaterThanOrEqualTo };

                if (entireWord == "<")
                    return new ComparisonOperatorToken { ComparisonType = ComparisonType.LessThan };

                if (entireWord == "<=")
                    return new ComparisonOperatorToken { ComparisonType = ComparisonType.LessThanOrEqualTo };

                throw new ArgumentException("Unsupported expression " + entireWord);
            }
        }



        public IReadOnlyCollection<BooleanExpressionToken> ReadBooleanExpressionTokens(string text)
        {
            List<BooleanExpressionToken> tokens = new List<BooleanExpressionToken>();

            var reader = new Reader { Expression = text };
            reader.MoveToNext();

            while (!reader.IsEof)
            {
                var nextCharacter = reader.Peek();

                if (Char.IsLetter(nextCharacter))
                {
                    var identifier = reader.ReadIdentifier();
                    tokens.Add(identifier);
                    reader.MoveToNext();
                    continue;
                }

                if (nextCharacter == '"')
                {
                    var literal = reader.ReadStringLiteral();
                    tokens.Add(literal);
                    reader.MoveToNext();
                    continue;
                }

                var isNumeric = Char.IsDigit(nextCharacter);
                if (isNumeric)
                {
                    var numeric = reader.ReadNumericLiteral();
                    tokens.Add(numeric);
                    reader.MoveToNext();
                    continue;
                }

                var isParen = reader.IsParenCharacter(nextCharacter);
                if (isParen)
                {
                    var paren = reader.ReadBooleanParenSymbology();
                    tokens.Add(paren);
                    reader.MoveToNext();
                    continue;
                }

                var isComparison = reader.IsBooleanComparisonCharacter(nextCharacter);
                if (isComparison)
                {
                    var paren = reader.ReadBooleanComparisonSymbology();
                    tokens.Add(paren);
                    reader.MoveToNext();
                    continue;
                }

                var isBooleanBinary = reader.IsBooleanBinaryCharacter(nextCharacter);
                if (isBooleanBinary)
                {
                    var paren = reader.ReadBooleanBinary();
                    tokens.Add(paren);
                    reader.MoveToNext();
                    continue;
                }

                throw new ArgumentException("Unable to identify boolean expression character: "+nextCharacter);
            }

            return tokens;

        }
    }



    public abstract class BooleanExpressionToken
    {

    }

    public class BooleanBinaryToken : BooleanExpressionToken
    {

        public LogicGateType GateType { get; set; }
    }

    public enum LogicGateType
    {
        And,Or
    }

    public class OpenParenToken : BooleanExpressionToken
    {
    }

    public class CloseParenToken : BooleanExpressionToken
    {
    }

    public class NegationToken : BooleanExpressionToken
    {
    }

    public class ComparisonOperatorToken : BooleanExpressionToken
    {
       
        public ComparisonType ComparisonType { get; set; }
    }

    public enum ComparisonType { NotEqual,Equals, GreaterThan, GreaterThanOrEqualTo, LessThan, LessThanOrEqualTo}
    


    public class VariableReference : BooleanExpressionToken
    {
        public string Name { get; set; }
    }

    public class IntegerLiteral : BooleanExpressionToken
    {
        public int Value { get; set; }
    }

    public class DecimalLiteral : BooleanExpressionToken
    {
        public decimal Value { get; set; }
    }

    public class StringLiteral : BooleanExpressionToken
    {
        public string Value => pendingString.ToString();

        private StringBuilder pendingString;

        public StringLiteral(StringBuilder pendingString)
        {
            this.pendingString = pendingString;
        }
    }

}
