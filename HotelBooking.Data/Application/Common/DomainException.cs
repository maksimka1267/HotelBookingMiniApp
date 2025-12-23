namespace HotelBooking.Data.Application.Common;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
