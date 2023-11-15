var waitingDialog = (function ($) {
    // Creating modal dialog's DOM
    var $dialog = $(
		'<div id="modal-waiting-dialog" class="modal fade" data-backdrop="static" data-keyboard="false" tabindex="-1" role="dialog" aria-hidden="true" style="padding-top:15%; overflow-y:visible;">' +
		'<div class="modal-dialog modal-m">' +
		'<div class="modal-content">' +
			'<div class="modal-header"><h4 style="margin:0;"></h4></div>' +
			'<div class="modal-body">' +
                '<div id="dato-msg-adicional"></div>'+
				'<div class="progress progress-striped active" style="margin-bottom:0;"><div class="progress-bar" style="width: 100%"></div></div>' +
			'</div>' +
		'</div></div></div>');

    return {
        /**
		 * Opens our dialog
		 * @param message Custom message
		 * @param options Custom options:
		 * 				  options.dialogSize - bootstrap postfix for dialog size, e.g. "sm", "m";
		 * 				  options.progressType - bootstrap postfix for progress bar type, e.g. "success", "warning".
		 */
        show: function (message, options) {
            // Assigning defaults
            var settings = $.extend({
                dialogSize: 'm',
                progressType: ''
            }, options);
            if (typeof message === 'undefined') {
                message = 'Loading';
            }
            if (typeof options === 'undefined') {
                options = {};
            }
            // Configuring dialog
            $dialog.find('.modal-dialog').attr('class', 'modal-dialog').addClass('modal-' + settings.dialogSize);
            $dialog.find('.progress-bar').attr('class', 'progress-bar');
            if (settings.progressType) {
                $dialog.find('.progress-bar').addClass('progress-bar-' + settings.progressType);
            }
            $dialog.find('h4').text(message);
            // Opening dialog
            $dialog.modal();
        },
        /**
		 * Closes dialog
		 */
        hide: function () {            
            $dialog.find('#dato-msg-adicional').text('');
            $dialog.modal('hide');

            setTimeout(function () {
                $('#modal-waiting-dialog').hide();
            }, 200);            
        },
        //Estableciendo un texto
        setText: function (text) {
            $dialog.find('#dato-msg-adicional').text(text);
        }
    }

})(jQuery);

var messageDialog = (function ($) {
    // Creating modal dialog's DOM
    var $dialog = $(
		'<div class="modal fade" data-backdrop="true" data-keyboard="true" tabindex="-1" role="dialog" aria-hidden="true" style="padding-top:15%; overflow-y:visible;">' +
		'<div class="modal-dialog modal-m">' +
		'<div class="modal-content">' +
			'<div class="modal-header">' +
            '<button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>' +
            '<h4 style="margin:0;"></h4></div>' +
			'<div class="modal-body">' +

			'</div>' +
            '<div class="modal-footer">' +
                '<button type="button" class="btn btn-default" data-dismiss="modal">Cerrar</button>' +
            '</div>' +
		'</div></div></div>');

    return {
        show: function (title,message) {            
            $dialog.find('h4').text(title);
            $dialog.find('.modal-body').text(message);
            // Opening dialog
            $dialog.modal();
        },
        hide: function () {
            $dialog.modal('hide');
        }
    }

})(jQuery);

var questionMsgDialog = function (){
    var obj = this;
    // Creating modal dialog's DOM
    var $dialog = $(
		'<div class="modal fade" data-backdrop="true" data-keyboard="true" tabindex="-1" role="dialog" aria-hidden="true" style="padding-top:15%; overflow-y:visible;">' +
		'<div class="modal-dialog modal-m">' +
		'<div class="modal-content">' +
			'<div class="modal-header">' +
            '<button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>' +
            '<h4 style="margin:0;"></h4></div>' +
			'<div class="modal-body">' +

			'</div>' +
            '<div class="modal-footer">'+
                '<button type="button" class="btn btn-default" data-dismiss="modal">Cerrar</button>'+
                '<button type="button" id="btn-dialogYes" class="btn btn-primary"></button>'+
            '</div>'+
		'</div></div></div>');

    this.show = function (title, message, textoYes, functionExecute) {
        $dialog.find('h4').text(title);
        $dialog.find('.modal-body').text(message);
        $dialog.find('#btn-dialogYes').text(textoYes);

        $dialog.on('click', '.close', function () {
            obj.hide();
        });
        $dialog.on('click', '.btn-default', function () {
            obj.hide();
        });

        $dialog.on('click', '#btn-dialogYes', functionExecute);
        // Opening dialog
        $dialog.modal();
    };
        
    this.hide = function () {
        $dialog.modal('hide');
        $(".modal-backdrop.fade.in").remove();
        $dialog.remove();
    };
};

var questionMsgDialog3Option = function(){
    var obj = this;
    // Creating modal dialog's DOM
    var $dialog = $(
		'<div class="modal fade" data-backdrop="true" data-keyboard="true" tabindex="-1" role="dialog" aria-hidden="true" style="padding-top:15%; overflow-y:visible;">' +
		'<div class="modal-dialog modal-m">' +
		'<div class="modal-content">' +
			'<div class="modal-header">' +
            '<button type="button" class="close" aria-label="Close"><span aria-hidden="true">&times;</span></button>' +
            '<h4 style="margin:0;"></h4></div>' +
			'<div class="modal-body">' +

			'</div>' +
            '<div class="modal-footer">' +
                '<button type="button" class="btn btn-default">Cerrar</button>' +
                '<button type="button" id="btn-dialogYes1" class="btn btn-primary"></button>' +
                '<button type="button" id="btn-dialogYes2" class="btn btn-success"></button>' +
            '</div>' +
		'</div></div></div>');

    this.show = function (title, message, textoYes1, textoYes2, functionExecute1, functionExecute2) {
        $dialog.find('h4').text(title);
        $dialog.find('.modal-body').text(message);
        $dialog.find('#btn-dialogYes1').text(textoYes1);
        $dialog.find('#btn-dialogYes2').text(textoYes2);

        $dialog.on('click', '.close', function () {                
            obj.hide();
        });
        $dialog.on('click', '.btn-default', function () {                
            obj.hide();
        });
        $dialog.on('click', '#btn-dialogYes1', functionExecute1);
        $dialog.on('click', '#btn-dialogYes2', functionExecute2);
        // Opening dialog
        $dialog.modal();            
    };

    this.hide = function () {
        $dialog.modal('hide');
        $(".modal-backdrop.fade.in").remove();
        $dialog.remove();
    };
};