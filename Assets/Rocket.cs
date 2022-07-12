using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rocket : MonoBehaviour
{
    float predictedEngineStart;
    public Vector3 predictedLandingCoordinate;
    public float predictedSecond;
    private float thrustToPathX;
    private float thrustToPathZ;

    Rigidbody rb;
    float thrustForce = 15.696f;
    public GameObject locationOfThrust;

    bool thrusting = false;
    bool manualThrusting = false;
    Vector3 landPosition = new Vector3(0f, 1.5f, 0f);

    float throttle = 1f;

    bool dead = false;
    public Transform startForce;
    Vector3 thrustDir = Vector3.zero;
    public Text text;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce((startForce.position - transform.position) * 10f, ForceMode.Acceleration);

        dead = false;
    }

    void Update()
    {
        predictedLandingCoordinate.x = transform.position.x + predictedSecond * rb.velocity.x;
        predictedLandingCoordinate.z = transform.position.z + predictedSecond * rb.velocity.z;

        thrustToPathX = Mathf.Clamp(predictedLandingCoordinate.x * 0.035f, -0.45f, 0.45f);
        thrustToPathZ = Mathf.Clamp(predictedLandingCoordinate.z * 0.035f, -0.45f, 0.45f);

        Vector2 autoThrustDirecion = new Vector2(-transform.up.x - thrustToPathX, -transform.up.z - thrustToPathZ);

        Vector2 thrustDirection = autoThrustDirecion - new Vector2(-rb.angularVelocity.z, rb.angularVelocity.x) * 0.5f - new Vector2(rb.velocity.x, rb.velocity.z).normalized * Mathf.Clamp((10 - Vector3.Distance(transform.position, landPosition)) * 0.05f, 0, 0.2f); //dampens the swaying back and fourth

        thrustDir = transform.TransformVector(Vector3.ClampMagnitude(new Vector3(-thrustDirection.x, 1, -thrustDirection.y), 1));

        float forceOfRocket = 0.75f * thrustForce - 9.81f;
        float a = forceOfRocket;
        float b = -rb.velocity.magnitude;
        float c = transform.position.y - landPosition.y; //add a constant to keep the rocket a bit lower offset when engines turn off
        float d = transform.position.y - landPosition.y - new Vector2(predictedLandingCoordinate.x, predictedLandingCoordinate.z).magnitude * 0.5f - thrustDirection.magnitude * 0.5f;
        predictedLandingCoordinate.y = -Mathf.Pow(b, 2) / (4 * a) + c;
        predictedEngineStart = -Mathf.Pow(b, 2) / (4 * a) + d;
        predictedSecond = -b / (2 * a);

        //control throttle
        if (predictedLandingCoordinate.y > 0) throttle = 0.65f;
        else if (predictedLandingCoordinate.y < 0) throttle = 1f;

        if (rb.velocity.magnitude < 1.5f || (Vector3.Distance(landPosition, transform.position) < 5f && rb.velocity.y > -0.1f)) { thrusting = false; dead = true; }
        else if (predictedLandingCoordinate.y > 5f) { thrusting = false; }
        else if (predictedEngineStart < 0f && !dead) thrusting = true;

        if (Input.GetKeyDown(KeyCode.Space)) { manualThrusting = true; dead = false; }
        else if (Input.GetKeyUp(KeyCode.Space)) manualThrusting = false;

        if (thrusting || manualThrusting)
        {
            locationOfThrust.GetComponentInChildren<ParticleSystem>().Play();
            rb.AddForceAtPosition(throttle * 50 * thrustDir * Time.deltaTime * thrustForce, locationOfThrust.transform.position, ForceMode.Acceleration);
        }
        else
        {
            locationOfThrust.GetComponentInChildren<ParticleSystem>().Stop();
        }

        locationOfThrust.transform.localRotation = Quaternion.EulerAngles(Mathf.Clamp(-thrustDirection.y, -1, 1), 0, Mathf.Clamp(thrustDirection.x, -1, 1));

        text.text = "" + predictedSecond.ToString("#.0"); ;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(predictedLandingCoordinate.x, predictedLandingCoordinate.y, predictedLandingCoordinate.z), 0.5f);
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(new Vector3(predictedLandingCoordinate.x, predictedEngineStart, predictedLandingCoordinate.z), 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(new Vector3(thrustToPathX, transform.position.y, thrustToPathZ), 0.2f);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(transform.up.x, -0.1f, transform.up.z).normalized * 20);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, new Vector3(predictedLandingCoordinate.x, predictedEngineStart, predictedLandingCoordinate.z));
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + new Vector3(thrustDir.x, -0.1f, thrustDir.z).normalized * 20, 1f);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(thrustDir.x, -0.1f, thrustDir.z).normalized * 20);
    }
}
