namespace Calendar.Core.Exceptions;

public class InvalidAppointmentException : Exception
{
    public InvalidAppointmentException(string message) : base(message) { }
}

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message) { }
}

public class DuplicateUsernameException : Exception
{
    public DuplicateUsernameException(string message) : base(message) { }
}

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

