using MezinBank.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using static MezinBank.Classes.BankAcc;

namespace MezinBank
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private List<BankAcc> _bankAccounts = new List<BankAcc>();

        private List<Transaction> _deposits = new List<Transaction>();
        private List<Transaction> _withdrawals = new List<Transaction>();
        private List<Transaction> _transfers = new List<Transaction>();

        private void OnOpenAccountButtonClick(object sender, RoutedEventArgs e)
        {
            string fullName = UserFullNameTextBox.Text;
            string passport = UserPassportTextBox.Text;
            DateTime? birthDate = UserBirthDatePicker.SelectedDate;

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(passport) ||
                birthDate == null)
            {
                ShowErrorMessage("Пожалуйста, заполните все поля");
                return;
            }

            if (fullName.Any(char.IsDigit))
            {
                ShowErrorMessage("Поле ФИО не должно содержать цифр");
                UserFullNameTextBox.Clear();
                return;
            }

            var passportRegex = new Regex(@"^\d{6}-\d{4}$");
            if (!passportRegex.IsMatch(passport))
            {
                ShowErrorMessage("Некорректный формат паспорта. Используйте формат: xxxxxx-xxxx");
                UserPassportTextBox.Clear();
                UserPassportTextBox.Focus();
                return;
            }

            var newBankAccount = new BankAcc(
                fullName,
                passport,
                BankAcc.GenerateRandomBAccNumber(),
                DateTime.Now,
                birthDate.Value,
                0,
                DateTime.Now.AddYears(10),
                AccountStatus.Открыт);

            _bankAccounts.Add(newBankAccount);
            UpdateUserSelectors();

            UserFullNameTextBox.Clear();
            UserPassportTextBox.Clear();
            UserBirthDatePicker.SelectedDate = null;

            ShowSuccessMessage("Счет успешно открыт!");
        }

        private void OnCloseAccountButtonClick(object sender, RoutedEventArgs e)
        {
            if (FirstUserSelector.SelectedItem == null)
            {
                ShowErrorMessage("Выберите пользователя!");
                return;
            }

            string selectedUserName = FirstUserSelector.SelectedItem.ToString();
            var selectedAccount = _bankAccounts.FirstOrDefault(acc => acc.BAccFullName == selectedUserName);

            if (selectedAccount == null)
            {
                ShowErrorMessage("Ошибка: Счет не найден!");
                return;
            }

            if (selectedAccount.BAccStatus == BankAcc.AccountStatus.Закрыт)
            {
                ShowErrorMessage("Счет уже закрыт.");
                return;
            }

            selectedAccount.CloseBAcc();

            FirstUserInfoTextBox.Text = $"Выбранный пользователь:{Environment.NewLine}{selectedAccount.OutputAccInfo()}";
            ShowSuccessMessage("Счет успешно закрыт.");
        }

        private void OnDepositButtonClick(object sender, RoutedEventArgs e)
        {
            if (FirstUserSelector.SelectedItem == null)
            {
                ShowErrorMessage("Сначала выберите пользователя!");
                return;
            }

            string selectedUserName = FirstUserSelector.SelectedItem.ToString();
            var selectedAccount = _bankAccounts.FirstOrDefault(acc => acc.BAccFullName == selectedUserName);

            if (selectedAccount == null)
            {
                ShowErrorMessage("Ошибка при поиске счета!");
                return;
            }

            if (!double.TryParse(TransactionAmountTextBox.Text, out double amount))
            {
                ShowErrorMessage("Введите корректную сумму!");
                return;
            }

            selectedAccount.DepositBalance(amount);

            Transaction newTransaction = new Transaction(
                selectedAccount.BAccNumber,
                DateTime.Now,
                amount,
                Transaction.OperationType.Пополнение);

            AddTransaction(newTransaction);

            FirstUserInfoTextBox.Text = $"Выбранный пользователь: {Environment.NewLine}{selectedAccount.OutputAccInfo()}";
            SecondUserInfoTextBox.Text = $"Выбранный пользователь: {Environment.NewLine}{selectedAccount.OutputAccInfo()}";

            ShowSuccessMessage("Операция выполнена успешно!");

            TransactionAmountTextBox.Clear();
        }

        private void OnWithdrawButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FirstUserSelector.SelectedItem == null)
                {
                    ShowErrorMessage("Сначала выберите пользователя и карту!");
                    return;
                }

                if (!double.TryParse(TransactionAmountTextBox.Text, out double amount) || amount <= 0)
                {
                    ShowErrorMessage("Введите корректную сумму!");
                    return;
                }

                string selectedUserName = FirstUserSelector.SelectedItem.ToString();
                var selectedAccount = _bankAccounts.FirstOrDefault(acc => acc.BAccFullName == selectedUserName);

                if (selectedAccount == null)
                {
                    ShowErrorMessage("Ошибка при поиске счета!");
                    return;
                }

                if (!selectedAccount.WithdrawBalance(amount))
                {
                    ShowErrorMessage("Недостаточно средств на счете!");
                    return;
                }

                Transaction newTransaction = new Transaction(
                    selectedAccount.BAccNumber,
                    DateTime.Now,
                    amount,
                    Transaction.OperationType.Снятие);

                AddTransaction(newTransaction);

                FirstUserInfoTextBox.Text = $"Выбранный пользователь: {Environment.NewLine}{selectedAccount.OutputAccInfo()}";
                SecondUserInfoTextBox.Text = $"Выбранный пользователь: {Environment.NewLine}{selectedAccount.OutputAccInfo()}";

                ShowSuccessMessage("Операция выполнена успешно!");
                TransactionAmountTextBox.Clear();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private void OnTransferButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FirstUserSelector.SelectedItem == null || SecondUserSelector.SelectedItem == null)
                {
                    ShowErrorMessage("Выберите обоих пользователей!");
                    return;
                }

                string senderName = FirstUserSelector.SelectedItem.ToString();
                string receiverName = SecondUserSelector.SelectedItem.ToString();

                if (senderName == receiverName)
                {
                    ShowErrorMessage("Нельзя переводить самому себе!");
                    return;
                }

                if (!double.TryParse(TransactionAmountTextBox.Text, out double amount) || amount <= 0)
                {
                    ShowErrorMessage("Введите корректную положительную сумму!");
                    return;
                }

                var senderAccount = _bankAccounts.FirstOrDefault(a => a.BAccFullName == senderName);
                var receiverAccount = _bankAccounts.FirstOrDefault(a => a.BAccFullName == receiverName);

                if (senderAccount == null || receiverAccount == null)
                {
                    ShowErrorMessage("Один из счетов не найден!");
                    return;
                }

                if (senderAccount.BAccStatus == BankAcc.AccountStatus.Закрыт ||
                    receiverAccount.BAccStatus == BankAcc.AccountStatus.Закрыт)
                {
                    ShowErrorMessage("Один из счетов закрыт!");
                    return;
                }

                if (!senderAccount.WithdrawBalance(amount))
                {
                    ShowErrorMessage("Недостаточно средств для перевода!");
                    return;
                }

                if (!receiverAccount.DepositBalance(amount))
                {
                    senderAccount.DepositBalance(amount);
                    ShowErrorMessage("Ошибка при зачислении средств!");
                    return;
                }

                var transaction = new Transaction(
                    senderAccount.BAccNumber,
                    DateTime.Now,
                    amount,
                    Transaction.OperationType.Перевод,
                    receiverAccount.BAccNumber,
                    senderName);

                _transfers.Add(transaction);

                FirstUserInfoTextBox.Text = $"Выбранный пользователь: {Environment.NewLine}{senderAccount.OutputAccInfo()}";
                SecondUserInfoTextBox.Text = $"Выбранный пользователь: {Environment.NewLine}{receiverAccount.OutputAccInfo()}";

                ShowSuccessMessage("Перевод выполнен успешно!");
                TransactionAmountTextBox.Clear();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка перевода: {ex.Message}");
            }
        }

        private void OnTransactionTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FirstUserSelector == null || FirstUserSelector.SelectedItem == null || TransactionTypeSelector == null || TransactionTypeSelector.SelectedItem == null)
            {
                return;
            }

            if (FirstUserSelector.SelectedItem is string selectedUserName)
            {
                var bankAccount = _bankAccounts.FirstOrDefault(acc => acc.BAccFullName == selectedUserName);
                if (bankAccount != null)
                {
                    var Transaction = GetFilteredTransaction(bankAccount);
                    UpdateTransactionOutput(bankAccount, TransactionLogTextBox, Transaction);
                }
            }
        }

        private List<Transaction> GetFilteredTransaction(BankAcc account)
        {
            if (TransactionTypeSelector == null || TransactionTypeSelector.SelectedItem == null)
            {
                return new List<Transaction>();
            }

            var selectedType = (TransactionTypeSelector.SelectedItem as ComboBoxItem)?.Content.ToString();

            var allTransaction = _deposits.Concat(_withdrawals).Concat(_transfers).ToList();

            allTransaction = allTransaction.Where(t => t.TrAccNumber == account.BAccNumber).ToList();

            if (string.IsNullOrEmpty(selectedType) || selectedType == "Все")
            {
                return allTransaction;
            }

            var filteredTransaction = allTransaction.Where(t =>
            {
                switch (selectedType)
                {
                    case "Пополнение":
                        return t.TrOperation == Transaction.OperationType.Пополнение;
                    case "Снятие":
                        return t.TrOperation == Transaction.OperationType.Снятие;
                    case "Перевод":
                        return t.TrOperation == Transaction.OperationType.Перевод;
                    default:
                        return false;
                }
            }).ToList();

            return filteredTransaction;
        }

        private void AddTransaction(Transaction transaction)
        {
            switch (transaction.TrOperation)
            {
                case Transaction.OperationType.Пополнение:
                    _deposits.Add(transaction);
                    break;
                case Transaction.OperationType.Снятие:
                    _withdrawals.Add(transaction);
                    break;
                case Transaction.OperationType.Перевод:
                    _transfers.Add(transaction);
                    break;
                default:
                    throw new InvalidOperationException("Неизвестный тип транзакции.");
            }

            var bankAccount = _bankAccounts.FirstOrDefault(acc => acc.BAccNumber == transaction.TrAccNumber);
            if (bankAccount != null)
            {
                UpdateTransactionOutput(bankAccount, TransactionLogTextBox, GetFilteredTransaction(bankAccount));
            }
        }

        private void UpdateTransactionOutput(BankAcc account, TextBox TransactionLogTextBox, List<Transaction> transactionList)
        {
            if (account == null || transactionList == null)
            {
                TransactionLogTextBox.Text = "Нет данных о транзакциях.";
                return;
            }

            if (transactionList.Count == 0)
            {
                TransactionLogTextBox.Text = "Нет совершенных транзакций.";
                return;
            }

            string output = "";

            foreach (var transaction in transactionList)
            {
                output += transaction.OutputTrInfo() + Environment.NewLine;

                if (transaction.TrOperation == Transaction.OperationType.Перевод)
                {
                    output += "Отправитель: " + transaction.TrSenderAccName + Environment.NewLine;
                    output += "Получатель: " + transaction.TrReceiverAccName + Environment.NewLine;
                }

                output += Environment.NewLine;
            }

            Dispatcher.Invoke(() =>
            {
                TransactionLogTextBox.Text = output;
            });
        }

        private void OnFirstUserSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectedUser(FirstUserSelector, FirstUserInfoTextBox);
        }

        private void OnSecondUserSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectedUser(SecondUserSelector, SecondUserInfoTextBox);
        }

        private void UpdateSelectedUser(ComboBox UserSelector, TextBox outputTextBox)
        {
            if (UserSelector?.SelectedItem is string selectedUserName)
            {
                var selectedUser = _bankAccounts.FirstOrDefault(user => user.BAccFullName == selectedUserName);

                if (selectedUser != null)
                {
                    outputTextBox.Text = "Выбранный пользователь:" + Environment.NewLine +
                                          selectedUser.OutputAccInfo();
                }
                else
                {
                    outputTextBox.Text = "Пользователь не найден.";
                }
            }
        }

        private void UpdateUserSelectors()
        {
            FirstUserSelector.Items.Clear();
            SecondUserSelector.Items.Clear();

            foreach (var user in _bankAccounts)
            {
                FirstUserSelector.Items.Add(user.BAccFullName);
                SecondUserSelector.Items.Add(user.BAccFullName);
            }

            if (_bankAccounts.Any())
            {
                FirstUserSelector.SelectedIndex = 0;
                SecondUserSelector.SelectedIndex = 0;
            }
        }

        private const string jsonFile = "UsersList.json";

        private void OnSaveJsonButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string json = JsonConvert.SerializeObject(_bankAccounts, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(jsonFile, json);
                ShowSuccessMessage("Данные успешно записаны в JSON!");
            }
            catch
            {
                ShowErrorMessage("Ошибка сохранения!");
            }
        }

        private void OnLoadJsonButtonClick(object sender, RoutedEventArgs e)
        {
            if (File.Exists(jsonFile))
            {
                string json = File.ReadAllText(jsonFile);
                _bankAccounts = JsonConvert.DeserializeObject<List<BankAcc>>(json);
                ShowSuccessMessage("Данные успешно считаны из JSON!");

                UpdateUserSelectors();
            }
            else
            {
                ShowErrorMessage("Ошибка при загрузке данных");
            }
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
