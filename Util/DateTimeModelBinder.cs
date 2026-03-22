using System;
using System.Globalization;
using System.Web.Mvc;

namespace SifizPlanning.Util
{
    /// <summary>
    /// Model Binder personalizado para manejar fechas de manera consistente
    /// evitando problemas de zona horaria entre diferentes países
    /// </summary>
    public class DateTimeModelBinder : IModelBinder
    {
        public bool BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(DateTime) && bindingContext.ModelType != typeof(DateTime?))
            {
                return false;
            }

            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (value == null || string.IsNullOrEmpty(value.AttemptedValue))
            {
                return false;
            }

            var dateString = value.AttemptedValue;

            try
            {
                DateTime dateTime;
                
                // Intentar parsear como fecha ISO (YYYY-MM-DD)
                if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                {
                    bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);
                    return true;
                }
                
                // Intentar parsear como fecha ISO con hora (YYYY-MM-DDTHH:mm:ss)
                if (DateTime.TryParseExact(dateString, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                {
                    bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);
                    return true;
                }

                // Intentar parsear como fecha ISO con zona horaria
                if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTime))
                {
                    // Convertir a fecha local del servidor (Ecuador)
                    bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);
                    return true;
                }

                // Fallback: usar cultura del servidor
                if (DateTime.TryParse(dateString, CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime))
                {
                    bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LoggerManager.LogError(ex, $"Error al procesar fecha: {dateString}");
            }

            return false;
        }
    }
}
