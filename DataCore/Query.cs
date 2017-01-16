using System;
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
        public string SqlOrderBy { get; set; }
        public string SqlHaving { get; set; }
        public string SqlGroupBy { get; set; }
        public string SqlEnd { get; set; }
        public StringBuilder SqlCommand { get; private set; }

        public Query(ITranslator translator)
        {
            Built = false;

            _translator = translator;
            SqlSelectFormat = "{0}";
            SqlWhere = string.Empty;
            SqlCommand = new StringBuilder();
            SqlColumns = "*";
            SqlGroupBy = "";
            SqlOrderBy = "";
            SqlEnd = "";
            TableName = typeof(T).Name;
            SqlFrom = _translator.GetTableName(TableName);
        }

        public Query<T> Build()
        {
            // run the query
            SqlCommand.Append("SELECT ");
            SqlCommand.AppendFormat(SqlSelectFormat, SqlColumns);
            SqlCommand.Append(" FROM ");
            SqlCommand.Append(SqlFrom);

            if (!string.IsNullOrWhiteSpace(SqlWhere))
                SqlCommand.AppendFormat(" WHERE {0}", SqlWhere);

            if (!string.IsNullOrWhiteSpace(SqlGroupBy))
                SqlCommand.AppendFormat(" GROUP BY {0}", SqlGroupBy);

            if (!string.IsNullOrWhiteSpace(SqlHaving))
                SqlCommand.AppendFormat(" HAVING {0}", SqlHaving);

            if (!string.IsNullOrWhiteSpace(SqlOrderBy))
                SqlCommand.AppendFormat(" ORDER BY {0}", SqlOrderBy);

            if (!string.IsNullOrWhiteSpace(SqlEnd))
                SqlCommand.Append(" ").Append(SqlEnd);

            Built = true;

            return this;
        }

        public Query<T> Select(Expression<Func<T, dynamic>> clause)
        {
            if (SqlColumns == "*")
                SqlColumns = string.Empty;

            SqlColumns = ExpressionHelper.FormatStringFromArguments(_translator, clause, SqlColumns);

            return this;
        }

        public Query<T> Top(int count)
        {
            _translator.Top(this, count);

            return this;
        }

        public Query<T> Count()
        {
            _translator.Count(this);

            return this;
        }

        public Query<T> Where(Expression<Func<T, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));

            var query = ExpressionHelper.GetQueryFromExpression(_translator, newExpression.Body);

            SqlWhere = string.IsNullOrEmpty(SqlWhere) ? query : string.Concat("(", SqlWhere, ") AND (", query, ")");

            return this;
        }

        public Query<T> Or(Expression<Func<T, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));

            var query = ExpressionHelper.GetQueryFromExpression(_translator, newExpression.Body);

            SqlWhere = string.IsNullOrEmpty(SqlWhere) ? query : string.Concat("(", SqlWhere, ") OR (", query, ")");

            return this;
        }

        public Query<T> Join<TJoined>(Expression<Func<T, TJoined, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));
            var query = ExpressionHelper.GetQueryFromExpression(_translator, newExpression.Body);

            var joinedTableName = _translator.GetTableName(typeof(TJoined).Name);
            SqlFrom = string.Concat(SqlFrom, " INNER JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> Join<TJoinedLeft, TJoinedRight>(Expression<Func<TJoinedLeft, TJoinedRight, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));
            var query = ExpressionHelper.GetQueryFromExpression(_translator, newExpression.Body);

            var joinedTableName = _translator.GetTableName(typeof(TJoinedRight).Name);
            SqlFrom = string.Concat(SqlFrom, " INNER JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> LeftJoin<TJoined>(Expression<Func<T, TJoined, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));
            var query = ExpressionHelper.GetQueryFromExpression(_translator, newExpression.Body);

            var joinedTableName = _translator.GetTableName(typeof(TJoined).Name);
            SqlFrom = string.Concat(SqlFrom, " LEFT JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> LeftJoin<TJoinedLeft, TJoinedRight>(Expression<Func<TJoinedLeft, TJoinedRight, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));
            var query = ExpressionHelper.GetQueryFromExpression(_translator, newExpression.Body);

            var joinedTableName = _translator.GetTableName(typeof(TJoinedRight).Name);
            SqlFrom = string.Concat(SqlFrom, " LEFT JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> RightJoin<TJoined>(Expression<Func<T, TJoined, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));
            var query = ExpressionHelper.GetQueryFromExpression(_translator, newExpression.Body);

            var joinedTableName = _translator.GetTableName(typeof(TJoined).Name);
            SqlFrom = string.Concat(SqlFrom, " RIGHT JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> RightJoin<TJoinedLeft, TJoinedRight>(Expression<Func<TJoinedLeft, TJoinedRight, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));
            var query = ExpressionHelper.GetQueryFromExpression(_translator, newExpression.Body);

            var joinedTableName = _translator.GetTableName(typeof(TJoinedRight).Name);
            SqlFrom = string.Concat(SqlFrom, " RIGHT JOIN ", joinedTableName, " ON ", query);

            return this;
        }

        public Query<T> OrderBy(Expression<Func<T, dynamic>> clause)
        {
            SqlOrderBy = ExpressionHelper.FormatStringFromArguments(_translator, clause, SqlOrderBy);

            return this;
        }

        public Query<T> OrderByDescending(Expression<Func<T, dynamic>> clause)
        {
            SqlOrderBy = ExpressionHelper.FormatStringFromArguments(_translator, clause, SqlOrderBy, _translator.GetOrderByDescendingFormat());

            return this;
        }

        public Query<T> Having(Expression<Func<T, bool>> clause)
        {
            var newExpression = Expression.Lambda(new QueryVisitor().Visit(clause));

            var query = ExpressionHelper.GetQueryFromExpression(_translator, newExpression.Body);

            SqlHaving = string.IsNullOrEmpty(SqlHaving) ? query : string.Concat("(", SqlHaving, ") AND (", query, ")");

            return this;
        }

        public Query<T> GroupBy(Expression<Func<T, dynamic>> clause)
        {
            SqlGroupBy = ExpressionHelper.FormatStringFromArguments(_translator, clause, SqlGroupBy);

            return this;
        }

        public Query<T> Paginate(int recordsPerPage, int currentPage)
        {
            _translator.Paginate(this, recordsPerPage, currentPage);

            return this;
        }
    }
}
