using System;
using System.Linq;
using RnD.Common.Enums;
using RnD.Model;
using RnD.Model.EF;
using SinExecSQLQuery.Exceptions;

namespace SinExecSQLQuery.Handlers
{
    /// <summary>
    /// Обработчик обращения к методу.
    /// </summary>
    public class MethodHandler : IHandler
    {
        private readonly IMethod _aMethod;
        private readonly RnDConnection _dataContext;

        public MethodHandler(IMethod aMethod, RnDConnection dataContext)
        {
            _aMethod = aMethod;
            _dataContext = dataContext;
        }

        public object Execute(string methodName, string argument = null)
        {
            return methodName switch
            {
                "Cell" => ExecuteCell(argument),
                "Attribute" => ExecuteAttribute(argument),
                "Property" => ExecuteProperty(argument),
                _ => throw new NotSupportedException($"Метод {methodName} неизвестен или не поддерживается.")
            };
        }

        /// <summary>
        /// Обращение к ячейке текущего метода.
        /// </summary>
        /// <param name="argument">Аргумент запроса(ShortDescription).</param>
        private object ExecuteCell(string argument = null)
        {
            var targetCell = _aMethod.MethodCells.First(x => x.ShortDescription == argument);
            return (object) targetCell.ValueS ?? targetCell.ValueR;
        }

        /// <summary>
        /// Обращение к атрибуту текущего метода.
        /// </summary>
        /// <param name="argument">Аргумент запроса(ShortDescription).</param>
        private object ExecuteAttribute(string argument)
        {
            var attribute = _aMethod.Attributes.FirstOrDefault(x => x.ShortDescription == argument);

            if (attribute != null)
                return attribute.Value;

            var au = EnumExtensions.FirstOrDefault(_dataContext.RndvAu, x => x.SHORT_DESC == argument);

            if (au == null)
                throw new NotFoundException($"Атрибут {argument} не найден или не существует");

            var rndvScMeAu = _dataContext
                .RndvScMeAu.Local.FirstOrDefault(x => x.AU == au.AU
                                                      && x.SC == _aMethod.Sample
                                                      && x.ME == _aMethod.Method
                                                      && x.MENODE == _aMethod.MethodNode
                                                      && x.PG == _aMethod.ParameterGroup
                                                      && x.PGNODE == _aMethod.ParameterGroupNode);
            return rndvScMeAu?.VALUE;
        }

        /// <summary>
        /// Обращение к свойству текущего метода.
        /// </summary>
        /// <param name="argument">Аргумент запроса(название столбца из таблицы RndtScMe).</param>
        private object ExecuteProperty(string argument)
        {
            var rndvScMe = _dataContext
                .RndvScMe.Local.FirstOrDefault(x => x.SC == _aMethod.Sample
                                                      && x.ME == _aMethod.Method
                                                      && x.MENODE == _aMethod.MethodNode
                                                      && x.PG == _aMethod.ParameterGroup
                                                      && x.PGNODE == _aMethod.ParameterGroupNode);

            if (rndvScMe == null)
                return null;

            var type = rndvScMe.GetType();
            var prop = type.GetProperty(argument);

            if (prop == null)
                throw new NotFoundException($"Свойство {argument} не найдено.");

            var value = prop.GetValue(rndvScMe);

            return value;
        }
    }
}
