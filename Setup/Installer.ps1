"Установка кастомных функций..."
$ApprovedPath = Join-Path ([Environment]::ExpandEnvironmentVariables("%Public%")) "OpcenterRDnL\DynamicLibraries\Approved"
Copy-Item "./Functions/*" -Destination $ApprovedPath 

Read-Host -Prompt "Для завершения установки нажмите Enter..."
