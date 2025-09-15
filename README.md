# Trading Simulator

A real-time stock price simulation system built with .NET 8, featuring WebAPI, SignalR, TCP server, and plugin architecture for data formatting.

## Features

- **Real-time Stock Price Updates**: Simulates live stock price changes for major stocks (AAPL, MSFT, GOOGL, TSLA, AMZN)
- **Multiple Communication Protocols**:
  - REST API endpoints for stock data retrieval
  - SignalR Hub for real-time price broadcasting
  - TCP Server for raw data streaming
- **Plugin Architecture**: Extensible data formatting system with JSON and CSV formatters
- **Health Monitoring**: Built-in health checks for system monitoring
- **Comprehensive Testing**: Unit and integration tests with high coverage
- **Docker Support**: Containerized deployment ready

## Architecture

### Solution Level Architecture

```
TradingSimulator Solution
├── TradingSimulator (Main Web API)
├── JsonFormatter.Plugin (JSON formatting plugin)
├── CsvFormatter.Plugin (CSV formatting plugin)
├── SignalRClient (SignalR client example)
├── TcpClient (TCP client example)
└── TradingSimulator.Tests (Test project)
```

### Project Level Architecture (Clean Architecture)

```
TradingSimulator/
├── Application/
│   ├── DTOs/                    # Data Transfer Objects
│   └── Services/                # Business Logic Services
├── Domain/
│   ├── Entities/                # Domain Models
│   └── Interfaces/              # Repository & Service Contracts
├── Infrastructure/
│   ├── Data/                    # Data Access Layer
│   ├── HealthChecks/            # Health Check Implementations
│   ├── Plugins/                 # Plugin Management
│   └── Repositories/            # Data Repository Implementations
└── Presentation/
    ├── Controllers/             # API Controllers
    └── Hubs/                    # SignalR Hubs
```

## Complete Project Flow

### 1. **Application Startup** (`Program.cs`)
```
1. Configure services (DI, SignalR, Swagger, CORS)
2. Register repositories and services
3. Add health checks
4. Load plugins (JSON/CSV formatters)
5. Start TCP server on port 8080
6. Initialize stock data with default prices
```

### 2. **Data Layer Flow**
```
Domain Entities (Stock, PriceHistory) 
    ↓
Repository Interface (IStockRepository)
    ↓
In-Memory Repository Implementation
    ↓
Stores 5 stocks: AAPL, MSFT, GOOGL, TSLA, AMZN
```

### 3. **Background Price Generation**
```
PriceUpdateService (Background Service)
    ↓
Every 2-5 seconds:
    - Generate random price changes (±2%)
    - Update stock prices in repository
    - Add to price history
    - Broadcast via SignalR
    - Send via TCP server
```

### 4. **Multi-Protocol Communication**

#### **REST API Flow:**
```
HTTP Request → StockController → StockPriceService → Repository → JSON Response
```

#### **SignalR Real-time Flow:**
```
Price Update → PriceHub → Broadcast to all connected clients → Real-time UI updates
```

#### **TCP Streaming Flow:**
```
Price Update → TcpServerService → Plugin Formatter (JSON/CSV) → Raw TCP stream
```

### 5. **Plugin Architecture**
```
IDataFormatter Interface
    ↓
JsonFormatter.Plugin (formats as JSON)
CsvFormatter.Plugin (formats as CSV)
    ↓
PluginManager loads at runtime
    ↓
Used by TCP server for data formatting
```

### 6. **Client Integration Points**

#### **Web Clients:**
- Connect to SignalR Hub (`/pricehub`)
- Receive real-time price updates
- Make REST API calls for historical data

#### **TCP Clients:**
- Connect to port 8080
- Receive formatted price streams (JSON/CSV)
- Process raw data for trading algorithms

#### **Health Monitoring:**
- `/health` endpoint checks:
  - Stock count (should be 5)
  - TCP server status
  - Plugin loading status

 
### 7. **Execution Sequence**
1. **Startup**: Load plugins → Start TCP server → Initialize stocks
2. **Runtime**: Background service generates prices every few seconds
3. **Distribution**: Same price data sent via 3 channels (REST, SignalR, TCP)
4. **Consumption**: Different clients consume data based on their needs
5. **Monitoring**: Health checks ensure all components are working

This architecture allows the stock price data to be consumed by different types of clients through Http communication protocol.

## Prerequisites

- **.NET 8 SDK** or later
- **Visual Studio 2022** or **VS Code** with C# extension
- **Git** for version control
- **Docker** (optional, for containerized deployment)

## API Details

### REST API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Health check endpoint |
| GET | `/api/stock/current` | Get all current stock prices |
| GET | `/api/stock/{symbol}/current` | Get current price for specific stock |
| GET | `/api/stock/{symbol}/history` | Get price history for specific stock |

### SignalR Hub

- **Hub URL**: `/pricehub`
- **Events**: 
  - `PriceUpdate`: Broadcasts real-time price changes
  - `JoinGroup`: Subscribe to specific stock updates
  - `LeaveGroup`: Unsubscribe from stock updates

### TCP Server

- **Port**: 8080
- **Protocol**: Raw TCP streaming of price data
- **Format**: Configurable via plugins (JSON/CSV)

## Setup Instructions

### 1. Clone the Repository

```bash
git clone <repository-url>
cd TradingSimulator
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Solution

```bash
dotnet build
```

### 4. Set Up Individual Projects

#### Main API Project
```bash
cd TradingSimulator
dotnet run
```
The API will be available at:
- HTTPS: `https://localhost:7150`
- HTTP: `https://localhost:5184`

#### Plugin Projects
Plugins are automatically built and copied to the main project's plugins folder during build.
 
## How to Run the Solution in local

### Option 1: Build Individual Projects and Run the REST API

1. **Build Individual Projects**:
```bash
# Build main API project
cd TradingSimulator
dotnet build

# Build plugin projects
cd ../JsonFormatter.Plugin
dotnet build

cd ../CsvFormatter.Plugin
dotnet build

# Build client projects
cd ../SignalRClient
dotnet build

cd ../TcpClient
dotnet build

# Build test project
cd ../TradingSimulator.Tests
dotnet build
```

2. **Run the Web Api Application**:
```bash
cd TradingSimulator
dotnet restore
# Debug mode, HTTP only (port 5184)
dotnet run
# Debug mode, HTTPS + HTTP (ports 7150 + 5184)  
dotnet run --launch-profile https
# Release mode, HTTP only (port 5184)
dotnet run -c Release
# Release mode, HTTPS + HTTP (ports 7150 + 5184)
dotnet run --launch-profile https -c Release
```

**Note**: 
- **dotnet run --launch-profile https**: makes the URL available on https://localhost:7150/swagger
- **dotnet run**: makes the URL available on http://localhost:5184/swagger

### Verify
- Open https://localhost:7150/swagger or http://localhost:5184/swagger 
- Test endpoint: `curl https://localhost:7150/api/stock/current` or `curl http://localhost:5184/api/stock/current`
- Open TradingSimulator.http file and check the port before clicking on send request.
 

### Option 2: Using Visual Studio

1. Set `TradingSimulator` as the startup project
2. Press F5 or click "Start Debugging"
3. Run client projects separately as needed

### Option 3: Using Docker

```bash
cd TradingSimulator
docker build -t trading-simulator .
docker run -p 7150:8080 trading-simulator
```

## How to Test

### Run Unit Tests

```bash
cd TradingSimulator.Tests
dotnet test
```

### Run Tests with Coverage

```bash
# Install coverlet.collector if missing
dotnet add package coverlet.collector

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults
```

### Generate HTML Coverage Report

**Option 1: Using ReportGenerator Tool**
```bash
# Install ReportGenerator tool
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report from XML coverage file
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:Html

# Open the report
start CoverageReport/index.html
```

**Option 2: Using Online Tool**
- Upload your `coverage.cobertura.xml` file to: https://reportgenerator.io/
- Select "Cobertura" as input format
- Download the generated HTML report

### Test Categories

- **Unit Tests**: Service layer and repository tests
- **Integration Tests**: API endpoint tests
- **Plugin Tests**: Formatter plugin functionality tests

### Manual Testing

1. **API Testing**: Use the provided `TradingSimulator.http` file
2. **Swagger UI**: Navigate to `https://localhost:7150/swagger`
3. **Health Check**: `GET https://localhost:7150/health`

## Verification Steps

### 1. Build and Run Verification

```bash
# Build entire solution
dotnet build

# Run main project
cd TradingSimulator
dotnet run
```

### 2. API Verification

Open browser and navigate to:
- **Swagger UI**: `https://localhost:7150/swagger` or `http://localhost:5184/swagger`
- **Health Check**: `https://localhost:7150/health` or `http://localhost:5184/health`

### 3. HTTP File Testing

Use the `TradingSimulator.http` file in VS Code or Visual Studio to test all endpoints:

```http
### Health check
GET https://localhost:7150/health

### Get all current stock prices
GET https://localhost:7150/api/stock/current

### Get current price for AAPL
GET https://localhost:7150/api/stock/AAPL/current
```

### 4. SignalR Testing

Run the SignalR client to verify real-time updates:

```bash
cd SignalRClient
dotnet run
```

### 5. TCP Server Testing

Run the TCP client to verify TCP streaming:

```bash
cd TcpClient
dotnet run
```

### 6. Plugin System Testing

Verify plugins are loaded correctly by checking logs or testing different output formats.

## HTTP Usage Examples

### Get Current Prices
```http
GET https://localhost:7150/api/stock/current
Accept: application/json
```

### Get Specific Stock Price
```http
GET https://localhost:7150/api/stock/AAPL/current
Accept: application/json
```

### Get Price History
```http
GET https://localhost:7150/api/stock/AAPL/history
Accept: application/json
```

## Configuration

### Application Settings

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "TradingSimulator": "Debug"
    }
  },
  "AllowedHosts": "*"
}
```

### Environment Variables

- `ASPNETCORE_ENVIRONMENT`: Set to `Development` or `Production`
- `ASPNETCORE_URLS`: Configure listening URLs

## Troubleshooting

### Common Issues

1. **Port Already in Use**: Change ports in `launchSettings.json`
2. **Plugin Loading Errors**: Ensure plugins are built and in the correct directory
3. **SignalR Connection Issues**: Check CORS configuration
4. **TCP Server Issues**: Verify port 8080 is available

### Logs

Check application logs for detailed error information:
- Console output during development
- Application Insights in production (if configured)
