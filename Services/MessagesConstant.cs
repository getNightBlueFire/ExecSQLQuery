using static System.Text.Encoding;

namespace ExecSQLQueryInfoField.Services
{
    public class MessagesConstant
    {
        public readonly static string EMPTY_FORMULA = "Пустая формула для вычисляемой ячейки";
        public readonly static string NOT_FOUND_ARGUMENT = "Не найден аргумент вычисляемой ячейки:";
        public readonly static string METHOD_NOT_SUPPERTED = "Метод неизвестен или не поддерживается:";
        public readonly static string ERROR_INFO_FIELD_EXEC = "Ошибка выполнения вычисляемой функции";

        public static string GetInUTF8(string errorMessage)
        {
            var res = UTF8.GetString(Convert(Unicode, UTF8, Unicode.GetBytes(errorMessage)));
            return res;
        }


    }
}
