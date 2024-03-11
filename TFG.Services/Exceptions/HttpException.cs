using System.Net;

namespace TFG.Services.Exceptions;

public class HttpException(int code, string message) : Exception(message)
{
    public int Code { get; } = code;
}