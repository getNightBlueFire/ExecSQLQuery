using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using RnD;
using RnD.API;
using RnD.Attributes;
using RnD.CustomFunctionTypes;
using RnD.Model;
using SinExecSQLQuery.Handlers;
using SinExecSQLQuery.Services;
using ValueType = RnD.ValueType;

namespace SinExecSQLQuery.Functions
{
    /// <summary>
    /// Функция, позволяющая выполнять произвольный SQL-запрос и использовать результат этого запроса в качестве значения ячейки.
    /// </summary>
    [CustomFunction("SinExecSqlQuery", RnD.Common.Enums.CustomFunctionType.CalculationFunction)]
    public class SinExecSqlQuery : AdvancedCustomFunction, IMethodCellCustomCalculation
    {
        private readonly List<string> _badWords  = new List<string> {
            "CREATE", "DROP", "UPDATE", "INSERT", "ALTER", "DELETE", "ATTACH", "DETACH"
        };

        public SinExecSqlQuery(IAPI aAPI) : base(aAPI)
        {
        }

        public ValueType Execute(List<object> aArguments, IMethod aMethod, IMethodCell aCalculatedCell)
        {
            if (aArguments.Count < 0)
                throw new ArgumentException();

            if (!(aArguments[0] is string sqlQuery))
                throw new ArgumentException();

            try
            {
                sqlQuery = SubstituteParams(sqlQuery, aMethod);

                if (sqlQuery == null)
                    return EmptyValue.Instance;

                var sqlResult = ExecuteSql(sqlQuery);

                return sqlResult;
            }
            catch (Exception e)
            {
                Log.Error("Ошибка выполнения SinExecSqlQuery", e);
                MessageService.SendErrorMessage("Ошибка выполнения SinExecSqlQuery");
                return EmptyValue.Instance;
            }
        }

        /// <summary>
        /// Сопоставляет пришедшие параметры запроса.
        /// </summary>
        /// <param name="sqlQuery">Sql запрос</param>
        /// <param name="aMethod">Метод</param>
        private string SubstituteParams(string sqlQuery, IMethod aMethod)
        {
            var methodHandler = new MethodHandler(aMethod, DatabaseContext);
            var sampleHandler = new SampleHandler(aMethod, DatabaseContext, API);
            var userHandler = new UserHandler(API, DatabaseContext);
            var pattern = new Regex("([\\w]+)\\.([\\w]+)(?:\\[([\\-\\s\\w]+)\\])?");
            var matches = pattern.Matches(sqlQuery);

            foreach (Match match in matches)
            {
                IHandler handler = match.Groups[1].Value switch
                {
                    "Method" => methodHandler,
                    "Sample" => sampleHandler,
                    "User" => userHandler,
                    _ => throw new NotSupportedException($"Класс {match.Groups[1].Value} неизвестен или не поддерживается.")
                };

                var value = handler.Execute(match.Groups[2].Value, match.Groups[3].Value);

                if(value == null)
                    return null;
                
                string strValue;

                if (value is string str && !double.TryParse(str, out var _))
                    strValue = "'" + str + "'";
                else
                    strValue = value.ToString();
                
                sqlQuery = sqlQuery.Replace(match.Groups[0].Value, strValue);
            }
            return sqlQuery;
        }

        /// <summary>
        /// Выполняет Sql запрос
        /// </summary>
        /// <param name="sqlQuery">Sql запрос</param>
        private ValueType ExecuteSql(string sqlQuery)
        {
            if (_badWords.Any(sqlQuery.Contains))
                throw new Exception("Потенциально опасный запрос.");

            var connection = (SqlConnection)DatabaseContext.Database.Connection;
            var command = connection.CreateCommand();
            command.CommandText = sqlQuery;

            command.Transaction = DatabaseContext.Database.CurrentTransaction?.UnderlyingTransaction as SqlTransaction;

            try
            {
                var value = command.ExecuteScalar();

                if (value == null)
                    return EmptyValue.Instance;

                switch (value)
                {
                    case string strValue:
                        return new StringValue(strValue);
                    case bool boolValue:
                        return new BooleanValue(boolValue);
                    case int _:
                    case double _:
                    case float _:
                    case decimal _:
                        return new DecimalValue(Convert.ToDecimal(value));
                    default:
                        throw new Exception();
                }
            }
            catch (Exception e)
            {
                Log.Error($"Ошибка выполнения Sql запроса: {sqlQuery}", e);
                MessageService.SendErrorMessage("Ошибка выполнения Sql запроса");
                return EmptyValue.Instance;
            }
        }
    }
}
