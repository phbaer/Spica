vendor "spica-robotics";

struct Container {
}

struct DataContainer : Container;

struct Point2dInfo : DataContainer [compare=x,y] {
	double x;
	double y;
}

struct Point3dInfo : Point2dInfo [compare=x,y,z] {
	double z;
}

struct Vector2dInfo : Point2dInfo;

struct DriveToPointInfo : Point2dInfo;

struct PathInfo : DataContainer [compare=positionInfos] {
	Point2dInfo[] positionInfos;
}

struct SharedWorldFlags : DataContainer {
	bool ballPossession;
}

struct PositionInfo : Point2dInfo [compare=x,y,angle,certainty] {
	double angle;
	double certainty;
}

struct ObstacleInfo : Point2dInfo [compare=x,y,diameter] {
	double diameter;
}

struct RectangleInfo : DataContainer [compare=x,y,width,height] {
	double x;
	double y;
	double width;
	double height;
}

struct TrackedObjectInfo : DataContainer [compare=position,velocity] {
	PositionInfo position;
	VelocityInfo velocity;
}

struct BallInfo : DataContainer [compare=point,velocity] {
	Point3dInfo point;
	Velocity3dInfo velocity;
	double confidence;
	uint8 ballType;
}

struct SharedBallInfo : BallInfo [compare=point,velocity,evidence] {
	int32 evidence;
}

struct NegotiatedBallInfo : BallInfo;

struct LineInfo : DataContainer [compare=startPoint,endPoint] {
	Point2dInfo startPoint;
	Point2dInfo endPoint;
}

struct VelocityInfo : DataContainer [compare=vx,vy] {
	double vx;
	double vy;
}

struct Velocity3dInfo : VelocityInfo [compare=vx,vy,vz] {
	double vz;
}


struct MotionInfo : DataContainer [compare=angle,translation,rotation] {
	double angle;
	double translation;
	double rotation;
}

struct KickInfo : DataContainer [compare=enabled,kicker,power] {
	bool enabled;
	uint16 kicker;
	uint16 power;
	uint16 forceVoltage;
	uint8 extension;
	uint16 extTime;
}

struct GoalInfo : DataContainer [compare=leftPost,rightPost,isOwn,isYellow] {
	Point2dInfo leftPost;
	Point2dInfo rightPost;
	bool isOwn;
	GoalColor goal;
	bool isYellow;
}

struct YellowGoalInfo : GoalInfo;
struct BlueGoalInfo : GoalInfo;

struct FreeAreaInfo : DataContainer [compare=angle1,angle2,isOwn,isYellow] {
	double angle1;
	double angle2;
	bool isOwn;
	GoalColor goal;
	bool isYellow;
}

struct YellowFreeAreaInfo : FreeAreaInfo;
struct BlueFreeAreaInfo : FreeAreaInfo;

struct OdometryInfo : DataContainer [compare=position,motion] {
	PositionInfo position;
	MotionInfo motion;
	uint64 timestamp;
}

struct RawOdometryInfo : OdometryInfo;
struct CorrectedOdometryInfo : OdometryInfo [compare=position,motion,certainty] {
	double certainty;
}

struct TrackedOdometryInfo : OdometryInfo [compare=position,motion,certainty] {
	double certainty;
}

struct DistanceScanInfo : DataContainer [compare=sectors] {
	double[] sectors;
}

struct PlanTreeInfo : DataContainer [compare=stateIDs] {
	int64[] stateIDs;
}

struct OpponentInfo : DataContainer [compare=positionInfos] {
	Point2dInfo[] positionInfos;
}

struct CompassInfo : DataContainer [compare=value] {
	int32 value;
}

struct TeamColorInfo : DataContainer [compare=color] {
	TeamColor color;
}

struct GoalColorInfo : DataContainer [compare=color] {
	GoalColor color;
}

struct MonitoringData : DataContainer {
	string name;
}

struct MotionStatInfo : DataContainer {
	float supplyVoltage;
}

struct KickerStatInfo : DataContainer {
	float supplyVoltage;
	float capVoltage;
}

struct MonitoringFloatArray : MonitoringData {
	float[] data;
}

struct MonitoringDoubleArray : MonitoringData {
	double[] data;
}

struct MonitoringIntArray : MonitoringData {
	int32[] data;
}

struct MonitoringStringArray : MonitoringData {
	string[] data;
}

struct MonitoringString : MonitoringData {
	string data;
}

struct MonitoringBody : Container {
	string concept;
	string rep;
	uint8 warnLevel;
	string warnMessage;
	uint16 priority;
	MonitoringData[] data;
}

struct LebtControlBody : Container {
	string robot;
	ProcessAction action;
	int32 id;
}

struct BehaviourEngineInfoBody : Container {
	string masterPlan;
	string currentPlan;
	string currentState;
	string currentRole;
	string currentTask;
	uint8[] robotIDsWithMe;
}

struct SystemInfoBody : Container {
	uint16 load;
	uint32 memTotal;
	uint32 memTotalFree;
	NetInfoStruct[] netInfo;
	CpuInfoStruct[] cpuInfo;
}

struct NetInfoStruct {
	string devicename;
	uint32 received;
	uint32 sent;
	bool wireless;
	int8 link;
	int8 level;
	int8 noise;
}

struct CpuInfoStruct {
	string name;
	uint16 busy;
}

struct LebtInfoBody : Container {
	int32 active;
	BundleInfo[] bundles;
}

struct BundleInfo {
	bool valid;
	int32 id;
	string name;
	ProcessStatus status;
	ProcessInfo[] processes;
}

struct ProcessInfo {
	int32 id;
	string name;
	uint16 cpuUsage;
	uint32 memoryUsage;
	ProcessExitCode exitCode;
	ProcessStatus status;
}

struct BehaviorEngineControlBody : Container {
	string robot;
	string transition;
	TeamColor teamColor;
	GoalColor goalColor;
}

struct EnforceColorsBody : Container {
	string robot;
	TeamColor teamColor;
	GoalColor goalColor;
}

struct RefereeBoxControlBody : Container {
	string name;
	TeamColor teamColor;
	GoalColor goal;
}

struct RefereeBoxInfoBody : Container {
	RefereeBoxState state;
	uint8 lastCommand;
	GameStage gameStage;
	uint16 elapsedSeconds;
	uint8 goalsCyan;
	uint8 goalsMagenta;	
	RobotTeamInfo[] team;
}

struct RobotTeamInfo {
	string name;
	TeamColor teamColor;
	GoalColor goalColor;
}

struct ActuatorControl {
	int32 priority;
	uint32 flags;
}

struct SimulatorControlBody : Container {
	string field;
}

struct Version {
	uint8 major;
	uint8 minor;
	uint8 revision;
	uint8 build;
}

struct ModuleInfoBody : Container {
	string name;
	Version version;
}

struct SimulatorRobotPositionBody : Container {
	Point2dInfo position;	
	uint8 robotID;
}

struct GemingaLEInfo {
	string name;
}
struct Timestamp : Container {
	int64 time;
}

struct PlanSuccessInfo: DataContainer [compare=epID] {
	int64 epID;
}

struct GemingaLEMessage {
	GemingaLEInfo body;
}

struct LebtModuleInfo {
	ModuleInfoBody body;
}

struct SimulatorUpdateRobotPosition {
	 SimulatorRobotPositionBody body;
}

struct MotionControl {
	ActuatorControl control;
	MotionInfo motion;
}

struct MotionStat {
	MotionStatInfo stat;
}

struct KickControl {
	ActuatorControl control;
	KickInfo kick;
}

struct KickerStat {
	KickerStatInfo stat;
}

struct JoystickRobotIDs {
	uint8[] ids;
}

struct JoystickControl {
	//JoystickRobots robots;
	JoystickRobotIDs robotIDs;
	MotionInfo motion;
	KickInfo kick;
}

struct DriveToPoint {
	Point2dInfo point;
}

struct MoveBallControl {
	Point2dInfo point;
	SimulatorControlBody sim;
}

struct WorldModelData {
	Container[] data;
}

struct WorldModelDataSim {
	Container[] data;
}

struct RawOdometry {
	RawOdometryInfo odometry;
}

struct CompassMessage {
	CompassInfo body;
}

struct SharedWorldInfo {
	DataContainer[] observations;
	TeamFlag teamFlag;
	//RobotSignals[] signals;
	//Commands[] commands;
}

struct TeamFlag : Container {
	bool teamColorIsMagenta;
}

struct SpeechAct {	
}

struct SyncData : Container [compare=robotID,transitionID,conditionHolds] {
	uint8 robotID;
	int64 transitionID;
	bool conditionHolds;
	bool ack;
}

struct WhiteBoardInfo : Container {
	uint8[] receiverIDs;
	int16 validFor;
}

struct WhiteBoardMsg : SpeechAct {
	WhiteBoardInfo whiteBoardInfo;	
}

struct PassMsg : WhiteBoardMsg {
	Point2dInfo origin;
	Point2dInfo destination;
}

struct WatchBallMsg : WhiteBoardMsg {
}

struct SyncTalk : SpeechAct {	
	SyncData[] syncData;
}

struct SyncReadyBody : Container {
	int64 syncTransitionID;
}

struct SyncReady : SpeechAct {
	SyncReadyBody body;
}

struct MonitoringInfo {
	MonitoringBody body;
}

struct LebtControl {
	LebtControlBody body;
}

struct BehaviourEngineInfo {
	BehaviourEngineInfoBody body;
}

struct SystemInfo {
	SystemInfoBody body;
}

struct LebtInfo {
	LebtInfoBody body;
}

struct BehaviorEngineControl {
	BehaviorEngineControlBody body;
}

struct RefereeBoxControl {
	RefereeBoxControlBody body;
}

struct RefereeBoxInfo {
	RefereeBoxInfoBody body;
}

struct SimulatorControl {
	SimulatorControlBody body;
}

struct EnforceColors {
	EnforceColorsBody body;
}

struct SharedBallMessage {
	SharedBallInfo ball;
}

struct CorrectedOdometryMessage {
	CorrectedOdometryInfo coi;
}

struct KickTime {
	Timestamp time;
}

