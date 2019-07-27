using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VRMouse : MonoBehaviour
{

    public GameObject source;
    public GameObject destination;
    public GameObject cursor;


    public VdmDesktopManager m_manager;

    private static VRMouse _VRMouse;

    Process currentProcess;
    IntPtr activeWindow;

    SimpleWebBrowser.WebBrowser LastWebWindow = null;
    InputField LastInputField = null;

    bool hasFocus = true;
    bool isHidden = false;

    public int lastMouseX = -1;
    public int lastMouseY = -1;
    public int MouseX = -1;
    public int MouseY = -1;

    public Material blueMaterial;
    public Material grayMaterial;
    GameObject lastOverBtn;

    GameObject dragTarget = null;
    Vector3 startCursorOffset = Vector3.zero;
    bool isMouseDrag;
    bool isMouseDragRotation ; 

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

        Cursor.visible = false;
        Screen.lockCursor = true;

      //  SetPosition(activeWindow, 5000, 5000);
    }

    // Update is called once per frame
    void Update()
    {
        float speed = 35; //how fast the object should rotate
        transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * Time.deltaTime * speed);
       // Debug.Log(Input.GetAxis("Mouse Y") +" "+ Input.GetAxis("Mouse X"));

        RaycastHit hit;
        Vector3 fromPosition = source.transform.position;
        Vector3 toPosition = destination.transform.position;
        Vector3 direction = toPosition - fromPosition;

        if (Physics.Raycast(source.transform.position, direction, out hit))
        {
            UnityEngine.Debug.DrawRay(source.transform.position, direction, Color.blue);
            if (hit.collider.gameObject.tag == "Desktop")
            {
               Cursor.visible = true;
               Screen.lockCursor = false;
                cursor.SetActive(false);

                print("ray just hit the gameobject: " + hit.collider.gameObject.tag);
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
                        //  ShowAndMinimized(activeWindow);
                       // SetPosition(activeWindow, 5000, 5000);
                        isHidden = true;
                    }
                }
                else
                {
                  
                    /* int AxisX = 0;
                     int AxisY = 0;
                     if (MouseX > lastMouseX) AxisX = 1;
                     if (MouseX < lastMouseX) AxisX = -1;
                     if (MouseY > lastMouseY) AxisY = 1;
                     if (MouseY < lastMouseY) AxisY = -1;

                     transform.Rotate(new Vector3(-AxisY, AxisX, 0) * Time.deltaTime * speed);
                     */
                    //  UnityEngine.Debug.Log(MouseX + ", " + MouseY);
                    if (MouseX < 1 || MouseX > m_manager.GetScreenWidth(0) -1 || MouseY < 1 || MouseY > m_manager.GetScreenHeight(0) - 1)
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
                        SetFocusAndHide(activeWindow);
                        isHidden = false;
                        Cursor.visible = false;
                        Screen.lockCursor = true;
                        cursor.SetActive(true);
#if  UNITY_EDITOR
                        EditorApplication.ExecuteMenuItem("Window/General/Game");
#endif
                        UnityEngine.Debug.Log("SetForegroundWindow ----------");
                    }
                }

                if (Input.GetMouseButtonDown(0))
                {
                    m_manager.SimulateMouseLeftDown();
                    UnityEngine.Debug.Log("SimulateMouseLeftDown SimulateMouseLeftDown");
                    VdmDesktopManager.ActionInThisFrame = true;
                }
                if (Input.GetMouseButtonUp(0))
                {
                    m_manager.SimulateMouseLeftUp();
                    VdmDesktopManager.ActionInThisFrame = true;
                }

            }
            else
            {
                Cursor.visible = false;
                Screen.lockCursor = true;
                cursor.SetActive(true);
            }
            if (hit.collider.gameObject.tag == "WebWindow")
            {
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

                destination.transform.position = hit.point;
            //    Vector3 moveDir = (source.transform.position - cursor.transform.position).normalized;
               
                /* Vector3 localPosition = destination.transform.localPosition;
                 localPosition.z = localPosition.z - 0.01f;
                 destination.transform.localPosition = localPosition;*/

                //cursor.transform.rotation = hit.collider.gameObject.transform.rotation;
                // cursor.transform.Rotate(new Vector3(180, 0, -180));

                cursor.transform.up = hit.normal;

                cursor.transform.position = destination.transform.position;
                cursor.transform.position += hit.normal * 0.0001f;

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
       //         UnityEngine.Debug.Log("WebWindowUi hit");
      //          UnityEngine.Debug.Log("Hit " + hit.collider.gameObject.name);
                destination.transform.position = hit.point;
              //  Vector3 moveDir = (source.transform.position - cursor.transform.position).normalized;
                //  cursor.transform.rotation = hit.collider.gameObject.transform.rotation;
              //  cursor.transform.Rotate(new Vector3(180, 0, -180));
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
            else
            {
                if(LastInputField != null) LastInputField.OnDeselect(new BaseEventData(EventSystem.current));
                if (lastOverBtn != null) lastOverBtn.GetComponent<Renderer>().material = blueMaterial;

            }

            

            if (hit.collider.gameObject.tag == "Borders")
            {
                destination.transform.position = hit.point;
             //   Vector3 moveDir = (source.transform.position - cursor.transform.position).normalized;
                /*  Vector3 localPosition = destination.transform.localPosition;
                   localPosition.z = localPosition.z - 0.0001f;
                   destination.transform.localPosition = localPosition;*/
                //cursor.transform.rotation = hit.collider.gameObject.transform.rotation;
                // cursor.transform.Rotate(new Vector3(180, 0, -180));
                cursor.transform.up = hit.normal;
                  cursor.transform.position = destination.transform.position;
                cursor.transform.position += hit.normal * 0.0001f;
                // cursor.transform.LookAt(source.transform);
                // cursor.transform.Rotate(new Vector3(90, 0, 0));
            }

             print("ray just hit the gameobject: " + hit.collider.gameObject.tag);
        }


        if (Input.GetMouseButtonUp(0))
        {
            DataController.Instance.UpdateWindowData(dragTarget);
            isMouseDrag = false;
            isMouseDragRotation = false;
        //    UnityEngine.Debug.Log("isMouseDrag " + isMouseDrag);
        }

        if (isMouseDrag)
        {
            if (dragTarget.tag == "Desktop" || dragTarget.tag == "Phone")
            {
                dragTarget.transform.Translate(new Vector3(Input.GetAxis("Mouse X") , Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse ScrollWheel") * 10) * Time.deltaTime * 4);
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

    void OnApplicationFocus(bool hasFocus)
    {
        this.hasFocus =  hasFocus;
     //   UnityEngine.Debug.Log("hasFocus ----------" + hasFocus);
        if (!hasFocus)
        {

        }

    }

    [DllImport("user32.dll")] static extern uint GetActiveWindow();
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hWnd);


    //  [DllImport("user32.dll")]
    //  static extern bool SetForegroundWindow(IntPtr hWnd);

    // SetFocus will just focus the keyboard on your application, but not bring your process to front.
    // You don't need it here, SetForegroundWindow does the same.
    // Just for documentation.
    [DllImport("user32.dll")]
    static extern bool SetFocus(IntPtr hWnd);


    [System.Runtime.InteropServices.DllImport("user32.dll")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

  //  [System.Runtime.InteropServices.DllImport("user32.dll")]
 //   private static extern int SetForegroundWindow(IntPtr hwnd);

    private enum ShowWindowEnum
    {
        Hide = 0,
        ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
        Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
        Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
        Restore = 9, ShowDefault = 10, ForceMinimized = 11
    };

    public void ShowAndMinimized(IntPtr MainWindowHandle)
    {
        // check if the window is hidden / minimized
        if (MainWindowHandle == IntPtr.Zero)
        {
            // the window is hidden so try to restore it before setting focus.
            ShowWindow(MainWindowHandle, ShowWindowEnum.Restore);
        }

       ShowWindow(MainWindowHandle, ShowWindowEnum.Minimize);
    }

    public void SetFocusAndHide(IntPtr MainWindowHandle)
    {
        // check if the window is hidden / minimized
        if (MainWindowHandle == IntPtr.Zero)
        {
            // the window is hidden so try to restore it before setting focus.
            ShowWindow(MainWindowHandle, ShowWindowEnum.Restore);
        }
    

        // set user the focus to the window
        SetForegroundWindow(MainWindowHandle);
     //   SetPosition(activeWindow, 5000, 5000);
        //       ShowWindow(MainWindowHandle, ShowWindowEnum.Hide);

    }




#if UNITY_STANDALONE_WIN || UNITY_EDITOR
    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);


    public static void SetPosition(IntPtr hwnd, int x, int y, int resX = 0, int resY = 0)
    {
        SetWindowPos(hwnd, 0, x, y, resX, resY, resX * resY == 0 ? 1 : 0);
    }
#endif
}


