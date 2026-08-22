using Android.Graphics;
using AndroidX.AppCompat.Widget;
using Biblia.Presentation.Components;
using Microsoft.Maui.Handlers;

namespace Biblia.Platforms.Android;

public sealed class CachedPhotoImageHandler : ViewHandler<CachedPhotoImage, AppCompatImageView>
{
    public static readonly IPropertyMapper<CachedPhotoImage, CachedPhotoImageHandler> Mapper =
        new PropertyMapper<CachedPhotoImage, CachedPhotoImageHandler>(ViewMapper)
        {
            [nameof(CachedPhotoImage.SourcePath)] = MapSourcePath
        };

    public CachedPhotoImageHandler() : base(Mapper) { }

    protected override AppCompatImageView CreatePlatformView()
    {
        var view = new AppCompatImageView(Context);
        view.SetScaleType(global::Android.Widget.ImageView.ScaleType.CenterCrop);
        return view;
    }

    protected override void DisconnectHandler(AppCompatImageView platformView)
    {
        platformView.SetImageDrawable(null);
        base.DisconnectHandler(platformView);
    }

    private static void MapSourcePath(CachedPhotoImageHandler handler, CachedPhotoImage image)
    {
        var value = image.SourcePath;
        var path = value?.StartsWith("file:", StringComparison.OrdinalIgnoreCase) == true
            ? new Uri(value).LocalPath
            : value;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            handler.PlatformView.SetImageDrawable(null);
            return;
        }

        var bytes = File.ReadAllBytes(path);
        var bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
        handler.PlatformView.SetImageBitmap(bitmap);
        handler.PlatformView.RequestLayout();
        handler.PlatformView.Invalidate();
        image.InvalidateMeasure();
    }
}
