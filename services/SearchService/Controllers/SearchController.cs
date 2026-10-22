using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nest;
using SharedContracts.Responses;
using SearchService.Consumers;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly IElasticClient _elasticClient;

    public SearchController(IElasticClient elasticClient)
    {
        _elasticClient = elasticClient;
    }

    [HttpGet("accounts")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AccountSearchDoc>>>> SearchAccounts([FromQuery] string query)
    {
        var searchResponse = await _elasticClient.SearchAsync<AccountSearchDoc>(s => s
            .Query(q => q
                .QueryString(qs => qs
                    .Query(query)
                )
            )
        );

        if (!searchResponse.IsValid)
        {
            return StatusCode(500, ApiResponse<IEnumerable<AccountSearchDoc>>.FailureResult("Search", searchResponse.ServerError?.Error?.Reason ?? "Elasticsearch query failed.", "ELASTICSEARCH_ERROR"));
        }

        var results = searchResponse.Documents.ToList();
        return Ok(ApiResponse<IEnumerable<AccountSearchDoc>>.SuccessResult(results, "Search completed successfully."));
    }
}
