using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Loading;
using ExitGames.Client.Photon;

public class Lobby : MonoBehaviour
{
    public int currentPage = 1; 
    public int maxPage = -1; 
    public int multiple = -1;
    public int _roomMaxPlayerCnt = 8;

    public InputField _roomNameInput;
    public Text _welcomeText;
    public Text _lobbyInfoText;
    public Button[] _roomCells;
    public Button _previousBtn;
    public Button _nextBtn;

    private List<RoomInfo> myList = new List<RoomInfo>();

    string _status = "";
    void Update()
    {
        if (_status != PhotonNetwork.NetworkClientState.ToString())
        {
            loading.RenewValue(_status = PhotonNetwork.NetworkClientState.ToString());
        }
        _lobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + " _lobby / " + PhotonNetwork.CountOfPlayers + " _connect";
    }

    // ◀버튼 -2 , ▶버튼 -1 , 셀 숫자
    public void MyListClick(int num)
    {
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        else PhotonNetwork.JoinRoom(myList[multiple + num].Name);
        RenewMyList();
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

    public void CreateRoom()
    {
        Debug.Log("CreateRoom");
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)_roomMaxPlayerCnt;
        roomOptions.CustomRoomProperties = new Hashtable() { }; //TODO: 게임 모드 데이터 저장 및 처리
        PhotonNetwork.CreateRoom("Room" + UnityEngine.Random.Range(0, 100) + "\n" 
            + (_roomNameInput.text == "" ? "Welcome Anyone!!" : _roomNameInput.text), roomOptions);
        //TODO: 방 생성 후 방 초기화 코드
        //InitRoomWhenCreate(); -> Room 스크립트에서 호출하도록
    }

    public void JoinRandomRoom()
    {
        //loading.ShowLoading(true, NetworkState.EnterRoom);
        PhotonNetwork.JoinRandomRoom();
    }

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
        gameObject.SetActive(true);
    }

    public void OnJoinedLobby(string nickname)
    {
        _roomNameInput.text = "";
        _roomNameInput.ActivateInputField();
        _welcomeText.text = "Welcome " + nickname;
        myList.Clear();
    }

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
}
