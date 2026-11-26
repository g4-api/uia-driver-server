using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Uia.DriverServer.Marshals;
using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain.UnitTests
{
    [TestClass]
    [TestCategory("SessionsRepository")]
    [TestCategory("UnitTest")]
    public sealed class SessionsRepositoryTests
    {
        // The session id the tests register, so the delete call and the dictionary check share one key.
        private const string SessionId = "session-under-test";

        [TestMethod(DisplayName = "Verify that deleting a mounted session succeeds, removes the session and " +
            "leaves the mounted application running.")]
        public void DeleteMountedSessionTest()
        {
            // Arrange: a running helper process, looked up by id the way mount finds a process, so its StartInfo
            // cannot be read.
            using var helper = StartHelperProcess();
            using var mounted = Process.GetProcessById(helper.Id);

            try
            {
                var sessions = NewSessions(application: mounted, isMounted: true);
                var repository = new SessionsRepository(sessions, logger: null);

                // Act: delete the session; before the fix this threw InvalidOperationException from StartInfo.
                var statusCode = repository.DeleteSession(SessionId);

                // Assert: the request succeeds, the session is gone and the mounted application still runs.
                Assert.AreEqual(expected: StatusCodes.Status200OK, actual: statusCode);
                Assert.IsFalse(sessions.ContainsKey(SessionId));
                Assert.IsFalse(helper.HasExited);
            }
            finally
            {
                // Cleanup: stop the helper process, which the driver intentionally left running.
                StopHelperProcess(helper);
            }
        }

        [TestMethod(DisplayName = "Verify that deleting a session whose application the driver started kills " +
            "the application and removes the session.")]
        public void DeleteStartedSessionTest()
        {
            // Arrange: a helper process started by this test, as the driver starts an application.
            using var helper = StartHelperProcess();

            try
            {
                var sessions = NewSessions(application: helper, isMounted: false);
                var repository = new SessionsRepository(sessions, logger: null);

                // Act: delete the session.
                var statusCode = repository.DeleteSession(SessionId);

                // Assert: the request succeeds, the session is gone and the application has exited.
                Assert.AreEqual(expected: StatusCodes.Status200OK, actual: statusCode);
                Assert.IsFalse(sessions.ContainsKey(SessionId));
                Assert.IsTrue(helper.WaitForExit(milliseconds: 10000));
            }
            finally
            {
                // Cleanup: stop the helper process in case the delete did not.
                StopHelperProcess(helper);
            }
        }

        [TestMethod(DisplayName = "Verify that bringing a missing window to the foreground does nothing and " +
            "reports false.")]
        public void ShowMissingWindowInForegroundTest()
        {
            // Act: ask for the foreground with a zero handle, which names no window.
            var isForeground = User32.ShowWindowInForeground(IntPtr.Zero);

            // Assert: the request is ignored and reported as not done.
            Assert.IsFalse(isForeground);
        }

        // Creates the session dictionary with one application session, mounted or started.
        private static Dictionary<string, UiaSessionResponseModel> NewSessions(Process application, bool isMounted)
        {
            // Build the session the way the driver stores it, with the options it was created from.
            var session = new UiaSessionResponseModel
            {
                Application = application,
                Options = new UiaOptionsModel
                {
                    App = "ping",
                    Mount = isMounted
                },
                SessionId = SessionId
            };

            // Return a dictionary that holds only that session.
            return new Dictionary<string, UiaSessionResponseModel>
            {
                [SessionId] = session
            };
        }

        // Starts a windowless helper process that runs for about a minute, long enough for every test.
        private static Process StartHelperProcess()
        {
            // Describe a hidden ping that the tests can mount, kill and observe.
            var startInfo = new ProcessStartInfo
            {
                Arguments = "-n 60 127.0.0.1",
                CreateNoWindow = true,
                FileName = "ping.exe",
                UseShellExecute = false
            };

            // Start the helper and hand it to the test, which owns its cleanup.
            return Process.Start(startInfo);
        }

        // Stops a helper process that is still running, so no test leaves a process behind.
        private static void StopHelperProcess(Process helper)
        {
            // Kill and wait only while the helper runs, because a test may have stopped it already.
            if (!helper.HasExited)
            {
                helper.Kill();
                helper.WaitForExit(milliseconds: 10000);
            }
        }
    }
}
