using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELTECity.WPF.ViewModel
{
    public class ExpenseIncome : ViewModelBase
    {
        #region Properties

        public String? Description { get; set; }
        public String? Money { get; set; }
        public String? Color { get; set; }

        #endregion
    }
}
