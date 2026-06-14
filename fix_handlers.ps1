$appPath = "src\AlNady.Application"
$files = Get-ChildItem -Recurse -Path $appPath -Filter "*.cs"

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    $newContent = $content `
        -replace 'using AlNady\.Infrastructure\.Persistence;', 'using AlNady.Application.Interfaces;' `
        -replace '\bApplicationDbContext\b', 'IApplicationDbContext'
    if ($content -ne $newContent) {
        Set-Content -Path $file.FullName -Value $newContent -NoNewline
        Write-Host "Updated: $($file.Name)"
    }
}
Write-Host "Done."
