using Biblia.Presentation.ViewModels;
namespace Biblia.Presentation.Views;
public partial class BibleComparisonPage : ContentPage, IQueryAttributable
{
    private readonly BibleComparisonViewModel _viewModel;
    public BibleComparisonPage(BibleComparisonViewModel viewModel){InitializeComponent();BindingContext=_viewModel=viewModel;}
    public void ApplyQueryAttributes(IDictionary<string,object> query)=>_=_viewModel.InitializeAsync(new Dictionary<string,object>(query));
}
