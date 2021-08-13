using System.Collections.Generic;

namespace ExplorerFM.Datas
{
    public class Player : Staff
    {
        [Field("Sides", 1, 20, false)]
        public Dictionary<Side, int?> Sides { get; set; }
        [Field("Positions", 1, 20, false)]
        public Dictionary<Position, int?> Positions { get; set; }
        [Field("Attributes", 1, 20, false)]
        public Dictionary<Attribute, int?> Attributes { get; set; }

        [Field("LeftFoot", 1, 20)]
        public int? LeftFoot { get; set; }
        [Field("RightFoot", 1, 20)]
        public int? RightFoot { get; set; }
        [Field("SquadNumber", 0, 999)]
        public int? SquadNumber { get; set; }

        public int GetPositionSideRate(Position p, Side s)
        {
            var sNote = p == Position.GoalKeeper ? 20 : (Sides.ContainsKey(s) ? Sides[s].GetValueOrDefault(1) : 1);
            var pNote = Positions.ContainsKey(p) ? Positions[p].GetValueOrDefault(1) : 1;
            return System.Math.Min(sNote, pNote);
        }
    }
}
