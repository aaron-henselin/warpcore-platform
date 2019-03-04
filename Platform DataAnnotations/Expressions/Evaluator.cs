using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        private bool EvaluteComparisonExpression(ComparisonExpression comparisonExpression)
        {
            var left= EvalulateScalarExpression(comparisonExpression.Left);
            var right = EvalulateScalarExpression(comparisonExpression.Right);
            comparisonExpression.ComparisonType == ComparisonType.Equals
            
        }

        private Tuple<object,object> DuckType(Tuple<object, object> values)
        {
            var leftType = values.Item1?.GetType();
            var rightType = values.Item2?.GetType();

            if (Type.Equals(leftType, rightType))
                return values;

            var convertTo = new[] { leftType, rightType }.Where(x => x != null && x != typeof(string)).FirstOrDefault();
            if (convertTo != null)
            {

            }
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

