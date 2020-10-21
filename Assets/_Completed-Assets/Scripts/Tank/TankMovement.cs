using UnityEngine;

public class TankMovement : MonoBehaviour
{
    public int m_PlayerNumber;
    public float m_Speed = 12f;
    public float m_TurnSpeed = 180f;
    public AudioSource m_MovementAudio;
    public AudioClip m_EngineIdling;
    public AudioClip m_EngineDriving;
    public float m_PitchRange = 0.2f;
    public bool m_isCPU;
    public bool isBoss;
    [HideInInspector] public Transform[] m_Targets;

    private string m_MovementAxisName;
    private string m_TurnAxisName;
    private Rigidbody m_Rigidbody;
    private float m_MovementInputValue;
    private float m_TurnInputValue;
    private float m_OriginalPitch;
    private Vector3 currentVelocity;
    private float smoothTime = 1f;


    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }


    private void OnEnable()
    {
        m_Rigidbody.isKinematic = false;
        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;
    }

    private void OnDisable()
    {
        m_Rigidbody.isKinematic = true;
    }


    private void Start()
    {
        m_MovementAxisName = "Vertical" + m_PlayerNumber;
        m_TurnAxisName = "Horizontal" + m_PlayerNumber;

        m_OriginalPitch = m_MovementAudio.pitch;
    }


    private void Update()
    {
        // Store the player's input and make sure the audio for the engine is playing.

        if (!m_isCPU)
        {
            m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
            m_TurnInputValue = Input.GetAxis(m_TurnAxisName);
        }
        else
        {
            float rand = Random.value;

            if (rand < 5f * Time.deltaTime)
            {
                m_MovementInputValue = (isBoss) ? Random.Range(-2, 3) : Random.Range(-1, 2);
                m_TurnInputValue = (isBoss) ? Random.Range(-2, 3) : Random.Range(-1, 2);
            }
        }

        Resize();

        EngineAudio();
    }

    private void Resize()
    {
        if (isBoss)
        {
            Enlarge();
        }
        else
        {
            ResetSize();
        }
    }

    private void Enlarge()
    {
        transform.localScale = Vector3.SmoothDamp(transform.localScale, new Vector3(2, 2, 2), ref currentVelocity, smoothTime);
    }

    public void ResetSize()
    {
        transform.localScale = new Vector3(1, 1, 1);
    }

    private void EngineAudio()
    {
        // Play the correct audio clip based on whether or not the tank is moving and what audio is currently playing.
        if (Mathf.Abs(m_MovementInputValue) < 0.1f && Mathf.Abs(m_TurnInputValue) < 0.1f)
        {
            if (m_MovementAudio.clip == m_EngineDriving)
            {
                m_MovementAudio.clip = m_EngineIdling;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();

            }
        }
        else
        {
            if (m_MovementAudio.clip == m_EngineIdling)
            {
                m_MovementAudio.clip = m_EngineDriving;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
    }


    private void FixedUpdate()
    {
        // Move and turn the tank.
        Move();
        Turn();
    }


    private void Move()
    {
        // Adjust the position of the tank based on the player's input.
        Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;
        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
    }


    private void Turn()
    {
        // Adjust the rotation of the tank based on the player's input.

        if (!m_isCPU)//!isBoss)
        {
            float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);

        }
        else
        {
            Vector3 targetDirection = m_Targets[1].position - transform.position;
            Quaternion turnRotation = Quaternion.LookRotation(targetDirection);
            m_Rigidbody.MoveRotation(turnRotation);
        }
    }
}

/*
       Vector3 desiredForward = Vector3.RotateTowards(transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
       m_Rotation = Quaternion.LookRotation(desiredForward);
   }

   void OnAnimatorMove()
   {
       m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * m_Animator.deltaPosition.magnitude);
       m_Rigidbody.MoveRotation(m_Rotation);
   }
*/
