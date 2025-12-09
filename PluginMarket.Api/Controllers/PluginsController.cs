using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PluginMarket.Api.Data;
using PluginMarket.Api.Filters;
using PluginMarket.Api.Models;

namespace PluginMarket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PluginsController : ControllerBase
{
    private readonly PluginDbContext _db;
    private readonly StorageOptions _storage;
    private readonly LinkGenerator _links;

    public PluginsController(PluginDbContext db, IOptions<StorageOptions> storage, LinkGenerator links)
    {
        _db = db;
        _storage = storage.Value;
        _links = links;
    }

    [HttpPost]
    [RequestSizeLimit(1024L * 1024L * 200L)]
    [RequestFormLimits(MultipartBodyLengthLimit = 1024L * 1024L * 200L)]
    public async Task<ActionResult<PluginDto>> Upload([FromForm] UploadPluginRequest meta, IFormFile package, string? uploadedBy)
    {
        if (package == null || package.Length == 0) return BadRequest("package required");
        var dir = Path.Combine(_storage.RootPath, meta.Slug, meta.Version);
        Directory.CreateDirectory(dir);
        var fileName = string.IsNullOrWhiteSpace(package.FileName) ? "plugin.dll" : Path.GetFileName(package.FileName);
        var filePath = Path.Combine(dir, fileName);
        using (var fs = System.IO.File.Create(filePath))
        {
            await package.CopyToAsync(fs);
        }
        string? sha = null;
        using (var fs = System.IO.File.OpenRead(filePath))
        using (var sha256 = SHA256.Create())
        {
            var hash = await sha256.ComputeHashAsync(fs);
            sha = Convert.ToHexString(hash).ToLowerInvariant();
        }
        var entity = new PluginPackage
        {
            Id = Guid.NewGuid(),
            Slug = meta.Slug,
            Name = meta.Name,
            Version = meta.Version,
            Description = meta.Description,
            RequiresRestart = meta.RequiresRestart,
            FileName = fileName,
            FilePath = filePath,
            SizeBytes = package.Length,
            Sha256 = sha,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy,
            Status = ReviewStatus.Pending
        };
        _db.PluginPackages.Add(entity);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("duplicate slug+version");
        }
        var url = _links.GetPathByAction(HttpContext, nameof(Download), values: new { id = entity.Id });
        return Ok(new PluginDto(entity.Id, entity.Slug, entity.Name, entity.Version, entity.Description, entity.RequiresRestart, entity.Status.ToString(), url ?? string.Empty, entity.SizeBytes, entity.UploadedAt));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PluginDto>>> List([FromQuery] string? q)
    {
        var query = _db.PluginPackages.AsNoTracking().Where(x => x.Status == ReviewStatus.Approved);
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x => x.Name.Contains(q) || x.Slug.Contains(q));
        }
        var entities = await query.OrderByDescending(x => x.UploadedAt).ToListAsync();
        var items = entities.Select(x => new PluginDto(x.Id, x.Slug, x.Name, x.Version, x.Description, x.RequiresRestart, x.Status.ToString(), Url.ActionLink(nameof(Download), values: new { id = x.Id }) ?? string.Empty, x.SizeBytes, x.UploadedAt)).ToList();
        return Ok(items);
    }

    [HttpGet("pending")]
    [AdminApiKey]
    public async Task<ActionResult<IEnumerable<PluginDto>>> Pending()
    {
        var entities = await _db.PluginPackages.AsNoTracking().Where(x => x.Status == ReviewStatus.Pending)
            .OrderBy(x => x.UploadedAt)
            .ToListAsync();
        var items = entities.Select(x => new PluginDto(x.Id, x.Slug, x.Name, x.Version, x.Description, x.RequiresRestart, x.Status.ToString(), Url.ActionLink(nameof(Download), values: new { id = x.Id }) ?? string.Empty, x.SizeBytes, x.UploadedAt)).ToList();
        return Ok(items);
    }

    [HttpPost("{id:guid}/approve")]
    [AdminApiKey]
    public async Task<ActionResult> Approve(Guid id)
    {
        var entity = await _db.PluginPackages.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Status = ReviewStatus.Approved;
        entity.RejectReason = null;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    [AdminApiKey]
    public async Task<ActionResult> Reject(Guid id, [FromBody] ReviewRequest req)
    {
        var entity = await _db.PluginPackages.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Status = ReviewStatus.Rejected;
        entity.RejectReason = req.Reason;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:guid}/download")]
    public async Task<ActionResult> Download(Guid id)
    {
        var entity = await _db.PluginPackages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.Status == ReviewStatus.Approved);
        if (entity == null) return NotFound();
        if (!System.IO.File.Exists(entity.FilePath)) return NotFound();
        var stream = System.IO.File.OpenRead(entity.FilePath);
        return File(stream, "application/octet-stream", entity.FileName);
    }

    [HttpGet("{slug}/latest")]
    public async Task<ActionResult<PluginDto>> Latest(string slug)
    {
        var entity = await _db.PluginPackages.AsNoTracking()
            .Where(x => x.Slug == slug && x.Status == ReviewStatus.Approved)
            .OrderByDescending(x => x.UploadedAt)
            .FirstOrDefaultAsync();
        if (entity == null) return NotFound();
        var url = Url.ActionLink(nameof(Download), values: new { id = entity.Id }) ?? string.Empty;
        return Ok(new PluginDto(entity.Id, entity.Slug, entity.Name, entity.Version, entity.Description, entity.RequiresRestart, entity.Status.ToString(), url, entity.SizeBytes, entity.UploadedAt));
    }
}
