﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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
    public List<GameObject> WindowsPrefabsList;
    List<WindowData> data = new List<WindowData>(); //!< list of Windows data
    Dictionary<GameObject, WindowData> WindowsList;

    bool DesktopExist = false;
    bool PhoneExist = false;

    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this) Destroy(gameObject);
    }

    /// <summary>  
    ///  This function will save the data to a file "save.binary".
    /// </summary> 
    public void Save()
    {
        if (!Directory.Exists("Saves")) Directory.CreateDirectory("Saves");

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile = File.Create("Saves/save.binary");

        formatter.Serialize(saveFile, data);

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
        else return null;
    }

    public void LoadAndCreateWindows()
    {
        data = Load();
        for (int i = 0; i < data.Count; i++)
        {
            GameObject window = Instantiate(WindowsPrefabsList[data[i].type], new Vector3(0, 0, 0), Quaternion.identity);
            WindowsList.Add(window, data[i]);
        }

    }

    public void UpdateWindowData(GameObject Window)
    {
        WindowData cuurentWindowData = WindowsList[Window];
        cuurentWindowData.xPos = Window.transform.position.x;
        cuurentWindowData.yPos = Window.transform.position.y;
        cuurentWindowData.zPos = Window.transform.position.z;
        cuurentWindowData.xRot = Window.transform.rotation.x;
        cuurentWindowData.yRot = Window.transform.rotation.y;
        cuurentWindowData.zRot = Window.transform.rotation.z;
        cuurentWindowData.xSca = Window.transform.localScale.x;
        cuurentWindowData.ySca = Window.transform.localScale.y;
        cuurentWindowData.zSca = Window.transform.localScale.z;
    }

    public void AddWindow(int type)
    {
        if (DesktopExist && type == 0) return;
        if (PhoneExist && type == 1) return;
        if (type == 0) DesktopExist = true;
        if (type == 1) PhoneExist = true;

        GameObject Window = Instantiate(WindowsPrefabsList[type], new Vector3(0, 0, 0), Quaternion.identity);
        WindowData cuurentWindowData = new WindowData();
        cuurentWindowData.type = type;
        cuurentWindowData.xPos = Window.transform.position.x;
        cuurentWindowData.yPos = Window.transform.position.y;
        cuurentWindowData.zPos = Window.transform.position.z;
        cuurentWindowData.xRot = Window.transform.rotation.x;
        cuurentWindowData.yRot = Window.transform.rotation.y;
        cuurentWindowData.zRot = Window.transform.rotation.z;
        cuurentWindowData.xSca = Window.transform.localScale.x;
        cuurentWindowData.ySca = Window.transform.localScale.y;
        cuurentWindowData.zSca = Window.transform.localScale.z;
        WindowsList.Add(Window, cuurentWindowData);
    }

    public void RemoveWindowData(GameObject Window)
    {
        WindowData cuurentWindowData = WindowsList[Window];
        if (cuurentWindowData.type == 0) DesktopExist = false;
        if (cuurentWindowData.type == 1) PhoneExist = false;
        WindowsList.Remove(Window);
    }

}