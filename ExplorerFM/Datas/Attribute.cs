using System.Collections.Generic;
using System.Linq;

namespace ExplorerFM.Datas
{
    public class Attribute : BaseData
    {
        private static List<Attribute> _playerInstances = null;

        public string Name { get; set; }
        public AttributeType Type { get; set; }

        private Attribute() { }

        public static IReadOnlyList<Attribute> PlayerInstances
        {
            get
            {
                if (_playerInstances == null)
                {
                    _playerInstances = new List<Attribute>(50)
                    {
                        new Attribute { Id = 01, Name = "Acceleration", Type = AttributeType.Physical },
                        new Attribute { Id = 02, Name = "Agility", Type = AttributeType.Physical },
                        new Attribute { Id = 03, Name = "Balance", Type = AttributeType.Physical },
                        new Attribute { Id = 04, Name = "InjuryProneness", Type = AttributeType.Physical },
                        new Attribute { Id = 05, Name = "Jumping", Type = AttributeType.Physical },
                        new Attribute { Id = 06, Name = "NaturalFitness", Type = AttributeType.Physical },
                        new Attribute { Id = 07, Name = "Pace", Type = AttributeType.Physical },
                        new Attribute { Id = 08, Name = "Stamina", Type = AttributeType.Physical },
                        new Attribute { Id = 09, Name = "Strength", Type = AttributeType.Physical },
                        new Attribute { Id = 10, Name = "Handling", Type = AttributeType.Goalkeeper },
                        new Attribute { Id = 11, Name = "OneOnOnes", Type = AttributeType.Goalkeeper },
                        new Attribute { Id = 12, Name = "Reflexes", Type = AttributeType.Goalkeeper },
                        new Attribute { Id = 13, Name = "Corners", Type = AttributeType.Technical },
                        new Attribute { Id = 14, Name = "FreeKicks", Type = AttributeType.Technical },
                        new Attribute { Id = 15, Name = "ThrowIns", Type = AttributeType.Technical },
                        new Attribute { Id = 16, Name = "Crossing", Type = AttributeType.Technical },
                        new Attribute { Id = 17, Name = "Dribbling", Type = AttributeType.Technical },
                        new Attribute { Id = 18, Name = "Finishing", Type = AttributeType.Technical },
                        new Attribute { Id = 19, Name = "Heading", Type = AttributeType.Technical },
                        new Attribute { Id = 20, Name = "LongShots", Type = AttributeType.Technical },
                        new Attribute { Id = 21, Name = "Marking", Type = AttributeType.Technical },
                        new Attribute { Id = 22, Name = "Movement", Type = AttributeType.Technical },
                        new Attribute { Id = 23, Name = "Passing", Type = AttributeType.Technical },
                        new Attribute { Id = 24, Name = "Penalties", Type = AttributeType.Technical },
                        new Attribute { Id = 25, Name = "Positioning", Type = AttributeType.Technical },
                        new Attribute { Id = 26, Name = "Tackling", Type = AttributeType.Technical },
                        new Attribute { Id = 27, Name = "Technique", Type = AttributeType.Technical },
                        new Attribute { Id = 28, Name = "Vision", Type = AttributeType.Technical },
                        new Attribute { Id = 29, Name = "Adaptability", Type = AttributeType.Psychological },
                        new Attribute { Id = 30, Name = "Agression", Type = AttributeType.Psychological },
                        new Attribute { Id = 31, Name = "Ambition", Type = AttributeType.Psychological },
                        new Attribute { Id = 32, Name = "Anticipation", Type = AttributeType.Psychological },
                        new Attribute { Id = 33, Name = "Bravery", Type = AttributeType.Psychological },
                        new Attribute { Id = 34, Name = "Consistency", Type = AttributeType.Psychological },
                        new Attribute { Id = 35, Name = "Decisions", Type = AttributeType.Psychological },
                        new Attribute { Id = 36, Name = "Determination", Type = AttributeType.Psychological },
                        new Attribute { Id = 37, Name = "Dirtiness", Type = AttributeType.Psychological },
                        new Attribute { Id = 38, Name = "Flair", Type = AttributeType.Psychological },
                        new Attribute { Id = 39, Name = "ImportantMatchs", Type = AttributeType.Psychological },
                        new Attribute { Id = 40, Name = "Leadership", Type = AttributeType.Psychological },
                        new Attribute { Id = 41, Name = "Loyalty", Type = AttributeType.Psychological },
                        new Attribute { Id = 42, Name = "Pressure", Type = AttributeType.Psychological },
                        new Attribute { Id = 43, Name = "Professionalism", Type = AttributeType.Psychological },
                        new Attribute { Id = 44, Name = "Sportsmanship", Type = AttributeType.Psychological },
                        new Attribute { Id = 45, Name = "TeamWork", Type = AttributeType.Psychological },
                        new Attribute { Id = 46, Name = "Temperament", Type = AttributeType.Psychological },
                        new Attribute { Id = 47, Name = "Versatility", Type = AttributeType.Psychological },
                        new Attribute { Id = 48, Name = "WorkRate", Type = AttributeType.Psychological }
                    };
                }

                return _playerInstances;
            }
        }

        public static Attribute GetPlayerAttributeBy(string exactName)
        {
            return PlayerInstances.FirstOrDefault(x => x.Name == exactName);
        }
    }
}
