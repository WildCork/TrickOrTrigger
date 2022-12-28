using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Loading;

public class UISystem : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public Login _login = null;
    public Lobby _lobby = null;
    public Room _room = null;

    public Network _network = null;

    public enum GameMode { TeamMatch, DeathMatch};
    public enum MapType {Castle, City };


    //CreateRoom Ŭ�� �� �� ���� â ���� -> ������ ���ϰ� ���� �ϱ�

    //TODO: 
    //UIBase ��ũ��Ʈ ���� �κ�, ��, �α��� ��ӽ�Ű��
    //���� Lobby ��ũ��Ʈ -> UISystem���� ���� ����
    //������: 
    // UISystem
    //      Login   (UIBase)
    //      Lobby   (UIBase)
    //      Room    (UIBase)
    //UIBase�� loading, network, ���� ���� �г���, networkStatus ���� ����


    private void Awake()
    {
        Screen.SetResolution(1920, 1080, false);
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
        Debug.Log("OnRoomListUpdate");
        _lobby.UpdateRoomList(roomList);
        _lobby.RenewMyList();
    }

    #region ��������

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

    #region ��

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
        _room.gameObject.SetActive(true);
        _login.gameObject.gameObject.SetActive(false);
        _lobby.gameObject.gameObject.SetActive(false);
        _room.RenewChat();
        if (PhotonNetwork.IsMasterClient)
        {
            _room.InitRoomWhenCreate();
            _network.Chat_RPC(RpcTarget.AllBufferedViaServer, "<color=yellow>Player " 
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
        Debug.Log("OnCreateRoomFailed");
        _lobby._roomNameInput.text = "";
        _lobby.CreateRoom();
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed");
        _lobby._roomNameInput.text = ""; 
        _lobby.CreateRoom(); 
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("OnPlayerEnteredRoom");
        _network.Chat_RPC(RpcTarget.AllBufferedViaServer, "<color=yellow> Player " + newPlayer.NickName + " enters this room.</color> (OnPlayerEnteredRoom)");
        _network.DecideCellNumber_RPC(newPlayer, _room.AddCell(newPlayer.NickName));
        _room.RenewPlayerCells_RPC();
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("OnPlayerLeftRoom");
        _room.RemoveCell(otherPlayer.NickName);
        _room.RenewPlayerCells_RPC();
        _network.Chat_RPC(RpcTarget.AllBufferedViaServer, "<color=yellow> Player " + otherPlayer.NickName + " exits this room.</color> (OnPlayerLeftRoom)");
    }

    #endregion


}
