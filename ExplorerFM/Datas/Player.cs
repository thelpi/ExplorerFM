using System.Collections.Generic;

namespace ExplorerFM.Datas
{
    public class Player : Staff
    {
        public Dictionary<Side, int?> Sides { get; set; }
        public Dictionary<Position, int?> Positions { get; set; }
        public Dictionary<Attribute, int?> Attributes { get; set; }
        [Field("LeftFoot")]
        public int? LeftFoot { get; set; }
        [Field("RightFoot")]
        public int? RightFoot { get; set; }
        [Field("SquadNumber")]
        public int? SquadNumber { get; set; }

        public int GetPositionSideRate(Position p, Side s)
        {
            var sNote = p == Position.GoalKeeper ? 20 : (Sides.ContainsKey(s) ? Sides[s].GetValueOrDefault(1) : 1);
            var pNote = Positions.ContainsKey(p) ? Positions[p].GetValueOrDefault(1) : 1;
            return System.Math.Min(sNote, pNote);
        }
    }
}
