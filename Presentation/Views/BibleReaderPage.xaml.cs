using System.ComponentModel;
using Biblia.Domain.Entities;
using Biblia.Presentation.ViewModels;

namespace Biblia.Presentation.Views;

public partial class BibleReaderPage:ContentPage,IQueryAttributable
{
    private readonly BibleReaderViewModel _viewModel;
    private bool _loading,_controlsVisible=true;
    private double _lastScrollY;
    private int _firstVisibleVerseIndex;

    public BibleReaderPage(BibleReaderViewModel viewModel)
    {
        InitializeComponent();BindingContext=_viewModel=viewModel;Loaded+=OnLoaded;_viewModel.PropertyChanged+=OnViewModelPropertyChanged;
    }

    private async void OnLoaded(object? sender,EventArgs e){Loaded-=OnLoaded;_loading=true;try{await _viewModel.InitializeAsync();}finally{_loading=false;ScrollToChapterStart();}}
    private void OnOpenBookSelector(object? sender,EventArgs e)=>BookSelectorOverlay.IsVisible=true;
    private void OnOpenChapterSelector(object? sender,EventArgs e)=>ChapterSelectorOverlay.IsVisible=true;
    private void OnOpenVersionSelector(object? sender,EventArgs e)=>VersionSelectorOverlay.IsVisible=true;
    private void OnCloseSelectors(object? sender,EventArgs e)=>CloseSelectors();
    private void CloseSelectors(){BookSelectorOverlay.IsVisible=false;ChapterSelectorOverlay.IsVisible=false;VersionSelectorOverlay.IsVisible=false;}
    private async void OnVersionSelected(object? sender,SelectionChangedEventArgs e){if(_loading||!VersionSelectorOverlay.IsVisible||e.CurrentSelection.FirstOrDefault() is not BibleVersionCatalogEntry selected)return;VersionSelectorOverlay.IsVisible=false;if(!ReferenceEquals(_viewModel.SelectedVersion,selected))_viewModel.SelectedVersion=selected;var preserveIndex=_firstVisibleVerseIndex;_loading=true;try{await _viewModel.ChangeVersionAsync();}finally{_loading=false;if(_viewModel.VerseItems.Count>0)VersesCollectionView.ScrollTo(Math.Min(preserveIndex,_viewModel.VerseItems.Count-1),animate:false);}}
    private async void OnBookSelected(object? sender,SelectionChangedEventArgs e){if(_loading||!BookSelectorOverlay.IsVisible||e.CurrentSelection.FirstOrDefault() is not BibleBook selected)return;BookSelectorOverlay.IsVisible=false;if(!ReferenceEquals(_viewModel.SelectedBook,selected))_viewModel.SelectedBook=selected;_loading=true;try{await _viewModel.ChangeBookAsync();}finally{_loading=false;ScrollToChapterStart();}}
    private async void OnChapterSelected(object? sender,SelectionChangedEventArgs e){if(_loading||!ChapterSelectorOverlay.IsVisible||e.CurrentSelection.FirstOrDefault() is not int selected)return;ChapterSelectorOverlay.IsVisible=false;if(_viewModel.SelectedChapter!=selected)_viewModel.SelectedChapter=selected;_loading=true;try{await _viewModel.ChangeChapterAsync();}finally{_loading=false;ScrollToChapterStart();}}
    private void ScrollToChapterStart(){if(_viewModel.VerseItems.Count>0)VersesCollectionView.ScrollTo(0,position:ScrollToPosition.Start,animate:false);_lastScrollY=0;_ = SetControlsVisibleAsync(true);}
    private void OnViewModelPropertyChanged(object? sender,PropertyChangedEventArgs e){if(e.PropertyName is nameof(BibleReaderViewModel.SelectionStart) or nameof(BibleReaderViewModel.SelectionEnd))Dispatcher.Dispatch(ScrollSelectionIntoView);}
    private void ScrollSelectionIntoView(){if(_viewModel.SelectionStart is null)return;var item=_viewModel.VerseItems.FirstOrDefault(x=>ReferenceEquals(x.Verse,_viewModel.SelectionStart));if(item is not null)VersesCollectionView.ScrollTo(item,position:ScrollToPosition.MakeVisible,animate:true);}
    private async void OnVersesScrolled(object? sender,ItemsViewScrolledEventArgs e){_firstVisibleVerseIndex=Math.Max(0,e.FirstVisibleItemIndex);var delta=e.VerticalOffset-_lastScrollY;_lastScrollY=e.VerticalOffset;if(e.VerticalOffset<24)await SetControlsVisibleAsync(true);else if(delta>8)await SetControlsVisibleAsync(false);else if(delta<-8)await SetControlsVisibleAsync(true);}
    private async Task SetControlsVisibleAsync(bool visible){if(_controlsVisible==visible)return;_controlsVisible=visible;if(visible){ReaderControls.IsVisible=true;ReaderControls.HeightRequest=56;await Task.WhenAll(ReaderControls.FadeToAsync(1,180),ReaderControls.ScaleYToAsync(1,180));}else{await Task.WhenAll(ReaderControls.FadeToAsync(0,180),ReaderControls.ScaleYToAsync(.8,180));ReaderControls.HeightRequest=0;ReaderControls.IsVisible=false;}}
    private void OnMoreClicked(object? sender,EventArgs e)=>ReadingOptionsOverlay.IsVisible=true;
    private void OnCloseMoreClicked(object? sender,EventArgs e)=>ReadingOptionsOverlay.IsVisible=false;
    public void ApplyQueryAttributes(IDictionary<string,object> query){if(query.TryGetValue("BookReferenceId",out var book)&&query.TryGetValue("Chapter",out var chapter)&&query.TryGetValue("Verse",out var verse))_viewModel.ConfigureInitialReference(query.TryGetValue("VersionCode",out var version)?version?.ToString():null,Convert.ToInt32(book),Convert.ToInt32(chapter),Convert.ToInt32(verse));}
}
