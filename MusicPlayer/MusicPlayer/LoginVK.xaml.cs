using System.Net.NetworkInformation;
using MusicPlayer.domain.vk;

namespace MusicPlayer
{
    /// <summary>
    /// Логика взаимодействия для LoginVK.xaml
    /// </summary>
    public partial class LoginVk
    {
        public string AccessToken = "";
        public LoginVk()
        {
            try
            {
                VkHelper.CheckEthernet();
                InitializeComponent();
                wb.ScriptErrorsSuppressed = true;
                wb.Navigate(
                    "https://oauth.vk.com/authorize?client_id=3987742&scope=2080255&redirect_uri=https://oauth.vk.com/blank.html&display=page&response_type=token");
            }
            catch (NetworkInformationException)
            {
                System.Windows.Forms.MessageBox.Show(@"Ошибка интернет соединения.");
            }
        }

        private void WebBrowser_Navigated(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e)
        {
            try
            {
                VkHelper.CheckEthernet();
                if (!e.Url.ToString().StartsWith("https://oauth.vk.com/blank.html")) return;
                AccessToken = e.Url.Fragment.Split('&')[0].Replace("#access_token=", "");
                Close();
            }
            catch (NetworkInformationException)
            {
                System.Windows.Forms.MessageBox.Show(@"Ошибка интернет соединения.");
            }
        }
    }
}
