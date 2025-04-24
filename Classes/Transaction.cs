using System;
using System.Text;

namespace MezinBank.Classes
{
    internal class Transaction
    {
        public string TrAccNumber { get; set; }
        public DateTime TrTime { get; }
        public double TrAmount { get; }
        public string TrReceiverAccName { get; }
        public string TrSenderAccName { get; }
        public OperationType TrOperation { get; }
        public enum OperationType { Снятие, Пополнение, Перевод }

        public Transaction(string trAccNumber,DateTime trTime, double trAmount, OperationType trOperation, string trReceiverAccName = null, string trSenderAccName = null)
        {
            TrAccNumber = trAccNumber;
            TrTime = trTime;
            TrAmount = trAmount;
            TrOperation = trOperation;
            TrReceiverAccName = trReceiverAccName;
            TrSenderAccName = trSenderAccName;
        }

        public string OutputTrInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"[{TrTime:dd.MM.yyyy HH:mm:ss}] ");

            switch (TrOperation)
            {
                case OperationType.Пополнение:
                    sb.Append($"Пополнение на сумму: {TrAmount:F2}");
                    if (!string.IsNullOrEmpty(TrReceiverAccName))
                        sb.Append($" (на счет: {TrReceiverAccName})");
                    break;

                case OperationType.Снятие:
                    sb.Append($"Снятие на сумму: {TrAmount:F2}");
                    if (!string.IsNullOrEmpty(TrSenderAccName))
                        sb.Append($" (со счета: {TrSenderAccName})");
                    break;

                case OperationType.Перевод:
                    sb.Append($"Перевод суммы: {TrAmount:F2}");
                    if (!string.IsNullOrEmpty(TrSenderAccName) && !string.IsNullOrEmpty(TrReceiverAccName))
                        sb.Append($" (со счета: {TrSenderAccName} на счет: {TrReceiverAccName})");
                    break;
            }

            return sb.ToString();
        }
    }
}