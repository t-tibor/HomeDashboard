using HomeDashboard.Web.MQTT;
using HomeDashboard.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace HomeDashboard.Web.Pages;

public class IndexModel : PageModel
{
	private readonly ILogger<IndexModel> _logger;
	private readonly ILightControlService _lightControlService;

	public IndexModel(ILogger<IndexModel> logger, ILightControlService lightControlService)
	{
		_logger = logger;
		_lightControlService = lightControlService;
	}

	public Task<IActionResult> OnGetAsync()
	{
		return Task.FromResult<IActionResult>(Page());
	}

	public async Task<IActionResult> OnPostAsync(string mode)
	{
		if (string.IsNullOrEmpty(mode))
		{
			return Page();
		}

		switch (mode)
		{
			case "on":
				await _lightControlService.StartTurnOnAsync(255);
				break;
			case "mild":
				await _lightControlService.StartTurnOnAsync(26);
				break;
			case "off":
				await _lightControlService.StartTurnOffAsync();
				break;
			default:
				return NotFound("Invalid mode");
		}

		return Page();
	}
}
