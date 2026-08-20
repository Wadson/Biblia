using Biblia.Application.Interfaces;
using Biblia.Domain.Entities;
using Biblia.Domain.Enums;
using Biblia.Presentation.ViewModels;
using Xunit;

namespace Biblia.Tests.Presentation;

public sealed class BibleComparisonViewModelTests
{
    [Fact]
    public async Task Initialize_ReceivesReferenceSelectsAllAndComparesAutomatically()
    {
        var service=new Comparison();var vm=new BibleComparisonViewModel(new Versions(),service);
        await vm.InitializeAsync(new Dictionary<string,object>{{"BookReferenceId",1},{"BookName","Gênesis"},{"Chapter",1},{"VerseStart",26},{"VerseEnd",28},{"SourceVersionCode","ACF"}});
        Assert.Equal("Gênesis 1:26–28",vm.ReferenceText);Assert.All(vm.Versions,x=>Assert.True(x.IsSelected));Assert.Equal(2,vm.Items.Count);Assert.True(vm.Items.Single(x=>x.VersionCode=="ACF").IsSource);Assert.Equal((1,1,26,28),service.Reference);
    }
    [Fact]
    public async Task Initialize_InvalidReferenceDoesNotCallComparison()
    {
        var service=new Comparison();var vm=new BibleComparisonViewModel(new Versions(),service);await vm.InitializeAsync(new Dictionary<string,object>());
        Assert.True(vm.HasError);Assert.Null(service.Reference);Assert.Equal("A referência recebida para comparação é inválida.",vm.Status);
    }
    private sealed class Versions:IBibleVersionManager
    {
        private readonly BibleVersionCatalogEntry[] _items=[Entry(1,"ACF"),Entry(2,"NVI")];
        private static BibleVersionCatalogEntry Entry(long id,string code)=>new(id,code,code+" completa","pt-BR",code+".sqlite",code,2,null,null,null,true,true,true,null,null,BibleVersionValidationStatus.Compatible,null);
        public Task<IReadOnlyList<BibleVersionCatalogEntry>> GetVersionsAsync(CancellationToken t=default)=>Task.FromResult<IReadOnlyList<BibleVersionCatalogEntry>>(_items);public Task<BibleVersionCatalogEntry?> GetActiveVersionAsync(CancellationToken t=default)=>Task.FromResult<BibleVersionCatalogEntry?>(_items[0]);public Task InitializeCatalogAsync(CancellationToken t=default)=>Task.CompletedTask;public Task SetActiveVersionAsync(string c,CancellationToken t=default)=>Task.CompletedTask;public Task SetEnabledAsync(string c,bool e,CancellationToken t=default)=>Task.CompletedTask;public Task<string> ResolveDatabasePathAsync(string c,CancellationToken t=default)=>throw new NotSupportedException();public Task<BibleValidationResult> ValidateAsync(string c,CancellationToken t=default)=>throw new NotSupportedException();
    }
    private sealed class Comparison:IBibleComparisonService
    {
        public (int,int,int,int)? Reference;
        public Task<IReadOnlyList<BibleComparisonItem>> CompareAsync(int b,int c,int s,int e,IReadOnlyList<string> versions,CancellationToken t=default){Reference=(b,c,s,e);return Task.FromResult<IReadOnlyList<BibleComparisonItem>>(versions.Select(v=>new BibleComparisonItem(v,BibleComparisonStatus.Available,[new(v,b,"Gênesis",c,s,"texto")],null)).ToArray());}
    }
}
