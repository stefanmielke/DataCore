using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DataCore
{
    public class Query<T>
    {
        private readonly ITranslator _translator;

        public string TableName { get; private set; }

        public bool Built { get; private set; }

        public string SqlSelectFormat { get; set; }
        public string SqlColumns { get; set; }
        public string SqlFrom { get; set; }
        public string SqlWhere { get; set; }
        public StringBuilder SqlCommand { get; private set; }

        public Query(ITranslator translator)
        {
            Built = false;

            _translator = translator;
            SqlSelectFormat = "{0}";
            SqlWhere = string.Empty;
            SqlCommand = new StringBuilder();
            SqlColumns = "*";
            TableName = typeof(T).Name;
            SqlFrom = TableName;
        }

        public Query<T> Build()
        {
            // run the query
            SqlCommand.Append("SELECT ");
            SqlCommand.AppendFormat(SqlSelectFormat, SqlColumns);
            SqlCommand.Append(" FROM ");
            SqlCommand.Append(SqlFrom);

            var where = SqlWhere;
            if (!string.IsNullOrWhiteSpace(where))
                SqlCommand.AppendFormat(" WHERE {0}", where);

            Built = true;

            return this;
        }

        public Query<T> Select(Expression<Func<T, dynamic>> clause)
        {
            SqlColumns = string.Join(", ", 
                            ((NewExpression)clause.Body).Arguments
                                .Select(f => string.Concat(((MemberExpression)f).Member.DeclaringType.Name, ".", ((MemberExpression)f).Member.Name)));

            return this;
        }

        public Query<T> Top(int count)
        {
            _translator.Top(this, count);

            return this;
        }

        public Query<T> Where(Expression<Func<T, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));

            var query = GetQueryFromExpression(newExpression.Body);

            SqlWhere = string.IsNullOrEmpty(SqlWhere) ? query : string.Concat("(", SqlWhere, ") AND (", query, ")");

            return this;
        }

        public Query<T> Or(Expression<Func<T, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));

            var query = GetQueryFromExpression(newExpression.Body);

            SqlWhere = string.IsNullOrEmpty(SqlWhere) ? query : string.Concat("(", SqlWhere, ") OR (", query, ")");

            return this;
        }

        public Query<T> Join<TJoined>(Expression<Func<T, TJoined, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));
            var query = GetQueryFromExpression(newExpression.Body);

            var joinedTableName = typeof(TJoined).Name;
            SqlFrom = string.Concat(SqlFrom, " INNER JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> Join<TJoinedLeft, TJoinedRight>(Expression<Func<TJoinedLeft, TJoinedRight, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));
            var query = GetQueryFromExpression(newExpression.Body);

            var joinedTableName = typeof(TJoinedRight).Name;

            SqlFrom = string.Concat(SqlFrom, " INNER JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> LeftJoin<TJoined>(Expression<Func<T, TJoined, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));
            var query = GetQueryFromExpression(newExpression.Body);

            var joinedTableName = typeof(TJoined).Name;
            SqlFrom = string.Concat(SqlFrom, " LEFT JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> LeftJoin<TJoinedLeft, TJoinedRight>(Expression<Func<TJoinedLeft, TJoinedRight, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));
            var query = GetQueryFromExpression(newExpression.Body);

            var joinedTableName = typeof(TJoinedRight).Name;
            SqlFrom = string.Concat(SqlFrom, " LEFT JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> RightJoin<TJoined>(Expression<Func<T, TJoined, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));
            var query = GetQueryFromExpression(newExpression.Body);

            var joinedTableName = typeof(TJoined).Name;
            SqlFrom = string.Concat(SqlFrom, " RIGHT JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> RightJoin<TJoinedLeft, TJoinedRight>(Expression<Func<TJoinedLeft, TJoinedRight, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));
            var query = GetQueryFromExpression(newExpression.Body);

            var joinedTableName = typeof(TJoinedRight).Name;
            SqlFrom = string.Concat(SqlFrom, " RIGHT JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        private string GetQueryFromExpression(Expression clause)
        {
            var members = GetMemberExpressions(clause);
            if (!members.Any())
                return string.Empty;

            var enumerator = members.GetEnumerator();
            return GetString(enumerator);
        }

        private string GetString(IEnumerator<Expression> iterator)
        {
            if (!iterator.MoveNext())
                return string.Empty;

            var expression = iterator.Current;

            var binaryExpression = expression as BinaryExpression;
            if (binaryExpression != null)
                return BinaryExpressionString(iterator, binaryExpression);

            var memberExpression = expression as MemberExpression;
            if (memberExpression != null)
                return MemberExpressionString(memberExpression);

            var constantExpression = expression as ConstantExpression;
            if (constantExpression != null)
                return ConstantExpressionString(constantExpression);

            return string.Empty;
        }

        private static string MemberExpressionString(MemberExpression memberExpression)
        {
            return string.Concat(memberExpression.Member.DeclaringType.Name, ".", memberExpression.Member.Name);
        }

        private string BinaryExpressionString(IEnumerator<Expression> iterator, BinaryExpression binaryExpression)
        {
            var format = _translator.GetFormatFor(binaryExpression.NodeType);

            var left = string.Empty;
            var right = string.Empty;

            if (iterator.MoveNext())
                left = GetQueryFromExpression(iterator.Current);

            if (iterator.MoveNext())
                right = GetQueryFromExpression(iterator.Current);

            return string.Format(format, left, right);
        }

        private string ConstantExpressionString(ConstantExpression constantExpression)
        {
            switch (constantExpression.Type.Name)
            {
                case "String":
                    return _translator.GetStringValue(constantExpression.Value);
                case "DateTime":
                    {
                        var date = Convert.ToDateTime(constantExpression.Value);
                        return _translator.GetDateTimeValue(date);
                    }
                default:
                    return Convert.ToString(constantExpression.Value);
            }
        }

        private static IEnumerable<Expression> GetMemberExpressions(Expression body)
        {
            // A Queue preserves left to right reading order of expressions in the tree
            var candidates = new Queue<Expression>(new[] { body });
            while (candidates.Count > 0)
            {
                var expr = candidates.Dequeue();
                if (expr is MemberExpression || expr is ConstantExpression)
                {
                    yield return expr;
                }
                else if (expr is UnaryExpression)
                {
                    candidates.Enqueue(((UnaryExpression)expr).Operand);
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
            }
        }
    }
}
