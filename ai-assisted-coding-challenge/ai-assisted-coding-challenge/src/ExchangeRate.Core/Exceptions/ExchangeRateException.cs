namespace ExchangeRate.Core.Exceptions;

public class ExchangeRateException : Exception
{
    public ExchangeRateException(string message, Exception innerException = null) : base(message, innerException)
    {
    }
}
