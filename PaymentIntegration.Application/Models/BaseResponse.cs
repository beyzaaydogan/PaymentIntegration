namespace PaymentIntegration.API.Models;

public class BaseResponse<T>
{
    public bool Success { get; set; }
    public string Error { get; set; }
    public T Data { get; set; }

    public static BaseResponse<T> Ok(T data) => new BaseResponse<T> { Success = true, Data = data };
    public static BaseResponse<T> Fail(string error) => new BaseResponse<T> { Success = false, Error = error };
}
