using BaseLib.Abstracts;

namespace Muelsyse.Scripts.Monsters;

/// <summary>Extra manifold copies spawned by 溯源 relics; uses the "B_" animation set.</summary>
public sealed class ManifoldCopy : Manifold
{
    public ManifoldCopy() : base()
    {
    }

    protected override string AnimPrefix => "B_";
}
