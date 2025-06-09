using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Estate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Estate;

public partial class WindowRealEstate : Window
{
    private RealEstate selectedRealEstate;
    private List<District> _districts = new();
    private List<RealEstateType> _realEstateTypes = new();
    private List<RealEstate> _realEstateList = new();

    public WindowRealEstate()
    {
        InitializeComponent();
        LoadDistricts();
        LoadRealEstateTypes();
        LoadRealEstateList();
        RealEstateTypeFilterComboBox.Items.Add("Квартира");
        RealEstateTypeFilterComboBox.Items.Add("Дом");
        RealEstateTypeFilterComboBox.Items.Add("Земля");
        RealEstateTypeFilterComboBox.SelectedIndex = -1;
        SearchTextBox.TextChanged += SearchTextBox_TextChanged;
    }

    private void RealEstateTypeFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        string selectedType = RealEstateTypeFilterComboBox.SelectedItem as string;
        List<RealEstate> filteredList = _realEstateList;
        switch (selectedType)
        {
            case "Квартира":
                filteredList = _realEstateList.Where(r =>
                    r.RealEstateType != null && r.RealEstateType.TypeName == "apartment").ToList();
                break;
            case "Дом":
                filteredList = _realEstateList.Where(r =>
                    r.RealEstateType != null && r.RealEstateType.TypeName == "house").ToList();
                break;
            case "Земля":
                filteredList = _realEstateList.Where(r =>
                    r.RealEstateType != null && r.RealEstateType.TypeName == "land").ToList();
                break;
        }
        UpdateRealEstateListBox(filteredList);
    }

    private void UpdateRealEstateListBox(List<RealEstate> realEstates)
    {
        RealEstateListBox.Items.Clear();
        foreach (var item in realEstates.Select(r =>
            $"{r.Address}, {r.HouseNumber} ({r.RealEstateType?.TypeName ?? "—"}, {r.District?.Name ?? "—"})"))
        {
            RealEstateListBox.Items.Add(item);
        }
    }

    private void LoadDistricts()
    {
        using var context = new RealEstateContext();
        _districts = context.Districts.ToList();
        DistrictComboBox.Items.Clear();
        foreach (var district in _districts)
        {
            DistrictComboBox.Items.Add(district.Name);
        }
    }

    private void LoadRealEstateTypes()
    {
        using var context = new RealEstateContext();
        _realEstateTypes = context.RealEstateTypes.ToList();
        RealEstateTypeComboBox.Items.Clear();
        foreach (var type in _realEstateTypes)
        {
            RealEstateTypeComboBox.Items.Add(type.TypeName);
        }
    }

    private void LoadRealEstateList(string search = null)
    {
        using var context = new RealEstateContext();
        IQueryable<RealEstate> query = context.RealEstates
            .Include(r => r.District)
            .Include(r => r.RealEstateType);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.Address.Contains(search) ||
                                   r.HouseNumber.Contains(search));
        }

        _realEstateList = query.ToList();
        UpdateRealEstateListBox(_realEstateList);
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateFields(out string errorMessage))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", errorMessage);
            await box.ShowAsync();
            return;
        }

        string selectedDistrictName = DistrictComboBox.SelectedItem?.ToString();
        string selectedTypeName = RealEstateTypeComboBox.SelectedItem?.ToString();

        District district = _districts.FirstOrDefault(d => d.Name == selectedDistrictName);
        RealEstateType type = _realEstateTypes.FirstOrDefault(t => t.TypeName == selectedTypeName);

        if (district == null || type == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Не удалось определить район или тип недвижимости.");
            await box.ShowAsync();
            return;
        }

        using var context = new RealEstateContext();
        var realEstate = new RealEstate
        {
            Address = AddressTextBox.Text.Trim(),
            HouseNumber = string.IsNullOrWhiteSpace(HouseNumberTextBox.Text) ? "" : HouseNumberTextBox.Text.Trim(),
            ApartmentNumber = string.IsNullOrWhiteSpace(ApartmentNumberTextBox.Text) ? "" : ApartmentNumberTextBox.Text.Trim(),
            LandNumber = string.IsNullOrWhiteSpace(LandNumberTextBox.Text) ? "" : LandNumberTextBox.Text.Trim(),
            District_ID = district.District_ID,
            Type_ID = type.Type_ID,
            TotalArea = string.IsNullOrWhiteSpace(TotalAreaTextBox.Text) ? (decimal?)null : decimal.Parse(TotalAreaTextBox.Text),
            Rooms = string.IsNullOrWhiteSpace(RoomsTextBox.Text) ? (int?)null : int.Parse(RoomsTextBox.Text),
            Floor = string.IsNullOrWhiteSpace(FloorTextBox.Text) ? (int?)null : int.Parse(FloorTextBox.Text),
            TotalFloors = string.IsNullOrWhiteSpace(TotalFloorsTextBox.Text) ? (int?)null : int.Parse(TotalFloorsTextBox.Text)
        };

        context.RealEstates.Add(realEstate);
        context.SaveChanges();

        ClearFields();
        LoadRealEstateList();

        var successBox = MessageBoxManager.GetMessageBoxStandard("Успех", "Недвижимость успешно добавлена.");
        await successBox.ShowAsync();
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (selectedRealEstate == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Выберите объект недвижимости для редактирования.");
            await box.ShowAsync();
            return;
        }

        if (!ValidateFields(out string errorMessage))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", errorMessage);
            await box.ShowAsync();
            return;
        }

        string selectedDistrictName = DistrictComboBox.SelectedItem?.ToString();
        string selectedTypeName = RealEstateTypeComboBox.SelectedItem?.ToString();

        District district = _districts.FirstOrDefault(d => d.Name == selectedDistrictName);
        RealEstateType type = _realEstateTypes.FirstOrDefault(t => t.TypeName == selectedTypeName);

        if (district == null || type == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Не удалось определить район или тип недвижимости.");
            await box.ShowAsync();
            return;
        }

        using var context = new RealEstateContext();
        var entity = context.RealEstates.Find(selectedRealEstate.RealEstate_ID);
        if (entity == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Объект не найден в базе данных.");
            await box.ShowAsync();
            return;
        }

        entity.Address = AddressTextBox.Text.Trim();
        entity.HouseNumber = string.IsNullOrWhiteSpace(HouseNumberTextBox.Text) ? "" : HouseNumberTextBox.Text.Trim();
        entity.ApartmentNumber = string.IsNullOrWhiteSpace(ApartmentNumberTextBox.Text) ? "" : ApartmentNumberTextBox.Text.Trim();
        entity.LandNumber = string.IsNullOrWhiteSpace(LandNumberTextBox.Text) ? "" : LandNumberTextBox.Text.Trim();
        entity.District_ID = district.District_ID;
        entity.Type_ID = type.Type_ID;
        entity.TotalArea = string.IsNullOrWhiteSpace(TotalAreaTextBox.Text) ? (decimal?)null : decimal.Parse(TotalAreaTextBox.Text);
        entity.Rooms = string.IsNullOrWhiteSpace(RoomsTextBox.Text) ? (int?)null : int.Parse(RoomsTextBox.Text);
        entity.Floor = string.IsNullOrWhiteSpace(FloorTextBox.Text) ? (int?)null : int.Parse(FloorTextBox.Text);
        entity.TotalFloors = string.IsNullOrWhiteSpace(TotalFloorsTextBox.Text) ? (int?)null : int.Parse(TotalFloorsTextBox.Text);

        context.SaveChanges();

        ClearFields();
        LoadRealEstateList();

        var successBox = MessageBoxManager.GetMessageBoxStandard("Успех", "Данные обновлены.");
        await successBox.ShowAsync();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (selectedRealEstate == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Выберите объект недвижимости для удаления.");
            await box.ShowAsync();
            return;
        }

        var confirmBox = MessageBoxManager.GetMessageBoxStandard("Подтверждение",
            "Вы уверены, что хотите удалить этот объект недвижимости?", ButtonEnum.YesNo);
        var result = await confirmBox.ShowAsync();
        if (result != ButtonResult.Yes) return;

        using var context = new RealEstateContext();
        var realEstate = context.RealEstates.Find(selectedRealEstate.RealEstate_ID);
        if (realEstate != null)
        {
            try
            {
                context.RealEstates.Remove(realEstate);
                context.SaveChanges();
                ClearFields();
                LoadRealEstateList();
                var successBox = MessageBoxManager.GetMessageBoxStandard("Успех", "Объект удален.");
                await successBox.ShowAsync();
            }
            catch (Exception ex)
            {
                var errorBox = MessageBoxManager.GetMessageBoxStandard("Ошибка", $"Не удалось удалить: {ex.Message}");
                await errorBox.ShowAsync();
            }
        }
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = SearchTextBox?.Text?.Trim() ?? string.Empty;
        LoadRealEstateList(searchText);
    }

    private void RealEstateListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        int selectedIndex = RealEstateListBox.SelectedIndex;
        if (selectedIndex < 0 || selectedIndex >= _realEstateList.Count)
        {
            ClearFields();
            return;
        }

        selectedRealEstate = _realEstateList[selectedIndex];
        AddressTextBox.Text = selectedRealEstate.Address;
        HouseNumberTextBox.Text = selectedRealEstate.HouseNumber;
        ApartmentNumberTextBox.Text = selectedRealEstate.ApartmentNumber;
        LandNumberTextBox.Text = selectedRealEstate.LandNumber;
        TotalAreaTextBox.Text = selectedRealEstate.TotalArea?.ToString("F2");
        RoomsTextBox.Text = selectedRealEstate.Rooms?.ToString();
        FloorTextBox.Text = selectedRealEstate.Floor?.ToString();
        TotalFloorsTextBox.Text = selectedRealEstate.TotalFloors?.ToString();
        DistrictComboBox.SelectedItem = selectedRealEstate.District?.Name;
        RealEstateTypeComboBox.SelectedItem = selectedRealEstate.RealEstateType?.TypeName;
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

    private void DealsButton_Click(object sender, RoutedEventArgs e)
    {
        var deals = new WindowDeals();
        deals.Show();
        this.Close();
    }

    private bool ValidateFields(out string errorMessage)
    {
        errorMessage = string.Empty;

        // Адрес обязателен
        if (string.IsNullOrWhiteSpace(AddressTextBox.Text))
        {
            errorMessage = "Введите адрес.";
        }
        else if (!Regex.IsMatch(AddressTextBox.Text, @"^[а-яА-ЯёЁa-zA-Z\s,]+$"))
        {
            errorMessage = "Адрес должен содержать только буквы и запятые.";
        }

        // Номер дома может быть пустым, но если указан — должно быть > 0
        else if (!string.IsNullOrWhiteSpace(HouseNumberTextBox.Text) &&
                 (!int.TryParse(HouseNumberTextBox.Text, out int houseNumber) || houseNumber <= 0))
        {
            errorMessage = "Номер дома должен быть целым числом больше 0 или пустым.";
        }

        // Номер квартиры может быть пустым, но если указан — должно быть > 0
        else if (!string.IsNullOrWhiteSpace(ApartmentNumberTextBox.Text) &&
                 (!int.TryParse(ApartmentNumberTextBox.Text, out int apartmentNumber) || apartmentNumber <= 0))
        {
            errorMessage = "Номер квартиры должен быть целым числом больше 0 или пустым.";
        }

        // Номер участка может быть пустым, но если указан — должно быть > 0
        else if (!string.IsNullOrWhiteSpace(LandNumberTextBox.Text) &&
                 (!int.TryParse(LandNumberTextBox.Text, out int landNumber) || landNumber <= 0))
        {
            errorMessage = "Номер участка должен быть целым числом больше 0 или пустым.";
        }

        // Район обязателен
        else if (DistrictComboBox.SelectedItem == null)
        {
            errorMessage = "Выберите район.";
        }

        // Тип недвижимости обязателен
        else if (RealEstateTypeComboBox.SelectedItem == null)
        {
            errorMessage = "Выберите тип недвижимости.";
        }

        return string.IsNullOrEmpty(errorMessage);
    }

    private void ClearFields()
    {
        AddressTextBox.Clear();
        HouseNumberTextBox.Clear();
        ApartmentNumberTextBox.Clear();
        LandNumberTextBox.Clear();
        TotalAreaTextBox.Clear();
        RoomsTextBox.Clear();
        FloorTextBox.Clear();
        TotalFloorsTextBox.Clear();
        DistrictComboBox.SelectedItem = null;
        RealEstateTypeComboBox.SelectedItem = null;
        selectedRealEstate = null;
        RealEstateListBox.SelectedItem = null;
    }
}