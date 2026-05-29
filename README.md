# PartTracker v1

Offline Windows desktop application for tracking engine parts disassembly.

## Features

✅ User authentication (Mechanic & Admin roles)
✅ Import parts from Excel (.xlsx)
✅ Fast part search by 5-digit code (<1 second)
✅ A6 label printing via Kyocera USB printer
✅ Print logging & audit trail
✅ Export removed parts to Excel
✅ 100% offline operation
✅ SQLite local database
✅ Dark mode UI optimized for workshop

## Tech Stack

- **UI**: WinUI 3 (.NET 8)
- **Database**: SQLite with Dapper ORM
- **Excel**: ClosedXML
- **Printing**: Windows Print API
- **Logging**: Serilog

## Setup

```bash
# Prerequisites
- Visual Studio 2022 (v17.12+)
- .NET 8 SDK
- Windows App SDK 1.6+

# Clone & restore
git clone https://github.com/viktoada/PartTracker.git
cd PartTracker
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

## First Run

1. Database creates automatically on first launch
2. Create admin account via UI
3. Admin imports parts Excel
4. Mechanic logs in and searches parts
5. Select printer and print labels

## Project Structure

```
PartTracker/
├── Models/                 # Data models
├── Services/              # Business logic
├── Views/                 # WinUI pages (UI)
├── PartTracker.csproj     # Project file
└── README.md
```

## Roadmap

- **v1**: MVP with search, print, export
- **v2**: Batch printing, QR codes, fuzzy search
- **v3**: Multi-project support, online sync, mobile

## License

Proprietary
