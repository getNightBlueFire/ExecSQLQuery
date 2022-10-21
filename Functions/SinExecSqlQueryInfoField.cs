﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using RnD;
using RnD.API;
using RnD.Attributes;
using RnD.CustomFunctionTypes;
using RnD.Model;
using SinExecSQLQueryInfoField.Handlers;
using SinExecSQLQueryInfoField.Services;
using ValueType = RnD.ValueType;

namespace SinExecSQLQueryInfoField.Functions
{
    /// <summary>
    /// Функция, позволяющая выполнять произвольный SQL-запрос и использовать результат этого запроса в качестве значения ячейки.
    /// </summary>
    [CustomFunction("SinExecSqlQueryInfoField", RnD.Common.Enums.CustomFunctionType.InfoFieldCalculationFunction)]
    public class SinExecSqlQueryInfoField : AdvancedCustomFunction, IStringInfoFieldCustomCalculationFuncion
    {
        private readonly List<string> _badWords = new List<string> {
            "CREATE", "DROP", "UPDATE", "INSERT", "ALTER", "DELETE", "ATTACH", "DETACH"
        };

        public SinExecSqlQueryInfoField(IAPI aAPI) : base(aAPI)
        {
        }

        public string Execute(List<object> aArguments, IInfoCard aInfoCard, IInfoField aCalculatedInfoField)
        {
            if (aArguments.Count < 0)
                throw new ArgumentException();

            if (!(aArguments[0] is string sqlQuery))
                throw new ArgumentException();

            sqlQuery = "";
            sqlQuery = SubstituteParams(sqlQuery, aInfoCard, aCalculatedInfoField);

            if (sqlQuery == null)
            {
                var sqlResult = ExecuteSql(sqlQuery);
                return sqlResult.ValueType.ToString();
            }
            return "X";
            
            

        }

        private string SubstituteParams(string sqlQuery, IInfoCard aInfoCard, IInfoField aCalculatedInfoField)
        {
            var sampleHandler = new SampleHandler(DatabaseContext, API, aInfoCard, aCalculatedInfoField);
            var specificationHandler = new SpecificationHandler(DatabaseContext, API, aInfoCard, aCalculatedInfoField);
            var userHandler = new UserHandler(DatabaseContext, API, aInfoCard, aCalculatedInfoField);
            var requestHandler = new RequestHandler(DatabaseContext, API, aInfoCard, aCalculatedInfoField);


            var pattern = new Regex("([\\w]+)\\.([\\w]+)(?:\\[([\\-\\s\\w]+)\\])?");
            var matches = pattern.Matches(sqlQuery);

            foreach (Match match in matches)
            {
                IHandler handler = match.Groups[1].Value switch
                {
                    "Request" => requestHandler,
                    "Sample" => sampleHandler,
                    "Specification" => specificationHandler,
                    "User" => userHandler,
                    _ => throw new NotSupportedException($"Класс {match.Groups[1].Value} неизвестен или не поддерживается.")
                };

                var value = handler.Execute(match.Groups[2].Value, match.Groups[3].Value);

                if (value == null)
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