using System;
using System.Collections.Generic;

namespace WarpCore.Platform.DataAnnotations
{
    public static class StackExtensions
    {
        public static T PopUntil<T>(this Stack<T> stack, Func<T, bool> untilCondition)
        {
            if (stack.Count == 0)
                return default(T);

            var peek = stack.Peek();
            var untilConditionIsMet = untilCondition.Invoke(peek);
            if (!untilConditionIsMet)
            {
                stack.Pop();
                PopUntil<T>(stack, untilCondition);
            }
            else
                return peek;

            return default(T);
        }
    }
}