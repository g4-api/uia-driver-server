using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Uia.DriverServer.Attributes;
using Uia.DriverServer.Extensions;

namespace Uia.DriverServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag(description:
        "Provides metadata endpoints for runtime automation clients, " +
        "including shared lookup data such as keyboard scan code maps and " +
        "other static definitions required by User32-based operations.")]
    public class MetaController : ControllerBase
    {
        [HttpGet]
        [Route("/scancodes")]
        [SwaggerOperation(
            Summary = "Retrieves available keyboard scan code maps.",
            Description = "Gets all keyboard scan code maps exposed by the CodeMaps class. Only public static properties decorated with KeyboardLayoutAttribute are included."
        )]
        [SwaggerResponse(200, "Keyboard scan code maps retrieved successfully.", typeof(Dictionary<string, object>))]
        public IActionResult GetCodeMaps()
        {
            // Retrieve all public static properties from the CodeMaps class.
            // Only properties decorated with KeyboardLayoutAttribute are considered valid keyboard scan code maps.
            var maps = typeof(CodeMaps)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(i => i.GetCustomAttribute<KeyboardLayoutAttribute>() != null);

            // Initialize the response payload.
            // The key is the code-map property name.
            // The value is the static property value exposed by CodeMaps.
            var response = new Dictionary<string, object>();

            // Iterate through each discovered keyboard scan code map.
            foreach (var map in maps)
            {
                // Read the KeyboardLayoutAttribute from the property.
                // This check is intentionally kept even though the LINQ filter already validates it,
                // so the loop remains safe if the filtering logic changes later.
                var attribute = map.GetCustomAttribute<KeyboardLayoutAttribute>();

                if (attribute != null)
                {
                    // Static properties do not require an instance, so null is passed to GetValue.
                    response.Add(attribute.Layout, map.GetValue(null));
                }
            }

            // Return all discovered keyboard scan code maps to the caller.
            return Ok(response);
        }
    }
}
