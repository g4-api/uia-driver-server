# Endpoints

The server implements the [W3C WebDriver protocol](https://www.w3.org/TR/webdriver/) for Windows applications. This page lists every route that the controllers in `src\Uia.DriverServer\Controllers\` expose. For request and response models, use the Swagger page that the running server hosts.

## Where to find the live API description

| What | Address | Source |
| --- | --- | --- |
| Swagger UI | `/swagger` | `src\Uia.DriverServer\Program.cs` (`UseSwaggerUI`) |
| OpenAPI document | `/swagger/v4/swagger.json` | `src\Uia.DriverServer\Program.cs` (`SwaggerEndpoint`) |

## Base path

Every controller except Meta is mapped twice: under `/wd/hub` and at the root. `POST /wd/hub/session` and `POST /session` reach the same action. Selenium clients use the `/wd/hub` form. The routes below are written without that prefix.

The default port is 5555. Start the server with `--port <number>` to change it (`Program.cs`, `GetPort`).

## Route list

Each summary is quoted from the `SwaggerOperation` attribute of the action.

### Sessions (SessionsController.cs)

| Verb | Route | Swagger summary |
| --- | --- | --- |
| POST | `session` | Creates a new session with the specified capabilities. |
| DELETE | `session/{session}` | Deletes the specified session. |
| DELETE | `session` | Deletes all active sessions. |
| GET | `status` | Gets the status of the current sessions. |
| GET | `session/{session}/window` | Gets the handle of the currently focused window for the specified session. |
| POST | `session/{session}/window` | Switches to the specified window in the session. |
| GET | `session/{session}/window/handles` | Gets all window handles for the specified session. |
| GET | `session/{session}/dom` | Gets the Document Object Model (DOM) for the specified session. |
| POST | `session/{session}/element/dom` | Creates the Document Object Model (DOM) for a specific element within a session. |

### Window and document (ContextController.cs, DocumentController.cs, ScreenCaptureController.cs)

| Verb | Route | Swagger summary |
| --- | --- | --- |
| POST | `session/{session}/window/maximize` | Maximizes the window for the specified session. |
| GET | `session/{session}/source` | Retrieves the document source for the specified session. |
| POST | `session/{session}/execute/sync` | Executes a script in the context of the specified session. |
| GET | `session/{session}/screenshot` | Captures a screenshot for the specified session. |

### Elements (ElementsController.cs)

| Verb | Route | Swagger summary |
| --- | --- | --- |
| POST | `session/{session}/element` | Finds an element in the specified session using the provided locator strategy. |
| POST | `session/{session}/elements` | Finds elements in the specified session using the provided locator strategy. |
| POST | `session/{session}/element/{element}/element` | Finds an element within the specified element in the given session using the provided locator strategy. |
| POST | `session/{session}/element/{element}/elements` | Finds elements within the specified element in the given session using the provided locator strategy. |
| GET | `session/{session}/element/{element}/attribute/{name}` | Gets the specified attribute of the specified element in the given session. |
| GET | `session/{session}/element/{element}/text` | Gets the text of the specified element in the given session. |
| POST | `session/{session}/element/{element}/click` | Invokes a click action on the specified element in the given session. |
| POST | `session/{session}/element/{element}/clear` | Clears the value of the specified element in the given session. |
| POST | `session/{session}/element/{element}/value` | Sets the value of the specified element in the given session. |

### Actions (ActionsController.cs)

| Verb | Route | Swagger summary |
| --- | --- | --- |
| POST | `session/{session}/actions` | Sends User32 actions to the specified session. |

### User32 extension (User32Controller.cs)

| Verb | Route | Swagger summary |
| --- | --- | --- |
| GET | `session/{session}/user32/layouts` | Retrieves available keyboard layouts. |
| POST | `session/{session}/user32/layouts` | Sets the keyboard layout. |
| POST | `session/{session}/user32/value` | Sends keystrokes to the specified session. |
| POST | `session/{session}/user32/inputs` | Sends key scan codes to the specified session. |
| POST | `session/{session}/user32/modified` | Sends a modified key (e.g., Ctrl+C) to the specified session. |
| POST | `session/{session}/user32/paste` | Invokes a paste action in the specified session. |
| POST | `session/{session}/user32/element/{element}/copy` | Invokes the copy action on the specified element in the given session. |
| POST | `session/{session}/user32/element/{element}/select` | Selects the specified element in the given session. |
| GET | `session/{session}/user32/element/{element}/focus` | Sets focus on the specified element in the given session. |
| POST | `session/{session}/user32/click` | Performs a native click at the specified coordinates in the given session. |
| POST | `session/{session}/user32/dclick` | Invokes a double-click action at the specified coordinates in the given session. |
| POST | `session/{session}/user32/element/{element}/click` | Invokes a click action on the specified element in the given session. |
| POST | `session/{session}/user32/element/{element}/dclick` | Invokes a double-click action on the specified element in the given session. |
| POST | `session/{session}/user32/mouse/move` | Sets the mouse position to the specified coordinates in the given session. |
| POST | `session/{session}/user32/element/{element}/mouse/move` | Sets the mouse position to the specified element in the given session. |

### Meta (MetaController.cs)

| Verb | Route | Swagger summary |
| --- | --- | --- |
| GET | `/scancodes` | Retrieves available keyboard scan code maps. |

## Notes

- The User32 routes are an extension. They are not part of the W3C protocol. They send native keystrokes and mouse input instead of using UI Automation patterns.
- `GET /scancodes` is the only route outside `/wd/hub`. Its action route starts with `/`, so it ignores the `api/meta` controller prefix. Source: `MetaController.cs`.
- The DOM routes build a virtual document that the [Object Model locator](locators.md) searches.
- Errors and their causes are on the [troubleshooting](troubleshooting.md) page.

## Reading Element Text

`GET /session/{session}/element/{element}/text` (`element.Text` in WebDriver clients) reads text through UI Automation patterns, in this order, and returns the first non-empty result:

1. **Value** pattern (edit boxes, combo boxes, address bars)
2. **Text**, **Text2**, **TextEdit** patterns (rich-text and document controls)
3. **TextChild** pattern (text ranges inside a text container)
4. The element's **Name** property, when no pattern produces text

A pattern that fails or is not supported is skipped, so text exposed by another pattern is still returned.

### Password fields

**Passwords cannot be read.** Windows deliberately hides the contents of password fields (elements where `IsPassword` is `true`, such as WPF/WinUI `PasswordBox` or a browser `<input type="password">`): the Value pattern refuses to return the value and the Text pattern returns only mask characters. For these elements the driver always returns an empty string for `.Text`, never the mask characters, and this cannot be overridden. If a flow needs a password, supply it from your own configuration or secret store instead of reading it from the screen.

Back to the README.
