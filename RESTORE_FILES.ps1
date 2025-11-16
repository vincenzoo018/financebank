# Script to restore broken Razor files
# This fixes the lambda expressions that were corrupted

Write-Host "Restoring broken Razor files..." -ForegroundColor Green

# Get all .razor files
$razorFiles = Get-ChildItem -Path . -Recurse -Filter *.razor

foreach ($file in $razorFiles) {
    $content = Get-Content $file.FullName -Raw
    
    # Fix broken lambda expressions: @onclick="() => Method("param")" back to @onclick='() => Method("param")'
    # Pattern: @onclick="() => MethodName("
    $content = $content -replace '@onclick="(\(\) => \w+)\("', '@onclick=''$1("'
    
    # Fix closing: ")"" back to ")'"
    $content = $content -replace '"\)""', "')'"
    
    # Save the file
    Set-Content -Path $file.FullName -Value $content -NoNewline
    
    Write-Host "Fixed: $($file.Name)" -ForegroundColor Yellow
}

Write-Host "`nAll files restored!" -ForegroundColor Green
Write-Host "Now run: dotnet build" -ForegroundColor Cyan
