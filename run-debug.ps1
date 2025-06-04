# Compilar el proyecto
$msbuild = 'C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe'
Write-Host "Compilando proyecto..." -ForegroundColor Yellow
& $msbuild SifizPlanning.sln /p:Configuration=Debug /verbosity:minimal

if ($LASTEXITCODE -eq 0) {
    Write-Host "Compilación exitosa. Iniciando aplicación..." -ForegroundColor Green
    # Ejecutar con IIS Express
    Start-Process 'C:\Program Files\IIS Express\iisexpress.exe' -ArgumentList "/path:`"$PWD`" /port:5000"
    
    # Esperar un momento para que IIS Express se inicie
    Start-Sleep -Seconds 2
    
    # Abrir navegador en el puerto 5000
    Start-Process "http://localhost:5000"
} else {
    Write-Host "Error en la compilación" -ForegroundColor Red
    exit 1
}