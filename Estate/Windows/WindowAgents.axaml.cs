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

public partial class WindowAgents : Window
{
    private Agent selectedAgent;
    private List<Agent> allAgents = new();

    public WindowAgents()
    {
        InitializeComponent();
        LoadAllAgents();
        UpdateAgentsList();
        AgentsListBox.SelectionChanged += AgentsListBox_SelectionChanged;
        SearchTextBox.TextChanged += SearchTextBox_TextChanged;
    }

    private void LoadAllAgents()
    {
        using var context = new RealEstateContext();
        allAgents = context.Agents.ToList();
    }

    private void UpdateAgentsList()
    {
        string searchText = SearchTextBox?.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(searchText))
        {
            AgentsListBox.ItemsSource = allAgents
                .Select(a => $"{a.LastName} {a.FirstName} {a.MiddleName}")
                .ToList();
            return;
        }
        var searchParts = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var filteredAgents = allAgents
            .Where(a =>
                searchParts.Any(part =>
                    LevenshteinDistance.IsMatch(a.LastName, part, 3) ||
                    LevenshteinDistance.IsMatch(a.FirstName, part, 3) ||
                    (!string.IsNullOrEmpty(a.MiddleName) && LevenshteinDistance.IsMatch(a.MiddleName, part, 3))))
            .Select(a => $"{a.LastName} {a.FirstName} {a.MiddleName}")
            .ToList();
        AgentsListBox.ItemsSource = filteredAgents;
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateAgentsList();
    }

    private bool IsValidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // ���������� ���������: ������ ����� ��������� ��� ��������, ��� �������� � �������
        return Regex.IsMatch(name, @"^[�-��-߸�a-zA-Z]+$");
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var firstName = FirstNameTextBox?.Text?.Trim() ?? "";
            var middleName = MiddleNameTextBox?.Text?.Trim() ?? "";
            var lastName = LastNameTextBox?.Text?.Trim() ?? "";
            var dealShare = DealShareTextBox?.Text?.Trim() ?? "";

            // �������� ������������ �����
            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(middleName))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("������", "��� ���� ��� (�������, ���, ��������) �����������.");
                await box.ShowAsync();
                return;
            }

            // �������� ������� ���
            if (!IsValidName(firstName))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("������", "��� ������ ��������� ������ ����� ��������� ��� �������� ��� �������� � �������.");
                await box.ShowAsync();
                return;
            }

            if (!IsValidName(lastName))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("������", "������� ������ ��������� ������ ����� ��������� ��� �������� ��� �������� � �������.");
                await box.ShowAsync();
                return;
            }

            if (!IsValidName(middleName))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("������", "�������� ������ ��������� ������ ����� ��������� ��� �������� ��� �������� � �������.");
                await box.ShowAsync();
                return;
            }

            // �������� ���� ��������
            int? share = null;
            if (!string.IsNullOrWhiteSpace(dealShare))
            {
                if (!int.TryParse(dealShare, out int parsedShare) || parsedShare < 0 || parsedShare > 100)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("������",
                        "���� �� �������� ������ ���� ����� ������ �� 0 �� 100 ��� ������.");
                    await box.ShowAsync();
                    return;
                }
                share = parsedShare;
            }

            // �������� �� ������������� ������
            using var context = new RealEstateContext();
            bool agentExists = context.Agents.Any(a =>
                a.FirstName == firstName &&
                a.LastName == lastName &&
                a.MiddleName == middleName);

            if (agentExists)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("������", "����� � ����� ��� ��� ����������.");
                await box.ShowAsync();
                return;
            }

            // ���������� ������ ������
            var newAgent = new Agent
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DealShare = share
            };

            context.Agents.Add(newAgent);
            context.SaveChanges();

            ClearFields();
            LoadAllAgents();
            UpdateAgentsList();

            var successBox = MessageBoxManager.GetMessageBoxStandard("�����", "����� ������� ��������.");
            await successBox.ShowAsync();
        }
        catch (Exception ex)
        {
            var errorBox = MessageBoxManager.GetMessageBoxStandard("����������� ������",
                $"��������� �������������� ������: {ex.Message}");
            await errorBox.ShowAsync();
        }
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var firstName = FirstNameTextBox?.Text?.Trim() ?? "";
            var middleName = MiddleNameTextBox?.Text?.Trim() ?? "";
            var lastName = LastNameTextBox?.Text?.Trim() ?? "";
            var dealShare = DealShareTextBox?.Text?.Trim() ?? "";

            if (selectedAgent == null)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("������", "�������� ������ ��� ��������������.");
                await box.ShowAsync();
                return;
            }

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(middleName))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("������", "��� ���� ��� (�������, ���, ��������) �����������.");
                await box.ShowAsync();
                return;
            }

            if (!IsValidName(firstName))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("������", "��� ������ ��������� ������ ����� ��������� ��� �������� ��� �������� � �������.");
                await box.ShowAsync();
                return;
            }

            if (!IsValidName(lastName))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("������", "������� ������ ��������� ������ ����� ��������� ��� �������� ��� �������� � �������.");
                await box.ShowAsync();
                return;
            }

            if (!IsValidName(middleName))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("������", "�������� ������ ��������� ������ ����� ��������� ��� �������� ��� �������� � �������.");
                await box.ShowAsync();
                return;
            }

            int? share = null;
            if (!string.IsNullOrWhiteSpace(dealShare))
            {
                if (!int.TryParse(dealShare, out int parsedShare) || parsedShare < 0 || parsedShare > 100)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("������",
                        "���� �� �������� ������ ���� ����� ������ �� 0 �� 100 ��� ����� ���� ������.");
                    await box.ShowAsync();
                    return;
                }
                share = parsedShare;
            }

            using var context = new RealEstateContext();
            var agentToUpdate = context.Agents.FirstOrDefault(a => a.Agent_ID == selectedAgent.Agent_ID);
            if (agentToUpdate != null)
            {
                agentToUpdate.FirstName = firstName;
                agentToUpdate.MiddleName = middleName;
                agentToUpdate.LastName = lastName;
                agentToUpdate.DealShare = share;

                context.SaveChanges();

                ClearFields();
                LoadAllAgents();
                UpdateAgentsList();

                var successBox = MessageBoxManager.GetMessageBoxStandard("�����", "������ ������ ������� ���������.");
                await successBox.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            var errorBox = MessageBoxManager.GetMessageBoxStandard("����������� ������",
                $"��������� �������������� ������: {ex.Message}");
            await errorBox.ShowAsync();
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (selectedAgent == null)
            {
                var errorBox = MessageBoxManager.GetMessageBoxStandard("������",
                    "�������� ������ ��� ��������.");
                await errorBox.ShowAsync();
                return;
            }

            var confirmBox = MessageBoxManager.GetMessageBoxStandard("������������� ��������",
                "�� �������, ��� ������ ������� ����� ������?", ButtonEnum.YesNo);
            var result = await confirmBox.ShowAsync();
            if (result != ButtonResult.Yes)
                return;

            using var context = new RealEstateContext();
            bool hasSupplies = context.Supplies.Any(s => s.Agent_ID == selectedAgent.Agent_ID);
            bool hasDemands = context.Demands.Any(d => d.Agent_ID == selectedAgent.Agent_ID);

            if (hasSupplies || hasDemands)
            {
                var usedBox = MessageBoxManager.GetMessageBoxStandard("������",
                    "���������� ������� ������, ������������ � ������������ ��� ������������.");
                await usedBox.ShowAsync();
                return;
            }

            var agentToDelete = context.Agents.FirstOrDefault(a => a.Agent_ID == selectedAgent.Agent_ID);
            if (agentToDelete != null)
            {
                context.Agents.Remove(agentToDelete);
                context.SaveChanges();

                ClearFields();
                LoadAllAgents();
                UpdateAgentsList();

                var successBox = MessageBoxManager.GetMessageBoxStandard("�����", "����� ������� ������!");
                await successBox.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            var exceptionBox = MessageBoxManager.GetMessageBoxStandard("������",
                $"������ ��� �������� ������: {ex.Message}");
            await exceptionBox.ShowAsync();
        }
    }

    private void AgentsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedAgentName = AgentsListBox.SelectedItem as string;
        if (selectedAgentName != null)
        {
            var nameParts = selectedAgentName.Split(' ');
            if (nameParts.Length >= 3)
            {
                LastNameTextBox.Text = nameParts[0];
                FirstNameTextBox.Text = nameParts[1];
                MiddleNameTextBox.Text = nameParts[2];
            }

            selectedAgent = allAgents.FirstOrDefault(a =>
                a.LastName == nameParts[0] &&
                a.FirstName == nameParts[1] &&
                a.MiddleName == nameParts[2]);

            if (selectedAgent != null)
            {
                DealShareTextBox.Text = selectedAgent.DealShare?.ToString() ?? "";
            }
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

    private void ClientsButton_Click(object sender, RoutedEventArgs e)
    {
        var clientsWindow = new WindowClients();
        clientsWindow.Show();
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
        FirstNameTextBox.Clear();
        MiddleNameTextBox.Clear();
        LastNameTextBox.Clear();
        DealShareTextBox.Clear();
        selectedAgent = null;
    }
}