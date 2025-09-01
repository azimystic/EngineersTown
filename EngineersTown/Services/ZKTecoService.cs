using zkemkeeper;

namespace EngineersTown.Services
{
    public class ZKTecoService : IZKTecoService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ZKTecoService> _logger;
        private CZKEMClass? _zkemClass;
        private bool _isConnected = false;

        private readonly string _deviceIP;
        private readonly int _port;
        private readonly int _machineNumber;
        private readonly int _connectionTimeout;

        public ZKTecoService(IConfiguration configuration, ILogger<ZKTecoService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _deviceIP = _configuration["ZKTecoSettings:DeviceIP"] ?? "192.168.0.201";
            _port = int.Parse(_configuration["ZKTecoSettings:Port"] ?? "4370");
            _machineNumber = int.Parse(_configuration["ZKTecoSettings:MachineNumber"] ?? "101");
            _connectionTimeout = int.Parse(_configuration["ZKTecoSettings:ConnectionTimeoutSeconds"] ?? "30");
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                _zkemClass = new CZKEMClass();

                await Task.Run(() =>
                {
                    _isConnected = _zkemClass.Connect_Net(_deviceIP, _port);
                });

                if (_isConnected)
                {
                    _logger.LogInformation("Successfully connected to ZKTeco device at {IP}:{Port}", _deviceIP, _port);
                }
                else
                {
                    int errorCode = 0;
                    _zkemClass?.GetLastError(ref errorCode);
                    _logger.LogWarning("Failed to connect to ZKTeco device. Error Code: {ErrorCode}", errorCode);
                }

                return _isConnected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while connecting to ZKTeco device");
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (_zkemClass != null && _isConnected)
                {
                    await Task.Run(() =>
                    {
                        _zkemClass.Disconnect();
                    });
                    _isConnected = false;
                    _logger.LogInformation("Disconnected from ZKTeco device");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while disconnecting from ZKTeco device");
            }
        }

        public async Task<List<AttendancePunch>> FetchNewAttendanceLogsAsync()
        {
            var punches = new List<AttendancePunch>();

            if (!_isConnected || _zkemClass == null)
            {
                _logger.LogWarning("Device not connected. Cannot fetch attendance logs.");
                return punches;
            }

            try
            {
                await Task.Run(() =>
                {
                    // Enable the device for reading attendance logs
                    _zkemClass.EnableDevice(_machineNumber, false);

                    // Read attendance logs
                    if (_zkemClass.ReadGeneralLogData(_machineNumber))
                    {
                        string employeeId = "";
                        int verifyMode = 0;
                        int inOutMode = 0;
                        int year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0, workCode = 0;

                        while (_zkemClass.SSR_GetGeneralLogData(_machineNumber, out employeeId, out verifyMode,
                               out inOutMode, out year, out month, out day, out hour, out minute, out second, ref workCode))
                        {
                            var punchTime = new DateTime(year, month, day, hour, minute, second);
                            var status = inOutMode == 0 ? "IN" : "OUT";

                            punches.Add(new AttendancePunch
                            {
                                EmployeeId = employeeId,
                                PunchTime = punchTime,
                                Status = status
                            });
                        }
                    }

                    // Re-enable the device
                    _zkemClass.EnableDevice(_machineNumber, true);
                });

                _logger.LogInformation("Fetched {Count} attendance punches from device", punches.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching attendance logs");
            }

            return punches;
        }

        public async Task<bool> IsConnectedAsync()
        {
            return await Task.FromResult(_isConnected);
        }

        public void Dispose()
        {
            DisconnectAsync().Wait();
        }
    }
}