using System;
using System.Collections.Generic;
using System.Text;

namespace PurpleExplorer.ViewModels
{
    public class DialogViewModelBase : ViewModelBase 
    {
        public bool Cancel { get; set; }
        
        public DialogViewModelBase()
        {
            this.Cancel = true;
        }
    }
}
