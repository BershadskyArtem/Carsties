using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems(
        [FromQuery] SearchParams searchParams)
    {
        var query = DB.PagedSearch<Item, Item>();

        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }

        query = searchParams.OrderBy switch
        {
            "make" => query
                .Sort(x => x.Ascending(a => a.Make))
                .Sort(x => x.Ascending(a => a.Model)),
            "new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
            _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
        };
        
        query = searchParams.FilterBy switch
        {
            "finished" => query.Match(a => a.AuctionEnd < DateTime.UtcNow),
            "endingSoon" => query.Match(
                a => a.AuctionEnd < DateTime.UtcNow.AddHours(6) && 
                     a.AuctionEnd > DateTime.UtcNow),
            _ => query.Match(a => a.AuctionEnd > DateTime.UtcNow)
        };

        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(x => x.Seller == searchParams.Seller); 
        }
        
        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(x => x.Winner == searchParams.Winner); 
        }
        
        searchParams.PageNumber ??= 1;
        searchParams.PageSize ??= 4;
        searchParams.PageNumber = Math.Max((int)searchParams.PageNumber, 1);
        searchParams.PageSize = Math.Clamp((int)searchParams.PageSize, 1, 20);
        
        query.PageSize((int)searchParams.PageSize);
        query.PageNumber((int)searchParams.PageNumber);
        
        var result = await query.ExecuteAsync();
        return Ok(new
        {
            results = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
    }
}