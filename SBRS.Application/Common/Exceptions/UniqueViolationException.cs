namespace SBRS.Application.Common.Exceptions;

/// <summary>
/// Thrown by the infrastructure layer when a save fails with a database UNIQUE-constraint
/// violation. Used by handlers to detect a lost race (e.g. two admins creating the same
/// facility code at the same moment) and retry, without coupling the Application layer
/// to the database provider (Npgsql).
/// </summary>
public sealed class UniqueViolationException(string message, Exception? innerException = null)
    : Exception(message, innerException);
