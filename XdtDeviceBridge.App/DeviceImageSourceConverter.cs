using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace XdtDeviceBridge.App;

public sealed class DeviceImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string imagePath || string.IsNullOrWhiteSpace(imagePath))
        {
            return null;
        }

        try
        {
            return LoadImageWithoutFileLock(imagePath.Trim());
        }
        catch
        {
            return null;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static ImageSource? LoadImageWithoutFileLock(string imagePath)
    {
        if (Uri.TryCreate(imagePath, UriKind.Absolute, out var absoluteUri)
            && absoluteUri.IsFile)
        {
            return LoadLocalFileWithoutLock(absoluteUri.LocalPath);
        }

        if (File.Exists(imagePath))
        {
            return LoadLocalFileWithoutLock(imagePath);
        }

        var uri = Uri.TryCreate(imagePath, UriKind.Absolute, out absoluteUri)
            ? absoluteUri
            : new Uri(imagePath, UriKind.RelativeOrAbsolute);

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.UriSource = uri;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    private static ImageSource LoadLocalFileWithoutLock(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }
}
