using System;
using System.Linq;
using RnD.API;
using RnD.Common.Enums;
using RnD.Model;
using RnD.Model.EF;
using ExecSQLQueryInfoField.Exceptions;
using ExecSQLQueryInfoField.Services;

namespace ExecSQLQueryInfoField.Handlers
{
    /// <summary>
    /// Обработчик обращения к пробе.
    /// </summary>
    public class SampleHandler : IHandler
    {
        private readonly RnDConnection _dataContext;
        private readonly IAPI _aApi;
        private readonly IInfoCard _infoCard;
        private readonly IInfoField _infoField;
        
        public SampleHandler(RnDConnection dataContext, IAPI aApi, IInfoCard infoCard, IInfoField info)
        {
            _infoCard = infoCard;
            _infoField = info;
            _dataContext = dataContext;
            _aApi = aApi;
        }

        public object Execute(string methodName, string argument = null)
        {
            return methodName switch
            {
                "InfoField" => ExecuteInfoField(argument),
                "Attribute" => ExecuteAttribute(argument),
                "Property" => ExecuteProperty(argument),
                _ => throw new NotSupportedException(MessagesConstant.METHOD_NOT_SUPPERTED)
            };
        }

        /// <summary>
        /// Обращение к инфополю текущей пробы.
        /// </summary>
        /// <param name="argument">Аргумент запроса(ShortDescription).</param>
        private object ExecuteInfoField(string argument)
        {
            var sample = (ISample)_infoCard.ParentInstance;
            var infoField = sample.InfoCards
                .SelectMany(x => x.InfoFields)
                .FirstOrDefault(x => x.ShortDescription == argument);
            if (infoField == null)
                throw new NotFoundException($"Атрибут {argument} не найден.", argument);
            return infoField.InfoFieldValueF ?? (object)infoField.InfoFieldValue;
        }

        /// <summary>
        /// Обращение к атрибуту текущей пробы.
        /// </summary>
        /// <param name="argument">Аргумент запроса(ShortDescription).</param>
        private object ExecuteAttribute(string argument)
        {
            var sample = (ISample)_infoCard.ParentInstance;
            var attribute = sample.Attributes.FirstOrDefault(x => x.ShortDescription == argument);

            if (attribute == null)
                throw new NotFoundException($"Атрибут {argument} не найден.", argument);

            return attribute.Value;
        }

        /// <summary>
        /// Обращение к свойству текущей пробы.
        /// </summary>
        /// <param name="argument">Аргумент запроса(название столбца из таблицы RndtSc).</param>
        private object ExecuteProperty(string argument)
        {
            var sample = (ISample)_infoCard.ParentInstance;
            var rndvSc = _dataContext
                .RndvSc.Local.FirstOrDefault(x => x.SC ==sample.ID);
            if (rndvSc == null)
            {
                rndvSc = EnumExtensions.FirstOrDefault(_dataContext.RndvSc, x => x.SC == sample.ID);
            }

            if (rndvSc == null)
                return null;

            var type = rndvSc.GetType();
            var prop = type.GetProperty(argument);

            if (prop == null)
                throw new NotFoundException($"Свойство {argument} не найдено.", argument);

            var value = prop.GetValue(rndvSc);

            return value;
        }
    }
}
