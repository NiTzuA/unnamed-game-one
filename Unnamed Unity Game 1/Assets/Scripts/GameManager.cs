using UnityEngine;
using TMPro;


public class GameManager : MonoBehaviour
{

    public TMP_Text velocity;
    
    public Rigidbody playerBodyRigidBody;

    void Start()
    {
        
    }

    void Update()
    {
        GetCurrentSpeed();
    }

    private void GetCurrentSpeed()
    {
        velocity.text = "Velocity: " + playerBodyRigidBody.linearVelocity.magnitude.ToString("F5");
    }
}
