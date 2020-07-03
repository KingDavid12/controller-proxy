module ControllerProxy.Program

open System
open System.Windows.Forms
open ControllerProxy.Controller
open ControllerProxy.KeyboardHooks

let defaultConfig = {
    LeftJoystick = {
        Up = Keys.W
        Down = Keys.S
        Left = Keys.A
        Right = Keys.D
    }
    RightJoystick = {
        Up = Keys.Up
        Down = Keys.Down
        Left = Keys.Left
        Right = Keys.Right
    }

    LeftTrigger = Keys.Space
    RightTrigger = Keys.LShiftKey

    LeftBumper = Keys.Q
    RightBumper = Keys.E

    LeftStick = Keys.OemOpenBrackets
    RightStick = Keys.OemCloseBrackets

    Up = Keys.I
    Down = Keys.K
    Left = Keys.J
    Right = Keys.L

    Y = Keys.Home
    A = Keys.End
    X = Keys.Delete
    B = Keys.PageDown

    Start = Keys.Enter
    Back = Keys.Back

    Logo = Keys.T
}

let configs = [
    "Default Config", defaultConfig
]

[<EntryPoint>]
let main _ =
    bus.UnplugAll() |> ignore

    for index, (name, _) in List.indexed configs do
        printfn "%i) %s" (index + 1) name

    let number = int (Console.ReadLine())

    applyConfig (snd configs.[number - 1])
    run()

    printfn "Press enter to exit."
    Console.ReadLine() |> ignore

    0