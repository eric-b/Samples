using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Demo.Frontend
{
    public class TriggerOptions
    {
        [Required]
        public string QueueName { get; set; } = default!;
    }

    [OptionsValidator]
    public partial class ValidateTriggerOptions : IValidateOptions<TriggerOptions>
    {
    }
}
