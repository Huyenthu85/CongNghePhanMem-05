namespace ThuyetMinh.Business.Services;

public class AuthException : Exception
{
    public AuthException(string m) : base(m) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string m) : base(m) { }
}

public class BusinessException : Exception
{
    public BusinessException(string m) : base(m) { }
}
