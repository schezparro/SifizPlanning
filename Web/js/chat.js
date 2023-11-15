$(function () {
    var proxie = $.connection.websocket;
    $.connection.hub.start();

    var cantidadVentanas = 0;
    var idUsuarioChat = 0;
    var nombreUsuarioChat = 0;
    var usuariosChat = 0;
    var nMensajes = 30;

    ///--------------INICIO DE LAS NOTIFICACIONES-----------------------
    //Verificando las notificaciones en el navegador
    function GetWebNotificationsSupported() {
        return !!window.Notification;
    }

    //Para el tab activo
    var isActive;
    window.onfocus = function () {
        isActive = true;
    };
    window.onblur = function () {
        isActive = false;
    };

    var sePermiteNotificacion = GetWebNotificationsSupported();

    function showNotification(objNotification) {
        if (sePermiteNotificacion) {
            Notification.requestPermission().then(function (result) {
                if (result === 'denied') {
                    console.log('Permission wasn\'t granted. Allow a retry.');
                    return;
                }
                if (result === 'default') {
                    console.log('The permission request was dismissed.');
                    Notification.requestPermission();
                    return;
                }
                if (isActive === false) {
                    // Do something with the granted permission.
                    if (objNotification.notificar) {
                        var options = {
                            body: objNotification.text,
                            icon: "Web/images/n_logo.png"
                        }
                        var notif = new Notification(objNotification.title, options);
                        setTimeout(notif.close.bind(notif), 5000);
                        ejecutarSonido();
                    }
                }
            });
        }
    }
    showNotification({ notificar: false });

    ///---------------FIN DE LAS NOTIFICACIONES-----------------------

    //Actualizando los usuarios
    var request = $.ajax({
        url: "user/usuarios-activos-chat",
        method: "POST",
        data: {},
        dataType: "json"
    })
    .done(function (data) {
        if (data.success) {
            if (data.noColaborador === true) {//Para quitarle a los clientes
            }
            else {
                $('#sifiz-chat').show();
                usuariosChat = data.usuarios;
                $.each(data.usuarios, function (key, person) {
                    var divNewUserChat = $($('#div-usuario-chat').html());
                    $(divNewUserChat).attr({ 'data-id': person.id });
                    $(divNewUserChat).find('.nombre-usuario').html(person.nombre);
                    $('#panel-usuarios-sfz').append(divNewUserChat);
                });

                idUsuarioChat = data.idUsuario;
                nombreUsuarioChat = data.nombreUsuario;
                $('.nombre-usuario-local').html(nombreUsuarioChat + ' (alfa)');

                buscarmensajes();
            }
        }
        else {
            alert("La configuración del chat ha fallado, error al cargar los usuarios.");
        }        
    })
    .fail(function (jqXHR, textStatus) {
        alert("La configuración del chat ha fallado.");
    });

    //Funciones utiles
    function darFechaDate(fechaDate) {
        var re = /-?\d+/;
        var m = re.exec(fechaDate);
        var d = new Date(parseInt(m[0]));
        var dia = d.getDate();
        if (dia < 10)
            dia = "0" + dia;
        var mes = d.getMonth() + 1;
        if (mes < 10)
            mes = "0" + mes;
        var hora = d.getHours();
        if (hora < 10)
            hora = "0" + hora;
        var minutos = d.getMinutes();
        if (minutos < 10)
            minutos = "0" + minutos;
        var segundos = d.getSeconds();
        if (segundos < 10)
            segundos = "0" + segundos;
        return dia + '/' + mes + '/' + d.getFullYear() + " " + hora + ":" + minutos + ":" + segundos;
    }

    //FUNCIONES DEL SISTEMA
    function adicionarVentanaChat(idPersona, nombrePersona) {
        $('.ventana-chat').removeClass('resaltar');

        var selectorInstanciaChat = '.ventana-chat textarea[data-id-persona="' + idPersona + '"]';        
        var chatSeleccionado = $(selectorInstanciaChat);
        if (chatSeleccionado.length > 0) {
            $(chatSeleccionado[0]).parents('.ventana-chat').addClass('resaltar');
            ventanaChat = $(chatSeleccionado[0]).parents('.ventana-chat');
            return ventanaChat[0];
        }

        cantidadVentanas++;
        var numeroLeft = 250 + cantidadVentanas * 350;
        var ventana = $('#ventana-chat-conversacion').html();
        var textoCssPosicion = "calc( 99% - " + numeroLeft + "px )";
                
        $('#ventanas-activas-chat').append(ventana);
        var ventanaChat = $('#ventanas-activas-chat .ventana-chat').last();
        $(ventanaChat).css({ left: textoCssPosicion });
        $(ventanaChat).find('.nombre-contacto').html(nombrePersona);
        $(ventanaChat).find('textarea').attr({ 'data-id-persona': idPersona });
        $(ventanaChat).find('textarea').attr({ 'data-id-msg': 0 });
        $(ventanaChat).find('textarea').focus();
        var divTexto = $(ventanaChat).find('.chat-text-ventana-chat');
        listenForScrollEvent($(divTexto));

        //Adicionar los ultimos N mensajes
        var idAnteriorA = $(ventanaChat).find('textarea').attr('data-id-msg');
        insertarNmensajes(ventanaChat, idPersona, idAnteriorA, true );
        
        return false;
    };

    var cargandoNMensajes = false;
    function insertarNmensajes(ventanaChat, idUsuario, idAnteriorA, ultimo) {
        if (cargandoNMensajes === true)
            return false;

        cargandoNMensajes = true;

        if( idAnteriorA == undefined )
            idAnteriorA = 0

        var request = $.ajax({
            url: "user/dar-nmensaje-chat",
            method: "POST",
            data: {
                idUsuario: idUsuario,
                cantidad: nMensajes,
                idAnterior: idAnteriorA,
                ultimo: ultimo
            },
            dataType: "json"
        })
        .done(function (data) {
            if (data.success) {
                var mensajes = data.mensajes;
                var divTexto = $(ventanaChat).find('.chat-text-ventana-chat');
                //listenForScrollEvent($(divTexto));
                var textArea = $(ventanaChat).find('textarea');
                var divAux = $('<div/>');                                
                $.each(mensajes, function (key, value) {
                    var nombre = value.nombre;
                    if (nombre === 'yo') {
                        var divMyMsg = $('<div/>');
                        $(divMyMsg).addClass('myChatText');
                        var divMyMsgHour = $('<div/>');
                        var divMyMsgText = $('<div/>');
                        $(divMyMsgText).html(value.texto);
                        $(divMyMsgHour).html('<div class="left">' + value.nombre + ':</div> <div class="right">' + darFechaDate(value.fecha) + '</div><div class="clear"></div>');
                        $(divMyMsgHour).addClass('texto-cabecera-chat');
                        $(divMyMsg).append(divMyMsgHour);
                        $(divMyMsg).append(divMyMsgText);
                        $(divAux).append(divMyMsg);                        
                    }
                    else {
                        var divMyMsg = $('<div/>');
                        $(divMyMsg).addClass('toMeChatText');
                        var divMyMsgHour = $('<div/>');
                        var divMyMsgText = $('<div/>');
                        $(divMyMsgText).html(value.texto);
                        $(divMyMsgHour).html('<div class="left">' + value.nombre + ':</div> <div class="right">' + darFechaDate(value.fecha) + '</div><div class="clear"></div>');
                        $(divMyMsgHour).addClass('texto-cabecera-chat');
                        $(divMyMsg).append(divMyMsgHour);
                        $(divMyMsg).append(divMyMsgText);
                        $(divAux).append(divMyMsg);                        
                    }
                });

                $(divAux).find(' > div').css({ 'display' : 'none' });
                var alturaInicial = $(divTexto).prop('scrollHeight');
                var textHtml = $(divAux).html() + $(divTexto).html();
                $(divTexto).html(textHtml);

                var newChatHistory = $(divTexto).find(' > div:hidden');
                
                $(newChatHistory).show(1000, 'swing', function () {
                    var alturaFinal = $(divTexto).prop('scrollHeight');
                    var altura = alturaFinal - alturaInicial;
                    if (ultimo === true) {
                        $(divTexto).animate({ 'scrollTop': 10000000 }, 0);
                    }
                    else {
                        $(divTexto).animate({ 'scrollTop': altura }, 0);
                    }
                });

                /*
                var alturaFinal = $(divTexto).prop('scrollHeight');
                var altura = alturaFinal - alturaInicial;
                if (ultimo === true) {
                    $(divTexto).animate({ 'scrollTop': 10000000 }, 0);
                }
                else {
                    $(divTexto).animate({ 'scrollTop': altura }, 1000);
                }*/

                //enviando la notificación
                if (mensajes.length > 0) {
                    var texto = mensajes[0].texto;
                    notificacion = mensajes[0].nombre + ": " + texto.substring(0, 35) + "...";
                    showNotification(
                        {
                            notificar: true,
                            title: "Nuevo mensaje en Sifizplanning",
                            text: notificacion
                        }
                    );

                    //var cant = mensajes.length;
                    var idMsg = mensajes[0].id;
                    $(textArea).attr({ 'data-id-msg': idMsg });
                }
            }
            else {
                alert("Error en la obtensión de los ficheros.");
            }
        })
        .fail(function (jqXHR, textStatus) {
            alert("La actualización del chat ha fallado.");
        });

        cargandoNMensajes = false;
    };

    function buscarmensajes() {
        var request = $.ajax({
            url: "user/dar-mensaje-chat",
            method: "POST",
            data: {},
            dataType: "json"
        })
        .done(function (data) {
            if (data.success) {
                var mensajes = data.mensajes;
                $.each(mensajes, function (key, value) {
                    var msgs = value.mensajes;
                    var ventanaChat = adicionarVentanaChat(value.id, value.nombre);
                    if (ventanaChat !== false) {                                                
                        $.each(msgs, function (k, val) {
                            var divTexto = $(ventanaChat).find('.chat-text-ventana-chat');
                            var divMyMsg = $('<div/>');
                            $(divMyMsg).addClass('toMeChatText');
                            var divMyMsgHour = $('<div/>');
                            var divMyMsgText = $('<div/>');
                            $(divMyMsgText).html(val.texto);
                            $(divMyMsgHour).html('<div class="left">' + value.nombre + ':</div> <div class="right">' + darFechaDate(val.fecha) + '</div><div class="clear"></div>');
                            $(divMyMsgHour).addClass('texto-cabecera-chat');
                            $(divMyMsg).append(divMyMsgHour);
                            $(divMyMsg).append(divMyMsgText);
                            $(divTexto).append(divMyMsg);
                            $(divTexto).animate({ 'scrollTop': 10000000 }, 0);
                        });

                        //enviando la notificación
                        var texto = msgs[0].texto;
                        notificacion = value.nombre + ": " + texto.substring(0, 35) + "...";
                        showNotification(
                            {
                                notificar: true,
                                title: "Nuevo mensaje en Sifizplanning",
                                text: notificacion
                            }
                        );
                    }
                });
            }
            else {
                alert("Error en la obtensión de los ficheros.");
            }
        })
        .fail(function (jqXHR, textStatus) {
            alert("La actualización del chat ha fallado.");
        });
    }

    //Minimizar Maximizar Chat
    var minimizarChat = function () {
        var t = $('#head-sifiz-chat .glyphicon-chevron-down')
        $("#body-sifiz-chat").hide();
        $("#sifiz-chat").css({ 'top': 'calc( 100% - 28px )' });
        $(t).removeClass('glyphicon-chevron-down');
        $(t).addClass('glyphicon-chevron-up');
    };
    $(document).on('click', '#head-sifiz-chat .glyphicon-chevron-down', function () {
        minimizarChat();
    });    
    $(document).on('click', '#head-sifiz-chat .glyphicon-chevron-up', function () {
        $("#body-sifiz-chat").show();
        $("#sifiz-chat").css({ 'top': 'calc( 100% - 400px )' });
        $(this).removeClass('glyphicon-chevron-up');
        $(this).addClass('glyphicon-chevron-down');
    });
    minimizarChat();

    //Click sobre usuario para conversación
    $(document).on('click', '.usuario-chat', function () {
        var idPersona = $(this).attr('data-id');
        var nombrePersona = $(this).find('.nombre-usuario').html();
        adicionarVentanaChat( idPersona, nombrePersona );
    });

    //Cerrar la ventana
    $(document).on('click', '.ventana-chat .glyphicon-remove', function () {
        var ventana = $(this).parent().parent().parent();
        $(ventana).remove();
        cantidadVentanas--;
        
        var ventanasChat = $('#ventanas-activas-chat .ventana-chat');
        var i = 1;
        $.each(ventanasChat, function ( key, value ) {
            var numeroLeft = 250 + i * 350;            
            var textoCssPosicion = "calc( 99% - " + numeroLeft + "px )";
            $(value).css({ left: textoCssPosicion });
            i++;
        });

    });

    //Enter en el envío de mensaje
    $(document).on('keyup', '.ventana-chat textarea', function (e) {
        if (e.which == 13) {
            $(this).val('');
        }
    })
    $(document).on('keydown', '.ventana-chat textarea', function (e) {
        if (e.which == 13) {
            var obj = this;
            var texto = $(this).val();
            $(this).val("");
            //Actualizando los usuarios
            var request = $.ajax({
                url: "user/enviar-mensaje-chat",
                method: "POST",
                data: {
                    idUsuarioRecive: $(this).attr('data-id-persona'),
                    texto: texto
                },
                dataType: "json"                
            })
            .done(function (data) {
                if (data.success) {

                    var divTexto = $(obj).parent().siblings('.chat-text-ventana-chat');
                    var divMyMsg = $('<div/>');
                    $(divMyMsg).addClass('myChatText');
                    var divMyMsgHour = $('<div/>');
                    var divMyMsgText = $('<div/>');
                    $(divMyMsgText).html(texto);
                    $(divMyMsgHour).html('<div class="left">yo:</div> <div class="right">'+ data.fecha +'</div><div class="clear"></div>');
                    $(divMyMsgHour).addClass('texto-cabecera-chat');
                    $(divMyMsg).append(divMyMsgHour);
                    $(divMyMsg).append(divMyMsgText);

                    $(divTexto).append(divMyMsg);
                    $(divTexto).animate({ 'scrollTop': 10000000 }, 0);
                }
                else {
                    alert("Error enviando mensaje.");
                }
            })
            .fail(function (jqXHR, textStatus) {
                alert("Error en la comunicación con el servidor.");
            });
        }
    });

    //Llamada desde el servidor    
    proxie.client.existenMensajes = function( id ){
        if (id === idUsuarioChat) {//Existen nuevos mensajes para el usuario
            buscarmensajes();
        }
    };    

    //Filtro de chat
    $(document).on('keyup', '#filtro-chat', function (e) {
        var valor = $(this).val().toLowerCase();
        var usuarios = [];
        $.each(usuariosChat, function (key, usuario) {
            var usuarioLow = usuario.nombre.toLowerCase();
            if (usuarioLow.indexOf(valor) != -1) {                
                usuarios.push(usuario);
            }
        });
        
        $('#panel-usuarios-sfz').html('');

        $.each(usuarios, function (key, person) {
            var divNewUserChat = $($('#div-usuario-chat').html());
            $(divNewUserChat).attr({ 'data-id': person.id });
            $(divNewUserChat).find('.nombre-usuario').html(person.nombre);
            $('#panel-usuarios-sfz').append(divNewUserChat);
        });
    });

    function ejecutarSonido() {
        var sonido = document.getElementById('audio-chat');
        sonido.play();
    }

    //Scroll en los Chat
    $(document).on('custom-scroll', '.chat-text-ventana-chat', function () {
        var pos = $(this).scrollTop();
        if (pos === 0 && cargandoNMensajes === false) {
            var ventanaChat = $(this).parent();
            var textArea = $(ventanaChat).find('textarea').first();
            var idPersona = $(textArea).attr('data-id-persona');
            var idAnterior = $(textArea).attr('data-id-msg');
            insertarNmensajes(ventanaChat, idPersona, idAnterior, false);
        }
    });

    //Creación del evento custom-scroll
    function listenForScrollEvent(el) {
        el.on("scroll", function () {
            el.trigger("custom-scroll");
        })
    }
});