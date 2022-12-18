using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public static InputController inputController = null;

    private void Awake()
    {
        if (inputController)
        {
            Destroy(this);
        }
        inputController = this;
    }

    public float _horizontal = 0;
    public bool _descend = false;

    public bool _attackUp = false;
    public bool _attackDown = false;
    //public bool _grenadeDown = false;
    public bool _jumpDown = false;
    public bool _jumpUp = false;

    //public bool _run = false;
    public bool _walk = false;

    public bool _tab = false;
    public bool _quitDown = false;

    private  KeyCode _tabCode = KeyCode.Tab;
    private  KeyCode _quitCode = KeyCode.Escape;

    private  string _horizontalString = "Horizontal";
    private  KeyCode _descendCode = KeyCode.DownArrow;

    private  KeyCode _attackCode = KeyCode.Z;
    private  KeyCode _jumpCode = KeyCode.X;

    private  KeyCode _walkCode = KeyCode.LeftControl;

    private void Update()
    {
        _jumpDown = Input.GetKeyDown(_jumpCode);
        _jumpUp = Input.GetKeyUp(_jumpCode);
        _horizontal = Input.GetAxisRaw(_horizontalString);
        _walk = Input.GetKey(_walkCode);

        _quitDown = Input.GetKeyDown(_quitCode);
        _tab = Input.GetKey(_tabCode);

        _descend = Input.GetKeyDown(_descendCode);
        _attackDown = Input.GetKeyDown(_attackCode);
        _attackUp = Input.GetKeyUp(_attackCode);
    }
}
