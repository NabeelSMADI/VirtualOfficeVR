using UnityEngine;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>  
///  The class VRMouse.cs is responsible for moving the mouse pointer in the environment and sending events to the windows.
///  
/// </summary> 

public class VRMouse : MonoBehaviour
{
    private static VRMouse _VRMouse; //!< VRMouse Singleton instance

    public GameObject source; //!< The start point of the Mouse RayCast
    public GameObject destination; //!< The End point of the Mouse RayCast
    public GameObject cursor;  //!< The cursor GameObject

    public VdmDesktopManager m_manager; //!< The VR Desktop Mirror API Manager

    Process currentProcess;  //!< Application current Process
    IntPtr activeWindow; //!< Application active Window

    SimpleWebBrowser.WebBrowser LastWebWindow = null; //!< Last Active Web Window
    InputField LastInputField = null;  //!< Last Active Web Window InputField

    bool hasFocus = true;  //!< Application has Focus
    bool isHidden = false; //!< Application isHidden

    public int lastMouseX = -1; //!< last Mouse Position on the X axis
    public int lastMouseY = -1; //!< last Mouse Position on the Y axis
    public int MouseX = -1; //!< Mouse Position on the X axis
    public int MouseY = -1; //!< Mouse Position on the Y axis

    public Material blueMaterial; //!< Blue Material for the Window UI
    public Material grayMaterial; //!< Gray Material for the Window UI
    GameObject lastOverBtn; //!< Last Button with cursor Focus

    GameObject dragTarget = null;  //!< Active drag Target

    bool isMouseDrag;  //!<  is drag Active
    bool isMouseDragRotation;  //!< is Rotation drag Active

    public Camera DesktopCamera; //!< DesktopCamera help to Hide the Application Window

    public static VRMouse GetInstance()
    {
        return _VRMouse;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(_VRMouse == null)
        {
            _VRMouse = this;
        }

        currentProcess = Process.GetCurrentProcess();
        UnityEngine.Debug.Log("currentProcess "+ currentProcess.ProcessName);

        activeWindow = (IntPtr)GetActiveWindow();

        lockCursor(true);
        ShowAndSetTransparent(activeWindow);
    }

    // Update is called once per frame
    void Update()
    {
        MoveVRMouseCursor();

        SendRaycast();

        CheckWindowDrag();
    }

    /// <summary>  
    ///  Move the VR Mouse Cursor with the Mouse Input.
    /// </summary> 
    void MoveVRMouseCursor()
    {
        float speed = 35; //how fast the object should rotate
        transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * Time.deltaTime * speed);
    }

    /// <summary>  
    ///  Send Raycast to detect windows and then check the window type by TAG
    /// </summary> 
    void SendRaycast()
    {
        RaycastHit hit;
        Vector3 fromPosition = source.transform.position;
        Vector3 toPosition = destination.transform.position;
        Vector3 direction = toPosition - fromPosition;

        if (Physics.Raycast(source.transform.position, direction, out hit))
        {
            UnityEngine.Debug.DrawRay(source.transform.position, direction, Color.blue);
            if (hit.collider.gameObject.tag == "Desktop")
            {
                CursorOnDesktop(hit);
            }
            else
            {
                lockCursor(true);
            }
            if (hit.collider.gameObject.tag == "WebWindow")
            {
                CursorOnWebWindow(hit);
            }
            else
            {
                if (LastWebWindow != null)
                {
                    LastWebWindow.OnMouseExit();
                    LastWebWindow = null;
                }
            }

            if (hit.collider.gameObject.tag == "WebWindowUi")
            {
                CursorOnWindowUi(hit);
            }
            else
            {
                if (LastInputField != null) LastInputField.OnDeselect(new BaseEventData(EventSystem.current));
                if (lastOverBtn != null) lastOverBtn.GetComponent<Renderer>().material = blueMaterial;

            }
            if (hit.collider.gameObject.tag == "Borders")
            {
                destination.transform.position = hit.point;
                cursor.transform.up = hit.normal;
                cursor.transform.position = destination.transform.position;
                cursor.transform.position += hit.normal * 0.0001f;
            }
        }
       
    }

    /// <summary>  
    ///  lock and unlock the Cursor.
    ///  
    /// <param name="lockC">True or false to lock and unlock</param>
    /// </summary> 
    void lockCursor(bool lockC)
    {
        if (lockC)
        {
            Cursor.visible = false;
            Screen.lockCursor = true;
            cursor.SetActive(true);
        }
        else
        {
            Cursor.visible = true;
            Screen.lockCursor = false;
            cursor.SetActive(false);
        }
    }

    /// <summary>  
    ///  When the Raycast hits the desktop mirror window.
    ///  
    /// <param name="hit">The RaycastHit point</param>
    /// </summary> 
    void CursorOnDesktop(RaycastHit hit)
    {
        lockCursor(false);
        destination.transform.position = hit.point;
        Vector3 localPosition = destination.transform.localPosition;
        localPosition.z = localPosition.z - 0.0001f;
        destination.transform.localPosition = localPosition;
        cursor.transform.position = destination.transform.position;
        cursor.transform.rotation = hit.collider.gameObject.transform.rotation;
        cursor.transform.Rotate(new Vector3(90, -180, 0));

        float dx = m_manager.GetScreenWidth(0);
        float dy = m_manager.GetScreenHeight(0);
        float vx = hit.textureCoord.x;
        float vy = hit.textureCoord.y;
        vy = 1 - vy;
        float x = (vx * dx);
        float y = (vy * dy);
        int iX = (int)x;
        int iY = (int)y;

        if (hasFocus)
        {
            m_manager.SetCursorPos(iX, iY);
               if (!isHidden)
               {
                  HideApplicationWindow(activeWindow);
                // SetPosition(activeWindow, 5000, 5000);
                isHidden = true;
              }
        }
        else
        {
            if (MouseX < 1 || MouseX > m_manager.GetScreenWidth(0) - 1 || MouseY < 1 || MouseY > m_manager.GetScreenHeight(0) - 1)
            {
                GameObject monitor = m_manager.gameObject.transform.GetChild(0).gameObject;
                Vector3 pos = monitor.transform.position;
                pos.x -= monitor.transform.localScale.x / 2;
                pos.y += monitor.transform.localScale.y / 2;
                pos.x += (MouseX / m_manager.GetScreenWidth(0)) * monitor.transform.localScale.x;
                pos.y -= (MouseY / m_manager.GetScreenHeight(0)) * monitor.transform.localScale.y;
                source.transform.LookAt(pos);

                //     SetForegroundWindow(activeWindow);
                //      SetFocusOnMainWindow(activeWindow);
                ShowAndSetTransparent(activeWindow);

                lockCursor(true);
                isHidden = false;

#if UNITY_EDITOR
                EditorApplication.ExecuteMenuItem("Window/General/Game");
#endif
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            m_manager.SimulateMouseLeftDown();
            VdmDesktopManager.ActionInThisFrame = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            m_manager.SimulateMouseLeftUp();
            VdmDesktopManager.ActionInThisFrame = true;
        }
    }

    /// <summary>  
    ///  When the Raycast hits the WebWindow window.
    ///  
    /// <param name="hit">The RaycastHit point</param>
    /// </summary> 
    void CursorOnWebWindow(RaycastHit hit)
    {
        destination.transform.position = hit.point;
        cursor.transform.up = hit.normal;
        cursor.transform.position = destination.transform.position;
        cursor.transform.position += hit.normal * 0.0001f;

        if (LastWebWindow == null)
        {
            LastWebWindow = hit.collider.gameObject.GetComponent<SimpleWebBrowser.WebBrowser>();
            LastWebWindow.OnMouseEnter();

        }
        else
        {
            if (LastWebWindow.Port != hit.collider.gameObject.GetComponent<SimpleWebBrowser.WebBrowser>().Port)
            {
                LastWebWindow.OnMouseExit();
                LastWebWindow = hit.collider.gameObject.GetComponent<SimpleWebBrowser.WebBrowser>();
                LastWebWindow.OnMouseEnter();
            }
        }
    }

    /// <summary>  
    ///  When the Raycast hits the WindowUi (Titlebar).
    ///  
    /// <param name="hit">The RaycastHit point</param>
    /// </summary> 
    void CursorOnWindowUi(RaycastHit hit)
    {
        destination.transform.position = hit.point;
        cursor.transform.up = hit.normal;
        cursor.transform.position = destination.transform.position;
        cursor.transform.position += hit.normal * 0.01f;

        if (hit.collider.gameObject.name == "UrlFieldBG" && Input.GetMouseButtonDown(0))
        {
            UnityEngine.Debug.Log("UrlFieldBG Mouse");
            LastInputField = hit.collider.gameObject.GetComponentInParent<InputField>();
            LastInputField.Select();
            LastInputField.ActivateInputField();
        }
        else if (hit.collider.gameObject.name == "BBtn" && Input.GetMouseButtonDown(0))
        {
            SimpleWebBrowser.WebBrowser WebWindow = hit.collider.gameObject.GetComponentInParent<SimpleWebBrowser.WebBrowser>();
            WebWindow.GoBackForward(false);
            UnityEngine.Debug.Log("BBtn");
        }
        else if (hit.collider.gameObject.name == "FBtn" && Input.GetMouseButtonDown(0))
        {
            SimpleWebBrowser.WebBrowser WebWindow = hit.collider.gameObject.GetComponentInParent<SimpleWebBrowser.WebBrowser>();
            WebWindow.GoBackForward(true);
            UnityEngine.Debug.Log("FBtn");
        }
        else if (hit.collider.gameObject.name == "CloseBtn")
        {
            if (lastOverBtn != null) lastOverBtn.GetComponent<Renderer>().material = blueMaterial;
            lastOverBtn = hit.collider.gameObject;
            hit.collider.gameObject.GetComponent<Renderer>().material = grayMaterial;
            if (Input.GetMouseButtonDown(0))
            {
                DataController.Instance.RemoveWindowData(hit.collider.gameObject.transform.parent.gameObject);
                Destroy(hit.collider.gameObject.transform.parent.gameObject);
            }
        }
        else if (hit.collider.gameObject.name == "RotationBtn")
        {
            if (lastOverBtn != null) lastOverBtn.GetComponent<Renderer>().material = blueMaterial;
            lastOverBtn = hit.collider.gameObject;
            hit.collider.gameObject.GetComponent<Renderer>().material = grayMaterial;
            if (Input.GetMouseButtonDown(0))
            {
                isMouseDragRotation = true;
                dragTarget = hit.collider.gameObject.transform.parent.gameObject;
            }
        }
        else if (hit.collider.gameObject.name == "Size+Btn")
        {
            if (lastOverBtn != null) lastOverBtn.GetComponent<Renderer>().material = blueMaterial;
            lastOverBtn = hit.collider.gameObject;
            hit.collider.gameObject.GetComponent<Renderer>().material = grayMaterial;
            if (Input.GetMouseButtonDown(0))
            {
                if (hit.collider.gameObject.transform.parent.tag == "Desktop" || hit.collider.gameObject.transform.parent.tag == "Phone")
                {
                    hit.collider.gameObject.transform.parent.localScale += new Vector3(transform.localScale.x * 0.05f, transform.localScale.y * 0.05f, 0) * Time.deltaTime * 10;
                }
                else
                {
                    hit.collider.gameObject.transform.parent.localScale += new Vector3(transform.localScale.x * 0.05f, 0, transform.localScale.z * 0.05f) * Time.deltaTime * 10;
                }
                DataController.Instance.UpdateWindowData(hit.collider.gameObject.transform.parent.gameObject);
            }
        }
        else if (hit.collider.gameObject.name == "Size-Btn")
        {
            if (lastOverBtn != null) lastOverBtn.GetComponent<Renderer>().material = blueMaterial;
            lastOverBtn = hit.collider.gameObject;
            hit.collider.gameObject.GetComponent<Renderer>().material = grayMaterial;
            if (Input.GetMouseButtonDown(0))
            {
                if (hit.collider.gameObject.transform.parent.tag == "Desktop" || hit.collider.gameObject.transform.parent.tag == "Phone")
                {
                    hit.collider.gameObject.transform.parent.localScale -= new Vector3(transform.localScale.x * 0.05f, transform.localScale.y * 0.05f, 0) * Time.deltaTime * 10;
                }
                else
                {
                    hit.collider.gameObject.transform.parent.localScale -= new Vector3(transform.localScale.x * 0.05f, 0, transform.localScale.z * 0.05f) * Time.deltaTime * 10;
                }
                DataController.Instance.UpdateWindowData(hit.collider.gameObject.transform.parent.gameObject);
            }
        }
        else if (hit.collider.gameObject.name == "Background" && Input.GetMouseButtonDown(0))
        {
            dragTarget = hit.collider.gameObject.transform.parent.gameObject;
            isMouseDrag = true;
            if (lastOverBtn != null) lastOverBtn.GetComponent<Renderer>().material = blueMaterial;
        }
    }

    /// <summary>  
    ///  Check WindowUI Drag and Drop.
    ///  
    /// </summary> 
    void CheckWindowDrag()
    {
        if (Input.GetMouseButtonUp(0))
        {
            DataController.Instance.UpdateWindowData(dragTarget);
            isMouseDrag = false;
            isMouseDragRotation = false;
        }

        if (isMouseDrag)
        {
            if (dragTarget.tag == "Desktop" || dragTarget.tag == "Phone")
            {
                dragTarget.transform.Translate(new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse ScrollWheel") * 10) * Time.deltaTime * 4);
            }
            else
            {
                dragTarget.transform.Translate(new Vector3(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse ScrollWheel") * 10, Input.GetAxis("Mouse Y")) * Time.deltaTime * 4);
            }
        }

        if (isMouseDragRotation)
        {
            if (dragTarget.tag == "Desktop" || dragTarget.tag == "Phone")
            {
                dragTarget.transform.Rotate(new Vector3(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), Input.GetAxis("Mouse ScrollWheel") * 2) * Time.deltaTime * 30, Space.Self);
            }
            else
            {
                dragTarget.transform.Rotate(new Vector3(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse ScrollWheel") * 2, Input.GetAxis("Mouse X")) * Time.deltaTime * 30, Space.Self);
            }
        }
    }

    /// <summary>  
    ///  Check if the Application has Focus
    ///  
    /// <param name="hasFocus">has Focus?</param>
    /// </summary> 
    void OnApplicationFocus(bool hasFocus)
    {
        this.hasFocus = hasFocus;
    }

    /// <summary>  
    ///  Hide The Application Window
    ///  
    /// <param name="IntPtr">The Application Process</param>
    /// </summary> 
    public void HideApplicationWindow (IntPtr MainWindowHandle)
    {
       ShowWindow(MainWindowHandle, ShowWindowEnum.Hide);
    }

    /// <summary>  
    ///  Show The Application Window And Set it as Transparent Window
    ///  
    /// <param name="IntPtr">The Application Process</param>
    /// </summary> 
    public void ShowAndSetTransparent(IntPtr MainWindowHandle)
    {
        ShowWindow(MainWindowHandle, ShowWindowEnum.Restore);
        ShowWindow(MainWindowHandle, ShowWindowEnum.ShowNormal);
        SetForegroundWindow(MainWindowHandle);
        DesktopCamera.backgroundColor = new Color();
        DesktopCamera.clearFlags = CameraClearFlags.SolidColor;

#if !UNITY_EDITOR

        Rectangle margins = new Rectangle() {Left = -1};
        const int GWL_STYLE = -16;
        const uint WS_POPUP = 0x80000000;
        const uint WS_VISIBLE = 0x10000000;
        SetWindowLong(MainWindowHandle, GWL_STYLE, WS_POPUP | WS_VISIBLE);
        DwmExtendFrameIntoClientArea(MainWindowHandle, ref margins);
#endif
    }



    /// <summary>  
    ///  Get The Application Process
    /// </summary> 
    [DllImport("user32.dll")] static extern uint GetActiveWindow();

    /// <summary>  
    ///  Set The Application Window as a ForegroundWindow
    /// </summary> 
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hWnd);

    /// <summary>  
    ///  Show The Application Window
    /// </summary> 
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

    /// <summary>  
    ///  Changes an attribute of the specified window.
    /// </summary> 
    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    /// <summary>  
    ///  Extends the window frame into the client area.
    /// </summary> 
    [DllImport("Dwmapi.dll")]
    static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Rectangle margins);

    /// <summary>  
    ///  ShowWindow Method enum
    ///  
    /// </summary> 
    private enum ShowWindowEnum
    {
        Hide = 0,
        ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
        Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
        Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
        Restore = 9, ShowDefault = 10, ForceMinimized = 11
    };

    /// <summary>  
    ///  Rectangle struct used in the DwmExtendFrameIntoClientArea Method
    /// </summary> 
    [StructLayout(LayoutKind.Sequential)]
    public struct Rectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

}


