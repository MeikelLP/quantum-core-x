namespace QuantumCore.Game;

public class ParserException : Exception
{
    public ParserException(string message) : base(message)
    {
    }
    
    public ParserException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class MissingRequiredFieldException : ParserException
{
    public string Field { get; init; }
    
    public MissingRequiredFieldException(string field) : base($"Missing required field: {field}")
    {
        Field = field;
    }
    
    public MissingRequiredFieldException(string field, string message) : base(message)
    {
        Field = field;
    }
    
    public MissingRequiredFieldException(string field, string message, Exception innerException) : base(message, innerException)
    {
        Field = field;
    }
}
