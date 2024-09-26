Write-Host "Validating service..."
$response = Invoke-WebRequest -Uri "http://localhost" -UseBasicParsing
if ($response.StatusCode -eq 200) {
    Write-Host "Validation successful."
    exit 0
} else {
    Write-Host "Validation failed."
    exit 1
}
