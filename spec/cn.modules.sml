
module Vision {
    pub.period = 33ms;
    pub.dmc = ringbuffer;

    sub.dmc = queue/10;
    sub.period = 33ms;
    sub.ttl = 500ms;

    pub WorldModelData [
        dmc    = queue/10;
        dst    = 1 | default:localhost/Base;
    ];
    pub KickControl [
        dst    = 1 | default:localhost/Kicker;
    ];
    pub CorrectedOrometryMessage [
        dst    = 1 | default:localhost/Base;
    ];

    sub RawOdometry [
        src    = 1 | default:localhost/Motion;
        type   = RawOdometryInfo;
    ];
    sub CompassMessage [
        dmc    = ringbuffer/1;
        src    = 1 | default:localhost/Compass;
        type   = CompassInfo;
    ];
    sub SpeechAct [
        dmc    = ringbuffer/1;
        src    = 1-n | default:shw_group/Base, !default:localhost/Base;
    ];
    sub SharedMallMessage [
        dmc    = ringbuffer/1;
        src    = 1-n | default:shw_group/Base, !default:localhost/Base;
        type   = SharedBallInfo;
    ];
}

module VisionDirected {
    sub.dmc = ringbuffer/1;
    sub.period = 33ms;
    sub.ttl = 500ms;

    sub SharedBallMessage [
        src = 1 | default:localhost/Base;
    ];
    sub CorrectedOdometryMessage [
        src = 1 | default:localhost/Motion;
    ];

    pub.period = 33ms;
    pub.dmc = ringbuffer;

    pub KickControl [
        dst = 1 | default:localhost/Kicker;
    ];
}

module Base [monitorable] {

    pub.period = 33ms;
    pub.dmc = ringbuffer;

    sub.dmc = ringbuffer/1;
    sub.period = 33ms;
    sub.ttl = 500ms;

    sub.extract.dmc = ringbuffer/10;
    sub.extract.ttl = 5s;

    sub WorldModelData [
        src = 1 | default:localhost/Vision;
        extract = data:CorrectedOdometryInfo;
        extract = data:TrackedOdometryInfo;
        extract = data:BallInfo;
        extract = data:YellowGoalInfo;
        extract = data:BlueGoalInfo;
        extract = data:DistanceScanInfo;
        extract = data:YellowFreeAreaInfo;
        extract = data:BlueFreeAreaInfo;
        extract = data:OpponentInfo;
    ];

    sub WorldModelDataSim [
        src = 1 | default:any/Simulator;
        extract = data:CorrectedOdometryInfo;
        extract = data:TrackedOdometryInfo;
        extract = data:BallInfo;
        extract = data:YellowGoalInfo;
        extract = data:BlueGoalInfo;
        extract = data:DistanceScanInfo;
        extract = data:YellowFreeAreaInfo;
        extract = data:BlueFreeAreaInfo;
        extract = data:OpponentInfo;
    ];
    sub BehaviorEngineControl [
        src = n | default:any/RemoteControl;
        dmc = queue/10;
        ttl = 5s;
    ];
    sub DriveToPoint [
        src = 1 | default:any/LebtClient;
    ];
    sub JoystickControl [
        src = 1 | default:any/RemoteControl;
    ];
    sub CompassMessage [
        src = 1 | default:localhost/Compass;
    ];
    sub RawOdometry [
        src = 1 | default:localhost/Motion;
    ];
    sub MotionStat [
        src = 1 | default:localhost/Motion;
    ];
    sub KickTime [
        src = 1 | default:localhost/Kicker;
    ];
    sub KickStat [
        src = 1 | default:localhost/Kicker;
    ];
    sub EnforceColors [
        src = n | default:ctrl_group/RemoteControl;
    ];
    sub SpeechAct [
        src = n | default:shw_group/Base;
    ];

    pub MotionControl [
        dst = 1 | default:localhost/Motion;
    ];

    pub KickControl [
        dst = 1 | default:localhost/Kicker;
    ];

    pub SimulatorControl [
        dst = 1 | default:sim_group/Simulator;
    ];

    pub BehaviourEngineInfo [
        dst = 1 | default:stat_group/LebtClient;
    ];

    pub SharedWorldInfo [
        dst = 1 | default:shw_group/Base, default:shw_group/LebtClient;
        dmc = queue/10;
    ];

    pub SpeechAct [
        dst = 1 | default:shw_group/Base;
        dmc = queue/10;
    ];

    sub SharedBallMessage [
        src = n | default:localhost/Base;
    ];
}

module Simulator {

    pub.period = 33ms;
    pub.dmc = ringbuffer;

    sub.dmc = ringbuffer/1;
    sub.period = 33ms;
    sub.ttl = 500ms;

    pub WorldModelDataSim [
        dst = n | default:any/Base;
    ];

    sub SimulatorControl [
        src = n | default:any/Base;
    ];

    sub MotionControl [
        src = n | default:any/Base;
    ];

    sub KickControl [
        src = n | default:any/Base, default:any/LebtClient;
    ];

    sub SimulatorUpdateRobotPosition [
        src = n | default:any/LebtClient;
    ];

    sub MoveBallControl [
        src = n | default:any/LebtClient;
    ];
}

module Motion [monitorable] {

    pub.period = 33ms;
    pub.dmc = ringbuffer;

    sub.dmc = ringbuffer/1;
    sub.period = 33ms;
    sub.ttl = 500ms;

    sub MotionControl [
        src = 1 | default:localhost/Base;
    ];

    pub RawOdometry [
        dst = 2 | default:localhost/Base, default:localhost/Vision;
    ];

    pub MotionStat [
        dst = 1 | default:localhost/Base;
        dst = 1 | default:stat_group/LebtClient;
    ];
}

module Kicker [monitorable] {

    pub.period = 33ms;
    pub.dmc = ringbuffer;

    sub.dmc = queue/10;
    sub.period = 100ms;
    sub.ttl = 500ms;

    sub KickControl [
        src = n | default:localhost/Base, default:localhost/Vision, default:localhost/VisionDirected, default:any/LebtClient;
    ];

    pub KickerStat [
        dst = 1 | default:stat_group/LebtClient;
    ];

    pub KickTime [
        dst = 1 | default:localhost/Base;
    ];
}

module Lebt [monitorable] {

    pub.period = 33ms;
    pub.dmc = ringbuffer;

    sub.dmc = queue/10;
    sub.period = 33ms;
    sub.ttl = 500ms;

    pub LebtModuleInfo [
        dst = 1 | default:stat_group/RemoteControl;
    ];

    pub LebtInfo [
        dst = 1 | default:stat_group/LebtClient;
    ];

    pub SystemInfo [
        dst = 1 | default:stat_group/LebtClient;
    ];

    pub BehaviorEngineControl [
        dst = n | default:any/RemoteControl;
        dmc = queue/10;
    ];

    sub LebtControl [
        src = 1 | default:any/RemoteControl, default:any/RefereeBox;
        dmc = queue/10;
    ];
}

module LebtClient [monitor] {

    pub.period = 33ms;
    pub.dmc = ringbuffer;

    sub.dmc = queue/10;
    sub.period = 33ms;
    sub.ttl = 500ms;

    pub MoveBallControl [
        dst = 1 | default:any/Simulator;
    ];

    pub SimulatorUpdateRobotPosition [
        dst = 1 | default:any/Simulator;
    ];

    pub EnforceColors [
        dst = 1 | default:ctrl_group/Base;
    ];

    sub LebtInfo [
        src = n | default:stat_group/Lebt;
    ];

    sub SystemInfo [
        src = n | default:stat_group/Lebt;
    ];

    sub BehaviourEngineInfo [
        src = n | default:stat_group/Base;
    ];

    sub SharedWorldInfo [
        src = n | default:shw_group/Base;
    ];

    sub MotionStat [
        src = n | default:stat_group/Motion;
    ];

    sub KickerStat [
        src = n | default:stat_group/Kicker;
    ];
}

module Joystick {

    pub.period = 100ms;
    pub.dmc = ringbuffer;

    pub JoystickControl [
        dst = n | default:any/Base;
    ];
}

module Replayer {

    pub.period = 100ms;
    pub.dmc = ringbuffer;

    pub WorldModelData [
        dst = n | default:localhost/Base;
    ];
}

module RemoteControl [monitorable] {

    pub.period = 33ms;
    pub.dmc = ringbuffer;

    sub.dmc = queue/10;
    sub.period = 33ms;
    sub.ttl = 500ms;

    pub BehaviorEngineControl [
        dst = n | default:any/Base;
        dmc = queue/10;
    ];

    pub LebtControl [
        dst = n | default:any/Lebt;
        dmc = queue/10;
    ];

    pub JoystickControl [
        dst = 1 | default:any/Base;
    ];

    pub KickControl [
        dst = n | default:any/Kicker;
    ];

    sub LebtModuleInfo [
        src = n | default:stat_group/Lebt;
    ];

    sub SharedWorldInfo [
        src = n | default:shw_group/Base;
    ];

    sub EnforceColors [
        src = n | default:ctrl_group/LebtClient;
    ];
}

module RefereeBox [monitorable] {

    pub.period = 33ms;
    pub.dmc = ringbuffer;

    sub.dmc = queue/10;
    sub.period = 33ms;
    sub.ttl = 500ms;

    pub RefereeBoxInfo [
        dst = 1 | default:stat_group/RefereeBoxInterface;
    ];

    pub LebtControl [
        dst = n | default:any/Lebt;
        dmc = queue/10;
    ];
}

module RefereeBoxInterface {

    sub.dmc = queue/10;
    sub.period = 33ms;
    sub.ttl = 500ms;

    sub RefereeBoxInfo [
        src = 1 | default:stat_group/RefereeBox;
    ];
}

module Compass [monitorable] {

    pub.period = 33ms;
    pub.dmc = ringbuffer;

    pub CompassMessage [
        dst = 2 | default:localhost/Base, default:localhost/Vision;
    ];
}

module CompassTestSender {

    pub.period = 33ms;
    pub.dmc = ringbuffer;

    pub CompassMessage [
        dst = 1 | default:localhost/CompassTestReceiver;
    ];
}

module CompassTestReceiver {

    sub.dmc = queue/10;
    sub.period = 33ms;
    sub.ttl = 500ms;

    sub CompassMessage [
        src = 1 | default:localhost/CompassTestSender;
    ];
}


