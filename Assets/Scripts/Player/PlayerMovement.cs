using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerCore))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController _controller;
    //private PlayerCore _playerCore;

    [SerializeField] private Transform _camera;
    [Space]
    [Tooltip("Used to keep player on the ground and to slide down.")]
    [SerializeField] private float _gravity = -30f;
    [SerializeField] private float _moveSpeed = 5f;
    private Vector3 _playerVelocity;
    [Space]
    [Header("Turning")]
    [SerializeField] private float _turnSmoothTime = 0.1f;
    private float _turnSmoothVelocity;
    [Space]
    [Header("Jumping")]
    private bool _jumpInput = false;
    [SerializeField] private float _jumpMaxSpeed = 3f;
    [SerializeField] private float _jumpTimeToMaxSpeed = 2f;
    private bool _jumpBackdoor = true;
    private float _jumpTimer;
    [SerializeField] private float _jumpLimitter = 2f;
    private float _jumpTopSpeed;
    [Space]
    [Header("Falling")]
    //[SerializeField] private float _fallMaxSpeed = 10f;
    [SerializeField] private float _fallTimeToMaxSpeed = 2f;
    [SerializeField] private bool _fallOnOff = true;
    private float _fallTimer = 0f;
    [Space]
    [SerializeField] private bool _raycastParentingOnOff = true;
    [Space]
    [Header("Sliding")]
    [Range(0.1f, 1.4f)]
    [SerializeField] private float _slidingStartLimit = 0.8f;
    [Space]
    [Header("2d Raycast Circle")]
    [SerializeField] private LayerMask _raycastedLayers;
    [SerializeField] private int _2dRaycastCircleRays = 8;
    [SerializeField] private float _2dRaycastCircleRadius = 0.5f;
    [SerializeField] private float _2dRaycastCircleDownMaxDistance = 0.4f;
    [Header("Player parenting")]
    [SerializeField] private float _parentVelocityFollowFix = 6f;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        //_playerCore = GetComponent<PlayerCore>();
    }

    public void MovementCore(Vector2 direction, bool jumping)
    {
        _jumpInput = jumping;
        //Vector2 direction = inputCore._inputActions.Player.Move.ReadValue<Vector2>();
        if (direction.magnitude > 0.1f) { PlayerBasicMovementControll(direction); }
        _controller.Move(Vector3.down * 0.1f);
        if (!_controller.isGrounded && _fallOnOff) { PlayerFallControll(); }
        else { _fallTimer = 0f; }
        PlayerSliding();
        PlayerJumpControll();
    }

    private void LateUpdate()
    {
        if (_raycastParentingOnOff) { PlayerParentingControll(); }
        if (ParentAnimatorVelocity().magnitude != 0) { _controller.Move(ParentAnimatorVelocity() * Time.deltaTime); }

    }

    private void PlayerFallControll()
    {
        print("fall");
        if (_fallTimer < _fallTimeToMaxSpeed) { _fallTimer += Time.deltaTime; }
        float trueFallSpeed = /*_fallMaxSpeed*/_gravity * (_fallTimer / _fallTimeToMaxSpeed);
        print(trueFallSpeed);
        Vector3 vector = new Vector3(0f, trueFallSpeed, 0f);
        _controller.Move(vector * Time.deltaTime);
    }

    private void PlayerParentingControll()
    {
        transform.parent = Raycast2dCircleDown().transform;
    }

    private RaycastHit Raycast2dCircleDown()
    {
        RaycastHit trueHit;
        float previousDistance = Mathf.Infinity;

        trueHit = SingleRaycast(transform.position, Vector3.down, _2dRaycastCircleDownMaxDistance);
        for (int i = 1; i < _2dRaycastCircleRays + 1; i++)
        {
            Quaternion rotation = Quaternion.AngleAxis((360 * ((float)i / _2dRaycastCircleRays)), Vector3.down) * transform.rotation;
            Vector3 rotatedPosition = transform.position + rotation * transform.forward * _2dRaycastCircleRadius;
            Debug.DrawLine(transform.position, rotatedPosition, Color.yellow);

            RaycastHit hit = SingleRaycast(rotatedPosition, Vector3.down, _2dRaycastCircleDownMaxDistance);
            if (hit.transform && hit.distance < previousDistance)
            {
                trueHit = hit;
                previousDistance = hit.distance;
                Debug.DrawLine(rotatedPosition, rotatedPosition + new Vector3(0f, -hit.distance, 0f), Color.red);
            }
        }
        return (trueHit);
    }

    private RaycastHit SingleRaycast(Vector3 position, Vector3 direction, float distance)
    {
        Physics.Raycast(position, transform.TransformDirection(direction), out RaycastHit hit, distance, _raycastedLayers);
        return (hit);
    }

    private void PlayerSliding()
    {
        RaycastHit hit;
        Vector3 normal;
        hit = Raycast2dCircleDown();
        
        normal = hit.normal;
        if (hit.transform)
        {
            Debug.DrawLine(hit.transform.position, hit.transform.position + normal * 50, Color.red);
        }
        
        if (_controller.isGrounded && (Mathf.Abs(normal.x) + Mathf.Abs(normal.z)) > _slidingStartLimit)
        {
            print("sliding");
            Vector3 slideDirection = normal + Vector3.down;
            _controller.Move(slideDirection.normalized * _gravity * Time.deltaTime);
        }
    }

    private void PlayerBasicMovementControll(Vector2 direction)
    {
        float targetAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg + _camera.eulerAngles.y; /*Hard float data from movement direction + camera rotation.*/
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _turnSmoothTime); /*Smoothening the player roation.*/
        if (direction.magnitude > 0.1f) { transform.rotation = Quaternion.Euler(0f, angle, 0f); } /*Turn player towards a direction by combining camera rotation and moving direction.*/

        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward; /*Changing the forward facing direction for movement or something.*/
        _controller.Move(((moveDir * direction.magnitude) * _moveSpeed) * Time.deltaTime); /*The moving part.*/
    }

    private Vector3 ParentAnimatorVelocity()
    {
        Vector3 platformVelocity = Vector3.zero;
        RaycastHit hit = SingleRaycast(transform.position, Vector3.down, _2dRaycastCircleDownMaxDistance);

        if (hit.transform && hit.transform.CompareTag("Moving"))
        {
            Animator animator = hit.transform.GetComponent<Animator>();
            if (animator) { platformVelocity = animator.velocity * Time.deltaTime; }
        }
        return (platformVelocity);
    }

    private void PlayerJumpControll()
    {
        //bool jumpInput = _playerCore._inputActions.Player.Jump.ReadValue<float>() > 0.1f;

        if (_controller.isGrounded && _jumpInput)
        {
            _jumpTimer = 0f;
            _jumpBackdoor = true;
        }

        if (_jumpTimer < _jumpTimeToMaxSpeed && _jumpInput && _jumpBackdoor) /*Jump*/
        {
            _jumpTimer += Time.deltaTime;
            if (_fallOnOff) { _fallOnOff = false; }
            float jumpFloat = Mathf.Lerp(_jumpMaxSpeed, 0f, (_jumpTimer / _jumpTimeToMaxSpeed));
            Vector3 jumpVector = new Vector3(0f, jumpFloat, 0f);
            _jumpTopSpeed = jumpFloat;
            _controller.Move(jumpVector * Time.deltaTime);
            return;
        }
        else if (_jumpTimer > 0f && !_fallOnOff) /*Jump ending*/
        {
            if (_jumpBackdoor)
            {
                _jumpTimer = 0f;
                _jumpBackdoor = false;
            }
            _jumpTimer += Time.deltaTime;
            float momentumLoss = Mathf.Lerp(_jumpTopSpeed, 0f, (_jumpTimer / _jumpLimitter));
            Vector3 vector = new Vector3(0f, momentumLoss, 0f);
            _controller.Move(vector * Time.deltaTime);

            if (momentumLoss <= 0f)
            {
                _fallOnOff = true;
            }
        }
    }
}
