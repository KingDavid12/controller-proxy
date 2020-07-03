module ControllerProxy.Controller

open System.Windows.Forms
open ControllerProxy.KeyboardHooks
open ScpDriverInterface

type Joystick = {
    Up: Keys
    Down: Keys
    Left: Keys
    Right: Keys
}
type Config = {
    LeftJoystick: Joystick
    RightJoystick: Joystick

    LeftTrigger: Keys
    RightTrigger: Keys

    LeftStick: Keys
    RightStick: Keys

    LeftBumper: Keys
    RightBumper: Keys

    Up: Keys
    Down: Keys
    Left: Keys
    Right: Keys

    Start: Keys
    Back: Keys
    Logo: Keys

    A: Keys
    B: Keys
    X: Keys
    Y: Keys
}

let createMapping config =
    Map.ofList [
        config.LeftStick, X360Buttons.LeftStick
        config.RightStick, X360Buttons.RightStick

        config.LeftBumper, X360Buttons.LeftBumper
        config.RightBumper, X360Buttons.RightBumper

        config.Up, X360Buttons.Up
        config.Down, X360Buttons.Down
        config.Left, X360Buttons.Left
        config.Right, X360Buttons.Right

        config.Start, X360Buttons.Start
        config.Back, X360Buttons.Back
        config.Logo, X360Buttons.Logo

        config.A, X360Buttons.A
        config.B, X360Buttons.B
        config.X, X360Buttons.X
        config.Y, X360Buttons.Y
    ]

let bus = new ScpBus()
let controller = X360Controller()

let minValue, maxValue = -32760s, 32760s

let printConfig config =
    printfn "Left Stick: %A %A %A %A" config.LeftJoystick.Up config.LeftJoystick.Left config.LeftJoystick.Down config.LeftJoystick.Right
    printfn "Right Stick: %A %A %A %A" config.RightJoystick.Up config.RightJoystick.Left config.RightJoystick.Down config.RightJoystick.Right

    printfn "LT %A" config.LeftTrigger
    printfn "RT %A" config.RightTrigger

    printfn "LB %A" config.LeftBumper
    printfn "RB %A" config.RightBumper

    printfn "LS %A" config.LeftStick
    printfn "RS %A" config.RightStick

    printfn "Up %A" config.Up
    printfn "Down %A" config.Down
    printfn "Left %A" config.Left
    printfn "Right %A" config.Right

    printfn "A %A" config.A
    printfn "B %A" config.B
    printfn "X %A" config.X
    printfn "Y %A" config.Y

    printfn "Start %A" config.Start
    printfn "Back %A" config.Back
    printfn "Logo %A" config.Logo

let applyConfig config =
    printfn "Controller Plugged In: %A" (bus.PlugIn 1)
    printConfig config

    let mapping = createMapping config

    let applyKey down key =
        // Left Stick
        if key = config.LeftJoystick.Up then
            controller.LeftStickY <- if down then maxValue else 0s
        elif key = config.LeftJoystick.Down then
            controller.LeftStickY <- if down then minValue else 0s
        elif key = config.LeftJoystick.Left then
            controller.LeftStickX <- if down then minValue else 0s
        elif key = config.LeftJoystick.Right then
            controller.LeftStickX <- if down then maxValue else 0s

        // Right Stick
        elif key = config.RightJoystick.Up then
            controller.RightStickY <- if down then maxValue else 0s
        elif key = config.RightJoystick.Down then
            controller.RightStickY <- if down then minValue else 0s
        elif key = config.RightJoystick.Left then
            controller.RightStickX <- if down then minValue else 0s
        elif key = config.RightJoystick.Right then
            controller.RightStickX <- if down then maxValue else 0s

        if key = config.LeftTrigger then
            controller.LeftTrigger <- if down then 255uy else 0uy
        elif key = config.RightTrigger then
            controller.RightTrigger <- if down then 255uy else 0uy

        else
            match mapping |> Map.tryFind key with
            | Some button ->
                if down then
                    controller.Buttons <- controller.Buttons ||| button
                else
                    controller.Buttons <- controller.Buttons &&& ~~~button
            | None -> ()

        bus.Report(1, controller.GetReport()) |> ignore

    keyDown.Add (applyKey true)
    keyUp.Add (applyKey false)