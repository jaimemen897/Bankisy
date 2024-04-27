using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.cards;
using TFG.Services;
using TFG.Services.Pagination;

namespace TFG.Controllers.Controllers;

[ApiController]
[Authorize(Policy = "Admin")]
[Route("[controller]")]
public class CardController(CardService cardService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Pagination<CardResponseDto>>> GetCards([FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10, [FromQuery] string orderBy = "cardNumber", [FromQuery] bool descending = false,
        [FromQuery] string? search = null, [FromQuery] string? filter = null, [FromQuery] bool? isDeleted = null,
        [FromQuery] bool? isBlocked = null)
    {
        return await cardService.GetCards(pageNumber, pageSize, orderBy, descending, search, filter, isDeleted,
            isBlocked);
    }

    [HttpGet("{cardNumber}")]
    public async Task<ActionResult<CardResponseDto>> GetCardByCardNumber(string cardNumber)
    {
        return await cardService.GetCardByCardNumber(cardNumber);
    }

    [HttpPost]
    public async Task<ActionResult<CardResponseDto>> CreateCard(CardCreateDto cardCreateDto)
    {
        return await cardService.CreateCard(cardCreateDto);
    }

    [HttpPut("{cardNumber}")]
    public async Task<ActionResult<CardResponseDto>> UpdateCard(string cardNumber, CardUpdateDto cardUpdateDto)
    {
        return await cardService.UpdateCard(cardNumber, cardUpdateDto);
    }

    [HttpDelete("{cardNumber}")]
    public async Task<ActionResult> DeleteCard(string cardNumber)
    {
        await cardService.DeleteCard(cardNumber);
        return Ok();
    }

    [HttpPost("{cardNumber}/renovate")]
    public async Task<ActionResult<CardResponseDto>> RenovateCard(string cardNumber)
    {
        return await cardService.RenovateCard(cardNumber);
    }

    [HttpPost("{cardNumber}/block")]
    public async Task<ActionResult> BlockCard(string cardNumber)
    {
        await cardService.BlockCard(cardNumber);
        return Ok();
    }

    [HttpPost("{cardNumber}/unblock")]
    public async Task<ActionResult> UnblockCard(string cardNumber)
    {
        await cardService.UnblockCard(cardNumber);
        return Ok();
    }

    [HttpPost("{cardNumber}/activate")]
    public async Task<ActionResult> ActivateCard(string cardNumber)
    {
        await cardService.ActivateCard(cardNumber);
        return Ok();
    }
}