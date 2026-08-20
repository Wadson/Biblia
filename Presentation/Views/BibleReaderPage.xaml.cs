using System.ComponentModel;
using Biblia.Domain.Entities;
using Biblia.Presentation.ViewModels;

namespace Biblia.Presentation.Views;

public partial class BibleReaderPage:ContentPage,IQueryAttributable
{
    private readonly BibleReaderViewModel _viewModel;
    private bool _loading;

    public BibleReaderPage(BibleReaderViewModel viewModel)
    {
        InitializeComponent();
        BindingContext=_viewModel=viewModel;
        Loaded+=OnLoaded;
        _viewModel.PropertyChanged+=OnViewModelPropertyChanged;
    }

    private async void OnLoaded(object? sender,EventArgs e)
    {
        Loaded-=OnLoaded;
        _loading=true;
        try { await _viewModel.InitializeAsync(); }
        finally
        {
            _loading=false;
            ScrollCurrentSelectors();
        }
    }

    private async void OnVersionChanged(
     object? sender,
     SelectionChangedEventArgs e)
    {
        if (_loading)
            return;

        if (e.CurrentSelection.FirstOrDefault()
            is not BibleVersionCatalogEntry selected)
            return;

        if (!ReferenceEquals(_viewModel.SelectedVersion, selected))
            _viewModel.SelectedVersion = selected;

        _loading = true;

        try
        {
            await _viewModel.ChangeVersionAsync();
        }
        finally
        {
            _loading = false;
            ScrollCurrentSelectors();
        }
    }

    private async void OnBookChanged(
     object? sender,
     SelectionChangedEventArgs e)
    {
        if (_loading)
            return;

        if (e.CurrentSelection.FirstOrDefault()
            is not BibleBook selected)
            return;

        if (!ReferenceEquals(_viewModel.SelectedBook, selected))
            _viewModel.SelectedBook = selected;

        _loading = true;

        try
        {
            await _viewModel.ChangeBookAsync();
        }
        finally
        {
            _loading = false;
            ScrollCurrentSelectors();
        }
    }

    private async void OnChapterChanged(
    object? sender,
    SelectionChangedEventArgs e)
    {
        if (_loading)
            return;

        if (e.CurrentSelection.FirstOrDefault()
            is not int selected)
            return;

        if (_viewModel.SelectedChapter != selected)
            _viewModel.SelectedChapter = selected;

        _loading = true;

        try
        {
            await _viewModel.ChangeChapterAsync();
        }
        finally
        {
            _loading = false;
            ScrollCurrentSelectors();
        }
    }

    private void OnViewModelPropertyChanged(object? sender,PropertyChangedEventArgs e)
    {
        if(e.PropertyName is nameof(BibleReaderViewModel.SelectedVersion)
            or nameof(BibleReaderViewModel.SelectedBook)
            or nameof(BibleReaderViewModel.SelectedChapter))
        {
            Dispatcher.Dispatch(ScrollCurrentSelectors);
        }

        if(e.PropertyName is nameof(BibleReaderViewModel.SelectionStart)
            or nameof(BibleReaderViewModel.SelectionEnd))
        {
            Dispatcher.Dispatch(ScrollSelectionIntoView);
        }
    }

    private void ScrollCurrentSelectors()
    {
        ScrollToSelected(VersionsCollectionView,_viewModel.SelectedVersion,_viewModel.Versions);
        ScrollToSelected(BooksCollectionView,_viewModel.SelectedBook,_viewModel.Books);
        ScrollToSelected(ChaptersCollectionView,_viewModel.SelectedChapter,_viewModel.Chapters);
    }

    private static void ScrollToSelected(CollectionView collection,object? item,System.Collections.IEnumerable items)
    {
        if(item is null||!items.Cast<object>().Contains(item))return;
        collection.ScrollTo(item,position:ScrollToPosition.Center,animate:true);
    }

    private void ScrollSelectionIntoView()
    {
        if(_viewModel.SelectionStart is null)return;
        var item=_viewModel.VerseItems.FirstOrDefault(x=>ReferenceEquals(x.Verse,_viewModel.SelectionStart));
        if(item is not null)VersesCollectionView.ScrollTo(item,position:ScrollToPosition.MakeVisible,animate:true);
    }
    public void ApplyQueryAttributes(IDictionary<string,object> query)
    {
        if(query.TryGetValue("BookReferenceId",out var book)&&query.TryGetValue("Chapter",out var chapter)&&query.TryGetValue("Verse",out var verse))
            _viewModel.ConfigureInitialReference(query.TryGetValue("VersionCode",out var version)?version?.ToString():null,Convert.ToInt32(book),Convert.ToInt32(chapter),Convert.ToInt32(verse));
    }
}
