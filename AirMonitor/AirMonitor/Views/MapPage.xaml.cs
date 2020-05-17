using AirMonitor.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AirMonitor.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        public MapPage()
        {
            InitializeComponent();
            BindingContext = new HomeViewModel(Navigation);
        }

        private void Pin_InfoWindowClicked(object sender, Xamarin.Forms.Maps.PinClickedEventArgs e)
        {
            var address = (sender as Xamarin.Forms.Maps.Pin).Address;
            var context = BindingContext as HomeViewModel;
            context.GoToDetailsCommandFromMap.Execute(address);
        }
    }
}