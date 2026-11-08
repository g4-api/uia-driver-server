# Quick Start Guide

## Prerequisites

- Basic understanding of WebDriver (e.g., Selenium)
- .NET 8 and above (the projects in this repository target `net10.0-windows`)

## Installation

1. **Download the Latest Release**:
    - Go to the [Releases](https://github.com/g4-api/uia-driver-server/releases) page.
    - Download the latest `Uia.DriverServer.<version>-win-x64.zip` artifact.

2. **Extract the Zip Artifact**:
    ```bash
    powershell -Command "Expand-Archive -Path 'Uia.DriverServer.<version>-win-x64.zip' -DestinationPath 'uia-driver-server'"
    cd uia-driver-server
    ```

3. **Run the Service**:
    ```bash
    ./Uia.DriverServer.exe
    ```

The following (or similar) output is expected:

```plaintext
   _   _ _       ____       _                  ____
  | | | (_) __ _|  _ \ _ __(_)_   _____ _ __  / ___|  ___ _ ____   _____ _ __
  | | | | |/ _` | | | | '__| \ \ / / _ \ '__| \___ \ / _ \ '__\ \ / / _ \ '__|
  | |_| | | (_| | |_| | |  | |\ V /  __/ |     ___) |  __/ |   \ V /  __/ |
   \___/|_|\__,_|____/|_|  |_| \_/ \___|_|    |____/ \___|_|    \_/ \___|_|

                                   WebDriver Implementation for Windows Native
                                                      Powered by IUIAutomation

  Project:           https://github.com/g4-api/uia-driver
  W3C Documentation: https://www.w3.org/TR/webdriver/
  Documentation:     https://docs.microsoft.com/en-us/windows/win32/api/_winauto/
  Open API:          /swagger


info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://[::]:5555
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\Uia.DriverServer
```

Back to the README.
