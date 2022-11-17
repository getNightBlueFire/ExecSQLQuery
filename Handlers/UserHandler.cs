using System;
using RnD.API;
using RnD.Common.Enums;
using RnD.Model;
using RnD.Model.EF;
using ExecSQLQueryInfoField.Exceptions;

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
                throw new NotSupportedException($"\u041c\u0435\u0442\u043e\u0434\u0020{methodName}\u0020\u043d\u0435\u0438\u0437\u0432\u0435\u0441\u0442\u0435\u043d\u0020\u0438\u043b\u0438\u0020\u043d\u0435\u0020\u043f\u043e\u0434\u0434\u0435\u0440\u0436\u0438\u0432\u0430\u0435\u0442\u0441\u044f\u002e");

            var userId = _aApi.CustomizationSession.User.ID;
            var dataLangId = _dataContext.RndvUsPref
                .FirstOrDefault(x => x.US == userId && x.PREF_NAME == "DataLanguage")
                .PREF_VALUE;

            if (dataLangId == null)
                throw new NotFoundException("\u0418\u0434\u0435\u043d\u0442\u0438\u0444\u0438\u043a\u0430\u0442\u043e\u0440\u0020\u044f\u0437\u044b\u043a\u0430\u0020\u0434\u0430\u043d\u043d\u044b\u0445\u0020\u043d\u0435\u0020\u043d\u0430\u0439\u0434\u0435\u043d\u002e");

            return dataLangId;
        }
    }
}
