using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CreditWhatIf.Model;

namespace CreditWhatIf.ViewModel
{
    public class AdventureWorksModel: Observable
    {
        private  DataTable _dt;
        public AdventureWorksModel()
        {
            _dt = AdventureWorksDAL.GetData();
             _groupData= new RelayCommand(GroupDataFunction);
             _unGroupData=new RelayCommand(UNGroupDataFunction);
        }
         
        public DataTable DT
        {
            get { return _dt; }
            set { _dt = value; OnPropertyChanged(); }
        }

        public ICommand _groupData;
        public ICommand _unGroupData;

        public ICommand GroupData
        {
            get { return _groupData; }
        }

        public ICommand UnGroupData
        {
            get { return _unGroupData; }
        }

        private void GroupDataFunction()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(_dt);
            view.GroupDescriptions.Clear();
            view.GroupDescriptions.Add(new PropertyGroupDescription("ProductLine"));
        }

        private void UNGroupDataFunction()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(_dt);
            view.GroupDescriptions.Clear();
        }

        private void FilterCheckBox_Checked()
        {
            _dt.DefaultView.RowFilter = "Color='Black'";
        }

        private void FilterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _dt.DefaultView.RowFilter = "";
        }
    }
}
