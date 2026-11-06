param([string]$Output="src/Brendigo.Web2APK.Windows/Assets/Brendigo.ico")
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName PresentationFramework
Add-Type -AssemblyName WindowsBase
$c = { param([string]$hex)
    return [System.Windows.Media.ColorConverter]::ConvertFromString($hex)
}
function MakeBrush([string]$color) {
    return [System.Windows.Media.SolidColorBrush]::new((& $c $color))
}
function MakePen([string]$color,[double]$width) {
    $pen = [System.Windows.Media.Pen]::new((MakeBrush $color),$width)
    $pen.StartLineCap = [System.Windows.Media.PenLineCap]::Round
    $pen.EndLineCap = [System.Windows.Media.PenLineCap]::Round
    $pen.Freeze()
    return $pen
}
function MakeIconPng([int]$size) {
    $visual=[System.Windows.Media.DrawingVisual]::new()
    $dc=$visual.RenderOpen()
    $scale=([double]$size)/512.0
    $dc.PushTransform([System.Windows.Media.ScaleTransform]::new($scale,$scale))
    $grad=[System.Windows.Media.LinearGradientBrush]::new()
    $grad.StartPoint=[System.Windows.Point]::new(0,0)
    $grad.EndPoint=[System.Windows.Point]::new(1,1)
    $grad.GradientStops.Add([System.Windows.Media.GradientStop]::new((& $c "#1E9DFF"),0))
    $grad.GradientStops.Add([System.Windows.Media.GradientStop]::new((& $c "#6767FF"),0.52))
    $grad.GradientStops.Add([System.Windows.Media.GradientStop]::new((& $c "#A05BFF"),1))
    $dc.DrawRoundedRectangle($grad,$null,[System.Windows.Rect]::new(0,0,512,512),111,111)
    $dc.DrawRoundedRectangle((MakeBrush "#14213F"),(MakePen "#7381CD" 3),
        [System.Windows.Rect]::new(30,30,452,452),94,94)
    $dc.DrawGeometry($null,(MakePen "#FFFFFF" 29),
        [System.Windows.Media.Geometry]::Parse("M132 104h135c79 0 112 103 45 144 80 46 36 152-45 152H132V104Z"))
    $dc.DrawGeometry($null,(MakePen "#81D9FF" 19),
        [System.Windows.Media.Geometry]::Parse("M155 246h158M155 345h145"))
    $dc.DrawEllipse((MakeBrush "#14274C"),(MakePen "#72E0FF" 16),
        [System.Windows.Point]::new(351,345),99,99)
    $dc.DrawGeometry($null,(MakePen "#72E0FF" 12),
        [System.Windows.Media.Geometry]::Parse("M254 345h194 M351 248c-51 63-51 131 0 194m0-194c51 63 51 131 0 194"))
    $dc.Pop()
    $dc.Close()
    $bmp=[System.Windows.Media.Imaging.RenderTargetBitmap]::new($size,$size,96,96,
        [System.Windows.Media.PixelFormats]::Pbgra32)
    $bmp.Render($visual)
    $encoder=[System.Windows.Media.Imaging.PngBitmapEncoder]::new()
    $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($bmp))
    $ms=[System.IO.MemoryStream]::new()
    $encoder.Save($ms)
    $bytes=$ms.ToArray()
    $ms.Dispose()
    return $bytes
}
$sizes=@(32,48,64,256)
$images=@()
foreach($size in $sizes) { $images+=,[PSCustomObject]@{ Size=$size; Bytes=(MakeIconPng $size) } }
$dir=[System.IO.Path]::GetDirectoryName($Output)
[System.IO.Directory]::CreateDirectory($dir) | Out-Null
$stream=[System.IO.File]::Create($Output)
$writer=[System.IO.BinaryWriter]::new($stream)
try {
    $writer.Write([uint16]0);$writer.Write([uint16]1);$writer.Write([uint16]$images.Count)
    $offset=6+16*$images.Count
    foreach($item in $images) {
        $dim=if($item.Size -eq 256){ 0 }else{$item.Size}
        $writer.Write([byte]$dim);$writer.Write([byte]$dim)
        $writer.Write([byte]0);$writer.Write([byte]0)
        $writer.Write([uint16]1);$writer.Write([uint16]32)
        $writer.Write([uint32]$item.Bytes.Length)
        $writer.Write([uint32]$offset)
        $offset+=$item.Bytes.Length
    }
    foreach($item in $images){$writer.Write([byte[]]$item.Bytes)}
}finally{$writer.Dispose();$stream.Dispose()}
Write-Host "Generated original Brendigo launcher icon: $Output"
