Write-Host "Starting the application..."

# If you're using IIS, start the application pool again
Start-WebAppPool -Name "YourAppPoolName"

Write-Host "Application started successfully."


# Write-Host "Starting the service..."
# Start-Service -Name "YourServiceName"

# Write-Host "Service started successfully."
