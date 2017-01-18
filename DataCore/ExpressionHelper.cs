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

        public static string GetQueryFromExpression(ITranslator translator, Expression clause, Parameters parameters)
        {
            var members = GetMemberExpressions(translator, clause);
            if (!members.Any())
                return string.Empty;

            var enumerator = members.GetEnumerator();
            return GetString(translator, enumerator, parameters);
        }

        public static string GetString(ITranslator translator, IEnumerator<Expression> iterator, Parameters parameters)
        {
            if (!iterator.MoveNext())
                return string.Empty;

            var expression = iterator.Current;

            var binaryExpression = expression as BinaryExpression;
            if (binaryExpression != null)
                return BinaryExpressionString(translator, iterator, binaryExpression, parameters);

            var memberExpression = expression as MemberExpression;
            if (memberExpression != null)
                return MemberExpressionString(memberExpression, parameters);

            var constantExpression = expression as ConstantExpression;
            if (constantExpression != null)
                return ConstantExpressionString(translator, constantExpression, parameters);

            return string.Empty;
        }

        public static string MemberExpressionString(MemberExpression memberExpression, Parameters parameters)
        {
            return string.Concat(memberExpression.Member.DeclaringType.Name, ".", memberExpression.Member.Name);
        }

        public static string BinaryExpressionString(ITranslator translator, IEnumerator<Expression> iterator, BinaryExpression binaryExpression, Parameters parameters)
        {
            var format = translator.GetFormatFor(binaryExpression.NodeType);

            var left = string.Empty;
            var right = string.Empty;

            if (iterator.MoveNext())
                left = GetQueryFromExpression(translator, iterator.Current, parameters);

            if (iterator.MoveNext())
                right = GetQueryFromExpression(translator, iterator.Current, parameters);

            return string.Format(format, left, right);
        }

        public static string ConstantExpressionString(ITranslator translator, ConstantExpression constantExpression, Parameters parameters)
        {
            var value = GetValueFrom(translator, constantExpression.Type, constantExpression.Value);

            if (value.GetType().Name == "StringAsIs")
                return value.ToString();

            if (value.GetType().Name == "StringAsIsWithParameter")
            {
                var stringWithParameter = (StringAsIsWithParameter)value;
                parameters.Add(stringWithParameter.Parameter);

                return stringWithParameter.Value;
            }

            return parameters.Add(value);
        }

        public static object GetValueFrom(ITranslator translator, Type type, object value)
        {
            switch (type.Name)
            {
                case "Boolean":
                    return translator.GetBooleanValue(value);
                case "String":
                    return translator.GetStringValue(value);
                case "DateTime":
                    return translator.GetDateTimeValue(value);
                case "StringAsIs":
                    return value;
                case "StringAsIsWithParameter":
                    return value;
                default:
                    return Convert.ToInt32(value);
            }
        }

        public static IEnumerable<Expression> GetMemberExpressions(ITranslator translator, Expression body)
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
                    var parameters = new Parameters();
                    string constantValue;
                    if (GetSqlExtensionMethodCallConstant(translator, (MethodCallExpression)expr, parameters, out constantValue))
                        yield return Expression.Constant(new StringAsIsWithParameter(constantValue, parameters));
                }
            }
        }

        public static string FormatStringFromArguments<T>(ITranslator translator, Expression<Func<T, dynamic>> clause,
            string startString, Parameters parameters, string format = "{0}")
        {
            var returnString = startString;

            var arguments = GetExpressionsFromDynamic(clause);

            if (arguments != null && arguments.Length > 0)
            {
                if (!string.IsNullOrEmpty(returnString))
                    returnString += ", ";

                returnString += string.Join(", ",
                    arguments.Select(f => string.Format(format, GetStringForExpression(translator, f, parameters))));
            }

            return returnString;
        }

        private static string GetStringForExpression(ITranslator translator, Expression expression, Parameters parameters)
        {
            var memberExpression = expression as MemberExpression;
            if (memberExpression != null)
            {
                var constantExpression = memberExpression.Expression as ConstantExpression;
                if (constantExpression != null)
                {
                    var container = constantExpression.Value;
                    var member = memberExpression.Member;

                    var fieldInfo = member as FieldInfo;
                    if (fieldInfo != null)
                    {
                        var value = fieldInfo.GetValue(container);
                        return GetStringForExpression(translator, Expression.Constant(value), parameters);
                    }

                    var propertyInfo = member as PropertyInfo;
                    if (propertyInfo != null)
                    {
                        var value = propertyInfo.GetValue(container, null);
                        return GetStringForExpression(translator, Expression.Constant(value), parameters);
                    }
                }

                return string.Concat(memberExpression.Member.DeclaringType.Name, ".", memberExpression.Member.Name);
            }

            var constExpr = expression as ConstantExpression;
            if (constExpr != null)
            {
                var value = GetValueFrom(translator, constExpr.Type, constExpr.Value);
                var key = parameters.Add(value);
                return key;
            }

            var methodExpression = expression as MethodCallExpression;
            if (methodExpression != null)
            {
                string concat;
                if (GetSqlExtensionMethodCallConstant(translator, methodExpression, parameters, out concat))
                    return concat;
            }

            var newArrayExpression = expression as NewArrayExpression;
            if (newArrayExpression != null)
                return string.Join(",", newArrayExpression.Expressions.Select(ex => GetStringForExpression(translator, ex, parameters)));

            return string.Empty;
        }

        public static bool GetSqlExtensionMethodCallConstant(ITranslator translator, MethodCallExpression methodExpression, Parameters parameters, out string concat)
        {
            if (methodExpression.Method.Name == "Min" && methodExpression.Method.ReflectedType.Name == "SqlExtensions")
            {
                concat = string.Concat("MIN(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), ")");
                return true;
            }

            if (methodExpression.Method.Name == "Max" && methodExpression.Method.ReflectedType.Name == "SqlExtensions")
            {
                concat = string.Concat("MAX(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), ")");
                return true;
            }

            if (methodExpression.Method.Name == "Sum" && methodExpression.Method.ReflectedType.Name == "SqlExtensions")
            {
                concat = string.Concat("SUM(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), ")");
                return true;
            }

            if (methodExpression.Method.Name == "As" && methodExpression.Method.ReflectedType.Name == "SqlExtensions")
            {
                concat = string.Concat(GetStringForExpression(translator, methodExpression.Arguments[0], parameters), " AS '",
                    Convert.ToString(methodExpression.Arguments[1]).Replace("\"", ""), "'");
                return true;
            }

            if (methodExpression.Method.Name == "Between" && methodExpression.Method.ReflectedType.Name == "SqlExtensions")
            {
                concat = string.Concat("(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), " BETWEEN ",
                    GetStringForExpression(translator, methodExpression.Arguments[1], parameters), " AND ",
                    GetStringForExpression(translator, methodExpression.Arguments[2], parameters), ")");
                return true;
            }

            if (methodExpression.Method.Name == "In" && methodExpression.Method.ReflectedType.Name == "SqlExtensions")
            {
                concat = string.Concat("(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), " IN (",
                    GetStringForExpression(translator, methodExpression.Arguments[1], parameters), "))");

                return true;
            }

            if (methodExpression.Method.Name == "Like" && methodExpression.Method.ReflectedType.Name == "SqlExtensions")
            {
                concat = string.Concat("(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), " LIKE ",
                    GetStringForExpression(translator, methodExpression.Arguments[1], parameters), ")");

                return true;
            }

            if (methodExpression.Method.Name == "TrimSql" && methodExpression.Method.ReflectedType.Name == "SqlExtensions")
            {
                concat = string.Concat("LTRIM(RTRIM(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), "))");

                return true;
            }

            if (methodExpression.Method.Name == "Length" && methodExpression.Method.ReflectedType.Name == "SqlExtensions")
            {
                var lengthName = translator.GetLengthFunctionName();

                concat = string.Concat(lengthName, "(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), ")");

                return true;
            }

            if (methodExpression.Method.Name == "Upper" && methodExpression.Method.ReflectedType.Name == "SqlExtensions")
            {
                concat = string.Concat("UPPER(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), ")");

                return true;
            }

            if (methodExpression.Method.Name == "Lower" && methodExpression.Method.ReflectedType.Name == "SqlExtensions")
            {
                concat = string.Concat("LOWER(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), ")");

                return true;
            }

            if (methodExpression.Method.Name == "IsNull" && methodExpression.Method.ReflectedType.Name == "SqlExtensions")
            {
                var isNullName = translator.GetIsNullFunctionName();

                concat = string.Concat(isNullName, "(",
                    GetStringForExpression(translator, methodExpression.Arguments[0], parameters), ",",
                    GetStringForExpression(translator, methodExpression.Arguments[1], parameters), ")");

                return true;
            }

            if (methodExpression.Method.Name == "Cast" && methodExpression.Method.ReflectedType.Name == "SqlExtensions")
            {
                concat = string.Concat("CAST(",
                    GetStringForExpression(translator, methodExpression.Arguments[0], parameters), " AS ",
                    translator.GetTextFor(methodExpression.Method.ReturnType), ")");

                return true;
            }

            if (methodExpression.Method.Name == "Average" && methodExpression.Method.ReflectedType.Name == "SqlExtensions")
            {
                concat = string.Concat("AVG(", GetStringForExpression(translator, methodExpression.Arguments[0], null), ")");

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
