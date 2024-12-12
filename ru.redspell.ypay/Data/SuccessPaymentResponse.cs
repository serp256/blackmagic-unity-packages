using YandexPay.Enums;

namespace YandexPay
{
    public struct SuccessPaymentResponse
    {
        public PaymentStatus Status;
        public string OrderId;
        public string ProductId;
    }
}