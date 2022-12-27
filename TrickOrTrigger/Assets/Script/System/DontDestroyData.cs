using UnityEngine;
using static PlayerCell;

public class DontDestroyData : MonoBehaviour
{
    public static DontDestroyData _dontDestroyData;
    public CharacterKind _characterKind = CharacterKind.Pumpkin;

    int _cellIndex = -1;
    int[] _status = null;
    string[] _nicknames = null;
    int[] _characterKinds = null;

    private void Awake()
    {
        if (_dontDestroyData)
        {
            Destroy(gameObject);
        }
        else
        {
            _dontDestroyData = this;
        }
    }

    public void SaveData(int[] status, string[] nicknames, int[] characterKinds)
    {
        _status= status;
        _nicknames= nicknames;
        _characterKinds= characterKinds;
    }
}
