using EngineersTown.Services;

namespace EngineersTown.Services
{
    public class AttendanceBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AttendanceBackgroundService> _logger;

        private readonly TimeSpan _pollingInterval;

        public AttendanceBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration,
            ILogger<AttendanceBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
            _logger = logger;

            var pollingMinutes = int.Parse(_configuration["ZKTecoSettings:PollingIntervalMinutes"] ?? "5");
            _pollingInterval = TimeSpan.FromMinutes(pollingMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Attendance Background Service started. Polling interval: {Interval} minutes", _pollingInterval.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAttendanceAsync();
                    await Task.Delay(_pollingInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in attendance background service");

                    // Wait a shorter time before retrying on error
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("Attendance Background Service stopped");
        }

        private async Task ProcessAttendanceAsync()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var zkTecoService = scope.ServiceProvider.GetRequiredService<IZKTecoService>();
            var attendanceService = scope.ServiceProvider.GetRequiredService<IAttendanceService>();

            try
            {
                // Check if device is connected
                bool isConnected = await zkTecoService.IsConnectedAsync();

                if (!isConnected)
                {
                    _logger.LogInformation("Device not connected. Attempting to connect...");
                    isConnected = await zkTecoService.ConnectAsync();
                }

                if (!isConnected)
                {
                    _logger.LogWarning("Failed to connect to ZKTeco device. Skipping this cycle.");
                    return;
                }

                // Fetch new attendance logs
                var punches = await zkTecoService.FetchNewAttendanceLogsAsync();

                if (punches.Any())
                {
                    _logger.LogInformation("Fetched {Count} new attendance punches", punches.Count);

                    // Save each punch to database
                    foreach (var punch in punches)
                    {
                        await attendanceService.SaveAttendanceLogAsync(
                            punch.EmployeeId,
                            punch.PunchTime,
                            punch.Status);
                    }

                    // Process daily attendance calculations
                    await attendanceService.ProcessAttendanceLogsAsync(DateTime.Today);

                    _logger.LogInformation("Successfully processed {Count} attendance punches", punches.Count);
                }
                else
                {
                    _logger.LogDebug("No new attendance punches found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing attendance in background service");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stopping Attendance Background Service...");

            // Disconnect from device when stopping
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var zkTecoService = scope.ServiceProvider.GetRequiredService<IZKTecoService>();
                await zkTecoService.DisconnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from device during service stop");
            }

            await base.StopAsync(stoppingToken);
        }
    }
}