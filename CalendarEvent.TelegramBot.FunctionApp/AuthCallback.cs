using System.Net;
using CalendarEvent.Application.Commands;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CalendarEvent.TelegramBot.FunctionApp;

public class AuthCallback(IMediator mediator, ILogger<Function> logger)
{
    [Function("oauth2callback")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        try
        {
            var q = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var code = q["code"];
            var state = q["state"];
            await mediator.Send(new ProcessOAuthCallbackCommand(code, state));

            var html = @"
<!DOCTYPE html>
<html lang=""ru"">
<head>
  <meta charset=""UTF-8""/>
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
  <title>Успешно!</title>
  <style>
    body {
      margin: 0;
      height: 100vh;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      background: #f0f4f8;
      font-family: sans-serif;
      color: #333;
    }
    .checkmark {
      width: 120px;
      height: 120px;
      margin-bottom: 20px;
    }
    h1 {
      margin: 0;
      font-size: 24px;
    }
    p {
      margin: 8px 0 0;
      font-size: 16px;
      color: #555;
    }
    /* Анимация круга */
    .circle {
      fill: none;
      stroke: #4CAF50;
      stroke-width: 4;
      stroke-linecap: round;
      animation: pop 0.5s ease-out forwards;
      transform-origin: 50% 50%;
    }
    /* Анимация галочки */
    .tick {
      fill: none;
      stroke: #4CAF50;
      stroke-width: 4;
      stroke-linecap: round;
      stroke-linejoin: round;
      stroke-dasharray: 75;
      stroke-dashoffset: 75;
      animation: draw 0.6s 0.5s ease-out forwards;
    }
    @keyframes pop {
      0%   { transform: scale(0); }
      80%  { transform: scale(1.1); }
      100% { transform: scale(1); }
    }
    @keyframes draw {
      to { stroke-dashoffset: 0; }
    }
  </style>
</head>
<body>
  <svg class=""checkmark"" viewBox=""0 0 100 100"">
    <!-- Круг -->
    <circle class=""circle"" cx=""50"" cy=""50"" r=""45""/>
    <!-- Более глубокая галочка -->
    <path class=""tick"" d=""M30 55 L45 70 L75 35""/>
  </svg>
  <h1>Авторизация прошла успешно!</h1>
  <p>Можно закрыть это окно и вернуться к боту.</p>
</body>
</html>

";

            var resp = req.CreateResponse(HttpStatusCode.OK);
            resp.Headers.Add("Content-Type", "text/html; charset=utf-8");
            await resp.WriteStringAsync(html);
            return resp;
        }
        catch (Exception e)
        {
            logger.LogError($"Error occurred={e.Message}", e);
            throw;
        }
    }
}