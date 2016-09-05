using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoErpArchiver.ViewModel
{
    public class ProgressDialogViewModel : ViewModelBase
    {
        readonly StringBuilder _stringBuilder;
        int _percentage;
        int _percentageMax;
        string _percentageText;
        string _status;

        public override string DisplayName
        {
            get
            {
                return "Progress Dialog";
            }

            protected set
            {
                base.DisplayName = value;
            }
        }

        public int Percentage
        {
            get { return _percentage; }
            set
            {
                _percentage = value;
                PercentageText = (value * 100 / _percentageMax) + "%";
                OnPropertyChanged("Percentage");
            }
        }

        public string PercentageText
        {
            get { return _percentageText; }
            set
            {
                _percentageText = value;
                OnPropertyChanged("PercentageText");
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public int PercentageMax
        {
            get
            {
                return _percentageMax;
            }

            set
            {
                _percentageMax = value;
                OnPropertyChanged("PercentageMax");
            }
        }

        public ProgressDialogViewModel(out Progress<int> progressPercentage, out Progress<string> progressStatus)
        {
            _stringBuilder = new StringBuilder();
            progressPercentage = new Progress<int>(percentage => Percentage = percentage);
            progressStatus = new Progress<string>(status => 
            {
                _stringBuilder.Append(status);
                Status = _stringBuilder.ToString();
            });
        }
    }
}
