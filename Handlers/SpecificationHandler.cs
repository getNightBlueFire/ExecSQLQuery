using RnD.API;
using RnD.Common.Enums;
using RnD.Model;
using RnD.Model.EF;
using SinExecSQLQuery.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinExecSQLQuery.Handlers
{
    public class SpecificationHandler : IHandler
    {
        private readonly IMethod _aMethod;
        private readonly RnDConnection _dataContext;
        private readonly IAPI _aApi;

        public SpecificationHandler(IMethod aMethod, RnDConnection dataContext, IAPI aApi)
        {
            _aMethod = aMethod;
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
                _ => throw new NotSupportedException($"Метод {methodName} неизвестен или не поддерживается.")
            };
        }

        /// <summary>
        /// Обращение к инфополю current specification.
        /// </summary>
        /// <param name="argument">Аргумент запроса(ShortDescription).</param>
        private object ExecuteInfoField(string argument)
        {
            var specification = _aApi.Specification.GetSpecification(_aMethod.Description);
            var infoField = specification.InfoCards
                .SelectMany(x => x.InfoFields)
                .First(x => x.ShortDescription == argument);

            return infoField.InfoFieldValueF ?? (object)infoField.InfoFieldValue;
        }

        /// <summary>
        /// Обращение к атрибуту current specification.
        /// </summary>
        /// <param name="argument">Аргумент запроса(ShortDescription).</param>
        private object ExecuteAttribute(string argument)
        {
            var specification = _aApi.Specification.GetSpecification(_aMethod.Description);
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
            var rndvSc = _dataContext
                .RndvSp.Local.FirstOrDefault(x => x.SP == _aMethod.Sample
                                                  && x.SP_VALUE == _aMethod.SampleShortDescription);
            if (rndvSc == null)
            {
                rndvSc = EnumExtensions.FirstOrDefault(_dataContext.RndvSp, x => x.SP == _aMethod.Sample
                    && x.SP_VALUE == _aMethod.SampleShortDescription);
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
