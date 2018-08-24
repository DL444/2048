using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Game2048
{
    class HighscoreViewModel : INotifyPropertyChanged
    {
        public int Mode { get; set; } = 0;
        public int Size { get; set; } = 4;

        public MicroMvvm.RelayCommand RefreshListEx { get => new MicroMvvm.RelayCommand( async () => await RefreshList() ); }

        async Task RefreshList()
        {
            try
            {
                ParseString(await LoginClient2048.LoginClient.GetHighscore(Mode, Size));
            }
            catch(ArgumentException)
            {
                MessageBox.Show("Argument error. Please contact technical support.", "Argument error");
                return;
            }
            catch(Exception)
            {
                MessageBox.Show("Fetch highscore failed. Please check your Internet connection.", "Connection failed");
                return;
            }
        }

        public ObservableCollection<HighscoreEntryViewModel> Entries { get; set; } = new ObservableCollection<HighscoreEntryViewModel>();

        public event PropertyChangedEventHandler PropertyChanged;

        void ParseString(string response)
        {
            Entries.Clear();
            if (response == "[]") { return; }
            string[] items = response.Split(new[] { "},{" }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var item in items)
            {
                HighscoreEntryViewModel entry = new HighscoreEntryViewModel();
                string[] props = item.Split(',');
                foreach(var prop in props)
                {
                    string[] pair = prop.Split(':');
                    if(pair[0].IndexOf("score", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        entry.Score = long.Parse(pair[1]);
                    }
                    else if (pair[0].IndexOf("username", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        entry.Username = pair[1].Replace("\"", "");
                    }
                    else if(pair[0].IndexOf("time", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        entry.DateTime = new DateTime(long.Parse(pair[1].Replace("}", "").Replace("]", ""))).AddHours(8);
                    }
                }
                Entries.Add(entry);
            }
        }
    }

    class HighscoreEntryViewModel : INotifyPropertyChanged
    {
        string _username = "";
        long _score = 0;
        DateTime _dateTime = new DateTime();

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Username"));
            }
        }
        public long Score {
            get => _score;
            set
            {
                _score = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Score"));
            }
        }
        public DateTime DateTime {
            get => _dateTime;
            set
            {
                _dateTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DateTime"));
            }
        }

        public HighscoreEntryViewModel() { }
        public HighscoreEntryViewModel(string username, long score, DateTime dateTime)
        {
            _username = username;
            _score = score;
            _dateTime = dateTime;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
