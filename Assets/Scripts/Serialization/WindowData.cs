using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


/// <summary>  
/// The NodeData class is a serializable object that
/// can hold data in a specified and serializable form.
/// This way, data that defines the Min3dmap can be
/// saved to a file and be converted back into a Min3dmap.
/// </summary> 
[Serializable]
public class WindowData{
    public int id = 0; //!< [0..n]. Unique Id for a node, 0 is the root node
    public int type = 0; //!< [0..n]. Unique Id for a node, 0 is the root node
    public float xPos; //!< xPos, yPos and zPos hold the Positions
    public float yPos;
    public float zPos;
    public float xRot; //!< xRot, yRot and zRot hold the Rotation
    public float yRot;
    public float zRot;
    public float xSca; //!< xSca, ySca and zSca hold the Scale
    public float ySca;
    public float zSca;
}
