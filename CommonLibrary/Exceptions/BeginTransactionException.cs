using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary.Exceptions
{
    public class BeginTransactionException : ApplicationException
    {
        public BeginTransactionException() 
            : base()
        {
        }
        public BeginTransactionException(string message) 
            : base(message)
        {
        }
        public BeginTransactionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


    }
}
