﻿using KITT.Web.Models.Streamings;
using LemonBot.Web.Areas.Console.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LemonBot.Web.Areas.Console.Controllers;

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

    [HttpGet]
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

    [HttpGet("{id}")]
    public IActionResult GetStreamingDetail(Guid id)
    {
        var model = ControllerServices.GetStreamingDetail(id);
        if (model is null)
        {
            return NotFound();
        }

        return Ok(model);
    }

    [HttpPost]
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
    public async Task<IActionResult> UpdateStreaming(Guid id, [FromBody] StreamingDetailModel model)
    {
        if (id == Guid.Empty)
        {
            return NotFound();
        }

        await ControllerServices.UpdateStreamingAsync(id, model);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStreaming(Guid id)
    {
        if (id == Guid.Empty)
        {
            return NotFound();
        }

        await ControllerServices.DeleteStreamingAsync(id);
        return Ok();
    }
}
