using System;
using System.Linq;
using RnD.API;
using RnD.Common.Enums;
using RnD.Model;
using RnD.Model.EF;
using SinExecSQLQuery.Exceptions;

namespace SinExecSQLQuery.Handlers
{
    /// <summary>
    /// Обработчик обращения к пробе.
    /// </summary>
    public class SampleHandler : IHandler
    {
        private readonly IMethod _aMethod;
        private readonly RnDConnection _dataContext;
        private readonly IAPI _aApi;
        
        public SampleHandler(IMethod aMethod, RnDConnection dataContext, IAPI aApi)
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
        /// Обращение к инфополю текущей пробы.
        /// </summary>
        /// <param name="argument">Аргумент запроса(ShortDescription).</param>
        private object ExecuteInfoField(string argument)
        {
            var sample = _aApi.Sample.GetSample(_aMethod.Sample);
            var infoField = sample.InfoCards
                .SelectMany(x => x.InfoFields)
                .First(x => x.ShortDescription == argument);

            return infoField.InfoFieldValueF ?? (object)infoField.InfoFieldValue;
        }

        /// <summary>
        /// Обращение к атрибуту текущей пробы.
        /// </summary>
        /// <param name="argument">Аргумент запроса(ShortDescription).</param>
        private object ExecuteAttribute(string argument)
        {
            var sample = _aApi.Sample.GetSample(_aMethod.Sample);
            var attribute = sample.Attributes.FirstOrDefault(x => x.ShortDescription == argument);

            if (attribute == null)
                throw new NotFoundException($"Атрибут {argument} не найден.");

            return attribute.Value;
        }

        /// <summary>
        /// Обращение к свойству текущей пробы.
        /// </summary>
        /// <param name="argument">Аргумент запроса(название столбца из таблицы RndtSc).</param>
        private object ExecuteProperty(string argument)
        {
            var rndvSc = _dataContext
                .RndvSc.Local.FirstOrDefault(x => x.SC == _aMethod.Sample
                                                  && x.SC_VALUE == _aMethod.SampleShortDescription);
            if (rndvSc == null)
            {
                rndvSc = EnumExtensions.FirstOrDefault(_dataContext.RndvSc, x => x.SC == _aMethod.Sample
                    && x.SC_VALUE == _aMethod.SampleShortDescription);
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
