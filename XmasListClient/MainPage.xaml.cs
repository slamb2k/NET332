using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Windows.Data.Json;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XmasListClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    { 
        private readonly HttpClient _httpClient = new HttpClient();

        public MainPage()
        {
            InitializeComponent();
        }

        private async void ShowError(MessageDialog dialog)
        {
            await dialog.ShowAsync();
        }

        private async void GetXmasList()
        {
            var comboBoxItem = (ComboBoxItem) OwnerDropDown.SelectedValue;
            if (comboBoxItem != null)
            {
                var response =
                    await _httpClient.GetAsync(App.XmasListBaseAddress + "/api/XmasList?owner=" + comboBoxItem.Content);

                if (response.IsSuccessStatusCode)
                {
                    // Read the response as a Json Array and databind to the GridView to display gift items
                    var giftArray = JsonArray.Parse(await response.Content.ReadAsStringAsync());

                    XmasList.ItemsSource = from gift in giftArray
                        select new
                        {
                            Title = gift.GetObject()["Title"].GetString()
                        };
                }
                else
                {
                    var dialog =
                        new MessageDialog("Sorry, an error occurred accessing your Xmas list.  Please try again.");
                    await dialog.ShowAsync();
                }
            }
        }

        private async void AddGift()
        {
            HttpContent content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("Title", GiftText.Text) });

            // Call the XmasList web api
            var comboBoxItem = (ComboBoxItem)OwnerDropDown.SelectedValue;
            if (comboBoxItem != null)
            {
                var response = await _httpClient.PostAsync(App.XmasListBaseAddress + "/api/XmasList?owner=" + comboBoxItem.Content, content);

                if (response.IsSuccessStatusCode)
                {
                    GiftText.Text = "";
                    GetXmasList();
                }
                else
                {
                    var dialog =
                        new MessageDialog("Sorry, an error occurred accessing your Xmas list.  Please try again.");
                    await dialog.ShowAsync();
                }
            }
            else
                ShowError(new MessageDialog("Please select a list..."));
        }

        // Post a new item to the Xmas list.
        private async void Button_Click_Add_Gift(object sender, RoutedEventArgs e)
        {
            if (OwnerDropDown.SelectedValue == null)
            {
                var dialog =
                    new MessageDialog("Please select a list...");
                await dialog.ShowAsync();
            }
            else
                AddGift();
        }

        private void OwnerDropDown_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetXmasList();
        }
    }
}
