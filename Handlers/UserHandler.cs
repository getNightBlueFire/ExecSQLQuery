using System;
using RnD.API;
using RnD.Common.Enums;
using RnD.Model;
using RnD.Model.EF;
using ExecSQLQueryInfoField.Exceptions;
using ExecSQLQueryInfoField.Services;

namespace ExecSQLQueryInfoField.Handlers
{
    /// <summary>
    /// Обработчик обращения к языку данных пользователя.
    /// </summary>
    public class UserHandler : IHandler
    {
        private readonly RnDConnection _dataContext;
        private readonly IAPI _aApi;
        private readonly IInfoCard _infoCard;
        private readonly IInfoField _infoField;

        public UserHandler(RnDConnection dataContext, IAPI aApi, IInfoCard infoCard, IInfoField info)
        {
            _infoCard = infoCard;
            _infoField = info;
            _dataContext = dataContext;
            _aApi = aApi;
        }

        public object Execute(string methodName, string argument = null)
        {
            if (methodName != "LanguageId")
                throw new NotSupportedException(MessagesConstant.METHOD_NOT_SUPPERTED+argument);

            var userId = _aApi.CustomizationSession.User.ID;
            var dataLangId = _dataContext.RndvUsPref
                .FirstOrDefault(x => x.US == userId && x.PREF_NAME == "DataLanguage")
                .PREF_VALUE;

            if (dataLangId == null)
                throw new NotSupportedException(MessagesConstant.METHOD_NOT_SUPPERTED);

            return dataLangId;
        }
    }
}
