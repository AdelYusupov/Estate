using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ThingLing.Controls;
using Estate.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Estate;

public partial class WindowTypes : Window
{
    private RealEstateType selectedType;
    private RealEstateContext context;

    public WindowTypes()
    {
        InitializeComponent();
        LoadTypes();
    }

    private void LoadTypes()
    {
        using var tempContext = new RealEstateContext();

        List<string> types = tempContext.RealEstateTypes
                                        .Select(t => t.TypeName)
                                        .ToList();

        TypesListBox.ItemsSource = types;
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        var typeName = NameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(typeName))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Введите название типа недвижимости.");
            await box.ShowAsync();
            return;
        }

        using var context = new RealEstateContext();

        // Проверяем, изменилось ли имя типа
        if (selectedType != null && selectedType.TypeName == typeName)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Информация", "Имя типа не изменилось.");
            await box.ShowAsync();
            return;
        }

        // Проверяем, существует ли уже тип с таким названием
        bool typeExists = context.RealEstateTypes.Any(t => t.TypeName == typeName && t.Type_ID != selectedType.Type_ID);
        if (typeExists)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Тип недвижимости с таким названием уже существует.");
            await box.ShowAsync();
            return;
        }

        // Если selectedType не null — обновляем запись
        if (selectedType != null)
        {
            var typeToUpdate = context.RealEstateTypes.FirstOrDefault(t => t.Type_ID == selectedType.Type_ID);

            if (typeToUpdate != null)
            {
                typeToUpdate.TypeName = typeName;
                context.SaveChanges();

                var successBox = MessageBoxManager.GetMessageBoxStandard("Успех", "Тип недвижимости успешно обновлен.");
                await successBox.ShowAsync();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Не выбран тип для редактирования.");
            await box.ShowAsync();
        }

        ClearFields();
        LoadTypes();
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var typeName = NameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(typeName))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Введите название типа недвижимости.");
            await box.ShowAsync();
            return;
        }

        using var context = new RealEstateContext();

        // Проверяем, существует ли уже тип с таким названием
        bool typeExists = context.RealEstateTypes.Any(t => t.TypeName == typeName);
        if (typeExists)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Тип недвижимости с таким названием уже существует.");
            await box.ShowAsync();
            return;
        }

        // Создаем новый тип недвижимости
        var newType = new RealEstateType
        {
            TypeName = typeName
        };

        // Добавляем в базу данных
        context.RealEstateTypes.Add(newType);
        context.SaveChanges();

        // Очищаем поля и обновляем список
        ClearFields();
        LoadTypes();

        var successBox = MessageBoxManager.GetMessageBoxStandard("Успех", "Тип недвижимости успешно добавлен.");
        await successBox.ShowAsync();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (selectedType == null)
            {
                var errorBox = MessageBoxManager.GetMessageBoxStandard("Ошибка",
                    "Выберите тип недвижимости для удаления.");
                await errorBox.ShowAsync();
                return;
            }

            var confirmBox = MessageBoxManager.GetMessageBoxStandard("Подтверждение удаления",
                "Вы уверены, что хотите удалить этот тип недвижимости?",
                ButtonEnum.YesNo);
            var result = await confirmBox.ShowAsync();

            if (result != ButtonResult.Yes)
                return;

            using var context = new RealEstateContext();

            // Проверяем, используется ли тип недвижимости в каких-либо объектах
            bool isUsedInProperties = context.RealEstates.Any(p => p.Type_ID == selectedType.Type_ID);

            if (isUsedInProperties)
            {
                var usedBox = MessageBoxManager.GetMessageBoxStandard("Ошибка",
                    "Невозможно удалить тип недвижимости, так как он связан с одним или несколькими объектами недвижимости.");
                await usedBox.ShowAsync();
                return;
            }

            var typeToDelete = context.RealEstateTypes
                .FirstOrDefault(t => t.Type_ID == selectedType.Type_ID);

            if (typeToDelete != null)
            {
                context.RealEstateTypes.Remove(typeToDelete);
                context.SaveChanges();

                ClearFields();
                LoadTypes();

                var successBox = MessageBoxManager.GetMessageBoxStandard("Успех",
                    "Тип недвижимости успешно удален!");
                await successBox.ShowAsync();
            }
            else
            {
                var notFoundBox = MessageBoxManager.GetMessageBoxStandard("Ошибка",
                    "Тип недвижимости не найден.");
                await notFoundBox.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            var exceptionBox = MessageBoxManager.GetMessageBoxStandard("Ошибка",
                $"Ошибка при удалении типа недвижимости: {ex.Message}");
            await exceptionBox.ShowAsync();
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        var back = new MainWindow();
        back.Show();
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

    private void ClearFields()
    {
        NameTextBox.Clear();
        selectedType = null;
    }

    private void TypesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedTypeName = TypesListBox.SelectedItem as string;

        if (selectedTypeName != null)
        {
            NameTextBox.Text = selectedTypeName;

            // Получаем полный объект типа
            using (var context = new RealEstateContext())
            {
                selectedType = context.RealEstateTypes
                    .FirstOrDefault(t => t.TypeName == selectedTypeName);
            }
        }
    }
}