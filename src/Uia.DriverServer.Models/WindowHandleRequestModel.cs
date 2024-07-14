using System.ComponentModel.DataAnnotations;

namespace Uia.DriverServer.Models
{
    public class WindowHandleRequestModel
    {
        [Required]
        public string Handle { get; set; }
    }
}
