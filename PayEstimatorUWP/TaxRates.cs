// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

using System.Collections.Generic;
using System.ComponentModel;
using Windows.Storage;

namespace PayEstimatorUWP
{
    public class TaxRates
    {
        public TaxFreeThreshold TaxFreeThreshould = new TaxFreeThreshold();
        public HELPLiability HELPLiability = new HELPLiability();

        //Where the tax-free threshold is not claimed in Tax file number declaration – Scale 1
        public List<Coefficients> Schedule1Scale1 = new List<Coefficients>()
        {
            new Coefficients(88,    0.19,   0.19),
            new Coefficients(371,   0.2348, 3.9639),
            new Coefficients(515,   0.2190, -1.9003),
            new Coefficients(932,   0.3477, 64.4297),
            new Coefficients(1957,  0.3450, 61.9132),
            new Coefficients(3111,  0.3900, 150.0093),
            new Coefficients(3111,  0.4700, 398.9324)
        };

        //Where the employee claimed the tax-free threshold in Tax file number declaration – Scale 2
        public List<Coefficients> Schedule1Scale2 = new List<Coefficients>()
        {
            new Coefficients(359,   0,      0),
            new Coefficients(438,   0.1900, 68.3462),
            new Coefficients(548,   0.2900, 112.1942),
            new Coefficients(721,   0.2100, 68.3465),
            new Coefficients(865,   0.2190, 74.8369),
            new Coefficients(1282,  0.3477, 186.2119),
            new Coefficients(2307,  0.3450, 182.7504),
            new Coefficients(3461,  0.3900, 286.5965),
            new Coefficients(3461,  0.4700, 563.5196)
        };

        //Foreign residents – Scale 3
        readonly List<Coefficients> Schedule1Scale3 = new List<Coefficients>();

        //Where a tax file number(TFN) was not provided by employee – Scale 4
        readonly List<Coefficients> Schedule1Scale4 = new List<Coefficients>();

        //HELP/SSL/TSL Tax-free threshold claimed or foreign resident - Scale 2
        public List<Coefficients> Schedule8Scale1 = new List<Coefficients>()
        {
            new Coefficients(88,    0.19,   0.19),
            new Coefficients(371,   0.2348, 3.9639),
            new Coefficients(515,   0.219,  -1.9003),
            new Coefficients(546,   0.3477, 64.4297),
            new Coefficients(685,   0.3577, 64.4297),
            new Coefficients(747,   0.3677, 64.4297),
            new Coefficients(813,   0.3727, 64.4297),
            new Coefficients(882,   0.3777, 64.4297),
            new Coefficients(932,   0.3827, 64.4297),
            new Coefficients(956,   0.38,   61.9132),
            new Coefficients(1035,  0.385,  61.9132),
            new Coefficients(1118,  0.39,   61.9132),
            new Coefficients(1206,  0.395,  61.9132),
            new Coefficients(1299,  0.4,    61.9132),
            new Coefficients(1398,  0.405,  61.9132),
            new Coefficients(1503,  0.41,   61.9132),
            new Coefficients(1615,  0.415,  61.9132),
            new Coefficients(1732,  0.42,   61.9132),
            new Coefficients(1855,  0.425,  61.9132),
            new Coefficients(1957,  0.43,   61.9132),
            new Coefficients(1990,  0.475,  150.0093),
            new Coefficients(2130,  0.48,   150.0093),
            new Coefficients(2279,  0.485,  150.0093),
            new Coefficients(3111,  0.49,   150.0093),
            new Coefficients(3111,  0.57,   398.9324)
        };

        //HELP/SSL/TSL No tax-free threshold claimed - Scale 1
        public List<Coefficients> Schedule8Scale2 = new List<Coefficients>()
        {
            new Coefficients(359,   0,  0),
            new Coefficients(438,   0.19,   68.3462),
            new Coefficients(548,   0.29,   112.1942),
            new Coefficients(721,   0.21,   68.3465),
            new Coefficients(865,   0.219,  74.8369),
            new Coefficients(896,   0.3477, 186.2119),
            new Coefficients(1035,  0.3577, 186.2119),
            new Coefficients(1097,  0.3677, 186.2119),
            new Coefficients(1163,  0.3727, 186.2119),
            new Coefficients(1232,  0.3777, 186.2119),
            new Coefficients(1282,  0.3827, 186.2119),
            new Coefficients(1306,  0.38,   182.7504),
            new Coefficients(1385,  0.385,  182.7504),
            new Coefficients(1468,  0.39,   182.7504),
            new Coefficients(1556,  0.395,  182.7504),
            new Coefficients(1649,  0.4,    182.7504),
            new Coefficients(1748,  0.405,  182.7504),
            new Coefficients(1853,  0.41,   182.7504),
            new Coefficients(1965,  0.415,  182.7504),
            new Coefficients(2082,  0.42,   182.7504),
            new Coefficients(2205,  0.425,  182.7504),
            new Coefficients(2307,  0.43,   182.7504),
            new Coefficients(2340,  0.475,  286.5965),
            new Coefficients(2480,  0.48,   286.5965),
            new Coefficients(2629,  0.485,  286.5965),
            new Coefficients(3461,  0.49,   286.5965),
            new Coefficients(3461,  0.57,   563.5196)
        };

        //SFSS Tax-free threshold claimed or foreign resident
        readonly List<Coefficients> Schedule8Scale3 = new List<Coefficients>();

        //SFSS No tax-free threshold claimed
        readonly List<Coefficients> Schedule8Scale4 = new List<Coefficients>();

        public Coefficients GetCoefficients(List<Coefficients> listOfCoefficients, double grossAmount)
        {
            Coefficients selectedCoefficients = new Coefficients();

            if (grossAmount >= listOfCoefficients[listOfCoefficients.Count - 1].y)
                return listOfCoefficients[listOfCoefficients.Count - 1];

            else if (listOfCoefficients.Count > 1)
            {
                for (int i = 0; i < listOfCoefficients.Count; i++)
                    if (grossAmount < listOfCoefficients[i].y)
                        return listOfCoefficients[i];
            }
            return selectedCoefficients;
        }
    }

    public class Coefficients
    {
        public double y = 0;
        public double a = 0;
        public double b = 0;

        public Coefficients()
        {
            y = 0;
            a = 0;
            b = 0;
        }

        public Coefficients(double yCoefficient, double aCoeficient, double bCoefficient)
        {
            y = yCoefficient;
            a = aCoeficient;
            b = bCoefficient;
        }
    }

    public class TaxFreeThreshold : INotifyPropertyChanged
    {
        public ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        private bool _taxFreeThresholdClaimed;

        public TaxFreeThreshold()
        {
            if (localSettings.Values["taxFreeThreshold"] != null)
            {
                _taxFreeThresholdClaimed = (bool)localSettings.Values["taxFreeThreshold"];
            }
            else
            {
                _taxFreeThresholdClaimed = false;
            }
        }

        public bool TaxFreeThresholdClaimed
        {
            get { return _taxFreeThresholdClaimed; }
            set
            {
                _taxFreeThresholdClaimed = value;
                localSettings.Values["taxFreeThreshold"] = value;


                OnPropertyChanged("taxFreeThreshold");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }

    public class HELPLiability : INotifyPropertyChanged
    {
        public ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        private bool _hasHELPLiability;

        public HELPLiability()
        {
            if (localSettings.Values["HELPLiability"] != null)
            {
                _hasHELPLiability = (bool)localSettings.Values["HELPLiability"];
            }
            else
            {
                _hasHELPLiability = false;
            }
        }

        public bool HasHELPLiability
        {
            get { return _hasHELPLiability; }
            set
            {
                _hasHELPLiability = value;
                localSettings.Values["HELPLiability"] = value;
                OnPropertyChanged("HELPLiability");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
