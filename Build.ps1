$ErrorActionPreference = "Stop"

[xml]$xml = Get-Content -Path "$PSScriptRoot\Directory.Build.Props"
$version = $xml.Project.PropertyGroup.Version

foreach ($platform in "ARM64", "x64")
{
    if (Test-Path -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.BraveFavorite\bin")
    {
        Remove-Item -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.BraveFavorite\bin\*" -Recurse
    }

    dotnet build $PSScriptRoot\Community.PowerToys.Run.Plugin.BraveFavorite.sln -c Release /p:Platform=$platform

    Remove-Item -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.BraveFavorite\bin\*" -Recurse -Include *.xml, *.pdb, PowerToys.*, Wox.*
    Rename-Item -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.BraveFavorite\bin\$platform\Release" -NewName "BraveFavorite"

    # Define the destination zip file path
    $zipPath = "$PSScriptRoot\BraveFavorite-$version-$platform.zip"
    
    # Check if the zip file already exists, and remove it if it does
    if (Test-Path -Path $zipPath)
    {
       Remove-Item -Path $zipPath -Force
    }
    
    # Create the ZIP file
    Compress-Archive -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.BraveFavorite\bin\$platform\BraveFavorite" -DestinationPath $zipPath
}
