using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarpCore.Platform.Kernel;

namespace WarpCore.Platform.DataAnnotations.Expressions
{
    public class Evaluator
    {
        IVariableScope VariableScope { get; }

        public Evaluator(IVariableScope variableScope)
        {
            VariableScope = variableScope;
        }

        public bool Evaluate(BooleanExpression expression)
        {
            if (expression is ComparisonExpression comparisonExpression)
                return EvaluteComparisonExpression(comparisonExpression);

            if (expression is BooleanBinaryExpression booleanBinaryExpression)
                return EvaluateBooleanBinaryExpression(booleanBinaryExpression);

            if (expression is UnaryExpression unaryExpression)
                return EvaluateUnaryExpression(unaryExpression);

            if (expression is BooleanParenExpression parenExpresion)
                return EvaluateParenExpression(parenExpresion);

            throw new ArgumentException(expression.GetType() +" was not expected.");
        }

        private bool EvaluateParenExpression(BooleanParenExpression parenExpresion)
        {
            return Evaluate(parenExpresion.InnerExpression);
        }

        private bool EvaluateUnaryExpression(UnaryExpression unaryExpression)
        {
            var result = Evaluate(unaryExpression.InnerExpression);
            if (unaryExpression.Not)
                return !result;
            return result;
        }

        private bool EvaluateBooleanBinaryExpression(BooleanBinaryExpression booleanBinaryExpression)
        {
            var leftResult = Evaluate(booleanBinaryExpression.Left);

            var isOr = booleanBinaryExpression.GateType == LogicGateType.Or;
            var isAnd = booleanBinaryExpression.GateType == LogicGateType.And;

            if (isOr && leftResult)
                return true;

            if (isOr && !leftResult)
                return Evaluate(booleanBinaryExpression.Right);

            if (isAnd && !leftResult)
                return false;

            if (isAnd && leftResult)
                return Evaluate(booleanBinaryExpression.Right);

            throw new ArgumentException();
        }

        private bool EvaluteComparisonExpression(ComparisonExpression comparisonExpression)
        {
            var left= EvalulateScalarExpression(comparisonExpression.Left);
            var right = EvalulateScalarExpression(comparisonExpression.Right);

            if (left == null && right == null)
                return true;

            var duckTyped = DuckType(new Tuple<object, object>(left, right));

            var compareResult = Comparer.Default.Compare(duckTyped.Item1, duckTyped.Item2);

            if (compareResult == 0)
            {
                if (ComparisonType.Equals == comparisonExpression.ComparisonType ||
                    ComparisonType.GreaterThanOrEqualTo == comparisonExpression.ComparisonType ||
                    ComparisonType.LessThanOrEqualTo == comparisonExpression.ComparisonType)
                    return true;

                return false;
            }

            if (compareResult == 1)
            {
                if (ComparisonType.NotEqual == comparisonExpression.ComparisonType ||
                    ComparisonType.GreaterThan == comparisonExpression.ComparisonType ||
                    ComparisonType.GreaterThanOrEqualTo == comparisonExpression.ComparisonType)
                    return true;

                return false;
            }

            if (compareResult == -1)
            {
                if (ComparisonType.NotEqual == comparisonExpression.ComparisonType ||
                    ComparisonType.LessThan == comparisonExpression.ComparisonType ||
                    ComparisonType.LessThanOrEqualTo == comparisonExpression.ComparisonType)
                    return true;

                return false;
            }

            throw new ArgumentException();
        }

        private Tuple<object,object> DuckType(Tuple<object, object> values)
        {
            var leftType = values.Item1?.GetType();
            var rightType = values.Item2?.GetType();

            if (Type.Equals(leftType, rightType))
                return values;

            var nonStringType = new[] { leftType, rightType }.FirstOrDefault(x => x != null && x != typeof(string));
            if (nonStringType == null)
                return values;

            var newValue1 = ExtensibleTypeConverter.ChangeType(values.Item1, nonStringType);
            var newValue2 = ExtensibleTypeConverter.ChangeType(values.Item1, nonStringType);
            return new Tuple<object, object>(newValue1,newValue2);
        }


        private object EvalulateScalarExpression(BooleanExpression left)
        {
            if (left is LiteralDecimalExpression literalDecimalExpression)
                return literalDecimalExpression.Value;

            if (left is LiteralStringExpression literalStringExpression)
                return literalStringExpression.Value;

            if (left is LiteralIntExpression literalIntExpression)
                return literalIntExpression.Value;

            if (left is IdentifierExpression identifier)
                return VariableScope.GetValue(identifier.Name);

            return Evaluate(left);
        }
    }

    public interface IVariableScope
    {
        object GetValue(string key);
    }

}

