using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{   
    private static EnvironmentManager _instance;
    public static EnvironmentManager inst { get { return _instance; } }


    public enum CurriculumPhase
    {
        DestroyImmobileTarget1,     //Phase 1 - Destroy Target directly in front
        DestroyImmobileTarget2,     //Phase 2 - Destroy Target that is slightly off centered from the front of the agent
        DestroyImmobileTarget3,     //Phase 3 - Destroy target that spawns randomly arround agent
        DestroyImmobileTarget4,     //Phase 4 - Find and Destroy target in arena
        DestroyMobileTarget,        //Phase 5 - Find and destroy target that moves in arena
        BattleSelf                  //Phase 6 - Agent Fights agaisnt itself
    }

    public CurriculumPhase currentPhase;

    public GameObject ImmobileTargetPrefab;


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

    public void MoveToNextPhase()
    {
        switch (currentPhase)
        {
            case CurriculumPhase.DestroyImmobileTarget1:
                Debug.Log("Moving to phase 2");
                currentPhase = CurriculumPhase.DestroyImmobileTarget2;
                break;
            case CurriculumPhase.DestroyImmobileTarget2:
                Debug.Log("Moving to phase 3");
                currentPhase = CurriculumPhase.DestroyImmobileTarget3;
                break;
            case CurriculumPhase.DestroyImmobileTarget3:
                Debug.Log("Moving to phase 4");
                currentPhase = CurriculumPhase.DestroyImmobileTarget4;
                break;
            case CurriculumPhase.DestroyImmobileTarget4:
                Debug.Log("Moving to phase 5");
                currentPhase = CurriculumPhase.DestroyMobileTarget;
                break;
            case CurriculumPhase.DestroyMobileTarget:
                Debug.Log("Moving to phase 6");
                currentPhase = CurriculumPhase.BattleSelf;
                break;
            case CurriculumPhase.BattleSelf:
                //do nothing
                break;
            default:
                break;
        }
    }
}
