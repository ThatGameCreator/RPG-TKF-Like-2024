using UnityEngine.UI;

public class CustomButton : Button
{
    // Public accessor for the protected IsPressed() method
    public bool IsButtonPressed()
    {
        return IsPressed();
    }
}
