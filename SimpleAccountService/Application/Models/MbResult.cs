namespace Simple_Account_Service.Application.Models;

public class MbResult<T>
{
    public bool Success { get; set; }

    public T? Response { get; set; } = default;

    public MbError? Error { get; set; } = null;

    public MbResult(T response)
    {
        Success = true;
        Response = response;
    }

    public MbResult(MbError error)
    {
        Success = false;
        Error = error;
    }
}


public class MbError
{
    public string Code { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string Description { get; set; } = string.Empty;

    // TODO выделить потом на класс, мб сделать список ошибок
}