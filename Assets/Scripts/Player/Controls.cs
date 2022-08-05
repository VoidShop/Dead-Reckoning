using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Controls : MonoBehaviour
{
    public KeyCode forwards = KeyCode.W;
    public KeyCode backwards = KeyCode.S;
    public KeyCode turnLeft = KeyCode.A;
    public KeyCode turnRight = KeyCode.D;
    public KeyCode strafeCondition = KeyCode.Mouse1;
    public KeyCode jump = KeyCode.Space;
    public KeyCode toggleWalk = KeyCode.KeypadDivide;
}
