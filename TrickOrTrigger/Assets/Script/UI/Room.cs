using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.UI;
using static PlayerCell;
using static UISystem;
using static DontDestroyData;

public class Room : MonoBehaviour
{
    public PlayerCell[] _playerCells;
    public Text _listText;
    public Text _roomInfoText;
    public InputField _chatInput;
    public Button _readyOrPlayBtn;

    public string _castleName = "Castle";
    public static string _chatCellsName = "ChatScrollView";
    private Text _readyOrPlayStatusName;
    [HideInInspector] public Text[] _chatCells;
    public int _cellNumber = -1;
    public int _roomMaxPlayerCnt = 8;
    public bool _isReady = false;

    public void Init()
    {
        _chatCells = transform.Find(_chatCellsName).GetComponentsInChildren<Text>();

        for (int i = 0; i < _roomMaxPlayerCnt; i++)
        {
            _playerCells[i].Init();
        }

        _readyOrPlayStatusName = _readyOrPlayBtn.GetComponentInChildren<Text>();
    }

    public void InitRoomWhenCreate()
    {
        _cellNumber = 0;
        for (int i = 0; i < _playerCells.Length; i++)
        {
            if (i == _cellNumber)
            {
                _playerCells[i].FillCell(PhotonNetwork.LocalPlayer.NickName);
            }
            else
            {
                _playerCells[i].OpenCell();
            }
        }
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
                if (_playerCells[_cellNumber]._characterKind != (CharacterType)characterKind)
                {
                    _playerCells[_cellNumber].FillCell("",
                        _dontDestroyData._characterKind = (CharacterType)characterKind);
                    RenewPlayerCells_RPC(RpcTarget.Others);
                }
                break;
            case CellStatus.Closed:
                break;
            case CellStatus.Ready:
                uiSystem.Chat("If you wanna change, you should be not ready!");
                break;
            default:
                break;
        }
    }

    public void RenewPlayerCells_RPC(RpcTarget rpcTarget = RpcTarget.All)
    {
        CellStatus[] status = new CellStatus[_playerCells.Length];
        string[] nicknames = new string[_playerCells.Length];
        CharacterType[] characterKinds = new CharacterType[_playerCells.Length];
        for (int i = 0; i < _playerCells.Length; i++)
        {
            status[i] = _playerCells[i]._status;
            nicknames[i] = _playerCells[i]._nickname.text;
            characterKinds[i] = _playerCells[i]._characterKind;
        }
        uiSystem.RenewPlayerCells_RPC(rpcTarget, Array.ConvertAll(status, value => (int)value),
            Array.ConvertAll(nicknames, value => value), Array.ConvertAll(characterKinds, value => (int)value));
    }

    [PunRPC]
    public void RenewRoom(int[] status, string[] nicknames, int[] characterKinds)
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
                    _playerCells[i].FillCell(nicknames[i], (CharacterType)characterKinds[i]);
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


    public int AddCell(string name)
    {
        for (int i = 0; i < _playerCells.Length; i++)
        {
            switch (_playerCells[i]._status)
            {
                case CellStatus.Empty:
                    _playerCells[i].FillCell(name);
                    return i;
                case CellStatus.Fill:
                    break;
                case CellStatus.Closed:
                    break;
                case CellStatus.Ready:
                    break;
                default:
                    break;
            }
        }
        return -1;
    }

    public void RemoveCell(string nickname)
    {
        for (int i = 0; i < _playerCells.Length; i++)
        {
            switch (_playerCells[i]._status)
            {
                case CellStatus.Empty:
                case CellStatus.Closed:
                    break;
                default:
                    if (_playerCells[i]._nickname.text == nickname)
                    {
                        _playerCells[i].OpenCell();
                        return;
                    }
                    break;
            }
        }
    }

    #region 게임
    public void ReadyOrPlay()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (!IsAllReady())
            {
                uiSystem.Chat("Any player is not ready!! (ReadyOrPlay())");
            }
            //else if(_readyPlayers == 0)
            //{
            //    _photonView.RPC(_chatRPC, RpcTarget.AllBufferedViaServer, PhotonNetwork.NickName + " : " + "You cannot play alone!! ㅠㅠ");
            //}
            else
            {
                for (int i = 0; i < _playerCells.Length; i++)
                {
                    switch (_playerCells[i]._status)
                    {
                        case CellStatus.Empty:
                            break;
                        case CellStatus.Fill:
                            break;
                        case CellStatus.Closed:
                            break;
                        case CellStatus.Ready:
                            _playerCells[i]._status = CellStatus.Fill;
                            break;
                        default:
                            break;
                    }
                }
                RenewPlayerCells_RPC();
                uiSystem.ChangeScene_RPC(RpcTarget.AllBufferedViaServer, _castleName);
            }
        }
        else
        {
            _isReady = !_isReady;
            if (_isReady)
            {
                _playerCells[_cellNumber].ReadyCell();
            }
            else
            {
                _playerCells[_cellNumber].FillCell();
            }
            RenewPlayerCells_RPC();
        }
    }

    private bool IsAllReady()
    {
        for (int i = 0; i < _playerCells.Length; i++)
        {
            switch (_playerCells[i]._status)
            {
                case CellStatus.Empty:
                    break;
                case CellStatus.Fill:
                    if (_playerCells[i]._nickname.text == PhotonNetwork.MasterClient.NickName)
                    {
                        break;
                    }
                    else
                    {
                        return false;
                    }
                case CellStatus.Closed:
                    break;
                case CellStatus.Ready:
                    break;
                default:
                    break;
            }
        }
        return true;
    }
    public void LeaveRoom()
    {
        //_photonView = GameObject.Find(_networkName).GetComponent<PhotonView>();
        _cellNumber = -1;
        _isReady = false;
        PhotonNetwork.LeaveRoom();
    }

    #endregion

    #region 채팅
    public void Send()
    {
        if (_chatInput.text != "")
        {
            uiSystem.Chat_RPC(RpcTarget.AllBufferedViaServer, PhotonNetwork.NickName + " : " + _chatInput.text);
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
