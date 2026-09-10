param(
    [Parameter(Mandatory = $true)]
    [string]$ValheimPath,
    [string]$LibrariesPath
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($LibrariesPath)) {
    $LibrariesPath = Join-Path $PSScriptRoot '../Libs/Managed'
}
$corePath = Join-Path $ValheimPath 'BepInEx/core'
$managedPath = Join-Path $ValheimPath 'valheim_Data/Managed'
Add-Type -Path (Join-Path $corePath 'Mono.Cecil.dll')

$resolver = New-Object Mono.Cecil.DefaultAssemblyResolver
$resolver.AddSearchDirectory((Resolve-Path $LibrariesPath).Path)
$resolver.AddSearchDirectory((Resolve-Path $managedPath).Path)
$resolver.AddSearchDirectory((Resolve-Path $corePath).Path)
$parameters = New-Object Mono.Cecil.ReaderParameters
$parameters.AssemblyResolver = $resolver
$failures = [System.Collections.Generic.List[string]]::new()
$checkedMethods = 0

try {
    # Inspect metadata only; these Unity assemblies cannot run in PowerShell.
    # Resolving references catches mismatched UniVRM DLLs even when the mod builds.
    foreach ($name in @('VRM.dll', 'VRM10.dll')) {
        $assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly(
            (Join-Path $LibrariesPath $name), $parameters)
        try {
            foreach ($reference in $assembly.MainModule.GetMemberReferences()) {
                if ($reference -isnot [Mono.Cecil.MethodReference]) { continue }
                if ($reference.DeclaringType.Scope.Name -ne 'UniGLTF') { continue }

                $checkedMethods++
                try {
                    if ($null -eq $reference.Resolve()) {
                        $failures.Add($name + ': ' + $reference.FullName)
                    }
                }
                catch {
                    $failures.Add($name + ': ' + $reference.FullName + ' (' + $_.Exception.Message + ')')
                }
            }
        }
        finally {
            $assembly.Dispose()
        }
    }
}
finally {
    $resolver.Dispose()
}

if ($checkedMethods -eq 0) { throw 'No UniGLTF method references were checked.' }
if ($failures.Count -gt 0) {
    $failures | Sort-Object -Unique | ForEach-Object { Write-Host $_ }
    throw ('Incompatible UniVRM dependencies: {0} unresolved references.' -f $failures.Count)
}
Write-Host ('Passed: {0} UniGLTF method references resolved in VRM 0.x and VRM 1.0.' -f $checkedMethods)
