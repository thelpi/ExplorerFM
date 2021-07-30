using System.Collections.Generic;

namespace ExplorerFM.RuleEngine
{
    public class CriteriaSet : CriterionBase
    {
        public IReadOnlyCollection<CriterionBase> Criteria { get; }
        public bool Or { get; }

        public CriteriaSet(bool or, params CriterionBase[] criteria)
        {
            Criteria = criteria;
            Or = or;
        }

        public override string ToString()
        {
            return !(Criteria?.Count > 0)
                ? SqlAll
                : $"({string.Join($") {(Or ? SqlOr : SqlAnd)} (", Criteria)})";
        }
    }
}
