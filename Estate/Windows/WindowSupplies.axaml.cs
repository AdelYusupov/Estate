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

public partial class WindowSupplies : Window
{
    private Supply selectedSupply;
    private List<RealEstate> _realEstates = new();
    private List<Agent> _agents = new();
    private List<Client> _clients = new();

    public WindowSupplies()
    {
        InitializeComponent();
        LoadData();
        LoadSupplies();
        SearchTextBox.TextChanged += SearchTextBox_TextChanged; // Подписка на событие
    }

    private void LoadData()
    {
        using var context = new RealEstateContext();
        _realEstates = context.RealEstates.ToList();
        _agents = context.Agents.ToList();
        _clients = context.Clients.ToList();

        RealEstateComboBox.Items.Clear();
        foreach (var re in _realEstates)
        {
            RealEstateComboBox.Items.Add($"{re.Address}, {re.HouseNumber} ({re.RealEstateType?.TypeName})");
        }

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

    private void LoadSupplies(string search = null)
    {
        using var context = new RealEstateContext();
        IQueryable<Supply> query = context.Supplies
            .Include(s => s.RealEstate)
            .Include(s => s.Agent)
            .Include(s => s.Client);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
                s.Price.ToString().Contains(search) ||
                s.RealEstate.Address.Contains(search));
        }

        SuppliesListBox.Items.Clear();
        foreach (var supply in query)
        {
            SuppliesListBox.Items.Add($"Цена: {supply.Price} ₽ — {supply.RealEstate.Address}, {supply.RealEstate.HouseNumber}");
        }
    }

    private bool ValidateFields(out string errorMessage)
    {
        errorMessage = "";
        if (string.IsNullOrWhiteSpace(PriceTextBox.Text))
            errorMessage = "Введите цену.";
        else if (AgentComboBox.SelectedItem == null)
            errorMessage = "Выберите риэлтора.";
        else if (ClientComboBox.SelectedItem == null)
            errorMessage = "Выберите клиента.";
        else if (RealEstateComboBox.SelectedItem == null)
            errorMessage = "Выберите объект недвижимости.";
        else if (!int.TryParse(PriceTextBox.Text, out int price) || price <= 0)
            errorMessage = "Цена должна быть положительным целым числом.";

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

        int selectedIndexRe = RealEstateComboBox.SelectedIndex;
        int selectedIndexAgent = AgentComboBox.SelectedIndex;
        int selectedIndexClient = ClientComboBox.SelectedIndex;

        if (selectedIndexRe < 0 || selectedIndexAgent < 0 || selectedIndexClient < 0)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Выберите все поля.");
            await box.ShowAsync();
            return;
        }

        var realEstate = _realEstates[selectedIndexRe];
        var agent = _agents[selectedIndexAgent];
        var client = _clients[selectedIndexClient];

        using var context = new RealEstateContext();
        var supply = new Supply
        {
            Price = int.Parse(PriceTextBox.Text),
            RealEstate_ID = realEstate.RealEstate_ID,
            Agent_ID = agent.Agent_ID,
            Client_ID = client.Client_ID
        };

        context.Supplies.Add(supply);
        context.SaveChanges();

        ClearFields();
        LoadSupplies();

        var successBox = MessageBoxManager.GetMessageBoxStandard("Успех", "Предложение добавлено.");
        await successBox.ShowAsync();
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (selectedSupply == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Выберите предложение для редактирования.");
            await box.ShowAsync();
            return;
        }

        if (!ValidateFields(out string errorMessage))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", errorMessage);
            await box.ShowAsync();
            return;
        }

        int selectedIndexRe = RealEstateComboBox.SelectedIndex;
        int selectedIndexAgent = AgentComboBox.SelectedIndex;
        int selectedIndexClient = ClientComboBox.SelectedIndex;

        if (selectedIndexRe < 0 || selectedIndexAgent < 0 || selectedIndexClient < 0)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Выберите все поля.");
            await box.ShowAsync();
            return;
        }

        var realEstate = _realEstates[selectedIndexRe];
        var agent = _agents[selectedIndexAgent];
        var client = _clients[selectedIndexClient];

        using var context = new RealEstateContext();
        var entity = context.Supplies.Find(selectedSupply.Supply_ID);
        if (entity != null)
        {
            entity.Price = int.Parse(PriceTextBox.Text);
            entity.RealEstate_ID = realEstate.RealEstate_ID;
            entity.Agent_ID = agent.Agent_ID;
            entity.Client_ID = client.Client_ID;

            context.SaveChanges();

            ClearFields();
            LoadSupplies();

            var successBox = MessageBoxManager.GetMessageBoxStandard("Успех", "Предложение обновлено.");
            await successBox.ShowAsync();
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (selectedSupply == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Выберите предложение для удаления.");
            await box.ShowAsync();
            return;
        }

        using var context = new RealEstateContext();
        var isUsedInDeal = context.Deals.Any(d => d.Supply_ID == selectedSupply.Supply_ID);

        if (isUsedInDeal)
        {
            var errorBox = MessageBoxManager.GetMessageBoxStandard("Ошибка",
                "Невозможно удалить предложение, так как оно участвует в сделке.");
            await errorBox.ShowAsync();
            return;
        }

        var confirmBox = MessageBoxManager.GetMessageBoxStandard("Подтверждение",
            "Вы уверены, что хотите удалить это предложение?", ButtonEnum.YesNo);
        var result = await confirmBox.ShowAsync();
        if (result != ButtonResult.Yes) return;

        var supply = context.Supplies.Find(selectedSupply.Supply_ID);
        if (supply != null)
        {
            try
            {
                context.Supplies.Remove(supply);
                context.SaveChanges();

                ClearFields();
                LoadSupplies();

                var successBox = MessageBoxManager.GetMessageBoxStandard("Успех", "Предложение удалено.");
                await successBox.ShowAsync();
            }
            catch (Exception ex)
            {
                var errorBox = MessageBoxManager.GetMessageBoxStandard("Ошибка",
                    $"Не удалось удалить предложение: {ex.Message}");
                await errorBox.ShowAsync();
            }
        }
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = SearchTextBox?.Text?.Trim() ?? string.Empty;
        LoadSupplies(searchText);
    }

    private void SuppliesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        int selectedIndex = SuppliesListBox.SelectedIndex;
        if (selectedIndex < 0) return;

        using var context = new RealEstateContext();
        var supplies = context.Supplies
            .Include(s => s.RealEstate)
            .Include(s => s.Agent)
            .Include(s => s.Client)
            .ToList();

        if (selectedIndex >= supplies.Count) return;

        selectedSupply = supplies[selectedIndex];
        PriceTextBox.Text = selectedSupply.Price.ToString();

        var realEstateIndex = _realEstates.FindIndex(r => r.RealEstate_ID == selectedSupply.RealEstate_ID);
        if (realEstateIndex >= 0)
            RealEstateComboBox.SelectedIndex = realEstateIndex;

        var agentIndex = _agents.FindIndex(a => a.Agent_ID == selectedSupply.Agent_ID);
        if (agentIndex >= 0)
            AgentComboBox.SelectedIndex = agentIndex;

        var clientIndex = _clients.FindIndex(c => c.Client_ID == selectedSupply.Client_ID);
        if (clientIndex >= 0)
            ClientComboBox.SelectedIndex = clientIndex;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = new MainWindow();
        mainWindow.Show();
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

    private void TypesButton_Click(object sender, RoutedEventArgs e)
    {
        var type = new WindowTypes();
        type.Show();
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
        PriceTextBox.Clear();
        RealEstateComboBox.SelectedItem = null;
        AgentComboBox.SelectedItem = null;
        ClientComboBox.SelectedItem = null;
        selectedSupply = null;
    }
}