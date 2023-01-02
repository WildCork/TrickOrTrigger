using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static ObjectBase;
using static GameManager;
using Photon.Pun;

public class DetectGround : MonoBehaviour
{
    [SerializeField] private List<Collider2D> m_Grounds = new();
    private Rigidbody2D _characterRigidBody = null;
    private CharacterBase _characterBase = null;

    private void Awake()
    {
        _characterBase = transform.parent.GetComponent<CharacterBase>();
        _characterRigidBody = _characterBase.gameObject.GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_characterBase.photonView.IsMine)
        {
            return;
        }
        if (collision.gameObject.tag == gameManager._playerTag)
        {
            return;
        }
        if (_characterRigidBody.velocity.y > 0)
        {
            return;
        }
        switch (_characterBase._locationStatus)
        {
            case LocationStatus.Out:
                if (collision.gameObject.layer == gameManager._outLayer)
                {
                    MakeGround(ref collision, true);
                }
                break;
            case LocationStatus.In:
                if (collision.gameObject.layer == gameManager._inLayer)
                {
                    MakeGround(ref collision, true);
                }
                break;
            case LocationStatus.Door:
                MakeGround(ref collision, true);
                break;
            default:
                break;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!_characterBase.photonView.IsMine)
        {
            return;
        }
        if (collision.gameObject.tag == gameManager._playerTag)
        {
            return;
        }
        switch (_characterBase._locationStatus)
        {
            case LocationStatus.Out:
                if (collision.gameObject.layer == gameManager._outLayer)
                {
                    MakeGround(ref collision, false);
                }
                break;
            case LocationStatus.In:
                if (collision.gameObject.layer == gameManager._inLayer)
                {
                    MakeGround(ref collision, false);
                }
                break;
            case LocationStatus.Door:
                MakeGround(ref collision, false);
                break;
            default:
                break;
        }
    }

    private void MakeGround(ref Collider2D collision, bool isReal) //isReal=True 지형 실체화 isReal=False 지형 투영화
    {
        if (isReal)
        {
            if (collision.gameObject.layer != gameManager._doorLayer)
            {
                if(PhotonNetwork.LocalPlayer.IsLocal)
                {
                    collision.isTrigger = false;
                }
                if (!m_Grounds.Contains(collision))
                {
                    m_Grounds.Add(collision);
                }
                _characterBase.RefreshOnGround(true);
            }
        }
        else
        {
            if (collision.gameObject.layer != gameManager._wallLayer &&
                collision.gameObject.CompareTag(gameManager._bottomTag) == false)
            {
                collision.isTrigger = true;
            }
            m_Grounds.Remove(collision);
            if (m_Grounds.Count == 0)
            {
                _characterBase.RefreshOnGround(false);
            }
        }
    }

    public void Descend()
    {
        foreach (Collider2D ground in m_Grounds)
        {
            if (!ground.CompareTag(gameManager._bottomTag))
            {
                ground.isTrigger = true;
                m_Grounds.Remove(ground);
                break;
            }
        }
    }
}
