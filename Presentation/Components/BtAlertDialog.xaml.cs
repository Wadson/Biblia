using System.Windows.Input;
namespace Biblia.Presentation.Components;
public partial class BtAlertDialog:ContentView
{
 public static readonly BindableProperty IsOpenProperty=BindableProperty.Create(nameof(IsOpen),typeof(bool),typeof(BtAlertDialog),false);public static readonly BindableProperty TitleProperty=BindableProperty.Create(nameof(Title),typeof(string),typeof(BtAlertDialog),"Atenção");public static readonly BindableProperty MessageProperty=BindableProperty.Create(nameof(Message),typeof(string),typeof(BtAlertDialog),"");public static readonly BindableProperty CloseCommandProperty=BindableProperty.Create(nameof(CloseCommand),typeof(ICommand),typeof(BtAlertDialog));public BtAlertDialog()=>InitializeComponent();public bool IsOpen{get=>(bool)GetValue(IsOpenProperty);set=>SetValue(IsOpenProperty,value);}public string Title{get=>(string)GetValue(TitleProperty);set=>SetValue(TitleProperty,value);}public string Message{get=>(string)GetValue(MessageProperty);set=>SetValue(MessageProperty,value);}public ICommand? CloseCommand{get=>(ICommand?)GetValue(CloseCommandProperty);set=>SetValue(CloseCommandProperty,value);}
}
