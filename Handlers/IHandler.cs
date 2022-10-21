namespace SinExecSQLQueryInfoField.Handlers
{
    /// <summary>
    /// Интерфейс для обработки частей запроса.
    /// </summary>
    public interface IHandler
    {
        object Execute(string methodName, string argument = null);
    }
}
