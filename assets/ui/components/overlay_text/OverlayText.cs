using Godot;
using Hjam.assets.scripts.lib.concurrency;

namespace Hjam.assets.ui.components.overlay_text;

public partial class OverlayText : Control
{
    public static OverlayText CreateInstance(Node parent, string text, (int width, int height)? size = null,
        bool reusable = false)
    {
        var overlayTextScene = GD.Load<PackedScene>("res://assets/ui/components/overlay_text/overlay_text.tscn");

        var overlayText = reusable
            ? parent.GetNodeOrNull<OverlayText>("OverlayText") ?? overlayTextScene.Instantiate<OverlayText>()
            : overlayTextScene.Instantiate<OverlayText>();

        overlayText.ZIndex = 4096;
        
        if (reusable)
        {
            overlayText.Name = "OverlayText";
        }

        var textLabel = overlayText.GetNodeOrNull<RichTextLabel>("Layout/Text");
        if (textLabel != null)
        {
            textLabel.Text = $"[b]{text}";
        }

        var actualSize = size ?? (1920, 1080);
        overlayText.SetSize(new Vector2(actualSize.width, actualSize.height));

        switch (overlayText.GetParent())
        {
            case null:
                parent.AddChild(overlayText);
                break;

            case var existingParent when existingParent != parent:
                overlayText.Reparent(parent);
                break;
        }

        return overlayText;
    }

    public static OverlayText DeleteInstance(Node parent)
    {
        var overlayText = parent.GetNodeOrNull<OverlayText>("OverlayText");
        overlayText?.QueueFree();

        return overlayText!;
    }
}