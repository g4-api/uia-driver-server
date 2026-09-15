using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain.UnitTests
{
    [TestClass]
    [TestCategory(nameof(DocumentRepository))]
    [TestCategory("IntegrationTest")]
    [DoNotParallelize]
    public sealed class DocumentRepositoryTests
    {
        // Retains the caller's process-local temporary paths for restoration after each isolated invocation.
        private string _originalTemporaryPath;

        // Retains TEMP independently because Windows can use it when TMP is unavailable.
        private string _originalTemporaryDirectory;

        // Owns a unique directory containing spaces so every PowerShell test exercises literal file invocation.
        private string _scriptDirectory;

        [TestCleanup]
        public void ClearScriptDirectory()
        {
            // Cleanup: restore the test host environment before removing this test's owned temporary directory.
            Environment.SetEnvironmentVariable(variable: "TMP", value: _originalTemporaryPath);
            Environment.SetEnvironmentVariable(variable: "TEMP", value: _originalTemporaryDirectory);

            // Cleanup: remove only the directory allocated by this fixture, including leftovers from failed assertions.
            if (_scriptDirectory != null && Directory.Exists(path: _scriptDirectory))
            {
                Directory.Delete(path: _scriptDirectory, recursive: true);
            }
        }

        [TestInitialize]
        public void InitializeScriptDirectory()
        {
            // Arrange: retain process state before redirecting file creation into an isolated directory with spaces.
            _originalTemporaryPath = Environment.GetEnvironmentVariable(variable: "TMP");
            _originalTemporaryDirectory = Environment.GetEnvironmentVariable(variable: "TEMP");
            _scriptDirectory = Path.Combine(path1: Path.GetTempPath(), path2: $"UIA script tests {Guid.NewGuid():N}");
            Directory.CreateDirectory(path: _scriptDirectory);

            // Arrange: control both temporary-path variables; DoNotParallelize protects other tests from this state.
            Environment.SetEnvironmentVariable(variable: "TMP", value: _scriptDirectory);
            Environment.SetEnvironmentVariable(variable: "TEMP", value: _scriptDirectory);
        }

        [TestMethod(DisplayName = "Verify that CMD dispatch still executes through the repository.")]
        public void InvokeScriptCmdDispatchTest()
        {
            // Arrange: select the existing static CMD implementation to protect mixed instance/static dispatch.
            var repository = NewRepository(terminal: "Cmd", logger: new TestLogger());

            // Act: execute a harmless command through the same reflection path used by PowerShell.
            var response = repository.InvokeScript(session: "script-test", src: "@echo off\r\necho cmd-completed");

            // Assert: preserve the established successful response and temporary-file cleanup.
            Assert.AreEqual(expected: 200, actual: response.StatusCode);
            Assert.AreEqual(expected: "cmd-completed", actual: response.Result);
            Assert.IsEmpty(collection: Directory.GetFiles(path: _scriptDirectory));
        }

        [TestMethod(DisplayName = "Verify that PowerShell failures expose diagnostics and delete their temporary files.")]
        #region *** Data Set ***
        [DataRow("[Console]::Error.Write('failure-marker'); exit 17", "code 17", "failure-marker")]
        [DataRow("throw 'terminating-marker'", "code 1", "terminating-marker")]
        [DataRow("exit 23", "code 23", "Standard error:")]
        [DataRow("if (", "code 1", "ParserError")]
        #endregion
        public void InvokeScriptFailureTest(string script, string expectedExitCode, string expectedDiagnostic)
        {
            // Arrange: use the default terminal selection to exercise the production PowerShell routing contract.
            var repository = NewRepository(terminal: null, logger: new TestLogger());

            // Act: capture the reflection wrapper that the existing middleware unwraps through GetBaseException.
            var exception = Assert.ThrowsExactly<TargetInvocationException>(
                action: () => repository.InvokeScript(session: "script-test", src: script));
            var failure = exception.GetBaseException();

            // Assert: retain the actionable child failure instead of producing a successful empty response.
            Assert.IsInstanceOfType<InvalidOperationException>(value: failure);
            Assert.Contains(substring: expectedExitCode, value: failure.Message);
            Assert.Contains(substring: expectedDiagnostic, value: failure.Message);
            Assert.Contains(substring: _scriptDirectory, value: failure.Message);
            Assert.IsEmpty(collection: Directory.GetFiles(path: _scriptDirectory));
        }

        [TestMethod(DisplayName = "Verify that large standard error is drained and logged without changing successful output.")]
        public void InvokeScriptLargeStandardErrorTest()
        {
            // Arrange: emit more data than a redirected pipe can hold while keeping a successful exit code.
            var logger = new TestLogger();
            var repository = NewRepository(terminal: "Powershell", logger);
            var script = "[Console]::Error.Write(('E' * 131072)); Write-Output 'completed'; exit 0";

            // Act: drain both streams through the synchronous endpoint without blocking on the full error pipe.
            var response = repository.InvokeScript(session: "script-test", src: script);

            // Assert: keep stderr in diagnostics and preserve the successful stdout response contract.
            Assert.AreEqual(expected: 200, actual: response.StatusCode);
            Assert.AreEqual(expected: "completed", actual: response.Result);
            Assert.HasCount(expected: 1, collection: logger.Messages);
            Assert.Contains(substring: new string(c: 'E', count: 131072), value: logger.Messages[0]);
            Assert.IsEmpty(collection: Directory.GetFiles(path: _scriptDirectory));
        }

        [TestMethod(DisplayName = "Verify that Windows PowerShell preserves non-ASCII script literals.")]
        public void InvokeScriptNonAsciiSourceTest()
        {
            // Arrange: inspect the literal's numeric value so console output encoding cannot hide source corruption.
            var repository = NewRepository(terminal: "Powershell", logger: new TestLogger());

            // Act: write a Unicode literal through the repository's temporary-script encoding path.
            var script = "Write-Output (([IO.File]::ReadAllBytes($PSCommandPath)[0..2]) -join ','); " +
                "Write-Output ([int][char]'\u00e9')";
            var response = repository.InvokeScript(session: "script-test", src: script);

            // Assert: require the UTF-8 BOM even on hosts whose default code page already decodes UTF-8 correctly.
            Assert.AreEqual(expected: 200, actual: response.StatusCode);
            Assert.AreEqual(expected: $"239,187,191{Environment.NewLine}233", actual: response.Result);
            Assert.IsEmpty(collection: Directory.GetFiles(path: _scriptDirectory));
        }

        [TestMethod(DisplayName = "Verify that nonterminating PowerShell errors are logged when the script succeeds.")]
        public void InvokeScriptNonterminatingErrorTest()
        {
            // Arrange: a nonterminating error followed by output leaves a zero exit code in file mode.
            var logger = new TestLogger();
            var repository = NewRepository(terminal: "Powershell", logger);

            // Act: exercise PowerShell's own error stream rather than a native program's stderr.
            var response = repository.InvokeScript(
                session: "script-test",
                src: "Write-Error 'nonterminating-marker'; Write-Output 'completed'");

            // Assert: retain successful stdout while making the nonterminating error visible in diagnostics.
            Assert.AreEqual(expected: 200, actual: response.StatusCode);
            Assert.AreEqual(expected: "completed", actual: response.Result);
            Assert.HasCount(expected: 1, collection: logger.Messages);
            Assert.Contains(substring: "nonterminating-marker", value: logger.Messages[0]);
            Assert.IsEmpty(collection: Directory.GetFiles(path: _scriptDirectory));
        }

        [TestMethod(DisplayName = "Verify that a script in a path with spaces executes and its temporary file is removed.")]
        public void InvokeScriptPathWithSpacesTest()
        {
            // Arrange: use case-insensitive terminal selection and the fixture's real spaced temporary directory.
            var repository = NewRepository(terminal: "pOwErShElL", logger: new TestLogger());

            // Act: return the executing script's own path to prove that its body ran from the controlled directory.
            var response = repository.InvokeScript(session: "script-test", src: "Write-Output $PSCommandPath");
            var scriptPath = response.Result as string;

            // Assert: successful execution uses the intended directory and leaves no script behind.
            Assert.AreEqual(expected: 200, actual: response.StatusCode);
            Assert.IsNotNull(value: scriptPath);
            Assert.AreEqual(expected: _scriptDirectory, actual: Path.GetDirectoryName(path: scriptPath));
            Assert.IsFalse(condition: File.Exists(path: scriptPath));
            Assert.IsEmpty(collection: Directory.GetFiles(path: _scriptDirectory));
        }

        // Creates an isolated desktop session for public repository dispatch without launching an application.
        // The caller owns the logger and the fixture owns any temporary scripts produced by the request.
        private static DocumentRepository NewRepository(string terminal, TestLogger logger)
        {
            // Arrange: register only the session consumed by this test, using the existing UIA model constructor.
            var sessions = new Dictionary<string, UiaSessionResponseModel>
            {
                ["script-test"] = new UiaSessionResponseModel
                {
                    Options = new UiaOptionsModel { Terminal = terminal }
                }
            };

            return new DocumentRepository(sessions, logger);
        }

        // Captures repository diagnostics in memory so tests can distinguish stderr from returned stdout.
        private sealed class TestLogger : ILogger<SessionsRepository>
        {
            #region *** Properties   ***

            /// <summary>
            /// Gets formatted diagnostics emitted during the current test's script invocation.
            /// </summary>
            public List<string> Messages { get; } = [];

            #endregion

            #region *** Methods      ***

            /// <inheritdoc />
            public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;

            /// <inheritdoc />
            public bool IsEnabled(LogLevel logLevel) => true;

            /// <inheritdoc />
            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception exception,
                Func<TState, Exception, string> formatter)
            {
                // Retain the full formatted diagnostic so tests can assert that even large error streams survive.
                Messages.Add(item: formatter(state, exception));
            }

            #endregion
        }
    }
}
