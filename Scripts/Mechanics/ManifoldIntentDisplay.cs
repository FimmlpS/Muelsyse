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
    private float _timer;

    public static void Attach(Creature manifold)
    {
        NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(manifold);
        if (creatureNode == null || creatureNode.IntentContainer.GetNodeOrNull(DisplayName) != null)
        {
            return;
        }

        ManifoldIntentDisplay display = new()
        {
            Name = DisplayName,
            MouseFilter = MouseFilterEnum.Ignore
        };
        display._manifold = manifold;
        creatureNode.IntentContainer.AddChildSafely(display);
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
            Position = new Vector2(-100f, -10f)
        };
        _label.AddThemeFontSizeOverride("normal_font_size", 30);
        _label.AddThemeConstantOverride("outline_size", 8);
        _label.AddThemeColorOverride("font_outline_color", Colors.Black);
        this.AddChildSafely(_label);
        Refresh();
    }

    public override void _Process(double delta)
    {
        _timer += (float)delta;
        if (_timer < RefreshInterval)
        {
            return;
        }

        _timer = 0f;
        Refresh();
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
