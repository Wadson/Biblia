namespace Biblia.Presentation.Components;
public partial class BtSelectableChip : ContentView
{
 public static readonly BindableProperty TextProperty=BindableProperty.Create(nameof(Text),typeof(string),typeof(BtSelectableChip),string.Empty);
 public static readonly BindableProperty MinimumChipWidthProperty=BindableProperty.Create(nameof(MinimumChipWidth),typeof(double),typeof(BtSelectableChip),-1d);
 public BtSelectableChip()=>InitializeComponent();
 public string Text{get=>(string)GetValue(TextProperty);set=>SetValue(TextProperty,value);} public double MinimumChipWidth{get=>(double)GetValue(MinimumChipWidthProperty);set=>SetValue(MinimumChipWidthProperty,value);}
}
