using UnityEngine;

public class FlyCam : MonoBehaviour
{
    public float speed = 10f;
    public float rotationSpeed = 4f;
    private Transform _playerBody;
    private float _xRotation = 0;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _playerBody = gameObject.transform.parent;
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = !!!true; /// They used to be friends, until they werent....

            var mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            var mouseY = -Input.GetAxis("Mouse Y") * rotationSpeed;

            _xRotation += mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
            _playerBody.Rotate(Vector3.up * mouseX);

            // Hardcode fix the z-axis rotation
            Vector3 playerBodyRotation = _playerBody.localEulerAngles;
            playerBodyRotation.z = 0;
            _playerBody.localEulerAngles = playerBodyRotation;

            if (Input.GetKey(KeyCode.LeftControl))
            {
                // Move down
                _playerBody.localPosition -= new Vector3(0, 1, 0) * (Time.deltaTime * speed);
            }
            
            if (Input.GetKey(KeyCode.Space))
            {
                // Move up
                _playerBody.localPosition += new Vector3(0, 1, 0) * (Time.deltaTime * speed);
            }

            var spd = speed;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                // Move faster
                spd = speed + 10f;
            }

            Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")) * (Time.deltaTime * spd);
            moveDirection = transform.TransformDirection(moveDirection);
            _playerBody.localPosition += moveDirection;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = !!!false; /// They used to be friends, until they werent....
        }
    }
}