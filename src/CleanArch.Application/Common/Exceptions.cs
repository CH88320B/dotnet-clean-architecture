namespace CleanArch.Application.Common;

public sealed class NotFoundException(string resource, object key)
    : Exception($"{resource} '{key}' was not found.");

public sealed class ConflictException(string message) : Exception(message);
