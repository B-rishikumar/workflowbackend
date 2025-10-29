
// WorkflowService.cs
using AutoMapper;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.Core.Interfaces.Repositories;
using WorkflowManagement.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
namespace WorkflowManagement.Application.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IEnvironmentRepository _environmentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<WorkflowService> _logger;

    public WorkflowService(
        IWorkflowRepository workflowRepository,
        IEnvironmentRepository environmentRepository,
        IMapper mapper,
        ILogger<WorkflowService> logger)
    {
        _workflowRepository = workflowRepository;
        _environmentRepository = environmentRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Workflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _workflowRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Workflow?> GetWithStepsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _workflowRepository.GetWithStepsAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Workflow>> GetByEnvironmentIdAsync(Guid environmentId, CancellationToken cancellationToken = default)
    {
        return await _workflowRepository.GetByEnvironmentIdAsync(environmentId, cancellationToken);
    }

    public async Task<(IEnumerable<Workflow> Workflows, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, Guid? environmentId = null, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        if (environmentId.HasValue && !string.IsNullOrEmpty(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            return await _workflowRepository.GetPagedAsync(
                pageNumber, pageSize,
                w => w.EnvironmentId == environmentId.Value &&
                     (w.Name.ToLower().Contains(lowerSearchTerm) ||
                      (w.Description != null && w.Description.ToLower().Contains(lowerSearchTerm))),
                cancellationToken: cancellationToken);
        }
        else if (environmentId.HasValue)
        {
            return await _workflowRepository.GetPagedAsync(
                pageNumber, pageSize,
                w => w.EnvironmentId == environmentId.Value,
                cancellationToken: cancellationToken);
        }
        else if (!string.IsNullOrEmpty(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            return await _workflowRepository.GetPagedAsync(
                pageNumber, pageSize,
                w => w.Name.ToLower().Contains(lowerSearchTerm) ||
                     (w.Description != null && w.Description.ToLower().Contains(lowerSearchTerm)),
                cancellationToken: cancellationToken);
        }

        return await _workflowRepository.GetPagedAsync(pageNumber, pageSize, cancellationToken: cancellationToken);
    }

    public async Task<Workflow> CreateAsync(Workflow workflow, CancellationToken cancellationToken = default)
    {
        // Validate environment exists
        var environment = await _environmentRepository.GetByIdAsync(workflow.EnvironmentId, cancellationToken);
        if (environment == null || !environment.IsActive)
        {
            throw new InvalidOperationException("Environment not found or inactive");
        }

        workflow.Status = WorkflowStatus.Draft;
        workflow.CreatedAt = DateTime.UtcNow;
        workflow.UpdatedAt = DateTime.UtcNow;

        return await _workflowRepository.AddAsync(workflow, cancellationToken);
    }

    public async Task<Workflow> UpdateAsync(Workflow workflow, CancellationToken cancellationToken = default)
    {
        var existingWorkflow = await _workflowRepository.GetByIdAsync(workflow.Id, cancellationToken);
        if (existingWorkflow == null)
        {
            throw new InvalidOperationException("Workflow not found");
        }

        // Don't allow updates to published workflows without creating a new version
        if (existingWorkflow.IsPublished && existingWorkflow.Status == WorkflowStatus.Active)
        {
            throw new InvalidOperationException("Cannot update published workflow. Create a new version instead.");
        }

        existingWorkflow.Name = workflow.Name;
        existingWorkflow.Description = workflow.Description;
        existingWorkflow.Tags = workflow.Tags;
        existingWorkflow.TimeoutMinutes = workflow.TimeoutMinutes;
        existingWorkflow.RetryCount = workflow.RetryCount;
        existingWorkflow.GlobalVariables = workflow.GlobalVariables;
        existingWorkflow.UpdatedAt = DateTime.UtcNow;

        return await _workflowRepository.UpdateAsync(existingWorkflow, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workflow = await _workflowRepository.GetByIdAsync(id, cancellationToken);
        if (workflow == null)
        {
            return false;
        }

        // Don't allow deletion of published workflows
        if (workflow.IsPublished && workflow.Status == WorkflowStatus.Active)
        {
            throw new InvalidOperationException("Cannot delete published workflow");
        }

        await _workflowRepository.DeleteAsync(workflow, cancellationToken);
        return true;
    }

    public async Task<Workflow> PublishAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var workflow = await _workflowRepository.GetWithStepsAsync(id, cancellationToken);
        if (workflow == null)
        {
            throw new InvalidOperationException("Workflow not found");
        }

        if (workflow.IsPublished)
        {
            throw new InvalidOperationException("Workflow is already published");
        }

        // Validate workflow before publishing
        if (!await ValidateWorkflowAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Workflow validation failed");
        }

        workflow.IsPublished = true;
        workflow.Status = WorkflowStatus.Active;
        workflow.PublishedAt = DateTime.UtcNow;
        workflow.PublishedBy = userId.ToString();
        workflow.UpdatedAt = DateTime.UtcNow;

        return await _workflowRepository.UpdateAsync(workflow, cancellationToken);
    }

    public async Task<Workflow> UnpublishAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var workflow = await _workflowRepository.GetByIdAsync(id, cancellationToken);
        if (workflow == null)
        {
            throw new InvalidOperationException("Workflow not found");
        }

        if (!workflow.IsPublished)
        {
            throw new InvalidOperationException("Workflow is not published");
        }

        workflow.IsPublished = false;
        workflow.Status = WorkflowStatus.Draft;
        workflow.UpdatedAt = DateTime.UtcNow;

        return await _workflowRepository.UpdateAsync(workflow, cancellationToken);
    }

    public async Task<WorkflowVersion> CreateVersionAsync(Guid workflowId, string description, CancellationToken cancellationToken = default)
    {
        var workflow = await _workflowRepository.GetWithStepsAsync(workflowId, cancellationToken);
        if (workflow == null)
        {
            throw new InvalidOperationException("Workflow not found");
        }

        var existingVersions = await GetVersionsAsync(workflowId, cancellationToken);
        var nextVersionNumber = GenerateNextVersionNumber(existingVersions);

        var version = new WorkflowVersion
        {
            WorkflowId = workflowId,
            VersionNumber = nextVersionNumber,
            ChangeDescription = description,
            WorkflowDefinition = System.Text.Json.JsonSerializer.Serialize(workflow),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // This would need a WorkflowVersionRepository
        // return await _workflowVersionRepository.AddAsync(version, cancellationToken);
        throw new NotImplementedException("WorkflowVersionRepository not implemented");
    }

    public async Task<IEnumerable<WorkflowVersion>> GetVersionsAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        var workflow = await _workflowRepository.GetWithVersionsAsync(workflowId, cancellationToken);
        return workflow?.Versions ?? new List<WorkflowVersion>();
    }

    public async Task<bool> ValidateWorkflowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workflow = await _workflowRepository.GetWithStepsAsync(id, cancellationToken);
        if (workflow == null)
        {
            return false;
        }

        // Validate workflow has at least one step
        if (!workflow.Steps.Any())
        {
            _logger.LogWarning("Workflow {WorkflowId} has no steps", id);
            return false;
        }

        // Validate all steps have valid API endpoints
        foreach (var step in workflow.Steps)
        {
            if (step.ApiEndpoint == null || !step.ApiEndpoint.IsActive)
            {
                _logger.LogWarning("Workflow {WorkflowId} has invalid API endpoint in step {StepId}", id, step.Id);
                return false;
            }
        }

        return true;
    }

    private string GenerateNextVersionNumber(IEnumerable<WorkflowVersion> existingVersions)
    {
        if (!existingVersions.Any())
        {
            return "1.0.0";
        }

        var latestVersion = existingVersions
            .Select(v => Version.Parse(v.VersionNumber))
            .OrderByDescending(v => v)
            .First();

        return $"{latestVersion.Major}.{latestVersion.Minor}.{latestVersion.Build + 1}";
    }
}