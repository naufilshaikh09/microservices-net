using Duende.IdentityServer;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Play.Identity.Contracts;
using Play.Identity.Service.Dtos;
using Play.Identity.Service.Entities;

namespace Play.Identity.Service.Controllers;

[ApiController]
[Route("users")]
[Authorize(Policy = IdentityServerConstants.LocalApi.PolicyName, Roles = Roles.Admin)]
public class UserController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserController(UserManager<ApplicationUser> userManager, IPublishEndpoint publishEndpoint)
    {
        _userManager = userManager;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> GetUsers()
    {
        var users = _userManager.Users.ToList()
            .Select(user => user.AsDto());

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null) return NotFound();

        return user.AsDto();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null) return NotFound();

        user.Email = dto.Email;
        user.UserName = dto.Email;
        user.Gil = dto.Gil;

        await _userManager.UpdateAsync(user);

        await _publishEndpoint.Publish(new UserUpdated(
            user.Id, user.Email, user.Gil));

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null) return NotFound();

        await _userManager.DeleteAsync(user);

        await _publishEndpoint.Publish(new UserUpdated(user.Id, user.Email, 0));

        return NoContent();
    }
}