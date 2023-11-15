var listTableHeaderFixed = [];

/**
	*	id: Id del elemento HTML
	*	columns arreglod de % del ancho de cada uno de los paneles 
	*	[25, 45, 60, 80, 90]
	*/
function TableHeaderFixed(id, columnas) {
	var obj = this;
	this.id = id;
	this.columnas = columnas;

	this.ajustar = function () {		
		var ancho = $("#" + this.id).width() - 20;

		var selectorCabeceras = "#" + this.id + " thead th";
		$.each($(selectorCabeceras), function (key, value) {
			$(value).css({
				width: ancho * obj.columnas[key] / 100
			});
		});

		var selectorColumnas = "#" + this.id + " tbody td";
		$.each($(selectorColumnas), function (key, value) {
			var pos = key % obj.columnas.length;
			$(value).css({
				width: ancho * obj.columnas[pos] / 100
			});
		});

		var selectorFooter = "#" + this.id + " tfoot th";
		$.each($(selectorFooter), function (key, value) {			
			$(value).css({
				width: ancho
			});
		});
	};
}

function tableHeaderFixedByID(id) {
	var cant = listTableHeaderFixed.length;
	var i = 0;
	while (i < cant) {
		if (listTableHeaderFixed[i].id == id) {
			return listTableHeaderFixed[i];
		}
		i++;
	}
	return null;
}

function ajustarTableHeaderFixedPorID(id) {
	setTimeout(function () {
		var tabla = tableHeaderFixedByID(id);
		if (tabla !== null) {
			tabla.ajustar();
		}
	}, 50);	
}

$(function () {

	//Buscando todas las table-header
	var tablasHF = $("table.header-fixed");
	$.each(tablasHF, function (key, tabla) {
		var headers = $(tabla).find("thead th");

		var arreglosWidthHeader = [];
		$.each(headers, function (key, th) {
			var wTh = $(th).attr("data-width");
			arreglosWidthHeader.push(wTh);
		});

		var id = $(tabla).attr("id");
		listTableHeaderFixed.push( new TableHeaderFixed( id, arreglosWidthHeader ) );
	});

	$.each(listTableHeaderFixed, function (key, value) {		
		value.ajustar();		
	});

	$(window).resize(function () {
		$.each(listTableHeaderFixed, function (key, value) {			
			value.ajustar();
		});
	});

});