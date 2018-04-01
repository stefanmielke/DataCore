﻿using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DataCore
{
    public class QueryVisitor : ExpressionVisitor
    {
        public Parameters Parameters { get; }

        public QueryVisitor(Parameters parameters)
        {
            Parameters = parameters;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var expression = Visit(memberExpression.Expression);
            if (expression is ConstantExpression constantExpression)
            {
                var container = constantExpression.Value;
                var member = memberExpression.Member;

                var fieldInfo = member as FieldInfo;
                if (fieldInfo != null)
                {
                    var value = fieldInfo.GetValue(container);
                    return Expression.Constant(value);
                }
            }

            var memberPropertyInfo = memberExpression.Member as PropertyInfo;
            if (memberPropertyInfo != null && memberPropertyInfo.PropertyType.Name == "Boolean")
            {
                return Expression.MakeBinary(ExpressionType.Equal, memberExpression, Expression.Constant(true));
            }

            return base.VisitMember(memberExpression);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var args = node.Arguments.Select(a => ((ConstantExpression)a).Value).ToArray();

            var newObject = node.Constructor.Invoke(args);

            return Expression.Constant(newObject);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (ExpressionHelper.GetSqlExtensionMethodCallConstant(new Translator(), node, Parameters, out _))
                return node;

            var obj = ((ConstantExpression) node.Object)?.Value;
            var args = node.Arguments.Select(
                a =>
                {
                    ConstantExpression constExpr;

                    if (a is MemberExpression memberExpr)
                        constExpr = VisitMember(memberExpr) as ConstantExpression;
                    else
                        constExpr = a as ConstantExpression;

                    return constExpr?.Value;
                }
            ).ToArray();

            var newObject = node.Method.Invoke(obj, args);

            return Expression.Constant(newObject);
        }
    }
}
