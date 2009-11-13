enum TeamColor : int8 {
	None = -1;
	Cyan = 0;
	Magenta = 1;
}

enum GoalColor : int8 {
	None = -1;
	Yellow = 0;
	Blue = 1;
}

enum ProcessAction : int16 {
	Undefined = -1;

	StartBundle = 0;
	StopBundle = 1;
	KillBundle = 2;

	StartProcess = 10;
	StopProcess = 11;
	KillProcess = 12;
}

enum ProcessStatus : int8 {
	Undefined = -1;
	Stopped = 0;
	Starting = 1;
	Running = 2;
	Stopping = 3;
	Error = 4;
}

enum ProcessExitCode : int16 {
	Undefined = -1;
	Success = 0;
	Hangup = 129;
	Interrupted = 130;
	Quit = 131;
	IllegalInstruction = 132;
	Trap = 133;
	Abort = 134;
	FloatingPointException = 136;
	Killed = 137;
	SegmentationFault = 139;
	Pipe = 141;
	Alarm = 142;
	Terminated = 143;
}

enum RefereeBoxState : uint8 {
	Undefined = 0;
	Connected = 1;
	Disconnected = 2;
}

enum GameStage : uint8 {
	Undefined = 0;
	FirstHalf = 1;
	HalfTime = 2;
	SecondHalf = 3;
	EndGame = 4;
	ShootOut = 5;
	PreGame = 6;	
}

enum Flags : uint32 {
	None = 0;
	Reliable = 1;
}

enum Role : int32 {
	None = 0;
	Attack = 1;
	Support = 2;
	Defend = 3;
	Keeper = 4;
	WithoutTask = 5;
	AttackSupport = 6;
	DefendSupport = 7;
}

