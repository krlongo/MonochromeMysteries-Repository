﻿/* Name: Photographer.cs
 * Author: Zackary Seiple
 * Description: Contains the behaviour and ability of the Photographer Character. Handles taking pictures and sending them to
 *              the PhotoLibrary to be cropped and stored
 * Last Updated: 2/18/2020 (Zackary Seiple)
 * Changes: Added header
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Photographer : Person
{
    
    Camera cam;
    GameObject hud;
    Rect newRect;

    private bool cameraLensActive = false; //NEVER EDIT THIS
    public bool CameraLensActive //Controls the camera HUD popping up: Edit this
    {
        get
        {
            return cameraLensActive;
        }
        set
        {
            //Activate Camera HUD Based on this Value
            if(value)
            {
                hud.SetActive(true);
            }
            else
            {
                hud.SetActive(false);
            }
            cameraLensActive = value;
        }
    }

    bool screenshotQueued = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        cam = Camera.main;
        hud = GameObject.Find("HUD").transform.Find("Camera").gameObject;

        Debug.Log(newRect.xMin + " " + newRect.xMax + " | " + newRect.yMin + " " + newRect.yMax);

        OnPossession += ToggleHUD;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    //Camera
    bool canTakePhoto = true;
    public override void Ability()
    {
        if (!GameController._instance.paused && canTakePhoto)
        {
            Player.EnableControls(false);
            StartCoroutine(CameraFlash());
            TakePhoto(Screen.width, Screen.height);
            Player.EnableControls(true);
        }
    }

    /// <summary>
    /// Pause camera texture for a frame and queue it up to save screen capture
    /// </summary>
    /// <param name="width">The width in pixels of the screen capture</param>
    /// <param name="height">The height in pixels of the screen capture</param>
    public void TakePhoto(int width, int height)
    {
        //Pause the camera texture and process the photo in the next OnRenderObject() call
        cam.targetTexture = RenderTexture.GetTemporary(width, height, 16);
        screenshotQueued = true;
    }

    float flashTime = 1f;
    /// <summary>
    /// Coroutine that makes the screen flash white and fade out when taking a picture
    /// </summary>
    /// <returns></returns>
    private IEnumerator CameraFlash()
    {
        canTakePhoto = false;

        float currentTime = 0;
        Image hudBackground = hud.GetComponent<Image>();
        Color baseColor = hudBackground.color;
        Color flashColor = Color.white;

        hudBackground.color = flashColor;
        yield return new WaitForSeconds(0.5f);

        while(hudBackground.color != baseColor)
        {
            hudBackground.color = Color.Lerp(flashColor, baseColor, currentTime / flashTime);
            currentTime += Time.deltaTime;
            yield return null;
        }

        canTakePhoto = true;
    }

    /// <summary>
    /// Activates and deactivates the Camera Lens HUD
    /// </summary>
    /// <param name="possessionActive">Turns camera lens HUD on if true, off if false</param>
    private void ToggleHUD(bool possessionActive)
    {
        CameraLensActive = possessionActive;
    }


    private void OnRenderObject()
    {
        if (screenshotQueued)
        {
            //Take Picture
            screenshotQueued = false;
            RenderTexture renderTexture = cam.targetTexture;
            Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            renderResult.ReadPixels(rect, 0, 0);

            //Release camera to retrun to normal
            RenderTexture.ReleaseTemporary(renderTexture);
            cam.targetTexture = null;


            //Store Picture In Library
            float width = hud.transform.Find("Lens").GetComponent<RectTransform>().sizeDelta.x;
            float height = hud.transform.Find("Lens").GetComponent<RectTransform>().sizeDelta.y;
            Debug.Log(width + " " + height);
            PhotoLibrary.StorePhoto(renderResult, 
                                    ClueCatalogue._instance.DetectCluesOnScreen(Screen.width / 2 -  width / 2, 
                                                                                Screen.height / 2 - height / 2,
                                                                                width,
                                                                                height));
        }
    }

    private void OnDisable()
    {
        OnPossession -= ToggleHUD;
    }

}
