using Biblia.Presentation.Components;
using Biblia.Presentation.ViewModels;

namespace Biblia.Presentation.Views;

public partial class BibleSearchPage : ContentPage
{
    private readonly BibleSearchViewModel _vm;

    public BibleSearchPage(BibleSearchViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        Shell.SetTitleView(this, new PageHeaderView { Title = "Pesquisar na Bíblia", FallbackRoute = "//Home" });
        Loaded += (_, _) => _vm.LoadCommand.Execute(null);
    }
}
