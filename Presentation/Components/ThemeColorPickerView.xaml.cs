using System.Collections;
using Biblia.Presentation.Models;

namespace Biblia.Presentation.Components;

public partial class ThemeColorPickerView : ContentView
{
    public static readonly BindableProperty ColorOptionsProperty = BindableProperty.Create(nameof(ColorOptions), typeof(IEnumerable), typeof(ThemeColorPickerView));
    public static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(nameof(SelectedColor), typeof(ThemeColorOption), typeof(ThemeColorPickerView), defaultBindingMode: BindingMode.TwoWay);
    public ThemeColorPickerView() => InitializeComponent();
    public IEnumerable? ColorOptions { get => (IEnumerable?)GetValue(ColorOptionsProperty); set => SetValue(ColorOptionsProperty, value); }
    public ThemeColorOption? SelectedColor { get => (ThemeColorOption?)GetValue(SelectedColorProperty); set => SetValue(SelectedColorProperty, value); }
}
