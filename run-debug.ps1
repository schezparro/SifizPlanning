# Compilar el proyecto
$msbuild = 'C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe'
Write-Host "Compilando proyecto..." -ForegroundColor Yellow
& $msbuild SifizPlanning.sln /p:Configuration=Debug /verbosity:minimal

if ($LASTEXITCODE -eq 0) {
    Write-Host "Compilación exitosa. Iniciando aplicación..." -ForegroundColor Green
    # Ejecutar con IIS Express
    & 'C:\Program Files\IIS Express\iisexpress.exe' /path:"$PWD" /port:5000
} else {
    Write-Host "Error en la compilación" -ForegroundColor Red
    exit 1
}