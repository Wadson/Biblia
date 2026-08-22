using Biblia.Presentation.Components;
using Biblia.Presentation.ViewModels;

namespace Biblia.Presentation.Views;

public partial class GlobalSearchPage : ContentPage
{
    public GlobalSearchPage(GlobalSearchViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        Shell.SetTitleView(this, new PageHeaderView { Title = "Pesquisa global", FallbackRoute = "//Home" });
    }
}
