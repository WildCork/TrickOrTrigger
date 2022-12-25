using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static Loading;
using static PlayerCell;

public class Lobby : MonoBehaviourPunCallbacks
{
    public static Lobby _lobby = null;

    [Header("Scene")]
    public string _castleName = "Castle";
    public string _lobbyName = "Lobby";

    [Header("Panels")]
    public GameObject _disconnectPanel;
    public GameObject _lobbyPanel;
    public GameObject _roomPanel;
    public Text StatusText;

    public Network _network = null;
    public PhotonView _photonView = null;
    //CreateRoom 클릭 후 상세 정보 창 생성 -> 정보를 정하고 생성 하기

    [Header("UI Name")]
    public static string _roomNameInputName = "RoomNameInput";
    public static string _welcomeTextName = "WelcomeText";
    public static string _lobbyInfoTextName = "LobbyInfoText";
    public static string _cellBtnName = "RoomCells";
    public static string _previousBtnName = "PreviousBtn";
    public static string _nextBtnName = "NextBtn";
    public static string _listTextName = "ListText";
    public static string _readyImageName = "ReadyImage";
    public static string _roomInfoTextName = "RoomInfoText";
    public static string _chatCellsName = "ChatScrollView";
    public static string _playerCellsName = "PlayerCells";
    public static string _chatInputName = "ChatInput";
    public static string _readyOrPlayBtnName = "GameBtn";

    [Header("Object Name")]
    public string _networkName = "Network";
    public string _loadingName = "Loading";

    [Header("Function Name")]
    public string _renewCellsRPC = "RenewPlayerCells";
    public string _decideCellNumRPC = "DecideCellNumber";
    public string _chatRPC = "ChatRPC";
    public string _changeScene = "ChangeScene";

    [Header("Status")]
    public int _cellNumber = -1;
    public CharacterKind _characterKind = CharacterKind.Pumpkin;
    public bool _isReady = false;
    public int currentPage = 1, maxPage, multiple;
    public int _roomMaxPlayerCnt = 8;

    [Header("Disconnect")]
    public InputField _nickNameInput; // 로그인 화면 아이디 입력창
    private Text _nickNameInputHolder;
    [Header("Lobby")]
    public InputField _roomNameInput;
    public Text _welcomeText;
    public Text _lobbyInfoText;
    public Button[] _roomCells;
    public Button _previousBtn;
    public Button _nextBtn;

    [Header("Room")]
    public PlayerCell[] _playerCells;
    public Text _listText;
    public Text _roomInfoText;
    public InputField _chatInput;
    public Button _readyOrPlayBtn;


    private Text _readyOrPlayStatusName;
    [HideInInspector] public Text[] _chatCells;

    public PhotonView PV
    {
        get
        {
            if (!_photonView)
            {
                _photonView = GameObject.Find(_networkName).GetComponent<PhotonView>();
            }
            return _photonView;
        }
    }

    public Network Network
    {
        get
        {
            if (!_network) 
            {
                _network = GameObject.Find(_networkName).GetComponent<Network>();
            }
            return _network;
        }
    }

    private void Awake()
    {
        if (_lobby)
        {
            Destroy(gameObject);
            _lobby.GetComponent<Canvas>().worldCamera = Camera.main;
            _lobby._photonView = GameObject.Find(_networkName).GetComponent<PhotonView>();
            return;
        }
        else
        {
            _lobby = this;
        }
        Screen.SetResolution(1920, 1080, false);
        InitLobby();
        DontDestroyOnLoad(gameObject);
    }


    #region 방 초기화

    private void InitLobby()
    {
        _nickNameInputHolder = _nickNameInput.GetComponentInChildren<Text>();

        _disconnectPanel.SetActive(true);
        _nickNameInput.ActivateInputField();
        _lobbyPanel.SetActive(false);
        InitRoomPanel();
    }

    private void InitRoomPanel()
    {
        _chatCells = _roomPanel.transform.Find(_chatCellsName).GetComponentsInChildren<Text>();

        for (int i = 0; i < _roomMaxPlayerCnt; i++)
        {
            _playerCells[i].Init();
        }

        _readyOrPlayStatusName = _readyOrPlayBtn.GetComponentInChildren<Text>();
        _roomPanel.SetActive(false);
    }


    #endregion


    #region 방리스트 갱신
    private List<RoomInfo> myList = new List<RoomInfo>();
    public void RenewMyList()
    {
        // 최대페이지
        maxPage = (myList.Count % _roomCells.Length == 0) ?
            myList.Count / _roomCells.Length : myList.Count / _roomCells.Length + 1;

        // 이전, 다음버튼
        _previousBtn.interactable = (currentPage <= 1) ? false : true;
        _nextBtn.interactable = (currentPage >= maxPage) ? false : true;

        // 페이지에 맞는 리스트 대입
        multiple = (currentPage - 1) * _roomCells.Length;
        for (int i = 0; i < _roomCells.Length; i++)
        {
            _roomCells[i].interactable = (multiple + i < myList.Count) ? true : false;
            _roomCells[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            _roomCells[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
        }
    }

    public void ClickPlayerCell(int i)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        _playerCells[i].Click();
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
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
                    _playerCells[_cellNumber].FillCell("", (CharacterKind)characterKind);
                    _characterKind = (CharacterKind)characterKind;
                    RenewPlayerCells_RPC(RpcTarget.Others);
                }
                break;
            case CellStatus.Closed:
                break;
            case CellStatus.Ready:
                _network.ChatRPC("If you wanna change, you should be not ready!");
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
        PV.RPC(_renewCellsRPC, rpcTarget, Array.ConvertAll(status, value => (int)value),
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

    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
    }

    public void OnJoinedLobby(string nickname)
    {
        _lobbyPanel.SetActive(true);
        _roomPanel.SetActive(false);
        _roomNameInput.text = "";
        _roomNameInput.ActivateInputField();
        _welcomeText.text = "Welcome " + nickname;
        myList.Clear();
    }

    // ◀버튼 -2 , ▶버튼 -1 , 셀 숫자
    public void MyListClick(int num)
    {
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        else PhotonNetwork.JoinRoom(myList[multiple + num].Name);
        RenewMyList();
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("OnRoomListUpdate");
        UpdateRoomList(roomList);
        RenewMyList();
    }
    #endregion

    #region 서버연결


    void Update()
    {
        if(StatusText.text != PhotonNetwork.NetworkClientState.ToString())
        {
            StatusText.text = PhotonNetwork.NetworkClientState.ToString();
            loading.RenewValue(StatusText.text);
        }
        _lobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + " _lobby / " + PhotonNetwork.CountOfPlayers + " _connect";
    }

    public void Connect()
    {
        //if(IsAlreadyNickname())
        //{
        //    _nickNameInput.text = "";
        //    _nickNameInputHolder.text = "It's Already nickname!!";
        //    return;
        //}
        if (_nickNameInput.text != "")
        {
            loading.ShowLoading(true, NetworkState.EnterServer);
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            _nickNameInputHolder.text = "Please write your name!!";
        }
    }

    //private bool IsAlreadyNickname() // 접속 유저 닉네임 데이터를 가져올 방법을 모르겟음
    //{
    //    foreach (var view in PhotonNetwork.NetworkingClient.)
    //    {
    //        if (view.Owner != null)
    //        {
    //            if (_nickNameInput.text == view.Owner.NickName)
    //            {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
        _disconnectPanel.gameObject.SetActive(true);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        _disconnectPanel.gameObject.SetActive(false);
        _roomPanel.gameObject.SetActive(false);
        loading.ShowLoading(false);
        PhotonNetwork.LocalPlayer.NickName = _nickNameInput.text;
        OnJoinedLobby(PhotonNetwork.LocalPlayer.NickName);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        loading.ShowLoading(false);
        _lobbyPanel.SetActive(false);
        _roomPanel.SetActive(false);
    }
    #endregion

    #region 방
    public void CreateRoom()
    {
        Debug.Log("CreateRoom");
        PhotonNetwork.CreateRoom("Room" + UnityEngine.Random.Range(0, 100) + "\n" +
            (_roomNameInput.text == "" ? "Welcome Anyone!!" : _roomNameInput.text),
            new RoomOptions { MaxPlayers = (byte)_roomMaxPlayerCnt });
        //loading.ShowLoading(true, NetworkState.EnterRoom);
        InitRoomWhenCreate();
    }

    private void InitRoomWhenCreate()
    {
        for (int i = 0; i < _playerCells.Length; i++)
        {
            if (i == 0)
            {
                _playerCells[i].FillCell(PhotonNetwork.LocalPlayer.NickName);
            }
            else
            {
                _playerCells[i].OpenCell();
            }
        }
    }

    public void JoinRandomRoom()
    {
        //loading.ShowLoading(true, NetworkState.EnterRoom);
        PhotonNetwork.JoinRandomRoom();
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
        _disconnectPanel.gameObject.SetActive(false);
        _lobbyPanel.gameObject.SetActive(false);
        _roomPanel.SetActive(true);
        RenewChat();
        if (PhotonNetwork.IsMasterClient)
        {
            _cellNumber= 0;
            PV.RPC(_chatRPC, RpcTarget.AllBufferedViaServer, "<color=yellow> Player " 
                + PhotonNetwork.LocalPlayer.NickName + " create this room.</color> (InitRoomWhenCreate)");
            RenewPlayerCells_RPC();
        }

        StartCoroutine(WaitCellNumber());
    }

    IEnumerator WaitCellNumber()
    {
        while (true)
        {
            if (_cellNumber >= 0)
            {
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
        loading.ShowLoading(false);
        yield return null;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnCreateRoomFailed");
        _roomNameInput.text = "";
        CreateRoom();
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed");
        _roomNameInput.text = ""; 
        CreateRoom(); 
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("OnPlayerEnteredRoom");
        PV.RPC(_chatRPC, RpcTarget.AllBufferedViaServer, "<color=yellow> Player " + newPlayer.NickName + " enters this room.</color> (OnPlayerEnteredRoom)");
        PV.RPC(_decideCellNumRPC, newPlayer, AddCell(newPlayer.NickName));
        RenewPlayerCells_RPC();
    }

    private int AddCell(string name)
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

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("OnPlayerLeftRoom");
        RemoveCell(otherPlayer.NickName);
        RenewPlayerCells_RPC();
        PV.RPC(_chatRPC, RpcTarget.AllBufferedViaServer, "<color=yellow> Player " + otherPlayer.NickName + " exits this room.</color> (OnPlayerLeftRoom)");
    }

    private void RemoveCell(string nickname)
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
    #endregion

    #region 게임
    public void ReadyOrPlay()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (!IsAllReady())
            {
               _network.ChatRPC("Any player is not ready!! (ReadyOrPlay())");
            }
            //else if(_readyPlayers == 0)
            //{
            //    PV.RPC(_chatRPC, RpcTarget.AllBufferedViaServer, PhotonNetwork.NickName + " : " + "You cannot play alone!! ㅠㅠ");
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
                PV.RPC(_changeScene, RpcTarget.AllBufferedViaServer, _castleName);
            }
        }
        else
        {
            _isReady = !_isReady;
            if(_isReady)
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
    public void LeaveGame()
    {
        gameObject.SetActive(true);
        PhotonNetwork.LoadLevel(_lobbyName);
        _photonView = GameObject.Find(_networkName).GetComponent<PhotonView>();
        LeaveRoom();
    }
    public void LeaveRoom()
    {
        _cellNumber = -1;
        _isReady = false;
        //loading.ShowLoading(true, NetworkState.LeaveRoom);
        PhotonNetwork.LeaveRoom();
    }

    #endregion

    #region 채팅
    public void Send()
    {
        if (_chatInput.text != "")
        {
            PV.RPC(_chatRPC, RpcTarget.AllBufferedViaServer, PhotonNetwork.NickName + " : " + _chatInput.text);
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
