
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Demo.BackendService
{
    public class DemoHostedServiceOptions
    {
        [Required]
        public string QueueName { get; set; } = default!;

        [Required]
        public string Container { get; set; } = default!;
    }

    [OptionsValidator]
    public partial class ValidateDemoHostedServiceOptions : IValidateOptions<DemoHostedServiceOptions>
    {
    }
}
