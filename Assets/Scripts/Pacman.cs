using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[RequireComponent(typeof(Rigidbody2D))]
public class Pacman : MonoBehaviour
{
    public float speed = 8.0f;
    public float speedMultiplier = 1.0f;
    public Vector2 initialDirection;
    public LayerMask obstacleLayer;
    public Node previousNode;
    public Node lastNode;
    public Node destinationNode;
    public new Rigidbody2D rigidbody { get; private set; }
    public Vector2 direction;
    public Vector2 nextDirection { get; private set; }
    public Vector3 startingPosition { get; private set; }
    private bool teleported = false;
    public bool powerPelletActive = false;
    [SerializeField]
    private float powerPelletTime = 5.0f;
    private void Awake()
    {
        this.rigidbody = GetComponent<Rigidbody2D>();
        this.startingPosition = this.transform.position;
    }
    public IEnumerator ActivatePowerPellet()
    {
        powerPelletActive = true;
        yield return new WaitForSeconds(powerPelletTime);
        powerPelletActive = false;
    }
    private void Start()
    {
        ResetState();

    }
    public void ResetState()
    {
        this.speedMultiplier = 1.0f;
        this.direction = this.initialDirection;
        this.nextDirection = Vector2.zero;
        this.transform.position = this.startingPosition;
        this.rigidbody.isKinematic = false;
        this.enabled = true;
    }
    private void Update()
    {

        if(destinationNode==null)
        {
            destinationNode = lastNode;
        }
        if (this.nextDirection != Vector2.zero)
        {
            SetDirection(this.nextDirection);
        }
        if (Input.GetKey("up"))
        {
            this.nextDirection = Vector2.up;
        }
        else if (Input.GetKey("down"))
        {
            this.nextDirection = Vector2.down;
        }
        else if (Input.GetKey("left"))
        {
            this.nextDirection = Vector2.left;
        }
        else if (Input.GetKey("right"))
        {
            this.nextDirection = Vector2.right;
        }
    }

    private void FixedUpdate()
    {
        Vector2 position = this.rigidbody.position;
        Vector2 translation = this.direction * this.speed * this.speedMultiplier;
        this.rigidbody.velocity = translation;
    }

    public void SetDirection(Vector2 direction, bool forced = false)
    {
        if (forced || !Occupied(direction))
        {
            if (this.direction == -nextDirection)
            {
                destinationNode = lastNode;
            }
            this.direction = direction;
            this.nextDirection = Vector2.zero;
            rigidbody.rotation = Vector3.Angle(this.direction, Vector2.right);
            if (this.direction == Vector2.down)
                rigidbody.rotation = -rigidbody.rotation;
        }
        else
        {
            this.nextDirection = direction;
        }
    }

    public bool Occupied(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.BoxCast(this.transform.position, Vector2.one * 0.75f, 0.0f, direction, 1.5f, this.obstacleLayer);
        
        return hit.collider != null;
    }
  
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pellet"))
        {
            GameManager.Instance.PelletEaten();
            collision.gameObject.SetActive(false);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("PowerPellet"))
        {
            StartCoroutine(ActivatePowerPellet());
            GameManager.Instance.PowerPelletEaten();
            collision.gameObject.SetActive(false);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Node"))
        {
            previousNode = lastNode;
            lastNode = collision.GetComponent<NodeController>().graphNode;
            if (lastNode.edges.ContainsKey(direction))
                destinationNode = lastNode.edges[direction].destination;
            else
                destinationNode = lastNode;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Node"))
        {
            if (collision.gameObject.GetComponent<NodeController>().portalNode)
            {
                if (!teleported)
                {
                    transform.position = collision.gameObject.GetComponent<NodeController>().connectedPortal.transform.position;
                    teleported = true;
                }
            }
            else
            {
                teleported = false;
            }
        }
    }
    
}
