using System;
using System.Text;

namespace EgoErpArchiver.ViewModel
{
    public class ProgressDialogViewModel : ViewModelBase
    {
        private readonly StringBuilder _stringBuilder;
        private int _percentage;
        private int _percentageMax;
        private string _percentageText;
        private string _status;

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
                OnPropertyChanged(nameof(Percentage));
            }
        }

        public string PercentageText
        {
            get { return _percentageText; }
            set
            {
                _percentageText = value;
                OnPropertyChanged(nameof(PercentageText));
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
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
                OnPropertyChanged(nameof(PercentageMax));
            }
        }

        public IProgress<int> ProgressPercentage { get; }

        public IProgress<string> ProgressStatus { get; }

        public ProgressDialogViewModel()
        {
            _stringBuilder = new StringBuilder();
            ProgressPercentage = new Progress<int>(percentage => Percentage = percentage);
            ProgressStatus = new Progress<string>(status => 
            {
                _stringBuilder.Append(status);
                Status = _stringBuilder.ToString();
            });
        }
    }
}
