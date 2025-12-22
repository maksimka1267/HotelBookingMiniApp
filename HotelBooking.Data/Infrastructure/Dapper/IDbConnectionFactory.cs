using System.Data;

namespace HotelBooking.Data.Infrastructure.Dapper;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}
