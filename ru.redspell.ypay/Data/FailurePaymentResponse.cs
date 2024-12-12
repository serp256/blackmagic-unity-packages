using YandexPay.Enums;

namespace YandexPay
{
    public struct FailurePaymentResponse
    {
        public PaymentStatus Status;
        public string ErrorMessage;
        public string ProductId;
    }
}