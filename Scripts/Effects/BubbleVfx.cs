using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Muelsyse.Scripts.Effects;

/// <summary>
/// The water bubbles that rise around Muelsyse and every manifold, ported from the first game's
/// <c>SingleBubbleEffect</c>. A bubble appears just under the creature's feet, swells while rising,
/// then shrinks away at the end of its life; horizontally it drifts out to one side and slows down,
/// so it curves slightly back towards the creature.
/// </summary>
public partial class BubbleVfx : Node2D
{
    private const string NodeName = "MuelsyseBubbles";
    private const string TexturePath = "res://Muelsyse/images/effects/bubble.png";

    /// <summary>The bubble texture is 128x128 in the first game as well.</summary>
    private const float TextureSize = 128f;

    private const float MinScale = 0.26f;
    private const float MaxScale = 0.34f;
    private const float MinDuration = 1.8f;
    private const float MaxDuration = 2.2f;
    private const float MinInterval = 0.25f;
    private const float MaxInterval = 0.45f;

    /// <summary>Sideways drift speed, in the same absolute pixels the first game used.</summary>
    private const float DriftSpeed = 64f;

    private static readonly ConditionalWeakTable<NCreature, BubbleVfx> Attached = new();

    private static Texture2D? _texture;

    private readonly List<Bubble> _bubbles = [];

    private Creature? _creature;
    private Control? _hitbox;
    private float _timer;

    /// <summary>
    /// Starts bubbling around a creature. Safe to call repeatedly: each creature node only ever gets
    /// one effect, and calls made before the combat room has built the node simply do nothing.
    /// </summary>
    public static void Attach(Creature? creature)
    {
        if (creature == null)
        {
            return;
        }

        NCreature? node = NCombatRoom.Instance?.GetCreatureNode(creature);
        if (node == null || Attached.TryGetValue(node, out _))
        {
            return;
        }

        BubbleVfx vfx = new()
        {
            Name = NodeName,
            _creature = creature
        };

        // Registered before AddChild, which may be deferred: a second Attach in the same frame would
        // otherwise not see the node yet and add a duplicate.
        Attached.Add(node, vfx);
        node.AddChildSafely(vfx);
    }

    public override void _Ready()
    {
        if (GetParent() is not NCreature || _creature == null)
        {
            QueueFree();
            return;
        }

        _hitbox = ResolveHitbox();
        _texture ??= GD.Load<Texture2D>(TexturePath);

        // The first game drew the bubbles with GL_SRC_ALPHA / GL_ONE, i.e. additive blending.
        Material = new CanvasItemMaterial
        {
            BlendMode = CanvasItemMaterial.BlendModeEnum.Add
        };

        // Stagger the first spawn so that five manifolds appearing together do not pulse in sync.
        _timer = Rand(0f, MaxInterval);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        if (CanSpawn())
        {
            _timer -= dt;
            if (_timer <= 0f)
            {
                _timer = Rand(MinInterval, MaxInterval);
                Spawn();
            }
        }

        for (int i = _bubbles.Count - 1; i >= 0; i--)
        {
            if (!Advance(_bubbles[i], dt))
            {
                _bubbles.RemoveAt(i);
            }
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_texture == null)
        {
            return;
        }

        foreach (Bubble bubble in _bubbles)
        {
            float size = TextureSize * Math.Max(0f, bubble.Scale);
            if (size <= 0.5f)
            {
                continue;
            }

            Rect2 rect = new(bubble.Position - new Vector2(size, size) * 0.5f, new Vector2(size, size));
            DrawTextureRect(_texture, rect, false, new Color(1f, 1f, 1f, bubble.Alpha));
        }
    }

    private bool CanSpawn()
    {
        return _creature != null
               && _creature.IsAlive
               && _creature.CombatState?.IsLiveCombat() == true;
    }

    /// <summary>
    /// The creature's hitbox node, which tells us where its body is. It is resolved lazily: when the
    /// effect is attached before the creature node finished entering the tree, the hitbox does not
    /// exist yet.
    /// </summary>
    private Control? ResolveHitbox()
    {
        if (_hitbox != null && IsInstanceValid(_hitbox))
        {
            return _hitbox;
        }

        _hitbox = (GetParent() as NCreature)?.Hitbox;
        return _hitbox;
    }

    private void Spawn()
    {
        Control? hitbox = ResolveHitbox();
        if (hitbox == null)
        {
            return;
        }

        Vector2 origin = hitbox.Position;
        Vector2 size = hitbox.Size;
        if (size.Y <= 0f || size.X <= 0f)
        {
            return;
        }

        float duration = Rand(MinDuration, MaxDuration);
        float side = GD.Randf() < 0.5f ? -1f : 1f;
        float scale = Rand(MinScale, MaxScale);

        _bubbles.Add(new Bubble
        {
            // Just below the feet, at one randomly chosen side of the body.
            Position = new Vector2(
                origin.X + size.X * 0.5f + side * size.X * Rand(0.15f, 0.35f),
                origin.Y + size.Y * 1.05f),
            Remaining = duration,
            Duration = duration,
            ScaleSpeed = 3f * scale / duration,
            SpeedY = size.Y * Rand(1.1f, 1.4f) / duration,
            SpeedX = Rand(0.5f, 0.7f) * side * DriftSpeed,
            AccelX = -side * DriftSpeed / duration,
            Alpha = Rand(0.8f, 1f)
        });
    }

    private static bool Advance(Bubble bubble, float dt)
    {
        // Godot counts y downwards, the first game counted it upwards, so rising subtracts.
        bubble.Position.Y -= bubble.SpeedY * dt;
        bubble.SpeedX += bubble.AccelX * dt;
        bubble.Position.X += bubble.SpeedX * dt;

        if (bubble.Remaining > 0.66f * bubble.Duration)
        {
            bubble.Scale += bubble.ScaleSpeed * dt;
        }
        else if (bubble.Remaining < 0.34f * bubble.Duration)
        {
            bubble.Scale -= bubble.ScaleSpeed * dt;
        }

        bubble.Remaining -= dt;
        return bubble.Remaining > 0f;
    }

    private static float Rand(float min, float max)
    {
        return min + GD.Randf() * (max - min);
    }

    private sealed class Bubble
    {
        public Vector2 Position;
        public float SpeedX;
        public float SpeedY;
        public float AccelX;
        public float Scale;
        public float ScaleSpeed;
        public float Alpha;
        public float Remaining;
        public float Duration;
    }
}
