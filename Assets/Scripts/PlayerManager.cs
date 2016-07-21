﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using XInputDotNetPure;

public class PlayerManager : MonoBehaviour {

    public List<PlayerData> PlayersData = new List<PlayerData>();

    public UnityEngine.Object PlayerPrefab;
    public float MaxInactivityTime;

    public int TeamsCount;

    private Texture2D[] _textures;

    [System.Serializable]
    public class PlayerData {
        public GameObject Player;
        public int PlayerID;
        public float InactivityTimer;
        public int TeamID;
    }

    public int maxPlayerCount;

    void Start()
    {
        _textures = Resources.LoadAll<Texture2D>("Textures");
    }

    void Update() {

        if(Input.GetKeyDown("backspace")) {
            ShuffleTeams();
        }

        // We constantly listen for start inputs
        for (int i = 1; i <= maxPlayerCount; i++) {
            if (Input.GetButtonDown("Start" + i)) {
                // we attempt to find the player
                PlayerData playerData = PlayersData.Find(item => item.PlayerID == i);

                if (playerData == null) {
                    createPlayer(i);
                } else if (!playerData.Player.activeSelf) {
                    activatePlayer(playerData);
                }                
            }

            // Check horizontal input to reset 
            if (Input.GetAxis("Horizontal" + i) != 0) {
                PlayerData playerData = PlayersData.Find(item => item.PlayerID == i);
                if (playerData != null) playerData.InactivityTimer = 0;
            }
        }

        // We will update the inactivity timers
        foreach (PlayerData playerData in PlayersData) {
            playerData.InactivityTimer += Time.deltaTime;
            // If an inactivity timer hits the max inactivity deactive the player
            if (playerData.InactivityTimer >= MaxInactivityTime) {
                deactivatePlayer(playerData);
            }
        }
    }

    public void ShuffleTeams() {
        List<PlayerData> ActivePlayerList = PlayersData.FindAll(item => item.Player.activeSelf);
        PlayersData.Clear();    
        for (int i = 0; i < ActivePlayerList.Count; i++) {
            PlayerData temp = ActivePlayerList[i];
            int RandomIndex = Random.Range(0, ActivePlayerList.Count);
            ActivePlayerList[i] = ActivePlayerList[RandomIndex];
            ActivePlayerList[RandomIndex] = temp;
        }

        foreach(PlayerData data in ActivePlayerList) { 
            int playerNumber = data.PlayerID;

            //KILL. DIE.
            Destroy(data.Player);
            PlayersData.Remove(data);

            createPlayer(playerNumber);
        }
    }

    private void createPlayer(int playerID) {
        // Create a new player with this id and give it an active state
        GameObject player = (GameObject)Instantiate(PlayerPrefab, new Vector3(1, 0, 1), Quaternion.identity);
        player.GetComponent<PlayerController>().PlayerNumber = playerID;
        

        int teamID = GetSmallestTeamId();

        PlayerData playerData = new PlayerData {
            Player = player,
            PlayerID = playerID,
            TeamID = teamID
        };

        player.GetComponent<PlayerController>().TeamID = teamID;

        Texture2D texture = GetTexture(playerID, teamID);
        SkinnedMeshRenderer renderer = player.GetComponentInChildren<SkinnedMeshRenderer>();
        renderer.material.mainTexture = texture;

        // And add it to the list
        PlayersData.Add(playerData);
    }

    private void deactivatePlayer(PlayerData playerData) {
        playerData.InactivityTimer = 0;
        GameManager.instance.RumbleStop((PlayerIndex)playerData.PlayerID - 1);
        playerData.Player.SetActive(false);
    }

    private void activatePlayer(PlayerData playerData) {
        playerData.Player.SetActive(true);
    }

    //TODO Make variable on team, teamsize, etc.
    private Vector3 GeneratePosition(PlayerData data) {
        return new Vector3(1, 0, 1);
    }

    private int GetSmallestTeamId() {
        int smallestTeamSize = 100;
        int smallestTeamId = 0;

        for (int i = 0; i <= TeamsCount; i++) {
            List<PlayerData> playersData = PlayersData.FindAll(item => item.TeamID == i && item.Player.activeSelf);
            if (playersData.Count < smallestTeamSize) {
                smallestTeamId = i;
                smallestTeamSize = playersData.Count;
            }
        }

        return smallestTeamId;
    }

    private Texture2D GetTexture(int playerID, int teamID)
    {
        string teamText = teamID == 1 ? "TeamBlue" : "TeamRed";
        return _textures.FirstOrDefault(t => t.name.Contains(teamText) && t.name.Contains("Color_" + playerID));
    }
}
