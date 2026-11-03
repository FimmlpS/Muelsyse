using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Muelsyse.Scripts.Pools;

public class MuelsyseCardPool : CustomCardPoolModel
{
    public override string Title => "Muelsyse";

    public override string? BigEnergyIconPath => "res://Muelsyse/images/icon/energybg_muelsyse.png";

    public override string? TextEnergyIconPath => "res://Muelsyse/images/icon/energy_icon_muelsyse.png";

    public override Color DeckEntryCardColor => new(0.36f, 0.66f, 0.72f);

    public override Color ShaderColor => new(0.36f, 0.66f, 0.72f);

    // BaseLib feeds these into the game's HSV card-frame shader, which recolors the frame texture.
    // Our cardbg_* art is already painted in Muelsyse's colors, so the shader has to be a no-op:
    // H = 0 turns the hue rotation into a full 360 degree turn (identity), S = V = 1 keep the
    // saturation and brightness untouched. The old teal ShaderColor rotated the hue by ~170 degrees
    // and halved the saturation, which washed the frames out into flat gray-white.
    public override float H => 0f;

    public override float S => 1f;

    public override float V => 1f;

    public override bool IsColorless => false;

    public override Texture2D? CustomFrame(CustomCardModel card)
    {
        if (card.Type == CardType.Attack)
        {
            return PreloadManager.Cache.GetAsset<Texture2D>("res://Muelsyse/images/bg/cardbg_attack.png");
        }
        if (card.Type == CardType.Power)
        {
            return PreloadManager.Cache.GetAsset<Texture2D>("res://Muelsyse/images/bg/cardbg_power.png");
        }
        return PreloadManager.Cache.GetAsset<Texture2D>("res://Muelsyse/images/bg/cardbg_skill.png");
    }
}
