/**
 * Script de testing para verificar el comportamiento de fechas
 * entre diferentes zonas horarias (Ecuador vs Cuba)
 * 
 * Para usar este script:
 * 1. Abrir la consola del navegador en SifizPlanning
 * 2. Copiar y pegar este código
 * 3. Ejecutar las funciones de test
 */

// Función de normalización (copia de la implementada en solicitudesController.js)
function normalizarFecha(fecha) {
    if (!fecha) return null;
    
    var fechaObj;
    if (typeof fecha === 'string') {
        fechaObj = new Date(fecha);
    } else {
        fechaObj = fecha;
    }
    
    // Crear una nueva fecha en UTC para evitar problemas de zona horaria
    var year = fechaObj.getFullYear();
    var month = ('0' + (fechaObj.getMonth() + 1)).slice(-2);
    var day = ('0' + fechaObj.getDate()).slice(-2);
    
    // Retornar en formato ISO date (YYYY-MM-DD) sin hora para evitar conversiones de zona horaria
    return year + '-' + month + '-' + day;
}

// Función para probar fechas
function testFechas() {
    console.log('=== TEST DE FECHAS - ECUADOR vs CUBA ===');
    
    // Simular fecha seleccionada: 15 de agosto de 2025
    var fechaInput = '2025-08-15';
    
    // Crear objeto Date como lo haría el input HTML
    var fechaLocal = new Date(fechaInput);
    console.log('Fecha original del input:', fechaInput);
    console.log('Fecha como objeto Date:', fechaLocal);
    console.log('Zona horaria local:', Intl.DateTimeFormat().resolvedOptions().timeZone);
    console.log('Offset de zona horaria:', fechaLocal.getTimezoneOffset(), 'minutos');
    
    // Mostrar cómo se vería sin normalización
    console.log('--- SIN NORMALIZACIÓN ---');
    console.log('toISOString():', fechaLocal.toISOString());
    console.log('toString():', fechaLocal.toString());
    console.log('getDate():', fechaLocal.getDate());
    console.log('getMonth():', fechaLocal.getMonth() + 1);
    console.log('getFullYear():', fechaLocal.getFullYear());
    
    // Mostrar cómo se ve con normalización
    console.log('--- CON NORMALIZACIÓN ---');
    var fechaNormalizada = normalizarFecha(fechaLocal);
    console.log('Fecha normalizada:', fechaNormalizada);
    
    // Simular diferentes zonas horarias
    console.log('--- SIMULACIÓN DE DIFERENTES ZONAS ---');
    
    // Fecha en formato ISO con diferentes horas (simulando zonas horarias)
    var fechas = [
        '2025-08-15T00:00:00.000Z',  // UTC
        '2025-08-15T05:00:00.000Z',  // Ecuador (UTC-5)
        '2025-08-15T04:00:00.000Z',  // Cuba (UTC-4)
        '2025-08-15T12:00:00.000Z',  // Medio día UTC
    ];
    
    fechas.forEach(function(fechaISO, index) {
        var fecha = new Date(fechaISO);
        console.log(`Fecha ${index + 1}:`, fechaISO);
        console.log('  Como Date:', fecha);
        console.log('  Normalizada:', normalizarFecha(fecha));
        console.log('  ---');
    });
}

// Función para probar el envío de solicitud
function testEnvioSolicitud() {
    console.log('=== TEST DE ENVÍO DE SOLICITUD ===');
    
    // Simular datos de solicitud
    var solicitudTest = {
        fechaIngresoSolicitud: new Date('2025-07-30'),
        fechaInicioVacaciones: new Date('2025-08-15'),
        fechaFinVacaciones: new Date('2025-08-25'),
        fechaPresentarseTrabajar: new Date('2025-08-26')
    };
    
    console.log('Solicitud original:', solicitudTest);
    
    // Aplicar normalización como lo haría la función real
    var solicitudNormalizada = {};
    Object.keys(solicitudTest).forEach(function(key) {
        if (solicitudTest[key] instanceof Date) {
            solicitudNormalizada[key] = normalizarFecha(solicitudTest[key]);
        } else {
            solicitudNormalizada[key] = solicitudTest[key];
        }
    });
    
    console.log('Solicitud normalizada:', solicitudNormalizada);
    
    // Mostrar JSON que se enviaría al servidor
    console.log('JSON que se enviaría:', JSON.stringify(solicitudNormalizada, null, 2));
}

// Ejecutar tests automáticamente
console.log('Ejecutando tests de fechas para SifizPlanning...');
testFechas();
testEnvioSolicitud();
console.log('Tests completados. Revisar logs arriba para ver resultados.');
