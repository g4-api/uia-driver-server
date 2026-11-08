# Integration with C#

## Installing Selenium Client Libraries

To integrate the Windows WebDriver Automation Service with C#, you first need to install the Selenium client libraries. You can do this using dotnet:

```bash
dotnet add package Selenium.WebDriver
```

## Example Code

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

## Breakdown and Explanation

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

Back to the README.
