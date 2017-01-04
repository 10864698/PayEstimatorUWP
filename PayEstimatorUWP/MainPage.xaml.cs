using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Appointments;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Outlook = Windows.ApplicationModel.Appointments;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PayEstimatorUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            Initialize();
            InitializeComponent();
            Test test = new Test();
            tbTest.DataContext = test;

        }

        public void Initialize()
        {
            GigsLastWeek();
        }

        public async void GigsLastWeek()
        {
            AppointmentStore appointmentStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);

            //DayOfWeek weekStart = DayOfWeek.Monday;
            //DateTime startingDate = DateTime.Today;

            //while (startingDate.DayOfWeek != weekStart)
            //    startingDate = startingDate.AddDays(-1);

            //DateTime previousWeekStart = startingDate.AddDays(-7);
            //DateTime previousWeekEnd = startingDate.AddDays(0);

            //var appCalendars = await appointmentStore.FindAppointmentsAsync(previousWeekStart, TimeSpan.FromDays(7));

            //if (appCalendars != null)
            //{
            //    List<Gig> gigs = new List<Gig>();

            //    foreach (var appt in appCalendars)
            //    {
            //        try
            //        {
            //            if (appt.Details.Substring(0, 18) == "CrewOnCall::LEVEL3")
            //                gigs.Add(new Gig(appt.StartTime, appt.Duration, appt.Subject, appt.Location, "LEVEL3"));
            //            else if (appt.Details.Substring(0, 18) == "CrewOnCall::VANDVR")
            //                gigs.Add(new Gig(appt.StartTime, appt.Duration, appt.Subject, appt.Location, "VANDVR"));
            //            else if (appt.Details.Substring(0, 17) == "CrewOnCall::MR/HR")
            //                gigs.Add(new Gig(appt.StartTime, appt.Duration, appt.Subject, appt.Location, "MR/HR"));
            //            else
            //                gigs.Add(new Gig(appt.StartTime, appt.Duration, appt.Subject, appt.Location, "LEVEL3"));
            //        }
            //        catch (ArgumentOutOfRangeException)
            //        {
            //            gigs.Add(new Gig(appt.StartTime, appt.Duration, appt.Subject, appt.Location, "LEVEL3"));
            //        }
            //    }

            //    lvGigs.ItemsSource = gigs;

            //    Hours hours = new Hours(gigs);
            //    tbHours.DataContext = hours;

            //    Gross gross = new Gross(gigs);
            //    tbGross.DataContext = gross;
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

        public Gig(DateTimeOffset startTime, TimeSpan duration, string subject, string location, string v)
        {
            StartDate = startTime.ToString("D");
            StartTime = startTime.ToString("t");
            EndTime = startTime.Add(duration).ToString("t");
            ClientName = subject;
            VenueName = location;
            Hours = 0;
            Skill = v;
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
                duration = new TimeSpan(3, 0, 0);
            }

            if ((span < new TimeSpan(4, 0, 0)) && (startTime.DayOfWeek == DayOfWeek.Sunday))
            {
                duration = new TimeSpan(4, 0, 0);
            }

            while (calcHours < startTime.Add(duration))
            {
                //MessageBox.Show(calcHours.ToString() + "\n" + LEVEL3.ToString());
                if ((calcHours.Hour > 7) && (calcHours.Hour < 20) && (calcHours.DayOfWeek != DayOfWeek.Sunday))
                {
                    Hours += 0.25;
                    if ((v == null) || (v == "LEVEL3"))
                        LEVEL3A += 0.25;
                    if (v == "VANDVR")
                        VANDVRA += 0.25;
                    if (v == "MR/HR")
                        MRHRA += 0.25;
                }
                if (((calcHours.Hour < 8) || (calcHours.Hour > 19)) && (calcHours.DayOfWeek != DayOfWeek.Sunday))
                {
                    Hours += 0.25;
                    if ((v == null) || (v == "LEVEL3"))
                        LEVEL3B += 0.25;
                    if (v == "VANDVR")
                        VANDVRB += 0.25;
                    if (v == "MR/HR")
                        MRHRB += 0.25;
                }
                if (calcHours.DayOfWeek == DayOfWeek.Sunday)
                {
                    Hours += 0.25;
                    if ((v == null) || (v == "LEVEL3"))
                        LEVEL3SUN += 0.25;
                    if (v == "VANDVR")
                        VANDVRSUN += 0.25;
                    if (v == "MR/HR")
                        MRHRSUN += 0.25;
                }
                calcHours = calcHours.AddMinutes(15);
            }
        }
    }

    public class Test : INotifyPropertyChanged
    {
        private double _test;

        public double TotalTest
        {
            get { return _test; }
            set
            {
                _test = value;
                OnPropertyChanged("TotalHours");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public Test()
        {
            TotalTest = 0.01;
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
        public double LEVEL3A { get; set; }
        public double LEVEL3B { get; set; }
        public double LEVEL3SUN { get; set; }
        public double VANDVRA { get; set; }
        public double VANDVRB { get; set; }
        public double VANDVRSUN { get; set; }
        public double MRHRA { get; set; }
        public double MRHRB { get; set; }
        public double MRHRSUN { get; set; }

        private double _grossAmount;

        public double GrossAmount
        {
            get { return _grossAmount; }
            set
            {
                _grossAmount = value;
                OnPropertyChanged("TotalHours");
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
            GrossAmount += LEVEL3A * 24.08 + LEVEL3B * 34.20 + LEVEL3SUN * 43.35;
            GrossAmount += VANDVRA * 25 + VANDVRB * 35 + VANDVRSUN * 44;
            GrossAmount += MRHRA * 26 + MRHRB * 35 + MRHRSUN * 44;
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
}

