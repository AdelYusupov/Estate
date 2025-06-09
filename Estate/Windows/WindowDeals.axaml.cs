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

public partial class WindowDeals : Window
{
    private Deal selectedDeal;
    private List<Demand> _demandList = new();
    private List<Supply> _supplyList = new();
    private List<Deal> _dealList = new();

    public WindowDeals()
    {
        InitializeComponent();
        LoadData();
        LoadDeals();
        UpdateComboBoxStates(); // Установка начального состояния
    }

    private void LoadData()
    {
        using var context = new RealEstateContext();
        _demandList = context.Demands
            .Include(d => d.Client)
            .Include(d => d.Agent)
            .ToList();
        _supplyList = context.Supplies
            .Include(s => s.RealEstate)
            .ThenInclude(r => r.RealEstateType)
            .Include(s => s.Agent)
            .ToList();

        DemandComboBox.Items.Clear();
        foreach (var demand in _demandList)
        {
            string clientName = $"{demand.Client?.LastName ?? "—"} {demand.Client?.FirstName ?? ""}";
            DemandComboBox.Items.Add($"Потребность #{demand.Demand_ID} — {clientName}");
        }

        SupplyComboBox.Items.Clear();
        foreach (var supply in _supplyList)
        {
            string address = $"{supply.RealEstate?.Address ?? "—"}, {supply.RealEstate?.HouseNumber ?? ""}";
            SupplyComboBox.Items.Add($"Предложение #{supply.Supply_ID} — {address}");
        }
    }

    private void LoadDeals(string search = null)
    {
        using var context = new RealEstateContext();
        IQueryable<Deal> query = context.Deals
            .Include(d => d.Supply)
            .ThenInclude(s => s.RealEstate)
            .Include(d => d.Demand)
            .ThenInclude(d => d.Client);

        // Если есть текст для поиска — применяем фильтр
        if (!string.IsNullOrWhiteSpace(search))
        {
            int.TryParse(search, out int id);
            query = query.Where(d =>
                d.Supply_ID == id || d.Demand_ID == id ||
                EF.Functions.Like(d.Supply.RealEstate.Address, $"%{search}%"));
        }
        // Если search == null или пустой — выбираем все записи без фильтрации

        _dealList = query.ToList();
        DealsListBox.Items.Clear();
        foreach (var deal in _dealList)
        {
            string buyer = $"{deal.Demand.Client?.LastName ?? "—"}";
            string address = $"{deal.Supply.RealEstate?.Address ?? "—"}";
            DealsListBox.Items.Add($"Сделка #{deal.Supply_ID}-{deal.Demand_ID}, Покупатель: {buyer}, Объект: {address}");
        }
    }

    private void CalculateCommissions(Supply supply, Demand demand)
    {
        if (supply == null || demand == null)
        {
            ClearCommissionFields();
            return;
        }

        decimal price = supply.Price;
        decimal sellerCommission = 0;
        decimal buyerCommission = price * 0.03m; // 3%
        string typeName = supply.RealEstate?.RealEstateType?.TypeName.ToLower() ?? "house";

        switch (typeName)
        {
            case "apartment":
                sellerCommission = 36000 + price * 0.01m;
                break;
            case "house":
                sellerCommission = 30000 + price * 0.01m;
                break;
            case "land":
                sellerCommission = 30000 + price * 0.02m;
                break;
            default:
                sellerCommission = 30000 + price * 0.01m;
                break;
        }

        decimal agentSellerShare = 0.45m;
        decimal agentBuyerShare = 0.45m;
        decimal agentSellerCommission = sellerCommission * agentSellerShare;
        decimal agentBuyerCommission = buyerCommission * agentBuyerShare;
        decimal companyCommission =
            (sellerCommission - agentSellerCommission) +
            (buyerCommission - agentBuyerCommission);

        SellerCommissionText.Text = $"Продавец: {sellerCommission:N0} ₽";
        BuyerCommissionText.Text = $"Покупатель: {buyerCommission:N0} ₽";
        AgentSellerCommissionText.Text = $"Риэлтору продавца: {agentSellerCommission:N0} ₽";
        AgentBuyerCommissionText.Text = $"Риэлтору покупателя: {agentBuyerCommission:N0} ₽";
        CompanyCommissionText.Text = $"Компании: {companyCommission:N0} ₽";
    }

    private void ClearCommissionFields()
    {
        SellerCommissionText.Text = "—";
        BuyerCommissionText.Text = "—";
        AgentSellerCommissionText.Text = "—";
        AgentBuyerCommissionText.Text = "—";
        CompanyCommissionText.Text = "—";
    }

    private void LoadMatchingOffers(int demandId)
    {
        MatchingOffersComboBox.Items.Clear();
        using var context = new RealEstateContext();
        var demand = context.Demands
            .Include(d => d.ApartmentFilter)
            .Include(d => d.HouseFilter)
            .Include(d => d.LandFilter)
            .FirstOrDefault(d => d.Demand_ID == demandId);
        if (demand == null) return;

        var offers = context.Supplies
            .Include(s => s.RealEstate)
            .ThenInclude(r => r.RealEstateType)
            .Where(s => !context.Deals.Any(deal => deal.Supply_ID == s.Supply_ID && deal.Demand_ID == demandId));

        var filteredOffers = new List<Supply>();
        foreach (var supply in offers)
        {
            string typeName = supply.RealEstate?.RealEstateType?.TypeName.ToLower() ?? "house";
            if ((typeName == "apartment" && demand.ApartmentFilter != null) ||
                (typeName == "house" && demand.HouseFilter != null) ||
                (typeName == "land" && demand.LandFilter != null))
            {
                filteredOffers.Add(supply);
            }
        }

        foreach (var offer in filteredOffers)
        {
            string address = $"{offer.RealEstate.Address}, {offer.RealEstate.HouseNumber}";
            MatchingOffersComboBox.Items.Add($"Предложение #{offer.Supply_ID}: {address}");
        }
    }

    private void LoadMatchingDemands(int supplyId)
    {
        MatchingOffersComboBox.Items.Clear();
        using var context = new RealEstateContext();
        var supply = context.Supplies
            .Include(s => s.RealEstate)
            .ThenInclude(r => r.RealEstateType)
            .FirstOrDefault(s => s.Supply_ID == supplyId);
        if (supply == null) return;

        string typeName = supply.RealEstate?.RealEstateType?.TypeName.ToLower() ?? "house";

        var demands = context.Demands
            .Include(d => d.Client)
            .Include(d => d.Agent)
            .Where(d => !context.Deals.Any(deal => deal.Supply_ID == supplyId && deal.Demand_ID == d.Demand_ID))
            .ToList();

        var filteredDemands = new List<Demand>();
        foreach (var d in demands)
        {
            if ((typeName == "apartment" && d.ApartmentFilter_ID != null) ||
                (typeName == "house" && d.HouseFilter_ID != null) ||
                (typeName == "land" && d.LandFilter_ID != null))
            {
                filteredDemands.Add(d);
            }
        }

        foreach (var d in filteredDemands)
        {
            string clientName = $"{d.Client.LastName} {d.Client.FirstName}";
            MatchingOffersComboBox.Items.Add($"Потребность #{d.Demand_ID}: {clientName}");
        }
    }

    private async void CreateDealButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DemandComboBox.SelectedIndex < 0 || SupplyComboBox.SelectedIndex < 0)
            {
                await MessageBoxManager
                    .GetMessageBoxStandard("Ошибка", "Выберите и потребность, и предложение")
                    .ShowAsync();
                return;
            }

            int demandId = _demandList[DemandComboBox.SelectedIndex].Demand_ID;
            int supplyId = _supplyList[SupplyComboBox.SelectedIndex].Supply_ID;

            using var ctx = new RealEstateContext();
            if (!await ctx.Demands.AnyAsync(d => d.Demand_ID == demandId) ||
                !await ctx.Supplies.AnyAsync(s => s.Supply_ID == supplyId))
            {
                await MessageBoxManager
                    .GetMessageBoxStandard("Ошибка", "Выбранные записи не найдены")
                    .ShowAsync();
                return;
            }

            if (await ctx.Deals.AnyAsync(d => d.Demand_ID == demandId && d.Supply_ID == supplyId))
            {
                await MessageBoxManager
                    .GetMessageBoxStandard("Ошибка", "Эта сделка уже существует")
                    .ShowAsync();
                return;
            }

            var deal = new Deal { Demand_ID = demandId, Supply_ID = supplyId };
            ctx.Deals.Add(deal);
            await ctx.SaveChangesAsync();
            LoadDeals();
            ClearFields();

            await MessageBoxManager
                .GetMessageBoxStandard("Успех", "Сделка создана")
                .ShowAsync();
        }
        catch (Exception ex)
        {
            await MessageBoxManager
                .GetMessageBoxStandard("Ошибка", $"Ошибка: {ex.Message}")
                .ShowAsync();
        }
    }

    private async void EditDealButton_Click(object sender, RoutedEventArgs e)
    {
        if (selectedDeal == null)
        {
            await MessageBoxManager
                .GetMessageBoxStandard("Ошибка", "Выберите сделку для редактирования")
                .ShowAsync();
            return;
        }

        if (DemandComboBox.SelectedIndex < 0 || SupplyComboBox.SelectedIndex < 0)
        {
            await MessageBoxManager
                .GetMessageBoxStandard("Ошибка", "Выберите и потребность, и предложение")
                .ShowAsync();
            return;
        }

        int newDemandId = _demandList[DemandComboBox.SelectedIndex].Demand_ID;
        int newSupplyId = _supplyList[SupplyComboBox.SelectedIndex].Supply_ID;

        if (selectedDeal.Demand_ID == newDemandId && selectedDeal.Supply_ID == newSupplyId)
        {
            await MessageBoxManager
                .GetMessageBoxStandard("Информация", "Не было изменений в сделке")
                .ShowAsync();
            return;
        }

        try
        {
            using var context = new RealEstateContext();

            if (await context.Deals.AnyAsync(d =>
                d.Demand_ID == newDemandId &&
                d.Supply_ID == newSupplyId &&
                !(d.Demand_ID == selectedDeal.Demand_ID && d.Supply_ID == selectedDeal.Supply_ID)))
            {
                await MessageBoxManager
                    .GetMessageBoxStandard("Ошибка", "Такая сделка уже существует")
                    .ShowAsync();
                return;
            }

            var existingDeal = await context.Deals
                .FirstOrDefaultAsync(d =>
                    d.Demand_ID == selectedDeal.Demand_ID &&
                    d.Supply_ID == selectedDeal.Supply_ID);

            if (existingDeal == null)
            {
                await MessageBoxManager
                    .GetMessageBoxStandard("Ошибка", "Выбранная сделка не найдена в базе данных")
                    .ShowAsync();
                return;
            }

            context.Deals.Remove(existingDeal);

            var newDeal = new Deal
            {
                Demand_ID = newDemandId,
                Supply_ID = newSupplyId
            };

            context.Deals.Add(newDeal);
            await context.SaveChangesAsync();

            LoadDeals();
            ClearFields();

            await MessageBoxManager
                .GetMessageBoxStandard("Успех", "Сделка успешно изменена")
                .ShowAsync();
        }
        catch (Exception ex)
        {
            await MessageBoxManager
                .GetMessageBoxStandard("Ошибка", $"Ошибка при изменении сделки: {ex.Message}")
                .ShowAsync();
        }
    }

    private void DemandComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DemandComboBox.SelectedIndex >= 0)
        {
            var selectedDemand = _demandList[DemandComboBox.SelectedIndex];
            LoadMatchingOffers(selectedDemand.Demand_ID);
        }
        else
        {
            MatchingOffersComboBox.Items.Clear();
        }

        UpdateComboBoxStates();
    }

    private void SupplyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SupplyComboBox.SelectedIndex >= 0)
        {
            var selectedSupply = _supplyList[SupplyComboBox.SelectedIndex];
            LoadMatchingDemands(selectedSupply.Supply_ID);
        }
        else
        {
            MatchingOffersComboBox.Items.Clear();
        }

        UpdateComboBoxStates();
    }

    private void MatchingOffersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MatchingOffersComboBox.SelectedIndex < 0) return;
        string selectedItem = MatchingOffersComboBox.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(selectedItem)) return;

        try
        {
            int id = int.Parse(selectedItem.Split('#')[1].Split(':')[0]);
            if (selectedItem.StartsWith("Потребность"))
            {
                var demand = _demandList.FirstOrDefault(d => d.Demand_ID == id);
                if (demand != null && _demandList.Contains(demand))
                {
                    int index = _demandList.IndexOf(demand);
                    if (index >= 0)
                    {
                        DemandComboBox.SelectedIndex = index;
                    }
                }
            }
            else if (selectedItem.StartsWith("Предложение"))
            {
                var supply = _supplyList.FirstOrDefault(s => s.Supply_ID == id);
                if (supply != null && _supplyList.Contains(supply))
                {
                    int index = _supplyList.IndexOf(supply);
                    if (index >= 0)
                    {
                        SupplyComboBox.SelectedIndex = index;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке выбора: {ex.Message}");
        }

        UpdateComboBoxStates();
    }

    private void UpdateComboBoxStates()
    {
        bool isMatchingSelected = MatchingOffersComboBox.SelectedIndex >= 0;
        bool isDemandSelected = DemandComboBox.SelectedIndex >= 0;
        bool isSupplySelected = SupplyComboBox.SelectedIndex >= 0;

        DemandComboBox.IsEnabled = !(isMatchingSelected && isSupplySelected);
        SupplyComboBox.IsEnabled = !(isMatchingSelected && isDemandSelected);
        MatchingOffersComboBox.IsEnabled = !(isDemandSelected && isSupplySelected);
    }

    private void DealsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        int selectedIndex = DealsListBox.SelectedIndex;
        if (selectedIndex < 0 || selectedIndex >= _dealList.Count)
        {
            ClearFields();
            return;
        }

        selectedDeal = _dealList[selectedIndex];

        if (selectedDeal != null)
        {
            var supply = _supplyList.FirstOrDefault(s => s.Supply_ID == selectedDeal.Supply_ID);
            var demand = _demandList.FirstOrDefault(d => d.Demand_ID == selectedDeal.Demand_ID);

            if (supply != null)
            {
                int index = _supplyList.IndexOf(supply);
                if (index >= 0)
                    SupplyComboBox.SelectedIndex = index;
            }

            if (demand != null)
            {
                int index = _demandList.IndexOf(demand);
                if (index >= 0)
                    DemandComboBox.SelectedIndex = index;
            }

            CalculateCommissions(supply, demand);
        }

        UpdateComboBoxStates(); // Обновляем состояние комбобоксов
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (selectedDeal == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Выберите сделку для удаления.");
            await box.ShowAsync();
            return;
        }

        var confirmBox = MessageBoxManager.GetMessageBoxStandard("Подтверждение",
            "Вы уверены, что хотите удалить эту сделку?", ButtonEnum.YesNo);
        var result = await confirmBox.ShowAsync();
        if (result != ButtonResult.Yes) return;

        using var context = new RealEstateContext();
        try
        {
            var entity = context.Deals
                .FirstOrDefault(d => d.Supply_ID == selectedDeal.Supply_ID &&
                                   d.Demand_ID == selectedDeal.Demand_ID);

            if (entity != null)
            {
                context.Deals.Remove(entity);
                await context.SaveChangesAsync();
                ClearFields();
                LoadDeals();
                var successBox = MessageBoxManager.GetMessageBoxStandard("Успех", "Сделка удалена.");
                await successBox.ShowAsync();
            }
            else
            {
                var errorBox = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Сделка не найдена.");
                await errorBox.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            var errorBox = MessageBoxManager.GetMessageBoxStandard("Ошибка", $"Не удалось удалить: {ex.Message}");
            await errorBox.ShowAsync();
        }
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        string searchText = string.Empty;
        if (SearchTextBox != null)
        {
            searchText = SearchTextBox.Text?.Trim() ?? string.Empty;
        }

        LoadDeals(searchText);
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
    private void DemandsButton_Click(object sender, RoutedEventArgs e)
    {

        var demand = new WindowDemands();
        demand.Show();
        this.Close();

    }
    
    private void ClearFields()
    {
        DemandComboBox.SelectedItem = null;
        SupplyComboBox.SelectedItem = null;
        MatchingOffersComboBox.Items.Clear();
        ClearCommissionFields();
        selectedDeal = null;

        UpdateComboBoxStates(); // Сбрасываем состояние комбобоксов
    }
}