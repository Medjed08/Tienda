using MercadoPago.Client.Payment;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;

namespace Asgard_Store.Services
{
    public class PagoService
    {
        private readonly string _accessToken;
        private readonly IConfiguration _configuration;

        public PagoService(IConfiguration configuration)
        {
            _configuration = configuration;
            _accessToken = configuration["MercadoPago:AccessToken"];
            MercadoPagoConfig.AccessToken = _accessToken;
        }

        // Procesa un pago con tarjeta de Mercado Pago
     
        public async Task<ResultadoPago> ProcesarPagoTarjeta(DatosPagoRequest datos)
        {
            try
            {
                var request = new PaymentCreateRequest
                {
                    // Token de la tarjeta 
                    Token = datos.TokenTarjeta,

                    // Información del pago
                    TransactionAmount = datos.Monto,
                    Description = datos.Descripcion,

                    // Número de cuotas
                    Installments = datos.Cuotas,

                    // Método de pago 
                    PaymentMethodId = datos.MetodoPago,

                    // Información del pagador
                    Payer = new PaymentPayerRequest
                    {
                        Email = datos.EmailCliente,
                        FirstName = datos.NombreCliente,
                        LastName = datos.ApellidoCliente,

                        Identification = new MercadoPago.Client.Common.IdentificationRequest
                        {
                            Type = "CI",  // Cédula de Identidad
                            Number = datos.DocumentoCliente
                        }
                    },

                    // Referencia externa (tu ID de pedido)
                    ExternalReference = datos.PedidoId.ToString(),

                    // Statement descriptor (aparece en resumen tarjeta)
                    StatementDescriptor = "ASGARD STORE",

                    // Metadatos adicionales
                    Metadata = new Dictionary<string, object>
                    {
                        { "pedido_id", datos.PedidoId },
                        { "tienda", "Asgard Store" }
                    },

                    // Notificación (webhook)
                  //  NotificationUrl = $"{_configuration["AppSettings:BaseUrl"]}/api/mercadopago/notificacion"
                };

                // Crear el pago
                var client = new PaymentClient();
                Payment payment = await client.CreateAsync(request);

                // Mapear resultado
                var resultado = new ResultadoPago
                {
                    Exitoso = payment.Status == "approved",
                    PagoId = payment.Id.ToString(),
                    Estado = payment.Status,
                    EstadoDetalle = payment.StatusDetail,
                    MensajeError = payment.Status == "rejected" ? ObtenerMensajeRechazo(payment.StatusDetail) : null,
                    MontoTotal = payment.TransactionAmount ?? 0,
                    FechaPago = payment.DateApproved ?? DateTime.Now
                };

                return resultado;
            }
            catch (Exception ex)
            {
                return new ResultadoPago
                {
                    Exitoso = false,
                    MensajeError = $"Error al procesar el pago: {ex.Message}"
                };
            }
        }

        // Obtiene mensaje de rechazo de Mercado Pago
        private string ObtenerMensajeRechazo(string statusDetail)
        {
            return statusDetail switch
            {
                "cc_rejected_insufficient_amount" => "Fondos insuficientes en la tarjeta.",
                "cc_rejected_bad_filled_card_number" => "Número de tarjeta incorrecto.",
                "cc_rejected_bad_filled_date" => "Fecha de vencimiento incorrecta.",
                "cc_rejected_bad_filled_security_code" => "Código de seguridad incorrecto.",
                "cc_rejected_bad_filled_other" => "Revisa los datos de tu tarjeta.",
                "cc_rejected_call_for_authorize" => "Debes autorizar este pago con tu banco.",
                "cc_rejected_card_disabled" => "Tu tarjeta está deshabilitada.",
                "cc_rejected_duplicated_payment" => "Ya realizaste este pago.",
                "cc_rejected_high_risk" => "Pago rechazado por seguridad.",
                "cc_rejected_max_attempts" => "Superaste el límite de intentos.",
                "cc_rejected_blacklist" => "No pudimos procesar tu pago.",
                _ => "Pago rechazado. Por favor, intenta con otra tarjeta."
            };
        }

        // Obtiene información de un pago por su ID
        
        public async Task<Payment?> ObtenerPago(long pagoId)
        {
            try
            {
                var client = new PaymentClient();
                return await client.GetAsync(pagoId);
            }
            catch
            {
                return null;
            }
        }

        // Procesa reembolso de un pago
        
        public async Task<bool> ReembolsarPago(long pagoId)
        {
            try
            {
                var client = new PaymentClient();
                var payment = await client.GetAsync(pagoId);

                if (payment != null && payment.Status == "approved")
                {
                    // Crear reembolso
                    // Nota: Implementar RefundClient si necesitas reembolsos
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    // ===== MODELOS =====

    public class DatosPagoRequest
    {
        public string TokenTarjeta { get; set; } = string.Empty;           // Token generado por MP.js
        public decimal Monto { get; set; }                 // Monto total
        public string Descripcion { get; set; } = string.Empty;            // Descripción del pago
        public int Cuotas { get; set; } = 1;              // Número de cuotas
        public string MetodoPago { get; set; } = string.Empty;             // visa, master, etc.
        public string EmailCliente { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string ApellidoCliente { get; set; } = string.Empty;
        public string DocumentoCliente { get; set; } = string.Empty;       // Cédula
        public int PedidoId { get; set; }                  // ID del pedido
    }

    public class ResultadoPago
    {
        public bool Exitoso { get; set; }
        public string? PagoId { get; set; }
        public string? Estado { get; set; }                // approved, rejected, pending
        public string? EstadoDetalle { get; set; }
        public string? MensajeError { get; set; }
        public decimal MontoTotal { get; set; }
        public DateTime FechaPago { get; set; }
    }
}