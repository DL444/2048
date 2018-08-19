using Lib2048;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Game2048
{
    class ToolsWindowViewModel : INotifyPropertyChanged
    {
        private int _row = 1;
        private int _column = 1;
        private int _value = 2;
        private int _size = 4;
        private ToolsMode _mode = ToolsMode.Bomb;
        private int _cost = 0;

        private ItemBoard brd;

        public ToolsWindowViewModel() { }
        public ToolsWindowViewModel(int size, ToolsMode mode, ItemBoard brd)
        {
            Size = size;
            Mode = mode;
            this.brd = brd;
            Value = brd[0, 0];
        }

        public int Row
        {
            get => _row;
            set
            {
                _row = value;
                if(Mode != ToolsMode.Wildcard)
                {
                    Value = ValueSource.First(x => x == brd[Row - 1, Column - 1]);
                    //Value = brd[Row - 1, Column - 1];
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Row"));
            }
        }
        public int Column
        {
            get => _column;
            set
            {
                _column = value;
                if (Mode != ToolsMode.Wildcard)
                {
                    Value = ValueSource.First(x => x == brd[Row - 1, Column - 1]);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Column"));
            }
        }
        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
                if(Mode == ToolsMode.Bomb)
                {
                    Cost = 256 * (value == 0 ? 0 : 1);
                }
                else
                {
                    Cost = value > 128 ? value : 128;
                }
            }
        }
        public int Size
        {
            get => _size;
            set
            {
                _size = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Size"));
            }
        }
        public int Cost
        {
            get => _cost;
            private set
            {
                _cost = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Cost"));
            }
        }
        public ToolsMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Mode"));
            }
        }
        public int[] ValueSource { get; } = GetValueItems();

        public event PropertyChangedEventHandler PropertyChanged;

        bool CanOkEx()
        {
            if(brd == null) { return false; }
            bool blockExist = brd[Row - 1, Column - 1] != 0;
            if(Mode != ToolsMode.Wildcard) { return blockExist; }
            if(Value != 0) { return !blockExist; }
            return false;
            //return Mode == ToolsMode.Wildcard ? !blockExist : blockExist;
        }
        void OkEx() { }
        public ICommand Ok { get => new MicroMvvm.RelayCommand(OkEx, CanOkEx); }

        static int[] GetValueItems()
        {
            List<int> items = new List<int>();
            items.Add(0);
            for(int i = 1; i < 31; i++)
            {
                items.Add((int)Math.Pow(2, i));
            }
            return items.ToArray();
        }
    }

    enum ToolsMode
    {
        Bomb, Wildcard, Promote
    }

    public class ValueSelectorEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is ToolsMode mode)
            {
                if(mode == ToolsMode.Wildcard) { return true; }
                return false;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
