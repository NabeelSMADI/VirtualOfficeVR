using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class InterceptMouse : MonoBehaviour
{
    private static MSLLHOOKSTRUCT hookStruct;

    private static LowLevelMouseProc _proc = HookCallback;

    private static IntPtr _hookID = IntPtr.Zero;
    // Start is called before the first frame update
    void Start()
    {
        UnhookWindowsHookEx(_hookID);
        _hookID = SetHook(_proc);
    }

    void OnApplicationQuit()
    {
        UnhookWindowsHookEx(_hookID);
    }

    // Update is called once per frame
    void Update()
    {

    }


    private static IntPtr SetHook(LowLevelMouseProc proc)

    {

        using (Process curProcess = Process.GetCurrentProcess())

        using (ProcessModule curModule = curProcess.MainModule)

        {
            return SetWindowsHookEx(WH_MOUSE_LL, proc,GetModuleHandle(curModule.ModuleName), 0);

        }

    }


    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);


    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam){

      //  if (nCode >= 0 && MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam){

            hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

        if(VRMouse.GetInstance().lastMouseX == -1)
        {
            VRMouse.GetInstance().lastMouseX = hookStruct.pt.x;
            VRMouse.GetInstance().lastMouseY = hookStruct.pt.y;
        }
        VRMouse.GetInstance().MouseX = hookStruct.pt.x;
        VRMouse.GetInstance().MouseY = hookStruct.pt.y;
        // UnityEngine.Debug.Log(MouseMessages.WM_MOUSEMOVE + ", " + hookStruct.pt.y);
        //   }hookStruct
        return CallNextHookEx(_hookID, nCode, wParam, lParam);

    }


    private const int WH_MOUSE_LL = 14;

    private MSLLHOOKSTRUCT HookStruct
    {
        get
        {
            return hookStruct;
        }

        set
        {
            hookStruct = value;
        }
    }

    private enum MouseMessages

    {

        WM_LBUTTONDOWN = 0x0201,

        WM_LBUTTONUP = 0x0202,

        WM_MOUSEMOVE = 0x0200,

        WM_MOUSEWHEEL = 0x020A,

        WM_RBUTTONDOWN = 0x0204,

        WM_RBUTTONUP = 0x0205

    }


    [StructLayout(LayoutKind.Sequential)]

    private struct POINT

    {

        public int x;

        public int y;

    }


    [StructLayout(LayoutKind.Sequential)]

    private struct MSLLHOOKSTRUCT

    {

        public POINT pt;

        public uint mouseData;

        public uint flags;

        public uint time;

        public IntPtr dwExtraInfo;

    }


    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

    private static extern IntPtr SetWindowsHookEx(int idHook,

        LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);


    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

    [return: MarshalAs(UnmanagedType.Bool)]

    private static extern bool UnhookWindowsHookEx(IntPtr hhk);


    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,

        IntPtr wParam, IntPtr lParam);


    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]

    private static extern IntPtr GetModuleHandle(string lpModuleName);

}


