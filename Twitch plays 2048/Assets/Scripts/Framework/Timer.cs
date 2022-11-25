using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    public bool IsTime { get { return Time == MaxTime; } }
    public float MaxTime { get; private set; }
    public float Time { get; private set; }
    public bool Pause = false;
    
    public void Setup(float maxTime, bool? pause)
    {
        MaxTime = maxTime;
        Reset(pause);
    }

    public void Update(float deltaTime)
    {
        if (Pause)
            return;
        Time = Mathf.Clamp(Time + deltaTime, 0.0f, MaxTime);
        if (IsTime)
            Pause = true;
    }

    public void Reset(bool? pause)
    {
        Time = 0.0f;
        Pause = pause.GetValueOrDefault(Pause);
    }
}
