using System;

namespace Monolypix.Models;

public class Result<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static Result<T> Successful(T data, string message = "")
    {
        return new Result<T> { Success = true, Data = data, Message = message };
    }
    public static Result<T> Failure(string message)
    {
        return new Result<T> { Success = false, Message = message };
    }
}
