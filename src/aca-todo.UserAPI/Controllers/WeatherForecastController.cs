using System.Net.Mime;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;

namespace aca_todo.UserAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly DaprClient _daprClient;

    public UsersController(ILogger<UsersController> logger, DaprClient daprClient)
    {
        _logger = logger;
        _daprClient = daprClient;
    }

    [HttpPost]
    [Route("")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CreateNewUser([FromBody] CreateNewUserRequest request)
    {
        var userId = request.UserId;
        await _daprClient.PublishEventAsync(pubsubName: "pubsub", topicName: "users", new NewUserMessage(userId));
        return NoContent();
    }
}

public record NewUserMessage(Guid UserId);
public record CreateNewUserRequest(Guid UserId);
