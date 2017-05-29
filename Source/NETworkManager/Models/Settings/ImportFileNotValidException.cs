using System;

namespace NETworkManager.Models.Settings
{
    public class ImportFileNotValidException : Exception
    {
        public ImportFileNotValidException()
        {

        }

        public ImportFileNotValidException(string message) : base(message)
        {

        }

        public ImportFileNotValidException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
