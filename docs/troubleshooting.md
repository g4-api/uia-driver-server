# UIA Driver Server Troubleshooting

This page lists the error messages that the UIA driver server produces, with the cause, a check and a fix for each one. Every message was found in the source under `src\`.

Paths in `Source` lines are relative to the `src` folder of this repository.

Errors arrive in two shapes. WebDriver errors come back as a `value` object with `error` and `message`. Unhandled exceptions come back as HTTP 500 with the root exception message and a stack trace.

Back to the README.

## Session errors

### invalid session id

**Cause:** The request used a session id that the server does not hold. The session was deleted, the server was restarted, or the id is wrong. The full message is "The session ID provided is invalid. The session might have been closed or does not exist." followed by `(Session info: UiaDriver=<id>)`.

**Check:** Call `GET /status` and read the `sessions` list in the answer.

**Fix:** Create a new session with `POST /session`. A `DELETE` request skips the session check, so it never gets this error.

**Source:** `Uia.DriverServer.Models/WebDriverResponseModel.cs:133`, `Uia.DriverServer/Middlewares/ErrorHandlingMiddleware.cs:85, 201-217`

### Current sessions stack is full, the maximum allowed sessions number is 1

**Cause:** `GET /status` reports this text when at least one session exists, and then sets `ready` to `false`.

**Check:** Read `ready` and `sessions` in the `GET /status` answer. Whether `POST /session` refuses a second session was not found in the code. The status text is the only place that states the limit of 1.

**Fix:** Delete the old session with `DELETE /session/{id}` before you create the next one. Deleting also kills the application process tree of that session.

**Source:** `Uia.DriverServer/Controllers/SessionsController.cs:182-186`, `Uia.DriverServer.Domain/SessionsRepository.cs:42-73`

### Session creation answers HTTP 500 with no body

**Cause:** The server could not start or find the application. The process is missing, it exited right away, or it has no usable handle. With `mount` set, the server looks for a running process whose name is part of the `app` value. Otherwise it starts `app` with the `arguments`.

**Check:** Read the server log line "Failed to create session. StatusCode: 500". Check the `app`, `arguments`, `workingDirectory` and `mount` values in `uia:options`.

**Fix:** Use a full path to the executable. For a process that is already running, set `mount` to `true` and make sure the process name is contained in `app`. Start the application yourself first if it needs a long start up.

**Source:** `Uia.DriverServer.Domain/SessionsRepository.cs:401-423`, `Uia.DriverServer.Domain/SessionsRepository.cs:211`

## Element errors

### no such element

**Cause:** The locator matched nothing. The message is `Unable to locate element: {"method": "<using>", "selector":"<value>"}`. The HTTP status is 404.

**Check:** Read the `method` and `selector` inside the message. Look at the element in the UIA inspector of your choice and compare names and ids.

**Fix:** Correct the locator, or wait for the window to finish drawing and retry.

**Source:** `Uia.DriverServer.Models/WebDriverResponseModel.cs:64-65`, `Uia.DriverServer/Controllers/ElementsController.cs:91-94`

### stale element reference

**Cause:** The element id is no longer in the session element cache. The full message is "Stale element reference: element is not attached to the application document" followed by `(Session info: UiaDriver=<id>)`. The HTTP status is 404.

**Check:** See whether the window or the control was closed or rebuilt after the element was found.

**Fix:** Find the element again and use the new id.

**Source:** `Uia.DriverServer.Models/WebDriverResponseModel.cs:155-156`, `Uia.DriverServer/Controllers/ElementsController.cs:58`

### Element or session not found

**Cause:** A user32 route could not resolve the session id or the element id.

**Check:** Confirm the session is listed by `GET /status` and that the element id came from this session.

**Fix:** Create the session again and find the element again.

**Source:** `Uia.DriverServer/Controllers/User32Controller.cs:171`, `Uia.DriverServer/Controllers/User32Controller.cs:434`

### The locator answers HTTP 400 with no body

**Cause:** The locator hierarchy was empty after it was parsed.

**Check:** Send a non-empty locator value for the strategy you use.

**Fix:** Send a locator with at least one segment.

**Source:** `Uia.DriverServer.Domain/ElementsRepository.cs:117-121`

### Unsupported control type: NAME

**Cause:** An XPath locator uses a control type name that the parser does not know. The server answers HTTP 500 with this message.

**Check:** Compare the name with the control type names in the parser mapping.

**Fix:** Use a supported control type name, or locate by another property.

**Source:** `Uia.DriverServer.Domain/XpathParser.cs:158-160`

### Unsupported property: NAME

**Cause:** An XPath locator uses an attribute that is not in the property mapping of the parser.

**Check:** Compare the attribute with the property mapping in the parser.

**Fix:** Use a supported property, or locate by another property.

**Source:** `Uia.DriverServer.Domain/XpathParser.cs:226-229`

### Unsupported operator: NAME

**Cause:** An XPath locator uses a logical operator that is not supported.

**Check:** Look for operators other than the ones the parser builds conditions for.

**Fix:** Rewrite the condition with the supported operators.

**Source:** `Uia.DriverServer.Domain/XpathParser.cs:199`

## Window errors

### no such window

**Cause:** The window switch request used a handle or name that does not exist. The server answers HTTP 404 with `error` set to `no such window`. A status 400 gives `invalid argument`.

**Check:** Call the window handles route and compare the handle you send.

**Fix:** Send a handle taken from the handles list of the same session.

**Source:** `Uia.DriverServer/Controllers/SessionsController.cs:308-316`

### The window state change answers HTTP 500

**Cause:** The window pattern is not available for the session root element, so the window visual state cannot be set. The log line is "Window pattern not available for session with ID <id>."

**Check:** Read the server log for that line. The session may be a desktop session, or the root control may not be a window.

**Fix:** Run the operation on a session that was started for an application window.

**Source:** `Uia.DriverServer.Domain/SessionsRepository.cs:339-342`

## Input errors

### The request body must be JSON formatted

**Cause:** A route that reads a body got text that is not JSON. The server answers HTTP 500 with this message.

**Check:** Look at the request `Content-Type` and the body.

**Fix:** Send a JSON body.

**Source:** `Uia.DriverServer.Extensions/DotnetExtensions.cs:144`

### Bad Request: The request could not be understood by the server due to malformed syntax

**Cause:** A request body failed model validation. The server rewrites the 400 answer to this text.

**Check:** Read the server log line "A BadRequest response was handled" for the original body.

**Fix:** Correct the field that the original body names.

**Source:** `Uia.DriverServer/Middlewares/ErrorHandlingMiddleware.cs:101-108`, `Uia.DriverServer/Middlewares/ErrorHandlingMiddleware.cs:153-154`

### Invalid alignment value. Allowed values: TopLeft, MiddleLeft, BottomLeft, TopCenter, MiddleCenter, BottomCenter, TopRight, MiddleRight, BottomRight

**Cause:** The mouse position request has an `alignment` value outside the list.

**Check:** Compare the value, including its letter case, with the list.

**Fix:** Use one of the nine values.

**Source:** `Uia.DriverServer.Models/MousePositionInputModel.cs:22-26`

### Invalid mouse event code

**Cause:** The mouse event code passed to the user32 layer is not one of the known codes. The argument name in the exception is `eventCode`.

**Check:** Read the stack trace of the HTTP 500 answer.

**Fix:** Use a mouse action that the user32 routes list.

**Source:** `Uia.DriverServer.Marshals/User32.cs:402`

### Failed to get cursor position

**Cause:** The Win32 call that reads the cursor position returned false. The code does not read the Win32 error code, so the reason is not recorded.

**Check:** Look at the stack trace in the HTTP 500 answer. Check whether the desktop is locked or the server runs in a session without an input desktop.

**Fix:** Run the server in the interactive desktop session of a signed-in user.

**Source:** `Uia.DriverServer.Marshals/User32.cs:357`

### Failed to set cursor position

**Cause:** The Win32 call that moves the cursor returned false, in either overload.

**Check:** Same as the previous card.

**Fix:** Same as the previous card.

**Source:** `Uia.DriverServer.Marshals/User32.cs:447`, `Uia.DriverServer.Marshals/User32.cs:463`

## Script errors

### PowerShell script PATH exited with code N

**Cause:** A script that the server ran as a child process ended with a non-zero code. The message adds "Standard error:" and the error text. The server answers HTTP 500.

**Check:** Read the standard error text in the message and in the server log.

**Fix:** Correct the script. Run it by hand in the same account as the server.

**Source:** `Uia.DriverServer.Domain/DocumentRepository.cs:229-244`

## Any other exception

### Internal Server Error

**Cause:** An exception reached the error middleware. The answer holds `value.error` set to "Internal Server Error", the root exception message and the stack trace. The same exception is written to the log as "An unhandled exception occurred."

**Check:** Read `value.message` and `value.stacktrace`. Search this page for the message.

**Fix:** Follow the card for that message. If none matches, report the stack trace.

**Source:** `Uia.DriverServer/Middlewares/ErrorHandlingMiddleware.cs:123-126`, `Uia.DriverServer/Middlewares/ErrorHandlingMiddleware.cs:173-193`
