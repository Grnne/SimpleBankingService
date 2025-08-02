namespace Simple_Account_Service.Application.Models;

public class MbResult<T>
{
    public bool Success { get; }

    public T? Response { get; }

    public MbError? Error { get; }

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