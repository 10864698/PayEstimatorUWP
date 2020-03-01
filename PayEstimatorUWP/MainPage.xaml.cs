﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.ApplicationModel.Appointments;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PayEstimatorUWP
{
    public sealed partial class MainPage : Page
    {
        public ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public MainPage()
        {
            Initialize();
            InitializeComponent();

            toggleSwitchTaxFreeThresholdClaimed.IsOn = localSettings.Values["taxFreeThreshold"] != null ? (bool)localSettings.Values["taxFreeThreshold"] : false;
            toggleSwitchHasHELPLiability.IsOn = localSettings.Values["HELPLiability"] != null ? (bool)localSettings.Values["HELPLiability"] : false;
        }

        public void Initialize()
        {
            GigsThisPay();
            GigsNextPay();
        }

        public async void GigsThisPay()
        {
            AppointmentStore appointmentStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);

            DayOfWeek weekStart = DayOfWeek.Monday;
            DateTimeOffset startingDate = DateTimeOffset.Now;

            while (startingDate.DayOfWeek != weekStart)
                startingDate = startingDate.AddDays(-1);

            DateTimeOffset currentPayStart = startingDate.AddDays(-14);
            var timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            var startTime = new DateTimeOffset(currentPayStart.Year, currentPayStart.Month, currentPayStart.Day, 0, 0, 0, timeZoneOffset);

            TimeSpan duration = TimeSpan.FromDays(7);

            FindAppointmentsOptions options = new FindAppointmentsOptions
            {
                MaxCount = 100
            };
            options.FetchProperties.Add(AppointmentProperties.Subject);
            options.FetchProperties.Add(AppointmentProperties.Location);
            options.FetchProperties.Add(AppointmentProperties.AllDay);
            options.FetchProperties.Add(AppointmentProperties.StartTime);
            options.FetchProperties.Add(AppointmentProperties.Duration);
            options.FetchProperties.Add(AppointmentProperties.Details);
            options.FetchProperties.Add(AppointmentProperties.DetailsKind);

            IReadOnlyList<Appointment> appointments = await appointmentStore.FindAppointmentsAsync(startTime, duration, options);

            if (appointments.Count > 0)
            {
                List<Gig> gigsthispay = new List<Gig>();

                foreach (var appointment in appointments)
                {
                    if (!appointment.AllDay)
                    {
                        string MealBreak = null;
                        try
                        {
                            if (appointment.Details.Contains("::NOBREAK"))
                                MealBreak = "No break";
                            if (appointment.Details.Contains("CrewOnCall::LEVEL3") && (appointment.StartTime.Date != startTime.AddDays(-1).Date))
                                gigsthispay.Add(new Gig(appointment.StartTime, appointment.Duration, appointment.Subject, appointment.Location, "LEVEL3", MealBreak));
                            if (appointment.Details.Contains("CrewOnCall::VANDVR") && (appointment.StartTime.Date != startTime.AddDays(-1).Date))
                                gigsthispay.Add(new Gig(appointment.StartTime, appointment.Duration, appointment.Subject, appointment.Location, "VANDVR", MealBreak));
                            if (appointment.Details.Contains("CrewOnCall::MR/HR") && (appointment.StartTime.Date != startTime.AddDays(-1).Date))
                                gigsthispay.Add(new Gig(appointment.StartTime, appointment.Duration, appointment.Subject, appointment.Location, "MR/HR", MealBreak));
                        }
                        catch (ArgumentOutOfRangeException)
                        {

                        }
                    }
                }

                lvGigs.ItemsSource = gigsthispay;

                Hours hours = new Hours(gigsthispay);
                tbHours.DataContext = hours;

                Gross gross = new Gross(gigsthispay);
                tbGross.DataContext = gross;

                Net net = new Net(gross.GrossAmount);
                tbNet.DataContext = net;

                Shifts shifts = new Shifts(gigsthispay);
                tbShifts.DataContext = shifts;
            }
            else
            {
                List<Gig> gigsthispay = new List<Gig>();
                lvGigs.ItemsSource = gigsthispay;

                Hours hours = new Hours(gigsthispay);
                tbHours.DataContext = hours;

                Gross gross = new Gross(gigsthispay);
                tbGross.DataContext = gross;

                Net net = new Net(gross.GrossAmount);
                tbNet.DataContext = net;

                Shifts shifts = new Shifts(gigsthispay);
                tbShifts.DataContext = shifts;

            }
        }

        public async void GigsNextPay()
        {
            AppointmentStore appointmentStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);

            DayOfWeek weekStart = DayOfWeek.Monday;
            DateTimeOffset startingDate = DateTimeOffset.Now;

            while (startingDate.DayOfWeek != weekStart)
                startingDate = startingDate.AddDays(-1);

            DateTimeOffset nextPayStart = startingDate.AddDays(-7);
            var timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            var startTime = new DateTimeOffset(nextPayStart.Year, nextPayStart.Month, nextPayStart.Day, 0, 0, 0, timeZoneOffset);

            TimeSpan duration = TimeSpan.FromDays(7);

            FindAppointmentsOptions options = new FindAppointmentsOptions
            {
                MaxCount = 100
            };
            options.FetchProperties.Add(AppointmentProperties.Subject);
            options.FetchProperties.Add(AppointmentProperties.Location);
            options.FetchProperties.Add(AppointmentProperties.AllDay);
            options.FetchProperties.Add(AppointmentProperties.StartTime);
            options.FetchProperties.Add(AppointmentProperties.Duration);
            options.FetchProperties.Add(AppointmentProperties.Details);
            options.FetchProperties.Add(AppointmentProperties.DetailsKind);

            IReadOnlyList<Appointment> appointments = await appointmentStore.FindAppointmentsAsync(startTime, duration, options);

            if (appointments.Count > 0)
            {
                List<Gig> gigsnextpay = new List<Gig>();

                foreach (var appointment in appointments)
                {
                    if (!appointment.AllDay)
                    {
                        string MealBreak = null;
                        try
                        {
                            if (appointment.Details.Contains("::NOBREAK"))
                                MealBreak = "No break";
                            if (appointment.Details.Contains("CrewOnCall::LEVEL3") && (appointment.StartTime.Date != startTime.AddDays(-1).Date))
                                gigsnextpay.Add(new Gig(appointment.StartTime, appointment.Duration, appointment.Subject, appointment.Location, "LEVEL3", MealBreak));
                            if (appointment.Details.Contains("CrewOnCall::VANDVR") && (appointment.StartTime.Date != startTime.AddDays(-1).Date))
                                gigsnextpay.Add(new Gig(appointment.StartTime, appointment.Duration, appointment.Subject, appointment.Location, "VANDVR", MealBreak));
                            if (appointment.Details.Contains("CrewOnCall::MR/HR") && (appointment.StartTime.Date != startTime.AddDays(-1).Date))
                                gigsnextpay.Add(new Gig(appointment.StartTime, appointment.Duration, appointment.Subject, appointment.Location, "MR/HR", MealBreak));
                        }
                        catch (ArgumentOutOfRangeException)
                        {

                        }
                    }
                }

                lvnextGigs.ItemsSource = gigsnextpay;

                Hours hours = new Hours(gigsnextpay);
                tbnextHours.DataContext = hours;

                Gross gross = new Gross(gigsnextpay);
                tbnextGross.DataContext = gross;

                Net net = new Net(gross.GrossAmount);
                tbnextNet.DataContext = net;

                Shifts shifts = new Shifts(gigsnextpay);
                tbnextShifts.DataContext = shifts;
            }
            else
            {
                List<Gig> gigsnextpay = new List<Gig>();
                lvnextGigs.ItemsSource = gigsnextpay;

                Hours hours = new Hours(gigsnextpay);
                tbnextHours.DataContext = hours;

                Gross gross = new Gross(gigsnextpay);
                tbnextGross.DataContext = gross;

                Net net = new Net(gross.GrossAmount);
                tbnextNet.DataContext = net;

                Shifts shifts = new Shifts(gigsnextpay);
                tbnextShifts.DataContext = shifts;

            }
        }

        public void HELPLiabilityToggleSwitchToggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                if (toggleSwitch.IsOn == true)
                {
                    localSettings.Values["HELPLiability"] = true;
                    Initialize();
                }
                else
                {
                    localSettings.Values["HELPLiability"] = false;
                    Initialize();
                }
            }
        }

        public void TaxFreeThresholdToggleSwitchToggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                if (toggleSwitch.IsOn == true)
                {
                    localSettings.Values["taxFreeThreshold"] = true;
                    Initialize();
                }
                else
                {
                    localSettings.Values["taxFreeThreshold"] = false;
                    Initialize();
                }
            }
        }
    }

    public class Gig
    {
        public string StartDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ClientName { get; set; }
        public string VenueName { get; set; }
        public string PH { get; set; }
        public double Hours { get; set; }
        public string MealBreak { get; set; }
        public string Skill { get; set; }
        public double LEVEL3A { get; set; }
        public double LEVEL3B { get; set; }
        public double LEVEL3SUN { get; set; }
        public double VANDVRA { get; set; }
        public double VANDVRB { get; set; }
        public double VANDVRSUN { get; set; }
        public double MRHRA { get; set; }
        public double MRHRB { get; set; }
        public double MRHRSUN { get; set; }

        public Gig(DateTimeOffset startTime, TimeSpan duration, string subject, string location, string skill, string mealbreak)
        {
            StartDate = startTime.ToString("D");
            StartTime = startTime.ToString("t");
            EndTime = startTime.Add(duration).ToString("t");
            ClientName = subject;
            VenueName = location;
            PH = "";
            Hours = 0;
            MealBreak = mealbreak;
            Skill = skill;
            LEVEL3A = 0;
            LEVEL3B = 0;
            LEVEL3SUN = 0;
            VANDVRA = 0;
            VANDVRB = 0;
            VANDVRSUN = 0;
            MRHRA = 0;
            MRHRB = 0;
            MRHRSUN = 0;

            var publicHoliday = new PublicHolidays(startTime);
            if (publicHoliday.PublicHoliday == true)
                PH = " (Public Holiday)";

            var span = duration;
            var calcHours = startTime;

            if ((span < new TimeSpan(3, 0, 0)) && (startTime.DayOfWeek != DayOfWeek.Sunday))
            {
                {
                    duration = new TimeSpan(3, 0, 0); //min 3 hour call (M-Sat)
                    MealBreak = "No break";
                }
            }

            if ((span < new TimeSpan(4, 0, 0)) && (startTime.DayOfWeek == DayOfWeek.Sunday))
            {
                {
                    duration = new TimeSpan(4, 0, 0); //min 4 hour call (Sun)
                    MealBreak = "No break";
                }
            }

            if ((span < new TimeSpan(4, 0, 0)) && (publicHoliday.PublicHoliday))
            {
                {
                    duration = new TimeSpan(4, 0, 0); //min 4 hour call (PH)
                    MealBreak = "No break";
                }
            }

            if ((span > new TimeSpan(5, 30, 0)) && (MealBreak == null))
            {
                duration = duration.Subtract(new TimeSpan(0, 30, 0)); //meal break after 5.5 hours
                MealBreak = "30 minute break";
            }

            if ((span > new TimeSpan(11, 30, 0)) && (MealBreak == null))
            {
                duration = duration.Subtract(new TimeSpan(0, 60, 0)); //2 meal breaks after 12 hours
                MealBreak = "60 minute break";
            }

            if ((span > new TimeSpan(17, 0, 0)) && (MealBreak == null))
            {
                duration = duration.Subtract(new TimeSpan(0, 90, 0)); //3 meal breaks after 17 hours
                MealBreak = "90 minute break";
            }

            if (MealBreak == null)
                MealBreak = "No break";

            while (calcHours < startTime.Add(duration))
            {
                if ((calcHours.DayOfWeek == DayOfWeek.Sunday) || (publicHoliday.PublicHoliday == true) || (((startTime.DayOfWeek == DayOfWeek.Sunday) || publicHoliday.PublicHoliday == true) && calcHours < startTime.AddHours(4)))
                {
                    Hours += 0.25;
                    if ((Skill == null) || (Skill == "LEVEL3"))
                        LEVEL3SUN += 0.25;
                    if (Skill == "VANDVR")
                        VANDVRSUN += 0.25;
                    if (Skill == "MR/HR")
                        MRHRSUN += 0.25;
                }
                else if ((calcHours.Hour < 8) || (calcHours.Hour > 19))
                {
                    Hours += 0.25;
                    if ((Skill == null) || (Skill == "LEVEL3"))
                        LEVEL3B += 0.25;
                    if (Skill == "VANDVR")
                        VANDVRB += 0.25;
                    if (Skill == "MR/HR")
                        MRHRB += 0.25;
                }
                else
                {
                    Hours += 0.25;
                    if ((Skill == null) || (Skill == "LEVEL3"))
                        LEVEL3A += 0.25;
                    if (Skill == "VANDVR")
                        VANDVRA += 0.25;
                    if (Skill == "MR/HR")
                        MRHRA += 0.25;
                }


                calcHours = calcHours.AddMinutes(15);
            }
        }
    }

    public class Shifts : INotifyPropertyChanged
    {
        private double shifts;

        public double TotalShifts
        {
            get { return shifts; }
            set
            {
                shifts = value;
                OnPropertyChanged("TotalShifts");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public Shifts(List<Gig> gigs)
        {
            TotalShifts = gigs.Count;
        }
    }

    public class Hours : INotifyPropertyChanged
    {
        private double totalhours;

        public double TotalHours
        {
            get { return totalhours; }
            set
            {
                totalhours = value;
                OnPropertyChanged("TotalHours");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public Hours(List<Gig> gigs)
        {
            TotalHours = 0.00;

            foreach (Gig gig in gigs)
            {
                TotalHours += gig.Hours;
            }
        }
    }

    public class Gross : INotifyPropertyChanged
    {
        public double LEVEL3A { get; private set; }
        public double LEVEL3B { get; private set; }
        public double LEVEL3SUN { get; private set; }
        public double VANDVRA { get; private set; }
        public double VANDVRB { get; private set; }
        public double VANDVRSUN { get; private set; }
        public double MRHRA { get; private set; }
        public double MRHRB { get; private set; }
        public double MRHRSUN { get; private set; }

        private double grossAmount;

        public double GrossAmount
        {
            get { return grossAmount; }
            set
            {
                grossAmount = value;
                OnPropertyChanged("GrossAmount");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public Gross(List<Gig> gigs)
        {
            LEVEL3A = 0;
            LEVEL3B = 0;
            LEVEL3SUN = 0;
            VANDVRA = 0;
            VANDVRB = 0;
            VANDVRSUN = 0;
            MRHRA = 0;
            MRHRB = 0;
            MRHRSUN = 0;
            GrossAmount = 0;

            foreach (Gig gig in gigs)
            {
                LEVEL3A += gig.LEVEL3A;
                LEVEL3B += gig.LEVEL3B;
                LEVEL3SUN += gig.LEVEL3SUN;
                VANDVRA += gig.VANDVRA;
                VANDVRB += gig.VANDVRB;
                VANDVRSUN += gig.VANDVRSUN;
                MRHRA += gig.MRHRA;
                MRHRB += gig.MRHRB;
                MRHRSUN += gig.MRHRSUN;
            }
            GrossAmount += LEVEL3A * PayRates.LEVEL3ARATE + LEVEL3B * PayRates.LEVEL3BRATE + LEVEL3SUN * PayRates.LEVEL3SUNRATE;
            GrossAmount += VANDVRA * PayRates.VANDVRARATE + VANDVRB * PayRates.VANDVRBRATE + VANDVRSUN * PayRates.VANDVRSUNRATE;
            GrossAmount += MRHRA * PayRates.MRHRAARATE + MRHRB * PayRates.MRHRABRATE + MRHRSUN * PayRates.MRHRASUNRATE;
        }
    }

    public class Net : INotifyPropertyChanged
    {
        private double GrossAmount { get; set; }
        private double TaxAmount { get; set; }
        private double netAmount;
        readonly TaxRates taxRates = new TaxRates();

        public double NetAmount
        {
            get { return netAmount; }
            set
            {
                netAmount = value;
                OnPropertyChanged("NetAmount");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public Net(double gross)
        {
            GrossAmount = 0;
            TaxAmount = 0;
            NetAmount = 0;

            GrossAmount = Math.Floor(gross);
            
            if (!taxRates.TaxFreeThreshould.TaxFreeThresholdClaimed && !taxRates.HELPLiability.HasHELPLiability)
            {
                Coefficients coefficients = taxRates.GetCoefficients(taxRates.Schedule1Scale1, GrossAmount);
                TaxAmount = (coefficients.a * (GrossAmount + 0.99)) - coefficients.b;
            }
            if (taxRates.TaxFreeThreshould.TaxFreeThresholdClaimed && !taxRates.HELPLiability.HasHELPLiability)
            {
                Coefficients coefficients = taxRates.GetCoefficients(taxRates.Schedule1Scale2, GrossAmount);
                TaxAmount = coefficients.a * (GrossAmount + 0.99) - coefficients.b;
            }
            if (!taxRates.TaxFreeThreshould.TaxFreeThresholdClaimed && taxRates.HELPLiability.HasHELPLiability)
            {
                Coefficients coefficients = taxRates.GetCoefficients(taxRates.Schedule8Scale1, GrossAmount);
                TaxAmount = coefficients.a * (GrossAmount + 0.99) - coefficients.b;
            }
            if (taxRates.TaxFreeThreshould.TaxFreeThresholdClaimed && taxRates.HELPLiability.HasHELPLiability)
            {
                Coefficients coefficients = taxRates.GetCoefficients(taxRates.Schedule8Scale2, GrossAmount);
                TaxAmount = coefficients.a * (GrossAmount + 0.99) - coefficients.b;
            }

            NetAmount = Math.Ceiling(GrossAmount - TaxAmount);
        }
    }

    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            string s = value.ToString();
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        //public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    throw new NotSupportedException();
        //}
    }

    public class DoubleToCurrencyFormatStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            if (value == null)
                return null;

            double.TryParse(value.ToString(), out double d);

            string s = d.ToString("C");
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        //public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    throw new NotSupportedException();
        //}
    }
}
