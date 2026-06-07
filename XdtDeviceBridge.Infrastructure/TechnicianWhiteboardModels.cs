namespace XdtDeviceBridge.Infrastructure;

public sealed record TechnicianWhiteboardState(
    IReadOnlyList<TechnicianWhiteboardTextItem> TextItems,
    IReadOnlyList<TechnicianWhiteboardImageItem> ImageItems)
{
    public static TechnicianWhiteboardState Empty { get; } = new(
        Array.Empty<TechnicianWhiteboardTextItem>(),
        Array.Empty<TechnicianWhiteboardImageItem>());
}

public sealed record TechnicianWhiteboardTextItem(
    string Id,
    string Text,
    double X,
    double Y,
    double Width,
    double Height,
    bool IsBold,
    bool IsItalic,
    bool IsUnderline,
    string Color,
    double FontSize = 16);

public sealed record TechnicianWhiteboardImageItem(
    string Id,
    string ImagePath,
    double X,
    double Y,
    double Width,
    double Height);
