using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Windows.Data.Json;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Microsoft.Identity.Client;

namespace XmasListClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private readonly HttpClient _httpClient = new HttpClient();

        private readonly PublicClientApplication _authContext;
        private readonly Uri _redirectUri;

        private User CurrentUser { get; set; }

        public MainPage()
        {
            InitializeComponent();

            _redirectUri = Windows.Security.Authentication.Web.WebAuthenticationBroker.GetCurrentApplicationCallbackUri();
            _authContext = new PublicClientApplication(App.ClientId);
        }

        private async void ShowError(MessageDialog dialog)
        {
            await dialog.ShowAsync();
        }

        private async void GetXmasList()
        {
            var response =
                await _httpClient.GetAsync(App.XmasListBaseAddress + "/api/XmasList");

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

        private async void AddGift()
        {
            HttpContent content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("Title", GiftText.Text) });

            // Call the XmasList web api
            var response = await _httpClient.PostAsync(App.XmasListBaseAddress + "/api/XmasList", content);

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

        // Post a new item to the Xmas list.
        private void Button_Click_Add_Gift(object sender, RoutedEventArgs e)
        {
            AddGift();
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser != null)
                SignOut();
            else
                SignIn();
        }

        private async void SignIn()
        {
            //
            // Use ADAL to get an access token to call the To Do list service.
            //
            AuthenticationResult result;
            try
            {
                result = await _authContext.AcquireTokenAsync(new[] {App.ClientId});
                CurrentUser = result.User;
            }
            catch (MsalException ex)
            {
                if (ex.ErrorCode == "authentication_canceled")
                {
                    // The user cancelled the sign-in, no need to display a message.
                }
                else
                {
                    MessageDialog dialog = new MessageDialog(string.Format("If the error continues, please contact your administrator.\n\nError Description:\n\n{0}", ex.Message), "Sorry, an error occurred while signing you in.");
                    ShowError(dialog);
                }
                return;
            }

            //
            // Add the access token to the Authorization Header of the call to the To Do list service, and call the service.
            //
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);

            OwnerTextBox.Text = CurrentUser.Name;

            GetXmasList();
            SignInButton.Content = "Sign Out...";
        }

        private void SignOut()
        {
            // Clear session state from the token cache.
            OwnerTextBox.Text = "Please sign in...";
            _authContext.UserTokenCache.Clear(App.ClientId);
            CurrentUser = null;

            // Reset UI elements
            XmasList.ItemsSource = null;
            GiftText.Text = "";

            SignInButton.Content = "Sign In...";
        }
    }
}
