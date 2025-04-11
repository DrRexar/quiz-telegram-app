using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuizTelegramApp.Data
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IDbContextFactory _dbContextFactory;

        public DatabaseHealthCheck(IDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var dbContext = _dbContextFactory.CreateDbContext();
                var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
                
                if (canConnect)
                {
                    return HealthCheckResult.Healthy("База данных доступна");
                }
                
                return HealthCheckResult.Unhealthy("Не удалось подключиться к базе данных");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Ошибка при проверке базы данных", ex);
            }
        }
    }
} 