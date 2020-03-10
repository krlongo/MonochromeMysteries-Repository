﻿/* Name: GameController.cs
 * Author: Zackary Seiple
 * Description: Handles the basic functions of the game including pulling up menus and pausing
 * Last Updated: 2/18/2020 (Zackary Seiple)
 * Changes: Added Header
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController _instance;
    public static Canvas mainHUD;
    public static Camera cam;
    public static GameObject soul;
    //Says whether ANY menu is active
    public static bool menuActive = false;

    public bool paused = false;

    private AudioSource[] audioSources;

    // Start is called before the first frame update
    void Start()
    {
        _instance = this;
        mainHUD = GameObject.Find("HUD").GetComponent<Canvas>();
        cam = Camera.main;
        soul = GameObject.Find("Player");

        //Initialize Game
        StartCoroutine(InitializeGame());

        StartCoroutine(LogTest());
        audioSources = this.GetComponents<AudioSource>();
    }

    private IEnumerator LogTest()
    {
        yield return new WaitForSeconds(1);
        Dialogue.AddLine(Dialogue.Character.Pete, "You! You're finally awake! It's me, your close friend, Johnathan Lennon.", "Yeet yeet skeet lmao");
    }

    // Update is called once per frame
    /// <summary>
    /// Checks each frame for inputs to open the photograph collection menu and to exit the game
    /// </summary>
    void Update()
    {
        //Photo Library Menu
        if(Input.GetKeyDown(KeyCode.Tab) && (!menuActive || PhotoLibrary._instance.photoCollectionMenu.gameObject.activeSelf == true))
        {
            TogglePause();
            PhotoLibrary._instance.photoCollectionMenu.gameObject.SetActive(paused);
            if (PhotoLibrary._instance.GetPhotoCount() > 3)
            {
                Transform grid = PhotoLibrary._instance.photoCollectionMenu.transform.GetChild(0);
                Vector3 topPos = grid.position;
                topPos.y = 225;
                grid.position = topPos;

            }
            //sounds for book
            if(!menuActive)
            {
                //open
                audioSources[0].Play();
            }
            else
            {
                //close
                audioSources[1].Play();
            }
            menuActive = paused;
        }

        //Enter/Exit Menu
        if(Input.GetKeyDown(KeyCode.F))
        {
            TriggerMainMenu();
        }
        //Just Save
        if(Input.GetKeyDown(KeyCode.X) && MainMenu.IsInRange())
        {
            //Save code
        }
    }

    public void TriggerMainMenu()
    {
        if (!MainMenu._instance.tvTransitionInProgress && MainMenu.IsInRange() && (!menuActive || MainMenu.active))
            MainMenu._instance.StartCoroutine(MainMenu._instance.TriggerTV());
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Toggles the Game Pausing (Not the menu)
    /// </summary>
    public static void TogglePause()
    {
        _instance.paused = !_instance.paused;

        if (_instance.paused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Player.EnableControls(false);
            Time.timeScale = 0;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Player.EnableControls(true);
            Time.timeScale = 1;
        }
    }

    private IEnumerator InitializeGame()
    {
        while(MainMenu._instance.TVs.Length == 0)
        {
            yield return null;
        }

        Television startTV = MainMenu._instance.TVs[0];
        MainMenu._instance.SetCurrentTV(startTV);

        Vector3 target = transform.forward;
        target.z += soul.GetComponent<MeshRenderer>().bounds.size.z;
        soul.transform.position = startTV.transform.TransformPoint(target);
        soul.transform.rotation = startTV.transform.Find("CamPoint").rotation;

        yield return new WaitForFixedUpdate();

        MainMenu._instance.UpdateTVRanges();
        TriggerMainMenu();
    }

}
