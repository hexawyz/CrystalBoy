if (ime)
{
	status = ProcessorStatus.Halted;
	cycleCount = bus.WaitForInterrupts();
	if (cycleCount < 0)
		return false;
	status = ProcessorStatus.Running;
}
