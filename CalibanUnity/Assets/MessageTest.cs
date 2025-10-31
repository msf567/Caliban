using Caliban.Core.Transport;
using Caliban.Unity;
using System.Collections;
using UnityEngine;

public class MessageTest : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(MessageDelay());
    }

    void Update()
    {
        
    }

    private IEnumerator MessageDelay()
    {
        yield return new WaitForSeconds(1f);
        DCon.Log("Sending message to host...");
        CalibanClientBehaviour.client.SendMessageToHost(MessageType.DEBUG_LOG,"Hello from Unity!");
    }
}
