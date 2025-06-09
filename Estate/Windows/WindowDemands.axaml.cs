using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Estate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Estate;

public partial class WindowDemands : Window
{
    private Demand selectedDemand;
    private List<Agent> _agents = new();
    private List<Client> _clients = new();
    private List<Demand> _demandList = new(); // Хранение загруженных данных

    public WindowDemands()
    {
        InitializeComponent();
        LoadData();
        LoadDemands();
        SearchTextBox.TextChanged += SearchTextBox_TextChanged; // Подписка на событие
    }

    private void LoadData()
    {
        using var context = new RealEstateContext();
        _agents = context.Agents.ToList();
        _clients = context.Clients.ToList();

        AgentComboBox.Items.Clear();
        foreach (var agent in _agents)
        {
            AgentComboBox.Items.Add($"{agent.LastName} {agent.FirstName}");
        }

        ClientComboBox.Items.Clear();
        foreach (var client in _clients)
        {
            ClientComboBox.Items.Add($"{client.LastName} {client.FirstName}");
        }
    }

    private void LoadDemands(string search = null)
    {
        using var context = new RealEstateContext();
        IQueryable<Demand> query = context.Demands
            .Include(d => d.Agent)
            .Include(d => d.Client);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d =>
                d.Address.Contains(search) ||
                (d.MinPrice.HasValue && d.MinPrice.ToString().Contains(search)) ||
                (d.MaxPrice.HasValue && d.MaxPrice.ToString().Contains(search)));
        }

        _demandList = query.ToList();
        DemandsListBox.Items.Clear();
        foreach (var demand in _demandList)
        {
            string agentName = $"{demand.Agent?.LastName ?? "—"} {demand.Agent?.FirstName ?? ""}";
            string clientName = $"{demand.Client?.LastName ?? "—"} {demand.Client?.FirstName ?? ""}";
            DemandsListBox.Items.Add($"Адрес: {demand.Address}, Клиент: {clientName}, Цена: {demand.MinPrice}-{demand.MaxPrice ?? 0}");
        }
    }

    private bool ValidateFields(out string errorMessage)
    {
        errorMessage = "";

        // Проверка обязательных полей
        if (AgentComboBox.SelectedItem == null)
            errorMessage = "Выберите риэлтора.";
        else if (ClientComboBox.SelectedItem == null)
            errorMessage = "Выберите клиента.";
        else if (string.IsNullOrWhiteSpace(ApartmentFilterTextBox.Text) &&
                 string.IsNullOrWhiteSpace(HouseFilterTextBox.Text) &&
                 string.IsNullOrWhiteSpace(LandFilterTextBox.Text))
            errorMessage = "Необходимо указать хотя бы один фильтр (квартира, дом или земля).";

        // Проверка цен, если они указаны
        if (string.IsNullOrEmpty(errorMessage))
        {
            if (!string.IsNullOrWhiteSpace(MinPriceTextBox.Text) &&
                (!int.TryParse(MinPriceTextBox.Text, out int minPrice) || minPrice <= 0))
                errorMessage = "Минимальная цена должна быть положительным целым числом или пустой.";

            if (!string.IsNullOrWhiteSpace(MaxPriceTextBox.Text) &&
                (!int.TryParse(MaxPriceTextBox.Text, out int maxPrice) || maxPrice <= 0))
                errorMessage = "Максимальная цена должна быть положительным целым числом или пустой.";

            if (string.IsNullOrEmpty(errorMessage) &&
                !string.IsNullOrWhiteSpace(MinPriceTextBox.Text) &&
                !string.IsNullOrWhiteSpace(MaxPriceTextBox.Text) &&
                int.TryParse(MinPriceTextBox.Text, out int min) &&
                int.TryParse(MaxPriceTextBox.Text, out int max) &&
                min > max)
                errorMessage = "Минимальная цена не может быть больше максимальной.";
        }

        // Проверка ID фильтров и их наличия в БД
        if (string.IsNullOrEmpty(errorMessage))
        {
            using var context = new RealEstateContext();

            if (!string.IsNullOrWhiteSpace(ApartmentFilterTextBox.Text))
            {
                if (!int.TryParse(ApartmentFilterTextBox.Text, out int aptId) || aptId <= 0)
                    errorMessage = "ID фильтра квартир должен быть положительным целым числом или пустым.";
                else if (!context.ApartmentFilters.Any(a => a.ApartmentFilter_ID == aptId))
                    errorMessage = $"Фильтр квартир с ID {aptId} не найден в базе данных.";
            }

            if (!string.IsNullOrWhiteSpace(HouseFilterTextBox.Text))
            {
                if (!int.TryParse(HouseFilterTextBox.Text, out int hseId) || hseId <= 0)
                    errorMessage = "ID фильтра домов должен быть положительным целым числом или пустым.";
                else if (!context.HouseFilters.Any(h => h.HouseFilter_ID == hseId))
                    errorMessage = $"Фильтр домов с ID {hseId} не найден в базе данных.";
            }

            if (!string.IsNullOrWhiteSpace(LandFilterTextBox.Text))
            {
                if (!int.TryParse(LandFilterTextBox.Text, out int lndId) || lndId <= 0)
                    errorMessage = "ID фильтра участков должен быть положительным целым числом или пустым.";
                else if (!context.LandFilters.Any(l => l.LandFilter_ID == lndId))
                    errorMessage = $"Фильтр участков с ID {lndId} не найден в базе данных.";
            }
        }

        return string.IsNullOrEmpty(errorMessage);
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateFields(out string errorMessage))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", errorMessage);
            await box.ShowAsync();
            return;
        }

        int? apartmentFilterId = null;
        if (!string.IsNullOrWhiteSpace(ApartmentFilterTextBox.Text) &&
            int.TryParse(ApartmentFilterTextBox.Text, out int aptId))
        {
            apartmentFilterId = aptId;
        }

        int? houseFilterId = null;
        if (!string.IsNullOrWhiteSpace(HouseFilterTextBox.Text) &&
            int.TryParse(HouseFilterTextBox.Text, out int hseId))
        {
            houseFilterId = hseId;
        }

        int? landFilterId = null;
        if (!string.IsNullOrWhiteSpace(LandFilterTextBox.Text) &&
            int.TryParse(LandFilterTextBox.Text, out int lndId))
        {
            landFilterId = lndId;
        }

        var agent = _agents[AgentComboBox.SelectedIndex];
        var client = _clients[ClientComboBox.SelectedIndex];

        using var context = new RealEstateContext();
        var demand = new Demand
        {
            Address = AddressTextBox.Text?.Trim(),
            MinPrice = int.TryParse(MinPriceTextBox.Text, out int min) ? min : (int?)null,
            MaxPrice = int.TryParse(MaxPriceTextBox.Text, out int max) ? max : (int?)null,
            Agent_ID = agent.Agent_ID,
            Client_ID = client.Client_ID,
            ApartmentFilter_ID = apartmentFilterId,
            HouseFilter_ID = houseFilterId,
            LandFilter_ID = landFilterId
        };

        context.Demands.Add(demand);
        context.SaveChanges();

        ClearFields();
        LoadDemands();

        var successBox = MessageBoxManager.GetMessageBoxStandard("Успех", "Потребность успешно добавлена.");
        await successBox.ShowAsync();
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (selectedDemand == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Выберите потребность для редактирования.");
            await box.ShowAsync();
            return;
        }

        if (!ValidateFields(out string errorMessage))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", errorMessage);
            await box.ShowAsync();
            return;
        }

        int? apartmentFilterId = null;
        if (!string.IsNullOrWhiteSpace(ApartmentFilterTextBox.Text) &&
            int.TryParse(ApartmentFilterTextBox.Text, out int aptId))
        {
            apartmentFilterId = aptId;
        }

        int? houseFilterId = null;
        if (!string.IsNullOrWhiteSpace(HouseFilterTextBox.Text) &&
            int.TryParse(HouseFilterTextBox.Text, out int hseId))
        {
            houseFilterId = hseId;
        }

        int? landFilterId = null;
        if (!string.IsNullOrWhiteSpace(LandFilterTextBox.Text) &&
            int.TryParse(LandFilterTextBox.Text, out int lndId))
        {
            landFilterId = lndId;
        }

        var agent = _agents[AgentComboBox.SelectedIndex];
        var client = _clients[ClientComboBox.SelectedIndex];

        using var context = new RealEstateContext();
        var entity = context.Demands.Find(selectedDemand.Demand_ID);
        if (entity != null)
        {
            entity.Address = AddressTextBox.Text?.Trim();
            entity.MinPrice = int.TryParse(MinPriceTextBox.Text, out int min) ? min : (int?)null;
            entity.MaxPrice = int.TryParse(MaxPriceTextBox.Text, out int max) ? max : (int?)null;
            entity.Agent_ID = agent.Agent_ID;
            entity.Client_ID = client.Client_ID;
            entity.ApartmentFilter_ID = apartmentFilterId;
            entity.HouseFilter_ID = houseFilterId;
            entity.LandFilter_ID = landFilterId;

            context.SaveChanges();

            ClearFields();
            LoadDemands();

            var successBox = MessageBoxManager.GetMessageBoxStandard("Успех", "Потребность обновлена.");
            await successBox.ShowAsync();
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (selectedDemand == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Выберите потребность для удаления.");
            await box.ShowAsync();
            return;
        }

        using var context = new RealEstateContext();
        bool isInDeal = context.Deals.Any(d => d.Demand_ID == selectedDemand.Demand_ID);
        if (isInDeal)
        {
            var errorBox = MessageBoxManager.GetMessageBoxStandard("Ошибка",
                "Невозможно удалить потребность, участвующую в сделке.");
            await errorBox.ShowAsync();
            return;
        }

        var confirmBox = MessageBoxManager.GetMessageBoxStandard("Подтверждение",
            "Вы уверены, что хотите удалить эту потребность?", ButtonEnum.YesNo);
        var result = await confirmBox.ShowAsync();
        if (result != ButtonResult.Yes) return;

        try
        {
            var demand = context.Demands.Find(selectedDemand.Demand_ID);
            if (demand != null)
            {
                context.Demands.Remove(demand);
                context.SaveChanges();
                ClearFields();
                LoadDemands();
                var successBox = MessageBoxManager.GetMessageBoxStandard("Успех", "Потребность удалена.");
                await successBox.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            var errorBox = MessageBoxManager.GetMessageBoxStandard("Ошибка", $"Не удалось удалить: {ex.Message}");
            await errorBox.ShowAsync();
        }
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = SearchTextBox?.Text?.Trim() ?? string.Empty;
        LoadDemands(searchText);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = new MainWindow();
        mainWindow.Show();
        this.Close();
    }

    private void TypesButton_Click(object sender, RoutedEventArgs e)
    {
        var type = new WindowTypes();
        type.Show();
        this.Close();
    }

    private void ClientsButton_Click(object sender, RoutedEventArgs e)
    {
        var clients = new WindowClients();
        clients.Show();
        this.Close();
    }

    private void AgentsButton_Click(object sender, RoutedEventArgs e)
    {
        var agents = new WindowAgents();
        agents.Show();
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

    private void DealsButton_Click(object sender, RoutedEventArgs e)
    {
        var deals = new WindowDeals();
        deals.Show();
        this.Close();
    }

    private void DemandsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        int selectedIndex = DemandsListBox.SelectedIndex;
        if (selectedIndex < 0 || selectedIndex >= _demandList.Count)
        {
            ClearFields();
            return;
        }

        selectedDemand = _demandList[selectedIndex];
        AddressTextBox.Text = selectedDemand.Address;
        MinPriceTextBox.Text = selectedDemand.MinPrice?.ToString();
        MaxPriceTextBox.Text = selectedDemand.MaxPrice?.ToString();

        var agentIndex = _agents.FindIndex(a => a.Agent_ID == selectedDemand.Agent_ID);
        if (agentIndex >= 0)
            AgentComboBox.SelectedIndex = agentIndex;

        var clientIndex = _clients.FindIndex(c => c.Client_ID == selectedDemand.Client_ID);
        if (clientIndex >= 0)
            ClientComboBox.SelectedIndex = clientIndex;

        ApartmentFilterTextBox.Text = selectedDemand.ApartmentFilter_ID?.ToString();
        HouseFilterTextBox.Text = selectedDemand.HouseFilter_ID?.ToString();
        LandFilterTextBox.Text = selectedDemand.LandFilter_ID?.ToString();
    }

    private void ClearFields()
    {
        AddressTextBox.Clear();
        MinPriceTextBox.Clear();
        MaxPriceTextBox.Clear();
        ApartmentFilterTextBox.Clear();
        HouseFilterTextBox.Clear();
        LandFilterTextBox.Clear();
        AgentComboBox.SelectedItem = null;
        ClientComboBox.SelectedItem = null;
        selectedDemand = null;
    }
}