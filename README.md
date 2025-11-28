# EngineersTown

A comprehensive Employee Management and Attendance Tracking System built with ASP.NET Core 8.0. This application integrates with ZKTeco biometric devices for automated attendance logging and provides complete HR management features including payroll processing.

## Features

### 👥 Employee Management
- **Multi-type Employee Support**: Regular (001), Contract (002), and Daily Wager (003) employees
- **Complete CRUD Operations**: Create, read, update, and soft-delete employees
- **Employee Details**: Name, CNIC, Date of Birth, Department, Designation, BPS (Basic Pay Scale)
- **Contract Tracking**: Contract expiry date management for contract employees

### 📊 Attendance Tracking
- **Biometric Integration**: Seamless integration with ZKTeco fingerprint devices
- **Automated Data Sync**: Background service for polling attendance data from devices
- **Real-time Dashboard**: View daily attendance by department
- **Status Tracking**: Present, Late, Absent status with configurable thresholds

### 🏢 Organization Management
- **Departments**: Create and manage organizational departments
- **Designations**: Role-based designation management linked to departments
- **Hierarchical Structure**: Departments contain designations with employee assignments

### 💰 Payroll Processing
- **Salary Definitions**: Comprehensive salary structure for all employee types
  - **Regular Employees**: Basic Salary, HRA, Conveyance, Medical, Gun, Supplementary, Wash, Adhoc, SRA allowances
  - **Contract Employees**: Lump Sum Amount with various allowances
  - **Daily Wage Employees**: Per-day wage calculation
- **Automatic Deductions**: EPF (8.33% of Basic), Income Tax (5% of Gross), EOBI, Mess, and other deductions
- **Monthly Reports**: Generate and view payroll reports by month/year
- **Absence Deductions**: Automatic calculation based on attendance

### 📅 Calendar & Holiday Management
- **Office Timings**: Configure working hours for each day of the week
- **Off Days**: Set weekly off days (default: Saturday, Sunday)
- **Custom Holidays**: Add/remove custom holidays affecting attendance calculations
- **Monthly Calendar View**: Visual calendar with holiday indicators

### 📈 Reports
- **Attendance Reports**: Daily and monthly attendance summaries
- **Payroll Reports**: Detailed monthly payroll with department-wise grouping
- **Print-ready Formats**: Optimized layouts for printing

## Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server with Entity Framework Core
- **Styling**: TailwindCSS
- **Device Integration**: ZKTeco SDK (COM Interop)
- **Background Services**: IHostedService for attendance polling

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (Express or higher)
- [Node.js](https://nodejs.org/) (for TailwindCSS compilation)
- ZKTeco Biometric Device (optional, for attendance integration)

## Installation

### 1. Clone the Repository
```bash
git clone https://github.com/azimystic/EngineersTown.git
cd EngineersTown
```

### 2. Configure Database Connection
Update the connection string in `EngineersTown/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=EngineersTownDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### 3. Install .NET Dependencies
```bash
cd EngineersTown
dotnet restore
```

### 4. Install Node.js Dependencies (for TailwindCSS)
```bash
cd EngineersTown
npm install
```

### 5. Build TailwindCSS
```bash
npm run build-css
```

### 6. Apply Database Migrations
The application automatically applies migrations on startup (configured in `Program.cs`). To apply migrations manually instead:
```bash
dotnet ef database update
```
> **Note**: Automatic migrations are enabled for development convenience. For production deployments, consider disabling auto-migrations and applying them manually during deployment.

### 7. ZKTeco SDK Setup (Optional)
If using ZKTeco biometric devices:
1. Navigate to the `EngineersTown` folder
2. Run `Register_ZKTeco.bat` **as Administrator**
3. Configure device settings in `appsettings.json`:
```json
{
  "ZKTecoSettings": {
    "DeviceIP": "192.168.0.201",
    "Port": 4370,
    "MachineNumber": 101,
    "PollingIntervalMinutes": 60,
    "ConnectionTimeoutSeconds": 30
  }
}
```

### 8. Run the Application
```bash
dotnet run
```
The application will start and automatically open your default browser. By default, it runs on `http://localhost:5000`. The actual URL may vary based on your configuration in `Properties/launchSettings.json`.

## Configuration

### Application Settings (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=EngineersTownDB;..."
  },
  "ZKTecoSettings": {
    "DeviceIP": "192.168.0.201",
    "Port": 4370,
    "MachineNumber": 101,
    "PollingIntervalMinutes": 60,
    "ConnectionTimeoutSeconds": 30
  },
  "AttendanceRules": {
    "LateThresholdMinutes": 20,
    "EarlyExitThresholdMinutes": 20,
    "MinimumWorkingHours": 7
  }
}
```

### Configuration Options

| Setting | Description | Default |
|---------|-------------|---------|
| `DeviceIP` | IP address of the ZKTeco device | `192.168.0.201` |
| `Port` | Communication port | `4370` |
| `MachineNumber` | Device machine number | `101` |
| `PollingIntervalMinutes` | How often to poll for new attendance data | `60` |
| `LateThresholdMinutes` | Minutes after start time to mark as late | `20` |
| `EarlyExitThresholdMinutes` | Minutes before end time to mark as early exit | `20` |
| `MinimumWorkingHours` | Minimum hours required for full day | `7` |

## Project Structure

```
EngineersTown/
├── Controllers/
│   ├── AttendanceController.cs      # Attendance management
│   ├── AttendanceReportController.cs # Attendance reporting
│   ├── CalendarController.cs        # Holiday management
│   ├── DepartmentController.cs      # Department CRUD
│   ├── DesignationController.cs     # Designation CRUD
│   ├── EmployeeController.cs        # Employee CRUD
│   ├── HomeController.cs            # Dashboard
│   ├── OfficeTimingController.cs    # Office hours configuration
│   ├── PayrollReportsController.cs  # Payroll generation
│   └── SalaryDefinitionController.cs # Salary structure
├── Models/
│   ├── AttendanceLog.cs             # Raw attendance punch data
│   ├── CalendarEvent.cs             # Holiday/event model
│   ├── DailyAttendance.cs           # Processed daily attendance
│   ├── Department.cs                # Department model
│   ├── Designation.cs               # Designation model
│   ├── Employee.cs                  # Employee model
│   ├── OfficeTiming.cs              # Office hours model
│   ├── PayrollReport.cs             # Monthly payroll snapshot
│   └── SalaryDefinition.cs          # Employee salary structure
├── Services/
│   ├── AttendanceBackgroundService.cs # Background polling service
│   ├── AttendanceService.cs         # Attendance processing logic
│   ├── IAttendanceService.cs        # Attendance service interface
│   ├── IZKTecoService.cs            # ZKTeco service interface
│   └── ZKTecoService.cs             # ZKTeco device communication
├── Views/                           # Razor views for each controller
├── ViewModels/                      # View-specific models
├── wwwroot/                         # Static files (CSS, JS, images)
├── ZKTeco_SDK/                      # ZKTeco DLL dependencies
├── ApplicationDbContext.cs          # EF Core database context
├── Program.cs                       # Application entry point
└── appsettings.json                 # Application configuration
```

## Usage Guide

### Getting Started

1. **Set Up Departments**: Navigate to Departments and create your organizational departments
2. **Create Designations**: Add designations/roles within each department
3. **Add Employees**: Register employees with their department, designation, and employment type
4. **Define Salaries**: Set up salary structures for each employee
5. **Configure Office Hours**: Set working hours and off days in Office Timing

### Daily Operations

1. **Attendance Tracking**: The system automatically syncs attendance from biometric devices
2. **Manual Processing**: Use "Process Attendance" button if needed
3. **View Attendance**: Check daily attendance by department on the main dashboard

### Monthly Operations

1. **Generate Payroll**: Go to Payroll Reports → Generate for the desired month
2. **Review Reports**: View detailed payroll breakdown by employee type
3. **Print Reports**: Use the print function for physical records

## Employee Types

| Type Code | Type | Salary Components |
|-----------|------|-------------------|
| 001 | Regular | Basic + HRA + Conveyance + Medical + Gun + Supplementary + Wash + Adhoc + SRA |
| 002 | Contract | Lump Sum + House Rent + Conveyance + Gun + Misc |
| 003 | Daily Wager | Daily Wage × Days Worked |

## Database Schema

The application uses the following main entities:

- **Employees**: Core employee information
- **Departments**: Organizational units
- **Designations**: Job titles/roles
- **AttendanceLogs**: Raw punch data from devices
- **DailyAttendances**: Processed daily attendance records
- **OfficeTimings**: Working hours configuration
- **CalendarEvents**: Holidays and special events
- **SalaryDefinitions**: Employee salary structures
- **PayrollReports**: Monthly payroll snapshots

## Development

### Building CSS
```bash
# Development (with watch)
npm run watch-css

# Production (minified)
npm run build-css
```

### Database Migrations
```bash
# Add a new migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update
```

## Troubleshooting

### ZKTeco SDK Issues
- Ensure `Register_ZKTeco.bat` is run as Administrator
- Verify all DLLs are present in `ZKTeco_SDK` folder
- Check device IP and port settings
- See `README_SDK.txt` for detailed SDK setup instructions

### Database Connection Issues
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure the SQL Server instance allows TCP/IP connections

### TailwindCSS Not Loading
- Run `npm install` to install dependencies
- Run `npm run build-css` to generate the CSS file
- Check that `wwwroot/css/tailwind.css` exists

## License

This project is proprietary software. All rights reserved.

## Support

For support and questions, please contact the system administrator or create an issue in the repository.
