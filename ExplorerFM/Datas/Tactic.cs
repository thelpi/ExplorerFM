using System.Collections.Generic;
using System.Linq;
using ExplorerFM.Extensions;

namespace ExplorerFM.Datas
{
    public class Tactic
    {
        private static List<Tactic> _tactics;

        public static IReadOnlyCollection<Tactic> Tactics => _tactics ?? (_tactics = GenerateTactics());

        public string Name { get; }

        public IReadOnlyList<(Position, Side)> Positions { get; }

        private Tactic(string name, params (Position, Side)[] positions)
        {
            Name = name;
            Positions = (Position.GoalKeeper, Side.Center)
                .Yield(positions)
                .ToList();
        }

        private static List<Tactic> GenerateTactics()
        {
            var centerDef = (Position.Defender, Side.Center);
            var leftDef = (Position.Defender, Side.Left);
            var rightDef = (Position.Defender, Side.Right);
            var centerFw = (Position.Striker, Side.Center);
            var leftFw = (Position.Striker, Side.Left);
            var rightFw = (Position.Striker, Side.Right);
            var centerMid = (Position.Midfielder, Side.Center);
            var centerDefMid = (Position.DefensiveMidfielder, Side.Center);
            var centerOffMid = (Position.OffensiveMidfielder, Side.Center);
            var leftOffMid = (Position.OffensiveMidfielder, Side.Left);
            var rightOffMid = (Position.OffensiveMidfielder, Side.Right);
            var leftMid = (Position.Midfielder, Side.Left);
            var rightMid = (Position.Midfielder, Side.Right);
            var leftDefMid = (Position.DefensiveMidfielder, Side.Left);
            var rightDefMid = (Position.DefensiveMidfielder, Side.Right);
            var sweeper = (Position.Sweeper, Side.Center);

            var fourDefender = new[]
            {
                leftDef,
                centerDef,
                centerDef,
                rightDef
            };

            var tactics = new List<Tactic>
            {
                new Tactic("4-3-1-2", fourDefender
                .Concat(centerMid.Yield(centerMid,
                    centerMid,
                    centerOffMid,
                    centerFw,
                    centerFw))
                .ToArray()),
                new Tactic("4-3-3 DM", fourDefender
                .Concat(centerFw.Yield(leftFw,
                    rightFw,
                    centerDefMid,
                    centerMid,
                    centerMid))
                .ToArray()),
                new Tactic("4-3-3 OM", fourDefender
                .Concat(centerFw.Yield(leftFw,
                    rightFw,
                    centerOffMid,
                    centerMid,
                    centerMid))
                .ToArray()),
                new Tactic("4-5-1", fourDefender
                .Concat(centerFw.Yield(leftOffMid,
                    rightOffMid,
                    centerOffMid,
                    centerMid,
                    centerMid))
                .ToArray()),
                new Tactic("4-4-2 OM", fourDefender
                .Concat(centerFw.Yield(leftOffMid,
                    rightOffMid,
                    centerFw,
                    centerMid,
                    centerMid))
                .ToArray()),
                new Tactic("3-5-2 SW", sweeper
                .Yield(centerDef,
                    centerDef,
                    rightDefMid,
                    leftDefMid,
                    centerMid,
                    centerMid,
                    centerOffMid,
                    centerFw,
                    centerFw)
                .ToArray())
            };
            return tactics;
        }
    }
}
