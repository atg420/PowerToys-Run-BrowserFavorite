# Check for Admin Privileges and Relaunch as Administrator if Needed
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)) {
    Write-Output "This script requires administrative privileges. Relaunching as Administrator..."
    Start-Process -FilePath "powershell.exe" -ArgumentList ("-NoProfile", "-ExecutionPolicy Bypass", "-File", "$($MyInvocation.MyCommand.Definition)") -Verb RunAs
    exit
}

$zip1 = "BrowserFavorite-0.1.0-x64.zip"
$buildScript = ".\Build.ps1"
$destinationPath = "$env:ProgramFiles\PowerToys\RunPlugins"  # Destination for the extracted files
$extractedFolderName = "BrowserFavorite"  # Name of the folder inside the ZIP file
$powerToysExecutable = "$env:ProgramFiles\PowerToys\PowerToys.exe"  # Path to PowerToys executable

# 1. Close the PowerToys program
Write-Output "Closing PowerToys if it is running..."
try {
    Get-Process -Name "PowerToys" -ErrorAction SilentlyContinue | Stop-Process -Force
    Write-Output "PowerToys closed successfully."
} catch {
    Write-Warning "PowerToys was not running or could not be closed."
}

# 3. Run the build.ps1 script
Write-Output "Running the build script..."
try {
    & $buildScript
    Write-Output "Build script executed successfully."
} catch {
    Write-Error "Error running build script: $_"
    exit
}

# 4. Move the newly created zip1.zip to the destination path
Write-Output "Moving the new zip file to the destination path..."
if (Test-Path $zip1) {
    try {
        Move-Item $zip1 -Destination $destinationPath -Force
        $extractedZipPath = Join-Path -Path $destinationPath -ChildPath $zip1
        Write-Output "$zip1 moved to $destinationPath."
    } catch {
        Write-Error "Failed to move the zip file: $_"
        exit
    }

    # 5. Check if the BrowserFavorite folder exists and delete it
    $folderPath = Join-Path -Path $destinationPath -ChildPath $extractedFolderName
    if (Test-Path $folderPath) {
        Write-Output "Deleting existing folder $extractedFolderName..."
        try {
            Remove-Item -Path $folderPath -Recurse -Force
            Write-Output "Folder $extractedFolderName deleted successfully."
        } catch {
            exit
        }
    }

    # 6. Extract the zip file and delete it
    Write-Output "Extracting the zip file..."
    try {
        Expand-Archive -Path $extractedZipPath -DestinationPath $destinationPath -Force
        Remove-Item $extractedZipPath -Force
        Write-Output "$zip1 extracted and deleted successfully."
    } catch {
        Write-Error "Error extracting the zip file: $_"
        exit
    }
} else {
    Write-Error "Error: $zip1 not found after build process."
    exit
}

# 7. Run PowerToys
Write-Output "Starting PowerToys..."
try {
    Start-Process -FilePath $powerToysExecutable
    Write-Output "PowerToys started successfully."
} catch {
    Write-Error "Failed to start PowerToys: $_"
}
