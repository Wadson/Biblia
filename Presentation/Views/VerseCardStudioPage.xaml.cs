using Biblia.Presentation.ViewModels;

namespace Biblia.Presentation.Views;

public partial class VerseCardStudioPage : ContentPage
{
    private readonly VerseCardStudioViewModel _viewModel; private bool _loading;
    private Func<object?,Task>? _selectorAction;
    public VerseCardStudioPage(VerseCardStudioViewModel viewModel){InitializeComponent();BindingContext=_viewModel=viewModel;Loaded+=OnLoaded;PhotoGallery.Loaded+=OnPhotoGalleryLoaded;}
#if ANDROID
    private void OnPhotoGalleryLoaded(object? sender,EventArgs e)
    {
        if(PhotoGallery.Handler?.PlatformView is Android.Webkit.WebView webView)
        {
            webView.VerticalScrollBarEnabled=true;
            webView.ScrollbarFadingEnabled=false;
            webView.ScrollBarStyle=Android.Views.ScrollbarStyles.InsideOverlay;
        }
    }
#else
    private void OnPhotoGalleryLoaded(object? sender,EventArgs e) { }
#endif
    private async void OnLoaded(object? sender,EventArgs e){Loaded-=OnLoaded;_loading=true;try{await _viewModel.InitializeAsync();}finally{_loading=false;}}
    private async void OnVersionChanged(object? sender,EventArgs e){if(_loading)return;_loading=true;try{await _viewModel.ChangeVersionAsync();}finally{_loading=false;}}
    private async void OnBookChanged(object? sender,EventArgs e){if(_loading)return;_loading=true;try{await _viewModel.ChangeBookAsync();}finally{_loading=false;}}
    private async void OnChapterChanged(object? sender,EventArgs e){if(_loading)return;_loading=true;try{await _viewModel.ChangeChapterAsync();}finally{_loading=false;}}
    private async void OnPassageChanged(object? sender,EventArgs e){if(_loading)return;_loading=true;try{await _viewModel.ApplyPassageAsync();}finally{_loading=false;}}
    private void OnCancelResult(object? sender,EventArgs e)=>_viewModel.CancelResult();
    private void OnClearTheme(object? sender,EventArgs e)=>_viewModel.SelectedTheme=null;
    private void OnGalleryNavigating(object? sender,WebNavigatingEventArgs e)
    {
        if(!e.Url.StartsWith("bibliatema://photo/",StringComparison.OrdinalIgnoreCase))return;
        e.Cancel=true;
        if(long.TryParse(e.Url[(e.Url.LastIndexOf('/')+1)..],out var id))_viewModel.SelectPhoto(id);
    }
    private void OnSelectVersion(object? sender,EventArgs e)=>OpenSelector("Versão da Bíblia",_viewModel.Versions,x=>((Domain.Entities.BibleVersionCatalogEntry)x!).Code,async x=>{_viewModel.SelectedVersion=(Domain.Entities.BibleVersionCatalogEntry)x!;await _viewModel.ChangeVersionAsync();});
    private void OnSelectBook(object? sender,EventArgs e)=>OpenSelector("Livro",_viewModel.Books,x=>((Domain.Entities.BibleBook)x!).Name,async x=>{_viewModel.SelectedBook=(Domain.Entities.BibleBook)x!;await _viewModel.ChangeBookAsync();});
    private void OnSelectChapter(object? sender,EventArgs e)=>OpenSelector("Capítulo",_viewModel.Chapters,x=>$"Capítulo {x}",async x=>{_viewModel.SelectedChapter=(int)x!;await _viewModel.ChangeChapterAsync();});
    private void OnSelectVerseStart(object? sender,EventArgs e)=>OpenSelector("Versículo inicial",_viewModel.Verses,x=>$"Versículo {x}",async x=>{_viewModel.SelectedVerseStart=(int)x!;await _viewModel.ApplyPassageAsync();});
    private void OnSelectVerseEnd(object? sender,EventArgs e)=>OpenSelector("Versículo final",_viewModel.Verses,x=>$"Versículo {x}",async x=>{_viewModel.SelectedVerseEnd=(int)x!;await _viewModel.ApplyPassageAsync();});
    private void OnSelectTheme(object? sender,EventArgs e)=>OpenSelector("Tema",new object?[]{null}.Concat(_viewModel.Themes),x=>x is Domain.Entities.Theme theme?theme.Name:"Sem tema",x=>{_viewModel.SelectedTheme=(Domain.Entities.Theme?)x;return Task.CompletedTask;});
    private void OnSelectGreeting(object? sender,EventArgs e)=>OpenSelector("Saudação",_viewModel.Greetings,x=>(string)x!,x=>{_viewModel.Greeting=(string)x!;return Task.CompletedTask;});
    private void OnSelectFormat(object? sender,EventArgs e)=>OpenSelector("Formato do card",_viewModel.Formats,x=>(string)x!,x=>{_viewModel.Format=(string)x!;return Task.CompletedTask;});
    private void OnSelectTemplate(object? sender,EventArgs e)=>OpenSelector("Template",_viewModel.Templates,x=>(string)x!,x=>{_viewModel.Template=(string)x!;return Task.CompletedTask;});
    private void OnSelectCategory(object? sender,EventArgs e)=>OpenSelector("Paisagem",_viewModel.Categories,x=>(string)x!,x=>{_viewModel.Category=(string)x!;return Task.CompletedTask;});
    private void OpenSelector(string title,System.Collections.IEnumerable values,Func<object?,string> text,Func<object?,Task> action)
    {
        SelectorTitle.Text=title;
        SelectorList.ItemsSource=values.Cast<object?>().Select(x=>new SelectorOption(text(x),x)).ToArray();
        SelectorList.SelectedItem=null;
        _selectorAction=action;
        SelectorOverlay.IsVisible=true;
    }
    private async void OnSelectorChanged(object? sender,SelectionChangedEventArgs e)
    {
        if(e.CurrentSelection.FirstOrDefault() is not SelectorOption option||_selectorAction is null)return;
        SelectorOverlay.IsVisible=false;
        SelectorList.SelectedItem=null;
        try{await _selectorAction(option.Value);}finally{_selectorAction=null;}
    }
    private void OnCloseSelector(object? sender,EventArgs e){SelectorOverlay.IsVisible=false;SelectorList.SelectedItem=null;_selectorAction=null;}
    private sealed record SelectorOption(string Text,object? Value);
}
