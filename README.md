# Windows WebDriver Automation Service

[![Build & Release](https://github.com/g4-api/uia-driver-server/actions/workflows/GithubActions.yml/badge.svg)](https://github.com/g4-api/uia-driver-server/actions/workflows/GithubActions.yml)

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Advantages](#advantages)
- [Quick Start Guide](#quick-start-guide)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
- [Integration with Python](#integration-with-python)
  - [Installing Selenium Client Libraries](#installing-selenium-client-libraries)
  - [Example Code](#example-code)
  - [Breakdown and Explanation](#breakdown-and-explanation)
- [Integration with C#](#integration-with-c)
  - [Installing Selenium Client Libraries](#installing-selenium-client-libraries-1)
  - [Example Code](#example-code-1)
  - [Breakdown and Explanation](#breakdown-and-explanation-1)
- [Integration with Java](#integration-with-java)
  - [Installing Selenium Client Libraries with Maven](#installing-selenium-client-libraries-with-maven)
  - [Example Code](#example-code-2)
  - [Breakdown and Explanation](#breakdown-and-explanation-2)
- [Integration with Grid](#integration-with-grid)
  - [Grid 4](#grid-4)
  - [Grid 3 (Legacy)](#grid-3-legacy)
    - [Example using `register` Command](#example-using-register-command)
    - [Example using Configuration File](#example-using-configuration-file)
    - [`register` Command `help` Switch](#register-command-help-switch)

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

## Quick Start Guide

### Prerequisites

- Basic understanding of WebDriver (e.g., Selenium)
- .NET 8 and above

### Installation

1. **Download the Latest Release**:
    - Go to the [Releases](https://github.com/g4-api/uia-driver-server/releases) page.
    - Download the latest `Uia.DriverServer.<version>-win-x64.zip` artifact.

2. **Extract the Zip Artifact**:
    ```bash
    unzip Uia.DriverServer.<version>-win-x64.zip -d uia-driver-server
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

## Integration with Python

### Installing Selenium Client Libraries

To integrate the Windows WebDriver Automation Service with Python, you first need to install the Selenium client libraries. You can do this using pip:

```bash
pip install selenium
```

### Example Code

Here is a detailed example of how to set up and use the service for automating Windows applications using Python:

```python
from selenium import webdriver
from selenium.common import WebDriverException, NoSuchElementException, StaleElementReferenceException
from selenium.webdriver.common.by import By
from selenium.webdriver.support import expected_conditions as ec
from selenium.webdriver.common.options import BaseOptions
from selenium.webdriver.support.wait import WebDriverWait

class UiaOptions(BaseOptions):
    """
    A class to define options specific to UI Automation (UIA) for Selenium WebDriver.
    """
    def __init__(self):
        """
        Initializes the UiaOptions with default values.
        """
        super().__init__()
        self.app = None
        self.uia_options = None

    def to_capabilities(self):
        """
        Returns the capabilities required for UIA in a dictionary format.
        """
        return {
            "browserName": "Uia",
            "platformName": "windows",
            "uia:options": self.uia_options
        }

    @property
    def default_capabilities(self):
        """
        Provides the default capabilities for the UIA WebDriver.
        """
        return {
            "browserName": "Uia",
            "platform": "windows",
        }

# Create an instance of UiaOptions and set the application to Notepad.
options = UiaOptions()

# Uncomment and set additional UIA options if needed.
options.uia_options = {
    "app": "notepad.exe",
    # "arguments": ["param1", "param2"],
    # "workingDirectory": "<>",
    # "mount": False,
    # "impersonation": {
    #     "domain": "<>",
    #     "username": "<>",
    #     "password": "<>",
    #     "enabled": False
    # }
}

# Initialize the WebDriver with the specified options.
driver = webdriver.Remote(command_executor="http://localhost:5555/wd/hub", options=options)

# Define exceptions to ignore during WebDriverWait.
ignored_exceptions = [WebDriverException, NoSuchElementException, StaleElementReferenceException]

# Create a WebDriverWait instance with a timeout of 20 seconds.
driver_wait = WebDriverWait(driver, timeout=20, ignored_exceptions=ignored_exceptions)

# Wait until the Notepad window is located and get the Notepad element.
notepad = driver_wait.until(ec.presence_of_element_located((By.XPATH, "//Window[@ClassName='Notepad']")))

# Find the Document element within Notepad and type "Hello from UiaDriver Server".
document = notepad.find_element(By.XPATH, "//Document[@Name='Text editor']")
document.send_keys("Hello from UiaDriver Server")

# Find the File menu item and click it.
file_menu_item = notepad.find_element(By.XPATH, "//MenuBar/MenuItem[@AutomationId='File']")
file_menu_item.click()

# Define the locator for the 'Save as' menu item and wait until it's located, then click it.
save_as_locator = (By.XPATH, "//Window[@ClassName='Notepad']//MenuItem[@Name='Save as']")
save_as = driver_wait.until(ec.presence_of_element_located(save_as_locator))
save_as.click()

# Define the locator for the Cancel button in the 'Save as' dialog, wait until it's located, then click it.
cancel_button_locator = (By.XPATH, "//Window[@ClassName='Notepad']//Window[@Name='Save as']/Button[@Name='Cancel']")
cancel_button = driver_wait.until(ec.presence_of_element_located(cancel_button_locator))
cancel_button.click()

# Quit the WebDriver session.
driver.quit()
```

### Breakdown and Explanation

1. **Import Required Modules**:
    - `webdriver`, `WebDriverException`, `NoSuchElementException`, `StaleElementReferenceException` from `selenium`: These modules are necessary for initializing the WebDriver, handling exceptions, and interacting with UI elements.
    - `By` and `expected_conditions` (`ec`) from `selenium.webdriver.common`: These are used for locating elements and defining expected conditions.
    - `BaseOptions` from `selenium.webdriver.common.options`: This module is used to define custom options for the WebDriver.
    - `WebDriverWait` from `selenium.webdriver.support.wait`: This is used to wait for specific conditions to be met before proceeding with actions.

2. **Define `UiaOptions` Class**:
    - The `UiaOptions` class inherits from `BaseOptions` and is used to define options specific to UI Automation.
    - The `__init__` method initializes the class with default values.
    - The `to_capabilities` method returns the capabilities required for UIA in a dictionary format.
    - The `default_capabilities` property provides the default capabilities for the UIA WebDriver.

3. **Set Application and Initialize WebDriver**:
    - An instance of `UiaOptions` is created, and the application is set to Notepad

 (`notepad.exe`).
    - Additional UIA options can be set as needed (e.g., arguments, working directory, impersonation).
    - The WebDriver is initialized with the specified options, connecting to the WebDriver server running at `http://localhost:5555/wd/hub`.

4. **Define Ignored Exceptions and Create WebDriverWait**:
    - A list of exceptions to ignore during `WebDriverWait` is defined.
    - An instance of `WebDriverWait` is created with a timeout of 20 seconds, ignoring the specified exceptions.

5. **Interact with Notepad Application**:
    - Wait until the Notepad window is located using the XPath locator and get the Notepad element.
    - Find the Document element within Notepad and type "Hello from UiaDriver Server".
    - Find the File menu item and click it.
    - Define the locator for the 'Save as' menu item, wait until it's located, and click it.
    - Define the locator for the Cancel button in the 'Save as' dialog, wait until it's located, and click it.

6. **Quit the WebDriver Session**:
    - Finally, quit the WebDriver session to clean up resources.

This integration allows you to automate interactions with Windows applications using the Windows WebDriver Automation Service and Python's Selenium WebDriver.

## Integration with C#

### Installing Selenium Client Libraries

To integrate the Windows WebDriver Automation Service with C#, you first need to install the Selenium client libraries. You can do this using dotnet:

```bash
dotnet add package Selenium.WebDriver
```

### Example Code

Here is a detailed example of how to set up and use the service for automating Windows applications using C#:

```csharp
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

using System;
using System.Collections.Generic;
using System.Threading;

using Uia.Models;
using Uia.Support;

// Create an instance of UiaOptions and set the application to Notepad.
var options = new UiaOptions
{
    // Uncomment and set additional UIA options if needed.
    UiaOptionsDictionary = new
    {
        app = "notepad.exe",
        //arguments = new[] { "param1", "param2" },
        //workingDirectory = "<>",
        //mount = false,
        //impersonation = new
        //{
        //    domain = "<>",
        //    username = "<>",
        //    password = "<>",
        //    enabled = false
        //}
    }
};

// Initialize the WebDriver with the specified options.
var driver = new RemoteWebDriver(new Uri("http://localhost:5555/wd/hub"), options.ToCapabilities());

// Define exceptions to ignore during WebDriverWait.
var ignoredExceptions = new List<Type>
{
    typeof(WebDriverException),
    typeof(NoSuchElementException),
    typeof(StaleElementReferenceException)
};

// Create a WebDriverWait instance with a timeout of 20 seconds.
var wait = new UiaWaiter(driver, ignoredExceptions, TimeSpan.FromSeconds(20));

// Wait until the Notepad window is located and get the Notepad element.
var notepad = wait.Until(d => d.FindElement(By.XPath("//Window[@ClassName='Notepad']")));

// Find the Document element within Notepad and type "Hello from UiaDriver Server".
var document = notepad.FindElement(By.XPath("//Document[@Name='Text editor']"));
document.SendKeys("Hello from UiaDriver Server");

// Find the File menu item and click it.
var fileMenuItem = notepad.FindElement(By.XPath("//MenuBar/MenuItem[@AutomationId='File']"));
fileMenuItem.Click();

// Define the locator for the 'Save as' menu item and wait until it's located, then click it.
var saveAsLocator = By.XPath("//Window[@ClassName='Notepad']//MenuItem[@Name='Save as']");
var saveAs = wait.Until(d => d.FindElement(saveAsLocator));
saveAs.Click();

// Define the locator for the Cancel button in the 'Save as' dialog, wait until it's located, then click it.
var cancelButtonLocator = By.XPath("//Window[@ClassName='Notepad']//Window[@Name='Save as']/Button[@Name='Cancel']");
var cancelButton = wait.Until(d => d.FindElement(cancelButtonLocator));
cancelButton.Click();

// Quit the WebDriver session.
driver.Quit();

namespace Uia.Models
{
    /// <summary>
    /// Represents the options for UI Automation (UIA) driver.
    /// </summary>
    public class UiaOptions : DriverOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UiaOptions"/> class with default values.
        /// </summary>
        public UiaOptions()
        {
            UiaOptionsDictionary = null;
        }

        /// <summary>
        /// Gets or sets the additional UIA options.
        /// </summary>
        public object UiaOptionsDictionary { get; set; }

        /// <summary>
        /// Returns the capabilities required for UIA in a dictionary format.
        /// </summary>
        /// <returns>A dictionary of capabilities.</returns>
        public override ICapabilities ToCapabilities()
        {
            // Generate the desired capabilities with default values
            var capabilities = GenerateDesiredCapabilities(true);

            // Set the specific capabilities for the UIA driver
            capabilities.SetCapability("browserName", "Uia");
            capabilities.SetCapability("platformName", "windows");
            capabilities.SetCapability("uia:options", UiaOptionsDictionary);

            // Return the capabilities as a read-only dictionary
            return capabilities.AsReadOnly();
        }

        /// <summary>
        /// Provides the default capabilities for the UIA WebDriver.
        /// </summary>
        /// <returns>A dictionary of default capabilities.</returns>
        public ICapabilities DefaultCapabilities()
        {
            // Generate the desired capabilities with default values
            var capabilities = GenerateDesiredCapabilities(true);

            // Set the default capabilities for the UIA driver
            capabilities.SetCapability("browserName", "Uia");
            capabilities.SetCapability("platform", "windows");
            capabilities.SetCapability("uia:options", new { app = "Desktop" });

            // Return the capabilities as a read-only dictionary
            return capabilities.AsReadOnly();
        }
    }
}

namespace Uia.Support
{
    /// <summary>
    /// Provides a mechanism to wait for conditions in the UI Automation (UIA) context.
    /// </summary>
    /// <param name="driver">The WebDriver instance to be used.</param>
    /// <param name="ignoredException">The list of exceptions to ignore while waiting.</param>
    /// <param name="timeout">The timeout duration for the wait.</param>
    public class UiaWaiter(IWebDriver driver, List<Type> ignoredException, TimeSpan timeout)
    {
        // Initialize the private fields with the provided values from the constructor parameters
        private readonly IWebDriver _driver = driver;
        private readonly List<Type> _ignoredExceptions = ignoredException;
        private readonly TimeSpan _timeout = timeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="UiaWaiter"/> class with a default timeout of 10 seconds.
        /// </summary>
        /// <param name="driver">The WebDriver instance to be used.</param>
        public UiaWaiter(IWebDriver driver)
            : this(driver, ignoredException: [], timeout: TimeSpan.FromSeconds(10))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UiaWaiter"/> class.
        /// </summary>
        /// <param name="driver">The WebDriver instance to be used.</param>
        /// <param name="timeout">The timeout duration for the wait.</param>
        public UiaWaiter(IWebDriver driver, TimeSpan timeout)
            : this(driver, ignoredException: [], timeout)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UiaWaiter"/> class with a default timeout of 10 seconds.
        /// </summary>
        /// <param name="driver">The WebDriver instance to be used.</param>
        /// <param name="ignoredException">The list of exceptions to ignore while waiting.</param>
        public UiaWaiter(IWebDriver driver, List<Type> ignoredException)
            : this(driver, ignoredException, timeout: TimeSpan.FromSeconds(10))
        { }

        /// <summary>
        /// Waits until a condition is met or the timeout is reached.
        /// </summary>
        /// <param name="condition">The condition to be met.</param>
        /// <returns>The web element that satisfies the condition.</returns>
        /// <exception cref="NoSuchElementException">Thrown if the condition is not met within the timeout period.</exception>
        public IWebElement Until(Func<IWebDriver, IWebElement> condition)
        {
            // Calculate the time when the wait should timeout
            var conditionTimeout = DateTime.Now.Add(_timeout);

            do
            {
                try
                {
                    // Try to evaluate the condition
                    return condition(_driver);
                }
                catch (Exception e)
                {
                    // Check if the exception should be ignored
                    if (_ignoredExceptions.Exists(i => i == e.GetType()))
                    {
                        // Wait for a short interval before retrying
                        Thread.Sleep(100);
                        continue;
                    }
                    // Rethrow the exception if it's not to be ignored
                    throw;
                }
            } while (DateTime.Now < conditionTimeout);

            // Throw an exception if the condition is not met within the timeout
            throw new NoSuchElementException();
        }
    }
}
```

### Breakdown and Explanation

1. **Import Required Namespaces**:
    - `OpenQA.Selenium`, `OpenQA.Selenium.Remote`: These namespaces are necessary for initializing the WebDriver and defining the options for UI Automation (UIA).
    - `System`, `System.Collections.Generic`, `System.Threading`: These namespaces provide essential system functionalities and collections.

2. **Create `UiaOptions` Instance**:
    - An instance of `UiaOptions` is created, and the application is set to Notepad (`notepad.exe`).
    - Additional UIA options can be set as needed (e.g., arguments, working directory, impersonation).

3. **Initialize WebDriver**:
    - The WebDriver is initialized with the specified options, connecting to the WebDriver server running at `http://localhost:5555/wd/hub`.

4. **Define Ignored Exceptions and Create `UiaWaiter`**:
    - A list of exceptions to ignore during `WebDriverWait` is defined.
    - An instance of `UiaWaiter` is created with a timeout of 20 seconds, ignoring the specified exceptions.

5. **Interact with Notepad Application**:
    - Wait until the Notepad window is located using the XPath locator and get the Notepad element.
    - Find the Document element within Notepad and type "Hello from UiaDriver Server".
    - Find the File menu item and click it.
    - Define the locator for the 'Save as' menu item, wait until it's located, and click it.
    - Define the locator for the Cancel button in the 'Save as' dialog, wait until it's located, and click it.

6. **Quit the WebDriver Session**:
    - Finally, quit the WebDriver session to clean up resources.

This integration allows you to automate interactions with Windows applications using the Windows WebDriver Automation Service and C#'s Selenium WebDriver.

## Integration with Java

To integrate the Windows WebDriver Automation Service with Java, you can use the Selenium WebDriver. Below is a detailed example of how to set up and use the service for automating Windows applications using Java:

### Installing Selenium Client Libraries with Maven

Add the following dependencies to your `pom.xml` file to include Selenium:

```xml
<dependencies>
    <dependency>
        <groupId>org.seleniumhq.selenium</groupId>
        <artifactId>selenium-java</artifactId>
        <version>4.0.0</version>
    </dependency>
</dependencies>
```

### Example Code

```java
package org.example;

import org.openqa.selenium.*;
import org.openqa.selenium.NoSuchElementException;
import org.openqa.selenium.remote.RemoteWebDriver;
import org.openqa.selenium.support.ui.WebDriverWait;

import java.io.Serializable;
import java.net.MalformedURLException;
import java.net.URL;
import java.time.Duration;
import java.util.*;

public class Main {
    public static void main(String[] args) {
        // Create an instance of UiaOptionsDictionary to configure UI Automation settings.
        UiaOptionsDictionary uiaOptionsDictionary = new UiaOptionsDictionary();

        // Set the application for the UI Automation to Notepad (notepad.exe).
        uiaOptionsDictionary.setApp("notepad.exe");

        // Create an instance of UiaOptions to configure UI Automation settings.
        UiaOptions uiaOptions = new UiaOptions();

        // Assign the previously created UiaOptionsDictionary to the UiaOptions instance.
        uiaOptions.setUiaOptionsDictionary(uiaOptionsDictionary);

        // Initialize the WebDriver with the specified options.
        WebDriver driver = null;
        try {
            driver = new RemoteWebDriver(new URL("http://localhost:5555/wd/hub"), uiaOptions.toCapabilities());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Define exceptions to ignore during WebDriverWait.
        List<Class<? extends Throwable>> ignoredExceptions = Arrays.asList(
                WebDriverException.class,
                NoSuchElementException.class,
                StaleElementReferenceException.class
        );

        // Create a WebDriverWait instance with a timeout of 20 seconds.
        WebDriverWait wait = new WebDriverWait(driver, Duration.ofSeconds(20));
        wait.ignoreAll(ignoredExceptions);

        // Wait until the Notepad window is located and get the Notepad element.
        WebElement notepad = wait.until(d -> d.findElement(By.xpath("//Window[@ClassName='Notepad']")));

        // Find the Document element within Notepad and type "Hello from UiaDriver Server".
        WebElement document = notepad.findElement(By.xpath("//Document[@Name='Text editor']"));
        document.sendKeys("Hello from UiaDriver Server");

        // Find the File menu item and click it.
        WebElement fileMenuItem = notepad.findElement(By.xpath("//MenuBar/MenuItem[@AutomationId='File']"));
        fileMenuItem.click();

        // Define the locator for the 'Save as' menu item and wait until it's located, then click it.
        By saveAsLocator = By.xpath("//Window[@ClassName='Notepad']//MenuItem[@Name='Save as']");
        WebElement saveAs = wait.until(d -> d.findElement(saveAsLocator));
        saveAs.click();

        // Define the locator for the Cancel button in the 'Save as' dialog, wait until it's located, then click it.
        By cancelButtonLocator = By.xpath("//Window[@ClassName='Notepad']//Window[@Name='Save as']/Button[@Name='Cancel']");
        WebElement cancelButton = wait.until(d -> d.findElement(cancelButtonLocator));
        cancelButton.click();

        // Quit the WebDriver session.
        assert driver != null;
        driver.quit();
    }
}

/**
 * UiaOptions class represents the configuration options for UI Automation.
 * This class extends MutableCapabilities to allow its instances to be used as Capabilities.
 */
class UiaOptions extends MutableCapabilities {

    // The UIA options dictionary.
    private UiaOptionsDictionary uiaOptionsDictionary;

    /**
     * Initializes a new instance of the UiaOptions class.
     */
    public UiaOptions() {
        this.uiaOptionsDictionary = new UiaOptionsDictionary();
    }

    /**
     * Gets the UIA options dictionary.
     */
    public UiaOptionsDictionary getUiaOptionsDictionary() {
        return uiaOptionsDictionary;
    }

    /**
     * Sets the UIA options dictionary.
     *
     * @param uiaOptionsDictionary The UIA options dictionary.
     */
    public void setUiaOptionsDictionary(UiaOptionsDictionary uiaOptionsDictionary) {
        this.uiaOptionsDictionary = uiaOptionsDictionary;
    }

    /**
     * Converts the UiaOptions instance to a Capabilities object.
     *
     * @return The Capabilities object with the UIA options set.
     */
    public Capabilities toCapabilities() {
        // Set the specific capabilities for the UIA driver
        setCapability("browserName", "Uia");
        setCapability("platformName", "windows");
        setCapability("uia:options", uiaOptionsDictionary.toMap());

        // Return the capabilities object with the UIA options set for the driver instance creation
        return this;
    }

    @Override
    public boolean equals(Object other) {
        return super.equals(other);
    }

    @Override
    public int hashCode() {
        return super.hashCode();
    }
}

/**
 * UiaOptionsDictionary class represents the configuration options for UI Automation.
 * This class implements Serializable to allow its instances to be serialized.
 */
class UiaOptionsDictionary implements Serializable {
    private String app;
    private String[] arguments;
    private String workingDirectory;
    private boolean mount;
    private ImpersonationOptions impersonation;

    /**
     * Gets the application name.
     *
     * @return the application name.
     */
    public String getApp() {
        return app;
    }

    /**
     * Sets the application name.
     *
     * @param app the application name to set.
     */
    public void setApp(String app) {
        this.app = app;
    }

    /**
     * Gets the arguments for the application.
     *
     * @return an array of arguments.
     */
    public String[] getArguments() {
        return arguments;
    }

    /**
     * Sets the arguments for the application.
     *
     * @param arguments an array of arguments to set.
     */
    public void setArguments(String[] arguments) {
        this.arguments = arguments;
    }

    /**
     * Gets the working directory.
     *
     * @return the working directory.
     */
    public String getWorkingDirectory() {
        return workingDirectory;
    }

    /**
     * Sets the working directory.
     *
     * @param workingDirectory the working directory to set.
     */
    public void setWorkingDirectory(String workingDirectory) {
        this.workingDirectory = workingDirectory;
    }

    /**
     * Checks if the mount option is enabled.
     *
     * @return true if mount is enabled, false otherwise.
     */
    public boolean isMount() {
        return mount;
    }

    /**
     * Sets the mount option.
     *
     * @param mount true to enable mount, false to disable.
     */
    public void setMount(boolean mount) {
        this.mount = mount;
    }

    /**
     * Gets the impersonation options.
     *
     * @return the impersonation options.
     */
    public ImpersonationOptions getImpersonation() {
        return impersonation;
    }

    /**
     * Sets the impersonation options.
     *
     * @param impersonation the impersonation options to set.
     */
    public void setImpersonation(ImpersonationOptions impersonation) {
        this.impersonation = impersonation;
    }

    /**
     * Converts the UiaOptionsDictionary to a Map.
     * The method ensures the app field defaults to "Desktop" if not set.
     *
     * @return a Map representing the options.
     */
    public Map<String, Object> toMap() {
        // Set default app to "Desktop" if app is null or empty
        app = app == null || app.isEmpty() ? "Desktop" : app;

        // Create a new map to store the options
        Map<String, Object> map = new HashMap<>();

        // Add arguments to the map if not null
        if (arguments != null) map.put("arguments", arguments);

        // Add workingDirectory to the map if not null
        if (workingDirectory != null) map.put("workingDirectory", workingDirectory);

        // Add impersonation options to the map if not null
        if (impersonation != null) map.put("impersonation", impersonation.toMap());

        // Add app and mount to the map
        map.put("app", app);
        map.put("mount", mount);

        // Return the map with the options
        return map;
    }


    /**
     * ImpersonationOptions class represents the options for impersonation in UI Automation.
     * This class implements Serializable to allow its instances to be serialized.
     */
    static class ImpersonationOptions implements Serializable {
        // Initialize the fields for domain, username, password, and enabled
        // status for impersonation options in UI Automation.
        private String domain;
        private String username;
        private String password;
        private boolean enabled;

        /**
         * Gets the domain name.
         *
         * @return the domain name.
         */
        public String getDomain() {
            return domain;
        }

        /**
         * Sets the domain name.
         *
         * @param domain the domain name to set.
         */
        public void setDomain(String domain) {
            this.domain = domain;
        }

        /**
         * Gets the username.
         *
         * @

return the username.
         */
        public String getUsername() {
            return username;
        }

        /**
         * Sets the username.
         *
         * @param username the username to set.
         */
        public void setUsername(String username) {
            this.username = username;
        }

        /**
         * Gets the password.
         *
         * @return the password.
         */
        public String getPassword() {
            return password;
        }

        /**
         * Sets the password.
         *
         * @param password the password to set.
         */
        public void setPassword(String password) {
            this.password = password;
        }

        /**
         * Checks if impersonation is enabled.
         *
         * @return true if impersonation is enabled, false otherwise.
         */
        public boolean isEnabled() {
            return enabled;
        }

        /**
         * Sets the impersonation enabled option.
         *
         * @param enabled true to enable impersonation, false to disable.
         */
        public void setEnabled(boolean enabled) {
            this.enabled = enabled;
        }

        /**
         * Converts the ImpersonationOptions to a Map.
         *
         * @return a Map representing the impersonation options.
         */
        public Map<String, Object> toMap() {
            // Create a new map to store the impersonation options
            Map<String, Object> map = new HashMap<>();

            // Add domain to the map if not null
            if (domain != null) map.put("domain", domain);

            // Add username to the map if not null
            if (username != null) map.put("username", username);

            // Add password to the map if not null
            if (password != null) map.put("password", password);

            // Add enabled status to the map
            map.put("enabled", enabled);

            // Return the map with the impersonation options
            return map;
        }
    }
}
```

### Breakdown and Explanation

1. **Import Required Packages**:
    - `org.openqa.selenium.*`: These packages are necessary for initializing the WebDriver, handling exceptions, and interacting with UI elements.
    - `java.net.*`, `java.time.*`, `java.util.*`: These packages provide essential system functionalities and collections.

2. **Create `UiaOptionsDictionary` Instance**:
    - An instance of `UiaOptionsDictionary` is created, and the application is set to Notepad (`notepad.exe`).

3. **Create `UiaOptions` Instance**:
    - An instance of `UiaOptions` is created, and the `UiaOptionsDictionary` instance is assigned to it.

4. **Initialize WebDriver**:
    - The WebDriver is initialized with the specified options, connecting to the WebDriver server running at `http://localhost:5555/wd/hub`.

5. **Define Ignored Exceptions and Create `WebDriverWait`**:
    - A list of exceptions to ignore during `WebDriverWait` is defined.
    - An instance of `WebDriverWait` is created with a timeout of 20 seconds, ignoring the specified exceptions.

6. **Interact with Notepad Application**:
    - Wait until the Notepad window is located using the XPath locator and get the Notepad element.
    - Find the Document element within Notepad and type "Hello from UiaDriver Server".
    - Find the File menu item and click it.
    - Define the locator for the 'Save as' menu item, wait until it's located, and click it.
    - Define the locator for the Cancel button in the 'Save as' dialog, wait until it's located, and click it.

7. **Quit the WebDriver Session**:
    - Finally, quit the WebDriver session to clean up resources.

This integration allows you to automate interactions with Windows applications using the Windows WebDriver Automation Service and Java's Selenium WebDriver.

## Integration with Grid

To integrate the Windows WebDriver Automation Service with a Selenium Grid, you can register the service with either Grid 4 or Grid 3 (Legacy). Below are the detailed instructions for both versions.

### Grid 4

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

### Grid 3 (Legacy)

Grid 3 is the older version of Selenium Grid and has different registration requirements. To register the Windows WebDriver Automation Service with Grid 3, you need to start the service with the `register` command and provide a combination of switches or a configuration file.

#### Example using `register` Command

To register the service with Grid 3, use the following example command:

```bash
Uia.DriverServer.exe register -b Uia -ht localhost -hb http://hub-address:4444 -p 4444 -P 5555 -t "tag1,tag2"
```

#### Example using Configuration File

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

#### `register` Command `help` Switch

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
