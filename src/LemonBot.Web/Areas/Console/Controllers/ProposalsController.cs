﻿using KITT.Web.Models.Proposals;
using LemonBot.Web.Areas.Console.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LemonBot.Web.Areas.Console.Controllers;

[Route("api/console/[controller]")]
[ApiController]
[Authorize]
public class ProposalsController : ControllerBase
{
    public ProposalsControllerServices ControllerServices { get; }

    public ProposalsController(ProposalsControllerServices controllerServices)
    {
        ControllerServices = controllerServices ?? throw new ArgumentNullException(nameof(controllerServices));
    }

    [HttpGet]
    public IActionResult GetProposals(int s = 10, ProposalsQueryModel.SortDirection sort = ProposalsQueryModel.SortDirection.Descending, ProposalStatus? st = null, string? q = null)
    {
        var model = ControllerServices.GetAllProposals(
            size: s,
            sort,
            query: q,
            status: st);

        return Ok(model);
    }

    [HttpGet("{id}")]
    public IActionResult GetProposalDetail(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest();
        }

        var model = ControllerServices.GetProposalDetail(id);
        if (model is null)
        {
            return NotFound();
        }

        return Ok(model);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> AcceptProposal(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest();
        }

        try
        {
            await ControllerServices.AcceptProposalAsync(id);
            return Ok();
        }
        catch (ArgumentOutOfRangeException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RejectProposal(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest();
        }

        try
        {
            await ControllerServices.RejectProposalAsync(id);
            return Ok();
        }
        catch (ArgumentOutOfRangeException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}/refuse")]
    public async Task<IActionResult> RefuseProposal(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest();
        }

        try
        {
            await ControllerServices.RefuseProposalAsync(id);
            return Ok();
        }
        catch (ArgumentOutOfRangeException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/schedule")]
    public async Task<IActionResult> ScheduleProposal(Guid id, [FromBody] ScheduleProposalModel model)
    {
        if (id == Guid.Empty)
        {
            return BadRequest();
        }

        await ControllerServices.ScheduleProposalAsync(id, model, User.GetUserId());
        return Ok();
    }
}
