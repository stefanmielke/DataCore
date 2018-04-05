using System.Linq.Expressions;
using System.Reflection;

namespace DataCore
{
    public class QueryVisitor : ExpressionVisitor
    {
        private readonly Parameters _parameters;

        public QueryVisitor(Parameters parameters)
        {
            _parameters = parameters;
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
            var args = new object[node.Arguments.Count];
            for (var i = 0; i < args.Length; i++)
            {
                args[i] = ((ConstantExpression)node.Arguments[i]).Value;
            }
            
            var newObject = node.Constructor.Invoke(args);

            return Expression.Constant(newObject);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (ExpressionHelper.GetSqlExtensionMethodCallConstant(new Translator(), node, _parameters, out _))
                return node;

            var obj = ((ConstantExpression) node.Object)?.Value;
            
            var args = new object[node.Arguments.Count];
            for (var i = 0; i < args.Length; i++)
            {
                ConstantExpression constExpr;

                if (node.Arguments[i] is MemberExpression memberExpr)
                    constExpr = VisitMember(memberExpr) as ConstantExpression;
                else
                    constExpr = node.Arguments[i] as ConstantExpression;

                args[i] = constExpr?.Value;
            }

            var newObject = node.Method.Invoke(obj, args);

            return Expression.Constant(newObject);
        }
    }
}
