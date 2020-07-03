module ControllerProxy.KeyboardHooks

open System
open System.Threading
open System.Diagnostics
open System.Windows.Forms
open System.Collections.Concurrent
open System.Runtime.InteropServices

let WH_KEYBOARD_LL = 13

let WM_KEYDOWN = 0x0100
let WM_KEYUP = 0x0101

let WM_SYSKEYDOWN = 0x0104
let WM_SYSKEYUP = 0x0105

// IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
type LowLevelKeyboardProc = delegate of int * IntPtr * IntPtr -> IntPtr

[<DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

[<DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
extern [<return: MarshalAs(UnmanagedType.Bool)>] bool UnhookWindowsHookEx(IntPtr hhk);

[<DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

[<DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
extern IntPtr GetModuleHandle(string lpModuleName);

let mutable private hookId = IntPtr.Zero

let setHook (callback: LowLevelKeyboardProc) =
    use currentProcess = Process.GetCurrentProcess()
    use currentModule = currentProcess.MainModule
    
    SetWindowsHookEx(WH_KEYBOARD_LL, callback, GetModuleHandle(currentModule.ModuleName), 0u)

let private _keyDown = Event<Keys>()
let private _keyUp = Event<Keys>()

let keyDown = _keyDown.Publish
let keyUp = _keyUp.Publish

let private _queue = new BlockingCollection<_>(ConcurrentQueue<bool * Keys>())

let processEvents () =
    Thread(ThreadStart (fun () ->
        while true do
            let down, key = _queue.Take()

            if down then
                _keyDown.Trigger key
            else
                _keyUp.Trigger key
    )).Start()

let callback = LowLevelKeyboardProc (fun nCode wParam lParam ->
    if nCode >= 0 && (wParam = nativeint WM_KEYDOWN || wParam = nativeint WM_KEYUP || wParam = nativeint WM_SYSKEYDOWN || wParam = nativeint WM_SYSKEYUP) then
        let vkCode = Marshal.ReadInt32 lParam
        let key = enum<Keys> vkCode

        _queue.Add (((wParam = nativeint WM_KEYDOWN || wParam = nativeint WM_SYSKEYDOWN), key))
    
    CallNextHookEx(hookId, nCode, wParam, lParam)
)

let run () =
    processEvents()

    let worker = Thread(ThreadStart(fun () ->
        hookId <- setHook callback
        Application.Run()
        UnhookWindowsHookEx hookId |> ignore
    ))

    worker.IsBackground <- false
    worker.Start()