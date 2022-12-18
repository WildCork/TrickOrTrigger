using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using UnityEngine.UI;

public class FirebaseAuthManager : MonoBehaviour
{
    private FirebaseAuth _auth; //to log in and sign up
    private FirebaseUser _user; //data of users who are identified


    public static FirebaseAuthManager _firebaseManager = null;

    public static FirebaseAuthManager firebaseManage
    {
        get
        {
            if (!_firebaseManager)
            {
                _firebaseManager = new();
            }
            return _firebaseManager;
        }
    }

    public string UserID => _user.UserId;
    public Action<bool> LoginState;

    public void Init()
    {
        _auth = FirebaseAuth.DefaultInstance;
        //임시 처리
        if (_auth.CurrentUser != null)
        {
            LogOut();
        }
        _auth.StateChanged += OnChanged;
    }

    private void OnChanged(object sender, EventArgs e)
    {
        if (_auth.CurrentUser != _user)
        {
            bool signed = (_auth.CurrentUser != _user && _auth.CurrentUser != null);

            if (!signed && _user != null)
            {
                Debug.Log("로그아웃");
                LoginState?.Invoke(false);
            }

            _user = _auth.CurrentUser;
            if (signed)
            {
                Debug.Log("로그인 성공");
                LoginState?.Invoke(true);
            }
        }
    }

    public void LogIn(string email, string password)
    {
        _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.Log("로그인 취소");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.Log("로그인 실패");
                return;
            }

            Debug.Log("로그인 성공");
            FirebaseUser newUser = task.Result;
        });
    }
    public void SignUp(string email, string password)
    {
        _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => 
        {
            if (task.IsCanceled)
            {
                Debug.Log("회원가입 취소");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.Log("회원가입 실패");
                return;
            }

            Debug.Log("회원가입 성공");
            FirebaseUser newUser = task.Result;
        });
    }
    public void LogOut()
    {
        _auth.SignOut();
        Debug.Log("로그아웃");
    }
}
