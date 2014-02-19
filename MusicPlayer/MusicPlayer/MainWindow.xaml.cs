using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKAudioPlayer.domain;


namespace MusicPlayer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var item = new TabItem{Header = "Test"};
            var bc = new BrushConverter();
            item.Foreground = (Brush)bc.ConvertFrom("White"); 
            PlayListTabs.Items.Add(item);
			TabItem t = (TabItem)PlayListTabs.Items[0];
			ListBox l = (ListBox)t.FindName("PlayList");
			l.Items.Add(new Audio("Ария","Ария - Я свободен","1.", "проал",true,"3:06"));
			
        }
             
    }
}
