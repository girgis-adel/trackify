using FluentResults;

namespace Common.Errors;

public sealed class NotFoundError(string message) : Error(message);
