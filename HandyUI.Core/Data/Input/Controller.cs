//namespace HandyUI.Core.Data.Input;

//public enum ControllerButton
//{
//    None = 0,
//    A = 1,
//    B = 2,
//    X = 3,
//    Y = 4,
//    LeftBumper = 5,
//    RightBumper = 6,
//    Back = 7,
//    Start = 8,
//    LeftStickClick = 9,
//    RightStickClick = 10,
//    DPadUp = 11,
//    DPadDown = 12,
//    DPadLeft = 13,
//    DPadRight = 14
//}

//public enum ControllerEventType
//{
//    None = 0,
//    ButtonDown = 1,
//    ButtonUp = 2,
//    AxisChanged = 3
//}

//public readonly record struct ControllerButtonEventContext(
//    EventData EventData,
//    ModifierKeys ModifierKeys,
//    ControllerEventType Type,
//    ControllerButton Button
//);

//public readonly record struct ControllerAxisEventContext(
//    EventData EventData,
//    ModifierKeys ModifierKeys,
//    ControllerEventType Type,
//    float LeftStickX = 0f,
//    float LeftStickY = 0f,
//    float RightStickX = 0f,
//    float RightStickY = 0f,
//    float LeftTrigger = 0f,
//    float RightTrigger = 0f
//);