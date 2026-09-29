namespace PolicyPlatform.Application.Common;

/// <summary>Thrown for request-shape problems that should surface as HTTP 400.</summary>
public class AppValidationException(string message) : Exception(message);
