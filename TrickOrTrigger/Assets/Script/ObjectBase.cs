using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using static GameManager;
using static Weapon;

public class ObjectBase : MonoBehaviourPunCallbacks
{
    public enum LocationStatus { In, Out, Door };

    [Header("ObjectBase")]
    public LocationStatus _locationStatus = LocationStatus.Out;

    protected SpriteRenderer _spriteRenderer = null;
    protected Rigidbody2D _rigidbody2D = null;
    protected Collider2D _collider2D = null;
    protected PhotonView _photonView = null;
    protected AudioSource _audioSource = null;

    public List<Collider2D> _triggerWallSet = new();
    public List<Collider2D> _triggerMapSet = new();

    public AudioClip[] _audioClips;

    protected virtual void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<Collider2D>();
        _photonView = GetComponent<PhotonView>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponentInChildren<AudioSource>();
    }

    protected void PlaySound_RPC(int index)
    {
        photonView.RPC(nameof(PlaySound), RpcTarget.All, index);
    }

    [PunRPC]
    protected void PlaySound(int index)
    {
        if (index < 0)
        {
            _audioSource.clip = null;
        }
        else if (_audioSource.clip != _audioClips[index])
        {
            _audioSource.clip = _audioClips[index];
            _audioSource.Play();
        }
    }

    #region In Out Logic

    private Vector2 doorDir; //From Out To In
    private float _dotValue;
    private ObjectBase _object;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == gameManager._doorLayer && _triggerWallSet.Count == 0)
        {
            doorDir = collision.transform.rotation * Vector2.left;
            _dotValue = Vector2.Dot(ContactNormalVec(collision.transform.position, transform.position), doorDir);
            switch (_locationStatus)
            {
                case LocationStatus.In:
                    if (_dotValue < 0)
                    {
                        RefreshLocationStatus(LocationStatus.Door);
                    }
                    break;
                case LocationStatus.Out:
                    if (_dotValue > 0)
                    {
                        RefreshLocationStatus(LocationStatus.Door);
                    }
                    break;
                case LocationStatus.Door:
                    break;
                default:
                    Debug.LogError($"It is no enum state for {_locationStatus}");
                    break;
            }
        }

        if (_object = collision.GetComponent<ObjectBase>())
        {
            if (_object._locationStatus == LocationStatus.Door)
            {
                Hit(collision);
            }
            else if (_object._locationStatus == _locationStatus)
            {
                Hit(collision);
            }
        }

        if (collision.gameObject.layer == gameManager._wallLayer
            || collision.gameObject.layer == gameManager._inLayer
            || collision.gameObject.layer == gameManager._outLayer)
        {
            if (!_triggerMapSet.Contains(collision))
                _triggerMapSet.Add(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == gameManager._doorLayer && _triggerWallSet.Count == 0)
        {
            doorDir = collision.transform.rotation * Vector2.left; //From Out To In
            _dotValue = Vector2.Dot(ContactNormalVec(collision.transform.position, transform.position), doorDir);
            switch (_locationStatus)
            {
                case LocationStatus.In:
                    break;
                case LocationStatus.Out:
                    break;
                case LocationStatus.Door:
                    if (_dotValue > 0)
                    {
                        RefreshLocationStatus(LocationStatus.Out);
                    }
                    else if (_dotValue < 0)
                    {
                        RefreshLocationStatus(LocationStatus.In);
                    }
                    break;
                default:
                    Debug.LogError($"It is no enum state for {_locationStatus}");
                    break;
            }
        }

        if (collision.gameObject.layer == gameManager._wallLayer
            || collision.gameObject.layer == gameManager._inLayer
            || collision.gameObject.layer == gameManager._outLayer)
        {
            if (_triggerMapSet.Contains(collision))
                _triggerMapSet.Remove(collision);
        }
    }

    protected virtual void RefreshLocationStatus(LocationStatus locationStatus)
    {
        _locationStatus = locationStatus;
    }
    protected virtual void Hit(Collider2D collision) { }
    private Vector2 ContactNormalVec(Vector2 collision, Vector2 pos)
    {
        return (collision - pos).normalized;
    }

    #endregion
}
