namespace WorkflowManagement.Application.DTOs.Common;

public class ResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();

    // Static helper methods for creating responses
    public static ResponseDto<T> CreateSuccess(T data, string message = "")
    {
        return new ResponseDto<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = new List<string>()
        };
    }

    public static ResponseDto<T> CreateSuccess(string message = "")
    {
        return new ResponseDto<T>
        {
            Success = true,
            Message = message,
            Data = default,
            Errors = new List<string>()
        };
    }

    public static ResponseDto<T> SuccessResult(T data, string message = "")
    {
        return new ResponseDto<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = new List<string>()
        };
    }

    public static ResponseDto<T> SuccessResult(string message = "")
    {
        return new ResponseDto<T>
        {
            Success = true,
            Message = message,
            Data = default,
            Errors = new List<string>()
        };
    }

    public static ResponseDto<T> Failure(string message, IEnumerable<string>? errors = null)
    {
        return new ResponseDto<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors ?? new List<string>()
        };
    }

    public static ResponseDto<T> Failure(string message, params string[] errors)
    {
        return new ResponseDto<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors.ToList()
        };
    }

    public static ResponseDto<T> Failure(IEnumerable<string> errors)
    {
        return new ResponseDto<T>
        {
            Success = false,
            Message = "Operation failed",
            Data = default,
            Errors = errors
        };
    }
}

public class ResponseDto : ResponseDto<object?>
{
    // Additional static methods for non-generic ResponseDto
    public static ResponseDto CreateSuccess(string message = "Operation completed successfully")
    {
        return new ResponseDto
        {
            Success = true,
            Message = message,
            Data = null,
            Errors = new List<string>()
        };
    }

    public static ResponseDto CreateSuccess(object data, string message = "Operation completed successfully")
    {
        return new ResponseDto
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = new List<string>()
        };
    }

    public new static ResponseDto Failure(string message, IEnumerable<string>? errors = null)
    {
        return new ResponseDto
        {
            Success = false,
            Message = message,
            Data = null,
            Errors = errors ?? new List<string>()
        };
    }

    public new static ResponseDto Failure(string message, params string[] errors)
    {
        return new ResponseDto
        {
            Success = false,
            Message = message,
            Data = null,
            Errors = errors.ToList()
        };
    }
}