namespace CapitalPos.Cpe.Api.Dtos;

public class ApiResponse<T>
{
    public bool Ok { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errores { get; set; } = new();

    public static ApiResponse<T> Success(string mensaje, T? data)
    {
        return new ApiResponse<T>
        {
            Ok = true,
            Mensaje = mensaje,
            Data = data,
            Errores = new List<string>()
        };
    }

    public static ApiResponse<T> Fail(string mensaje, List<string> errores)
    {
        return new ApiResponse<T>
        {
            Ok = false,
            Mensaje = mensaje,
            Data = default,
            Errores = errores
        };
    }

    public static ApiResponse<T> Fail(string mensaje, string error)
    {
        return new ApiResponse<T>
        {
            Ok = false,
            Mensaje = mensaje,
            Data = default,
            Errores = new List<string> { error }
        };
    }
}