using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Attack;

[Pool(typeof(MuelsyseCardPool))]
public class CondensateWish : AbstractMuelsyseCard
{
    // The damage is the health the manifolds have already lost: 溯源 raises their maximum health,
    // so it still grows with the water engine, but the card now cashes in damage taken instead of
    // selling the same health twice.
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        MakeCalculatedDamage(0, (card, _) => card.Owner == null ? 0 : ManifoldCmd.AliveLostHp(card.Owner!));

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Manifold)
    ];

    public CondensateWish() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
