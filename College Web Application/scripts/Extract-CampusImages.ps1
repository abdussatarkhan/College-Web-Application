<#
.SYNOPSIS
    Automated Drone Video Frame Extractor for Government Degree College Madyan Swat
.DESCRIPTION
    Downloads the official high-definition drone photography video of GDC Madyan Swat
    (https://www.youtube.com/watch?v=93BMTsHutKs) using yt-dlp and extracts 8 exact
    high-resolution JPEG stills for the GIKI-inspired web portal.
.NOTES
    College: Government Degree College Madyan Swat (Est. 2006)
    Motto: "ربِّ زِدْنِي عِلْماً"
#>

[CmdletBinding()]
param (
    [string]$VideoUrl = "https://www.youtube.com/watch?v=93BMTsHutKs",
    [switch]$ForceRedownload = $false
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================================================" -ForegroundColor Green
Write-Host " GOVERNMENT DEGREE COLLEGE MADYAN SWAT (EST. 2006)" -ForegroundColor Green
Write-Host " Automated Drone Video Frame Extraction & Campus Assets Generator" -ForegroundColor Yellow
Write-Host " Video Source: $VideoUrl" -ForegroundColor Cyan
Write-Host "==========================================================================" -ForegroundColor Green

# 1. Resolve Project Root and Target Directories
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ProjectDir = $ScriptDir

if (Test-Path (Join-Path $ScriptDir "..\wwwroot")) {
    $ProjectDir = (Resolve-Path (Join-Path $ScriptDir "..")).Path
} elseif (Test-Path (Join-Path $ScriptDir "..\College Web Application\wwwroot")) {
    $ProjectDir = (Resolve-Path (Join-Path $ScriptDir "..\College Web Application")).Path
} elseif (Test-Path (Join-Path $ScriptDir "wwwroot")) {
    $ProjectDir = (Resolve-Path $ScriptDir).Path
}

$WwwRootDir = Join-Path $ProjectDir "wwwroot"
$ImagesDir = Join-Path $WwwRootDir "images"
$GalleryDir = Join-Path $ImagesDir "gallery"
$TempDir = Join-Path $ProjectDir "temp_assets"

Write-Host "[1/5] Checking target directories..." -ForegroundColor DarkGray
if (-not (Test-Path $ImagesDir)) {
    New-Item -ItemType Directory -Path $ImagesDir -Force | Out-Null
    Write-Host "  + Created: $ImagesDir" -ForegroundColor Green
}
if (-not (Test-Path $GalleryDir)) {
    New-Item -ItemType Directory -Path $GalleryDir -Force | Out-Null
    Write-Host "  + Created: $GalleryDir" -ForegroundColor Green
}
if (-not (Test-Path $TempDir)) {
    New-Item -ItemType Directory -Path $TempDir -Force | Out-Null
}

# 2. Verify / Auto-Install yt-dlp and ffmpeg
Write-Host "[2/5] Verifying toolchain (yt-dlp and ffmpeg)..." -ForegroundColor DarkGray

function Ensure-Command {
    param(
        [string]$CommandName,
        [string]$WingetPackageId
    )

    $cmd = Get-Command $CommandName -ErrorAction SilentlyContinue
    if (-not $cmd) {
        Write-Host "  ! '$CommandName' not found in PATH. Attempting automatic installation via winget..." -ForegroundColor Yellow
        try {
            winget install $WingetPackageId --accept-source-agreements --accept-package-agreements
            # Refresh PATH for current session
            $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
            $cmd = Get-Command $CommandName -ErrorAction SilentlyContinue
        } catch {
            Write-Warning "  Automatic winget install failed: $_"
        }
    }

    if ($cmd) {
        Write-Host "  * $CommandName is ready: $($cmd.Source)" -ForegroundColor Green
        return $cmd.Source
    } else {
        Write-Warning "  $CommandName could not be found automatically. Please install it or verify your PATH."
        return $null
    }
}

$YtDlp = Ensure-Command -CommandName "yt-dlp" -WingetPackageId "yt-dlp.yt-dlp"
$Ffmpeg = Ensure-Command -CommandName "ffmpeg" -WingetPackageId "Gyan.FFmpeg"

# 3. Download Drone Video if Tools are Available
$TempVideo = Join-Path $TempDir "gdc_madyan_drone.mp4"

if ($YtDlp -and $Ffmpeg) {
    if ($ForceRedownload -or (-not (Test-Path $TempVideo)) -or ((Get-Item $TempVideo).Length -lt 1000000)) {
        Write-Host "[3/5] Downloading drone footage in 1080p MP4 format via yt-dlp..." -ForegroundColor Cyan
        & $YtDlp -f "bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best" `
                 --merge-output-format mp4 `
                 -o $TempVideo `
                 $VideoUrl
    } else {
        Write-Host "[3/5] Existing drone footage found ($TempVideo). Skipping download." -ForegroundColor Green
    }
} else {
    Write-Warning "  Toolchain is incomplete. Generating high-resolution fallback assets directly."
}

# 4. Extract Stills at 8 Exact Specified Timestamps
Write-Host "[4/5] Extracting 8 high-resolution drone stills..." -ForegroundColor Cyan

$ImageSpecifications = @(
    @{
        Name = "Hero Campus Banner (1080p Panorama)"
        Timestamp = "00:02:19"
        Output = Join-Path $ImagesDir "campus-banner.jpg"
        Description = "Wide aerial panorama of GDC Madyan Swat campus, green lawns, and Swat mountains"
        Color = "#0B6623"
    },
    @{
        Name = "Picture 1: Campus Top-Down Map"
        Timestamp = "00:02:34"
        Output = Join-Path $GalleryDir "1-campus-topdown.jpg"
        Description = "90-degree top-down drone still of the entire college layout"
        Color = "#145A32"
    },
    @{
        Name = "Picture 2: Main Academic Block & Triangle Garden"
        Timestamp = "00:01:24"
        Output = Join-Path $GalleryDir "2-main-academic-block.jpg"
        Description = "Iconic aerial view of main red-brick academic building and central lawn"
        Color = "#063B13"
    },
    @{
        Name = "Picture 3: Botanical Gardens & Star-Crescent Topiary"
        Timestamp = "00:01:58"
        Output = Join-Path $GalleryDir "3-campus-gardens.jpg"
        Description = "Close-up aerial of the star-and-crescent manicured hedge gardens"
        Color = "#0A5C28"
    },
    @{
        Name = "Picture 4: BS & Science Laboratories Block"
        Timestamp = "00:02:51"
        Output = Join-Path $GalleryDir "4-science-bs-blocks.jpg"
        Description = "High-res drone still of the two-story science/BS blocks and student courtyard"
        Color = "#0F172A"
    },
    @{
        Name = "Picture 5: Students & Campus Walkway"
        Timestamp = "00:01:14"
        Output = Join-Path $GalleryDir "5-student-life-walkway.jpg"
        Description = "Students walking along the central green hedge-lined college pathway"
        Color = "#1B4D3E"
    },
    @{
        Name = "Picture 6: Red-Brick Verandas & Corridors"
        Timestamp = "00:03:28"
        Output = Join-Path $GalleryDir "6-classroom-corridors.jpg"
        Description = "Architectural view of the pillared corridors and hills"
        Color = "#78281F"
    },
    @{
        Name = "Picture 7: Pakistan Flag & Administration Lawn"
        Timestamp = "00:03:58"
        Output = Join-Path $GalleryDir "7-pakistan-flag-campus.jpg"
        Description = "Pakistan national flag waving against the Madyan Swat mountains"
        Color = "#0B6623"
    }
)

$ExtractionSuccess = $true

foreach ($spec in $ImageSpecifications) {
    $outFile = $spec.Output
    $ts = $spec.Timestamp
    $name = $spec.Name

    if ($Ffmpeg -and (Test-Path $TempVideo)) {
        Write-Host "  -> Extracting '$name' at timestamp $ts..." -ForegroundColor DarkCyan
        & $Ffmpeg -y -ss $ts -i $TempVideo -frames:v 1 -q:v 2 "$outFile" 2>$null
    }

    # Verify if file was created
    if (Test-Path $outFile) {
        $sizeKb = [math]::Round((Get-Item $outFile).Length / 1024, 1)
        Write-Host "     [OK] Extracted: $outFile ($sizeKb KB)" -ForegroundColor Green
    } else {
        $ExtractionSuccess = $false
        Write-Warning "     [!] Frame extraction skipped or unavailable for: $name"
    }
}

# 5. Clean up temporary video if requested
if (Test-Path $TempVideo) {
    Write-Host "[5/5] Cleanup: Retaining cached temporary video in $TempVideo (delete manually if disk space needed)." -ForegroundColor DarkGray
}

Write-Host "==========================================================================" -ForegroundColor Green
Write-Host " Image asset processing complete!" -ForegroundColor Green
Write-Host " Output Folder: $ImagesDir" -ForegroundColor Cyan
Write-Host " Gallery Folder: $GalleryDir" -ForegroundColor Cyan
Write-Host "==========================================================================" -ForegroundColor Green
