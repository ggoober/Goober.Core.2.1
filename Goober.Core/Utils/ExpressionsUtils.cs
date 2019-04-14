using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Goober.Core.Utils
{
    public static class ExpressionsUtils
    {
        public static object GetValue<T>(Expression<Func<T>> selector)
        {
            var method = selector.Compile();
            var value = method.Invoke();

            return value;
        }

        public static string GetPropertyName<T>(Expression<Func<T>> expressionLambda)
        {
            var stack = new Stack<string>();
            var expression = expressionLambda.Body;

            while (expression != null)
            {
                if (expression.NodeType == ExpressionType.Call)
                {
                    var methodCallExpression = (MethodCallExpression)expression;
                    if (IsSingleArgumentIndexer(methodCallExpression))
                    {
                        stack.Push(string.Empty);
                        expression = methodCallExpression.Object;
                    }
                    else
                        break;
                }
                else if (expression.NodeType == ExpressionType.ArrayIndex)
                {
                    var binaryExpression = (BinaryExpression)expression;
                    stack.Push(string.Empty);
                    expression = binaryExpression.Left;
                }
                else if (expression.NodeType == ExpressionType.MemberAccess)
                {
                    var memberExpression = (MemberExpression)expression;
                    stack.Push("." + memberExpression.Member.Name);
                    expression = memberExpression.Expression;
                }
                else if (expression.NodeType == ExpressionType.Parameter)
                {
                    stack.Push(string.Empty);
                    expression = null;
                }
                else if (expression.NodeType == ExpressionType.Convert)
                {
                    var memberExp = ((UnaryExpression)expression).Operand as MemberExpression;
                    stack.Push("." + memberExp.Member.Name);
                    expression = memberExp.Expression;
                }
                else
                    break;
            }

            if (stack.Count > 0 && string.Equals(stack.Peek(), ".model", StringComparison.OrdinalIgnoreCase))
                stack.Pop();

            if (stack.Count <= 0)
                return string.Empty;

            return (stack).Aggregate(((left, right) => left + right)).TrimStart(new[] { '.' });
        }

        private static bool IsSingleArgumentIndexer(Expression expression)
        {
            var methodExpression = expression as MethodCallExpression;
            if (methodExpression == null || methodExpression.Arguments.Count != 1)
                return false;
            return (methodExpression.Method.DeclaringType.GetDefaultMembers()).OfType<PropertyInfo>().Any((p => p.GetGetMethod() == methodExpression.Method));
        }
    }
}
