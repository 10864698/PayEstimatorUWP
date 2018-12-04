// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

using System;
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
            new Coefficients(72,    0.19,   0.19),
            new Coefficients(361,   0.2342, 3.213),
            new Coefficients(932,   0.3477, 44.2476),
            new Coefficients(1380,  0.345,  41.7311),
            new Coefficients(3111,  0.39,   103.8657),
            new Coefficients(3111,  0.47,   352.7888)
        };

        //Where the employee claimed the tax-free threshold in Tax file number declaration – Scale 2
        public List<Coefficients> Schedule1Scale2 = new List<Coefficients>()
        {
            new Coefficients(355,   0,      0),
            new Coefficients(422,   0.1900, 67.4635),
            new Coefficients(528,   0.2900, 109.7327),
            new Coefficients(711,   0.2100, 67.4635),
            new Coefficients(1282,  0.3477, 165.4423),
            new Coefficients(1730,  0.3450, 161.9808),
            new Coefficients(3461,  0.3900, 239.8654),
            new Coefficients(3461,  0.4700, 516.7885)
        };

        //Foreign residents – Scale 3
        readonly List<Coefficients> Schedule1Scale3 = new List<Coefficients>();

        //Where a tax file number(TFN) was not provided by employee – Scale 4
        readonly List<Coefficients> Schedule1Scale4 = new List<Coefficients>();

        //HELP/SSL/TSL Tax-free threshold claimed or foreign resident
        public List<Coefficients> Schedule8Scale1 = new List<Coefficients>()
        {
            new Coefficients(72,    0.1900, 0.1900),
            new Coefficients(361,   0.2342, 3.2130),
            new Coefficients(649,   0.3477, 44.2476),
            new Coefficients(760,   0.3677, 44.2476),
            new Coefficients(886,   0.3877, 44.2476),
            new Coefficients(932,   0.3927, 44.2476),
            new Coefficients(1013,  0.3900, 41.7311),
            new Coefficients(1084,  0.3950, 41.7311),
            new Coefficients(1192,  0.4000, 41.7311),
            new Coefficients(1320,  0.4050, 41.7311),
            new Coefficients(1380,  0.4100, 41.7311),
            new Coefficients(1408,  0.4550, 103.8657),
            new Coefficients(1584,  0.4600, 103.8657),
            new Coefficients(1711,  0.4650, 103.8657),
            new Coefficients(3111,  0.4700, 103.8657),
            new Coefficients(3111,  0.5500, 352.7888)
        };

        //HELP/SSL/TSL No tax-free threshold claimed
        public List<Coefficients> Schedule8Scale2 = new List<Coefficients>()
        {
            new Coefficients(355,   0,      0),
            new Coefficients(422,   0.1900, 67.4635),
            new Coefficients(528,   0.2900, 109.7327),
            new Coefficients(711,   0.2100, 67.4635),
            new Coefficients(999,   0.3477, 165.4423),
            new Coefficients(1110,  0.3677, 165.4423),
            new Coefficients(1236,  0.3877, 165.4423),
            new Coefficients(1282,  0.3927, 165.4423),
            new Coefficients(1363,  0.3900, 161.9808),
            new Coefficients(1434,  0.3950, 161.9808),
            new Coefficients(1542,  0.4000, 161.9808),
            new Coefficients(1670,  0.4050, 161.9808),
            new Coefficients(1730,  0.4100, 161.9808),
            new Coefficients(1758,  0.4550, 239.8654),
            new Coefficients(1934,  0.4600, 239.8654),
            new Coefficients(2061,  0.4650, 239.8654),
            new Coefficients(3461,  0.4700, 239.8654),
            new Coefficients(3461,  0.5500, 516.7885)
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

    public class TaxFreeThreshold:INotifyPropertyChanged
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
