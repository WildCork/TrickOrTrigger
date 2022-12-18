using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Loading;
using static Lobby;

public class Network : MonoBehaviourPunCallbacks
{
    [Header("Camera")]
    public Camera _mainCamera = null;

    [PunRPC]
    private void ChangeScene(string sceneName)
    {
        loading.ShowLoading(true, NetworkState.PlayGame);
        _lobby.gameObject.SetActive(false);
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(sceneName);
    }


    [PunRPC]
    private void DecideCellNumber(int i) 
    {
        _lobby._cellNumber = i;
    }

    [PunRPC] // _RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    public void ChatRPC(string msg)
    {
        for (int i = 0; i < _lobby._chatCells.Length; i++)
        {
            if (i < _lobby._chatCells.Length - 1)
            {
                _lobby._chatCells[i].text = _lobby._chatCells[i + 1].text;
            }
            else
            {
                _lobby._chatCells[i].text = msg;
            }
        }
    }
    [PunRPC]
    private void RenewPlayerCells(int[] cellStatus, string[] names, int[] characterKinds)
    {
        _lobby.RenewPlayerCells(cellStatus, names, characterKinds);
    }
}
