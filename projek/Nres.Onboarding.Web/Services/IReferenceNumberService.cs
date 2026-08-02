using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;

namespace Nres.Onboarding.Web.Services;

/// <summary>
/// Produces the human readable reference number shown on every application,
/// e.g. <c>LD-2026-0001</c>.
/// </summary>
public interface IReferenceNumberService
{
    /// <summary>
    /// Returns the next reference number for <paramref name="moduleCode"/> in the current year.
    /// </summary>
    /// <param name="moduleCode">Module prefix, e.g. <c>LD</c>. See <see cref="Models.ModuleCodes"/>.</param>
    Task<string> GenerateAsync(string moduleCode, CancellationToken cancellationToken = default);
}

/// <summary>
/// Sequential, per-module, per-year reference numbers in the format
/// <c>{PREFIX}-{yyyy}-{0000}</c>.
/// <para>
/// Concurrency, training-grade: generation is serialised inside the process with a
/// semaphore, the caller is expected to save the new number inside the same database
/// transaction it opened, and a unique index on <c>Submissions.ReferenceNo</c> is the
/// final guard. That is enough for a classroom or a single web server. A production
/// deployment behind a load balancer should replace this with a database sequence or a
/// dedicated counter table updated with <c>UPDATE ... RETURNING</c>.
/// </para>
/// </summary>
public class ReferenceNumberService : IReferenceNumberService
{
    private const int SequenceDigits = 4;

    // Static so every request in this process queues behind the same lock.
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private readonly ApplicationDbContext _db;

    public ReferenceNumberService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<string> GenerateAsync(string moduleCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleCode);

        var year = DateTime.UtcNow.Year;
        var prefix = $"{moduleCode}-{year.ToString(CultureInfo.InvariantCulture)}-";

        await Gate.WaitAsync(cancellationToken);
        try
        {
            // The sequence is zero padded to a fixed width, so ordering the strings
            // descending gives the same answer as ordering the numbers descending.
            var lastReference = await _db.Submissions
                .AsNoTracking()
                .Where(s => s.ReferenceNo.StartsWith(prefix))
                .OrderByDescending(s => s.ReferenceNo)
                .Select(s => s.ReferenceNo)
                .FirstOrDefaultAsync(cancellationToken);

            var next = ParseSequence(lastReference, prefix) + 1;

            return prefix + next.ToString(new string('0', SequenceDigits), CultureInfo.InvariantCulture);
        }
        finally
        {
            Gate.Release();
        }
    }

    private static int ParseSequence(string? reference, string prefix)
    {
        if (string.IsNullOrEmpty(reference) || reference.Length <= prefix.Length)
        {
            return 0;
        }

        var tail = reference[prefix.Length..];

        return int.TryParse(tail, NumberStyles.None, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }
}
