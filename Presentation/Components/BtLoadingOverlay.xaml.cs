namespace Biblia.Presentation.Components;
public partial class BtLoadingOverlay:ContentView
{
 public static readonly BindableProperty IsBusyProperty=BindableProperty.Create(nameof(IsBusy),typeof(bool),typeof(BtLoadingOverlay),false);public static readonly BindableProperty MessageProperty=BindableProperty.Create(nameof(Message),typeof(string),typeof(BtLoadingOverlay),"Carregando...");public BtLoadingOverlay()=>InitializeComponent();public bool IsBusy{get=>(bool)GetValue(IsBusyProperty);set=>SetValue(IsBusyProperty,value);}public string Message{get=>(string)GetValue(MessageProperty);set=>SetValue(MessageProperty,value);}
}
