namespace VirtualMachineManager.EvaluationExtensions.Configs
{
    public class ResourcesEvaluationParams
    {
        public float CpuWeight { get; set; }
        public float MemoryWeight { get; set; }
        public float NetworkWeight { get; set; }
        public float IopsWeight { get; set; }

        public float CpuCap { get; set; }
        public float MemoryCap { get; set; }
        public float NetworkCap { get; set; }
        public float IopsCap { get; set; }
    }
}
