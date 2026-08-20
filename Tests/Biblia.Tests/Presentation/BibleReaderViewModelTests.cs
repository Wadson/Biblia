using Biblia.Application.Interfaces;
using Biblia.Application.Interfaces.Repositories;
using Biblia.Domain.Entities;
using Biblia.Domain.Enums;
using Biblia.Domain.ValueObjects;
using Biblia.Presentation.ViewModels;
using Xunit;

#pragma warning disable xUnit1051

namespace Biblia.Tests.Presentation;

public sealed class BibleReaderViewModelTests
{
    [Fact]
    public async Task Reader_LoadsSwitchesVersionAndPersistsCanonicalSelection()
    {
        var manager=new Versions();var bible=new Bible();var saved=new Saved();var themes=new Themes();var messages=new Messages();var clipboard=new Clip();var vm=new BibleReaderViewModel(manager,bible,saved,themes,messages,clipboard,new Navigator());
        await vm.InitializeAsync();Assert.Equal("ACF",vm.SelectedVersion!.Code);Assert.Equal(43,vm.SelectedBook!.BookReferenceId);Assert.Equal(3,vm.SelectedChapter);Assert.Equal(2,vm.Verses.Count);
        vm.SelectionStart=vm.Verses[0];vm.SelectionEnd=vm.Verses[1];vm.CopyCommand.Execute(null);await WaitAsync(()=>clipboard.Text is not null);Assert.Contains("João 3:16-17",clipboard.Text);
        vm.SaveCommand.Execute(null);await WaitAsync(()=>saved.Item is not null);Assert.Equal(43,saved.Item!.BookReferenceId);Assert.Equal(16,saved.Item.VerseStart);Assert.Equal(17,saved.Item.VerseEnd);
        vm.SelectedVersion=manager.Items[1];await vm.ChangeVersionAsync();Assert.Equal(43,vm.SelectedBook.BookReferenceId);Assert.Equal(3,vm.SelectedChapter);Assert.All(vm.Verses,v=>Assert.Equal("NVI",v.VersionCode));
    }
    [Fact]
    public async Task VerseTap_TogglesSingleSelectionAndBuildsRangeWithoutNativeSelection()
    {
        var vm=CreateReader(new Navigator());await vm.InitializeAsync();
        vm.SelectVerse(vm.Verses[0]);Assert.True(vm.VerseItems[0].IsSelected);Assert.False(vm.VerseItems[1].IsSelected);
        vm.SelectVerse(vm.Verses[0]);Assert.Null(vm.SelectionStart);Assert.All(vm.VerseItems,x=>Assert.False(x.IsSelected));
        vm.SelectVerse(vm.Verses[1]);vm.SelectVerse(vm.Verses[0]);Assert.All(vm.VerseItems,x=>Assert.True(x.IsSelected));
    }
    [Fact]
    public async Task CompareCommand_NavigatesWithCanonicalParameters()
    {
        var navigator=new Navigator();var vm=CreateReader(navigator);await vm.InitializeAsync();vm.SelectVerse(vm.Verses[0]);
        vm.CompareCommand.Execute(null);await WaitAsync(()=>navigator.Route is not null);
        Assert.Equal("BibleComparison",navigator.Route);Assert.Equal(43,navigator.Parameters!["BookReferenceId"]);Assert.Equal("João",navigator.Parameters["BookName"]);Assert.Equal(3,navigator.Parameters["Chapter"]);Assert.Equal(16,navigator.Parameters["VerseStart"]);Assert.Equal(16,navigator.Parameters["VerseEnd"]);Assert.Equal("ACF",navigator.Parameters["SourceVersionCode"]);
    }
    private static BibleReaderViewModel CreateReader(IAppNavigator navigator)=>new(new Versions(),new Bible(),new Saved(),new Themes(),new Messages(),new Clip(),navigator);
    private static async Task WaitAsync(Func<bool> done){for(var i=0;i<50&&!done();i++)await Task.Delay(10);Assert.True(done());}
    private sealed class Versions:IBibleVersionManager
    {public List<BibleVersionCatalogEntry> Items{get;}=[Entry(1,"ACF"),Entry(2,"NVI")];private static BibleVersionCatalogEntry Entry(long id,string code)=>new(id,code,code,"pt-BR",code+".sqlite",code,2,null,null,null,true,true,true,null,null,BibleVersionValidationStatus.Compatible,null);public Task<IReadOnlyList<BibleVersionCatalogEntry>> GetVersionsAsync(CancellationToken t=default)=>Task.FromResult<IReadOnlyList<BibleVersionCatalogEntry>>(Items);public Task<BibleVersionCatalogEntry?> GetActiveVersionAsync(CancellationToken t=default)=>Task.FromResult<BibleVersionCatalogEntry?>(Items[0]);public Task InitializeCatalogAsync(CancellationToken t=default)=>Task.CompletedTask;public Task SetActiveVersionAsync(string c,CancellationToken t=default)=>Task.CompletedTask;public Task SetEnabledAsync(string c,bool e,CancellationToken t=default)=>Task.CompletedTask;public Task<string> ResolveDatabasePathAsync(string c,CancellationToken t=default)=>throw new NotSupportedException();public Task<BibleValidationResult> ValidateAsync(string c,CancellationToken t=default)=>throw new NotSupportedException();}
    private sealed class Bible:IBibleRepository
    {public Task<IReadOnlyList<BibleBook>> GetBooksAsync(string v,CancellationToken t=default)=>Task.FromResult<IReadOnlyList<BibleBook>>([new(43,2,"João")]);public Task<IReadOnlyList<int>> GetChaptersAsync(string v,int b,CancellationToken t=default)=>Task.FromResult<IReadOnlyList<int>>([3]);public Task<IReadOnlyList<BibleVerse>> GetVersesAsync(string v,int b,int c,CancellationToken t=default)=>Task.FromResult<IReadOnlyList<BibleVerse>>([new(v,43,"João",3,16,"Porque Deus amou"),new(v,43,"João",3,17,"Deus enviou")]);public Task<IReadOnlyList<BibleVerse>> GetVerseAsync(string v,BibleReference r,CancellationToken t=default)=>throw new NotSupportedException();public Task<BiblePassage> GetPassageAsync(string v,int b,int c,int s,int e,CancellationToken t=default)=>throw new NotSupportedException();public Task<IReadOnlyList<BibleVerse>> SearchAsync(string v,string q,int? b=null,int? c=null,int skip=0,int take=100,CancellationToken t=default)=>throw new NotSupportedException();}
    private sealed class Saved:ISavedReferenceRepository{public SavedReference? Item;public Task<SavedReference> CreateAsync(SavedReference r,CancellationToken t=default){Item=r with{Id=1};return Task.FromResult(Item);}public Task<SavedReference?> GetAsync(long i,CancellationToken t=default)=>throw new NotSupportedException();public Task<SavedReferenceDetails?> GetDetailsAsync(long i,CancellationToken t=default)=>throw new NotSupportedException();public Task<IReadOnlyList<SavedReferenceDetails>> SearchAsync(string? q,CancellationToken t=default)=>Task.FromResult<IReadOnlyList<SavedReferenceDetails>>([]);public Task UpdateAsync(SavedReference r,CancellationToken t=default)=>throw new NotSupportedException();public Task AddThemeAsync(long r,long th,CancellationToken t=default)=>throw new NotSupportedException();public Task RemoveThemeAsync(long r,long th,CancellationToken t=default)=>throw new NotSupportedException();public Task SetThemesAsync(long r,IReadOnlyCollection<long> th,CancellationToken t=default)=>Task.CompletedTask;public Task DeleteAsync(long i,CancellationToken t=default)=>throw new NotSupportedException();}
    private sealed class Themes:IThemeRepository{public Task<IReadOnlyList<Theme>> GetAllAsync(CancellationToken t=default)=>Task.FromResult<IReadOnlyList<Theme>>([]);public Task<IReadOnlyList<Theme>> SearchAsync(string q,CancellationToken t=default)=>Task.FromResult<IReadOnlyList<Theme>>([]);public Task<Theme> CreateAsync(string n,string? c,string? d,CancellationToken t=default)=>throw new NotSupportedException();public Task<Theme?> GetAsync(long i,CancellationToken t=default)=>throw new NotSupportedException();public Task<ThemeUsage> GetUsageAsync(long i,CancellationToken t=default)=>Task.FromResult(new ThemeUsage(0,0));public Task UpdateAsync(Theme x,CancellationToken t=default)=>throw new NotSupportedException();public Task DeleteAsync(long i,CancellationToken t=default)=>throw new NotSupportedException();}
    private sealed class Messages:IMessageRepository{public Task<IReadOnlyList<Message>> GetAllAsync(CancellationToken t=default)=>Task.FromResult<IReadOnlyList<Message>>([]);public Task<Message> CreateAsync(Message x,CancellationToken t=default)=>throw new NotSupportedException();public Task<Message?> GetAsync(long i,CancellationToken t=default)=>throw new NotSupportedException();public Task UpdateAsync(Message x,CancellationToken t=default)=>throw new NotSupportedException();public Task<MessageTopic> AddTopicAsync(MessageTopic x,CancellationToken t=default)=>throw new NotSupportedException();public Task<IReadOnlyList<MessageTopic>> GetTopicsAsync(long i,CancellationToken t=default)=>Task.FromResult<IReadOnlyList<MessageTopic>>([]);public Task UpdateTopicAsync(MessageTopic x,CancellationToken t=default)=>Task.CompletedTask;public Task DeleteTopicAsync(long i,CancellationToken t=default)=>Task.CompletedTask;public Task ReorderTopicsAsync(long i,IReadOnlyList<long> ids,CancellationToken t=default)=>Task.CompletedTask;public Task<MessageReference> AddReferenceAsync(MessageReference x,CancellationToken t=default)=>throw new NotSupportedException();public Task<IReadOnlyList<MessageReference>> GetReferencesAsync(long i,CancellationToken t=default)=>Task.FromResult<IReadOnlyList<MessageReference>>([]);public Task UpdateReferenceAsync(MessageReference x,CancellationToken t=default)=>Task.CompletedTask;public Task DeleteReferenceAsync(long i,CancellationToken t=default)=>Task.CompletedTask;public Task<int> GetNextReferenceOrderAsync(long i,CancellationToken t=default)=>Task.FromResult(0);public Task DeleteAsync(long i,CancellationToken t=default)=>throw new NotSupportedException();}
    private sealed class Clip:IClipboardService{public string? Text;public Task SetTextAsync(string text,CancellationToken t=default){Text=text;return Task.CompletedTask;}}
    private sealed class Navigator:IAppNavigator{public string? Route;public IReadOnlyDictionary<string,object>? Parameters;public Task GoToAsync(string route,IReadOnlyDictionary<string,object>? parameters=null,CancellationToken cancellationToken=default){Route=route;Parameters=parameters;return Task.CompletedTask;}public Task GoBackAsync(CancellationToken cancellationToken=default)=>Task.CompletedTask;}
}

#pragma warning restore xUnit1051
