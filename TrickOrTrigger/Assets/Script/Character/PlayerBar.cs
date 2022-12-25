using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static GameManager;

public class PlayerBar : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Value")]
    [SerializeField] private int _actNum = -1;
    [SerializeField] private float _offsetY = 6f;
    [SerializeField] private int _bulletShowCnt = 30;
    [SerializeField] private SortingGroup _sortingGroup;

    [Header("String")]
    public string _hpTextString = "HPText";
    public string _hpString = "HPBar";
    public string _bulletCntString = "BulletCntBar";
    public string _fillAreaString = "Fill Area";

    [Header("Text")]
    [SerializeField] private Text _hpText;
    [SerializeField] private Text _nicknameText;
    [SerializeField] private Text _bulletCntText;

    [Header("Slider")]
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private Slider _bulletCntSlider;

    [Header("Image")]
    [SerializeField] private Image _bulletCntImage;

    [Header("Color")]
    public Color _nonInfiniteColor = Color.white;
    public Color _infiniteColor = Color.white;

    private CharacterBase _character = null;

    Vector3 _curPos;
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else
        {
            _curPos = (Vector3)stream.ReceiveNext();
        }
    }

    public void RefreshDefault(CharacterBase character, string nickname)
    {
        character._playerBar = this;
        _character = character;
        _nicknameText.text = nickname;
        RefreshHP();
        RefreshBulletCnt();
    }

    private void Awake()
    {
        _sortingGroup = GetComponent<SortingGroup>();
        if (photonView.IsMine)
        {
            _bulletCntSlider.gameObject.SetActive(true);
            _bulletCntText.gameObject.SetActive(true);
            _sortingGroup.sortingOrder = 1;
        }
        else
        {
            _bulletCntSlider.gameObject.SetActive(false);
            _bulletCntText.gameObject.SetActive(false);
            _sortingGroup.sortingOrder = 0;
        }
        gameManager._playerBarViewIdSet.Add(gameObject.GetPhotonView().ViewID);
        gameManager._playerBarIDToPlayerBar[gameObject.GetPhotonView().ViewID] = this;
    }

    private void Update()
    {
        if (gameManager._isGame)
        {
            if (photonView.IsMine)
            {
                transform.position = _character.transform.position + Vector3.up * _offsetY;
            }
            else if ((transform.position - _curPos).sqrMagnitude >= 100)
            {
                transform.position = _curPos;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, _curPos, Time.deltaTime * 10);
            }
        }
    }


    public void RefreshHP()
    {
        _hpText.text = _character.hp.ToString();
        _hpSlider.value = (float)_character.hp / _character._maxHp;
    }

    public void RefreshBulletCnt()
    {
        if (_character.bulletCnt < 0)
        {
            _bulletCntImage.color = _infiniteColor;
            _bulletCntText.text = "";
            _bulletCntSlider.value = 1;
        }
        else
        {
            _bulletCntImage.color = _nonInfiniteColor;

            if (_character.bulletCnt < _bulletShowCnt)
            {
                _bulletCntText.text = _character.bulletCnt.ToString();
            }

            _bulletCntSlider.value = (float)_character.bulletCnt / _character._maxBulletCnt;
        }
    }

}
