# Locators

Besides the usual XPath and CSS selector strategies, the server understands three special locators. They are passed as the XPath or CSS selector value, so every W3C client can use them.

| Locator | Syntax | Use it when |
| --- | --- | --- |
| [OCR](#using-the-ocr-locator) | `ocr(text)` | The element has no automation interfaces and you can only identify it by the text on screen. |
| [Coords](#using-the-coords-locator) | `coords(x,y)` | You need a fixed point on the screen. |
| [Object Model](#using-the-object-model-locator) | `//Window/Pane/ObjectModel:Pane` | You need full XPath over a small part of the element tree. |


## Using the OCR Locator

The OCR (Optical Character Recognition) locator in the UiaDriver Server allows you to find elements based on their OCR value. This is particularly useful when elements cannot be inspected or have no automation interfaces. To ensure compatibility with all W3C clients, OCR locators can be passed as XPath or CssSelector.

### Overview

OCR locators enable the identification of UI elements based on the text content recognized by OCR technology. Instead of relying on the exact text, OCR locators use the text identified by the OCR tool, which increases the chances of finding the element even if the text recognition is not perfect.

### Passing OCR Locators as XPath or CssSelector

To use OCR locators, you need to pass them using the syntax `ocr(ocrValue)` in either XPath or CssSelector. The UiaDriver Server will interpret these locators and apply the OCR logic accordingly. Multiple OCR values can be piped using the syntax `ocr(ocrValue1|ocrValue2)`, providing fallback options if the OCR tool identifies the text differently.

#### Example Code

Below are examples of how to use OCR locators in different languages by passing them as XPath or CssSelector.

#### Python Example

```python
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.common.options import BaseOptions
from selenium.webdriver.support.wait import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC

class UiaOptions(BaseOptions):
    def __init__(self):
        super().__init__()
        self.app = None
        self.uia_options = None

    def to_capabilities(self):
        return {
            "browserName": "Uia",
            "platformName": "windows",
            "uia:options": self.uia_options
        }

options = UiaOptions()
options.uia_options = {"app": "notepad.exe"}

driver = webdriver.Remote(command_executor="http://localhost:5555/wd/hub", options=options)
wait = WebDriverWait(driver, 20)

# Use OCR locator passed as XPath with fallback options
ocr_locator = "ocr('Hello from UiaDriver Server'|'Hello from Uia')"
document = wait.until(EC.presence_of_element_located((By.XPATH, ocr_locator)))
document.send_keys("Automated text input using OCR")

driver.quit()
```

#### C# Example

```csharp
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using Uia.Models;
using Uia.Support;

var options = new UiaOptions
{
    UiaOptionsDictionary = new
    {
        app = "notepad.exe"
    }
};

var driver = new RemoteWebDriver(new Uri("http://localhost:5555/wd/hub"), options.ToCapabilities());
var wait = new UiaWaiter(driver, new List<Type> { typeof(WebDriverException), typeof(NoSuchElementException) }, TimeSpan.FromSeconds(20));

// Use OCR locator passed as XPath with fallback options
string ocrLocator = "ocr('Hello from UiaDriver Server'|'Hello from Uia')";
var document = wait.Until(d => d.FindElement(By.XPath(ocrLocator)));
document.SendKeys("Automated text input using OCR");

driver.Quit();
```

#### Java Example

```java
package org.example;

import org.openqa.selenium.*;
import org.openqa.selenium.remote.RemoteWebDriver;
import org.openqa.selenium.support.ui.WebDriverWait;

import java.net.MalformedURLException;
import java.net.URL;
import java.time.Duration;
import java.util.Arrays;
import java.util.List;

public class Main {
    public static void main(String[] args) {
        UiaOptionsDictionary uiaOptionsDictionary = new UiaOptionsDictionary();
        uiaOptionsDictionary.setApp("notepad.exe");

        UiaOptions uiaOptions = new UiaOptions();
        uiaOptions.setUiaOptionsDictionary(uiaOptionsDictionary);

        WebDriver driver = null;
        try {
            driver = new RemoteWebDriver(new URL("http://localhost:5555/wd/hub"), uiaOptions.toCapabilities());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        List<Class<? extends Throwable>> ignoredExceptions = Arrays.asList(
                WebDriverException.class,
                NoSuchElementException.class
        );

        WebDriverWait wait = new WebDriverWait(driver, Duration.ofSeconds(20));
        wait.ignoreAll(ignoredExceptions);

        // Use OCR locator passed as XPath with fallback options
        String ocrLocator = "ocr('Hello from UiaDriver Server'|'Hello from Uia')";
        WebElement document = wait.until(d -> d.findElement(By.xpath(ocrLocator)));
        document.sendKeys("Automated text input using OCR");

        driver.quit();
    }
}
```

### OCR Inspector Tool

To find the OCR value of an element, you can use the [OCR Inspector tool](https://github.com/g4-api/ocr-inspector). This tool helps you obtain the OCR value that can be passed as a locator value.

### Best Practices and Limitations

- **Accuracy**: OCR technology identifies text based on image recognition, which may not always be perfect. Using multiple OCR values as fallback options increases the likelihood of correctly identifying elements.
- **Performance**: OCR-based identification may be slower compared to direct element access. Use it only when other methods are not feasible.
- **Fallback**: Implement fallback strategies by providing multiple OCR values. For example, "Bar" can be identified as "8ar" or "88n", so the OCR locator can be `ocr('8ar'|'88n')`.

## Using the Coords Locator

The Coords (coordinates) locator in the UiaDriver Server allows you to create a virtual element that points to a static point on the screen. This is particularly useful when there are no identifiable UI elements or when you need to interact with a specific point on the screen. To ensure compatibility with all W3C clients, coordinates locators can be passed as XPath or CssSelector.

### Overview

Coords locators enable the creation of virtual elements based on their absolute screen coordinates. This can be especially helpful for automating interactions with applications that render elements in fixed positions or when other locators are not feasible.

### Passing Coords Locators as XPath or CssSelector

To use Coords locators, you need to pass them using the syntax `coords(x,y)` in either XPath or CssSelector. The UiaDriver Server will interpret these locators and create virtual elements pointing to the specified coordinates. These virtual elements can then be interacted with using standard WebDriver Element endpoints such as `click` and `sendKeys`.

#### Example Code

Below are examples of how to use Coords locators in different languages by passing them as XPath or CssSelector.

#### Python Example

```python
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.common.options import BaseOptions

class UiaOptions(BaseOptions):
    def __init__(self):
        super().__init__()
        self.app = None
        self.uia_options = None

    def to_capabilities(self):
        return {
            "browserName": "Uia",
            "platformName": "windows",
            "uia:options": self.uia_options
        }

options = UiaOptions()
options.uia_options = {"app": "notepad.exe"}

driver = webdriver.Remote(command_executor="http://localhost:5555/wd/hub", options=options)

# Use Coords locator passed as XPath
coords_locator = "coords(100,200)"
element = driver.find_element(By.XPATH, coords_locator)
element.click()

driver.quit()
```

#### C# Example

```csharp
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using Uia.Models;

var options = new UiaOptions
{
    UiaOptionsDictionary = new
    {
        app = "notepad.exe"
    }
};

var driver = new RemoteWebDriver(new Uri("http://localhost:5555/wd/hub"), options.ToCapabilities());

// Use Coords locator passed as XPath
string coordsLocator = "coords(100,200)";
var element = driver.FindElement(By.XPath(coordsLocator));
element.Click();

driver.Quit();
```

#### Java Example

```java
package org.example;

import org.openqa.selenium.*;
import org.openqa.selenium.remote.RemoteWebDriver;

import java.net.MalformedURLException;
import java.net.URL;

public class Main {
    public static void main(String[] args) {
        UiaOptionsDictionary uiaOptionsDictionary = new UiaOptionsDictionary();
        uiaOptionsDictionary.setApp("notepad.exe");

        UiaOptions uiaOptions = new UiaOptions();
        uiaOptions.setUiaOptionsDictionary(uiaOptionsDictionary);

        WebDriver driver = null;
        try {
            driver = new RemoteWebDriver(new URL("http://localhost:5555/wd/hub"), uiaOptions.toCapabilities());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Use Coords locator passed as XPath
        String coordsLocator = "coords(100,200)";
        WebElement element = driver.findElement(By.xpath(coordsLocator));
        element.click();

        driver.quit();
    }
}
```

### Cursor Coordinate Tracker Tool

To find the screen coordinates of a point, you can use the [Cursor Coordinate Tracker tool](https://github.com/g4-api/cursor-coordinate-tracker). This tool helps you obtain the coordinates that can be passed as a locator value.

### Best Practices and Limitations

- **Accuracy**: Ensure that the coordinates provided are accurate and consistent with the screen resolution and scaling settings.
- **Performance**: Coordinates-based identification is generally faster but less reliable if the UI layout changes.
- **Usage**: Use coordinates to create virtual elements that point to static positions on the screen. These virtual elements can be interacted with using standard WebDriver Element endpoints such as `click` and `sendKeys`.

## Using the Object Model Locator

The Object Model locator in the UiaDriver Server allows you to create a virtual DOM from a specified point downward in the element tree. This enables full-featured XPath searching, which is particularly useful for refining element searches with very complicated conditions that cannot be achieved using the standard UIA XPath.

### Overview

Object Model locators enable the creation of a virtual DOM, allowing you to use comprehensive XPath queries. Unlike the standard UIA XPath, which is limited to logical operators and text/partial text only, the Object Model locator provides full XPath capabilities. This is useful for scenarios requiring complex element identification logic.

### Syntax

To start an Object Model search, use the syntax `//Window/Pane/ObjectModel:Pane`. From this point, the Pane tree becomes a virtual DOM with full XPath capabilities. The Object Model stops at the next element in the XPath.

**Warning**: Use this feature only as a last resort and on a small element scope since creating a DOM tree can be time-consuming.

### Example Code

Below are examples of how to use Object Model locators in different languages by passing them as XPath.

#### Python Example

```python
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.common.options import BaseOptions
from selenium.webdriver.support.wait import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.common.exceptions import WebDriverException, NoSuchElementException, StaleElementReferenceException

class UiaOptions(BaseOptions):
    def __init__(self):
        super().__init__()
        self.app = None
        self.uia_options = None

    def to_capabilities(self):
        return {
            "browserName": "Uia",
            "platformName": "windows",
            "uia:options": self.uia_options
        }

options = UiaOptions()
options.uia_options = {"app": "notepad.exe"}

driver = webdriver.Remote(command_executor="http://localhost:5555/wd/hub", options=options)
wait = WebDriverWait(driver, 20, ignored_exceptions=[WebDriverException, NoSuchElementException, StaleElementReferenceException])

# Use Object Model locator passed as XPath
object_model_locator = "//Window/Pane/ObjectModel:Pane//Button[@Name='Save']"
element = wait.until(EC.presence_of_element_located((By.XPATH, object_model_locator)))
element.click()

driver.quit()
```

#### C# Example

```csharp
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using Uia.Models;
using Uia.Support;

var options = new UiaOptions
{
    UiaOptionsDictionary = new
    {
        app = "notepad.exe"
    }
};

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

// Use Object Model locator passed as XPath
string objectModelLocator = "//Window/Pane/ObjectModel:Pane//Button[@Name='Save']";
var element = wait.Until(d => d.FindElement(By.XPath(objectModelLocator)));
element.Click();

driver.Quit();
```

#### Java Example

```java
package org.example;

import org.openqa.selenium.*;
import org.openqa.selenium.remote.RemoteWebDriver;
import org.openqa.selenium.support.ui.WebDriverWait;
import org.openqa.selenium.support.ui.ExpectedConditions;

import java.net.MalformedURLException;
import java.net.URL;
import java.time.Duration;
import java.util.Arrays;
import java.util.List;

public class Main {
    public static void main(String[] args) {
        UiaOptionsDictionary uiaOptionsDictionary = new UiaOptionsDictionary();
        uiaOptionsDictionary.setApp("notepad.exe");

        UiaOptions uiaOptions = new UiaOptions();
        uiaOptions.setUiaOptionsDictionary(uiaOptionsDictionary);

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

        // Use Object Model locator passed as XPath
        String objectModelLocator = "//Window/Pane/ObjectModel:Pane//Button[@Name='Save']";
        WebElement element = wait.until(ExpectedConditions.presenceOfElementLocated(By.xpath(objectModelLocator)));
        element.click();

        driver.quit();
    }
}
```

### Best Practices and Limitations

- **Performance**: Creating a virtual DOM tree can be time-consuming. Use Object Model locators only as a last resort and on small element scopes.
- **Complex Conditions**: This feature is ideal for refining element searches with complex conditions that are not feasible with standard UIA XPath.
- **Usage**: Start the Object Model search with the syntax `//Window/Pane/ObjectModel:Pane`. The virtual DOM stops at the next element in the XPath.

Back to the README.
