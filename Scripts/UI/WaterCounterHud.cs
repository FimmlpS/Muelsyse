using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using Muelsyse.Scripts.Relics;

namespace Muelsyse.Scripts.UI;

/// <summary>
/// A small teardrop at the right tip of the health bar of whoever holds 生命吊坠, showing its
/// counter so the player can see how close the next manifold / 溯源 is without opening the relic.
/// Mounted on <see cref="NHealthBar.SetCreature"/>, the same entry point Reed uses for its spark
/// counter.
/// </summary>
[HarmonyPatch(typeof(NHealthBar), nameof(NHealthBar.SetCreature))]
public static class WaterCounterHealthBarPatch
{
    [HarmonyPostfix]
    public static void Postfix(NHealthBar __instance, Creature creature)
    {
        WaterCounterHud.Attach(__instance, creature);
    }
}

[HarmonyPatch(typeof(NCreatureStateDisplay), nameof(NCreatureStateDisplay.SetCreatureBounds))]
public static class WaterCounterLayoutPatch
{
    [HarmonyPostfix]
    public static void Postfix(NCreatureStateDisplay __instance)
    {
        WaterCounterHud.Relayout(__instance);
    }
}

/// <summary>
/// The HP bar容器 is resized a frame after SetCreature (and again whenever the creature bounds change),
/// so anchor once more the moment its geometry actually settles — same hook Reed uses for its bars.
/// </summary>
[HarmonyPatch(typeof(NHealthBar), "SetHpBarContainerSizeWithOffsetsImmediately")]
public static class WaterCounterGeometryPatch
{
    [HarmonyPostfix]
    public static void Postfix(NHealthBar __instance)
    {
        WaterCounterHud.FollowBar(__instance);
    }
}

public static class WaterCounterHud
{
    public const string NodeName = "MuelsyseWaterCounter";

    public static void Attach(NHealthBar bar, Creature creature)
    {
        NCreatureStateDisplay? display = FindDisplay(bar);
        if (display == null)
        {
            return;
        }

        WaterCounterIcon? existing = display.GetNodeOrNull<WaterCounterIcon>(NodeName);
        if (existing != null && !GodotObject.IsInstanceValid(existing))
        {
            existing = null;
        }

        // Whoever holds 生命吊坠 gets the droplet. Don't filter through LocalContext: the health bar
        // is created before the local player's combat state is reliably resolvable, and the relic
        // itself is the authoritative owner of the counter.
        LifeCircle? relic = creature.Player?.Relics.OfType<LifeCircle>().FirstOrDefault();

        if (relic == null)
        {
            if (existing != null)
            {
                display.RemoveChild(existing);
                existing.QueueFree();
            }

            return;
        }

        if (existing != null)
        {
            existing.Bind(bar, creature);
            return;
        }

        WaterCounterIcon icon = new() { Name = NodeName };
        display.AddChild(icon);
        icon.Bind(bar, creature);
    }

    public static void Relayout(NCreatureStateDisplay display)
    {
        display.GetNodeOrNull<WaterCounterIcon>(NodeName)?.FollowBar();
    }

    public static void FollowBar(NHealthBar bar)
    {
        FindDisplay(bar)?.GetNodeOrNull<WaterCounterIcon>(NodeName)?.FollowBar();
    }

    private static NCreatureStateDisplay? FindDisplay(Node node)
    {
        for (Node? current = node.GetParent(); current != null; current = current.GetParent())
        {
            if (current is NCreatureStateDisplay display)
            {
                return display;
            }
        }

        return null;
    }
}

/// <summary>The teardrop itself: a drawn droplet, the counter in the middle, and a hover tip.</summary>
public partial class WaterCounterIcon : Control
{
    private static readonly Vector2 IconSize = new(26f, 34f);

    /// <summary>How far the droplet is pushed over the bar's right edge, in pixels.</summary>
    private const float BarOverlap = 8f;

    private static readonly Color Fill = new("4fc3f7");
    private static readonly Color Outline = new("0b3a52");

    private NHealthBar? _bar;
    private Creature? _creature;
    private Label? _label;
    private NHoverTipSet? _tip;

    public WaterCounterIcon()
    {
        Size = IconSize;
        CustomMinimumSize = IconSize;
        MouseFilter = MouseFilterEnum.Stop;
        SetProcess(true);
    }

    public void Bind(NHealthBar bar, Creature creature)
    {
        _bar = bar;
        _creature = creature;
        FollowBar();
    }

    private const float BodyCenterY = 0.62f;
    private const float BodyRadiusX = 0.40f;

    public override void _Ready()
    {
        // Centre the number on the round belly of the droplet rather than on the whole control:
        // the pointed tip above it is too narrow to hold the glyph.
        float radius = IconSize.X * BodyRadiusX;
        _label = new Label
        {
            Position = new Vector2(0f, IconSize.Y * BodyCenterY - radius),
            Size = new Vector2(IconSize.X, radius * 2f),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };
        _label.AddThemeFontSizeOverride("font_size", 16);
        _label.AddThemeColorOverride("font_color", new Color("ffffff"));
        _label.AddThemeColorOverride("font_outline_color", Outline);
        _label.AddThemeConstantOverride("outline_size", 6);
        AddChild(_label);

        MouseEntered += ShowTip;
        MouseExited += HideTip;
        FollowBar();
    }

    public override void _Process(double delta)
    {
        LifeCircle? relic = Relic();
        if (_label != null)
        {
            _label.Text = (relic?.WaterCount ?? 0).ToString();
        }

        FollowBar();
        Visible = relic != null && BarRect() != null;
    }

    public override void _Draw()
    {
        float w = Size.X;
        float h = Size.Y;
        Vector2 center = new(w * 0.5f, h * BodyCenterY);
        float radius = w * BodyRadiusX;

        Vector2 apex = new(w * 0.5f, 1f);
        Vector2 left = new(center.X - radius * 0.86f, center.Y - radius * 0.52f);
        Vector2 right = new(center.X + radius * 0.86f, center.Y - radius * 0.52f);

        DrawCircle(center, radius + 2f, Outline);
        DrawColoredPolygon([apex, left, right], Outline);
        DrawCircle(center, radius, Fill);
        DrawColoredPolygon([apex, left, right], Fill);
    }

    /// <summary>
    /// The drawn bar is <see cref="NHealthBar.HpBarContainer"/>, not the NHealthBar control itself:
    /// the latter is the whole state-display anchor and its rect has nothing to do with the visible
    /// capsule. Anchor to the container's global rect and re-run every frame so the intro tween and
    /// any mid-combat resize keep the droplet glued to the right tip of the bar.
    /// </summary>
    public void FollowBar()
    {
        if (GetParent() == null || !IsInsideTree() || BarRect() is not { } rect)
        {
            return;
        }

        if (Size != IconSize)
        {
            Size = IconSize;
            QueueRedraw();
        }

        // Tucked into the bar rather than floating next to it: the droplet's outline sits on top of
        // the bar's right cap so the two read as one piece of UI.
        GlobalPosition = new Vector2(rect.End.X - BarOverlap, rect.Position.Y + (rect.Size.Y - Size.Y) * 0.5f);
    }

    private Rect2? BarRect()
    {
        if (_bar == null || !GodotObject.IsInstanceValid(_bar) || !_bar.Visible)
        {
            return null;
        }

        Control? container = _bar.HpBarContainer;
        if (container == null || !GodotObject.IsInstanceValid(container) || !container.Visible)
        {
            return null;
        }

        return container.GetGlobalRect();
    }

    private LifeCircle? Relic()
    {
        return _creature?.Player?.Relics.OfType<LifeCircle>().FirstOrDefault();
    }

    private void ShowTip()
    {
        LocString title = new("static_hover_tips", "MUELSYSE-WATER_COUNTER.title");
        LocString description = new("static_hover_tips", "MUELSYSE-WATER_COUNTER.description");
        _tip = NHoverTipSet.CreateAndShow(this, new HoverTip(title, description), HoverTipAlignment.Right);
    }

    private void HideTip()
    {
        if (_tip != null && GodotObject.IsInstanceValid(_tip))
        {
            NHoverTipSet.Remove(this);
        }

        _tip = null;
    }
}
