using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Muelsyse.Scripts.Mechanics;

/// <summary>
/// The attack number floating above a manifold. It shows the manifold's own attack value, and while the
/// player hovers, selects or plays a 协同 attack card it appends that card's halved attack in a golden color.
/// </summary>
public partial class ManifoldIntentDisplay : Control
{
    private const string DisplayName = "MuelsyseIntent";
    private const float RefreshInterval = 0.1f;

    private RichTextLabel _label = null!;
    private Creature? _manifold;
    private Control? _anchor;
    private float _timer;

    public static void Attach(Creature manifold)
    {
        NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(manifold);
        if (creatureNode == null || creatureNode.GetNodeOrNull(DisplayName) != null)
        {
            return;
        }

        ManifoldIntentDisplay display = new()
        {
            Name = DisplayName,
            MouseFilter = MouseFilterEnum.Ignore
        };
        display._manifold = manifold;
        display._anchor = creatureNode.IntentContainer;

        // Deliberately a sibling of the intent container rather than a child of it:
        // NCreature.UpdateIntent() prunes every IntentContainer child beyond the creature's intent
        // count, and it runs on PrepareForNextTurn and every time a power is applied to the
        // creature. A manifold has no intents of its own, so the pruning would delete this display.
        // _Process mirrors the container's position so the number still follows UpdateBounds.
        creatureNode.AddChildSafely(display);
    }

    public override void _Ready()
    {
        _label = new RichTextLabel
        {
            BbcodeEnabled = true,
            ScrollActive = false,
            AutowrapMode = TextServer.AutowrapMode.Off,
            MouseFilter = MouseFilterEnum.Ignore,
            Size = new Vector2(200f, 48f),
            Position = new Vector2(-100f, -52f)
        };
        _label.AddThemeFontSizeOverride("normal_font_size", 30);
        _label.AddThemeConstantOverride("outline_size", 8);
        _label.AddThemeColorOverride("font_outline_color", Colors.Black);
        this.AddChildSafely(_label);
        Refresh();
    }

    public override void _Process(double delta)
    {
        FollowAnchor();

        _timer += (float)delta;
        if (_timer < RefreshInterval)
        {
            return;
        }

        _timer = 0f;
        Refresh();
    }

    /// <summary>
    /// Keeps the display where it used to sit as a child of the intent container. The container is a
    /// centre-aligned HBoxContainer, so a zero-sized child is laid out at the horizontal centre of
    /// the container, not at its top-left corner: mirror <c>Position + Size.X / 2</c>, not Position.
    /// </summary>
    private void FollowAnchor()
    {
        if (_anchor == null || !IsInstanceValid(_anchor))
        {
            return;
        }

        Position = _anchor.Position + new Vector2(_anchor.Size.X * 0.5f, 0f);
    }

    private void Refresh()
    {
        if (_manifold == null || !_manifold.IsAlive)
        {
            Visible = false;
            return;
        }

        Visible = true;
        int attack = _manifold.GetPower<StrengthPower>()?.Amount ?? 0;

        CardModel? card = TogetherListener.CurrentTogetherCard();
        decimal bonus = card != null ? TogetherListener.DamageBonus(card) : 0m;

        string text = attack.ToString();
        if (bonus > 0m)
        {
            text += $" [color=#FFD84D]+{(int)bonus}[/color]";
        }

        _label.Text = $"[center]{text}[/center]";
    }
}
