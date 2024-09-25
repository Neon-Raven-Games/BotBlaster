using System.Linq;
using NRTools.Animator.NRNodes;

namespace GraphProcessor
{
    public class AnimationGraphProcessor : BaseGraphProcessor
    {
        private BaseNode _root;
        public AnimationGraphProcessor(BaseGraph graph) : base(graph)
        {
        }

        public override void UpdateComputeOrder()
        {
            _root = graph.nodes.FirstOrDefault();
        }


        public override void Run()
        {

        }
    }
}