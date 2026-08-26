using Biblia.Application.Interfaces.Repositories;
using Biblia.Application.Services;
using Biblia.Domain.Entities;
using Biblia.Domain.Exceptions;
using Biblia.Presentation.ViewModels;
using Xunit;

namespace Biblia.Tests.Presentation;

public sealed class ThemesViewModelTests
{
    [Fact]
    public async Task TwoCreates_KeepDistinctIdsAndRecords()
    {
        var (viewModel, repository) = Create();
        viewModel.Name = "Esperança";
        await viewModel.SaveAsync();
        viewModel.Name = "Salvação";
        await viewModel.SaveAsync();

        Assert.Equal(2, repository.Items.Count);
        Assert.NotEqual(repository.Items[0].Id, repository.Items[1].Id);
        Assert.Equal(["Esperança", "Salvação"], repository.Items.Select(x => x.Name));
    }

    [Fact]
    public async Task ExplicitEdit_UpdatesSameRecordWithoutCreatingAnother()
    {
        var (viewModel, repository) = Create();
        viewModel.Name = "Esperança";
        await viewModel.SaveAsync();
        viewModel.SelectedTheme = repository.Items.Single();
        await viewModel.BeginEditAsync();
        var id = viewModel.EditingThemeId;
        viewModel.Name = "Esperança em Cristo";
        await viewModel.SaveAsync();

        Assert.Single(repository.Items);
        Assert.Equal(id, repository.Items[0].Id);
        Assert.Equal("Esperança em Cristo", repository.Items[0].Name);
    }

    [Fact]
    public async Task NewAfterEdit_CreatesAnotherRecord()
    {
        var (viewModel, repository) = Create();
        viewModel.Name = "A";
        await viewModel.SaveAsync();
        viewModel.SelectedTheme = repository.Items.Single();
        await viewModel.BeginEditAsync();
        await viewModel.BeginCreateAsync();
        viewModel.Name = "B";
        await viewModel.SaveAsync();

        Assert.Equal(["A", "B"], repository.Items.Select(x => x.Name));
        Assert.Equal(2, repository.Items.Select(x => x.Id).Distinct().Count());
    }

    [Fact]
    public async Task CancelEditThenCreate_DoesNotUpdateEditedRecord()
    {
        var (viewModel, repository) = Create();
        viewModel.Name = "A";
        await viewModel.SaveAsync();
        viewModel.SelectedTheme = repository.Items.Single();
        await viewModel.BeginEditAsync();
        await viewModel.ResetEditorAsync();
        viewModel.Name = "B";
        await viewModel.SaveAsync();

        Assert.Equal(["A", "B"], repository.Items.Select(x => x.Name));
    }

    [Fact]
    public async Task DuplicateName_IsFriendlyAndDoesNotChangeFirstRecord()
    {
        var (viewModel, repository) = Create();
        viewModel.Name = "Esperança";
        await viewModel.SaveAsync();
        viewModel.Name = "esperança";
        await viewModel.SaveAsync();

        Assert.Single(repository.Items);
        Assert.Equal("Esperança", repository.Items[0].Name);
        Assert.Equal("Já existe um tema com esse nome.", viewModel.Status);
    }

    private static (ThemesViewModel ViewModel, Repository Repository) Create()
    {
        var repository = new Repository();
        return (new ThemesViewModel(new ThemeService(repository)), repository);
    }

    private sealed class Repository : IThemeRepository
    {
        public List<Theme> Items { get; } = [];
        private long _nextId = 1;
        public Task<Theme> CreateAsync(string name, string? colorHex, string? description, CancellationToken cancellationToken = default)
        {
            if (Items.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))) throw new DomainValidationException("Já existe um tema com esse nome.");
            var now = DateTimeOffset.UtcNow;
            var value = new Theme(_nextId++, name, colorHex, description, now, now);
            Items.Add(value);
            return Task.FromResult(value);
        }
        public Task<Theme?> GetAsync(long id, CancellationToken cancellationToken = default) => Task.FromResult(Items.SingleOrDefault(x => x.Id == id));
        public Task<IReadOnlyList<Theme>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Theme>>(Items.ToArray());
        public Task<IReadOnlyList<Theme>> SearchAsync(string query, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Theme>>(Items.Where(x => x.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToArray());
        public Task<ThemeUsage> GetUsageAsync(long id, CancellationToken cancellationToken = default) => Task.FromResult(new ThemeUsage(0, 0));
        public Task UpdateAsync(Theme theme, CancellationToken cancellationToken = default) { var index = Items.FindIndex(x => x.Id == theme.Id); if (index < 0) throw new KeyNotFoundException(); Items[index] = theme; return Task.CompletedTask; }
        public Task DeleteAsync(long id, CancellationToken cancellationToken = default) { Items.RemoveAll(x => x.Id == id); return Task.CompletedTask; }
    }
}
