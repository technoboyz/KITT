using KITT.Web.Models.Streamings;
using LemonBot.Web.Areas.Console.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LemonBot.Web.Areas.Console.Controllers;

/// <summary>
/// Gestione delle dirette streaming
/// </summary>
[Route("api/console/[controller]")]
[ApiController]
[Authorize]
public class StreamingsController : ControllerBase
{
    public StreamingsControllerServices ControllerServices { get; }

    public StreamingsController(StreamingsControllerServices controllerServices)
    {
        ControllerServices = controllerServices ?? throw new ArgumentNullException(nameof(controllerServices));
    }

    /// <summary>
    /// Ottiene l'elenco delle live paginate e filtrate in base ai parametri indicati
    /// </summary>
    /// <param name="p">La pagina corrente</param>
    /// <param name="s">Il numero di elementi per pagina</param>
    /// <param name="sort">L'ordine in cui mostrare gli elementi</param>
    /// <param name="q">La stringa di ricerca per titolo delle live</param>
    /// <returns>L'elenco delle live disponibili</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAllStreamings(int p = 0, int s = 10, StreamingQueryModel.SortDirection sort = StreamingQueryModel.SortDirection.Descending, string? q = null)
    {
        var userId = User.GetUserId();

        var model = ControllerServices.GetAllStreamings(
            userId, 
            page: p, 
            size: s, 
            sort, 
            query: q);

        return Ok(model);
    }

    /// <summary>
    /// Ottiene il dettaglio di una live
    /// </summary>
    /// <param name="id">L'id della live da recuperare</param>
    /// <returns>Le informazioni della live</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StreamingDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetStreamingDetail(Guid id)
    {
        var model = ControllerServices.GetStreamingDetail(id);
        if (model is null)
        {
            return NotFound();
        }

        return Ok(model);
    }

    /// <summary>
    /// Pianifica una nuova live
    /// </summary>
    /// <param name="model">Le informazioni della live da pianificare</param>
    /// <returns>La live pianificata</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ScheduleStreamingModel), StatusCodes.Status201Created, "application/json")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ScheduleStreaming([FromBody] ScheduleStreamingModel model)
    {
        var scheduledStreamingId = await ControllerServices.ScheduleStreamingAsync(model, User.GetUserId());
        return CreatedAtAction(nameof(GetStreamingDetail), new { id = scheduledStreamingId }, model);
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportStreaming([FromBody] ImportStreamingModel model)
    {
        var importedStreamingId = await ControllerServices.ImportStreamingAsync(model, User.GetUserId());
        return CreatedAtAction(nameof(GetStreamingDetail), new { id = importedStreamingId }, model);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStreaming(Guid id, [FromBody]StreamingDetailModel model)
    {
        await ControllerServices.UpdateStreamingAsync(id, model);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStreaming(Guid id)
    {
        await ControllerServices.DeleteStreamingAsync(id);
        return Ok();
    }
}
