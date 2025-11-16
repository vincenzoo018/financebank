# Final fix for all Razor syntax errors
Write-Host "Fixing all Razor files..." -ForegroundColor Green

$files = Get-ChildItem -Path . -Recurse -Filter *.razor

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    
    # Fix 1: @onclick with lambda and string parameter
    # Pattern: @onclick='() => Method("param")' 
    # This is correct, but if it got changed to @onclick="() => Method("param")" we need to fix it
    
    # Replace broken: @onclick="() => ShowReportModal("
    # With correct: @onclick='() => ShowReportModal("
    $content = $content -replace '@onclick="(\(\)\s*=>\s*\w+)\("', '@onclick=''$1("'
    
    # Fix closing parenthesis and quote
    # Replace: ")"" with ")'"
    $content = $content -replace '"\)\s*""\s*class=', "')' class="
    $content = $content -replace '"\)\s*""\s*style=', "')' style="
    
    # Fix 2: SVG attributes that got broken
    # Replace: width="16" height="16" with width='16' height='16' when inside @onclick
    # Actually, let's just ensure SVG tags use proper quotes
    
    # Save only if changed
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "Fixed: $($file.Name)" -ForegroundColor Yellow
    }
}

Write-Host "`nDone! Now run: dotnet build" -ForegroundColor Green
