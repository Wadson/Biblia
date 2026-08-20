using System.Collections;
using System.Windows.Input;
namespace Biblia.Presentation.Components;
public partial class AppSelectionSheet : ContentView
{
 public static readonly BindableProperty IsOpenProperty=BindableProperty.Create(nameof(IsOpen),typeof(bool),typeof(AppSelectionSheet),false);
 public static readonly BindableProperty TitleProperty=BindableProperty.Create(nameof(Title),typeof(string),typeof(AppSelectionSheet),"Selecionar");
 public static readonly BindableProperty ItemsSourceProperty=BindableProperty.Create(nameof(ItemsSource),typeof(IEnumerable),typeof(AppSelectionSheet));
 public static readonly BindableProperty CloseCommandProperty=BindableProperty.Create(nameof(CloseCommand),typeof(ICommand),typeof(AppSelectionSheet));
 public static readonly BindableProperty ItemTappedCommandProperty=BindableProperty.Create(nameof(ItemTappedCommand),typeof(ICommand),typeof(AppSelectionSheet));
 public static readonly BindableProperty SearchCommandProperty=BindableProperty.Create(nameof(SearchCommand),typeof(ICommand),typeof(AppSelectionSheet));
 public static readonly BindableProperty SearchTextProperty=BindableProperty.Create(nameof(SearchText),typeof(string),typeof(AppSelectionSheet),string.Empty,BindingMode.TwoWay);
 public static readonly BindableProperty SearchPlaceholderProperty=BindableProperty.Create(nameof(SearchPlaceholder),typeof(string),typeof(AppSelectionSheet),"Pesquisar...");
 public static readonly BindableProperty IsSearchVisibleProperty=BindableProperty.Create(nameof(IsSearchVisible),typeof(bool),typeof(AppSelectionSheet),false);
 public AppSelectionSheet()=>InitializeComponent();
 public bool IsOpen{get=>(bool)GetValue(IsOpenProperty);set=>SetValue(IsOpenProperty,value);} public string Title{get=>(string)GetValue(TitleProperty);set=>SetValue(TitleProperty,value);} public IEnumerable? ItemsSource{get=>(IEnumerable?)GetValue(ItemsSourceProperty);set=>SetValue(ItemsSourceProperty,value);} public ICommand? CloseCommand{get=>(ICommand?)GetValue(CloseCommandProperty);set=>SetValue(CloseCommandProperty,value);} public ICommand? ItemTappedCommand{get=>(ICommand?)GetValue(ItemTappedCommandProperty);set=>SetValue(ItemTappedCommandProperty,value);} public ICommand? SearchCommand{get=>(ICommand?)GetValue(SearchCommandProperty);set=>SetValue(SearchCommandProperty,value);} public string SearchText{get=>(string)GetValue(SearchTextProperty);set=>SetValue(SearchTextProperty,value);} public string SearchPlaceholder{get=>(string)GetValue(SearchPlaceholderProperty);set=>SetValue(SearchPlaceholderProperty,value);} public bool IsSearchVisible{get=>(bool)GetValue(IsSearchVisibleProperty);set=>SetValue(IsSearchVisibleProperty,value);}
 private void OnSearchTextChanged(object? sender,TextChangedEventArgs e){if(SearchCommand?.CanExecute(e.NewTextValue)==true)SearchCommand.Execute(e.NewTextValue);}
}
