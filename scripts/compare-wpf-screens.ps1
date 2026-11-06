# Compare actual WPF pixels with the previous committed, real WPF captures.
# This detects large rendering regressions, but does not prove design-reference 1:1.
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$baseline='wpf-baseline'
$current='docs/screenshots/native'
$reportDir='visual-audit'
New-Item -ItemType Directory -Force -Path $reportDir | Out-Null

$screens=@(
    'welcome','dashboard','projects','preview','devices','tools','settings','docs','about',
    'wizard-1-basics','wizard-2-appearance','wizard-3-webview',
    'wizard-4-features','wizard-5-permissions','wizard-6-build',
    'welcome-small-900','dashboard-small-900','projects-small-900',
    'settings-small-900','wizard-small-900'
)
$results=@()
$violations=@()
foreach($screen in $screens) {
    $before=Join-Path $baseline "$screen.png"
    $after=Join-Path $current "$screen.png"
    if(!(Test-Path $before) -or !(Test-Path $after)) {
        throw "Missing real WPF screenshot baseline: $screen"
    }
    $old=[System.Drawing.Bitmap]::FromFile((Resolve-Path $before).Path)
    $new=[System.Drawing.Bitmap]::FromFile((Resolve-Path $after).Path)
    try {
        if($old.Width -ne $new.Width -or $old.Height -ne $new.Height) {
            throw "WPF viewport dimensions changed unexpectedly: $screen"
        }
        [int]$changed=0
        [int]$samples=0
        for($y=0; $y -lt $old.Height; $y+=20) {
            for($x=0; $x -lt $old.Width; $x+=20) {
                $a=$old.GetPixel($x,$y)
                $b=$new.GetPixel($x,$y)
                $difference=[math]::Abs([int]$a.R-[int]$b.R)+
                    [math]::Abs([int]$a.G-[int]$b.G)+
                    [math]::Abs([int]$a.B-[int]$b.B)
                if($difference -gt 54) { $changed++ }
                $samples++
            }
        }
        $ratio=[math]::Round($changed/[double]$samples,4)
        $record=[pscustomobject]@{
            screen=$screen; viewport="$($old.Width)x$($old.Height)"
            changedSampleFraction=$ratio; sampledPoints=$samples
            comparedTo='previous committed native WPF capture'
        }
        $results+= $record
        Write-Host ("WPF pixel regression: {0} = {1:P2} changed samples" -f $screen,$ratio)
        if($ratio -gt 0.40) { $violations+=$screen }
    }
    finally { $old.Dispose(); $new.Dispose() }
}

# Design concepts may be a different size than the real WPF screenshot;
# record their existence, never misrepresent them as pixel-equivalent.
$originalConcepts=@('dashboard.png','new-project.png','projects.png',
                    'permissions.png','settings.png','build.png')
$designs=@()
foreach($filename in $originalConcepts) {
    $path=Join-Path 'docs/screenshots' $filename
    $designs+=[pscustomobject]@{
        file=$filename
        exists=(Test-Path $path)
        verifiedPixelPerfect=$false
        note='Reference artwork not validated against the live control tree in this test'
    }
}
$report=[pscustomobject]@{
    generatedAtUtc=[DateTime]::UtcNow.ToString('o')
    comparisonType='Sampled real WPF image regression; 20px stride, RGB delta >54'
    maxAllowedChangedSampleFraction=0.40
    actualScreens=$results
    suppliedDesignConcepts=$designs
    unverified='Reference pixel-perfect matching, interactive state, physical DPI, accessibility and Android hardware'
}
$report | ConvertTo-Json -Depth 8 |
    Set-Content -Path (Join-Path $reportDir 'wpf-visual-regression.json') -Encoding UTF8
if($violations.Count -gt 0) {
    throw ('Major WPF visual regressions: '+($violations -join ', '))
}
Write-Host 'PASS: previous native WPF screenshots have no major unexpected regressions.'
