status = ProcessorStatus.Stopped;
cycleCount = bus.HandleProcessorStop();
if (cycleCount < 0)
	return false;
status = ProcessorStatus.Running;
