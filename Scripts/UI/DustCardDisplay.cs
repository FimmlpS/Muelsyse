using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using Muelsyse.Scripts.Mechanics;

namespace Muelsyse.Scripts.UI;

/// <summary>
/// Marks the temporary random cards that 遗尘 adds to a 漫步 with a gold "临时" tag while the
/// selection is open. The tag disappears as soon as the card is picked — from then on it is an
/// ordinary free card, so there is nothing left to call out.
/// </summary>
public static class DustCardDisplay
{
    private const string BadgeName = "MuelsyseDustBadge";

    public static void Clear(NCard node)
    {
        if (node.OverlayContainer is not Control overlay)
        {
            return;
        }

        if (overlay.GetNodeOrNull<Control>(BadgeName) is not { } badge)
        {
            return;
        }

        overlay.RemoveChildSafely(badge);
        badge.QueueFree();
    }

    public static void Refresh(NCard node)
    {
        if (!node.IsNodeReady() || node.Model == null || node.OverlayContainer is not Control overlay)
        {
            return;
        }

        if (!RemainDustMarker.IsMarked(node.Model))
        {
            Clear(node);
            return;
        }

        Control? existing = overlay.GetNodeOrNull<Control>(BadgeName);
        if (existing != null && GodotObject.IsInstanceValid(existing))
        {
            return;
        }

        overlay.AddChild(BuildBadge());
    }

    private static Control BuildBadge()
    {
        Panel panel = new()
        {
            Name = BadgeName,
            Position = new Vector2(-28f, NCard.defaultSize.Y / 2f - 36f),
            Size = new Vector2(56f, 22f),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };

        StyleBoxFlat style = new()
        {
            BgColor = new Color(0.17f, 0.11f, 0f, 0.88f),
            BorderColor = new Color("ffcf4d"),
            AntiAliasing = true,
            ShadowColor = new Color(0f, 0f, 0f, 0.4f),
            ShadowSize = 2
        };
        style.SetCornerRadiusAll(6);
        style.SetBorderWidthAll(2);
        panel.AddThemeStyleboxOverride("panel", style);

        Label label = new()
        {
            Text = "临时",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        label.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        label.AddThemeFontSizeOverride("font_size", 14);
        label.AddThemeColorOverride("font_color", new Color("ffe9a8"));
        panel.AddChild(label);

        return panel;
    }
}
