# Integration with Grid

To integrate the Windows WebDriver Automation Service with a Selenium Grid, you can register the service with either Grid 4 or Grid 3 (Legacy). Below are the detailed instructions for both versions.

## Grid 4

Grid 4 provides enhanced features and capabilities for managing and scaling your Selenium infrastructure. Below are the steps to register the Windows WebDriver Automation Service with Grid 4.

1. **Download the Selenium Server Jar**:
    - Download the Selenium Server jar from [Selenium Downloads](https://www.selenium.dev/downloads/).

2. **Create Node Configuration File**:
    - Create a file named `node.toml` with the following content:

    ```toml
    [node]
    detect-drivers = false

    [relay]
    # Default UIA server endpoint
    url = "http://localhost:5555/wd/hub"
    status-endpoint = "/status"
    # Optional, enforce a specific protocol version in HttpClient when communicating with the endpoint service status (e.g. HTTP/1.1, HTTP/2)
    protocol-version = "HTTP/1.1"
    configs = [
      "1", "{\"browserName\": \"Uia\", \"platformName\": \"windows\", \"uia:platformVersion\": \"11\"}"
    ]
    ```

    - **Note**: The `max-sessions` on Windows automation is always 1. More than that can cause unexpected behavior due to Windows trying to focus on multiple applications at the same time.

3. **Start the Selenium Grid Hub**:
    ```bash
    java -jar selenium-server-<version>.jar hub
    ```

4. **Start the Selenium Grid Node**:
    - Use the node configuration file created in step 2. The node port can be any available port on the host machine.

    ```bash
    java -jar selenium-server-<version>.jar node --config node.toml --port 5554
    ```

Following these steps will ensure that the Windows WebDriver Automation Service is properly integrated and registered with Selenium Grid 4, allowing you to leverage distributed and scalable automation for Windows applications.

Ensure that the service is running and accessible at the node URL you provide to the Grid.

## Grid 3 (Legacy)

Grid 3 is the older version of Selenium Grid and has different registration requirements. To register the Windows WebDriver Automation Service with Grid 3, you need to start the service with the `register` command and provide a combination of switches or a configuration file.

### Example using `register` Command

To register the service with Grid 3, use the following example command:

```bash
Uia.DriverServer.exe register -b Uia -ht localhost -hb http://hub-address:4444 -p 4444 -P 5555 -t "tag1,tag2"
```

### Example using Configuration File

Alternatively, you can use a configuration file to register the service. Create a JSON configuration file (`config.json`) with the following content:

```json
{
    "browserName": "Uia",
    "host": "localhost",
    "hub": "http://hub-address:4444",
    "hubPort": 4444,
    "hostPort": 5555,
    "tags": ["tag1", "tag2"]
}
```

Then, register the service using the configuration file:

```bash
Uia.DriverServer.exe register -c path/to/config.json
```

### `register` Command `help` Switch

You can use the `--help` switch with the `register` command to display help information about the available options. The following output is expected:

```plaintext
C:\Uia.DriverServer.0000.00.00.0-win-x64>Uia.DriverServer register --help

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


register -b|--browserName -c|--config -ht|--host -hb|--hub -p|--hubPort -P|--hostPort -t|--tags -h|--help
    -b,  --browserName  Specifies the name of the browser to use.
    -c,  --config       Path to the configuration file.
    -hb, --hub          Specifies the hub address.
    -ht, --host         Specifies the node host address.
    -p,  --hubPort      Specifies the port of the hub.
    -P,  --hostPort     Specifies the port of the node host.
    -t,  --tags         Specifies the tags for the node. These tags will be converted to capabilities.
    -h,  --help         Displays help information for the specified command.
```

This help information provides details about each command-line switch available for the `register` command, helping you configure the service appropriately for integration with Grid 3.

Back to the README.
