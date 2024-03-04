-- Añadir un nuevo campo temporal para almacenar los minutos
ALTER TABLE [TICKET].[RECURSOS]
ADD tiempo_minutos INT;

-- Convertir los valores de tiempo a minutos y almacenarlos en el nuevo campo
UPDATE [TICKET].[RECURSOS]
SET tiempo_minutos = DATEDIFF(MINUTE, '1900-01-01', CAST([TIEMPOCAPACITACION] AS DATETIME));

-- Eliminar el campo original
ALTER TABLE [TICKET].[RECURSOS]
DROP COLUMN [TIEMPOCAPACITACION];

-- Renombrar el nuevo campo al nombre original
EXEC sp_rename '[TICKET].[RECURSOS].[tiempo_minutos]', 'TIEMPOCAPACITACION', 'COLUMN';
