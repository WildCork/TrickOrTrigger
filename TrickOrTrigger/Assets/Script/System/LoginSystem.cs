using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static FirebaseAuthManager;

public class LoginSystem : MonoBehaviour
{
    public InputField _emial;
    public InputField _password;

    public Text _outputText;

    private void Start()
    {
        _firebaseManager.LoginState += OnChangedState;
        _firebaseManager.Init();
    }

    private void OnChangedState(bool sign)
    {
        _outputText.text = sign ? "로그인: " : "로그아웃: ";
        _outputText.text += _firebaseManager.UserID;
    }

    public void LogIn()
    {
        _firebaseManager.LogIn(_emial.text, _password.text);
    }
    public void SignUp()
    {
        _firebaseManager.SignUp(_emial.text, _password.text);
    }
    public void LogOut()
    {
        _firebaseManager.LogOut();
    }
}
