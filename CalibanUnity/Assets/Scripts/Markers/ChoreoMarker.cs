using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class ChoreoMarker : Marker, INotification
{
    public PropertyName id
    {
        get;
    }

    public string Command;
}
