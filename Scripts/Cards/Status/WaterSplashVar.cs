using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Powers;

namespace Muelsyse.Scripts.Cards.Status;

/// <summary>
/// The damage printed on 水. It deliberately is not a <see cref="DamageVar"/>: 水 hits with an
/// unpowered splash, so it must never be treated as an attack (协同, attack cultivation and strength
/// all ignore it). The only external bonus is 水培, which is folded into the preview here.
/// </summary>
public class WaterSplashVar : DynamicVar
{
    public const string Key = "Splash";

    public WaterSplashVar(decimal value) : base(Key, value)
    {
    }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target,
        bool runGlobalHooks)
    {
        PreviewValue = BaseValue + (card.Owner?.Creature.GetPower<FlowerWaterPower>()?.Amount ?? 0);
    }
}
