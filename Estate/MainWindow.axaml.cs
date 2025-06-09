using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using ThingLing.Controls;
using Estate.Models;

namespace Estate
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

           
        }

        private async void Button_Click_Click(object sender, RoutedEventArgs e)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Внимание",
                "Хочешь что-то сделать?",
                ButtonEnum.YesNo);
            var result = await box.ShowAsync();
        }

        private void Button_Click_Click1(object sender, RoutedEventArgs e)
        {
            var wt = new WindowTypes();
            wt.Show();
            this.Close();
        }
    }
}