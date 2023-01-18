using System;
using UnityEngine;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Database;
using Photon.Pun;

public class FirebaseAuthManager : MonoBehaviour
{
    private FirebaseAuth _auth; //to log in and sign up
    private FirebaseUser _user; //data of users who are identified
    private DatabaseReference _databaseReference;

    public static FirebaseAuthManager _firebaseManager = null;

    public static FirebaseAuthManager firebaseManager
    {
        get
        {
            if (!_firebaseManager)
            {
                _firebaseManager = new ();
            }
            return _firebaseManager;
        }
    }

    public string UserID => _user.UserId;
    public Action<bool> LoginState;

    public void Init()
    {
        _auth = FirebaseAuth.DefaultInstance;
        _databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
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
                Debug.Log("LogOut");
                LoginState?.Invoke(false);
            }

            _user = _auth.CurrentUser;
            if (signed)
            {
                Debug.Log("LogIn Success");
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
                Debug.Log("LogIn Cancel");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.Log("LogIn Failed");
                return;
            }

            Debug.Log("LogIn Success");
            PhotonNetwork.ConnectUsingSettings();
            FirebaseUser newUser = task.Result;
        });
    }
    public void SignUp(string email, string id, string password)
    {
        _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => 
        {
            if (task.IsCanceled)
            {
                Debug.Log("SignUp Cancel");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.Log("SignUp Failed");
                return;
            }

            Debug.Log("SignUp Success");
            FirebaseUser newUser = task.Result;
        });
    }
    public void LogOut()
    {
        _auth.SignOut();
        Debug.Log("LogOut Success");
    }
    private void SaveNewUser(string userId, string name, string email) //데이터 저장하기
    {
        User user = new User();
        user.username = name;
        user.email = email;

        string json = JsonUtility.ToJson(user);

        _databaseReference.Child("Users").Child(userId).SetRawJsonValueAsync(json);

        Debug.Log("Save Data Success!");
    }

    private void LoadAllUsers() //데이터 불러오기
    {
        FirebaseDatabase.DefaultInstance.GetReference("Users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("Load Data Faulted");

            }
            else if (task.IsCompleted)
            {
                Debug.Log("Load Data Success!");
                DataSnapshot snapshot = task.Result;

            }
        });
    }
}

public class User
{
    public string username;
    public string email;
}
