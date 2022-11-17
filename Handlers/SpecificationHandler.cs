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
                _ => throw new NotSupportedException($"\u041c\u0435\u0442\u043e\u0434\u0020{methodName}\u0020\u043d\u0435\u0438\u0437\u0432\u0435\u0441\u0442\u0435\u043d\u0020\u0438\u043b\u0438\u0020\u043d\u0435\u0020\u043f\u043e\u0434\u0434\u0435\u0440\u0436\u0438\u0432\u0430\u0435\u0442\u0441\u044f\u002e")
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
                .First(x => x.ShortDescription == argument);
            if (infoField == null)
                throw new NotFoundException($"Атрибут {argument} не найден.");
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
                throw new NotFoundException($"Атрибут {argument} не найден.");

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
                throw new NotFoundException($"Свойство {argument} не найдено.");

            var value = prop.GetValue(rndvSc);

            return value;
        }
    }
}
