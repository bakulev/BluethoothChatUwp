using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using BluethoothChatUwp.Core;
using BluethoothChatUwp.Models;
using SimpleMvvm;
using SimpleMvvm.Commands;
using SimpleMvvm.Common;

namespace BluethoothChatUwp.ViewModels
{
    internal class MainPageViewModel
        : ViewModel
    {
        private readonly IModel _model;

        public MainPageViewModel(IModel model)
		{
            Guard.NotNull(model, nameof(model));

            _model = model;
        }

        #region Values

        private int _result = 0;
        private int _value1 = 0;
        private int _value2 = 0;

        public int Value1
        {
            get
            {
                return _value1;
            }
            set
            {
                if (value != _value1)
                {
                    _value1 = value;
                    OnPropertyChanged("Value1");
                }
            }
        }

        public int Value2
        {
            get
            {
                return _value2;
            }
            set
            {
                if (value != _value2)
                {
                    _value2 = value;
                    OnPropertyChanged("Value2");
                }
            }
        }
        public int Result
        {
            get
            {
                return _result;
            }
            set
            {
                if (value != _result)
                {
                    _result = value;
                    OnPropertyChanged("Result");
                }
            }
        }

        #endregion

        #region Commands

        #region SummCommand

        private AsyncCommand _summCommand;

        public AsyncCommand SummCommand => GetCommand(ref _summCommand, DoSumm);

        private async Task DoSumm()
        {
            _result = _model.Summ(_value1, _value2);
            Result = _result;
            OnPropertyChanged("Result");
           return;
        }

        #endregion

        #endregion
    }
}
