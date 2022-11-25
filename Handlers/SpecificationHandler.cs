using RnD.API;
using RnD.Common.Enums;
using RnD.Model;
using RnD.Model.EF;
using ExecSQLQueryInfoField.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExecSQLQueryInfoField.Services;

namespace ExecSQLQueryInfoField.Handlers
{
    public class SpecificationHandler : IHandler
    {
        private readonly RnDConnection _dataContext;
        private readonly IAPI _aApi;
        private readonly IInfoCard _infoCard;
        private readonly IInfoField _infoField;

        public SpecificationHandler(RnDConnection dataContext, IAPI aApi, IInfoCard infoCard, IInfoField info)
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
                _ => throw new NotSupportedException(MessagesConstant.METHOD_NOT_SUPPERTED+argument)
            };
        }

        /// <summary>
        /// Обращение к инфополю current specification.
        /// </summary>
        /// <param name="argument">Аргумент запроса(ShortDescription).</param>
        private object ExecuteInfoField(string argument)
        {
            var specification = (ISpecification)_infoCard.ParentInstance;

            var infoField = specification.InfoCards
                .SelectMany(x => x.InfoFields)
                .FirstOrDefault(x => x.ShortDescription == argument);
            if (infoField == null)
                throw new NotFoundException($"Атрибут {argument} не найден.", argument);
            return infoField.InfoFieldValueF ?? (object)infoField.InfoFieldValue;
        }

        /// <summary>
        /// Обращение к атрибуту current specification.
        /// </summary>
        /// <param name="argument">Аргумент запроса(ShortDescription).</param>
        private object ExecuteAttribute(string argument)
        {
            var specification = (ISpecification)_infoCard.ParentInstance;
            var attribute = specification.Attributes.FirstOrDefault(x => x.ShortDescription == argument);

            if (attribute == null)
                throw new NotFoundException($"Атрибут {argument} не найден.", argument);

            return attribute.Value;
        }

        /// <summary>
        /// Обращение к свойству current specification.
        /// </summary>
        /// <param name="argument">Аргумент запроса(название столбца из таблицы RndtSc).</param>
        private object ExecuteProperty(string argument)
        {
            var specification = (ISpecification)_infoCard.ParentInstance;
            var rndvSc = _dataContext
                .RndvSp.Local.FirstOrDefault(x => x.SP_VERSION == specification.Version
                                                  && x.SP_VALUE == specification.SpecificationValue);
            if (rndvSc == null)
            {
                rndvSc = EnumExtensions.FirstOrDefault(_dataContext.RndvSp, x => x.SP_VERSION == specification.Version
                                                  && x.SP_VALUE == specification.SpecificationValue);
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
