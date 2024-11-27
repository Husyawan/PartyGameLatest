using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using Photon.Realtime;

public class RoomManager_KothMode : MonoBehaviourPunCallbacks
{
    public static RoomManager_KothMode Instance;
    [Header("Red Player ")] public GameObject redPlayerPrefab; 
    [Header("Blue Player ")] public GameObject bluePlayerPrefab; 
    public Transform redSpawnPoint; 
    public Transform blueSpawnPoint; 

    [Header("UI")] public GameObject roomCam;
    public GameObject nameUI;
    public GameObject connectingUI;
    public int requiredPlayerCount = 2; 
    private bool timerstarted = false;
    string name;
    private PhotonView _photonview;
    public Text countdownText;
    public Text waitText;
    private float countdownTime = 5f;
    public TMPro.TMP_InputField nicknameinput;
    int playerIndex;
    public bool isdead;
    public void ChangeNickname(string _name)
    {
        name = _name;
        PhotonNetwork.LocalPlayer.NickName = name;
    }

    void Awake()
    {
        Instance = this;
        isdead = false;
        if (!PhotonNetwork.IsConnectedAndReady)
            SceneManager.LoadScene(0);

        string roomNameToJoin = PlayerPrefs.GetString("roomNameToJoinOrCreate");

        if (roomNameToJoin == "")
            roomNameToJoin = "room" + Random.Range(0, 999);

        PhotonNetwork.JoinOrCreateRoom(roomNameToJoin, null, null);
        connectingUI.SetActive(true);
        nameUI.SetActive(false);
        _photonview = GetComponent<PhotonView>();
        countdownTime = 5;
    }
    private void Start()
    {
    }
    public void CheckNickNameenteredornot()
    {
        _photonview.RPC("CheckNicknamesAndStartTimer", RpcTarget.All);
    }
    [PunRPC]
    void CheckNicknamesAndStartTimer()
    {
        // Check if both players have entered their nicknames
        bool allPlayersHaveNicknames = true;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (string.IsNullOrEmpty(player.NickName))
            {
                allPlayersHaveNicknames = false;
                break;
            }
        }

        // If all players have nicknames, start the game timer
        if (allPlayersHaveNicknames && !timerstarted)
        {
            StartCoroutine(CountdownCoroutine());
            timerstarted = true;
        }
    }
    public void CheckAndStartCountdown()
    {
        nameUI.SetActive(false);
        PhotonNetwork.LocalPlayer.NickName = nicknameinput.text;
        if (PhotonNetwork.CurrentRoom.PlayerCount == requiredPlayerCount)
        {
            waitText.text = "Please wait other player entered their name";
            waitText.gameObject.SetActive(true);
            CheckNickNameenteredornot();
        }
        else
        {
            waitText.text = "wait for 2nd player to join the room";
            waitText.gameObject.SetActive(true);
        }    
   

    }
    private IEnumerator CountdownCoroutine()
    {
        waitText.gameObject.SetActive(false);
        float remainingTime = countdownTime;
        countdownText.gameObject.SetActive(true);
        while (remainingTime > 0)
        {
            countdownText.text = remainingTime.ToString("F0");
            yield return new WaitForSeconds(1f);
            remainingTime--;
        }

        countdownText.text = "Start!";
        yield return new WaitForSeconds(1f);
        countdownText.text = "";
        countdownText.gameObject.SetActive(false);
        SpawnPlayer();
    }
    public void SpawnPlayer()
    {
        roomCam.SetActive(false);
        CapPointManager capPointManager = FindObjectOfType<CapPointManager>();
        playerIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["PlayerIndex"];
        // Spawn Red player (first player to join)
        if (playerIndex == 1)
            {
                GameObject _player=  PhotonNetwork.Instantiate(redPlayerPrefab.name, redSpawnPoint.position, redSpawnPoint.rotation);
               _player.GetComponentInChildren<PlayerCappedPoint>().TeamName = "Red";
               capPointManager.SpawnedGORedTeam.Add(_player);
                PhotonView photonView = _player.GetComponent<PhotonView>();
                if (photonView.IsMine)
                {
                    _player.GetComponent<PlayerSetup>().IsLocalPlayer();
                    _player.GetComponent<PhotonView>().RPC("SetNickname", RpcTarget.AllBuffered, name);
                    _player.GetComponent<PlayerHealth_Koth>().isLocalInstance = true;
                  
                }
            }
            // Spawn Blue player (second player to join)
            else if (playerIndex == 2)
            {
                GameObject _player= PhotonNetwork.Instantiate(bluePlayerPrefab.name, blueSpawnPoint.position, blueSpawnPoint.rotation);
                _player.GetComponentInChildren<PlayerCappedPoint>().TeamName ="Blue";
                capPointManager.SpawnedGOBlueTeam.Add(_player);
                PhotonView photonView = _player.GetComponent<PhotonView>();
                if (photonView.IsMine)
                {
                    _player.GetComponent<PlayerSetup>().IsLocalPlayer();
                    _player.GetComponent<PhotonView>().RPC("SetNickname", RpcTarget.AllBuffered, name);
                    _player.GetComponent<PlayerHealth_Koth>().isLocalInstance = true;
                 
            }
            }
        if (PhotonNetwork.IsMasterClient)
        {
            StartGame();
        }
    }
    public void Respawn_Red()
    {
        GameObject _player = PhotonNetwork.Instantiate(redPlayerPrefab.name, redSpawnPoint.position, redSpawnPoint.rotation);
        PhotonView photonView = _player.GetComponent<PhotonView>();
        if (photonView.IsMine)
        {
            _player.GetComponent<PlayerSetup>().IsLocalPlayer();
            _player.GetComponent<PhotonView>().RPC("SetNickname", RpcTarget.AllBufferedViaServer, name);
            _player.GetComponent<PlayerHealth_Koth>().isLocalInstance = true;
        }
    }
    public void Respawn_Blue()
    {
        GameObject _player = PhotonNetwork.Instantiate(bluePlayerPrefab.name, blueSpawnPoint.position, blueSpawnPoint.rotation);
        PhotonView photonView = _player.GetComponent<PhotonView>();
        if (photonView.IsMine)
        {
            _player.GetComponent<PlayerSetup>().IsLocalPlayer();
            _player.GetComponent<PhotonView>().RPC("SetNickname", RpcTarget.AllBufferedViaServer, name);
            _player.GetComponent<PlayerHealth_Koth>().isLocalInstance = true;
        }
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        nameUI.SetActive(true);
        connectingUI.SetActive(false);


        playerIndex = PhotonNetwork.CurrentRoom.PlayerCount;
        Debug.Log("Player index assigned: " + playerIndex);

        // Optionally, set this value on the player's custom properties so others can see it
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable { { "PlayerIndex", playerIndex } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }
    private void StartGame()
    {
        _photonview.RPC("BeginKOTHMode", RpcTarget.All);
    }
    [PunRPC]
    private void BeginKOTHMode()
    {
        CapPointManager capPointManager = FindObjectOfType<CapPointManager>();
        if (capPointManager != null)
        {
            Debug.Log("Working Starting Koth");
            capPointManager.defaultvaluetext();
            capPointManager.StartKOTH();
        }
    }
    public void BacktoMainDead()
    {
        //isdead = true;
        PhotonNetwork.LeaveRoom();
    }
    public void BacktoMain()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        CapPointManager capPointManager = FindObjectOfType<CapPointManager>();
        if (capPointManager != null&& !isdead)
        {
            capPointManager.GameCompletedtxt.text = "Other Player Leave the Game";
            capPointManager.GameCompleted.SetActive(true);
            Time.timeScale = 1;
        }
        //else if(capPointManager != null && isdead)
        //{
        //    capPointManager.GameCompletedtxt.text = "Other Player Died You Won";
        //    capPointManager.GameCompleted.SetActive(true);
        //    Time.timeScale = 1;
        //}
    }
}
