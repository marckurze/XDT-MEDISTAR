namespace XdtDeviceBridge.Infrastructure;

public interface IInterfaceProfileActivationExecutor
{
    // Future execution contract only. No production implementation is registered in this step.
    Task<InterfaceProfileActivationExecutorResult> ExecuteAsync(
        InterfaceProfileActivationExecutorRequest request,
        CancellationToken cancellationToken = default);
}
