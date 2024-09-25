Write-Host "Stopping the existing application..."

# If you're using IIS to host the application, stop the IIS app pool or website
# Replace "YourAppPoolName" with your IIS Application Pool name
Stop-WebAppPool -Name "YourAppPoolName"

Write-Host "Existing application stopped."

# Example: Stop a Windows Service
# Write-Host "Stopping the service..."
# Stop-Service -Name "YourServiceName" -Force

# Write-Host "Service stopped."
