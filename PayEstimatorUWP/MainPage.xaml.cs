using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Windows.ApplicationModel.Appointments;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PayEstimatorUWP
{
    static class PayRates
    {
        public static float LEVEL3A_RATE = 24.08f;
        public static float LEVEL3B_RATE = 34.20f;
        public static float LEVEL3SUN_RATE = 43.35f;
        public static float VANDVRA_RATE = 25f;
        public static float VANDVRB_RATE = 35f;
        public static float VANDVRSUN_RATE = 44f;
        public static float MRHRAA_RATE = 26f;
        public static float MRHRAB_RATE = 35f;
        public static float MRHRASUN_RATE = 44f;
    }

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            Initialize();
            InitializeComponent();
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

            var date = currentPayStart;
            var timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            var startTime = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, timeZoneOffset);

            TimeSpan duration = TimeSpan.FromDays(7);

            FindAppointmentsOptions options = new FindAppointmentsOptions();
            options.MaxCount = 100;
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

                var i = 0;

                while (i < appointments.Count)
                {
                    if (!appointments[i].AllDay)
                    {
                        try
                        {
                            if (appointments[i].Details.Contains("CrewOnCall::LEVEL3"))
                                gigsthispay.Add(new Gig(appointments[i].StartTime, appointments[i].Duration, appointments[i].Subject, appointments[i].Location, "LEVEL3"));
                            if (appointments[i].Details.Contains("CrewOnCall::VANDVR"))
                                gigsthispay.Add(new Gig(appointments[i].StartTime, appointments[i].Duration, appointments[i].Subject, appointments[i].Location, "VANDVR"));
                            if (appointments[i].Details.Contains("CrewOnCall::MR/HR"))
                                gigsthispay.Add(new Gig(appointments[i].StartTime, appointments[i].Duration, appointments[i].Subject, appointments[i].Location, "MR/HR"));
                        }
                        catch (ArgumentOutOfRangeException)
                        {

                        }
                    }
                    i++;
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

            var date = nextPayStart;
            var timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            var startTime = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, timeZoneOffset);

            TimeSpan duration = TimeSpan.FromDays(7);

            FindAppointmentsOptions options = new FindAppointmentsOptions();
            options.MaxCount = 100;
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

                var i = 0;

                while (i < appointments.Count)
                {
                    if (!appointments[i].AllDay)
                    {
                        try
                        {
                            if (appointments[i].Details.Contains("CrewOnCall::LEVEL3"))
                                gigsnextpay.Add(new Gig(appointments[i].StartTime, appointments[i].Duration, appointments[i].Subject, appointments[i].Location, "LEVEL3"));
                            if (appointments[i].Details.Contains("CrewOnCall::VANDVR"))
                                gigsnextpay.Add(new Gig(appointments[i].StartTime, appointments[i].Duration, appointments[i].Subject, appointments[i].Location, "VANDVR"));
                            if (appointments[i].Details.Contains("CrewOnCall::MR/HR"))
                                gigsnextpay.Add(new Gig(appointments[i].StartTime, appointments[i].Duration, appointments[i].Subject, appointments[i].Location, "MR/HR"));
                        }
                        catch (ArgumentOutOfRangeException)
                        {

                        }
                    }
                    i++;
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
    }

    public class Gig
    {
        public string StartDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ClientName { get; set; }
        public string VenueName { get; set; }
        public double Hours { get; set; }
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

        public Gig(DateTimeOffset startTime, TimeSpan duration, string subject, string location, string skill)
        {
            StartDate = startTime.ToString("D");
            StartTime = startTime.ToString("t");
            EndTime = startTime.Add(duration).ToString("t");
            ClientName = subject;
            VenueName = location;
            Hours = 0;
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

            var span = new TimeSpan();
            var calcHours = new DateTimeOffset();

            span = duration;
            calcHours = startTime;

            if ((span < new TimeSpan(3, 0, 0)) && (startTime.DayOfWeek != DayOfWeek.Sunday))
            {
                duration = new TimeSpan(3, 0, 0); //min call9
            }

            if ((span < new TimeSpan(4, 0, 0)) && (startTime.DayOfWeek == DayOfWeek.Sunday))
            {
                duration = new TimeSpan(4, 0, 0); //min call (Sun)
            }

            if ((span > new TimeSpan(5, 0, 0)))
                duration = duration.Subtract(new TimeSpan(0, 30, 0)); //meal break after 5 hours

            while (calcHours < startTime.Add(duration))
            {
                if ((calcHours.Hour > 7) && (calcHours.Hour < 20) && (calcHours.DayOfWeek != DayOfWeek.Sunday))
                {
                    Hours += 0.25;
                    if ((Skill == null) || (Skill == "LEVEL3"))
                        LEVEL3A += 0.25;
                    if (Skill == "VANDVR")
                        VANDVRA += 0.25;
                    if (Skill == "MR/HR")
                        MRHRA += 0.25;
                }
                if (((calcHours.Hour < 8) || (calcHours.Hour > 19)) && (calcHours.DayOfWeek != DayOfWeek.Sunday))
                {
                    Hours += 0.25;
                    if ((Skill == null) || (Skill == "LEVEL3"))
                        LEVEL3B += 0.25;
                    if (Skill == "VANDVR")
                        VANDVRB += 0.25;
                    if (Skill == "MR/HR")
                        MRHRB += 0.25;
                }
                if (calcHours.DayOfWeek == DayOfWeek.Sunday)
                {
                    Hours += 0.25;
                    if ((Skill == null) || (Skill == "LEVEL3"))
                        LEVEL3SUN += 0.25;
                    if (Skill == "VANDVR")
                        VANDVRSUN += 0.25;
                    if (Skill == "MR/HR")
                        MRHRSUN += 0.25;
                }
                calcHours = calcHours.AddMinutes(15);
            }
        }
    }

    public class Shifts : INotifyPropertyChanged
    {
        private double _shifts;

        public double TotalShifts
        {
            get { return _shifts; }
            set
            {
                _shifts = value;
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
        private double _totalhours;

        public double TotalHours
        {
            get { return _totalhours; }
            set
            {
                _totalhours = value;
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

        private double _grossAmount;

        public double GrossAmount
        {
            get { return _grossAmount; }
            set
            {
                _grossAmount = value;
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
            GrossAmount += LEVEL3A * PayRates.LEVEL3A_RATE + LEVEL3B * PayRates.LEVEL3B_RATE + LEVEL3SUN * PayRates.LEVEL3SUN_RATE;
            GrossAmount += VANDVRA * PayRates.VANDVRA_RATE + VANDVRB * PayRates.VANDVRB_RATE + VANDVRSUN * PayRates.VANDVRSUN_RATE;
            GrossAmount += MRHRA * PayRates.MRHRAA_RATE + MRHRB * PayRates.MRHRAB_RATE + MRHRSUN * PayRates.MRHRASUN_RATE;
        }
    }

    public class Net : INotifyPropertyChanged
    {
        public double _grossAmount { get; private set; }
        public double _taxAmount { get; private set; }
        public double _HELPAmount { get; private set; }
        private double _netAmount;

        public double NetAmount
        {
            get { return _netAmount; }
            set
            {
                _netAmount = value;
                OnPropertyChanged("NetAmount");
            }
        }

        public bool TaxFreeThresholdClaimed { get; private set; }
        public bool HECSLiability { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public Net(double gross)
        {
            _grossAmount = 0;
            _taxAmount = 0;
            _netAmount = 0;
            _HELPAmount = 0;
            TaxFreeThresholdClaimed = false;
            HECSLiability = true;

            _grossAmount = Math.Floor(gross);

            //PAYG Tax withheld no tax free threashould claimed
            if (!TaxFreeThresholdClaimed && (_grossAmount < 60))
                _taxAmount = (0.19 * (_grossAmount + 0.99)) - 0.19;
            else if (!TaxFreeThresholdClaimed && (_grossAmount < 361))
                _taxAmount = (0.2332 * (_grossAmount + 0.99)) - 2.6045;
            else if (!TaxFreeThresholdClaimed && (_grossAmount < 932))
                _taxAmount = (0.3477 * (_grossAmount + 0.99)) - 44.0006;
            else if (!TaxFreeThresholdClaimed && (_grossAmount < 1323))
                _taxAmount = (0.345 * (_grossAmount + 0.99)) - 41.4841;
            else if (!TaxFreeThresholdClaimed && (_grossAmount < 3111))
                _taxAmount = (0.39 * (_grossAmount + 0.99)) - 101.0225;
            else
                _taxAmount = (0.49 * (_grossAmount + 0.99)) - 412.1764;

            _taxAmount = Math.Round(_taxAmount, 2);

            //PAYG Tax withheld HELP/SSL/TSL & SFSS debt
            if (HECSLiability &&(_grossAmount < 705))
                _HELPAmount = 0;
            else if (HECSLiability && (_grossAmount < 825))
                _HELPAmount = _grossAmount * 0.04;
            else if (HECSLiability && (_grossAmount < 945))
                _HELPAmount = _grossAmount * 0.045;
            else if (HECSLiability && (_grossAmount < 1013))
                _HELPAmount = _grossAmount * 0.05;
            else if (HECSLiability && (_grossAmount < 1115))
                _HELPAmount = _grossAmount * 0.055;
            else if (HECSLiability && (_grossAmount < 1237))
                _HELPAmount = _grossAmount * 0.06;
            else if (HECSLiability && (_grossAmount < 1321))
                _HELPAmount = _grossAmount * 0.065;
            else if (HECSLiability && (_grossAmount < 1489))
                _HELPAmount = _grossAmount * 0.07;
            else if (HECSLiability && (_grossAmount < 1609))
                _HELPAmount = _grossAmount * 0.075;
            else
                _HELPAmount = _grossAmount * 0.08;

            _HELPAmount = Math.Round(_HELPAmount, 2);

            NetAmount = Math.Ceiling(_grossAmount - _taxAmount - _HELPAmount);
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class DoubleToCurrencyFormatStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double d = 0;

            if (value == null)
                return null;

            double.TryParse(value.ToString(), out d);

            string s = d.ToString("C");
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

