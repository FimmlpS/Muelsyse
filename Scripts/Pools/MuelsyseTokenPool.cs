using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Muelsyse.Scripts.Pools;

public class MuelsyseTokenPool : CustomCardPoolModel
{
    public override string Title => "MuelsyseToken";

    public override string? BigEnergyIconPath => "res://Muelsyse/images/icon/energybg_muelsyse.png";

    public override string? TextEnergyIconPath => "res://Muelsyse/images/icon/energy_icon_muelsyse.png";

    public override Color DeckEntryCardColor => new(0.36f, 0.66f, 0.72f);

    public override Color ShaderColor => new(0.36f, 0.66f, 0.72f);

    // Keep the HSV card-frame shader a no-op so the hand-painted cardbg_* art keeps its colors.
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
