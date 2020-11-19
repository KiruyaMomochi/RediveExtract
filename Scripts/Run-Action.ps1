param (
    [string]
    $Repository,
    [string]
    $Workflow,
    [string]
    $Token
)

Invoke-RestMethod `
    -Method Post `
    -Headers @{Accept='application/vnd.github.v3+json'; `
               Authorization="Bearer $Token"} `
    -Body '{"ref": "master"}' `
    -Uri "https://api.github.com/repos/$Repository/actions/workflows/$Workflow/dispatches" `
