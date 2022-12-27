using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Loading;

public class Login : MonoBehaviour
{
    public InputField _nickNameInput; // 로그인 화면 아이디 입력창
    private Text _nickNameInputHolder;

    public void Init()
    {
        _nickNameInputHolder = _nickNameInput.GetComponentInChildren<Text>();
    }

    public void Connect()
    {
        //TODO: 닉네임 중복 검사 로직 (미완성)
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
}
