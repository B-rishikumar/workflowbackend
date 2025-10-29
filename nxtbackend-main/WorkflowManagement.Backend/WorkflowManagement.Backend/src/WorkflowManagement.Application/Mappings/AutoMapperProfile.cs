using AutoMapper;
using WorkflowManagement.Application.DTOs.ApiEndpoint;
using WorkflowManagement.Application.DTOs.Auth;
using WorkflowManagement.Application.DTOs.Environment;
using WorkflowManagement.Application.DTOs.Project;
using WorkflowManagement.Application.DTOs.User;
using WorkflowManagement.Application.DTOs.Workflow;
using WorkflowManagement.Application.DTOs.Workspace;
using WorkflowManagement.Core.Entities;

namespace WorkflowManagement.Application.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateUserMappings();
        CreateWorkspaceMappings();
        CreateProjectMappings();
        CreateEnvironmentMappings();
        CreateWorkflowMappings();
        CreateApiEndpointMappings();
    }

    private void CreateUserMappings()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

        CreateMap<UpdateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Username, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());
    }

    private void CreateWorkspaceMappings()
    {
        CreateMap<Workspace, WorkspaceDto>()
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner.FullName))
            .ForMember(dest => dest.ProjectsCount, opt => opt.MapFrom(src => src.Projects.Count));

        CreateMap<CreateWorkspaceDto, Workspace>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

        CreateMap<UpdateWorkspaceDto, Workspace>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());
    }

    private void CreateProjectMappings()
    {
        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.WorkspaceName, opt => opt.MapFrom(src => src.Workspace.Name))
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner.FullName))
            .ForMember(dest => dest.EnvironmentsCount, opt => opt.MapFrom(src => src.Environments.Count));

        CreateMap<CreateProjectDto, Project>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
    }

    private void CreateEnvironmentMappings()
    {
        CreateMap<Core.Entities.WorkflowEnvironment, EnvironmentDto>()
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.Name))
            .ForMember(dest => dest.WorkflowsCount, opt => opt.MapFrom(src => src.Workflows.Count));

        CreateMap<CreateEnvironmentDto, Core.Entities.WorkflowEnvironment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
    }

    private void CreateWorkflowMappings()
    {
        CreateMap<Workflow, WorkflowDto>()
            .ForMember(dest => dest.EnvironmentName, opt => opt.MapFrom(src => src.Environment.Name))
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner.FullName))
            .ForMember(dest => dest.StepsCount, opt => opt.MapFrom(src => src.Steps.Count))
            .ForMember(dest => dest.ExecutionsCount, opt => opt.MapFrom(src => src.Executions.Count))
            .ForMember(dest => dest.LatestVersion, opt => opt.MapFrom(src => 
                src.Versions.OrderByDescending(v => v.CreatedAt).FirstOrDefault()!.VersionNumber));

        CreateMap<CreateWorkflowDto, Workflow>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Core.Enums.WorkflowStatus.Draft))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

        CreateMap<UpdateWorkflowDto, Workflow>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EnvironmentId, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

        CreateMap<WorkflowExecution, WorkflowExecutionDto>()
            .ForMember(dest => dest.WorkflowName, opt => opt.MapFrom(src => src.Workflow.Name))
            .ForMember(dest => dest.ExecutedByName, opt => opt.MapFrom(src => src.ExecutedBy.FullName))
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => 
                src.CompletedAt.HasValue && src.StartedAt.HasValue 
                    ? src.CompletedAt.Value - src.StartedAt.Value 
                    : (TimeSpan?)null));

        CreateMap<WorkflowVersion, WorkflowVersionDto>();
    }

    private void CreateApiEndpointMappings()
    {
        CreateMap<ApiEndpoint, ApiEndpointDto>()
            .ForMember(dest => dest.Parameters, opt => opt.MapFrom(src => src.Parameters));

        CreateMap<CreateApiEndpointDto, ApiEndpoint>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Parameters, opt => opt.MapFrom(src => src.Parameters));

        CreateMap<ApiParameter, ApiParameterDto>();

        CreateMap<CreateApiParameterDto, ApiParameter>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ApiEndpointId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
    }
}