using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.UI;
using static PlayerCell;
using static DontDestroyData;

public class Room : MonoBehaviour
{
    public Network _network = null;

    public PlayerCell[] _playerCells;
    public Text _listText;
    public Text _roomInfoText;
    public InputField _chatInput;
    public Button _readyOrPlayBtn;

    public static string _chatCellsName = "ChatScrollView";
    private Text _readyOrPlayStatusName;
    [HideInInspector] public Text[] _chatCells;
    public int _cellNumber = -1;
    public int _roomMaxPlayerCnt = 8;


    private void Init()
    {
        _chatCells = transform.Find(_chatCellsName).GetComponentsInChildren<Text>();

        for (int i = 0; i < _roomMaxPlayerCnt; i++)
        {
            _playerCells[i].Init();
        }

        _readyOrPlayStatusName = _readyOrPlayBtn.GetComponentInChildren<Text>();
    }


    public void ClickPlayerCell(int i)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _playerCells[i].Click();
            RenewPlayerCells_RPC();
        }
    }


    public void ChangeCharacter(int characterKind)
    {
        switch (_playerCells[_cellNumber]._status)
        {
            case CellStatus.Empty:
                break;
            case CellStatus.Fill:
                if (_playerCells[_cellNumber]._characterKind != (CharacterKind)characterKind)
                {
                    _playerCells[_cellNumber].FillCell("",
                        _dontDestroyData._characterKind = (CharacterKind)characterKind);
                    RenewPlayerCells_RPC(RpcTarget.Others);
                }
                break;
            case CellStatus.Closed:
                break;
            case CellStatus.Ready:
                _network.Chat("If you wanna change, you should be not ready!");
                break;
            default:
                break;
        }
    }

    private void RenewPlayerCells_RPC(RpcTarget rpcTarget = RpcTarget.All)
    {
        CellStatus[] status = new CellStatus[_playerCells.Length];
        string[] nicknames = new string[_playerCells.Length];
        CharacterKind[] characterKinds = new CharacterKind[_playerCells.Length];
        for (int i = 0; i < _playerCells.Length; i++)
        {
            status[i] = _playerCells[i]._status;
            nicknames[i] = _playerCells[i]._nickname.text;
            characterKinds[i] = _playerCells[i]._characterKind;
        }
        _network.RenewPlayerCells_RPC(rpcTarget, Array.ConvertAll(status, value => (int)value),
            Array.ConvertAll(nicknames, value => value), Array.ConvertAll(characterKinds, value => (int)value));
    }


    public void RenewPlayerCells(int[] status, string[] nicknames, int[] characterKinds)
    {
        _listText.text = "";

        if (PhotonNetwork.IsMasterClient)
        {
            _readyOrPlayStatusName.text = "Start!!";
        }
        else
        {
            _readyOrPlayStatusName.text = "Ready??";
        }
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            _listText.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        }
        for (int i = 0; i < status.Length; i++)
        {
            switch (status[i])
            {
                case (int)CellStatus.Empty:
                    _playerCells[i].OpenCell();
                    break;
                case (int)CellStatus.Fill:
                    _playerCells[i].FillCell(nicknames[i], (CharacterKind)characterKinds[i]);
                    break;
                case (int)CellStatus.Closed:
                    _playerCells[i].CloseCell();
                    break;
                case (int)CellStatus.Ready:
                    _playerCells[i].ReadyCell();
                    break;
                default:
                    break;
            }
        }
        int cnt = 0;
        for (int i = 0; i < _playerCells.Length; i++)
        {
            if (_playerCells[i]._status != CellStatus.Closed)
                cnt++;
        }
        PhotonNetwork.CurrentRoom.MaxPlayers = (byte)cnt;
        _roomInfoText.text = PhotonNetwork.CurrentRoom.Name + "\nNow " + PhotonNetwork.CurrentRoom.PlayerCount +
            " / Max " + PhotonNetwork.CurrentRoom.MaxPlayers;
    }


    #region Ã¤ÆÃ
    public void Send()
    {
        if (_chatInput.text != "")
        {
            _network.Chat_RPC(RpcTarget.AllBufferedViaServer, PhotonNetwork.NickName + " : " + _chatInput.text);
            _chatInput.text = "";
            _chatInput.ActivateInputField();
        }
    }
    public void RenewChat()
    {
        _chatInput.text = "";
        _chatInput.ActivateInputField();
        for (int i = 0; i < _chatCells.Length; i++)
        {
            _chatCells[i].text = "";
        }
    }

    #endregion
}
