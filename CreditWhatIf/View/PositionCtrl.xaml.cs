using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CreditWhatIf.ViewModel;

namespace CreditWhatIf.View
{
    /// <inheritdoc />
    /// <summary>
    /// Interaction logic for PositionCtrl.xaml
    /// </summary>
    public partial class PositionCtrl : UserControl
    {
        
        public PositionCtrl()
        {
            InitializeComponent();
            this.DataContext = new AdventureWorksModel();

        }
       
    }
}
