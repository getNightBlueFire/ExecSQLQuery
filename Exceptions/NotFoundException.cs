using System;

namespace ExecSQLQueryInfoField.Exceptions
{
    public class NotFoundException : Exception
    {
        public string Argument { get; }
        public NotFoundException(string message, string arg) : base(message)
        {
            this.Argument = arg;
        }
    }
}
