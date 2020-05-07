﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

public static class SaveSystem
{
    public const int MAX_SAVE_SLOTS = 3;
    [Range(1,MAX_SAVE_SLOTS)]
    public static int currentSaveSlot = 1;
    public static bool existingSaveData = false;
    public static bool loading = false;
    public static Data.GameData gameData;
    public static Data.SaveData saveData;

    public delegate void SaveSlotUpdate(int saveSlot, string date, float playTime);
    public static event SaveSlotUpdate OnUpdatedSaveStats;

    public delegate void SaveSlotDelete(int saveSlot);
    public static event SaveSlotDelete OnDeleteSave;

    private static string saveDataPath = Path.Combine(Application.persistentDataPath, "_GameData");
    private static string fileExtension = ".david";

    private static BinaryFormatter formatter;

    public static string GetSavePath(int saveSlot)
    {
        return Path.Combine(saveDataPath, string.Format("save{0}", saveSlot) + fileExtension);
    }

    public static void Save(int saveSlot)
    {
        loading = true;
        formatter = new BinaryFormatter();
        Data.GameData newGameData;
        //Game Save
        using (var stream = new FileStream(Path.Combine(saveDataPath, string.Format("save{0}", saveSlot) + fileExtension), FileMode.Create))
        {
            Player player = (Player) GameObject.FindObjectOfType(typeof(Player));
            Player[] players = (Player[]) GameObject.FindObjectsOfType(typeof(Player));
            if(players.Length > 1)
            {
                foreach(Player x in players)
                {
                    if (!x.name.Contains("Player"))
                        player = x;
                }
            }
            else
            {
                foreach (Player x in players)
                {
                    if (x.name.Contains("Player"))
                        player = x;
                }

            }


            newGameData = new Data.GameData(player, (MainMenu)GameObject.FindObjectOfType(typeof(MainMenu)), stream);
            formatter.Serialize(stream, newGameData);
        }

        //Save SaveInfo
        using (var stream = new FileStream(Path.Combine(saveDataPath, "saveInfo" + fileExtension), FileMode.Create))
        {
            Data.SaveData saveData = new Data.SaveData();
            formatter.Serialize(stream, saveData);
        }


        //Update Save Slot to reflect save info
        OnUpdatedSaveStats?.Invoke(saveSlot, newGameData.gameStats.date, newGameData.gameStats.playTime);
        //GameController.UpdateSaveSlotInfo(saveSlot, gameData.gameStats.date, gameData.gameStats.playTime);

        loading = false;
        Log.AddEntry("Save Completed");
    }

    public static void Load(int saveSlot)
    {
        loading = true;
        Debug.Log("Starting to load");
        string path;

        //Create Save Directory if non-existant
        if (!Directory.Exists(Path.Combine(saveDataPath, "_GameData")))
        {
            Directory.CreateDirectory(Path.Combine(saveDataPath, "_GameData"));
        }

        //Create universal saveInfo file for game-state independent save stuff
        if (File.Exists(path = Path.Combine(saveDataPath, "saveInfo" + fileExtension)))
        {
            Data.SaveData data;
            using (var stream = new FileStream(path, FileMode.Open))
            {
                if (stream.Length == 0)
                {
                    loading = false;
                    return;
                }
                formatter = new BinaryFormatter();
                data = formatter.Deserialize(stream) as Data.SaveData;
                //Load General Save information
                SaveSystem.existingSaveData = data.existingSave;
                SaveSystem.currentSaveSlot = data.mostRecentSaveSlot;
            }
        }
        else
        {
            File.Create(Path.Combine(saveDataPath, "saveInfo" + fileExtension)).Dispose();
            loading = false;
            return;
        }

        if (File.Exists(path = Path.Combine(saveDataPath, string.Format("save{0}", saveSlot) + fileExtension)))
        {
            Debug.Log("STARTING LOAD");
            //Decode Game Data
            using (var stream = new FileStream(path, FileMode.Open))
            {
                if (stream.Length == 0)
                {
                    loading = false;
                    return;
                }
                formatter = new BinaryFormatter();
                gameData = formatter.Deserialize(stream) as Data.GameData;
            }

            //if (GameController.initialLoad == true)
            //{
            currentSaveSlot = saveSlot;
            SceneManager.LoadScene(0, LoadSceneMode.Single);
            MainMenu.TriggerMainMenu();
            //}

            //Transform gameController = GameObject.Find("Game Management").transform.Find("GameController");
            //Debug.Log("GameController = " + gameController.name);

            ////GAME STATS
            //GameController._instance = gameController.GetComponent<GameController>();
            //GameController._instance.playTime = data.gameStats.playTime;
            //if (GameController._instance.paused)
            //    GameController.TogglePause();

            ////MAIN MENU
            //MainMenu._instance = gameController.GetComponent<MainMenu>();
            //MainMenu._instance.Load(data.mainMenuData);

            ////PLAYER
            //GameController.soul.GetComponent<Player>().TriggerLoad(data.playerData);

            ////PHOTO LIBRARY
            //PhotoLibrary._instance = gameController.GetComponent<PhotoLibrary>();
            //PhotoLibrary.TriggerLoad(data.libraryData);

            ////DIALOGUE
            //Dialogue.instance = gameController.GetComponent<Dialogue>();
            //Dialogue.ForceStop();

            ////TUTORIAL
            //Tutorial.instance = gameController.GetComponent<Tutorial>();
            //Tutorial.Load(data.tutorialData);

            ////RAT TRAPS
            //for(int i = 0; i < GameController._instance.ratTraps.Length; i++)
            //{
            //    GameController._instance.ratTraps[i].Load(data.trapData);
            //}

            ////DOORS
            //for(int i = 0; i < GameController._instance.doors.Length; i++)
            //{
            //    GameController._instance.doors[i].Load(data.doorData);
            //}

            //START SEQUENCE
            
        }
        else
        {
            gameData = null;
            
            Debug.Log("Save Data for Slot " + saveSlot + " Not Found.");
        }


        //Load Game Stats for Save Slots
        for (int i = 1; i <= MAX_SAVE_SLOTS; i++)
        {
            int temp = i;
            if (File.Exists(path = Path.Combine(saveDataPath, string.Format("save{0}", temp) + fileExtension)))
            {
                formatter = new BinaryFormatter();
                Data.GameData data;
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    data = formatter.Deserialize(stream) as Data.GameData;
                    OnUpdatedSaveStats?.Invoke(temp, data.gameStats.date, data.gameStats.playTime);
                    //GameController.UpdateSaveSlotInfo(temp, data.gameStats.date, data.gameStats.playTime);

                }

            }
        }

        loading = false;
    }

    public static void DeleteSave(int saveSlot)
    {
        string path;
        if (File.Exists(path = Path.Combine(saveDataPath, string.Format("save{0}", saveSlot) + fileExtension)))
        {
            formatter = new BinaryFormatter();
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            {
                Data.PhotoLibraryData data = (formatter.Deserialize(stream) as Data.GameData).libraryData;
                foreach(string imgPath in data.photoImgPaths)
                {
                    File.Delete(imgPath);
                }
            }

            File.Delete(path);
            OnDeleteSave?.Invoke(saveSlot);

            //if(saveSlot == currentSaveSlot)
            //{
            //    SaveSystem.Load(saveSlot);
                
            //}
        }
        else
        {
            Debug.Log("Can't Delete Save Slot, No Save Data for Slot " + saveSlot + " Found.");
        }
    }

    public static void NewGame(int saveSlot)
    {
        currentSaveSlot = saveSlot;
        if(SaveExists(saveSlot))
            DeleteSave(currentSaveSlot);
        //Load(saveSlot);
        SceneManager.LoadScene(0, LoadSceneMode.Single);
        //Save(saveSlot);
        //MainMenu.TriggerMainMenu();
    }

    public static bool SaveExists(int saveSlot)
    {
        return File.Exists(Path.Combine(saveDataPath, string.Format("save{0}", saveSlot) + fileExtension));
    }

    public static bool AnySaveExists()
    {
        for(int i = 1; i <= MAX_SAVE_SLOTS; i++)
        {
            int temp = i;
            if(SaveExists(temp))
            {
                return true;
            }
        }

        return false;
    }
}
