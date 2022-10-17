using System;
using RnD.API;
using RnD.Common.Enums;
using RnD.Model.EF;
using SinExecSQLQuery.Exceptions;

namespace SinExecSQLQuery.Handlers
{
    /// <summary>
    /// Обработчик обращения к языку данных пользователя.
    /// </summary>
    public class UserHandler : IHandler
    {
        private readonly IAPI _aAPI;
        private readonly RnDConnection _dataContext;

        public UserHandler(IAPI aAPI, RnDConnection dataContext)
        {
            _aAPI = aAPI;
            _dataContext = dataContext;
        }

        public object Execute(string methodName, string argument = null)
        {
            if (methodName != "LanguageId")
                throw new NotSupportedException($"Метод {methodName} неизвестен или не поддерживается.");

            var userId = _aAPI.CustomizationSession.User.ID;
            var dataLangId = _dataContext.RndvUsPref
                .FirstOrDefault(x => x.US == userId && x.PREF_NAME == "DataLanguage")
                .PREF_VALUE;

            if (dataLangId == null)
                throw new NotFoundException("Идентификатор языка данных не найден.");

            return dataLangId;
        }
    }
}
