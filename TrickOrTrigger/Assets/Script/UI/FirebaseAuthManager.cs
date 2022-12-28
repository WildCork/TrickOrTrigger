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
        //�ӽ� ó��
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
                Debug.Log("�α׾ƿ�");
                LoginState?.Invoke(false);
            }

            _user = _auth.CurrentUser;
            if (signed)
            {
                Debug.Log("�α��� ����");
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
                Debug.Log("�α��� ���");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.Log("�α��� ����");
                return;
            }

            Debug.Log("�α��� ����");
            FirebaseUser newUser = task.Result;
        });
    }
    public void SignUp(string email, string password)
    {
        _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => 
        {
            if (task.IsCanceled)
            {
                Debug.Log("ȸ������ ���");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.Log("ȸ������ ����");
                return;
            }

            Debug.Log("ȸ������ ����");
            FirebaseUser newUser = task.Result;
        });
    }
    public void LogOut()
    {
        _auth.SignOut();
        Debug.Log("�α׾ƿ�");
    }
}
