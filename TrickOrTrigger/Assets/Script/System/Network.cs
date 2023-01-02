using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using static Room;
using static Loading;

public class Network : MonoBehaviourPunCallbacks
{
    [Header("Camera")]
    public Camera _mainCamera = null;

    [Header("User")]
    public int _cellNumber;

    [Header("Photon")]
    public static Network _network = null;
    public PhotonView _photonView = null;

    [Header("UI")]
    public Room _room = null;

    private void Awake()
    {
        if (_network)
        {
            Destroy(gameObject);
        }   
        else
        {
            _network = this;
        }
    }

    public void ChangeScene_RPC(RpcTarget rpcTarget, string sceneName)
    {
        _photonView.RPC(nameof(ChangeScene), rpcTarget, sceneName);
    }

    public void DecideCellNumber_RPC(Player player,  int i)
    {
        _photonView.RPC(nameof(DecideCellNumber), player, i);
    }

    public void Chat_RPC(RpcTarget rpcTarget, string msg)
    {
        _photonView.RPC(nameof(Chat), rpcTarget, msg);
    }

    public void RenewPlayerCells_RPC(RpcTarget rpcTarget, int[] cellStatus, string[] names, int[] characterKinds)
    {
        _photonView.RPC(nameof(RenewPlayerCells), rpcTarget, cellStatus, names, characterKinds);
    }

    [PunRPC]
    protected void DecideCellNumber(int i)
    {
        _room._cellNumber = _cellNumber = i;
    }

    [PunRPC]
    public void ChangeScene(string sceneName)
    {
        loading.ShowLoading(true, NetworkState.PlayGame);
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(sceneName);
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

    [PunRPC]
    public void RenewPlayerCells(int[] cellStatus, string[] names, int[] characterKinds)
    {
        _room.RenewRoom(cellStatus, names, characterKinds);
    }
}
