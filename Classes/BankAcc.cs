using System;
using System.Linq;

namespace MezinBank.Classes
{
    internal class BankAcc
    {
        public string BAccFullName { get; private set; }
        public string BAccPassport { get; private set; }
        public string BAccNumber { get; private set; }
        public DateTime BAccOpenDate { get; private set; }
        public DateTime BAccDateBirth { get; private set; }
        public double BAccBalance { get; private set; }
        public DateTime BAccEndDate { get; private set; }
        public AccountStatus BAccStatus { get; private set; }
        public enum AccountStatus { Открыт, Закрыт }

        public BankAcc(string bAccFullName, string bAccPassport, string bAccNumber, DateTime bAccOpenDate, DateTime bAccDateBirth, double bAccBalance, DateTime bAccEndDate, AccountStatus bAccStatus)
        {
            BAccFullName = bAccFullName;
            BAccPassport = bAccPassport;
            BAccNumber = bAccNumber;
            BAccOpenDate = bAccOpenDate;
            BAccDateBirth = bAccDateBirth;
            BAccBalance = bAccBalance;
            BAccEndDate = bAccEndDate;
            BAccStatus = AccountStatus.Открыт;
        }

        public bool DepositBalance(double amount)
        {
            BAccBalance += amount;
            return true;
        }

        public bool WithdrawBalance(double amount)
        {
            BAccBalance -= amount;
            return true;
        }

        public bool CloseBAcc()
        {
            BAccStatus = AccountStatus.Закрыт;
            return true;
        }

        public static string GenerateRandomBAccNumber()
        {
            Random random = new Random();
            return new string(Enumerable.Repeat("0123456789", 8).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string OutputAccInfo()
        {
            return $"ФИО: {BAccFullName}{Environment.NewLine}" +
                   $"Текущий баланс счета: {BAccBalance}{Environment.NewLine}" +
                   $"Паспорт: {BAccPassport}{Environment.NewLine}" +
                   $"Дата рождения: {BAccDateBirth:dd-MM-yyyy}{Environment.NewLine}" +
                   $"Дата открытия счета: {BAccOpenDate:dd-MM-yyyy}{Environment.NewLine}" +
                   $"Номер счета: {BAccNumber}{Environment.NewLine}" +
                   $"Статус счета: {BAccStatus}{Environment.NewLine}";
        }
    }
}
