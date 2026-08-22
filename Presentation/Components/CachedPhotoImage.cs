namespace Biblia.Presentation.Components;

public sealed class CachedPhotoImage : View
{
    public static readonly BindableProperty AspectProperty = BindableProperty.Create(
        nameof(Aspect), typeof(Aspect), typeof(CachedPhotoImage), Microsoft.Maui.Aspect.AspectFill);
    public static readonly BindableProperty SourcePathProperty = BindableProperty.Create(
        nameof(SourcePath), typeof(string), typeof(CachedPhotoImage));

    public Aspect Aspect
    {
        get => (Aspect)GetValue(AspectProperty);
        set => SetValue(AspectProperty, value);
    }

    public string? SourcePath
    {
        get => (string?)GetValue(SourcePathProperty);
        set => SetValue(SourcePathProperty, value);
    }
}
