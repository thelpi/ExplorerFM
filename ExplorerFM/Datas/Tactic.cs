using System;
using System.Collections.Generic;
using System.Linq;

namespace ExplorerFM.Datas
{
    public class Tactic
    {
        private static List<Tactic> _tactics;

        public static IReadOnlyCollection<Tactic> Tactics => _tactics ?? (_tactics = GenerateTactics());

        public string Name { get; }

        public IReadOnlyList<Tuple<Position, Side>> Positions { get; }

        private Tactic(string name, params Tuple<Position, Side>[] positions)
        {
            Name = name;
            Positions = new Tuple<Position, Side>(Position.GoalKeeper, Side.Center)
                .Yield(positions)
                .ToList();
        }

        private static List<Tactic> GenerateTactics()
        {
            var centerDef = new Tuple<Position, Side>(Position.Defender, Side.Center);
            var leftDef = new Tuple<Position, Side>(Position.Defender, Side.Left);
            var rightDef = new Tuple<Position, Side>(Position.Defender, Side.Right);
            var centerFw = new Tuple<Position, Side>(Position.Striker, Side.Center);
            var leftFw = new Tuple<Position, Side>(Position.Striker, Side.Left);
            var rightFw = new Tuple<Position, Side>(Position.Striker, Side.Right);
            var centerMid = new Tuple<Position, Side>(Position.Midfielder, Side.Center);
            var centerDefMid = new Tuple<Position, Side>(Position.DefensiveMidfielder, Side.Center);
            var centerOffMid = new Tuple<Position, Side>(Position.OffensiveMidfielder, Side.Center);
            var leftOffMid = new Tuple<Position, Side>(Position.OffensiveMidfielder, Side.Left);
            var rightOffMid = new Tuple<Position, Side>(Position.OffensiveMidfielder, Side.Right);
            var leftMid = new Tuple<Position, Side>(Position.Midfielder, Side.Left);
            var rightMid = new Tuple<Position, Side>(Position.Midfielder, Side.Right);
            var leftDefMid = new Tuple<Position, Side>(Position.DefensiveMidfielder, Side.Left);
            var rightDefMid = new Tuple<Position, Side>(Position.DefensiveMidfielder, Side.Right);
            var sweeper = new Tuple<Position, Side>(Position.Sweeper, Side.Center);

            var fourDefender = new Tuple<Position, Side>[]
            {
                    leftDef,
                    centerDef,
                    centerDef,
                    rightDef
            };

            var tactics = new List<Tactic>();
            tactics.Add(new Tactic("4-3-1-2", fourDefender
                .Concat(centerMid.Yield(centerMid,
                    centerMid,
                    centerOffMid,
                    centerFw,
                    centerFw))
                .ToArray()));
            tactics.Add(new Tactic("4-3-3 DM", fourDefender
                .Concat(centerFw.Yield(leftFw,
                    rightFw,
                    centerDefMid,
                    centerMid,
                    centerMid))
                .ToArray()));
            tactics.Add(new Tactic("4-3-3 OM", fourDefender
                .Concat(centerFw.Yield(leftFw,
                    rightFw,
                    centerOffMid,
                    centerMid,
                    centerMid))
                .ToArray()));
            tactics.Add(new Tactic("4-5-1", fourDefender
                .Concat(centerFw.Yield(leftOffMid,
                    rightOffMid,
                    centerOffMid,
                    centerMid,
                    centerMid))
                .ToArray()));
            tactics.Add(new Tactic("4-4-2 OM", fourDefender
                .Concat(centerFw.Yield(leftOffMid,
                    rightOffMid,
                    centerFw,
                    centerMid,
                    centerMid))
                .ToArray()));
            tactics.Add(new Tactic("3-5-2 SW", sweeper
                .Yield(centerDef,
                    centerDef,
                    rightDefMid,
                    leftDefMid,
                    centerMid,
                    centerMid,
                    centerOffMid,
                    centerFw,
                    centerFw)
                .ToArray()));
            return tactics;
        }
    }
}
