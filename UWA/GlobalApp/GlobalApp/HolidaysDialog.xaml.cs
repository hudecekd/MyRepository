using AlarmLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GlobalApp
{
    public sealed partial class HolidaysDialog : ContentDialog
    {
        public ObservableCollection<HolidayVM> Holidays { get; private set; } = new ObservableCollection<HolidayVM>();

        public HolidaysDialog()
        {
            this.InitializeComponent();

            DataContext = this;
        }

        private void GetHolidays(bool refreshCache = false)
        {
            Holidays.Clear();
            foreach (var holiday in BaseAlarmSettings.Instance.GetHolidays())
            {
                var holidayVM = new HolidayVM();
                holidayVM.Date = holiday.Date;
                holidayVM.LocalDescription = holiday.LocalDescription;
                holidayVM.EnglishDescription = holiday.EnglishDescription;
                Holidays.Add(holidayVM);
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            GetHolidays();
        }
    }
}
