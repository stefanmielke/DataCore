using System;
using System.Linq.Expressions;

namespace DataCore
{
    public interface ITranslator
    {
        void Top<T>(Query<T> query, int count);

        string GetFormatFor(ExpressionType type);

        string GetStringValue(object value);
        string GetDateTimeValue(DateTime date);
    }
}