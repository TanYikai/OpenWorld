using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

public class OwnThirdPersonController : MonoBehaviour {


    public FixedJoystick LeftJoystick;
    public FixedButton ShootButton;
    public FixedButton JumpButton;
    public FixedButton CrouchButton;
    public FixedTouchField TouchField;
    public float JumpForce = 5f;
    protected Actions Actions;
    protected PlayerController PlayerController;
    protected Rigidbody Rigidbody;
    protected ParticleSystem ParticleSystem;

    protected float CameraAngleY;
    protected float CameraAngleSpeed = 0.1f;
    protected float CameraPosY = 3f;
    protected float CameraPosSpeed = 0.02f;

    protected bool isCrouching = false;
    protected CapsuleCollider CapCollider;

    //car
    protected CarController carController;
    protected SkinnedMeshRenderer Renderer;
    public FixedButton CarButton;

    protected float Cooldown;

    // Use this for initialization
    void Start ()
    {
        Actions = GetComponent<Actions>();
        PlayerController = GetComponent<PlayerController>();
        Rigidbody = GetComponent<Rigidbody>();
        ParticleSystem = GetComponentInChildren<ParticleSystem>();
        CapCollider = GetComponent<CapsuleCollider>();
        Renderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }
	
	// Update is called once per frame
	void Update () {

        if (carController == null)
        {
            Walk();
            Shoot();
            Jump();
            Crouch();
        }
        CarUpdate();
    }

    private void CarUpdate()
    {
        if (carController == null)
        {

            //check if the player wants to get into a car
            if (CarButton.Pressed)
            {
                CarButton.Pressed = false;
                var colliders = Physics.OverlapSphere(transform.position, 2f);
                foreach (var collider in colliders)
                {
                    var car = collider.GetComponent<CarController>();
                    if (car != null)
                    {
                        carController = car;
                        Renderer.gameObject.SetActive(false);
                    }
                }
            }

        }
        else
        {
            //drive the car
            carController.Move(LeftJoystick.Horizontal + Input.GetAxis("Horizontal"), LeftJoystick.Vertical + Input.GetAxis("Vertical"), LeftJoystick.Vertical + Input.GetAxis("Vertical"), 0);
            transform.position = carController.transform.position;

            Camera.main.transform.position = carController.transform.position + carController.transform.rotation * new Vector3(0, 7, -10);
            Camera.main.transform.rotation = Quaternion.LookRotation(carController.transform.position + Vector3.up * 2f - Camera.main.transform.position, Vector3.up);

            //get out
            if (CarButton.Pressed)
            {
                CarButton.Pressed = false;
                carController = null;
                Renderer.gameObject.SetActive(true);
            }
        }
    }

    private void Crouch()
    {
        var crouchbutton = CrouchButton.Pressed || Input.GetKey(KeyCode.C);

        if (!isCrouching && crouchbutton)
        {
            //crouch
            CapCollider.height = 0.5f;
            CapCollider.center = new Vector3(CapCollider.center.x, 0.25f, CapCollider.center.z);
            isCrouching = true;
            Actions.Sitting(true);
        }

        Debug.DrawRay(transform.position, Vector3.up * 2f, Color.green);
        if (isCrouching && !crouchbutton)
        {
            //try to stand up
            var cantStandUp = Physics.Raycast(transform.position, Vector3.up, 2f);

            if (!cantStandUp)
            {
                CapCollider.height = 1f;
                CapCollider.center = new Vector3(CapCollider.center.x, 0.5f, CapCollider.center.z);
                isCrouching = false;
                Actions.Sitting(false);
            }
        }
    }

    private void Walk()
    {
        //Control.m_Jump = Button.Pressed;
        //Control.Hinput = LeftJoystick.inputVector.x;
        //Control.Vinput = LeftJoystick.inputVector.y;
        var input = new Vector3(LeftJoystick.inputVector.x, 0, LeftJoystick.inputVector.y);

        var vel = Quaternion.AngleAxis(CameraAngleY + 180, Vector3.up) * input * 5f;
        transform.rotation = Quaternion.AngleAxis(CameraAngleY + Vector3.SignedAngle(Vector3.forward, input.normalized + Vector3.forward * 0.0001f, Vector3.up) + 180, Vector3.up);
        Rigidbody.velocity = new Vector3(vel.x, Rigidbody.velocity.y, vel.z);

        CameraAngleY += TouchField.TouchDist.x * CameraAngleSpeed;
        CameraPosY = Mathf.Clamp(CameraPosY - TouchField.TouchDist.y * CameraPosSpeed, 0, 5f);

        Camera.main.transform.position = transform.position + Quaternion.AngleAxis(CameraAngleY, Vector3.up) * new Vector3(0, CameraPosY, 4);
        Camera.main.transform.rotation = Quaternion.LookRotation(transform.position + Vector3.up * 2f - Camera.main.transform.position, Vector3.up);

    }

    private void Shoot()
    {

        Cooldown -= Time.deltaTime;
        if (Cooldown < 0)
            ParticleSystem.Stop();

        var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        Debug.DrawRay(ray.origin, ray.direction, Color.red);

        if (ShootButton.Pressed)
        {
            PlayerController.SetArsenal("Rifle");
            Actions.Attack();

            if (Cooldown <= 0f)
            {
                Cooldown = 0.3f;
                ParticleSystem.Play();

                RaycastHit hitinfo;
                if (Physics.Raycast(ray, out hitinfo))
                {
                    var other = hitinfo.collider.GetComponent<Shootable>();
                    if (other != null)
                    {
                        other.GetComponent<Rigidbody>().AddForceAtPosition((hitinfo.point - ParticleSystem.transform.position).normalized * 500f, hitinfo.point);
                    }
                }

            }
        }
        else
        {

            if (Rigidbody.velocity.magnitude > 3f)
                Actions.Run();
            else if (Rigidbody.velocity.magnitude > 0.5f)
                Actions.Walk();
            else
                Actions.Stay();
        }
    }

    private void Jump()
    {

        var grounded = Physics.Raycast(transform.position + Vector3.up * 0.05f, Vector3.down, 0.1f);
        Debug.DrawRay(transform.position + Vector3.up * 0.05f, Vector3.down, Color.red, 0.1f);

        if (JumpButton.Pressed && grounded)
        {
            Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, JumpForce, Rigidbody.velocity.z);
            Actions.Jump();
        }

    }
}
/*
var crouchbutton = CrouchButton.Pressed || Input.GetKey(KeyCode.C);

        if (!isCrouching && crouchbutton)
        {
            //croch
            CapCollider.height = 0.5f;
            CapCollider.center = new Vector3(CapCollider.center.x, 0.25f, CapCollider.center.z);
isCrouching = true;
            Actions.Sitting(true);
        }

        Debug.DrawRay(transform.position, Vector3.up* 2f, Color.green);
        if (isCrouching && !crouchbutton)
        {
            //try to stand up
            var cantStandUp = Physics.Raycast(transform.position, Vector3.up, 2f);

            if (!cantStandUp)
            {
                CapCollider.height = 1f;
                CapCollider.center = new Vector3(CapCollider.center.x, 0.5f, CapCollider.center.z);
isCrouching = false;
                Actions.Sitting(false);
            }
        }
        */