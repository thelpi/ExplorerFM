using ExplorerFM.FieldsAttributes;

namespace ExplorerFM.Datas
{
    public enum Position
    {
        [MongoName("goalKeeper")]
        GoalKeeper = 1,
        [MongoName("sweeper")]
        Sweeper,
        [MongoName("defender")]
        Defender,
        [MongoName("defMidfielder")]
        DefensiveMidfielder,
        [MongoName("midfielder")]
        Midfielder,
        [MongoName("offMidfielder")]
        OffensiveMidfielder,
        [MongoName("forward")]
        Striker,
        [MongoName("wingBack")]
        WingBack,
        [MongoName("freeRole")]
        FreeRole
    }
}
