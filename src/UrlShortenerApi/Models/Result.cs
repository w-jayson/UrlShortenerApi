// Tipo auxiliar do padrão Result para representar sucesso ou erro de negócio sem usar exceções de fluxo.
using System.Diagnostics.CodeAnalysis;

namespace UrlShortenerApi.Models;

// Categorias de erro usadas pelo controller para escolher o status HTTP do ProblemDetails.
public enum ErrorType
{
    Validation,
    NotFound,
    Unexpected
}

public sealed record Result<T>
{
    private Result(T? value, ErrorType? errorType, string? errorMessage)
    {
        Value = value;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
    }

    public T? Value { get; }
    public ErrorType? ErrorType { get; }
    public string? ErrorMessage { get; }

    // Garante ao compilador (Nullable) que Value existe quando IsSuccess é true, e ErrorType/ErrorMessage existem quando é false.
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(ErrorType), nameof(ErrorMessage))]
    public bool IsSuccess => ErrorType is null;

    public static Result<T> Success(T value) => new(value, null, null);

    public static Result<T> Failure(ErrorType errorType, string errorMessage) =>
        new(default, errorType, errorMessage);
}
