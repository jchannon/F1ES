namespace F1ES

module Constants = 
    open Microsoft.FSharp.Core

    [<Literal>]
    let Start = "start"

    [<Literal>]
    let Stop = "stop"

    [<Literal>]
    let Restart = "restart"

    [<Literal>]
    let RedFlag = "redflag"

    [<Literal>]
    let OpenPitLane = "openpitlane"

    [<Literal>]
    let ClosePitLane = "closepitlane"

    [<Literal>]
    let DelayStartTime = "delay"
    
    [<Literal>]
    let EnterPitLane = "enterpitlane"
    
    [<Literal>]
    let ExitPitLane = "exitpitlane"
    
    [<Literal>]
    let EnterPitBox = "enterpitbox"
    
    [<Literal>]
    let ExitPitBox = "exitpitbox"
    
    [<Literal>]
    let StartLap = "startlap"
    
    [<Literal>]
    let DeploySafetyCar = "deploysafetycar"
    
    [<Literal>]
    let RecallSafetyCar = "recallsafetycar"
    
    [<Literal>]
    let DeployVirtualSafetyCar = "deployvirtualsafetycar"
    
    [<Literal>]
    let RecallVirtualSafetyCar = "recallvirtualsafetycar"

    [<Literal>]
    let ChangeTyre = "changetyre"
    
    [<Literal>]
    let ChangeNose = "changenose"
    
    [<Literal>]
    let ChangeDownforce = "changedownforce"
    
    [<Literal>]
    let ApplyPenaltyPoints = "applypenaltypoints"
    
    [<Literal>]
    let ApplyDriveThroughPenalty = "applydrivethroughpenalty"
    
    [<Literal>]
    let RetireDriver = "retiredriver"
    
    [<Literal>]
    let BlackFlag = "blackflag"