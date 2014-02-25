namespace MusicPlayer
{
    /// <summary>
    /// Логика взаимодействия для LoginVK.xaml
    /// </summary>
    public partial class LoginVK
    {
        public string AccessToken = "";
        public LoginVK()
        {
            InitializeComponent();
            wb.ScriptErrorsSuppressed = true;
            wb.Navigate("https://oauth.vk.com/authorize?client_id=3987742&scope=2080255&redirect_uri=https://oauth.vk.com/blank.html&display=page&response_type=token");
        
        }

        private void WebBrowser_Navigated(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e)
        {
            if (!e.Url.ToString().StartsWith("https://oauth.vk.com/blank.html")) return;
            AccessToken = e.Url.Fragment.Split('&')[0].Replace("#access_token=", "");
            Close();
        }
    }
}
