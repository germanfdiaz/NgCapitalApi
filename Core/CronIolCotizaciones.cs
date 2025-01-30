namespace NgCapitalApi.Core
{
    public class CronIolCotizaciones : IHostedService, IDisposable
    {
        private readonly ILogger<CronIolCotizaciones> _logger;
        private Timer _timer;

        public CronIolCotizaciones(ILogger<CronIolCotizaciones> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            //_logger = logger;
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            if (IsWorkingHours())
            {
                // Aquí va la lógica de tu proceso
                _logger.LogInformation("Ejecutando tarea programada");
                Console.WriteLine("Ejecutando tarea programada");

                // ... tu código aquí ...
            }
        }

        private bool IsWorkingHours()
        {
            // Lógica para determinar si estamos dentro del horario laboral
            var now = DateTime.Now;
            return now.DayOfWeek >= DayOfWeek.Monday &&
                    now.DayOfWeek <= DayOfWeek.Friday &&
                    now.Hour >= 11 &&
                    now.Hour < 17;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
