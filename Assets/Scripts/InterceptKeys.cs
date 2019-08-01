using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

/// <summary>  
///  InterceptKeys class is a low-level keyboard hook with C#
///  to track the keyboard events even if the unity Application runs in the background Mode
///  
///  Low-Level Keyboard Hook in C# From Microsoft Bolg
///  https://blogs.msdn.microsoft.com/toub/2006/05/03/low-level-keyboard-hook-in-c/
///  "Low-Level Keyboard Hook in C#" by Stephen Toub - MSFT, 03.05.2006
///  Accessed 01.07.2019
/// </summary> 

public class InterceptKeys : MonoBehaviour
{

    private const int WH_KEYBOARD_LL = 13;

    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
     

    private static LowLevelKeyboardProc _proc = HookCallback;

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


    private static IntPtr SetHook(LowLevelKeyboardProc proc)

    {

        using (Process curProcess = Process.GetCurrentProcess())

        using (ProcessModule curModule = curProcess.MainModule)

        {

            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,

                GetModuleHandle(curModule.ModuleName), 0);

        }


    }


    private delegate IntPtr LowLevelKeyboardProc(

        int nCode, IntPtr wParam, IntPtr lParam);


    private static IntPtr HookCallback(

        int nCode, IntPtr wParam, IntPtr lParam)

    {

        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)

        {

            int vkCode = Marshal.ReadInt32(lParam);
          //  UnityEngine.Debug.Log(vkCode);


        }

        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN && Marshal.ReadInt32(lParam) == 162)
        {
            KeyboardLocation.GetInstance().LeftControlKeyDown = true;
         }
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP && Marshal.ReadInt32(lParam) == 162)
        {
            KeyboardLocation.GetInstance().LeftControlKeyDown = false;
        }
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP && Marshal.ReadInt32(lParam) == 39)
        {
            KeyboardLocation.GetInstance().RightKeyUp  = true;
        }





        return CallNextHookEx(_hookID, nCode, wParam, lParam);

    }


    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

    private static extern IntPtr SetWindowsHookEx(int idHook,

        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);


    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

    [return: MarshalAs(UnmanagedType.Bool)]

    private static extern bool UnhookWindowsHookEx(IntPtr hhk);


    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,

        IntPtr wParam, IntPtr lParam);


    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]

    private static extern IntPtr GetModuleHandle(string lpModuleName);

}

