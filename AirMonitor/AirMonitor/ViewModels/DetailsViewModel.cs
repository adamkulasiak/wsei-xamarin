using AirMonitor.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace AirMonitor.ViewModels
{
    public class DetailsViewModel: BaseViewModel
    {
        private readonly INavigation _navigation;
        public DetailsViewModel()
        {

        }
        public DetailsViewModel(INavigation navigation, Installation installation)
        {
            _navigation = navigation;
            _installation = installation;
            _caqiValue = Convert.ToInt32(installation.MeasurementItem.Indexes[0].Value);
            _pm25Value = Convert.ToInt32(installation.MeasurementItem.Values.FirstOrDefault(x => x.Name == "PM25").Value);
        }

        private Installation _installation;

        private int _caqiValue;
        public int CaqiValue
        {
            get => _caqiValue;
            set => SetProperty(ref _caqiValue, value);
        }

        private int _pm25Value = 10;
        public int Pm25Value
        {
            get => _pm25Value;
            set => SetProperty(ref _pm25Value, value);
        }

    }
}
