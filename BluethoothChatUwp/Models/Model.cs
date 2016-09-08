using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using BluethoothChatUwp.Core;
using SimpleMvvm;
using SimpleMvvm.Annotations;

namespace BluethoothChatUwp.Models
{
    internal class Model
        : ObservableObject, IModel
    {
        /*public Model(IState state, IUIService uiService)
        {
            Guard.NotNull(state, nameof(state));

            _state = state;
            _spectrometer = new Spectrometer(uiService);
        }*/

        #region Properties

        #region UserName

        private string _userName;

        public string UserName
        {
            get { return _userName; }
            private set { SetValue(ref _userName, value); }
        }

        #endregion

        #region CompanyName

        private string _companyName;
        public string CompanyName
        {
            get { return _companyName; }
            private set { SetValue(ref _companyName, value); }
        }

        #endregion

        #endregion

        public void SetRegistrationData(string userName, string companyName)
        {
            UserName = userName;
            CompanyName = companyName;
        }

        public int Summ(int a, int b)
        {
            return a + b;
        }
    }
}
