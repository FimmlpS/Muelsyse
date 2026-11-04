using BaseLib.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using Muelsyse.Scripts.Mechanics.Cultivate;

namespace Muelsyse.Scripts.UI;

/// <summary>
/// Draws the cultivation icon row and the 失肥 stamp on every card node that carries a
/// <see cref="CultivateModifier"/>. Hooked from <see cref="NCard.UpdateVisuals"/>.
/// </summary>
public static class CultivateCardDisplay
{
    private const string StripName = "MuelsyseCultivateStrip";
    private const string DryName = "MuelsyseCultivateDry";

    private const float IconSize = 22f;
    private const float IconSpacing = 27f;

    /// <summary>Both the icon row and the 失肥 stamp sit half an icon higher than the title.</summary>
    private const float IconLift = IconSize * 0.5f;

    /// <summary>The icon row sits a few icons to the left of the title's centre.</summary>
    private const float IconOffsetX = -IconSize * 3.5f;

    private static readonly (CultivateAim Aim, string Icon)[] Order =
    [
        (CultivateAim.Attack, "attack"),
        (CultivateAim.Block, "block"),
        (CultivateAim.Draw, "draw"),
        (CultivateAim.Energy, "energy"),
        (CultivateAim.Water, "water"),
        (CultivateAim.Spread, "spread"),
        (CultivateAim.Parasitism, "parasitism"),
        (CultivateAim.Cure, "cure"),
        (CultivateAim.WaterGrow, "water2")
    ];

    /// <summary>
    /// Removes every icon this display ever added to the node. Card nodes are pooled and reused for a
    /// different card model; <see cref="NCard.Model"/> assignment refreshes the vanilla overlay but
    /// leaves our children behind, so the model setter path calls this first.
    /// </summary>
    public static void Clear(NCard node)
    {
        if (node.OverlayContainer is not Control overlay)
        {
            return;
        }

        Remove(overlay, StripName);
        Remove(overlay, DryName);
    }

    /// <summary>
    /// Detaches the icon immediately instead of only queueing it for deletion: QueueFree is deferred
    /// to the end of the frame, and <see cref="Refresh"/> may run again in the same frame and would
    /// otherwise still find (and keep) the stale node.
    /// </summary>
    private static void Remove(Control overlay, string name)
    {
        if (overlay.GetNodeOrNull<Control>(name) is not { } icon)
        {
            return;
        }

        overlay.RemoveChildSafely(icon);
        icon.QueueFree();
    }

    public static void Refresh(NCard node)
    {
        if (!node.IsNodeReady() || node.Model == null || node.OverlayContainer is not Control overlay)
        {
            return;
        }

        CultivateModifier? modifier = node.Model.GetModifier<CultivateModifier>();
        bool dry = CultivateDryModifier.IsDry(node.Model);

        Control? strip = overlay.GetNodeOrNull<Control>(StripName);
        if (modifier == null && !dry)
        {
            Clear(node);
            return;
        }

        string signature = Signature(modifier, dry);
        if (strip != null && strip.HasMeta("signature") && strip.GetMeta("signature").AsString() == signature)
        {
            return;
        }

        // The signature changed (or only the bonus values did): rebuild both decorations.
        Remove(overlay, StripName);
        Remove(overlay, DryName);

        if (modifier != null)
        {
            overlay.AddChild(BuildStrip(node, overlay, modifier, signature));
        }

        if (dry)
        {
            overlay.AddChild(BuildDryStamp());
        }
    }

    private static string Signature(CultivateModifier? modifier, bool dry)
    {
        if (modifier == null)
        {
            return dry ? "dry" : "";
        }

        return string.Join(',', Order.Select(pair => modifier.Get(pair.Aim))) + (dry ? "|dry" : "");
    }

    private static Control BuildStrip(NCard node, Control overlay, CultivateModifier modifier, string signature)
    {
        List<(string Icon, int Amount)> values = Order
            .Select(pair => (pair.Icon, modifier.Get(pair.Aim)))
            .Where(pair => pair.Item2 != 0)
            .ToList();

        Control strip = new()
        {
            Name = StripName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Position = StripOrigin(node, overlay),
            Size = new Vector2(values.Count * IconSpacing, IconSize)
        };
        strip.SetMeta("signature", signature);

        float totalWidth = values.Count * IconSpacing;
        for (int i = 0; i < values.Count; i++)
        {
            (string icon, int amount) = values[i];
            Control cell = new()
            {
                MouseFilter = Control.MouseFilterEnum.Ignore,
                Position = new Vector2(-totalWidth / 2f + i * IconSpacing, 0),
                Size = new Vector2(IconSize, IconSize),
                TooltipText = CultivateText.Tooltip(icon, amount)
            };

            TextureRect texture = new()
            {
                Texture = GD.Load<Texture2D>($"res://Muelsyse/images/icon/cultivate/{icon}.png"),
                Position = Vector2.Zero,
                Size = new Vector2(IconSize, IconSize),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            cell.AddChild(texture);

            Label label = new()
            {
                Text = amount.ToString(),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Position = new Vector2(10, 7),
                Size = new Vector2(20, 18),
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            label.AddThemeFontSizeOverride("font_size", 18);
            label.AddThemeColorOverride("font_color", amount < 0 ? new Color("ff7b7b") : new Color("eaffea"));
            label.AddThemeColorOverride("font_outline_color", new Color("072414"));
            label.AddThemeConstantOverride("outline_size", 6);
            cell.AddChild(label);

            strip.AddChild(cell);
        }

        return strip;
    }

    /// <summary>
    /// A single row just above the card title, centered on it. The strip is parented to the overlay
    /// container (whose origin is the card center), so the card-local offset is derived from the
    /// title label's global rectangle.
    /// </summary>
    private static Vector2 StripOrigin(NCard node, Control overlay)
    {
        Control? title = node.GetNodeOrNull<Control>("%TitleLabel");
        if (title == null || !title.IsInsideTree() || !overlay.IsInsideTree())
        {
            return new Vector2(IconOffsetX, -NCard.defaultSize.Y / 2f - IconSize - 4f - IconLift);
        }

        Rect2 titleRect = title.GetGlobalRect();
        Vector2 scale = node.GetGlobalTransform().Scale;
        if (Mathf.IsZeroApprox(scale.X))
        {
            scale.X = 1f;
        }

        if (Mathf.IsZeroApprox(scale.Y))
        {
            scale.Y = 1f;
        }

        Vector2 overlayOrigin = overlay.GetGlobalRect().Position;
        float x = (titleRect.Position.X + titleRect.Size.X / 2f - overlayOrigin.X) / scale.X + IconOffsetX;
        float y = (titleRect.Position.Y - overlayOrigin.Y) / scale.Y - IconSize - 4f - IconLift;
        return new Vector2(x, y);
    }

    private static Control BuildDryStamp()
    {
        TextureRect stamp = new()
        {
            Name = DryName,
            Texture = GD.Load<Texture2D>("res://Muelsyse/images/icon/cultivate/dry.png"),
            Position = new Vector2(-42, -42 - IconLift),
            Size = new Vector2(84, 84),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Modulate = new Color(1, 1, 1, 0.85f)
        };
        return stamp;
    }
}
