using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Cards.Attack;
using Muelsyse.Scripts.Cards.Power;
using Muelsyse.Scripts.Cards.Skill;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Relics;

namespace Muelsyse.Scripts.Characters;

public class Muelsyse : PlaceholderCharacterModel
{
    public override Color NameColor => new(0.45f, 0.78f, 0.85f);

    public override Color EnergyLabelOutlineColor => new(0.16f, 0.42f, 0.52f);

    public override CharacterGender Gender => CharacterGender.Feminine;

    public override int StartingHp => 75;

    public override string CustomVisualPath => "res://Muelsyse/scenes/MuelsyseModel.tscn";
    public override string CustomIconTexturePath => "res://Muelsyse/images/icon/character_icon_muelsyse.png";
    public override string CustomIconPath => "res://Muelsyse/scenes/MuelsyseIcon.tscn";
    public override string CustomEnergyCounterPath => "res://Muelsyse/scenes/MuelsyseEnergy.tscn";
    public override string CustomRestSiteAnimPath => "res://Muelsyse/scenes/MuelsyseRest.tscn";
    public override string CustomMerchantAnimPath => "res://Muelsyse/scenes/MuelsyseMerchant.tscn";
    public override string CustomCharacterSelectBg => "res://Muelsyse/scenes/MuelsyseSelectBg.tscn";
    public override string CustomCharacterSelectIconPath => "res://Muelsyse/images/icon/char_select_muelsyse.png";
    public override string CustomCharacterSelectLockedIconPath => "res://Muelsyse/images/icon/char_select_muelsyse_locked.png";
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

    public override CardPoolModel CardPool => ModelDb.CardPool<MuelsyseCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<MuelsyseRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<MuelsysePotionPool>();

    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<WaterStrike>(),
        ModelDb.Card<WaterStrike>(),
        ModelDb.Card<WaterStrike>(),
        ModelDb.Card<WaterStrike>(),
        ModelDb.Card<WaterDefend>(),
        ModelDb.Card<WaterDefend>(),
        ModelDb.Card<WaterDefend>(),
        ModelDb.Card<WaterDefend>(),
        ModelDb.Card<GraduallyWater>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [
        ModelDb.Relic<LifeCircle>()
    ];

    public override CreatureAnimator? SetupCustomAnimationStates(MegaSprite controller)
    {
        return SetupAnimationState(
            controller,
            "Idle", "die", false,
            null, false,
            "Attack", false,
            null, false,
            null, false
        );
    }
}
