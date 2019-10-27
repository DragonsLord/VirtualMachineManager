using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.Prognosing.Models
{
    public class VMPrognose
    {
        public VM VM { get; }
        public Resources[] Prognoses { get; }

        public VMPrognose(VM vm, Resources[] prognoses)
        {
            VM = vm;
            Prognoses = prognoses;
        }

        public VM GetPrognosedVmState(int prognoseDepth) =>
            new VM()
            {
                Id = VM.Id,
                Resources = Prognoses[prognoseDepth - 1], // 0 should not be a prognosed value
                IsMigrating = VM.IsMigrating
            };
    }
}
