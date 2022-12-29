using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class Storage : MonoBehaviourPun
{
    public Transform _ownerStorage;
    private void Awake()
    {
        gameManager._storageSet.Add(this);
    }
}
