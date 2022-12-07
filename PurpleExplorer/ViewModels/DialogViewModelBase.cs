namespace PurpleExplorer.ViewModels;

public class DialogViewModelBase : ViewModelBase 
{
    public bool Cancel { get; set; }
        
    public DialogViewModelBase()
    {
        this.Cancel = true;
    }
}