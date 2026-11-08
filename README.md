# Windows WebDriver Automation Service

[![Target](https://img.shields.io/badge/Target-.NET%2010-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![Build & Release Production](https://github.com/g4-api/uia-driver-server/actions/workflows/release-pipeline.yml/badge.svg)](https://github.com/g4-api/uia-driver-server/actions/workflows/release-pipeline.yml)

## Overview

The **Windows WebDriver Automation Service** is a powerful tool that enables WebDriver clients, such as Selenium, to automate interactions with Windows applications. By implementing the WebDriver protocol, this service provides a seamless interface for controlling and testing Windows applications using familiar WebDriver commands.

## Features

- **WebDriver Protocol Compliance**: Seamlessly integrates with Selenium and other WebDriver clients.
- **Session Management**: Create, manage, and delete UI Automation sessions.
- **Element Interaction**: Locate and interact with UI elements using various locator strategies.
- **Screen Capture**: Capture screenshots for visual verification and debugging.
- **Native User32 Actions**: Perform native actions like keystrokes, mouse clicks, double-clicks, and clipboard operations.
- **OCR-Based Element Identification**: Find elements based on their OCR (Optical Character Recognition) value, useful when elements cannot be inspected or have no automation interfaces. Use the [OCR Inspector tool](https://github.com/g4-api/ocr-inspector) to find the OCR value of the element to pass as the locator value.
- **Static Point References**: Create elements that reference static points on the screen, even if there is no actual element there. This is useful for quick reference to static points and (X, Y)-based automation when no other options are available. Use the [Cursor Coordinate Tracker tool](https://github.com/g4-api/cursor-coordinate-tracker) to find static points values on the screen to pass as the locator value.

## Advantages

- **Familiar Interface**: Use standard WebDriver commands to interact with Windows applications, leveraging your existing knowledge of Selenium.
- **Cross-Platform Automation**: Unify your web and desktop application testing under a single framework.
- **Comprehensive API**: Detailed endpoints for a wide range of UI automation tasks, documented with Swagger for easy understanding.
- **Community Friendly**: Welcomes contributions and provides extensive documentation to help you get started.

## Documentation

| Page | What it covers |
| --- | --- |
| [Quick start](docs/quick-start.md) | Prerequisites, download, install and first run |
| [UIA options](docs/uia-options.md) | Every `uia:options` setting, plus a G4 `driverParameters` example |
| [Endpoints](docs/endpoints.md) | Route list, Swagger, and how text is read from elements |
| [Locators](docs/locators.md) | The OCR, Coords and Object Model locators |
| [Grid](docs/grid.md) | Register the server with Selenium Grid 4 or Grid 3 |
| Examples | [Python](docs/examples-python.md), [C#](docs/examples-csharp.md) and [Java](docs/examples-java.md) |
| [Troubleshooting](docs/troubleshooting.md) | Error messages, causes and fixes |

## Quick start in three steps

1. Download `Uia.DriverServer.<version>-win-x64.zip` from the [Releases](https://github.com/g4-api/uia-driver-server/releases) page and extract it.
2. Run `Uia.DriverServer.exe`. The server listens on port 5555.
3. Point a WebDriver client at `http://localhost:5555/wd/hub`.

The full steps are in the [quick start](docs/quick-start.md).

## Contributing and builds

Build tools, test commands, version numbers, Docker images and development ports are in the [build, test and release guide](../g4-github-actions/docs/build-test-release.md).
