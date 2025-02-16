using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Frontend.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly Trigger _trigger;

        public IndexModel(ILogger<IndexModel> logger, Trigger trigger)
        {
            _logger = logger;
            _trigger = trigger;
        }

        public void OnGet()
        {

        }

        public Task OnPostTrigger(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Trigger clicked");
            return _trigger.Send(cancellationToken);
        }
    }
}
