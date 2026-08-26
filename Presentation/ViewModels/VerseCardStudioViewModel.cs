using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Biblia.Application.Interfaces;
using Biblia.Domain.Entities;
using Biblia.Domain.Rules;
using Biblia.Presentation.Commands;
using Microsoft.Extensions.Logging;

namespace Biblia.Presentation.ViewModels;

public sealed class VerseCardStudioViewModel : INotifyPropertyChanged
{
    private readonly IDailyVerseService _daily; private readonly IBibleVersionManager _versions; private readonly IBibleRepository _bible; private readonly IThemeService _themes; private readonly ISavedReferenceService _savedReferences; private readonly INatureMediaService _media; private readonly IVerseCardService _cards; private readonly IFileTransferService _files; private readonly ILogger<VerseCardStudioViewModel> _logger;
    private BibleVersionCatalogEntry? _version; private BibleBook? _book; private int _chapter,_verseStart,_verseEnd,_photoPage; private Theme? _theme; private PhotoOption? _photo; private string _greeting="Automática",_customGreeting="",_format="Vertical 4:5",_template="Clássico",_category="Natureza",_loadedPhotoCategory="",_verseText="",_reference="",_status="",_mediaStatus=""; private double _overlay=.4; private bool _busy,_chooseOther,_resultVisible,_hasMorePhotos; private VerseCardResult? _result;
    public ObservableCollection<BibleVersionCatalogEntry> Versions{get;}=[]; public ObservableCollection<BibleBook> Books{get;}=[]; public ObservableCollection<int> Chapters{get;}=[]; public ObservableCollection<int> Verses{get;}=[]; public ObservableCollection<Theme> Themes{get;}=[]; public ObservableCollection<PhotoOption> Photos{get;}=[]; public HtmlWebViewSource GallerySource{get;private set;}=new();
    public IReadOnlyList<string> Greetings { get; }=["Automática","Bom dia!","Boa tarde!","Boa noite!","Uma abençoada semana!","Um bom final de semana!","Um bom feriado!","Deus abençoe seu dia!","Sem saudação","Personalizada"];
    public IReadOnlyList<string> Formats { get; }=["Vertical 4:5","Story 9:16","Quadrado 1:1"]; public IReadOnlyList<string> Templates { get; }=["Clássico","Editorial","Minimalista"];
    public IReadOnlyList<string> Categories { get; }=["Natureza","Flores","Floresta","Campos","Rio","Lago","Montanhas","Nascer do sol","Pôr do sol","Trigo","Campo de flores","Vegetação","Ovelhas","Campo rural","Gado","Céu","Mar","Cachoeira"];
    public BibleVersionCatalogEntry? SelectedVersion{get=>_version;set{if(Set(ref _version,value)){On(nameof(VersionCode));On(nameof(VersionDisplay));}}} public BibleBook? SelectedBook{get=>_book;set{if(Set(ref _book,value))On(nameof(BookDisplay));}} public int SelectedChapter{get=>_chapter;set{if(Set(ref _chapter,value))On(nameof(ChapterDisplay));}} public int SelectedVerseStart{get=>_verseStart;set{if(Set(ref _verseStart,value))On(nameof(VerseStartDisplay));}} public int SelectedVerseEnd{get=>_verseEnd;set{if(Set(ref _verseEnd,value))On(nameof(VerseEndDisplay));}}
    public string VersionDisplay=>SelectedVersion?.Code??"Versão"; public string BookDisplay=>SelectedBook?.Name??"Livro"; public string ChapterDisplay=>SelectedChapter>0?$"Capítulo {SelectedChapter}":"Capítulo"; public string VerseStartDisplay=>SelectedVerseStart>0?$"Início {SelectedVerseStart}":"Início"; public string VerseEndDisplay=>SelectedVerseEnd>0?$"Fim {SelectedVerseEnd}":"Fim";
    public Theme? SelectedTheme{get=>_theme;set{if(Set(ref _theme,value)){On(nameof(ThemeText));On(nameof(ThemeDisplay));}}} public string ThemeText=>SelectedTheme?.Name??""; public string ThemeDisplay=>SelectedTheme?.Name??"Sem tema";
    public PhotoOption? SelectedPhoto{get=>_photo;set{if(Set(ref _photo,value)){UpdatePreview();On(nameof(PreviewBackgroundColor));On(nameof(PhotoAttribution));}}} public HtmlWebViewSource PreviewSource{get;private set;}=new(); public string PreviewBackgroundColor=>SelectedPhoto?.AverageColor??"#0D1E30"; public string PhotoAttribution=>SelectedPhoto?.Attribution??"Fundo BíbliaTema";
    public string Greeting{get=>_greeting;set{if(Set(ref _greeting,value)){On(nameof(GreetingText));On(nameof(IsCustomGreeting));}}} public string CustomGreeting{get=>_customGreeting;set{if(Set(ref _customGreeting,value))On(nameof(GreetingText));}} public bool IsCustomGreeting=>Greeting=="Personalizada";
    public string GreetingText=>Greeting switch{"Automática"=>VerseCardGreeting.Automatic(DateTime.Now),"Sem saudação"=>"","Personalizada"=>CustomGreeting.Trim(),_=>Greeting};
    public string Format{get=>_format;set{if(Set(ref _format,value))On(nameof(PreviewHeight));}} public double PreviewHeight=>Format switch{"Story 9:16"=>720,"Quadrado 1:1"=>430,_=>538}; public string Template{get=>_template;set{if(Set(ref _template,value))On(nameof(PreviewVerseSize));}} public double PreviewVerseSize=>Template switch{"Editorial"=>21,"Minimalista"=>20,_=>23}; public string Category{get=>_category;set=>Set(ref _category,value);} public double OverlayOpacity{get=>_overlay;set{if(Set(ref _overlay,value)){On(nameof(OverlayDisplay));On(nameof(PreviewImageOpacity));}}} public double PreviewImageOpacity=>1-OverlayOpacity; public string OverlayDisplay=>$"Escurecimento: {OverlayOpacity:P0}";
    public string VerseText{get=>_verseText;private set=>Set(ref _verseText,value);} public string Reference{get=>_reference;private set=>Set(ref _reference,value);} public string VersionCode=>SelectedVersion?.Code??""; public bool ChooseOther{get=>_chooseOther;private set=>Set(ref _chooseOther,value);} public bool IsBusy{get=>_busy;private set{if(Set(ref _busy,value))NotifyCommands();}} public string Status{get=>_status;private set=>Set(ref _status,value);} public string MediaStatus{get=>_mediaStatus;private set{if(Set(ref _mediaStatus,value))On(nameof(HasMediaStatus));}} public bool HasMediaStatus=>!string.IsNullOrWhiteSpace(MediaStatus); public bool HasMorePhotos{get=>_hasMorePhotos;private set{if(Set(ref _hasMorePhotos,value))LoadMorePhotosCommand.NotifyCanExecuteChanged();}} public bool ResultVisible{get=>_resultVisible;private set=>Set(ref _resultVisible,value);}
    public AsyncCommand LoadCommand{get;} public AsyncCommand UseDailyCommand{get;} public AsyncCommand ChooseOtherCommand{get;} public AsyncCommand SearchPhotosCommand{get;} public AsyncCommand LoadMorePhotosCommand{get;} public AsyncCommand RenderCommand{get;} public AsyncCommand SaveCommand{get;} public AsyncCommand ShareCommand{get;} public ICommand SelectPhotoCommand{get;}
    public event PropertyChangedEventHandler? PropertyChanged;
    public VerseCardStudioViewModel(IDailyVerseService daily,IBibleVersionManager versions,IBibleRepository bible,IThemeService themes,ISavedReferenceService savedReferences,INatureMediaService media,IVerseCardService cards,IFileTransferService files,ILogger<VerseCardStudioViewModel> logger){_daily=daily;_versions=versions;_bible=bible;_themes=themes;_savedReferences=savedReferences;_media=media;_cards=cards;_files=files;_logger=logger;LoadCommand=new(LoadAsync,()=>!IsBusy);UseDailyCommand=new(UseDailyAsync,()=>!IsBusy);ChooseOtherCommand=new(ChooseOtherAsync,()=>!IsBusy);SearchPhotosCommand=new(SearchPhotosAsync,()=>!IsBusy);LoadMorePhotosCommand=new(LoadMorePhotosAsync,()=>!IsBusy&&HasMorePhotos);RenderCommand=new(RenderAsync,()=>!IsBusy&&!string.IsNullOrWhiteSpace(VerseText)&&SelectedPhoto is not null);SaveCommand=new(SaveAsync,HasResult);ShareCommand=new(ShareAsync,HasResult);SelectPhotoCommand=new Command<PhotoOption>(x=>SelectedPhoto=x);}
    public Task InitializeAsync()=>LoadAsync();
    private async Task LoadAsync()=>await RunAsync(async()=>{await _versions.InitializeCatalogAsync();Versions.Clear();foreach(var v in (await _versions.GetVersionsAsync()).Where(x=>x.IsInstalled&&x.IsEnabled))Versions.Add(v);Themes.Clear();foreach(var t in await _themes.SearchAsync(null))Themes.Add(t);Photos.Clear();foreach(var p in _media.GetOfflineBackgrounds())Photos.Add(new(p));SelectedPhoto=Photos.FirstOrDefault();UpdateGallery();await UseDailyCoreAsync();Status="Card preenchido com o Versículo do Dia.";});
    private async Task UseDailyAsync()=>await RunAsync(UseDailyCoreAsync);
    private async Task ChooseOtherAsync(){ChooseOther=true;await ChangeVersionAsync();}
    private async Task UseDailyCoreAsync(){var daily=await _daily.GetDailyVerseAsync()??throw new InvalidOperationException("Versículo do Dia indisponível.");SelectedVersion=Versions.FirstOrDefault(x=>x.Code==daily.BibleVersionCode);VerseText=daily.BibleText;Reference=daily.ReferenceText;SelectedTheme=Themes.FirstOrDefault(x=>x.Name.Equals(daily.Category,StringComparison.CurrentCultureIgnoreCase));ChooseOther=false;On(nameof(VersionCode));}
    public async Task ChangeVersionAsync(){if(SelectedVersion is null)return;await RunAsync(async()=>{Books.Clear();foreach(var b in await _bible.GetBooksAsync(SelectedVersion.Code))Books.Add(b);SelectedBook=Books.FirstOrDefault();await ChangeBookCoreAsync();});}
    public async Task ChangeBookAsync()=>await RunAsync(ChangeBookCoreAsync); private async Task ChangeBookCoreAsync(){if(SelectedVersion is null||SelectedBook is null)return;Chapters.Clear();foreach(var c in await _bible.GetChaptersAsync(SelectedVersion.Code,SelectedBook.BookReferenceId))Chapters.Add(c);SelectedChapter=Chapters.FirstOrDefault();await ChangeChapterCoreAsync();}
    public async Task ChangeChapterAsync()=>await RunAsync(ChangeChapterCoreAsync); private async Task ChangeChapterCoreAsync(){if(SelectedVersion is null||SelectedBook is null||SelectedChapter<1)return;var verses=await _bible.GetVersesAsync(SelectedVersion.Code,SelectedBook.BookReferenceId,SelectedChapter);Verses.Clear();foreach(var v in verses.Select(x=>x.Verse).Distinct())Verses.Add(v);SelectedVerseStart=Verses.FirstOrDefault();SelectedVerseEnd=SelectedVerseStart;await ApplyPassageCoreAsync();}
    public async Task ApplyPassageAsync()=>await RunAsync(ApplyPassageCoreAsync); private async Task ApplyPassageCoreAsync(){if(SelectedVersion is null||SelectedBook is null||SelectedVerseStart<1)return;var end=Math.Max(SelectedVerseStart,SelectedVerseEnd);var passage=await _bible.GetPassageAsync(SelectedVersion.Code,SelectedBook.BookReferenceId,SelectedChapter,SelectedVerseStart,end);VerseText=string.Join(" ",passage.Verses.Select(x=>x.Text));Reference=$"{SelectedBook.Name} {SelectedChapter}:{SelectedVerseStart}"+(end==SelectedVerseStart?"":$"–{end}");var saved=await _savedReferences.FindCanonicalAsync(SelectedBook.BookReferenceId,SelectedChapter,SelectedVerseStart,end);if(saved?.Themes.Count>0)SelectedTheme=saved.Themes[0];On(nameof(VersionCode));}
    private async Task SearchPhotosAsync()=>await LoadPhotosAsync(false);
    private async Task LoadMorePhotosAsync()=>await LoadPhotosAsync(true);
    private async Task LoadPhotosAsync(bool append)=>await RunAsync(async()=>{try{if(!append||!string.Equals(_loadedPhotoCategory,Category,StringComparison.Ordinal)){_photoPage=0;_loadedPhotoCategory=Category;if(!append){foreach(var item in Photos.Where(x=>!x.Photo.IsLocal).ToArray())Photos.Remove(item);}}var nextPage=_photoPage+1;var photos=await _media.SearchAsync(Query(Category),nextPage,8);var added=0;foreach(var p in photos){if(Photos.Any(x=>x.Id==p.Id))continue;Photos.Add(new(p));added++;}_photoPage=nextPage;HasMorePhotos=added>0;if(!append&&added>0)SelectedPhoto=Photos.First(x=>!x.Photo.IsLocal);UpdateGallery();MediaStatus="";Status=added==0?"Não há novas fotos neste lote.":$"{added} novas fotos carregadas. Total: {Photos.Count(x=>!x.Photo.IsLocal)}.";}catch(Exception ex){MediaStatus=ex.Message;Status=ex.Message;}});
    private async Task RenderAsync()=>await RunAsync(async()=>{try{_result=await _cards.RenderAsync(new(GreetingText,ThemeText,VerseText,Reference,VersionCode,SelectedPhoto!.Photo,OverlayOpacity,Format,Template));MediaStatus="";ResultVisible=true;Status="Card gerado com sucesso.";NotifyCommands();}catch(InvalidOperationException ex){MediaStatus=ex.Message;throw;}});
    private async Task SaveAsync()=>await RunAsync(async()=>{var outcome=await _files.SaveCopyAsync(_result!.FilePath);if(outcome==FileSaveOutcome.Saved){ResultVisible=false;Status="Imagem salva no destino escolhido.";}else Status="Salvamento cancelado.";}); private async Task ShareAsync()=>await RunAsync(async()=>{await _files.ShareAsync(_result!.FilePath);ResultVisible=false;Status="Compartilhamento aberto.";}); public void CancelResult()=>ResultVisible=false;
    private bool HasResult()=>!IsBusy&&_result is not null&&File.Exists(_result.FilePath); private void NotifyCommands(){LoadCommand.NotifyCanExecuteChanged();UseDailyCommand.NotifyCanExecuteChanged();ChooseOtherCommand.NotifyCanExecuteChanged();SearchPhotosCommand.NotifyCanExecuteChanged();LoadMorePhotosCommand.NotifyCanExecuteChanged();RenderCommand.NotifyCanExecuteChanged();SaveCommand.NotifyCanExecuteChanged();ShareCommand.NotifyCanExecuteChanged();}
    private async Task RunAsync(Func<Task> action){if(IsBusy)return;IsBusy=true;try{await action();}catch(OperationCanceledException){Status="Operação cancelada.";}catch(Exception ex){_logger.LogError(ex,"Falha no fluxo do VerseCardStudio");Status="Não foi possível concluir a operação. Tente outro fundo ou tente novamente.";MediaStatus=ex.Message;}finally{IsBusy=false;}}
    private static string Query(string value)=>value switch{"Flores"=>"flowers","Floresta"=>"forest","Campos"=>"countryside field","Rio"=>"river nature","Lago"=>"lake nature","Montanhas"=>"mountains landscape","Nascer do sol"=>"sunrise landscape","Pôr do sol"=>"sunset landscape","Trigo"=>"wheat field","Campo de flores"=>"flower field","Vegetação"=>"green vegetation","Ovelhas"=>"sheep field","Campo rural"=>"countryside","Gado"=>"cattle field","Céu"=>"sky clouds","Mar"=>"ocean nature","Cachoeira"=>"waterfall",_=>"nature"};
    public void SelectPhoto(long id){var photo=Photos.FirstOrDefault(x=>x.Id==id);if(photo is not null)SelectedPhoto=photo;UpdateGallery();}
    private void UpdatePreview()
    {
        var photo=SelectedPhoto;
        var color=photo?.AverageColor??"#0D1E30";
        var image=photo is null||string.IsNullOrWhiteSpace(photo.PreviewDataUrl)?string.Empty:$"<img src='{photo.PreviewDataUrl}' alt=''/>";
        PreviewSource=new HtmlWebViewSource{Html=$"<!doctype html><meta name='viewport' content='width=device-width,initial-scale=1'><style>html,body{{margin:0;width:100%;height:100%;overflow:hidden;background:{color}}}img{{display:block;width:100%;height:100%;object-fit:cover}}</style>{image}"};
        On(nameof(PreviewSource));
    }
    private void UpdateGallery()
    {
        var cards = string.Join("", Photos.Select(x => x.Photo.IsLocal
            ? $"<a href='bibliatema://photo/{x.Id}' class='card local' style='background:{x.AverageColor}'><span>Fundo BíbliaTema</span></a>"
            : $"<a href='bibliatema://photo/{x.Id}' class='card {(SelectedPhoto?.Id == x.Id ? "selected" : "")}'><img src='{x.PreviewDataUrl}'/><span>{System.Net.WebUtility.HtmlEncode(x.Attribution)}</span></a>"));

        var html = $$$"""
            <!doctype html><meta name="viewport" content="width=device-width,initial-scale=1,maximum-scale=1">
            <style>
            *{box-sizing:border-box}html,body{height:100%;margin:0;background:#fff;font-family:sans-serif}
            body{overflow-y:scroll;overscroll-behavior:contain;padding-right:50px;scrollbar-width:none}
            body::-webkit-scrollbar{display:none}.grid{display:grid;grid-template-columns:1fr 1fr;gap:7px;padding-bottom:4px}
            .card{height:125px;border:1px solid #E0E0E0;border-radius:14px;overflow:hidden;position:relative;background:#fff}
            .selected{border:3px solid #0066CC}img{width:100%;height:100%;object-fit:cover}
            span{position:absolute;left:0;right:0;bottom:0;padding:7px;color:white;background:#0007;font-size:10px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}.local span{background:#0004}
            #scrollControls{position:fixed;z-index:9999;right:2px;top:2px;bottom:2px;width:44px;display:flex;flex-direction:column;justify-content:space-between;pointer-events:none}
            .scrollArrow{width:44px;height:54px;border:2px solid #B8D4F0;border-radius:13px;background:#E4EDF7;color:#0066CC;font-size:25px;font-weight:800;line-height:46px;text-align:center;box-shadow:0 2px 5px #0D1E3033;pointer-events:auto;touch-action:none;user-select:none;-webkit-user-select:none}
            .scrollArrow:active{background:#0066CC;color:#fff;border-color:#0066CC;transform:scale(.96)}
            </style>
            <div class="grid">{{{cards}}}</div><div id="scrollControls"><button id="scrollUp" class="scrollArrow" aria-label="Rolar fotos para cima">▲</button><button id="scrollDown" class="scrollArrow" aria-label="Rolar fotos para baixo">▼</button></div>
            <script>
            const controls=document.getElementById('scrollControls'),up=document.getElementById('scrollUp'),down=document.getElementById('scrollDown');let repeatTimer=null;
            function canScroll(){return Math.max(document.documentElement.scrollHeight,document.body.scrollHeight)>innerHeight+2}
            function updateControls(){controls.style.display=canScroll()?'flex':'none';up.style.opacity=scrollY<=1?'.45':'1';down.style.opacity=scrollY+innerHeight>=document.documentElement.scrollHeight-2?'.45':'1'}
            function step(direction){scrollBy({top:direction*Math.max(110,innerHeight*.72),behavior:'smooth'})}
            function stopRepeat(){if(repeatTimer){clearInterval(repeatTimer);repeatTimer=null}}
            function bind(button,direction){button.addEventListener('click',e=>{step(direction);e.preventDefault()});button.addEventListener('pointerdown',e=>{stopRepeat();repeatTimer=setInterval(()=>step(direction),240);button.setPointerCapture(e.pointerId);e.preventDefault()});button.addEventListener('pointerup',stopRepeat);button.addEventListener('pointercancel',stopRepeat);button.addEventListener('pointerleave',stopRepeat)}
            bind(up,-1);bind(down,1);addEventListener('scroll',updateControls,{passive:true});addEventListener('resize',updateControls);requestAnimationFrame(updateControls);setTimeout(updateControls,100);
            </script>
            """;
        GallerySource = new HtmlWebViewSource { Html = html };
        On(nameof(GallerySource));
    }
    private bool Set<T>(ref T field,T value,[CallerMemberName]string? name=null){if(EqualityComparer<T>.Default.Equals(field,value))return false;field=value;On(name);return true;}private void On([CallerMemberName]string? name=null)=>PropertyChanged?.Invoke(this,new(name));

    public sealed class PhotoOption
    {
        public PhotoOption(NaturePhoto photo)
        {
            Photo=photo;
            if(string.IsNullOrWhiteSpace(photo.PreviewUrl))return;
            var source=photo.PreviewUrl.StartsWith("file:",StringComparison.OrdinalIgnoreCase)?new Uri(photo.PreviewUrl).LocalPath:photo.PreviewUrl;
            PreviewUrl=Path.IsPathRooted(source)
                ? CreateMemoryImage(source)
                : ImageSource.FromUri(new Uri(source));
        }
        public NaturePhoto Photo{get;} public long Id=>Photo.Id; public string PreviewPath=>Photo.PreviewUrl; public string PreviewDataUrl=>CreateDataUrl(Photo.PreviewUrl); public ImageSource? PreviewUrl{get;} public string? AverageColor=>Photo.AverageColor; public string Attribution=>Photo.Attribution;
        private static ImageSource CreateMemoryImage(string path)
        {
            var bytes=File.ReadAllBytes(path);
            return ImageSource.FromStream(()=>new MemoryStream(bytes,false));
        }
        private static string CreateDataUrl(string value)
        {
            if(string.IsNullOrWhiteSpace(value))return string.Empty;
            var path=value.StartsWith("file:",StringComparison.OrdinalIgnoreCase)?new Uri(value).LocalPath:value;
            return Path.IsPathRooted(path)&&File.Exists(path)?$"data:image/jpeg;base64,{Convert.ToBase64String(File.ReadAllBytes(path))}":value;
        }
    }
}
