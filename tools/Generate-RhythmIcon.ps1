param(
    [string]$OutputDirectory = "src\Rhythm.App\Assets"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$targetDirectory = Join-Path $root $OutputDirectory
New-Item -ItemType Directory -Force -Path $targetDirectory | Out-Null

function New-RhythmBitmap {
    param(
        [int]$Size
    )

    $bitmap = [System.Drawing.Bitmap]::new($Size, $Size)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.Clear([System.Drawing.Color]::Transparent)

    $background = [System.Drawing.Color]::FromArgb(255, 246, 241, 232)
    $shadow = [System.Drawing.Color]::FromArgb(60, 13, 33, 28)
    $green = [System.Drawing.Color]::FromArgb(255, 23, 98, 78)
    $orange = [System.Drawing.Color]::FromArgb(255, 212, 137, 72)
    $dark = [System.Drawing.Color]::FromArgb(255, 27, 30, 28)

    $pad = [Math]::Round($Size * 0.08)
    $shadowRect = [System.Drawing.RectangleF]::new($pad + ($Size * 0.01), $pad + ($Size * 0.03), $Size - (2 * $pad), $Size - (2 * $pad))
    $baseRect = [System.Drawing.RectangleF]::new($pad, $pad, $Size - (2 * $pad), $Size - (2 * $pad))
    $corner = [Math]::Round($Size * 0.23)

    $shadowPath = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $shadowPath.AddArc($shadowRect.X, $shadowRect.Y, $corner, $corner, 180, 90)
    $shadowPath.AddArc($shadowRect.Right - $corner, $shadowRect.Y, $corner, $corner, 270, 90)
    $shadowPath.AddArc($shadowRect.Right - $corner, $shadowRect.Bottom - $corner, $corner, $corner, 0, 90)
    $shadowPath.AddArc($shadowRect.X, $shadowRect.Bottom - $corner, $corner, $corner, 90, 90)
    $shadowPath.CloseFigure()
    $graphics.FillPath([System.Drawing.SolidBrush]::new($shadow), $shadowPath)

    $basePath = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $basePath.AddArc($baseRect.X, $baseRect.Y, $corner, $corner, 180, 90)
    $basePath.AddArc($baseRect.Right - $corner, $baseRect.Y, $corner, $corner, 270, 90)
    $basePath.AddArc($baseRect.Right - $corner, $baseRect.Bottom - $corner, $corner, $corner, 0, 90)
    $basePath.AddArc($baseRect.X, $baseRect.Bottom - $corner, $corner, $corner, 90, 90)
    $basePath.CloseFigure()
    $graphics.FillPath([System.Drawing.SolidBrush]::new($background), $basePath)

    $ringPen = [System.Drawing.Pen]::new($green, [Math]::Max(4, $Size * 0.09))
    $ringPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $ringPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $ringRect = [System.Drawing.RectangleF]::new($Size * 0.23, $Size * 0.20, $Size * 0.54, $Size * 0.54)
    $graphics.DrawArc($ringPen, $ringRect, 208, 292)

    $accentPen = [System.Drawing.Pen]::new($orange, [Math]::Max(3, $Size * 0.05))
    $accentPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $accentPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $accentRect = [System.Drawing.RectangleF]::new($Size * 0.31, $Size * 0.28, $Size * 0.38, $Size * 0.38)
    $graphics.DrawArc($accentPen, $accentRect, 220, 122)

    $barWidth = [Math]::Round($Size * 0.08)
    $barHeight = [Math]::Round($Size * 0.24)
    $barGap = [Math]::Round($Size * 0.05)
    $barY = [Math]::Round($Size * 0.36)
    $barX = [Math]::Round(($Size - (($barWidth * 2) + $barGap)) / 2)
    $leftBar = [System.Drawing.RectangleF]::new($barX, $barY, $barWidth, $barHeight)
    $rightBar = [System.Drawing.RectangleF]::new($barX + $barWidth + $barGap, $barY, $barWidth, $barHeight)
    $barBrush = [System.Drawing.SolidBrush]::new($dark)
    $graphics.FillRoundedRectangle($barBrush, $leftBar, [Math]::Round($barWidth * 0.6))
    $graphics.FillRoundedRectangle($barBrush, $rightBar, [Math]::Round($barWidth * 0.6))

    $graphics.Dispose()
    return $bitmap
}

Update-TypeData -TypeName System.Drawing.Graphics -MemberType ScriptMethod -MemberName FillRoundedRectangle -Value {
    param(
        [System.Drawing.Brush]$Brush,
        [System.Drawing.RectangleF]$Rectangle,
        [float]$Radius
    )

    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $diameter = $Radius * 2
    $path.AddArc($Rectangle.X, $Rectangle.Y, $diameter, $diameter, 180, 90)
    $path.AddArc($Rectangle.Right - $diameter, $Rectangle.Y, $diameter, $diameter, 270, 90)
    $path.AddArc($Rectangle.Right - $diameter, $Rectangle.Bottom - $diameter, $diameter, $diameter, 0, 90)
    $path.AddArc($Rectangle.X, $Rectangle.Bottom - $diameter, $diameter, $diameter, 90, 90)
    $path.CloseFigure()
    $this.FillPath($Brush, $path)
    $path.Dispose()
} -Force

$sizes = @(16, 32, 48, 64, 128, 256)
$pngFrames = @()

foreach ($size in $sizes) {
    $bitmap = New-RhythmBitmap -Size $size
    $pngPath = Join-Path $targetDirectory "Rhythm-$size.png"
    $bitmap.Save($pngPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $memory = [System.IO.MemoryStream]::new()
    $bitmap.Save($memory, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngFrames += [PSCustomObject]@{
        Size = $size
        Bytes = $memory.ToArray()
    }
    $memory.Dispose()
    $bitmap.Dispose()
}

$pngFrames[($pngFrames.Count - 1)].Bytes | Set-Content -Encoding Byte -Path (Join-Path $targetDirectory "Rhythm.png")

$iconPath = Join-Path $targetDirectory "Rhythm.ico"
$fileStream = [System.IO.File]::Open($iconPath, [System.IO.FileMode]::Create)
$writer = [System.IO.BinaryWriter]::new($fileStream)

$writer.Write([UInt16]0)
$writer.Write([UInt16]1)
$writer.Write([UInt16]$pngFrames.Count)

$offset = 6 + (16 * $pngFrames.Count)
foreach ($frame in $pngFrames) {
    $dimension = if ($frame.Size -ge 256) { 0 } else { [byte]$frame.Size }
    $writer.Write([byte]$dimension)
    $writer.Write([byte]$dimension)
    $writer.Write([byte]0)
    $writer.Write([byte]0)
    $writer.Write([UInt16]1)
    $writer.Write([UInt16]32)
    $writer.Write([UInt32]$frame.Bytes.Length)
    $writer.Write([UInt32]$offset)
    $offset += $frame.Bytes.Length
}

foreach ($frame in $pngFrames) {
    $writer.Write($frame.Bytes)
}

$writer.Dispose()
$fileStream.Dispose()
