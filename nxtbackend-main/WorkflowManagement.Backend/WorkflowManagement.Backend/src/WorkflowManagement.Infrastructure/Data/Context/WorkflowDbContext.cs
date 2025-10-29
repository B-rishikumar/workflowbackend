using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using WorkflowManagement.Core.Entities;
using WorkflowManagement.Core.Entities.Base;

namespace WorkflowManagement.Infrastructure.Data.Context;

public class WorkflowDbContext : DbContext
{
    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Core.Entities.WorkflowEnvironment> Environments { get; set; }
    public DbSet<Workflow> Workflows { get; set; }
    public DbSet<WorkflowStep> WorkflowSteps { get; set; }
    public DbSet<WorkflowVersion> WorkflowVersions { get; set; }
    public DbSet<ApiEndpoint> ApiEndpoints { get; set; }
    public DbSet<ApiParameter> ApiParameters { get; set; }
    public DbSet<WorkflowExecution> WorkflowExecutions { get; set; }
    public DbSet<ExecutionLog> ExecutionLogs { get; set; }
    public DbSet<WorkflowApproval> WorkflowApprovals { get; set; }
    public DbSet<WorkflowSchedule> WorkflowSchedules { get; set; }
    public DbSet<WorkflowMetrics> WorkflowMetrics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure decimal precision for TimeSpan properties
        ConfigureTimeSpanProperties(modelBuilder);

        // Configure JSON columns
        ConfigureJsonColumns(modelBuilder);

        // Configure soft delete global filter
        ConfigureSoftDeleteFilter(modelBuilder);

        // Configure indexes
        ConfigureIndexes(modelBuilder);
    }

    private void ConfigureTimeSpanProperties(ModelBuilder modelBuilder)
    {
        // Configure TimeSpan properties to store as bigint (ticks)
        modelBuilder.Entity<WorkflowExecution>()
            .Property(e => e.Duration)
            .HasConversion(
                v => v.HasValue ? v.Value.Ticks : (long?)null,
                v => v.HasValue ? new TimeSpan(v.Value) : (TimeSpan?)null);

        modelBuilder.Entity<ExecutionLog>()
            .Property(e => e.Duration)
            .HasConversion(
                v => v.HasValue ? v.Value.Ticks : (long?)null,
                v => v.HasValue ? new TimeSpan(v.Value) : (TimeSpan?)null);

        modelBuilder.Entity<WorkflowMetrics>()
            .Property(e => e.AverageExecutionTime)
            .HasConversion(v => v.Ticks, v => new TimeSpan(v));

        modelBuilder.Entity<WorkflowMetrics>()
            .Property(e => e.MinExecutionTime)
            .HasConversion(v => v.Ticks, v => new TimeSpan(v));

        modelBuilder.Entity<WorkflowMetrics>()
            .Property(e => e.MaxExecutionTime)
            .HasConversion(v => v.Ticks, v => new TimeSpan(v));
    }

    private void ConfigureJsonColumns(ModelBuilder modelBuilder)
    {
        // Configure JSON columns for properties that store complex objects
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer")
        {
            // SQL Server JSON configuration
            modelBuilder.Entity<Role>().OwnsOne(e => e.Settings, builder => builder.ToJson());
            modelBuilder.Entity<Permission>().OwnsOne(e => e.Metadata, builder => builder.ToJson());
            modelBuilder.Entity<Workspace>().OwnsOne(e => e.Settings, builder => builder.ToJson());
            modelBuilder.Entity<Project>().OwnsOne(e => e.Settings, builder => builder.ToJson());
            modelBuilder.Entity<Core.Entities.WorkflowEnvironment>().OwnsOne(e => e.Variables, builder => builder.ToJson());
            modelBuilder.Entity<Core.Entities.WorkflowEnvironment>().OwnsOne(e => e.Settings, builder => builder.ToJson());
            modelBuilder.Entity<Workflow>().OwnsOne(e => e.GlobalVariables, builder => builder.ToJson());
            modelBuilder.Entity<Workflow>().OwnsOne(e => e.Configuration, builder => builder.ToJson());
            modelBuilder.Entity<WorkflowStep>().OwnsOne(e => e.InputMapping, builder => builder.ToJson());
            modelBuilder.Entity<WorkflowStep>().OwnsOne(e => e.OutputMapping, builder => builder.ToJson());
            modelBuilder.Entity<WorkflowStep>().OwnsOne(e => e.Conditions, builder => builder.ToJson());
            modelBuilder.Entity<WorkflowStep>().OwnsOne(e => e.Configuration, builder => builder.ToJson());
            modelBuilder.Entity<ApiEndpoint>().OwnsOne(e => e.Headers, builder => builder.ToJson());
            modelBuilder.Entity<ApiEndpoint>().OwnsOne(e => e.Authentication, builder => builder.ToJson());
            modelBuilder.Entity<ApiEndpoint>().OwnsOne(e => e.Configuration, builder => builder.ToJson());
            modelBuilder.Entity<ApiParameter>().OwnsOne(e => e.Schema, builder => builder.ToJson());
            modelBuilder.Entity<ApiParameter>().OwnsOne(e => e.Configuration, builder => builder.ToJson());
            modelBuilder.Entity<WorkflowExecution>().OwnsOne(e => e.InputData, builder => builder.ToJson());
            modelBuilder.Entity<WorkflowExecution>().OwnsOne(e => e.OutputData, builder => builder.ToJson());
            modelBuilder.Entity<WorkflowExecution>().OwnsOne(e => e.Context, builder => builder.ToJson());
            modelBuilder.Entity<ExecutionLog>().OwnsOne(e => e.Data, builder => builder.ToJson());
            modelBuilder.Entity<WorkflowApproval>().OwnsOne(e => e.Changes, builder => builder.ToJson());
            modelBuilder.Entity<WorkflowApproval>().OwnsOne(e => e.Metadata, builder => builder.ToJson());
            modelBuilder.Entity<WorkflowSchedule>().OwnsOne(e => e.Parameters, builder => builder.ToJson());
            modelBuilder.Entity<WorkflowSchedule>().OwnsOne(e => e.Configuration, builder => builder.ToJson());
            modelBuilder.Entity<WorkflowVersion>().OwnsOne(e => e.Configuration, builder => builder.ToJson());
            modelBuilder.Entity<WorkflowVersion>().OwnsOne(e => e.Metadata, builder => builder.ToJson());
            modelBuilder.Entity<WorkflowMetrics>().OwnsOne(e => e.ErrorCounts, builder => builder.ToJson());
            modelBuilder.Entity<WorkflowMetrics>().OwnsOne(e => e.CustomMetrics, builder => builder.ToJson());
        }
        else
        {
            // Oracle or other providers - convert to string
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var jsonProperties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(Dictionary<string, object>) ||
                               p.PropertyType == typeof(Dictionary<string, string>) ||
                               p.PropertyType == typeof(Dictionary<string, int>));

                foreach (var property in jsonProperties)
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasConversion(
                            v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                            v => System.Text.Json.JsonSerializer.Deserialize(v, property.PropertyType, (System.Text.Json.JsonSerializerOptions?)null));
                }
            }
        }
    }

    private void ConfigureSoftDeleteFilter(ModelBuilder modelBuilder)
    {
        // Global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(SoftDeleteEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var body = Expression.Equal(
                    Expression.Property(parameter, nameof(SoftDeleteEntity.IsDeleted)),
                    Expression.Constant(false));
                var lambda = Expression.Lambda(body, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    private void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // User indexes
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("IX_Users_Username");

        // Workflow indexes
        modelBuilder.Entity<Workflow>()
            .HasIndex(w => w.Status)
            .HasDatabaseName("IX_Workflows_Status");

        modelBuilder.Entity<Workflow>()
            .HasIndex(w => w.IsPublished)
            .HasDatabaseName("IX_Workflows_IsPublished");

        // WorkflowExecution indexes
        modelBuilder.Entity<WorkflowExecution>()
            .HasIndex(we => we.Status)
            .HasDatabaseName("IX_WorkflowExecutions_Status");

        modelBuilder.Entity<WorkflowExecution>()
            .HasIndex(we => we.CreatedAt)
            .HasDatabaseName("IX_WorkflowExecutions_CreatedAt");

        // ExecutionLog indexes
        modelBuilder.Entity<ExecutionLog>()
            .HasIndex(el => el.Level)
            .HasDatabaseName("IX_ExecutionLogs_Level");

        modelBuilder.Entity<ExecutionLog>()
            .HasIndex(el => el.Timestamp)
            .HasDatabaseName("IX_ExecutionLogs_Timestamp");

        // ApiEndpoint indexes
        modelBuilder.Entity<ApiEndpoint>()
            .HasIndex(ae => new { ae.Url, ae.Method })
            .IsUnique()
            .HasDatabaseName("IX_ApiEndpoints_Url_Method");

        // WorkflowApproval indexes
        modelBuilder.Entity<WorkflowApproval>()
            .HasIndex(wa => wa.Status)
            .HasDatabaseName("IX_WorkflowApprovals_Status");

        // WorkflowSchedule indexes
        modelBuilder.Entity<WorkflowSchedule>()
            .HasIndex(ws => ws.NextRunTime)
            .HasDatabaseName("IX_WorkflowSchedules_NextRunTime");

        // WorkflowMetrics indexes
        modelBuilder.Entity<WorkflowMetrics>()
            .HasIndex(wm => wm.Date)
            .HasDatabaseName("IX_WorkflowMetrics_Date");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                // CreatedBy and UpdatedBy should be set by the service layer
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                // UpdatedBy should be set by the service layer
                entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                entry.Property(nameof(BaseEntity.CreatedBy)).IsModified = false;
            }
        }
    }
}