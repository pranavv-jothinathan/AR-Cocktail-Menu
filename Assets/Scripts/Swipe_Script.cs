using UnityEngine;


public class Swipe_Script : MonoBehaviour 
{
    [SerializeField] private CocktailARManager arManager;

    public ViewMode_Script viewmodescript;

    private Vector2 touchStart;
    private Vector2 touchEnd;
    private float expectedWeightOfSwipe = 50f;

    void Start()
    {
        Debug.Log("Swipe Script Started");
    }

    void Update()
    {
        arManager = CocktailARManager.Instance;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStart = touch.position;
            }

            if (touch.phase == TouchPhase.Ended)
            {
                touchEnd = touch.position;

                float currentWeightOfSwipe = touchEnd.x - touchStart.x;

                if (Mathf.Abs(currentWeightOfSwipe) > expectedWeightOfSwipe)
                {
                    TutorialManager.Instance.CompleteTutorial();

                    arManager = CocktailARManager.Instance;

                    if (currentWeightOfSwipe > 0)
                    {
                        arManager.SwitchToPreviousDrink(); // Swipe Right - Previous drink
                    }
                    else
                    { 
                        arManager.SwitchToNextDrink(); // Swipe Left - Next drink
                    }
                }
            }
        }
    }
}
