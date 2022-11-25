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
using ExecSQLQueryInfoField.Handlers;
using ExecSQLQueryInfoField.Services;
using ValueType = RnD.ValueType;
using System.Text;
using NLog;
using ExecSQLQueryInfoField.Exceptions;
using NLog.Targets;
using static ExecSQLQueryInfoField.Services.MessagesConstant;

namespace ExecSQLQueryInfoField.Functions
{
	/// <summary>
	/// Функция, позволяющая выполнять произвольный SQL-запрос и использовать результат этого запроса в качестве значения ячейки.
	/// </summary>
	[CustomFunction("ExecSqlQueryInfoField", RnD.Common.Enums.CustomFunctionType.InfoFieldCalculationFunction)]
	public class ExecSqlQueryInfoField : AdvancedCustomFunction, IStringInfoFieldCustomCalculationFuncion
	{
		private readonly List<string> _badWords = new List<string> {
			"CREATE", "DROP", "UPDATE", "INSERT", "ALTER", "DELETE", "ATTACH", "DETACH"
		};

		public readonly ILogger log = LogManager.GetCurrentClassLogger();
		
		private readonly IAPI api;
		private readonly string username;
		private readonly string db;
		private readonly Guid guid;

		public ExecSqlQueryInfoField(IAPI aAPI) : base(aAPI)
		{
			api = aAPI;
			db = api.CustomizationSession.ConnectionName;
			username = api.CustomizationSession.User.UserName;
			guid = api.CustomizationSession.BrowserGuid;
		}

		public string Execute(List<object> aArguments, IInfoCard aInfoCard, IInfoField aCalculatedInfoField)
		{
			try
			{
				if (aArguments.Count < 0)
					throw new ArgumentException();

				if (!(aArguments[0] is string sqlQuery))
					throw new ArgumentException();
				sqlQuery = SubstituteParams(sqlQuery, aInfoCard, aCalculatedInfoField);
				if (sqlQuery == null)
					return String.Empty;

				sqlQuery = sqlQuery.Replace("#", "'")
					.Replace("SELECT"+ " '", "SELECT N'")
					.Replace("SELECT '","SELECT N'");
				var sqlResult = ExecuteSql(sqlQuery, username);

				return sqlResult;
			}
			catch (ArgumentException e)
			{
				var exc = GetInUTF8(EMPTY_FORMULA);
				log.Error(e, exc);
				MessageService.SendErrorMessage(exc, username, db, guid);
				return String.Empty;
			}
			catch (NotFoundException e)
			{
				var exc = GetInUTF8(NOT_FOUND_ARGUMENT + e.Argument);
				log.Error(e, exc);
				MessageService.SendErrorMessage(exc, username, db, guid);
				return String.Empty;
			}
			catch (InvalidOperationException e)
			{
				var exc = GetInUTF8(METHOD_NOT_SUPPERTED);
				log.Error(e, exc);
				MessageService.SendErrorMessage(exc, username, db, guid);
				return String.Empty;
			}
			catch (Exception e)
			{
				var exc = GetInUTF8(ERROR_INFO_FIELD_EXEC);
				log.Error(e, exc);
				MessageService.SendErrorMessage(exc, username, db, guid);
				return String.Empty;
			}

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
		public string ExecuteSql(string sqlQuery, string username)
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
					return String.Empty;

				switch (value)
				{
					case string strValue:
						return strValue;
					case bool boolValue:
						return boolValue.ToString();
					case int _:
					case double _:
					case float _:
					case decimal _:
						return value.ToString();
					default:
						throw new Exception();
				}
			}
			catch (Exception e)
			{
				var exc = GetInUTF8("Ошибка выполнения Sql запроса");
				log.Error(e, exc);
				MessageService.SendErrorMessage(exc, username, db, guid);
				return String.Empty;
			}
		}

	}
}
