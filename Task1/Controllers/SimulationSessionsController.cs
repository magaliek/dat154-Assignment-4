using Assignment_4.Data;
using Assignment_4.Mapping;
using Assignment_4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedLogic;

namespace Assignment_4.Controllers;

[ApiController]
[Route("api/sessions")]
public class SimulationSessionsController : ControllerBase
{
    private readonly Dat154Gr2Context _db;

    public SimulationSessionsController(Dat154Gr2Context db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<int>> Start([FromBody] StartSessionDto dto)
    {
        var patient = await _db.Patients.FindAsync(dto.PatientId);
        if (patient == null)
            return NotFound();
        var student = await _db.CustomUsers.FindAsync(dto.StudentCustomUserId);
        if (student == null)
            return NotFound();

        var session = new SimulationSession
        {
            PatientId = dto.PatientId,
            StudentCustomUserId = dto.StudentCustomUserId,
            StartedAt = DateTimeOffset.UtcNow
        };
        _db.SimulationSessions.Add(session);
        await _db.SaveChangesAsync();
        return Ok(session.Id);
    }

    [HttpGet("for-student/{studentCustomUserId:int}")]
    public async Task<ActionResult<int>> GetSessionIdForStudent(int studentCustomUserId)
    {
        if (!await _db.CustomUsers.AsNoTracking().AnyAsync(u => u.Id == studentCustomUserId))
            return NotFound("Unknown student id.");

        var openId = await _db.SimulationSessions.AsNoTracking()
            .Where(s => s.StudentCustomUserId == studentCustomUserId && s.EndedAt == null)
            .OrderByDescending(s => s.StartedAt)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync();

        if (openId.HasValue)
            return Ok(openId.Value);

        var latestId = await _db.SimulationSessions.AsNoTracking()
            .Where(s => s.StudentCustomUserId == studentCustomUserId)
            .OrderByDescending(s => s.StartedAt)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync();

        if (latestId.HasValue)
            return Ok(latestId.Value);

        return NotFound(
            "No simulation session for this student yet. The student must load a case in Task 2 first.");
    }

    [HttpPost("{sessionId:int}/end")]
    public async Task<IActionResult> End(int sessionId)
    {
        var session = await _db.SimulationSessions.FindAsync(sessionId);
        if (session == null)
            return NotFound();
        session.EndedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("{sessionId:int}/actions")]
    public async Task<ActionResult<SimulationActionDto>> AddAction(int sessionId, [FromBody] RegisterActionDto dto)
    {
        var session = await _db.SimulationSessions.FindAsync(sessionId);
        if (session == null)
            return NotFound();

        var action = new SimulationAction
        {
            SessionId = sessionId,
            OccurredAt = DateTimeOffset.UtcNow,
            Kind = dto.Kind,
            Drug = dto.Drug,
            DoseMg = dto.DoseMg,
            Route = dto.Route,
            Notes = dto.Notes
        };

        await ApplyRulesAsync(action, session.PatientId);

        _db.SimulationActions.Add(action);
        await _db.SaveChangesAsync();

        return Ok(MapAction(action));
    }

    [HttpGet("{sessionId:int}/actions")]
    public async Task<ActionResult<List<SimulationActionDto>>> GetActions(int sessionId, [FromQuery] DateTimeOffset? since)
    {
        var q = _db.SimulationActions
            .Include(a => a.Deviations)
            .Where(a => a.SessionId == sessionId);
        if (since.HasValue)
            q = q.Where(a => a.OccurredAt > since.Value);
        var list = await q.OrderBy(a => a.OccurredAt).ThenBy(a => a.Id).ToListAsync();
        return Ok(list.Select(MapAction).ToList());
    }

    [HttpPost("{sessionId:int}/observations")]
    public async Task<ActionResult<int>> AddObservation(int sessionId, [FromBody] AddObservationDto dto)
    {
        var session = await _db.SimulationSessions.FindAsync(sessionId);
        if (session == null)
            return NotFound();

        var obs = new TeacherObservation
        {
            SessionId = sessionId,
            Text = dto.Text,
            RelatedActionId = dto.RelatedActionId,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.TeacherObservations.Add(obs);
        await _db.SaveChangesAsync();
        return Ok(obs.Id);
    }

    [HttpGet("{sessionId:int}/observations")]
    public async Task<ActionResult<List<TeacherObservationDto>>> GetObservations(int sessionId)
    {
        if (!await _db.SimulationSessions.AsNoTracking().AnyAsync(s => s.Id == sessionId))
            return NotFound();

        var list = await _db.TeacherObservations.AsNoTracking()
            .Where(o => o.SessionId == sessionId)
            .OrderBy(o => o.CreatedAt)
            .ThenBy(o => o.Id)
            .Select(o => new TeacherObservationDto
            {
                Id = o.Id,
                SessionId = o.SessionId,
                RelatedActionId = o.RelatedActionId,
                CreatedAt = o.CreatedAt,
                Text = o.Text
            })
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("{sessionId:int}/debrief")]
    public async Task<ActionResult<DebriefDto>> Debrief(int sessionId)
    {
        var session = await _db.SimulationSessions
            .Include(s => s.Actions).ThenInclude(a => a.Deviations)
            .Include(s => s.Observations)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session == null)
            return NotFound();

        return Ok(new DebriefDto
        {
            SessionId = session.Id,
            PatientId = session.PatientId,
            StudentCustomUserId = session.StudentCustomUserId,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            Actions = session.Actions.OrderBy(a => a.OccurredAt).Select(MapAction).ToList(),
            Observations = session.Observations.OrderBy(o => o.CreatedAt).Select(o => new TeacherObservationDto
            {
                Id = o.Id,
                SessionId = o.SessionId,
                RelatedActionId = o.RelatedActionId,
                CreatedAt = o.CreatedAt,
                Text = o.Text
            }).ToList()
        });
    }

    [HttpGet("{sessionId:int}/patient-summary")]
    public async Task<ActionResult<CaseDto>> PatientSummary(int sessionId)
    {
        var s = await _db.SimulationSessions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sessionId);
        if (s == null)
            return NotFound();
        var patient = await _db.Patients
            .Include(p => p.Medications)
            .Include(p => p.Allergies)
            .Include(p => p.Diagnoses)
            .Include(p => p.Goals)
            .FirstOrDefaultAsync(p => p.Id == s.PatientId);
        if (patient == null)
            return NotFound();
        return Ok(CaseMapper.ToDto(patient));
    }

    [HttpGet("recent")]
    public async Task<ActionResult<List<SimulationSessionDto>>> Recent([FromQuery] int take = 30)
    {
        var list = await _db.SimulationSessions
            .OrderByDescending(s => s.StartedAt)
            .Take(take)
            .Select(s => new SimulationSessionDto
            {
                Id = s.Id,
                PatientId = s.PatientId,
                StudentCustomUserId = s.StudentCustomUserId,
                StartedAt = s.StartedAt,
                EndedAt = s.EndedAt
            })
            .ToListAsync();
        return Ok(list);
    }

    private static SimulationActionDto MapAction(SimulationAction a)
    {
        return new SimulationActionDto
        {
            Id = a.Id,
            SessionId = a.SessionId,
            OccurredAt = a.OccurredAt,
            Kind = a.Kind,
            Drug = a.Drug,
            DoseMg = a.DoseMg,
            Route = a.Route,
            Notes = a.Notes,
            Deviations = a.Deviations.Select(d => new SimulationDeviationDto
            {
                Id = d.Id,
                ActionId = d.ActionId,
                RuleName = d.RuleName,
                Message = d.Message
            }).ToList()
        };
    }

    private async Task ApplyRulesAsync(SimulationAction action, int patientId)
    {
        if (string.IsNullOrWhiteSpace(action.Drug))
            return;

        var drugLower = action.Drug.ToLowerInvariant();
        var allergies = await _db.Allergies.Where(x => x.PatientId == patientId).ToListAsync();
        foreach (var allergy in allergies)
        {
            if (allergy.Allergy.Contains(action.Drug!, StringComparison.OrdinalIgnoreCase))
            {
                action.Deviations.Add(new SimulationDeviation
                {
                    RuleName = "Allergy",
                    Message = $"Possible allergy issue: {action.Drug}"
                });
            }
        }

        if (drugLower.Contains("ketorolac") || drugLower.Contains("ibuprofen") || drugLower == "nsaid")
        {
            var goals = await _db.Goals.Where(g => g.PatientId == patientId).Select(g => g.Goal1).ToListAsync();
            foreach (var g in goals)
            {
                if (g.ToLowerInvariant().Contains("nsaid") || g.ToLowerInvariant().Contains("ketorolac"))
                {
                    action.Deviations.Add(new SimulationDeviation
                    {
                        RuleName = "ContraindicatedDrug",
                        Message = "NSAID administration flagged against case goals."
                    });
                    break;
                }
            }
        }
    }
}
