# UIA Options

A session request tells the server what to automate through the `uia:options` capability. This page lists every option the server reads, and shows how to send them from a G4 automation.

## Where the options go

The server reads `uia:options` from `capabilities.alwaysMatch`. If `alwaysMatch` is empty, it reads the first entry of `firstMatch`. Any other capability, such as `browserName` or `platformName`, is stored on the session but does not change how the session starts. Source: `src\Uia.DriverServer.Domain\SessionsRepository.cs:156-184`.

Option names are camelCase. The server matches them without regard to case. Source: `SessionsRepository.cs:32-36`.

## Option reference

| Option | Type | Default | Where it is read | What it does |
| --- | --- | --- | --- | --- |
| `app` | string | empty | `SessionsRepository.cs:187-192, 404-410` | Empty: the server creates a desktop session and starts nothing. Not empty: the server starts this file, or attaches to it when `mount` is on. If the value is a folder, the server opens it in `explorer.exe`. |
| `arguments` | array of strings | empty | `SessionsRepository.cs:405, 410` | Command-line arguments for `app`. The server joins them with one space. |
| `workingDirectory` | string | empty | `UiaUtilities.cs:94-116` | Working directory for the started process. A value turns on shell execution. |
| `mount` | boolean | `false` | `SessionsRepository.cs:408-409` | `true`: do not start `app`. Find a running process whose name is contained in the `app` value, ignoring case. |
| `impersonation` | object | none | `UiaUtilities.cs:53-91, 121-123` | Starts the process as another user. The server uses it only when `enabled` is `true`. See the table below. |
| `scaleRatio` | number | `1.0` | `SessionsRepository.cs:430` | Stored on the session as its scale ratio. No other code in `src` reads it. |
| `terminal` | string | `Powershell` | `src\Uia.DriverServer.Domain\DocumentRepository.cs:88-105, 126, 193` | Shell for `POST /session/{session}/execute/sync`. The server has two: `Cmd` and `Powershell`. Any other value makes the call answer HTTP 500. |

### impersonation

| Field | Type | What it does |
| --- | --- | --- |
| `enabled` | boolean | Must be `true` for the other fields to count. |
| `domain` | string | Domain of the user. |
| `username` | string | User name. |
| `password` | string | Password. It travels in the session request, so use the server only on a trusted network. |

Source: `src\Uia.DriverServer.Models\ImpersonationModel.cs`, `UiaUtilities.cs:63-91`.

## Desktop session or application session

| You send | The server creates |
| --- | --- |
| No `app`, or an empty one | A desktop session. Find elements anywhere on the desktop. |
| `app` with a path or command | An application session. The server starts the process and the session is bound to it. |
| `app` with `mount: true` | An application session on a process that already runs. |

If the process cannot start, has exited, or has no usable handle, the server answers HTTP 500. Source: `SessionsRepository.cs:412-424`.

## Use the options from a G4 automation

In G4, the `UiaDriver` driver plugin sends these capabilities for you. Put the options under `uia:options` and point `driverBinaries` at the server. A URL selects remote mode. Source: [drivers table](../../g4-abstraction/docs/drivers.md).

This automation opens a desktop session, so it has no `app`:

```json
{
    "driverParameters": {
        "driver": "UiaDriver",
        "driverBinaries": "http://localhost:5555/wd/hub",
        "capabilities": {
            "alwaysMatch": {
                "browserName": "Uia",
                "uia:options": {}
            }
        }
    }
}
```

This one starts Notepad with a working directory:

```json
{
    "driverParameters": {
        "driver": "UiaDriver",
        "driverBinaries": "http://localhost:5555/wd/hub",
        "capabilities": {
            "alwaysMatch": {
                "browserName": "Uia",
                "uia:options": {
                    "app": "notepad.exe",
                    "arguments": [],
                    "workingDirectory": "C:\\Temp"
                }
            }
        }
    }
}
```

The same options in a Selenium client are in the [Python](examples-python.md), [C#](examples-csharp.md) and [Java](examples-java.md) examples.

Back to the README.
