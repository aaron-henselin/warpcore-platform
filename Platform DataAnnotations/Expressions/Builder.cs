using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarpCore.Platform.DataAnnotations.Expressions
{
    public class BooleanExpressionTokenReader
    {
        private class BooleanExpressionTokenReaderCursor
        {
            public int Position=-1;
            public List<BooleanExpressionToken> Tokens { get; set; }
            public bool IsEof => Position >= Tokens.Count;

            //public bool Read<T>(out T token)
            //{
            //    var baseRead = Read(out token);
            //    if (!(token is T))
            //        throw new Exception("Expected " + typeof(T));

            //    return baseRead;
            //}

            public bool Read(out BooleanExpressionToken token)
            {
                Position++;

                if (IsEof)
                {
                    token = null;
                    return false;
                }

                token = Tokens[Position];
                return true;
            }

            public BooleanExpressionToken Peek()
            {
                return Tokens[Position];
            }

        }

        private class ReducedParenToken : ReducedToken
        {
        }


        private class ReducedToken : BooleanExpressionToken
        {
            public BooleanExpression ReducedExpression { get; set; }
        }

        private class ReducedScalarToken : ReducedToken
        {
        }

        private IReadOnlyCollection<BooleanExpressionToken> ReduceParenTokens(IReadOnlyCollection<BooleanExpressionToken> tokens)
        {
            var cursor = new BooleanExpressionTokenReaderCursor { Tokens = tokens.ToList() };

            List<BooleanExpressionToken> allTokens = new List<BooleanExpressionToken>();
            while (cursor.Read(out var token))
            {
                if (token is OpenParenToken)
                {
                    var innerTokens = ReadUntilClosingParen(cursor);
                    var parenExpression = new BooleanParenExpression { InnerExpression = new BooleanExpressionTokenReader().CreateBooleanExpressionFromTokens(innerTokens) };

                    var reduced = new ReducedToken { ReducedExpression = parenExpression };
                    allTokens.Add(reduced);
                }
                else
                    allTokens.Add(token);
            }
            return allTokens;
        }

        private IReadOnlyCollection<BooleanExpressionToken> ReduceScalarTokens(IReadOnlyCollection<BooleanExpressionToken> tokens)
        {
            var cursor = new BooleanExpressionTokenReaderCursor { Tokens = tokens.ToList() };

            List<BooleanExpressionToken> allTokens = new List<BooleanExpressionToken>();
            while (cursor.Read(out var token))
            {
                if (token is NegationToken)
                {
                    var reducedScalar = new ReducedScalarToken();

                    cursor.Read(out var negatedToken);
                    if (negatedToken is ReducedParenToken _reduced)
                    {
                        reducedScalar.ReducedExpression = _reduced.ReducedExpression;
                        allTokens.Add(reducedScalar);
                        continue;
                    }

                    if (negatedToken is VariableReference)
                    {
                        reducedScalar.ReducedExpression = new BooleanExpressionTokenReader().ReadScalar(negatedToken);
                        allTokens.Add(reducedScalar);
                        continue;
                    }

                    throw new ArgumentException("Unsupported use of unary not");
                }
                else if (token is VariableReference || token is DecimalLiteral || token is StringLiteral || token is IntegerLiteral)
                {
                    allTokens.Add(new ReducedScalarToken { ReducedExpression = new BooleanExpressionTokenReader().ReadScalar(token) });
                }
                else
                {
                    allTokens.Add(token);
                }
            }
            return allTokens;
        }

        private IReadOnlyCollection<BooleanExpressionToken> ReduceComparisonTokens(IReadOnlyCollection<BooleanExpressionToken> tokens)
        {
            var cursor = new BooleanExpressionTokenReaderCursor { Tokens = tokens.ToList() };

            List<BooleanExpressionToken> allTokens = new List<BooleanExpressionToken>();
            //BooleanExpression comparisonLeftScope = null;
            while (cursor.Read(out var token))
            {
                if (token is ComparisonOperatorToken comparison)
                {
                    if (!allTokens.Any())
                        throw new ArgumentException("Expect left operand for comparison.");

                    var success = cursor.Read(out var rightToken);
                    if (!success)
                        throw new ArgumentException("No right operand for comparison.");

                    var pop = (ReducedScalarToken)allTokens.Last();
                    allTokens.Remove(pop);
                    allTokens.Add(new ReducedScalarToken
                    {
                        ReducedExpression = new ComparisonExpression
                        {
                            Left = pop.ReducedExpression,
                            ComparisonType = comparison.ComparisonType,
                            Right = ((ReducedToken)rightToken).ReducedExpression
                        }
                    });

                }
                else
                {
                    allTokens.Add(token);
                }
            }
            return allTokens;
        }

        private IReadOnlyCollection<BooleanExpressionToken> ReduceBooleanBinaryTokens(IReadOnlyCollection<BooleanExpressionToken> tokens)
        {
            var cursor = new BooleanExpressionTokenReaderCursor { Tokens = tokens.ToList() };

            List<BooleanExpressionToken> allTokens = new List<BooleanExpressionToken>();

            while (cursor.Read(out var token))
            {
                if (token is BooleanBinaryToken logicGateToken)
                {
                    if (!allTokens.Any())
                        throw new ArgumentException("Expect left operand for boolean binary.");

                    var success = cursor.Read(out var rightToken);
                    if (!success)
                        throw new ArgumentException("No right operand for logic gate.");

                    var pop = (ReducedScalarToken)allTokens.Last();
                    allTokens.Remove(pop);
                    allTokens.Add(new ReducedScalarToken
                    {
                        ReducedExpression = new BooleanBinaryExpression
                        {
                            Left = pop.ReducedExpression,
                            GateType = logicGateToken.GateType,
                            Right = ((ReducedToken)rightToken).ReducedExpression
                        }
                    });
                }
                else
                {
                    allTokens.Add(token);
                }
            }
            return allTokens;
        }

        public BooleanExpression CreateBooleanExpressionFromTokens(IReadOnlyCollection<BooleanExpressionToken> tokens)
        {
            tokens = ReduceParenTokens(tokens);
            tokens = ReduceScalarTokens(tokens);
            tokens = ReduceComparisonTokens(tokens);

            while (!tokens.All(x => x is ReducedToken))
                tokens = ReduceBooleanBinaryTokens(tokens);

            return ((ReducedToken)tokens.Single()).ReducedExpression;
        }

        private ScalarValueExpression ReadScalar(BooleanExpressionToken token)
        {
            if (token is VariableReference _variable)
                return new IdentifierExpression { Name = _variable.Name };

            if (token is DecimalLiteral @decimal)
                return new LiteralDecimalExpression { Value = @decimal.Value };

            if (token is StringLiteral @string)
                return new LiteralStringExpression { Value = @string.Value };

            if (token is IntegerLiteral @int)
                return new LiteralIntExpression { Value = @int.Value };

            throw new ArgumentException("Invalid scalar expression.");
        }

        private IReadOnlyCollection<BooleanExpressionToken> ReadUntilClosingParen(BooleanExpressionTokenReaderCursor cursor)
        {
            List<BooleanExpressionToken> innerTokens = new List<BooleanExpressionToken>();
            var currentDepth = 1;
            while (cursor.Read(out var token))
            {
                if (token is OpenParenToken)
                    currentDepth++;

                if (token is CloseParenToken)
                {
                    currentDepth--;
                    if (currentDepth == 0)
                        return innerTokens; //don't add the closing paren to the inner tokens.
                }

                innerTokens.Add(token);
            }

            return innerTokens;
        }
    }

    public class LiteralIntExpression : ScalarValueExpression
    {
        public int Value { get; set; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class LiteralDecimalExpression : ScalarValueExpression
    {
        public decimal Value { get; set; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class LiteralStringExpression : ScalarValueExpression
    {
        public string Value { get; set; }

        public override string ToString()
        {
            //todo doesn't cover everything.
            return '"'+Value.Replace("\"","\\\"")+'"';
        }
    }

    public class IdentifierExpression : ScalarValueExpression
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public abstract class ScalarValueExpression : BooleanExpression
    {
        


    }

    public abstract class BooleanExpression
    {

    }

    public class BooleanBinaryExpression : BooleanExpression
    {
        public BooleanExpression Left { get; set; }

        public LogicGateType GateType { get; set; }

        public BooleanExpression Right { get; set; }

        private string GateTypeToString(LogicGateType type)
        {
            if (type == LogicGateType.And)
                return "&&";

            if (type == LogicGateType.Or)
                return "||";

            throw new ArgumentException();
        }

        public override string ToString()
        {
            return Left.ToString() + " " + GateTypeToString(GateType) + " " + Right.ToString();
        }
    }

    public class BooleanParenExpression : BooleanExpression
    {
        public BooleanExpression InnerExpression { get; set; }

        public override string ToString()
        {
            return "(" + InnerExpression.ToString() + ")";
        }
    }

    public class UnaryExpression : ScalarValueExpression
    {
        public bool Not { get; set; }

        public BooleanExpression InnerExpression { get; set; }

        public override string ToString()
        {
            if (Not)
                return "!" + InnerExpression;

            return InnerExpression.ToString();
        }
    }


    public class ComparisonExpression : BooleanExpression
    {
        public BooleanExpression Left { get; set; }
        public ComparisonType ComparisonType { get; set; }
        public BooleanExpression Right { get; set; }

        private string ComparisonTypeToString(ComparisonType type)
        {
            switch (type)
            {
                case ComparisonType.NotEqual:
                    return "!=";
                case ComparisonType.Equals:
                    return "==";
                case ComparisonType.GreaterThan:
                    return ">";
                case ComparisonType.GreaterThanOrEqualTo:
                    return ">=";
                case ComparisonType.LessThan:
                    return "<";
                case ComparisonType.LessThanOrEqualTo:
                    return "<=";
            }

            throw new ArgumentException();
        }

        public override string ToString()
        {
            return Left.ToString() + " " + ComparisonTypeToString(ComparisonType) + " " + Right.ToString();
        }

    }
}