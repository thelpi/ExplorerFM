using System.Collections.Generic;

namespace ExplorerFM.RuleEngine
{
    public class CriteriaSet : CriterionBase
    {
        public static CriteriaSet Empty => new CriteriaSet(false);

        public IReadOnlyCollection<CriterionBase> Criteria { get; }
        public bool Or { get; }

        public CriteriaSet(bool or, params CriterionBase[] criteria)
        {
            Criteria = criteria;
            Or = or;
        }
    }
}
