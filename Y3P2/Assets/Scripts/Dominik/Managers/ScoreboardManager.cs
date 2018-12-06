﻿using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using System;

public class ScoreboardManager : MonoBehaviourPunCallbacks
{

    public static ScoreboardManager instance;

    public class PlayerScore
    {
        public int playerPhotonViewID;
        public string playerName;
        public int playerGamePoints;
    }
    private List<PlayerScore> playerScores = new List<PlayerScore>();

    public static event Action OnScoreboardUpdated = delegate { };

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else if (instance && instance != this)
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        PlayerManager[] players = FindObjectsOfType<PlayerManager>();
        for (int i = 0; i < players.Length; i++)
        {
            AddPlayer(players[i].photonView.ViewID, players[i].photonView.Owner.NickName);
        }

        photonView.RPC("SendGameStats", RpcTarget.Others, PlayerManager.instance.photonView.ViewID, PhotonNetwork.NickName);
    }

    public void AddPlayer(int viewID, string name)
    {
        PlayerScore playerScore = new PlayerScore { playerPhotonViewID = viewID, playerName = name };

        if (!playerScores.Contains(playerScore))
        {
            playerScore.playerGamePoints = 0;
            playerScores.Add(playerScore);
            OnScoreboardUpdated();
        }
    }

    private void RemovePlayer(string name)
    {
        for (int i = 0; i < playerScores.Count; i++)
        {
            if (playerScores[i].playerName == name)
            {
                playerScores.RemoveAt(i);
                OnScoreboardUpdated();
                return;
            }
        }
    }

    private int GetScore()
    {
        for (int i = 0; i < playerScores.Count; i++)
        {
            if (playerScores[i].playerPhotonViewID == PlayerManager.instance.photonView.ViewID)
            {
                return playerScores[i].playerGamePoints;
            }
        }

        return 0;
    }

    public List<PlayerScore> GetSortedPlayerScores()
    {
        List<PlayerScore> sorted = new List<PlayerScore>(playerScores);
        sorted = sorted.OrderBy(x => x.playerGamePoints).Reverse().ToList();

        return sorted;
    }

    public void RegisterPlayerGamePoint(int playerViewID)
    {
        for (int i = 0; i < playerScores.Count; i++)
        {
            if (playerScores[i].playerPhotonViewID == playerViewID)
            {
                playerScores[i].playerGamePoints++;
                OnScoreboardUpdated();
                return;
            }
        }
    }

    [PunRPC]
    private void SendGameStats(int callerPlayerManagerID, string callerNickName)
    {
        AddPlayer(callerPlayerManagerID, callerNickName);
        photonView.RPC("ReceiveGameStats", RpcTarget.Others, instance.photonView.ViewID, instance.GetScore());
    }

    [PunRPC]
    private void ReceiveGameStats(int viewID, int gamePoints)
    {
        for (int i = 0; i < playerScores.Count; i++)
        {
            if (playerScores[i].playerPhotonViewID == viewID)
            {
                playerScores[i].playerGamePoints += gamePoints;
                OnScoreboardUpdated();
            }
        }
    }

    public void GetScoreBoardToText(TMPro.TextMeshProUGUI text)
    {
        text.text = "";

        List<PlayerScore> latestPlayerGameStats = GetSortedPlayerScores();

        for (int i = 0; i < latestPlayerGameStats.Count; i++)
        {
            text.text += latestPlayerGameStats[i].playerName + ": <color=yellow>" + latestPlayerGameStats[i].playerGamePoints + "</color>\n";
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // TODO: Refactor, this is not safe when multiple people have the same nickname.
        RemovePlayer(otherPlayer.NickName);
    }
}