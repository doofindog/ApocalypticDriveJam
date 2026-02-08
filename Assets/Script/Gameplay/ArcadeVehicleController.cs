using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace ArcadeVP
{
    public class ArcadeVehicleController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference movementInput;
        [SerializeField] private InputActionReference breakInput;
        [SerializeField] private InputActionReference boostInput;
        
        [Header("Stats")]
        public SerializedFloat health;
        public SerializedFloat boostFuel;
        
            
        public enum groundCheck { rayCast, sphereCaste };
        public enum MovementMode { Velocity, AngularVelocity };
        public MovementMode movementMode;
        public groundCheck GroundCheck;
        public LayerMask drivableSurface;

        public float MaxSpeed, accelaration, turn, gravity = 7f, downforce = 5f;
        public bool AirControl = false;
        public Rigidbody rb, carBody;
        public float breakValue;
        public float boostValue;

        [Header("Health")] 
        public float maxHealth;

        [Header("Boost")] 
        public float boostComsumptionRate;
        public float boostFillRate;
        public float maxBoostFuel;
        

        [HideInInspector]
        public RaycastHit hit;
        public AnimationCurve frictionCurve;
        public AnimationCurve turnCurve;
        public PhysicsMaterial frictionMaterial;
        [Header("Visuals")]
        public Transform BodyMesh;
        public Transform[] FrontWheels = new Transform[2];
        public Transform[] RearWheels = new Transform[2];
        [HideInInspector]
        public Vector3 carVelocity;

        [Range(0, 10)]
        public float BodyTilt;
        [Header("Audio settings")]
        public AudioSource engineSound;
        [Range(0, 1)]
        public float minPitch;
        [Range(1, 3)]
        public float MaxPitch;
        public AudioSource SkidSound;

        [HideInInspector]
        public float skidWidth;


        private float radius, horizontalInput, verticalInput;
        private Vector3 origin;
        
        private CameraShake cameraShake;

        public bool isDestroyed;

        [Header("SFX")]
        [SerializeField] private GameObject blackSmokeSFX;
        [SerializeField] private GameObject fireSFX;

        public Action PlayerDestroyed;
        
        private void Start()
        {
            radius = rb.GetComponent<SphereCollider>().radius;
            if (movementMode == MovementMode.AngularVelocity)
            {
                Physics.defaultMaxAngularSpeed = 100;
            }
            
            if(Camera.main)
                cameraShake ??= Camera.main.GetComponent<CameraShake>();

            boostInput.action.started += OnBoostPressed;
            boostInput.action.performed += OnBoostPressed;
            boostInput.action.canceled += OnBoostPressed;

            boostFuel.Value = maxBoostFuel;
            health.Value = maxHealth;
        }

        private void OnDestroy()
        {
            boostInput.action.started -= OnBoostPressed;
            boostInput.action.performed -= OnBoostPressed;
            boostInput.action.canceled -= OnBoostPressed;
        }

        private void OnBoostPressed(InputAction.CallbackContext obj)
        {
            if (obj.performed)
            {
                Debug.Log("Perform Boost");
                boostValue = 1;
                if (boostFuel.Value > 0)
                    cameraShake.Shake(2, 0.5f);
            }

            if (obj.canceled)
            {
                Debug.Log("Cancelled Boost");
                boostValue = 0;
            }
        }

        private void Update()
        {
            if (!isDestroyed)
            {
                Vector2 movement =  movementInput.action.ReadValue<Vector2>();
                horizontalInput = movement.x; //turning input
                verticalInput = movement.y;     //accelaration input
            }
            
            
            HandleBoost();
            Visuals();
            AudioHandler();
        }

        private void HandleBoost()
        {
            switch (boostValue)
            {
                case > 0 when boostFuel.Value > 0:
                    boostFuel.Value -= boostComsumptionRate * Time.deltaTime;
                    boostFuel.Value = Mathf.Clamp(boostFuel.Value, 0, maxBoostFuel);
                    break;
                case 0 when boostFuel.Value < maxBoostFuel:
                    boostFuel.Value += Time.deltaTime * boostFillRate;
                    boostFuel.Value = Mathf.Clamp(boostFuel.Value, 0, maxBoostFuel);
                    break;
            }
        }
        public void AudioHandler()
        {
            engineSound.pitch = Mathf.Lerp(minPitch, MaxPitch, Mathf.Abs(carVelocity.z) / MaxSpeed);
            if (Mathf.Abs(carVelocity.x) > 10 && grounded())
            {
                SkidSound.mute = false;
            }
            else
            {
                SkidSound.mute = true;
            }
        }


        void FixedUpdate()
        {
            carVelocity = carBody.transform.InverseTransformDirection(carBody.linearVelocity);

            if (Mathf.Abs(carVelocity.x) > 0)
            {
                //changes friction according to sideways speed of car
                frictionMaterial.dynamicFriction = frictionCurve.Evaluate(Mathf.Abs(carVelocity.x / 100));
            }


            if (grounded())
            {
                //turnlogic
                float sign = Mathf.Sign(carVelocity.z);
                float TurnMultiplyer = turnCurve.Evaluate(carVelocity.magnitude / MaxSpeed);
                if (verticalInput > 0.1f || carVelocity.z > 1)
                {
                    carBody.AddTorque(Vector3.up * horizontalInput * sign * turn * 100 * TurnMultiplyer);
                }
                else if (verticalInput < -0.1f || carVelocity.z < -1)
                {
                    carBody.AddTorque(Vector3.up * horizontalInput * sign * turn * 100 * TurnMultiplyer);
                }

                //== brakelogic ===
                rb.constraints = breakValue > 0.1f ? RigidbodyConstraints.FreezeRotationX : RigidbodyConstraints.None;
                
                //=== accelaration logic ===
                if (movementMode == MovementMode.AngularVelocity)
                {
                    if (Mathf.Abs(verticalInput) > 0.1f)
                    {
                        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, carBody.transform.right * verticalInput * MaxSpeed / radius, accelaration * Time.deltaTime);
                    }
                }
                else if (movementMode == MovementMode.Velocity)
                {
                    float speed = boostValue > 0.1 ? MaxSpeed * 2 : MaxSpeed;
                    
                    if (Mathf.Abs(verticalInput) > 0.1f && breakValue < 0.1f)
                    {
                        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, carBody.transform.forward * (verticalInput * speed), accelaration / 10 * Time.deltaTime);
                    }
                }

                //=== down force ===
                rb.AddForce(-transform.up * downforce * rb.mass);

                //=== body tilt ===
                carBody.MoveRotation(Quaternion.Slerp(carBody.rotation, Quaternion.FromToRotation(carBody.transform.up, hit.normal) * carBody.transform.rotation, 0.12f));
            }
            else
            {
                if (AirControl)
                {
                    //=== turnlogic ===
                    float TurnMultiplyer = turnCurve.Evaluate(carVelocity.magnitude / MaxSpeed);

                    carBody.AddTorque(Vector3.up * horizontalInput * turn * 100 * TurnMultiplyer);
                }

                carBody.MoveRotation(Quaternion.Slerp(carBody.rotation, Quaternion.FromToRotation(carBody.transform.up, Vector3.up) * carBody.transform.rotation, 0.02f));
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, rb.linearVelocity + Vector3.down * gravity, Time.deltaTime * gravity);
            }

        }
        public void Visuals()
        {
            //tires
            foreach (Transform FW in FrontWheels)
            {
                FW.localRotation = Quaternion.Slerp(FW.localRotation, Quaternion.Euler(FW.localRotation.eulerAngles.x,
                                   30 * horizontalInput, FW.localRotation.eulerAngles.z), 0.1f);
                FW.GetChild(0).localRotation = rb.transform.localRotation;
            }
            RearWheels[0].localRotation = rb.transform.localRotation;
            RearWheels[1].localRotation = rb.transform.localRotation;

            //Body
            if (carVelocity.z > 1 )
            {
                BodyMesh.localRotation = Quaternion.Slerp(BodyMesh.localRotation, Quaternion.Euler(Mathf.Lerp(0, -5, carVelocity.z / MaxSpeed),
                                   BodyMesh.localRotation.eulerAngles.y, BodyTilt * horizontalInput), 0.05f);
            }
            else
            {
                BodyMesh.localRotation = Quaternion.Slerp(BodyMesh.localRotation, Quaternion.Euler(0, 0, 0), 0.05f);
            }


        }

        public bool grounded() //checks for if vehicle is grounded or not
        {
            origin = rb.position + rb.GetComponent<SphereCollider>().radius * Vector3.up;
            var direction = -transform.up;
            var maxdistance = rb.GetComponent<SphereCollider>().radius + 0.2f;

            if (GroundCheck == groundCheck.rayCast)
            {
                if (Physics.Raycast(rb.position, Vector3.down, out hit, maxdistance, drivableSurface))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            else if (GroundCheck == groundCheck.sphereCaste)
            {
                if (Physics.SphereCast(origin, radius + 0.1f, direction, out hit, maxdistance, drivableSurface))
                {
                    return true;

                }
                else
                {
                    return false;
                }
            }
            else { return false; }
        }

        private void OnDrawGizmos()
        {
            //debug gizmos
            radius = rb.GetComponent<SphereCollider>().radius;
            float width = 0.02f;
            if (!Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(rb.transform.position + ((radius + width) * Vector3.down), new Vector3(2 * radius, 2 * width, 4 * radius));
                if (GetComponent<BoxCollider>())
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider>().size);
                }

            }
        }

        public void TakeDamage(float damage)
        {
            if(health.Value <= 0)
                return;
            
            AudioManager.Instance.PlaySFX(AudioManager.Instance.carDamage);
            
            health.Value -= damage;
            if (health.Value <= 20)
            {
                blackSmokeSFX.SetActive(true);
            }
            
            if (health.Value <= 0)
            {
                DestroyCar();
            }
        }

        private void DestroyCar()
        {
            fireSFX.SetActive(true);
            isDestroyed = true;
            AudioManager.Instance.PlaySFX(AudioManager.Instance.carExplode);

            horizontalInput = 0;
            verticalInput = 0;
            breakValue = 0;
            boostValue = 0;
            
            GameManager.Instance.GameOverEvent?.Invoke();
        }
    }
}
