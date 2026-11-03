using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Muelsyse.Scripts.Enums;

public class MuelsyseKeywords
{
    [CustomEnum("MANIFOLD")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Manifold;

    [CustomEnum("TOGETHER")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Together;

    [CustomEnum("WATER")]
    [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Water;

    [CustomEnum("SOURCE")]
    [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Source;

    [CustomEnum("THRESHOLD")]
    [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Threshold;

    [CustomEnum("MOLDING")]
    [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Molding;

    [CustomEnum("ABSORB")]
    [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Absorb;

    [CustomEnum("DISAPPEAR")]
    [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Disappear;

    [CustomEnum("RESPAWN")]
    [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Respawn;

    [CustomEnum("FIRST_WORD")]
    [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword FirstWord;
}
