using UnityEngine;
using static UISystem_;

public class DontDestroyData_ : MonoBehaviour
{
    public static DontDestroyData_ _dontDestroyData;
    public CharacterType _characterKind = CharacterType.Pumpkin;
    public GameMode _gameMode = GameMode.Bounty;

    private void Awake()
    {
        if (_dontDestroyData)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            _dontDestroyData = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void SaveData()
    {

    }
}
