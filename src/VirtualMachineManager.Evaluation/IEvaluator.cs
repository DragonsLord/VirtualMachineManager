using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.Evaluation
{
    public interface IEvaluator
    {
        float Evaluate(Resources res);

        bool IsServerOverload(Server server, Resources load);

        bool IsServerUnderload(Server server, Resources load);
    }
}
