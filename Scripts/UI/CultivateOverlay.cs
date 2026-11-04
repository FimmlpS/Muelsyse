using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Mechanics.Cultivate;

namespace Muelsyse.Scripts.UI;

/// <summary>
/// The cultivation selection screen: the target card before and after cultivation, a row of
/// pulsing arrows between them, and numbered, curved arrows from every auto-filled sub material
/// in the hand towards the material row.
/// </summary>
public partial class CultivateOverlay : Control
{
    private static readonly Vector2 PreviewScale = new(0.92f, 0.92f);

    private static CanvasLayer? _layer;
    private static CultivateOverlay? _instance;

    private CultivateSession? _session;
    private List<CardModel> _materials = [];

    private Control? _beforeAnchor;
    private Control? _afterAnchor;
    private Control? _labelLayer;
    private NCardPhase _phase = NCardPhase.None;

    private readonly List<Label> _numbers = [];

    private static Texture2D ArrowTexture =>
        ResourceLoader.Load<Texture2D>("res://images/ui/cards/upgrade_preview/upgrade_arrow.png");

    private static Texture2D ArrowHeadTexture =>
        ResourceLoader.Load<Texture2D>("res://images/ui/combat/targeting_arrow_head.png");

    private static Texture2D ArrowSegmentTexture =>
        ResourceLoader.Load<Texture2D>("res://images/ui/combat/targeting_arrow_segment.png");

    private enum NCardPhase
    {
        None,
        Waiting,
        Previewing
    }

    public static void Show(CultivateSession session)
    {
        Dismiss();

        _layer = new CanvasLayer { Layer = 10, Name = "MuelsyseCultivateOverlay" };
        // Starts hidden: Attach positions the preview anchors and builds the cards, and only then
        // makes the overlay visible. Without this, the cards could be drawn for one frame at the
        // default anchor position before the first layout pass.
        _instance = new CultivateOverlay { Name = "CultivateOverlay", Visible = false };
        _layer.AddChildSafely(_instance);

        Node? host = NCombatRoom.Instance as Node
                     ?? (Engine.GetMainLoop() as SceneTree)?.Root;
        host?.AddChildSafely(_layer);

        _instance.Attach(session);
    }

    public static void Dismiss()
    {
        _instance?.Cleanup();
        _layer?.QueueFreeSafely();
        _instance = null;
        _layer = null;
    }

    private void Attach(CultivateSession session)
    {
        _session = session;
        _materials = CultivateCmd.HandMaterials(session.Player, session.Target);
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        BuildAnchors();
        UpdateSize();
        // The preview anchors have to be in place before any card is added to them: a card built on
        // an anchor that has not been laid out yet is drawn at the screen origin for one frame (the
        // flash at the bottom of the screen) and only lands in the right spot on the next _Process.
        PositionAnchors();
        RebuildPreviewCards();
        _phase = NCardPhase.Waiting;
        Visible = true;
    }

    private void BuildAnchors()
    {
        _beforeAnchor = new Control { Name = "Before", MouseFilter = MouseFilterEnum.Ignore };
        _afterAnchor = new Control { Name = "After", MouseFilter = MouseFilterEnum.Ignore };
        _labelLayer = new Control { Name = "Numbers", MouseFilter = MouseFilterEnum.Ignore };
        this.AddChildSafely(_beforeAnchor);
        this.AddChildSafely(_afterAnchor);
        this.AddChildSafely(_labelLayer);
    }

    private void Cleanup()
    {
        _session = null;
        _numbers.Clear();
        if (IsInsideTree())
        {
            this.QueueFreeSafely();
        }
    }

    private void UpdateSize()
    {
        Size = GetViewportRect().Size;
    }

    public override void _Process(double delta)
    {
        if (_session == null)
        {
            return;
        }

        UpdateSize();
        TrackSelection();
        PositionAnchors();
        PositionNumbers();
        QueueRedraw();
    }

    private void TrackSelection()
    {
        if (_session == null)
        {
            return;
        }

        CardModel? selected = NPlayerHand.Instance?
            .GetNodeOrNull<NSelectedHandCardContainer>("%SelectedHandCardContainer")?
            .Holders
            .Select(holder => holder.CardModel)
            .FirstOrDefault(card => card != null);

        CardModel? main = selected != null && selected != _session.Target && _materials.Contains(selected)
            ? selected
            : null;

        if (main == _session.Main)
        {
            return;
        }

        // No main material means no sub materials either: the bonus and the upgraded card reset.
        _session.SetMain(main);
        _phase = main == null ? NCardPhase.Waiting : NCardPhase.Previewing;
        RebuildPreviewCards();
    }

    private void PositionAnchors()
    {
        Vector2 size = Size;
        if (_beforeAnchor != null)
        {
            _beforeAnchor.Position = new Vector2(size.X * 0.29f, size.Y * 0.48f);
        }

        if (_afterAnchor != null)
        {
            _afterAnchor.Position = new Vector2(size.X * 0.71f, size.Y * 0.48f);
        }
    }

    private void RebuildPreviewCards()
    {
        if (_beforeAnchor == null || _afterAnchor == null || _session == null)
        {
            return;
        }

        // A rebuild can be triggered from TrackSelection, which may run before the first _Process;
        // make sure both anchors are already at their final spot before the new cards are built.
        PositionAnchors();
        ClearChildren(_beforeAnchor);
        ClearChildren(_afterAnchor);

        // Both cards are always visible, even before a main material is picked. They are clones so
        // the comparison shows printed (cultivated) values instead of live combat modifiers.
        CardModel? before = CreatePreviewCard(_session.Target);
        if (before != null)
        {
            AddPreview(_beforeAnchor, before);
        }

        CardModel? after = CreatePreviewCard(_session.Target);
        if (after != null)
        {
            if (_session.Main != null)
            {
                CultivateCmd.Apply(after, _session.Bonus);
            }

            AddPreview(_afterAnchor, after);
        }
    }

    private static CardModel? CreatePreviewCard(CardModel source)
    {
        CardModel? clone = source.CardScope?.CloneCard(source);
        if (clone == null)
        {
            return null;
        }

        // Combat preview type runs the global damage hooks, which is what makes the cultivated
        // attack show up. 协同 itself is suppressed by the preview marker below.
        clone.UpgradePreviewType = CardUpgradePreviewType.Combat;
        TogetherListener.MarkPreviewCard(clone);
        return clone;
    }

    private static void AddPreview(Control anchor, CardModel card)
    {
        NCard? node = NCard.Create(card);
        if (node == null)
        {
            return;
        }

        // The node is borrowed from the shared card pool, so it may still carry the transform of
        // its previous owner (a hand card lives at the bottom of the screen). Reset it before the
        // holder gets it, otherwise that stale position can be rendered for a frame.
        NormalizeCard(node);
        node.Visible = false;

        NPreviewCardHolder? holder = NPreviewCardHolder.Create(node, showHoverTips: false, scaleOnHover: false);
        if (holder == null)
        {
            return;
        }

        holder.FocusMode = FocusModeEnum.None;
        holder.MouseFilter = MouseFilterEnum.Ignore;
        holder.SetCardScale(PreviewScale);
        holder.Visible = false;
        node.MouseFilter = MouseFilterEnum.Ignore;

        anchor.AddChildSafely(holder);

        // Adding the card to the holder reparents it, and Godot's reparent keeps the old global
        // transform. Pin the whole chain back to the anchor before anything is drawn.
        holder.Position = Vector2.Zero;
        holder.Scale = PreviewScale;
        NormalizeCard(node);
        node.UpdateVisuals(card.Pile?.Type ?? PileType.Hand, CardPreviewMode.Normal);

        node.Visible = true;
        holder.Visible = true;
    }

    /// <summary>
    /// Puts a card node back into the state <c>card.tscn</c> defines. The pool only resets the
    /// position, rotation and scale on return (see <c>NCard.OnReturnedFromPool</c>) and leaves the
    /// pivot alone, so a recycled card can be drawn off its holder origin.
    /// </summary>
    private static void NormalizeCard(NCard node)
    {
        node.Position = Vector2.Zero;
        node.Rotation = 0f;
        node.Scale = Vector2.One;
        node.PivotOffset = Vector2.Zero;
        node.TopLevel = false;
        node.Modulate = Colors.White;
        node.SelfModulate = Colors.White;
    }

    private static void ClearChildren(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            // QueueFree only takes effect at the end of the frame; hide the old holder right away so
            // it cannot show through (or next to) the rebuilt cards.
            if (child is CanvasItem item)
            {
                item.Visible = false;
            }

            child.QueueFreeSafely();
        }
    }

    private void PositionNumbers()
    {
        if (_labelLayer == null || _session == null)
        {
            return;
        }

        if (_labelLayer.GetChildCount() != _session.SubMaterials.Count)
        {
            ClearChildren(_labelLayer);
            _numbers.Clear();
            for (int i = 0; i < _session.SubMaterials.Count; i++)
            {
                Label label = new()
                {
                    Text = (i + 1).ToString(),
                    MouseFilter = MouseFilterEnum.Ignore,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    ZIndex = 1
                };
                label.AddThemeFontSizeOverride("font_size", 52);
                label.AddThemeColorOverride("font_color", new Color("ffd85c"));
                label.AddThemeColorOverride("font_outline_color", new Color("4b3a10"));
                label.AddThemeConstantOverride("outline_size", 8);
                _numbers.Add(label);
                _labelLayer.AddChildSafely(label);
            }
        }

        int index = 0;
        foreach (CardModel card in _session.SubMaterials)
        {
            if (index >= _numbers.Count)
            {
                break;
            }

            Label label = _numbers[index];
            Vector2? center = HandCardCenter(card);
            label.Visible = center.HasValue;
            if (center.HasValue)
            {
                label.Size = new Vector2(96, 72);
                label.Position = center.Value + new Vector2(-48, -215);
            }

            index++;
        }
    }

    private static Vector2? HandCardCenter(CardModel card)
    {
        NPlayerHand? hand = NPlayerHand.Instance;
        if (hand == null)
        {
            return null;
        }

        foreach (NHandCardHolder holder in hand.ActiveHolders)
        {
            if (holder.CardNode?.Model == card)
            {
                return holder.CardNode.GetGlobalRect().GetCenter();
            }
        }

        return null;
    }

    public override void _Draw()
    {
        if (_session == null)
        {
            return;
        }

        Vector2 size = Size;
        Vector2 center = size / 2f;
        DrawConnectingArrows(center);
        DrawSubArrows(center);
    }

    private void DrawConnectingArrows(Vector2 center)
    {
        Texture2D arrows = ArrowTexture;
        if (arrows == null)
        {
            return;
        }

        double time = Time.GetTicksMsec() / 1000.0;
        const int count = 5;
        float startX = center.X - count * 32f;
        float y = center.Y - 20f;

        for (int i = 0; i < count; i++)
        {
            float pulse = 0.82f + (float)((Math.Cos(time * 4.0 - i * 0.8) + 1.0) / 8.0);
            Vector2 position = new(startX + i * 64f + 32f, y);
            DrawSetTransform(position, 0f, new Vector2(pulse, pulse));
            DrawTexture(arrows, -arrows.GetSize() / 2f, new Color(1f, 1f, 1f, 0.85f));
        }

        DrawSetTransform(Vector2.Zero);
    }

    private void DrawSubArrows(Vector2 center)
    {
        if (_session!.SubMaterials.Count == 0)
        {
            return;
        }

        Texture2D? head = ArrowHeadTexture;
        Texture2D? segment = ArrowSegmentTexture;
        if (head == null || segment == null)
        {
            return;
        }

        double time = Time.GetTicksMsec() / 1000.0;
        int total = _session.SubMaterials.Count;
        float rowY = center.Y + 135f;
        float spacing = Mathf.Min(64f, (Size.X * 0.5f) / Mathf.Max(1, total));

        for (int i = 0; i < total; i++)
        {
            Vector2? start = HandCardCenter(_session.SubMaterials[i]);
            if (!start.HasValue)
            {
                continue;
            }

            Vector2 end = new(center.X - (spacing * (total - 1)) / 2f + spacing * i, rowY);
            Vector2 control = new(start.Value.X - (end.X - start.Value.X) / 4f,
                end.Y + (end.Y - start.Value.Y) / 2f);

            DrawBezier(start.Value, control, end, segment, time + i * 0.35);

            Vector2 direction = end - control;
            DrawSetTransform(end, direction.Angle(), new Vector2(0.22f, 0.22f));
            DrawTexture(head, -head.GetSize() / 2f, new Color(1f, 1f, 1f, 0.8f));
            DrawSetTransform(Vector2.Zero);
        }

        DrawSetTransform(Vector2.Zero);
    }

    private void DrawBezier(Vector2 start, Vector2 control, Vector2 end, Texture2D segment, double offset)
    {
        const int steps = 22;
        for (int i = 1; i < steps; i++)
        {
            float t = i / (float)steps;
            Vector2 point = start.Lerp(control, t).Lerp(control.Lerp(end, t), t);
            float pulse = 0.5f + 0.5f * (float)Math.Sin(offset * 3.0 + i * 0.5);
            float scale = 0.09f + 0.035f * pulse;
            DrawSetTransform(point, 0f, new Vector2(scale, scale));
            DrawTexture(segment, -segment.GetSize() / 2f, new Color(1f, 1f, 1f, 0.35f + 0.25f * pulse));
        }

        DrawSetTransform(Vector2.Zero);
    }
}
