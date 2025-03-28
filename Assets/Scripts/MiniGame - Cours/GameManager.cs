using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    int _score = 0;
    [SerializeField] private TextMeshPro _scoreText;

    // Update is called once per frame
    void Update()
    {
        _score = (int)(Time.time * 10);
        _scoreText.SetText(_score.ToString());
    }
}
