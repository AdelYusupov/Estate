using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Estate.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;

namespace Estate;

public partial class WindowClients : Window
{
    private Client selectedClient;

    public WindowClients()
    {
        InitializeComponent();
        LoadClients();
        SearchTextBox.TextChanged += SearchTextBox_TextChanged; // Подписка на событие
        ClientsListBox.SelectionChanged += ClientsListBox_SelectionChanged;
    }

    private void LoadClients()
    {
        using var context = new RealEstateContext();
        List<string> clients = context.Clients
            .Select(c => $"{c.LastName} {c.FirstName} {c.MiddleName}")
            .ToList();
        ClientsListBox.ItemsSource = clients;
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = SearchTextBox?.Text?.Trim() ?? string.Empty;
        using var context = new RealEstateContext();
        if (string.IsNullOrWhiteSpace(searchText))
        {
            LoadClients();
            return;
        }

        var searchParts = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var clients = context.Clients
            .AsEnumerable()
            .Where(c =>
                (!string.IsNullOrEmpty(c.Phone) && c.Phone.Contains(searchText)) ||
                (!string.IsNullOrEmpty(c.Email) && c.Email.Contains(searchText)) ||
                searchParts.Any(part =>
                    LevenshteinDistance.IsMatch(c.LastName, part, 3) ||
                    LevenshteinDistance.IsMatch(c.FirstName, part, 3) ||
                    (!string.IsNullOrEmpty(c.MiddleName) && LevenshteinDistance.IsMatch(c.MiddleName, part, 3))))
            .Select(c => $"{c.LastName} {c.FirstName} {c.MiddleName}")
            .ToList();

        ClientsListBox.ItemsSource = clients;
    }

    private void ClientsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedClientName = ClientsListBox.SelectedItem as string;
        if (selectedClientName != null)
        {
            var nameParts = selectedClientName.Split(' ');
            if (nameParts.Length >= 2)
            {
                LastNameTextBox.Text = nameParts[0];
                FirstNameTextBox.Text = nameParts[1];
                MiddleNameTextBox.Text = nameParts.Length > 2 ? nameParts[2] : "";
            }

            using var context = new RealEstateContext();
            selectedClient = context.Clients
                .FirstOrDefault(c =>
                    c.LastName == nameParts[0] &&
                    c.FirstName == nameParts[1] &&
                    (nameParts.Length <= 2 || c.MiddleName == nameParts[2]));

            if (selectedClient != null)
            {
                PhoneTextBox.Text = selectedClient.Phone;
                EmailTextBox.Text = selectedClient.Email;
            }
        }
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var firstName = FirstNameTextBox?.Text?.Trim() ?? "";
        var middleName = MiddleNameTextBox?.Text?.Trim() ?? "";
        var lastName = LastNameTextBox?.Text?.Trim() ?? "";
        var phone = PhoneTextBox?.Text?.Trim() ?? "";
        var email = EmailTextBox?.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Введите имя и фамилию клиента.");
            await box.ShowAsync();
            return;
        }

        if (!IsValidName(firstName))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Имя должно содержать только буквы кириллицы или латиницы без пробелов и дефисов.");
            await box.ShowAsync();
            return;
        }

        if (!IsValidName(lastName))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Фамилия должна содержать только буквы кириллицы или латиницы без пробелов и дефисов.");
            await box.ShowAsync();
            return;
        }

        if (!string.IsNullOrWhiteSpace(middleName) && !IsValidName(middleName))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Отчество должно содержать только буквы кириллицы или латиницы без пробелов и дефисов.");
            await box.ShowAsync();
            return;
        }

        if (string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(email))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Укажите хотя бы один контактный способ: телефон или email.");
            await box.ShowAsync();
            return;
        }

        if (!string.IsNullOrWhiteSpace(phone) && !IsValidPhoneNumber(phone))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Номер телефона должен содержать ровно 11 цифр.");
            await box.ShowAsync();
            return;
        }

        if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Укажите корректный email адрес.");
            await box.ShowAsync();
            return;
        }

        using var context = new RealEstateContext();
        bool clientExists = context.Clients.Any(c =>
            c.FirstName == firstName &&
            c.LastName == lastName &&
            c.MiddleName == middleName);

        if (clientExists)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Клиент с таким ФИО уже существует.");
            await box.ShowAsync();
            return;
        }

        var newClient = new Client
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            Phone = phone,
            Email = email
        };

        context.Clients.Add(newClient);
        context.SaveChanges();

        ClearFields();
        LoadClients();

        var successBox = MessageBoxManager.GetMessageBoxStandard("Успех", "Клиент успешно добавлен.");
        await successBox.ShowAsync();
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        var firstName = FirstNameTextBox?.Text?.Trim() ?? "";
        var middleName = MiddleNameTextBox?.Text?.Trim() ?? "";
        var lastName = LastNameTextBox?.Text?.Trim() ?? "";
        var phone = PhoneTextBox?.Text?.Trim() ?? "";
        var email = EmailTextBox?.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Введите имя и фамилию клиента.");
            await box.ShowAsync();
            return;
        }

        if (!IsValidName(firstName))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Имя должно содержать только буквы кириллицы или латиницы без пробелов и дефисов.");
            await box.ShowAsync();
            return;
        }

        if (!IsValidName(lastName))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Фамилия должна содержать только буквы кириллицы или латиницы без пробелов и дефисов.");
            await box.ShowAsync();
            return;
        }

        if (!string.IsNullOrWhiteSpace(middleName) && !IsValidName(middleName))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Отчество должно содержать только буквы кириллицы или латиницы без пробелов и дефисов.");
            await box.ShowAsync();
            return;
        }

        if (string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(email))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Укажите хотя бы один контактный способ: телефон или email.");
            await box.ShowAsync();
            return;
        }

        if (!string.IsNullOrWhiteSpace(phone) && !IsValidPhoneNumber(phone))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Номер телефона должен содержать ровно 11 цифр.");
            await box.ShowAsync();
            return;
        }

        if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Укажите корректный email адрес.");
            await box.ShowAsync();
            return;
        }

        if (selectedClient == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Выберите клиента для редактирования.");
            await box.ShowAsync();
            return;
        }

        using var context = new RealEstateContext();
        var clientToUpdate = context.Clients.FirstOrDefault(c => c.Client_ID == selectedClient.Client_ID);
        if (clientToUpdate != null)
        {
            clientToUpdate.FirstName = firstName;
            clientToUpdate.MiddleName = middleName;
            clientToUpdate.LastName = lastName;
            clientToUpdate.Phone = phone;
            clientToUpdate.Email = email;

            context.SaveChanges();

            ClearFields();
            LoadClients();

            var successBox = MessageBoxManager.GetMessageBoxStandard("Успех", "Данные клиента успешно обновлены.");
            await successBox.ShowAsync();
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (selectedClient == null)
            {
                var errorBox = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Выберите клиента для удаления.");
                await errorBox.ShowAsync();
                return;
            }

            var confirmBox = MessageBoxManager.GetMessageBoxStandard("Подтверждение удаления",
                "Вы уверены, что хотите удалить этого клиента?",
                ButtonEnum.YesNo);
            var result = await confirmBox.ShowAsync();
            if (result != ButtonResult.Yes) return;

            using var context = new RealEstateContext();
            bool hasSupplies = context.Supplies.Any(s => s.Client_ID == selectedClient.Client_ID);
            bool hasDemands = context.Demands.Any(d => d.Client_ID == selectedClient.Client_ID);

            if (hasSupplies || hasDemands)
            {
                var usedBox = MessageBoxManager.GetMessageBoxStandard("Ошибка",
                    "Невозможно удалить клиента, так как у него есть связанные предложения или потребности.");
                await usedBox.ShowAsync();
                return;
            }

            var clientToDelete = context.Clients.FirstOrDefault(c => c.Client_ID == selectedClient.Client_ID);
            if (clientToDelete != null)
            {
                context.Clients.Remove(clientToDelete);
                context.SaveChanges();
                ClearFields();
                LoadClients();

                var successBox = MessageBoxManager.GetMessageBoxStandard("Успех", "Клиент успешно удален!");
                await successBox.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            var exceptionBox = MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Ошибка при удалении клиента: {ex.Message}");
            await exceptionBox.ShowAsync();
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = new MainWindow();
        mainWindow.Show();
        this.Close();
    }

    private void TypesButton_Click(object sender, RoutedEventArgs e)
    {
        var typesWindow = new WindowTypes();
        typesWindow.Show();
        this.Close();
    }

    private void AgentsButton_Click(object sender, RoutedEventArgs e)
    {
        var agentsWindow = new WindowAgents();
        agentsWindow.Show();
        this.Close();
    }

    private void RealEstatesButton_Click(object sender, RoutedEventArgs e)
    {
        var estates = new WindowRealEstate();
        estates.Show();
        this.Close();
    }

    private void SuppliesButton_Click(object sender, RoutedEventArgs e)
    {
        var supply = new WindowSupplies();
        supply.Show();
        this.Close();
    }

    private void DemandsButton_Click(object sender, RoutedEventArgs e)
    {
        var demand = new WindowDemands();
        demand.Show();
        this.Close();
    }

    private void DealsButton_Click(object sender, RoutedEventArgs e)
    {
        var deals = new WindowDeals();
        deals.Show();
        this.Close();
    }

    private void ClearFields()
    {
        FirstNameTextBox.Clear();
        MiddleNameTextBox.Clear();
        LastNameTextBox.Clear();
        PhoneTextBox.Clear();
        EmailTextBox.Clear();
        selectedClient = null;
    }

    private bool IsValidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // Регулярное выражение: только буквы кириллицы или латиницы, **без пробелов и дефисов**
        return Regex.IsMatch(name, @"^[а-яА-ЯёЁa-zA-Z]+$");
    }

    private bool IsValidPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return true;
        var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
        return digitsOnly.Length == 11;
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return true;
        int atIndex = email.IndexOf('@');
        int dotIndex = email.LastIndexOf('.');
        return atIndex > 0 &&
               dotIndex > atIndex + 1 &&
               dotIndex < email.Length - 1;
    }
}