using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DataCore
{
    public static class ExpressionHelper
    {
        public static Expression[] GetExpressionsFromDynamic<T>(Expression<Func<T, dynamic>> clause)
        {
            Expression[] arguments = null;

            var newBody = clause.Body as NewExpression;
            if (newBody != null)
                arguments = ((NewExpression)clause.Body).Arguments.ToArray();

            var unaryBody = clause.Body as UnaryExpression;
            if (unaryBody != null)
                arguments = new[] { unaryBody.Operand };

            var memberExpression = clause.Body as MemberExpression;
            if (memberExpression != null)
                arguments = new Expression[] { memberExpression };

            var methodExpression = clause.Body as MethodCallExpression;
            if (methodExpression != null)
                arguments = new Expression[] { methodExpression };

            return arguments;
        }

        public static string GetQueryFromExpression(ITranslator translator, Expression clause)
        {
            var members = GetMemberExpressions(clause);
            if (!members.Any())
                return string.Empty;

            var enumerator = members.GetEnumerator();
            return GetString(translator, enumerator);
        }

        public static string GetString(ITranslator translator, IEnumerator<Expression> iterator)
        {
            if (!iterator.MoveNext())
                return string.Empty;

            var expression = iterator.Current;

            var binaryExpression = expression as BinaryExpression;
            if (binaryExpression != null)
                return BinaryExpressionString(translator, iterator, binaryExpression);

            var memberExpression = expression as MemberExpression;
            if (memberExpression != null)
                return MemberExpressionString(memberExpression);

            var constantExpression = expression as ConstantExpression;
            if (constantExpression != null)
                return ConstantExpressionString(translator, constantExpression);

            return string.Empty;
        }

        public static string MemberExpressionString(MemberExpression memberExpression)
        {
            return string.Concat(memberExpression.Member.DeclaringType.Name, ".", memberExpression.Member.Name);
        }

        public static string BinaryExpressionString(ITranslator translator, IEnumerator<Expression> iterator, BinaryExpression binaryExpression)
        {
            var format = translator.GetFormatFor(binaryExpression.NodeType);

            var left = string.Empty;
            var right = string.Empty;

            if (iterator.MoveNext())
                left = GetQueryFromExpression(translator, iterator.Current);

            if (iterator.MoveNext())
                right = GetQueryFromExpression(translator, iterator.Current);

            return string.Format(format, left, right);
        }

        public static string ConstantExpressionString(ITranslator translator, ConstantExpression constantExpression)
        {
            return GetValueFrom(translator, constantExpression.Type, constantExpression.Value);
        }

        public static string GetValueFrom(ITranslator translator, Type type, object value)
        {
            switch (type.Name)
            {
                case "Boolean":
                    return translator.GetBooleanValue(value);
                case "String":
                    return translator.GetStringValue(value);
                case "DateTime":
                    {
                        var date = Convert.ToDateTime(value);
                        return translator.GetDateTimeValue(date);
                    }
                default:
                    return Convert.ToString(value);
            }
        }

        public static IEnumerable<Expression> GetMemberExpressions(Expression body)
        {
            var candidates = new Queue<Expression>(new[] { body });
            while (candidates.Count > 0)
            {
                var expr = candidates.Dequeue();
                if (expr is MemberExpression)
                {
                    yield return expr;
                }
                else if (expr is ConstantExpression)
                {
                    yield return expr;
                }
                else if (expr is UnaryExpression)
                {
                    var unary = (UnaryExpression)expr;
                    if (unary.NodeType == ExpressionType.Not)
                    {
                        var binaryExpression = unary.Operand as BinaryExpression;
                        if (binaryExpression != null)
                        {
                            if (unary.Operand.NodeType == ExpressionType.Equal)
                                candidates.Enqueue(Expression.MakeBinary(ExpressionType.NotEqual, binaryExpression.Left, binaryExpression.Right));
                            else
                                candidates.Enqueue(Expression.MakeBinary(ExpressionType.Equal, binaryExpression.Left, binaryExpression.Right));

                            continue;
                        }
                    }

                    candidates.Enqueue(unary.Operand);
                }
                else if (expr is BinaryExpression)
                {
                    var binary = (BinaryExpression)expr;
                    candidates.Enqueue(binary.Left);
                    candidates.Enqueue(binary.Right);

                    yield return expr;
                }
                else if (expr is LambdaExpression)
                {
                    candidates.Enqueue(((LambdaExpression)expr).Body);
                }
                else if (expr is MethodCallExpression)
                {
                    string constantValue;
                    if (GetSqlExtensionMethodCallConstant((MethodCallExpression)expr, out constantValue))
                        yield return Expression.Constant(new StringAsIs(constantValue));
                }
            }
        }

        public static string FormatStringFromArguments<T>(Expression<Func<T, dynamic>> clause, string startString, string format = "{0}")
        {
            var returnString = startString;

            var arguments = GetExpressionsFromDynamic(clause);

            if (arguments != null && arguments.Length > 0)
            {
                if (!string.IsNullOrEmpty(returnString))
                    returnString += ", ";

                returnString += string.Join(", ",
                    arguments.Select(f => string.Format(format, GetStringForExpression(f))));
            }

            return returnString;
        }

        private static string GetStringForExpression(Expression expression)
        {
            var memberExpression = expression as MemberExpression;
            if (memberExpression != null)
                return string.Concat(memberExpression.Member.DeclaringType.Name, ".", memberExpression.Member.Name);

            var constExpr = expression as ConstantExpression;
            if (constExpr != null)
                return Convert.ToString(constExpr.Value);

            var methodExpression = expression as MethodCallExpression;
            if (methodExpression != null)
            {
                string concat;
                if (GetSqlExtensionMethodCallConstant(methodExpression, out concat))
                    return concat;
            }

            return string.Empty;
        }

        public static bool GetSqlExtensionMethodCallConstant(MethodCallExpression methodExpression, out string concat)
        {
            if (methodExpression.Method.Name == "Min" && methodExpression.Method.ReflectedType.Name == "SqlExtensions")
            {
                concat = string.Concat("MIN(", GetStringForExpression(methodExpression.Arguments[0]), ")");
                return true;
            }

            if (methodExpression.Method.Name == "Max" && methodExpression.Method.ReflectedType.Name == "SqlExtensions")
            {
                concat = string.Concat("MAX(", GetStringForExpression(methodExpression.Arguments[0]), ")");
                return true;
            }

            if (methodExpression.Method.Name == "Sum" && methodExpression.Method.ReflectedType.Name == "SqlExtensions")
            {
                concat = string.Concat("SUM(", GetStringForExpression(methodExpression.Arguments[0]), ")");
                return true;
            }

            if (methodExpression.Method.Name == "As" && methodExpression.Method.ReflectedType.Name == "SqlExtensions")
            {
                concat = string.Concat(GetStringForExpression(methodExpression.Arguments[0]), " AS '",
                    Convert.ToString(methodExpression.Arguments[1]).Replace("\"", ""), "'");
                return true;
            }

            concat = "";
            return false;
        }

        public static IEnumerable<string> GetStringsFromArguments<T>(Expression<Func<T, dynamic>> clause)
        {
            var arguments = GetExpressionsFromDynamic(clause);
            if (arguments != null && arguments.Length > 0)
            {
                return arguments.Select(f => ((MemberExpression) f).Member.Name);
            }

            return new string[0];
        }
    }
}
