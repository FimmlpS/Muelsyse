using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Muelsyse.Scripts.Monsters;

public class Manifold : CustomPetModel
{
    public Manifold() : base(true)
    {
    }

    public override int MinInitialHp => 5;

    public override int MaxInitialHp => 5;

    /// <summary>Animation prefix. The basic manifold uses "A_", copies use "B_".</summary>
    protected virtual string AnimPrefix => "A_";

    public override NCreatureVisuals? CreateCustomVisuals()
    {
        return NodeFactory<NCreatureVisuals>.CreateFromScene("res://Muelsyse/scenes/ManifoldModel.tscn");
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle = new(AnimPrefix + "Idle", true);
        AnimState attack = new(AnimPrefix + "Attack", false) { NextState = idle };
        AnimState dead = new(AnimPrefix + "Die", false);
        AnimState revive = new(AnimPrefix + "Start", false) { NextState = idle };

        CreatureAnimator animator = new(revive, controller);
        animator.AddAnyState("Idle", idle);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Dead", dead);
        animator.AddAnyState("Hit", idle);
        animator.AddAnyState("Revive", revive);
        return animator;
    }
}
