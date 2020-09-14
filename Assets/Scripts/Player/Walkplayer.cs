using UnityEngine;

#pragma warning disable CS0618

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Walkplayer : MonoBehaviour
{
    [Header("Camera")]
    public Camera cam;
    public float cameraSpeed = 1.0f;
    public float clamp = 90;

    private float _mouseX, _mouseY;
    private float _cameraPitch, _cameraYaw;

    public float cameraPitch 
    { 
        set
        {
            _cameraPitch = value;

            if (Mathf.Abs(_cameraPitch) > clamp)
                _cameraPitch = _cameraPitch > 0 ? clamp : -clamp;
        }

        get => _cameraPitch;
    }
    public float cameraYaw
    {
        set
        {
            _cameraYaw = value;
            _cameraYaw %= 360;
        }

        get => _cameraYaw;
    }

    [Header("Movement")]
    public float movementSpeed = 1.0f;
    public float sprintMultiplier = 1.50f;
    public float jumpStrength = 5.0f;

    private float _vertical, _horizontal;

    public bool isSprinting { get; set; }
    public bool isAiming { get; set; }

    [Space]
    [Range(0.1f, 1.0f)]
    public float dragCoefficient = 0.5f;

    private Rigidbody _rigidbody;
    private Collider _collider;

    // Start is called before the first frame update
    void Start()
    {
        cam.enabled = true;
        cam.GetComponent<AudioListener>().enabled = true;

        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called for frame independent work
    void FixedUpdate()
    {
        Movement();
    }

    // Update is called once per frame
    void Update()
    {
        _mouseX = Input.GetAxis("Mouse X");
        _mouseY = Input.GetAxis("Mouse Y");

        _horizontal = Input.GetAxis("Horizontal");
        _vertical = Input.GetAxis("Vertical");

        isAiming = Input.GetKey(KeyCode.Mouse1);
        isSprinting = Input.GetKey(KeyCode.LeftShift) && !isAiming;

        Camera();

        if (Input.GetButtonDown("Jump") && isGrounded())
            Jump();
    }

    void Camera()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            cameraYaw += _mouseX * cameraSpeed;
            cameraPitch -= _mouseY * cameraSpeed;

            transform.localRotation = Quaternion.Euler(0, cameraYaw, 0);
            cam.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
        }
    }

    void Movement()
    {
        if (Cursor.lockState != CursorLockMode.None)
        {
            Vector3 direction = new Vector3(_horizontal, 0, _vertical);

            if (direction.x != 0 || direction.z != 0)
            {
                if (direction.magnitude > 1)
                    direction.Normalize();

                float movementSpeed = this.movementSpeed * (isSprinting ? sprintMultiplier : 1.0f);

                _rigidbody.AddRelativeForce(direction * (_rigidbody.mass * movementSpeed / Time.fixedDeltaTime), ForceMode.Force);
            }
        }

        _rigidbody.velocity -= new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z) * dragCoefficient;
    }

    void Jump()
    {
        _rigidbody.AddRelativeForce(Vector3.up * _rigidbody.mass * jumpStrength, ForceMode.Impulse);
    }

    bool isGrounded()
    {
        return Physics.CheckCapsule(
            _collider.bounds.center,
            new Vector3(_collider.bounds.center.x, _collider.bounds.min.y-0.1f, _collider.bounds.center.z),
            0.1f,
            LayerMask.GetMask("Default")
        );
    }
}
