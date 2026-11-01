using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;

namespace ClinicFlow_Unified.Services;

public sealed class AuditService
{
    private readonly AuditLogRepository _auditLogRepository;

    public AuditService(AuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<long> LogAsync(
        int? userId,
        string actionType,
        string entityName,
        string? recordId,
        string description)
    {
        AuditLogEntry auditLogEntry = new AuditLogEntry
        {
            UserId = userId,
            ActionType = actionType,
            EntityName = entityName,
            RecordId = recordId,
            Description = description,
            MachineName = Environment.MachineName
        };

        return await _auditLogRepository.InsertAsync(auditLogEntry);
    }
}