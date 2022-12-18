using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class PlayerBar : MonoBehaviourPunCallbacks
{
    [Header("Value")]
    [SerializeField] private int _actNum = -1;
    [SerializeField] private float _offsetY = 6f;
    [SerializeField] private int _bulletShowCnt = 30;

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
    public void RefreshDefault(CharacterBase character, string nickname)
    {
        _character = character;
        _nicknameText.text = nickname;
        RefreshHP();
        RefreshBulletCnt();
    }

    private void Awake()
    {
        if(!photonView.IsMine)
        {
            _bulletCntSlider.gameObject.SetActive(false);
            _bulletCntText.gameObject.SetActive(false);
        }
        else
        {
            _bulletCntSlider.gameObject.SetActive(true);
            _bulletCntText.gameObject.SetActive(true);
        }
        gameManager._playerBarViewIdSet.Add(gameObject.GetPhotonView().ViewID);
        gameManager._playerBarIDToPlayerBar[gameObject.GetPhotonView().ViewID] = this;
    }

    private void Update()
    {
        if (!gameManager._isGame)
        {
            return;
        }
        transform.position = _character.transform.position + Vector3.up * _offsetY;
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
