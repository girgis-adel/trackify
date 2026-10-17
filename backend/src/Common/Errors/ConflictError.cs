using FluentResults;

namespace Common.Errors;

public sealed class ConflictError(string message) : Error(message);
