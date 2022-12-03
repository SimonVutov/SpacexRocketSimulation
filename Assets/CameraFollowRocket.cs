using System;
using UnityEngine;

public class CameraFollowRocket : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    private Vector3 lookatsmooth = Vector3.zero;

    private void Start()
    {
        transform.position = player.position;
    }

    void Update()
    {
        lookatsmooth = Vector3.Lerp(lookatsmooth, player.transform.position + player.GetComponent<Rigidbody>().velocity * 0.3f, 0.1f);
        transform.LookAt(lookatsmooth);

        transform.position = Vector3.Lerp(transform.position, (player.transform.position + offset + player.GetComponent<Rigidbody>().velocity * 0.01f) + (transform.position - player.position) * 0.1f/(1 + player.GetComponent<Rigidbody>().velocity.magnitude * 0.1f), 0.08f);
    }
}