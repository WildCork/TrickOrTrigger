using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CellCovers : MonoBehaviour
{
    public static CellCovers cellCovers = null;

    public Transform _playerCells;

    private void Awake()
    {
        InitChangeCovers();
    }

    private void InitChangeCovers()
    {
        if (_playerCells.childCount != transform.childCount)
        {
            Debug.LogError($"Player cells count {_playerCells.childCount}" +
                $" is not same of covers count {transform.childCount}");
        }
        else
        {
            for (int i = 0; i < _playerCells.childCount; i++)
            {
                RectTransform rectTransform1 = transform.GetChild(i).GetComponent<RectTransform>();
                RectTransform rectTransform2 = _playerCells.GetChild(i).GetComponent<RectTransform>();
                rectTransform1.position = rectTransform2.position;
                rectTransform1.localScale = rectTransform2.localScale;
                rectTransform2.GetComponent<PlayerCell>()._changeCover = rectTransform1.GetComponent<Image>();
                rectTransform1.gameObject.SetActive(false);
            }
        }
    }

}
