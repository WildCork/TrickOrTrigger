using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Lobby;
using static PlayerCell;

public class PlayerCell : MonoBehaviour
{
    public enum CellStatus { Empty = 0, Fill = 1, Closed = 2, Ready = 3 }
    public enum CharacterKind { Pumpkin, Santa };


    [Header("Image")]
    public Image _background;
    public Image _readyImage;
    public Image _spotlight;

    [Header("Character")]
    public GameObject[] _characters;

    [Header("Status")]
    public Text _nickname;
    public CellStatus _status;
    public CharacterKind _chacterKind = CharacterKind.Pumpkin;

    [Header("Setting")]
    [SerializeField] private Color _closedCell = Color.white;
    [SerializeField] private Color _openCell = Color.white;

    private string _emptyName = "";
    private string _closedName = "";

    public void Init()
    {
        _readyImage.gameObject.SetActive(false);
        _spotlight.gameObject.SetActive(false);
        HideCharacter();
    }

    public void FillCell(string name = "")
    {
        //Debug.Log("FillCell");
        _status = CellStatus.Fill;
        _background.color = _openCell;
        _readyImage.gameObject.SetActive(false);
        _spotlight.gameObject.SetActive(true);
        if (name != "")
            _nickname.text = name;
        _characters[(int)_chacterKind].SetActive(true);
    }

    public void OpenCell()
    {
        //Debug.Log("OpenCell");
        _status = CellStatus.Empty;
        _background.color = _openCell;
        _readyImage.gameObject.SetActive(false);
        _spotlight.gameObject.SetActive(false);
        _nickname.text = _emptyName;
        HideCharacter();
    }

    public void CloseCell()
    {
        //Debug.Log("CloseCell");
        _status = CellStatus.Closed;
        _background.color = _closedCell;
        _readyImage.gameObject.SetActive(false);
        _nickname.text = _closedName;
        _spotlight.gameObject.SetActive(false);
    }

    public void ReadyCell()
    {
        //Debug.Log("ReadyCell");
        _status = CellStatus.Ready;
        if (_nickname.text == PhotonNetwork.MasterClient.NickName)
            _readyImage.gameObject.SetActive(false);
        else
            _readyImage.gameObject.SetActive(true);
    }

    public void ChangeCell(int chacterKind)
    {
        _chacterKind = (CharacterKind)chacterKind;
        HideCharacter();
    }

    public void Click()
    {
        switch (_status)
        {
            case CellStatus.Empty:
                CloseCell();
                break;
            case CellStatus.Fill:
                break;
            case CellStatus.Closed:
                OpenCell();
                break;
            case CellStatus.Ready:
                break;
            default:
                break;
        }
    }

    private void HideCharacter()
    {
        for (int i = 0; i < _characters.Length; i++)
        {
            _characters[i].SetActive(false);
        }
    }
}
