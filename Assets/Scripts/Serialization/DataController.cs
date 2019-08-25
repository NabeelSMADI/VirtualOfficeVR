using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;

/// <summary>  
///  The DataController functions as a global object
///  that persists over scenes. It's responsability
///  is to save the Windows to and from a file.
///  
///  Save and Load implemented following the tutorial on 
///  https://www.sitepoint.com/saving-and-loading-player-game-data-in-unity/?fbclid=IwAR1eXRSoYcsvvfLzSFCiD8J_nX1_pKqhL3Z_DEvdeThe7VCdPOa74Ez9Uog
///  https://github.com/Eudaimonium/SitepointExample_SavingData/tree/57e166c65e0d435db7fbc7d79bab078b829b402e
///  "Saving and Loading Player Game Data in Unity" by Zdravko Jakupec, 21.10.2015
///  Accessed 01.07.2019
/// </summary> 

public class DataController : MonoBehaviour
{

    public static DataController Instance;
    public List<GameObject> WindowsPrefabsList; //!< list of Windows GameObjects
    public List<WindowData> data = new List<WindowData>(); //!< list of Windows data
    public Dictionary<GameObject, WindowData> WindowsList = new Dictionary<GameObject, WindowData>(); //!< Dictionary of Windows data and GameObjects


    bool DesktopExist = false; //!< is Desktop Window Exist
    bool PhoneExist = false; //!< is Phone Window Exist

    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this) Destroy(gameObject);
    }

    void Start()
    {
        LoadAndCreateWindows();
    }

    /// <summary>  
    ///  This function will save the data to a file "save.binary".
    /// </summary> 
    public void Save()
    {
        if (!Directory.Exists("Saves")) Directory.CreateDirectory("Saves");

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile = File.Create("Saves/save.binary");

        formatter.Serialize(saveFile, WindowsList.Values.ToList());

        saveFile.Close();
    }

    /// <summary>  
    /// This function will load the data from the file "save.binary".
    /// 
    /// \return the List of Nodes loaded from the storage
    /// </summary> 
    public List<WindowData> Load()
    {
        if (File.Exists("Saves/save.binary"))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream saveFile = File.Open("Saves/save.binary", FileMode.Open);

            data = (List<WindowData>)formatter.Deserialize(saveFile);

            saveFile.Close();

            return data;
        }
        else return new List<WindowData>();
    }

    /// <summary>  
    /// This function will load the data from the file "save.binary",
    /// and create all the windows depending on the saved data.
    /// </summary> 
    public void LoadAndCreateWindows()
    {
        data = Load();
        for (int i = 0; i < data.Count; i++)
        {
            GameObject window = Instantiate(WindowsPrefabsList[data[i].type]) as GameObject;
            window.transform.localPosition = new Vector3(data[i].xPos, data[i].yPos, data[i].zPos);
            window.transform.localScale = new Vector3(data[i].xSca, data[i].ySca, data[i].zSca);
            window.transform.eulerAngles = new Vector3(data[i].xRot, data[i].yRot, data[i].zRot);
            WindowsList.Add(window, data[i]);
         }
    }

    /// <summary>  
    ///  Update the Raw data if the move transform change.
    ///  
    /// <param name="Window">The Window GameObject</param>
    /// </summary> 
    public void UpdateWindowData(GameObject Window)
    {
        WindowData cuurentWindowData = WindowsList[Window];
        cuurentWindowData.xPos = Window.transform.localPosition.x;
        cuurentWindowData.yPos = Window.transform.localPosition.y;
        cuurentWindowData.zPos = Window.transform.localPosition.z;
        cuurentWindowData.xRot = Window.transform.eulerAngles.x;
        cuurentWindowData.yRot = Window.transform.eulerAngles.y;
        cuurentWindowData.zRot = Window.transform.eulerAngles.z;
        cuurentWindowData.xSca = Window.transform.localScale.x;
        cuurentWindowData.ySca = Window.transform.localScale.y;
        cuurentWindowData.zSca = Window.transform.localScale.z;
    }

    /// <summary>  
    ///  Add new Window the Raw data.
    ///  
    /// <param name="type">The Window Type</param>
    /// </summary> 
    public void AddWindow(int type)
    {
        if (DesktopExist && type == 0) return;
        if (PhoneExist && type == 1) return;
        if (type == 0) DesktopExist = true;
        if (type == 1) PhoneExist = true;

        GameObject Window = Instantiate(WindowsPrefabsList[type].gameObject) as GameObject;
        WindowData cuurentWindowData = new WindowData();
        cuurentWindowData.type = type;
        cuurentWindowData.xPos = Window.transform.localPosition.x;
        cuurentWindowData.yPos = Window.transform.localPosition.y;
        cuurentWindowData.zPos = Window.transform.localPosition.z;
        cuurentWindowData.xRot = Window.transform.eulerAngles.x;
        cuurentWindowData.yRot = Window.transform.eulerAngles.y;
        cuurentWindowData.zRot = Window.transform.eulerAngles.z;
        cuurentWindowData.xSca = Window.transform.localScale.x;
        cuurentWindowData.ySca = Window.transform.localScale.y;
        cuurentWindowData.zSca = Window.transform.localScale.z;
        WindowsList.Add(Window, cuurentWindowData);
    }

    /// <summary>  
    ///  Remove Window Raw data.
    ///  
    /// <param name="Window">The Window GameObject</param>
    /// </summary> 
    public void RemoveWindowData(GameObject Window)
    {
        WindowData cuurentWindowData = WindowsList[Window];
        if (cuurentWindowData.type == 0) DesktopExist = false;
        if (cuurentWindowData.type == 1) PhoneExist = false;
        WindowsList.Remove(Window);
    }


    /// <summary>  
    ///  OnApplicationQuit Save all the raw data to file 
    /// </summary> 
    private void OnApplicationQuit()
    {
        foreach (KeyValuePair<GameObject, WindowData> w in WindowsList) // loop through both
        {
            GameObject Window = w.Key;
            WindowData cuurentWindowData = w.Value;
            cuurentWindowData.xPos = Window.transform.localPosition.x;
            cuurentWindowData.yPos = Window.transform.localPosition.y;
            cuurentWindowData.zPos = Window.transform.localPosition.z;
            cuurentWindowData.xRot = Window.transform.eulerAngles.x;
            cuurentWindowData.yRot = Window.transform.eulerAngles.y;
            cuurentWindowData.zRot = Window.transform.eulerAngles.z;
            cuurentWindowData.xSca = Window.transform.localScale.x;
            cuurentWindowData.ySca = Window.transform.localScale.y;
            cuurentWindowData.zSca = Window.transform.localScale.z;

        }
        data = WindowsList.Values.ToList();
        Save();
    }

    public void ApplicationQuit()
    {
        Application.Quit();
    }


}
