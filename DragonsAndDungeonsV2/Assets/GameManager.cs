using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameManager
{
    public static WindowManager windowManager = null;
    public static ObjectManager objectManager = null;
    public static Token SelectedToken {get; private set;} = null;

    public static void SelectToken(Token newToken) {
        if(SelectedToken != null) {
            SelectedToken.Unhighlight();
        }
        SelectedToken = newToken;
        if(newToken != null) {
            newToken.Highlight();
        }
        if(windowManager != null) {
            if(windowManager.CharacterManager != null) {
                // Do something with the Character Manager
                windowManager.CharacterManager.UpdateAll();
            }
        }
    }
}
