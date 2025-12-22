using System.Data;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace HotelBooking.Data.Infrastructure.Dapper;

public sealed class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseOptions _options;
    public MySqlConnectionFactory(IOptions<DatabaseOptions> options) => _options = options.Value;

    public IDbConnection Create()
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
            throw new InvalidOperationException("DatabaseOptions.ConnectionString is not set");

        return new MySqlConnection(_options.ConnectionString);
    }
}
