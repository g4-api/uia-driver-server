# Integration with Java

To integrate the Windows WebDriver Automation Service with Java, you can use the Selenium WebDriver. Below is a detailed example of how to set up and use the service for automating Windows applications using Java:

## Installing Selenium Client Libraries with Maven

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

## Example Code

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
         * @return the username.
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

## Breakdown and Explanation

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

Back to the README.
