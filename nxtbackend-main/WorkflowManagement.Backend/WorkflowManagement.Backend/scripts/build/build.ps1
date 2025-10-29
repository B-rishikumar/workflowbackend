# =============================================
# WorkflowManagement Build Script
# =============================================

param(
    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "./bin",
    
    [Parameter(Mandatory=$false)]
    [switch]$RunTests,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipRestore,
    
    [Parameter(Mandatory=$false)]
    [switch]$CreatePackage,
    
    [Parameter(Mandatory=$false)]
    [switch]$Verbose,
    
    [Parameter(Mandatory=$false)]
    [switch]$Clean
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Colors for output
$ColorInfo = "Cyan"
$ColorSuccess = "Green"
$ColorWarning = "Yellow"
$ColorError = "Red"

function Write-BuildLog {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] $Message" -ForegroundColor $Color
}

function Test-Prerequisites {
    Write-BuildLog "Checking prerequisites..." -Color $ColorInfo
    
    # Check if .NET SDK is installed
    try {
        $dotnetVersion = dotnet --version
        Write-BuildLog "✓ .NET SDK version: $dotnetVersion" -Color $ColorSuccess
    }
    catch {
        Write-BuildLog "✗ .NET SDK is not installed or not in PATH" -Color $ColorError
        exit 1
    }
    
    # Check if solution file exists
    if (-not (Test-Path "WorkflowManagement.sln")) {
        Write-BuildLog "✗ Solution file 'WorkflowManagement.sln' not found" -Color $ColorError
        exit 1
    }
    
    Write-BuildLog "✓ Prerequisites check passed" -Color $ColorSuccess
}

function Clean-Solution {
    if ($Clean) {
        Write-BuildLog "Cleaning solution..." -Color $ColorInfo
        
        try {
            dotnet clean WorkflowManagement.sln --configuration $Configuration --verbosity minimal
            
            # Remove bin and obj directories
            Get-ChildItem -Path . -Recurse -Directory -Name "bin", "obj" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
            
            Write-BuildLog "✓ Solution cleaned successfully" -Color $ColorSuccess
        }
        catch {
            Write-BuildLog "✗ Failed to clean solution: $_" -Color $ColorError
            exit 1
        }
    }
}

function Restore-Packages {
    if (-not $SkipRestore) {
        Write-BuildLog "Restoring NuGet packages..." -Color $ColorInfo
        
        try {
            dotnet restore WorkflowManagement.sln --verbosity minimal
            Write-BuildLog "✓ Package restore completed successfully" -Color $ColorSuccess
        }
        catch {
            Write-BuildLog "✗ Package restore failed: $_" -Color $ColorError
            exit 1
        }
    }
    else {
        Write-BuildLog "Skipping package restore..." -Color $ColorWarning
    }
}

function Build-Solution {
    Write-BuildLog "Building solution in $Configuration configuration..." -Color $ColorInfo
    
    try {
        $verbosity = if ($Verbose) { "normal" } else { "minimal" }
        
        dotnet build WorkflowManagement.sln `
            --configuration $Configuration `
            --output $OutputPath `
            --verbosity $verbosity `
            --no-restore:$SkipRestore
            
        Write-BuildLog "✓ Build completed successfully" -Color $ColorSuccess
    }
    catch {
        Write-BuildLog "✗ Build failed: $_" -Color $ColorError
        exit 1
    }
}

function Run-Tests {
    if ($RunTests) {
        Write-BuildLog "Running tests..." -Color $ColorInfo
        
        try {
            # Run unit tests
            dotnet test tests/WorkflowManagement.UnitTests/WorkflowManagement.UnitTests.csproj `
                --configuration $Configuration `
                --logger "console;verbosity=normal" `
                --collect:"XPlat Code Coverage" `
                --no-build
            
            # Run integration tests
            dotnet test tests/WorkflowManagement.IntegrationTests/WorkflowManagement.IntegrationTests.csproj `
                --configuration $Configuration `
                --logger "console;verbosity=normal" `
                --no-build
            
            Write-BuildLog "✓ All tests passed" -Color $ColorSuccess
        }
        catch {
            Write-BuildLog "✗ Tests failed: $_" -Color $ColorError
            exit 1
        }
    }
    else {
        Write-BuildLog "Skipping tests (use -RunTests to run tests)..." -Color $ColorWarning
    }
}

function Create-DeploymentPackage {
    if ($CreatePackage) {
        Write-BuildLog "Creating deployment package..." -Color $ColorInfo
        
        try {
            # Create publish folder
            $publishPath = Join-Path $OutputPath "publish"
            
            # Publish the API project
            dotnet publish src/WorkflowManagement.API/WorkflowManagement.API.csproj `
                --configuration $Configuration `
                --output $publishPath `
                --runtime win-x64 `
                --self-contained false `
                --no-build
            
            # Create zip package
            $packageName = "WorkflowManagement-$(Get-Date -Format 'yyyyMMdd-HHmmss').zip"
            $packagePath = Join-Path $OutputPath $packageName
            
            Compress-Archive -Path "$publishPath/*" -DestinationPath $packagePath -Force
            
            Write-BuildLog "✓ Deployment package created: $packagePath" -Color $ColorSuccess
        }
        catch {
            Write-BuildLog "✗ Package creation failed: $_" -Color $ColorError
            exit 1
        }
    }
}

function Show-BuildSummary {
    Write-BuildLog "=== BUILD SUMMARY ===" -Color $ColorInfo
    Write-BuildLog "Configuration: $Configuration" -Color "White"
    Write-BuildLog "Output Path: $OutputPath" -Color "White"
    Write-BuildLog "Tests Run: $(if ($RunTests) { 'Yes' } else { 'No' })" -Color "White"
    Write-BuildLog "Package Created: $(if ($CreatePackage) { 'Yes' } else { 'No' })" -Color "White"
    Write-BuildLog "Build completed at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -Color "White"
    Write-BuildLog "====================" -Color $ColorInfo
}

function Show-Usage {
    Write-Host @"
WorkflowManagement Build Script

Usage: ./build.ps1 [OPTIONS]

Options:
  -Configuration <config>   Build configuration (Debug|Release). Default: Release
  -OutputPath <path>        Output directory for build artifacts. Default: ./bin
  -RunTests                 Run unit and integration tests after build
  -SkipRestore             Skip NuGet package restore
  -CreatePackage           Create deployment package after successful build
  -Verbose                 Enable verbose build output
  -Clean                   Clean solution before build
  -Help                    Show this help message

Examples:
  ./build.ps1                                    # Basic build in Release mode
  ./build.ps1 -Configuration Debug -RunTests    # Debug build with tests
  ./build.ps1 -RunTests -CreatePackage         # Release build with tests and package
  ./build.ps1 -Clean -Verbose                   # Clean build with verbose output

"@ -ForegroundColor $ColorInfo
}

# =============================================
# Main Execution
# =============================================

try {
    # Show usage if help requested
    if ($args -contains "-Help" -or $args -contains "--help" -or $args -contains "-h") {
        Show-Usage
        exit 0
    }
    
    Write-BuildLog "Starting WorkflowManagement build process..." -Color $ColorInfo
    Write-BuildLog "Build started at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -Color $ColorInfo
    
    # Execute build steps
    Test-Prerequisites
    Clean-Solution
    Restore-Packages
    Build-Solution
    Run-Tests
    Create-DeploymentPackage
    
    # Show summary
    Show-BuildSummary
    
    Write-BuildLog "✓ Build process completed successfully!" -Color $ColorSuccess
    exit 0
}
catch {
    Write-BuildLog "✗ Build process failed: $_" -Color $ColorError
    exit 1
}

# =============================================
# Additional Helper Functions
# =============================================

function Get-ProjectVersion {
    param([string]$ProjectFile)
    
    try {
        [xml]$proj = Get-Content $ProjectFile
        $version = $proj.Project.PropertyGroup.Version
        return $version
    }
    catch {
        return "Unknown"
    }
}

function Update-AssemblyInfo {
    param(
        [string]$Version,
        [string]$BuildNumber
    )
    
    # This function can be extended to update assembly info files
    # with build numbers, git commit hashes, etc.
    Write-BuildLog "Version: $Version, Build: $BuildNumber" -Color $ColorInfo
}

function Generate-BuildReport {
    param([string]$ReportPath)
    
    $report = @{
        BuildDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        Configuration = $Configuration
        OutputPath = $OutputPath
        TestsRun = $RunTests
        PackageCreated = $CreatePackage
        MachineName = $env:COMPUTERNAME
        UserName = $env:USERNAME
    }
    
    $report | ConvertTo-Json | Out-File -FilePath $ReportPath -Encoding UTF8
    Write-BuildLog "Build report generated: $ReportPath" -Color $ColorInfo
}

# Code coverage analysis function
function Analyze-CodeCoverage {
    if ($RunTests -and (Test-Path "coverage.cobertura.xml")) {
        Write-BuildLog "Analyzing code coverage..." -Color $ColorInfo
        
        # This can be extended to use tools like ReportGenerator
        # to create HTML coverage reports
        Write-BuildLog "Code coverage analysis completed" -Color $ColorSuccess
    }
}

# Database migration function (if applicable)
function Run-DatabaseMigrations {
    param([string]$ConnectionString)
    
    if ($ConnectionString) {
        Write-BuildLog "Running database migrations..." -Color $ColorInfo
        
        try {
            dotnet ef database update --project src/WorkflowManagement.Infrastructure --startup-project src/WorkflowManagement.API --connection $ConnectionString
            Write-BuildLog "✓ Database migrations completed" -Color $ColorSuccess
        }
        catch {
            Write-BuildLog "✗ Database migration failed: $_" -Color $ColorError
            exit 1
        }
    }
}

# Docker build function
function Build-DockerImage {
    param(
        [string]$ImageName = "workflowmanagement",
        [string]$Tag = "latest"
    )
    
    if (Get-Command docker -ErrorAction SilentlyContinue) {
        Write-BuildLog "Building Docker image..." -Color $ColorInfo
        
        try {
            docker build -t "${ImageName}:${Tag}" -f scripts/deployment/Dockerfile .
            Write-BuildLog "✓ Docker image built successfully: ${ImageName}:${Tag}" -Color $ColorSuccess
        }
        catch {
            Write-BuildLog "✗ Docker build failed: $_" -Color $ColorError
        }
    }
    else {
        Write-BuildLog "Docker is not available, skipping Docker build" -Color $ColorWarning
    }
}