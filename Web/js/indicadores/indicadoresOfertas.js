indicadoresApp.controller('indicadoresOfertasController', ['$scope', function($scope) {
    // Datos reales simulados de ofertas
    var hoy = new Date();
    $scope.ofertas = [
        {codigo: '250001', ticket: '20001', cliente: '4 DE OCTUBRE', tema: 'REVISION DE CUADRE FINANCIERO CONTABLE CORRESPONDIENTE AL 12-05-2023 EN LA AGENCIA PENIPE CTA 21013505 ACTIVAS $ 1100.36', fechaGeneracion: new Date('2025-07-08'), fechaVencimiento: new Date('2025-08-01'), tipo: '1-"REVISION DE CUADRE FINANCIERO', precioOferta: null, formaPago: null, descuento: 'SI', proximaActividad: 'NO APLICA', estado: ''},
        {codigo: '250002', ticket: '20002', cliente: 'UNION EL EJIDO', tema: 'SE SOLICITA LA REVISIÓN DELE REPORTE: SegnuimientoCuentasPorInactivarActivasInactivadas_Report YA QUE SEGUN LO CONVERSADO CON LA ING JAQUELINE REINO SE REQUIERE LA ASIGNACIÓN DE UN TÉCNICO PARA VALIDAR LA CONCLUSIÓN DE ESTE REPORTE DEBIDO A QUE  SE', fechaGeneracion: new Date('2025-07-08'), fechaVencimiento: new Date('2025-07-30'), tipo: '2-SE SOLICITA LA REVISIÓN DELE R', precioOferta: 450, formaPago: '70/30', descuento: 'SI', proximaActividad: 'NO APLICA', estado: 'VENCIDA'},
        {codigo: '250003', ticket: '22260', cliente: 'UNION EL EJIDO', tema: 'De la revisión a las cuentas de orden que maneja la Cooperativa, se observó que no se encuentra registrado las obligaciones con el público de personas vinculadas, en la cuenta “7402 Operaciones pasivas con personas naturales o jurídicas vinculadas”,', fechaGeneracion: new Date('2025-07-10'), fechaVencimiento: new Date('2025-08-08'), tipo: '1003-De la revisión a las cuentas d', precioOferta: 0, formaPago: 'Mensual', descuento: 'SI', proximaActividad: 'CERTIFICAR POR CLIENTE', estado: 'ACEPTADA / CONTRATO'},
        {codigo: '250004', ticket: '23227', cliente: 'PEDRO MONCAYO', tema: 'Estiamdos buenas tardes, Se solicita su ayuda revisando, debido a que al momento de descargar la estructura l01 me refleja que es mensual cuando esta estructura es semanal, además dentro del doc preliminar no es parecido al que me genera  la herra', fechaGeneracion: new Date('2025-07-11'), fechaVencimiento: new Date('2025-08-06'), tipo: '1004-Estiamdos buenas tardes, Se', precioOferta: 200, formaPago: '100 % a la entrega', descuento: 'SI', proximaActividad: 'PLANIFICAR', estado: 'VENCIDA'},
        {codigo: '250005', ticket: '23126', cliente: 'Base de Taura', tema: 'Se requiere que los porcentajes de provisión de la cartera de crédito se calculen en el sistema Financial como indica la Normativa emitida por la Superintendencia de Economía Popular y Solidaria para las Cooperativas de Ahorro y Crédito, constituir p', fechaGeneracion: new Date('2025-07-11'), fechaVencimiento: new Date('2025-08-07'), tipo: '1005-Se requiere que los porcentaje', precioOferta: 200, formaPago: 'Mensual', descuento: 'SI', proximaActividad: 'ACEPTADO', estado: 'ACEPTADA / CONTRATO'},
        {codigo: '250006', ticket: '24222', cliente: 'UNION EL EJIDO', tema: 'Se solicita el desarrollo de una pestaña/opción en captaciones vista que permita validar si el socio tiene firmado la autorización de debito automático permita poner un check y en lo posterior las cuentas de los socios que tengan el check habilitado', fechaGeneracion: new Date('2025-07-11'), fechaVencimiento: new Date('2025-07-31'), tipo: '1002-Se solicita el desarrollo de u', precioOferta: 1500, formaPago: '100 % a la entrega', descuento: 'NO', proximaActividad: 'VALIDAR ESTIMACION', estado: 'ACEPTADA / TICKET'},
        {codigo: '', ticket: '24757', cliente: 'FLORESTA', tema: 'Implementar el ingreso de dos porcentajes de iva en el mismo comprobante de compra', fechaGeneracion: new Date('2025-02-17'), fechaVencimiento: null, tipo: 'TICKET', precioOferta: null, formaPago: null, descuento: 'NO', proximaActividad: 'COTIZAR', estado: 'PENDIENTE'},
        {codigo: '', ticket: '24435', cliente: 'CACPE YANTZAZA', tema: 'Manual y Capacitacion', fechaGeneracion: new Date('2025-02-17'), fechaVencimiento: null, tipo: 'TICKET', precioOferta: null, formaPago: null, descuento: 'NO', proximaActividad: 'COTIZAR', estado: 'PENDIENTE'},
        {codigo: '', ticket: '24975', cliente: 'UNIVERSIDAD CATÓLICA', tema: 'Integracion Banca web y Banca Movil Bankingly', fechaGeneracion: new Date('2025-02-16'), fechaVencimiento: null, tipo: 'TICKET', precioOferta: null, formaPago: null, descuento: 'NO', proximaActividad: 'COTIZAR', estado: 'PENDIENTE'},
        {codigo: '', ticket: '25561', cliente: 'UNION EL EJIDO', tema: 'FUNCIONALIDAD MANEJO DE PORCENTAJES DE IVA', fechaGeneracion: new Date('2025-02-17'), fechaVencimiento: null, tipo: 'TICKET', precioOferta: null, formaPago: null, descuento: 'NO', proximaActividad: 'COTIZAR', estado: 'PENDIENTE'},
        {codigo: '', ticket: '25562', cliente: 'COAC Los Andes Latinos Ltda.', tema: 'SINCRONIZACIÓN AL ACTUALIZAR CORREO ELECTRONICO EN SISTEMA FINANCIAL Y COOP VIRTUAL', fechaGeneracion: new Date('2025-02-17'), fechaVencimiento: null, tipo: 'TICKET', precioOferta: null, formaPago: null, descuento: 'NO', proximaActividad: 'COTIZAR', estado: 'PENDIENTE'},
        {codigo: '', ticket: '25545', cliente: 'ASPIRE', tema: 'Control de Cambios - CC-35', fechaGeneracion: new Date('2025-02-17'), fechaVencimiento: null, tipo: 'TICKET', precioOferta: null, formaPago: null, descuento: 'NO', proximaActividad: 'COTIZAR', estado: 'PENDIENTE'},
        {codigo: '', ticket: '25601', cliente: 'FLORESTA', tema: 'REPORTE DE BENEFICIARIOS FINALES Y DE COMPOSICIÓN SOCIETARIA REBEFICS', fechaGeneracion: new Date('2025-02-14'), fechaVencimiento: null, tipo: 'TICKET', precioOferta: null, formaPago: null, descuento: 'NO', proximaActividad: 'COTIZAR', estado: 'PENDIENTE'},
        {codigo: '', ticket: '25698', cliente: 'SALINAS', tema: 'INFORMACION SOLICITADO POR EL SRI', fechaGeneracion: new Date('2025-02-17'), fechaVencimiento: null, tipo: 'TICKET', precioOferta: null, formaPago: null, descuento: 'NO', proximaActividad: 'COTIZAR', estado: 'PENDIENTE'},
        {codigo: '', ticket: '25700', cliente: 'SANTA ANITA', tema: 'REQUERIMIENTO-REBEFICS-SRI', fechaGeneracion: new Date('2025-02-17'), fechaVencimiento: null, tipo: 'TICKET', precioOferta: null, formaPago: null, descuento: 'NO', proximaActividad: 'COTIZAR', estado: 'PENDIENTE'}
    ];

    // Calcular clientes, tipos y formas de pago únicos
    var ofertas = $scope.ofertas;
    var hoyStr = hoy.toISOString().substr(0,7);
    function getUnique(arr, key) {
        var set = new Set();
        arr.forEach(function(o) {
            if (o[key] && o[key].toString().trim() !== '') set.add(o[key]);
        });
        return Array.from(set);
    }
    var clientes = getUnique(ofertas, 'cliente');
    var tipos = getUnique(ofertas, 'tipo');
    var formasPago = getUnique(ofertas, 'formaPago');
    var actividades = getUnique(ofertas, 'proximaActividad');
    // KPIs corregidos para datos reales
    function parsePrecio(oferta) {
        // Puede venir como string, number o null
        if (oferta.precioOferta != null && oferta.precioOferta !== '') return Number(oferta.precioOferta) || 0;
        if (oferta.precio != null && oferta.precio !== '') return Number(oferta.precio) || 0;
        return 0;
    }
    var preciosValidos = ofertas.map(parsePrecio).filter(v => !isNaN(v));
    var montoTotal = preciosValidos.reduce((a,b) => a+b, 0);
    var montoPromedio = preciosValidos.length > 0 ? Math.round(montoTotal / preciosValidos.length) : 0;
    var mesActual = hoy.getMonth()+1;
    var anioActual = hoy.getFullYear();
    $scope.kpis = {
        total: ofertas.length,
        aceptadas: ofertas.filter(o => o.estado && o.estado.includes('ACEPTADA')).length,
        rechazadas: ofertas.filter(o => o.estado && o.estado.includes('RECHAZADA')).length,
        vencidas: ofertas.filter(o => o.estado && o.estado.includes('VENCIDA')).length,
        montoTotal: preciosValidos.reduce((a,b) => a+b, 0),
        montoPromedio: preciosValidos.length > 0 ? Math.round(preciosValidos.reduce((a,b) => a+b, 0) / preciosValidos.length) : 0,
        ofertasMes: ofertas.filter(o => {
            if (!o.fechaGeneracion) return false;
            var d = (o.fechaGeneracion instanceof Date) ? o.fechaGeneracion : new Date(o.fechaGeneracion);
            return d.getMonth()+1 === mesActual && d.getFullYear() === anioActual;
        }).length,
        conDescuento: ofertas.filter(o => (o.descuento||'').toString().trim().toUpperCase() === 'SI').length,
        proximasVencer: ofertas.filter(o => {
            if (!o.fechaVencimiento) return false;
            var d = (o.fechaVencimiento instanceof Date) ? o.fechaVencimiento : new Date(o.fechaVencimiento);
            var dias = (d - hoy)/(1000*60*60*24);
            return dias > 0 && dias < 7;
        }).length,
        porCliente: clientes.map(c => ({cliente: c, cantidad: ofertas.filter(o => o.cliente === c).length})),
        porTipo: tipos.map(t => ({tipo: t, cantidad: ofertas.filter(o => o.tipo === t).length})),
        porFormaPago: formasPago.map(f => ({forma: f, cantidad: ofertas.filter(o => o.formaPago === f).length})),
        porActividad: actividades.map(a => ({actividad: a, cantidad: ofertas.filter(o => o.proximaActividad === a).length}))
    };

    // Gráfico de estado de ofertas
    setTimeout(function() {
        // Gráfico de estados reales
        var estadosUnicos = getUnique(ofertas, 'estado');
        Highcharts.chart('grafico-ofertas-estado', {
            chart: { type: 'pie' },
            title: { text: 'Distribución por Estado' },
            series: [{
                name: 'Ofertas',
                colorByPoint: true,
                data: estadosUnicos.map(function(e) {
                    return { name: e, y: ofertas.filter(o => o.estado === e).length };
                })
            }]
        });
        // Gráfico de ofertas por cliente
        Highcharts.chart('grafico-ofertas-cliente', {
            chart: { type: 'column' },
            title: { text: 'Ofertas por Cliente' },
            xAxis: { categories: $scope.kpis.porCliente.map(x=>x.cliente) },
            yAxis: { min: 0, title: { text: 'Cantidad' } },
            series: [{ name: 'Ofertas', data: $scope.kpis.porCliente.map(x=>x.cantidad) }]
        });
        // Gráfico de ofertas por tipo
        Highcharts.chart('grafico-ofertas-tipo', {
            chart: { type: 'bar' },
            title: { text: 'Ofertas por Tipo' },
            xAxis: { categories: $scope.kpis.porTipo.map(x=>x.tipo) },
            yAxis: { min: 0, title: { text: 'Cantidad' } },
            series: [{ name: 'Ofertas', data: $scope.kpis.porTipo.map(x=>x.cantidad) }]
        });
        // Gráfico de ofertas por forma de pago
        Highcharts.chart('grafico-ofertas-pago', {
            chart: { type: 'column' },
            title: { text: 'Ofertas por Forma de Pago' },
            xAxis: { categories: $scope.kpis.porFormaPago.map(x=>x.forma) },
            yAxis: { min: 0, title: { text: 'Cantidad' } },
            series: [{ name: 'Ofertas', data: $scope.kpis.porFormaPago.map(x=>x.cantidad) }]
        });
        // Gráfico de próximas actividades
        Highcharts.chart('grafico-ofertas-actividad', {
            chart: { type: 'bar' },
            title: { text: 'Ofertas por Próxima Actividad' },
            xAxis: { categories: $scope.kpis.porActividad.map(x=>x.actividad) },
            yAxis: { min: 0, title: { text: 'Cantidad' } },
            series: [{ name: 'Ofertas', data: $scope.kpis.porActividad.map(x=>x.cantidad) }]
        });
    }, 200);

    // Gráfico de ofertas por mes
    setTimeout(function() {
        var meses = {};
        ofertas.forEach(function(oferta) {
            if (!oferta.fechaGeneracion) return;
            var d = (oferta.fechaGeneracion instanceof Date) ? oferta.fechaGeneracion : new Date(oferta.fechaGeneracion);
            var mes = d.getFullYear() + '-' + String(d.getMonth()+1).padStart(2,'0');
            if (!meses[mes]) meses[mes] = 0;
            meses[mes]++;
        });
        Highcharts.chart('grafico-ofertas-mes', {
            chart: { type: 'column' },
            title: { text: 'Ofertas Generadas por Mes' },
            xAxis: { categories: Object.keys(meses) },
            yAxis: { min: 0, title: { text: 'Cantidad' } },
            series: [{ name: 'Ofertas', data: Object.values(meses) }]
        });
    }, 200);

}]);
