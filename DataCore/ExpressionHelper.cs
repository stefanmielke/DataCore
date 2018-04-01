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

            switch (clause.Body)
            {
                case NewExpression _:
                    arguments = ((NewExpression)clause.Body).Arguments.ToArray();
                    break;
                case UnaryExpression unaryBody:
                    arguments = new[] { unaryBody.Operand };
                    break;
                case MemberExpression memberExpression:
                    arguments = new Expression[] { memberExpression };
                    break;
                case MethodCallExpression methodExpression:
                    arguments = new Expression[] { methodExpression };
                    break;
            }

            return arguments;
        }

        public static string GetQueryFromExpression(ITranslator translator, Expression clause, Parameters parameters)
        {
            var members = GetMemberExpressions(translator, clause).ToList();
            if (!members.Any())
                return string.Empty;

            var enumerator = members.GetEnumerator();
            return GetString(translator, enumerator, parameters);
        }

        private static string GetString(ITranslator translator, List<Expression>.Enumerator iterator, Parameters parameters)
        {
            if (!iterator.MoveNext())
                return string.Empty;

            var expression = iterator.Current;

            switch (expression)
            {
                case BinaryExpression binaryExpression:
                    return BinaryExpressionString(translator, iterator, binaryExpression, parameters);
                case MemberExpression memberExpression:
                    return MemberExpressionString(translator, memberExpression, parameters);
                case ConstantExpression constantExpression:
                    return ConstantExpressionString(translator, constantExpression, parameters);
                case MethodCallExpression methodCallExpression:
                    if (GetSqlExtensionMethodCallConstant(translator, methodCallExpression, parameters, out var constantValue))
                        return constantValue;
                    break;
            }

            return string.Empty;
        }

        private static string MemberExpressionString(ITranslator translator, MemberExpression memberExpression, Parameters parameters)
        {
            if (memberExpression.Expression is ConstantExpression)
                return GetStringForExpression(translator, memberExpression, parameters);

            var tableDefinition = new TableDefinition(memberExpression.Member.DeclaringType);

            var member = (PropertyInfo)memberExpression.Member;
            var field = tableDefinition.Fields.FirstOrDefault(f => f.PropertyInfo.Name == member.Name);
            
            return string.Concat(tableDefinition.Name, ".", field?.ToString() ?? member.Name);
        }

        private static string BinaryExpressionString(ITranslator translator, List<Expression>.Enumerator iterator, Expression binaryExpression, Parameters parameters)
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

        private static string ConstantExpressionString(ITranslator translator, ConstantExpression constantExpression, Parameters parameters)
        {
            var value = GetValueFrom(translator, constantExpression.Type, constantExpression.Value);
            return parameters.Add(translator, value);
        }

        public static object GetValueFrom(ITranslator translator, Type type, object value)
        {
            if (value == null)
                return null;

            switch (type.Name)
            {
                case "Boolean":
                    return translator.GetBooleanValue(value);
                case "String":
                    return translator.GetStringValue(value);
                case "DateTime":
                    return translator.GetDateTimeValue(value);
                default:
                    return Convert.ToInt32(value);
            }
        }

        private static IEnumerable<Expression> GetMemberExpressions(ITranslator translator, Expression body)
        {
            var candidates = new Queue<Expression>();
            candidates.Enqueue(body);
            while (candidates.Count > 0)
            {
                var expr = candidates.Dequeue();
                switch (expr)
                {
                    case MemberExpression _:
                        yield return expr;
                        break;
                    case ConstantExpression _:
                        yield return expr;
                        break;
                    case UnaryExpression unary:
                        if (unary.NodeType == ExpressionType.Not)
                        {
                            if (unary.Operand is BinaryExpression binaryExpression)
                            {
                                var isEqualOp = unary.Operand.NodeType == ExpressionType.Equal;
                                candidates.Enqueue(Expression.MakeBinary(isEqualOp ? ExpressionType.NotEqual : ExpressionType.Equal, binaryExpression.Left, binaryExpression.Right));

                                continue;
                            }
                        }

                        candidates.Enqueue(unary.Operand);
                        break;
                    case BinaryExpression binary:
                        candidates.Enqueue(binary.Left);
                        candidates.Enqueue(binary.Right);

                        yield return expr;
                        break;
                    case LambdaExpression _:
                        candidates.Enqueue(((LambdaExpression)expr).Body);
                        break;
                    case MethodCallExpression _:
                        yield return expr;
                        break;
                }
            }
        }

        public static string FormatStringFromArguments<T>(ITranslator translator, Expression<Func<T, dynamic>> clause,
            string startString, Parameters parameters, string format = "{0}")
        {
            var returnString = startString;

            var arguments = GetExpressionsFromDynamic(clause);

            if (arguments == null || arguments.Length <= 0)
                return returnString;
            
            if (!string.IsNullOrEmpty(returnString))
                returnString += ", ";

            returnString += string.Join(", ",
                arguments.Select(f => string.Format(format, GetStringForExpression(translator, f, parameters))));

            return returnString;
        }

        private static string GetStringForExpression(ITranslator translator, Expression expression, Parameters parameters)
        {
            switch (expression)
            {
                case MemberExpression memberExpression:
                {
                    if (memberExpression.Expression is ConstantExpression constantExpression)
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

                    var tableDefinition = new TableDefinition(memberExpression.Member.DeclaringType);

                    return string.Concat(tableDefinition.Name, ".", memberExpression.Member.Name);
                }
                case ConstantExpression constExpr:
                {
                    var value = GetValueFrom(translator, constExpr.Type, constExpr.Value);
                    var key = parameters.Add(translator, value);
                    return key;
                }
                case MethodCallExpression methodExpression:
                {
                    if (GetSqlExtensionMethodCallConstant(translator, methodExpression, parameters, out var concat))
                        return concat;
                    break;
                }
                case NewArrayExpression newArrayExpression:
                {
                    return string.Join(",", newArrayExpression.Expressions.Select(ex => GetStringForExpression(translator, ex, parameters)));
                }
            }

            return string.Empty;
        }

        public static bool GetSqlExtensionMethodCallConstant(ITranslator translator, MethodCallExpression methodExpression, Parameters parameters, out string concat)
        {
            if (methodExpression.Method.ReflectedType == null ||
                methodExpression.Method.ReflectedType.Name != "SqlExtensions")
            {
                concat = "";
                return false;
            }
            
            switch (methodExpression.Method.Name)
            {
                case "Min":
                    concat = string.Concat("MIN(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), ")");
                    return true;
                case "Max":
                    concat = string.Concat("MAX(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), ")");
                    return true;
                case "Sum":
                    concat = string.Concat("SUM(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), ")");
                    return true;
                case "Count":
                    concat = string.Concat("COUNT(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), ")");
                    return true;
                case "As":
                    var aliasFormat = translator.GetAliasFormat();
                    var alias = string.Format(aliasFormat,
                        Convert.ToString(methodExpression.Arguments[1]).Replace("\"", "").Replace("'", ""));

                    concat = string.Concat(GetStringForExpression(translator, methodExpression.Arguments[0], parameters), " AS ", alias);
                    return true;
                case "Between":
                    concat = string.Concat("(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), " BETWEEN ",
                        GetStringForExpression(translator, methodExpression.Arguments[1], parameters), " AND ",
                        GetStringForExpression(translator, methodExpression.Arguments[2], parameters), ")");
                    return true;
                case "In":
                    concat = string.Concat("(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), " IN (",
                        GetStringForExpression(translator, methodExpression.Arguments[1], parameters), "))");
                    return true;
                case "Like":
                    concat = string.Concat("(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), " LIKE ",
                        GetStringForExpression(translator, methodExpression.Arguments[1], parameters), ")");
                    return true;
                case "TrimSql":
                    concat = string.Concat("LTRIM(RTRIM(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), "))");
                    return true;
                case "Length":
                    var lengthName = translator.GetLengthFunctionName();
                    concat = string.Concat(lengthName, "(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), ")");
                    return true;
                case "Upper":
                    concat = string.Concat("UPPER(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), ")");
                    return true;
                case "Lower":
                    concat = string.Concat("LOWER(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), ")");
                    return true;
                case "IsNull":
                    var isNullName = translator.GetIsNullFunctionName();
                    concat = string.Concat(isNullName, "(",
                        GetStringForExpression(translator, methodExpression.Arguments[0], parameters), ",",
                        GetStringForExpression(translator, methodExpression.Arguments[1], parameters), ")");
                    return true;
                case "Cast":
                    var returnDbType = TableDefinition.GetTypeForProperty(methodExpression.Method.ReturnType);
                    concat = string.Concat("CAST(",
                        GetStringForExpression(translator, methodExpression.Arguments[0], parameters), " AS ",
                        translator.GetTextFor(new FieldDefinition { Type = returnDbType }, true), ")");
                    return true;
                case "Average":
                    concat = string.Concat("AVG(", GetStringForExpression(translator, methodExpression.Arguments[0], parameters), ")");
                    return true;
            }

            concat = "";
            return false;
        }

        public static IEnumerable<string> GetStringsFromArguments<T>(Expression<Func<T, dynamic>> clause)
        {
            var arguments = GetExpressionsFromDynamic(clause);
            if (arguments != null && arguments.Length > 0)
                return arguments.Select(f => ((MemberExpression)f).Member.Name);

            return Enumerable.Empty<string>();
        }
    }
}
