using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;

using Uia.DriverServer.Extensions;
using Uia.DriverServer.Models;

using UIAutomationClient;

using static System.Collections.Specialized.BitVector32;

namespace Uia.DriverServer.Domain
{
    public class SessionRepository : ISessionsRepository
    {
        // members
        private readonly ILogger<SessionRepository> _logger;
        private static readonly JsonSerializerOptions s_jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public SessionRepository(IDictionary<string, UiaSessionModel> sessions, ILogger<SessionRepository> logger)
        {
            Sessions = sessions;
            _logger = logger;
        }

        public IDictionary<string, UiaSessionModel> Sessions { get; }

        public (int StatusCode, object Entity) NewSession(UiaCapabilitiesModel capabilities)
        {
            // setup
            var isAppCapability = capabilities.Capabilities.ContainsKey(UiaCapabilities.Application);
            var isApp = isAppCapability && !string.IsNullOrEmpty($"{capabilities.Capabilities[UiaCapabilities.Application]}");
            var isDesktop = isApp && $"{capabilities.Capabilities[UiaCapabilities.Application]}".Equals("Desktop", StringComparison.OrdinalIgnoreCase);

            // create
            var (statusCode, response, seesion) = !isApp || isDesktop
                ? CreateDesktopSession(capabilities.Capabilities, _logger)
                : CreateApplicationSession(capabilities.Capabilities, _logger);

            // setup
            Sessions[seesion.SessionId] = seesion;

            // get
            return (statusCode, response);
        }

        public (int StatusCode, XDocument ElementsXml) NewDocumentObjectModel(string id)
        {
            throw new NotImplementedException();
        }

        public int DeleteSession(string id)
        {
            throw new NotImplementedException();
        }

        public (int StatusCode, string Entity) GetScreenshot()
        {
            throw new NotImplementedException();
        }

        public (int StatusCode, UiaSessionModel Session) GetSession(string id)
        {
            throw new NotImplementedException();
        }

        public (int StatusCode, RectangleModel Entity) SetWindowVisualState(string id, WindowVisualState visualState)
        {
            throw new NotImplementedException();
        }

        private static (int StatusCode, object Response, UiaSessionModel Seesion) CreateDesktopSession(IDictionary<string, object> capabilities, ILogger logger)
        {
            // setup
            var id = Guid.NewGuid();

            // build
            var session = new UiaSessionModel
            {
                SessionId = $"{id}",
                Capabilities = capabilities,
                Runtime = Array.Empty<int>(),
                TreeScope = TreeScope.TreeScope_Children,
            };

            // build
            var response = new
            {
                Value = new
                {
                    SessionId = id,
                    Capabilities = capabilities
                }
            };

            // get
            return (StatusCodes.Status200OK, response, session);
        }

        private static (int StatusCode, object Response, UiaSessionModel Seesion) CreateApplicationSession(IDictionary<string, object> capabilities, ILogger logger)
        {
            // constants
            const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

            // build
            var mount = capabilities.ContainsKey(UiaCapabilities.Mount) && ((JsonElement)capabilities[UiaCapabilities.Mount]).GetBoolean();
            var executeable = $"{capabilities[UiaCapabilities.Application]}";
            var arguments = capabilities.TryGetValue(UiaCapabilities.Arguments, out object argumentsOut) && argumentsOut != null
                ? JsonSerializer.Deserialize<IEnumerable<string>>($"{capabilities[UiaCapabilities.Arguments]}", s_jsonOptions)
                : Array.Empty<string>();
            var scaleRatio = capabilities.TryGetValue(UiaCapabilities.ScaleRatio, out object scaleOut)
                ? double.Parse(scaleOut.ToString())
                : 1.0D;
            var impersonation = capabilities.TryGetValue(UiaCapabilities.Impersonation, out object impersonationOut) && impersonationOut != null
                ? JsonSerializer.Deserialize<ImpersonationModel>($"{impersonationOut}", s_jsonOptions)
                : default;

            // get session
            var process = mount
                ? Array.Find(Process.GetProcesses(), i => executeable.Contains(i.ProcessName, Compare))
                : UiaUtilities.StartProcess(impersonation, executeable, string.Join(" ", arguments), "");

            // internal server error
            if (process?.MainWindowHandle == default && process.Handle == default && (process.SafeHandle.IsInvalid || process.SafeHandle.IsClosed))
            {
                return (StatusCodes.Status500InternalServerError, default, default);
            }

            // build session
            var session = new UiaSessionModel(new CUIAutomation8(), process)
            {
                Capabilities = capabilities,
                TreeScope = TreeScope.TreeScope_Children,
                ScaleRatio = scaleRatio
            };

            // log
            var id = session.SessionId;
            var name = session.Application.GetNameOrFile();

            logger?.LogInformation("Create-Session -Session {id} -Application {name} = Created", id, name);
            logger?.LogInformation("Get-VirtualDom = /session/{id}", id);

            // get
            var response = new
            {
                Value = new
                {
                    session.SessionId,
                    Capabilities = capabilities
                }
            };
            return (StatusCodes.Status200OK, response, session);
        }
    }
}
