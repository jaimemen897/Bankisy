using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.cards;
using TFG.Services;
using TFG.Services.Exceptions;
using TFG.Services.Pagination;

namespace TFG.Controllers.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class CardController(CardService cardService) : ControllerBase
{
    //GET
    [Authorize(Policy = "Admin")]
    [HttpGet]
    public async Task<ActionResult<Pagination<CardResponseDto>>> GetCards([FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10, [FromQuery] string orderBy = "cardNumber", [FromQuery] bool descending = false,
        [FromQuery] string? search = null, [FromQuery] string? filter = null, [FromQuery] bool? isDeleted = null,
        [FromQuery] bool? isBlocked = null)
    {
        return await cardService.GetCards(pageNumber, pageSize, orderBy, descending, search, filter, isDeleted,
            isBlocked);
    }

    [Authorize(Policy = "User")]
    [HttpGet("my-cards")]
    public async Task<ActionResult<List<CardResponseDto>>> GetMyCards()
    {
        return await cardService.GetCardsByUserId(GetUserId());
    }

    [Authorize(Policy = "Admin")]
    [HttpGet("{cardNumber}")]
    public async Task<ActionResult<CardResponseDto>> GetCardByCardNumber(string cardNumber)
    {
        return await cardService.GetCardByCardNumber(cardNumber);
    }

    //CREATE
    [Authorize(Policy = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CardResponseDto>> CreateCard(CardCreateDto cardCreateDto)
    {
        return await cardService.CreateCard(cardCreateDto);
    }

    [Authorize(Policy = "User")]
    [HttpPost("my-card")]
    public async Task<ActionResult<CardResponseDto>> CreateCardForMySelf(CardCreateDto cardCreateDto)
    {
        return await cardService.CreateCard(cardCreateDto, GetUserId());
    }

    //UPDATE
    [Authorize(Policy = "Admin")]
    [HttpPut("{cardNumber}")]
    public async Task<ActionResult<CardResponseDto>> UpdateCard(string cardNumber, CardUpdateDto cardUpdateDto)
    {
        return await cardService.UpdateCard(cardNumber, cardUpdateDto);
    }

    [Authorize(Policy = "User")]
    [HttpPut("my-card/{cardNumber}")]
    public async Task<ActionResult<CardResponseDto>> UpdateMyCard(string cardNumber, CardUpdateDto cardUpdateDto)
    {
        return await cardService.UpdateCard(cardNumber, cardUpdateDto, GetUserId());
    }

    //DELETE
    [Authorize(Policy = "Admin")]
    [HttpDelete("{cardNumber}")]
    public async Task<ActionResult> DeleteCard(string cardNumber)
    {
        await cardService.DeleteCard(cardNumber);
        return Ok();
    }

    [Authorize(Policy = "User")]
    [HttpDelete("my-card/{cardNumber}")]
    public async Task<ActionResult> DeleteMyCard(string cardNumber)
    {
        await cardService.DeleteCard(cardNumber, GetUserId());
        return Ok();
    }

    //ACTIONS
    [Authorize(Policy = "Admin")]
    [HttpPost("{cardNumber}/renovate")]
    public async Task<ActionResult<CardResponseDto>> RenovateCard(string cardNumber)
    {
        return await cardService.RenovateCard(cardNumber);
    }

    [Authorize(Policy = "User")]
    [HttpPost("my-card/{cardNumber}/renovate")]
    public async Task<ActionResult<CardResponseDto>> RenovateMyCard(string cardNumber)
    {
        return await cardService.RenovateCard(cardNumber, GetUserId());
    }

    [Authorize(Policy = "Admin")]
    [HttpPost("{cardNumber}/block")]
    public async Task<ActionResult> BlockCard(string cardNumber)
    {
        await cardService.BlockCard(cardNumber);
        return Ok();
    }

    [Authorize(Policy = "User")]
    [HttpPost("my-card/{cardNumber}/block")]
    public async Task<ActionResult> BlockMyCard(string cardNumber)
    {
        await cardService.BlockCard(cardNumber, GetUserId());
        return Ok();
    }

    [Authorize(Policy = "Admin")]
    [HttpPost("{cardNumber}/unblock")]
    public async Task<ActionResult> UnblockCard(string cardNumber)
    {
        await cardService.UnblockCard(cardNumber);
        return Ok();
    }

    [Authorize(Policy = "User")]
    [HttpPost("my-card/{cardNumber}/unblock")]
    public async Task<ActionResult> UnblockMyCard(string cardNumber)
    {
        await cardService.UnblockCard(cardNumber, GetUserId());
        return Ok();
    }

    [Authorize(Policy = "Admin")]
    [HttpPost("{cardNumber}/activate")]
    public async Task<ActionResult> ActivateCard(string cardNumber)
    {
        await cardService.ActivateCard(cardNumber);
        return Ok();
    }

    [Authorize(Policy = "User")]
    [HttpPost("my-card/{cardNumber}/activate")]
    public async Task<ActionResult> ActivateMyCard(string cardNumber)
    {
        await cardService.ActivateCard(cardNumber, GetUserId());
        return Ok();
    }

    private Guid GetUserId()
    {
        return Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new HttpException(401, "Unauthorized"));
    }
}