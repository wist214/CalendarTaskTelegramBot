using CalendarEvent.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CalendarEvent.TelegramBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController(IMediator mediator) : ControllerBase
{

    [HttpGet("oauth-callback")]
    public async Task<IActionResult> OAuthCallback(string code, string state)
    {
        await mediator.Send(new ProcessOAuthCallbackCommand(code, state));
        return Content("<html><body>Login successful. You can close this window.</body></html>", "text/html");
    }
}