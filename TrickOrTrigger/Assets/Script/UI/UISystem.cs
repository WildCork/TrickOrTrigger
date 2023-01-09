using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Loading;

public class UISystem : MonoBehaviourPunCallbacks
{
    public static UISystem uiSystem = null;

    [Header("Camera")]
    public Camera _mainCamera = null;

    [Header("UI")]
    public Login _login = null;
    public Lobby _lobby = null;
    public Room _room = null;

    [Header("User")]
    public int _cellNumber;
    //public Network _network = null;

    public enum GameMode { TeamMatch, DeathMatch};
    public enum MapType {Castle, City };


    //UI 갈아엎기 (브롤스타즈)


    private void Awake()
    {
        Screen.SetResolution(1920, 1080, false);
        if (uiSystem)
        {
            Destroy(gameObject);
            return;
        }
        uiSystem = this;
        Init();       
    }

    private void Init()
    {
        _login.gameObject.SetActive(!PhotonNetwork.IsConnected);
        _lobby.gameObject.SetActive(PhotonNetwork.InLobby);
        _room.gameObject.SetActive(PhotonNetwork.InRoom);

        _login.Init();
        _room.Init();
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //Debug.Log("OnRoomListUpdate");
        _lobby.UpdateRoomList(roomList);
        _lobby.RenewMyList();
    }

    #region 서버연결

    string _status = "";
    void Update()
    {
        if (_status != PhotonNetwork.NetworkClientState.ToString())
        {
            loading.RenewValue(_status = PhotonNetwork.NetworkClientState.ToString());
        }
        _lobby._lobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + " _lobby / " + PhotonNetwork.CountOfPlayers + " _connect";
    }


    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        _lobby.gameObject.SetActive(true);
        _login.gameObject.SetActive(false);
        _room.gameObject.SetActive(false);
        loading.ShowLoading(false);
        PhotonNetwork.LocalPlayer.NickName = _login._nickNameInput.text;
        _lobby.OnJoinedLobby(PhotonNetwork.LocalPlayer.NickName);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        loading.ShowLoading(false);
        _login.gameObject.SetActive(true);
        _lobby.gameObject.SetActive(false);
        _room.gameObject.SetActive(false);
    }
    #endregion

    #region 방

    public override void OnJoinedRoom()
    {
        //Debug.Log("OnJoinedRoom");
        _room.gameObject.SetActive(true);
        _login.gameObject.gameObject.SetActive(false);
        _lobby.gameObject.gameObject.SetActive(false);
        _room.RenewChat();
        if (PhotonNetwork.IsMasterClient)
        {
            _room.InitRoomWhenCreate();
            Chat_RPC(RpcTarget.AllBufferedViaServer, "<color=yellow>Player " 
                + PhotonNetwork.LocalPlayer.NickName + " create this room.</color> (InitRoomWhenCreate)");
            _room.RenewPlayerCells_RPC();
        }

        StartCoroutine(WaitCellNumber());
    }

    IEnumerator WaitCellNumber()
    {
        while (true)
        {
            if (_room._cellNumber >= 0)
            {
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.2f);
        loading.ShowLoading(false);
        yield return null;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        //Debug.Log("OnCreateRoomFailed");
        _lobby._roomNameInput.text = "";
        _lobby.CreateRoom();
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //Debug.Log("OnJoinRandomFailed");
        _lobby._roomNameInput.text = ""; 
        _lobby.CreateRoom(); 
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //Debug.Log("OnPlayerEnteredRoom");
        Chat_RPC(RpcTarget.AllBufferedViaServer, "<color=yellow> Player " + newPlayer.NickName + " enters this room.</color> (OnPlayerEnteredRoom)");
        DecideCellNumber_RPC(newPlayer, _room.AddCell(newPlayer.NickName));
        _room.RenewPlayerCells_RPC();
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //Debug.Log("OnPlayerLeftRoom");
        _room.RemoveCell(otherPlayer.NickName);
        _room.RenewPlayerCells_RPC();
        Chat_RPC(RpcTarget.AllBufferedViaServer, "<color=yellow> Player " + otherPlayer.NickName + " exits this room.</color> (OnPlayerLeftRoom)");
    }

    #endregion

    public void Chat_RPC(RpcTarget rpcTarget, string msg)
    {
        photonView.RPC(nameof(Chat), rpcTarget, msg);
    }

    [PunRPC] // _RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    public void Chat(string msg)
    {
        for (int i = 0; i < _room._chatCells.Length; i++)
        {
            if (i < _room._chatCells.Length - 1)
            {
                _room._chatCells[i].text = _room._chatCells[i + 1].text;
            }
            else
            {
                _room._chatCells[i].text = msg;
            }
        }
    }

    public void DecideCellNumber_RPC(Player player, int i)
    {
        photonView.RPC(nameof(DecideCellNumber), player, i);
    }

    [PunRPC]
    protected void DecideCellNumber(int i)
    {
        _room._cellNumber = _cellNumber = i;
    }

    public void RenewPlayerCells_RPC(RpcTarget rpcTarget, int[] cellStatus, string[] names, int[] characterKinds)
    {
        photonView.RPC(nameof(RenewPlayerCellsss), rpcTarget, cellStatus, names, characterKinds);
    }

    [PunRPC]
    public void RenewPlayerCellsss(int[] cellStatus, string[] names, int[] characterKinds)
    {
        _room.RenewRoom(cellStatus, names, characterKinds);
    }

    public void ChangeScene_RPC(RpcTarget rpcTarget, string sceneName)
    {
        photonView.RPC(nameof(ChangeScene), rpcTarget, sceneName);
    }

    [PunRPC]
    public void ChangeScene(string sceneName)
    {
        loading.ShowLoading(true, NetworkState.PlayGame);
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(sceneName);
    }
}
