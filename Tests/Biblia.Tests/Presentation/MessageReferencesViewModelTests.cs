using Biblia.Domain.Entities;
using Biblia.Domain.Enums;
using Biblia.Presentation.ViewModels;
using Xunit;

namespace Biblia.Tests.Presentation;

public sealed class MessageReferencesViewModelTests
{
    [Fact]
    public void VerseTouch_SelectsClearsAndBuildsOrderedRange()
    {
        var viewModel = CreateViewModel();
        var verse29 = Verse(29); var verse30 = Verse(30); var verse31 = Verse(31);
        viewModel.Verses.Add(verse29); viewModel.Verses.Add(verse30); viewModel.Verses.Add(verse31);

        viewModel.ToggleVerseCommand.Execute(verse29);
        Assert.True(verse29.IsSelected); Assert.False(verse30.IsSelected); Assert.Equal(1, viewModel.SelectionCount);

        viewModel.ToggleVerseCommand.Execute(verse29);
        Assert.All(viewModel.Verses, item => Assert.False(item.IsSelected)); Assert.Equal(0, viewModel.SelectionCount);

        viewModel.ToggleVerseCommand.Execute(verse31); viewModel.ToggleVerseCommand.Execute(verse29);
        Assert.All(viewModel.Verses, item => Assert.True(item.IsSelected)); Assert.Equal(3, viewModel.SelectionCount);
    }

    [Fact]
    public void ThemeSelection_AlwaysKeepsOnlyOneThemeSelected()
    {
        var viewModel = CreateViewModel();
        var holiness = new ThemeSelectionItem(new Theme(1,"Santidade","#0066CC",null,default,default));
        var promises = new ThemeSelectionItem(new Theme(2,"Promessas","#D4AF37",null,default,default));
        viewModel.ThemeOptions.Add(holiness); viewModel.ThemeOptions.Add(promises);

        viewModel.SelectThemeCommand.Execute(holiness); viewModel.SelectThemeCommand.Execute(promises);

        Assert.False(holiness.IsSelected); Assert.True(promises.IsSelected); Assert.Equal("Promessas",viewModel.SelectedThemeText);
    }

    [Theory]
    [InlineData(MessageType.Message, "Mensagem")]
    [InlineData(MessageType.Sermon, "Pregação")]
    [InlineData(MessageType.Study, "Estudo")]
    [InlineData(MessageType.Devotional, "Devocional")]
    public void MessageType_UsesPortugueseLabels(MessageType type, string expected)
    {
        var viewModel = CreateViewModel();
        viewModel.SelectedMessage = new Message(1, "Título", null, type, default, default);

        Assert.Equal(expected, viewModel.MessageType);
    }

    [Fact]
    public void SearchResult_MarkAlreadyAdded_UpdatesResultState()
    {
        var item = Verse(16);

        item.SetSelected(true);
        item.MarkAlreadyAdded();
        item.SetSelected(false);

        Assert.True(item.IsAlreadyAdded);
        Assert.False(item.IsSelected);
        Assert.Equal("Já adicionada a um tema", item.AddedStatus);
    }

    private static BibleVerseItemViewModel Verse(int number) => new(new BibleVerse("ACF",1,"Gênesis",1,number,$"Versículo {number}"));
    private static MessageReferencesViewModel CreateViewModel() => new(null!,null!,null!,null!,null!,null!,null!);
}
