using UnityEngine;
using System;

using UnityEngine.UI;

/// <summary>
/// A Unity3D Script to dipsplay Mjpeg streams. Apply this script to the mesh that you want to use to view the Mjpeg stream. 
/// </summary>
public class MjpegTexture : MonoBehaviour
{
    /// <param name="streamAddress">
    /// Set this to be the network address of the mjpg stream. 
    /// Example: "http://extcam-16.se.axis.com/mjpg/video.mjpg"
    /// </param>
    [Tooltip("Set this to be the network address of the mjpg stream. ")]
    public string streamAddress;



    /// <summary>
    /// Chunk size for stream processor in kilobytes.
    /// </summary>
    [Tooltip("Chunk size for stream processor in kilobytes.")]
    public int chunkSize = 4;

    public Material material;
    public GameObject ScreenPortrait;
    public GameObject ScreenLandscape;

    Texture2D tex;

    const int initWidth = 2;
    const int initHeight = 2;

    bool updateFrame = false;

    MjpegProcessor mjpeg;

    float deltaTime = 0.0f;
    float mjpegDeltaTime = 0.0f;

    public InputField UrlField;

    public void Start()
    {
        mjpeg = new MjpegProcessor(chunkSize * 1024);
        mjpeg.FrameReady += OnMjpegFrameReady;
        mjpeg.Error += OnMjpegError;
        Uri mjpegAddress = new Uri(streamAddress);
        mjpeg.ParseStream(mjpegAddress);
        // Create a 16x16 texture with PVRTC RGBA4 format
        // and will it with raw PVRTC bytes.   PVRTC_RGBA4
        tex = new Texture2D(initWidth, initHeight, TextureFormat.ARGB32, false);

        UrlField.text = streamAddress;
    }
    private void OnMjpegFrameReady(object sender, FrameReadyEventArgs e)
    {
        updateFrame = true;
    }
    void OnMjpegError(object sender, ErrorEventArgs e)
    {
        Debug.Log("Error received while reading the MJPEG.");
    }
    
    // Update is called once per frame
    void Update()
    {
        deltaTime += Time.deltaTime;
        
        if (updateFrame)
        {
            tex.LoadImage(mjpeg.CurrentFrame);
            // Assign texture to renderer's material.
            material.mainTexture = tex;
            if (tex.width < tex.height)
            {
                ScreenPortrait.SetActive(true);
                ScreenLandscape.SetActive(false);
            }
            else
            {
                ScreenPortrait.SetActive(false);
                ScreenLandscape.SetActive(true);
            }
            updateFrame = false;

            mjpegDeltaTime += (deltaTime - mjpegDeltaTime) * 0.2f;

            deltaTime = 0.0f;
        }
    }

    public void changeUrl()
    {
        Debug.Log("changeUrl");
        streamAddress = UrlField.text;
        Uri mjpegAddress = new Uri(streamAddress);
        mjpeg.ParseStream(mjpegAddress);
    }


    void OnDestroy()
    {
        mjpeg.StopStream();
    }

}