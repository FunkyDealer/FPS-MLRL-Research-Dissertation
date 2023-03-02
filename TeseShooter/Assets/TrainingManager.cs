using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingManager : MonoBehaviour
{
    [SerializeField]
    List<GameManager> games = new List<GameManager>();


    private static TrainingManager _instance;
    public static TrainingManager inst { get { return _instance; } }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void CheckForPhaseUp()
    {
        int ready = 0;

        foreach (var g in games)
        {
            if (g.ReadyForPhaseUp) ready++;
        }

        if (ready == games.Count)
        {
            foreach (var g in games)
            {
                g.GoToNextPhase();
            }
        }
    }
}
