using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacMan : MonoBehaviour 
{
    public AudioClip chomp1;
    public AudioClip chomp2;

    public RuntimeAnimatorController chompAnimation;
    public RuntimeAnimatorController deathAnimation;

    public Vector2 orientation;
    public float speed = 6.0f;
    public bool canMove = true;

    public Sprite idleSprite;
	private Vector2 direction = Vector2.zero;
    private Vector2 nextDirection;

    private bool playedChomp1 = false;
    private  new AudioSource audio;

    private Node currentNode;
    private Node targetNode;
    private Node previousNode;

	private Vector3 fp;   //First touch position
	private Vector3 lp;   //Last touch position
	private float dragDistance;  //minimum distance for a swipe to be registered

    private Node startingPosition;
	// Use this for initialization
	void Start () 
	{
        audio = transform.GetComponent<AudioSource>();
        Node node = GetNodeAtPosition(transform.localPosition);
        startingPosition = node;
        MoveToStartingPosition(); // Remove this function call if something bugs in the game at start
        if (node!= null)
        {
            currentNode = node;
            //Debug.Log(currentNode);
        }
        direction = Vector2.left;
        orientation = Vector2.left;
        ChangePosition(direction);
		dragDistance = Screen.height * 10 / 100; //dragDistance is 10% height of the screen	
	}
	
    public void MoveToStartingPosition()
    {
        transform.position = startingPosition.transform.position;
        transform.GetComponent<SpriteRenderer>().sprite = idleSprite;
        direction = Vector2.left;
        orientation = Vector2.left;
        UpdateOrientation();
    }
    public void Restart()
    {
        canMove = true;
        currentNode = startingPosition;
        nextDirection = Vector2.left;
        transform.GetComponent<Animator>().runtimeAnimatorController = chompAnimation;
        transform.GetComponent<Animator>().enabled = true;
        ChangePosition(direction);
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            CheckInput();

            Move();

            UpdateOrientation();

            UpdateAnimationState();

            ConsumePellet();

        }
    }

    void PlayChompSound()
    {
        if(playedChomp1)
        {
            //- play chomp2, set playchomp1 to false;
            audio.PlayOneShot(chomp2);
            playedChomp1 = false;
        }
        else
        {
            //- Play Chomp1, set playchomp1 to true;
            audio.PlayOneShot(chomp1);
            playedChomp1 = true;
        }
    }


	void CheckInput()
    {
        if (Input.touchCount == 1) // user is touching the screen with a single touch
        {
            Touch touch = Input.GetTouch(0); // get the touch
            if (touch.phase == TouchPhase.Began) //check for the first touch
            {
                fp = touch.position;
                lp = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved) // update the last position based on where they moved
            {
                lp = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended) //check if the finger is removed from the screen
            {
                lp = touch.position;  //last touch position. Ommitted if you use list

                //Check if drag distance is greater than 10% of the screen height
                if (Mathf.Abs(lp.x - fp.x) > dragDistance || Mathf.Abs(lp.y - fp.y) > dragDistance)
                {//It's a drag
                 //check if the drag is vertical or horizontal
                    if (Mathf.Abs(lp.x - fp.x) > Mathf.Abs(lp.y - fp.y))
                    {   //If the horizontal movement is greater than the vertical movement...
                        if ((lp.x > fp.x))  //If the movement was to the right)
                        {   //Right swipe

                            ChangePosition(Vector2.right);
                            //Debug.Log("Right Swipe");
                        }
                        else
                        {   //Left swipe
                            ChangePosition(Vector2.left);
                            //Debug.Log("Left Swipe");
                            
                        }
                    }
                    else
                    {   //the vertical movement is greater than the horizontal movement
                        if (lp.y > fp.y)  //If the movement was up
                        {   //Up swipe
                            ChangePosition(Vector2.up);
                            //Debug.Log("Up Swipe");
                        }
                        else
                        {   //Down swipe
                            ChangePosition(Vector2.down);
                            //Debug.Log("Down Swipe");
                        }
                    }
                }
                else
                {   //It's a tap as the drag distance is less than 20% of the screen height
                    //Debug.Log("Tap");
                }
            }
        }
    }


    void UpdateAnimationState()
    {
        if(direction == Vector2.zero)
        {
            GetComponent<Animator>().enabled = false;
            GetComponent<SpriteRenderer>().sprite = idleSprite;
        }
        else
        {
            GetComponent<Animator>().enabled = true;
        }
    }

    void ConsumePellet()
    {
        GameObject o = GetTileAtPosition(transform.position);
        if(o!=null)
        {
            Tile tile = o.GetComponent<Tile>();

            if(tile!=null)
            {
                if(!tile.didConsume &&(tile.isPellet || tile.isSupperPellet))
                {
                    o.GetComponent<SpriteRenderer>().enabled = false;
                    tile.didConsume = true;
                    GameBoard.player1Score += 10;
                    GameObject.Find("Game").GetComponent<GameBoard>().playerPelletsConsumed++;
                    //Debug.Log("Pellets Consumed by Player: " + GameObject.Find("Game").GetComponent<GameBoard>().playerPelletsConsumed);
                    PlayChompSound();

                    if(tile.isSupperPellet)
                    {
                        GameBoard.player1Score += 50;
                        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");

                        foreach (GameObject go in ghosts)
                        {
                            go.GetComponent<ghost>().StartFrightenedMode();
                        }
                    }   

                }
            }
        }
    }

    Node CanMove(Vector2 d)
    {
        Node moveToNode = null;
        for(int i=0; i< currentNode.neighbors.Length; i ++)
        {
            GameObject tile = GetTileAtPosition(currentNode.transform.position);
            if (tile.transform.GetComponent<Tile>().isGhostHouseEntrance == false)
            {
                //Allow movement since Pacman has not reached to the ghost house entrance
             if (currentNode.validDirections[i] == d)
                {
                 moveToNode = currentNode.neighbors[i];
                  break;
                 }
            }
            else
            {
                //Cease the downside Movement, Since PacMan has reached to the entrance of the ghost house
                if (currentNode.validDirections[i] != Vector2.down)
                {
                    if(currentNode.validDirections[i] == d)
                    {
                        moveToNode = currentNode.neighbors[i];
                        break;
                    }
                }
            }
        }
        return moveToNode;
    }

    void ChangePosition(Vector2 d)
    {
        if(d != direction)
        {
            nextDirection = d;
        }
        if(currentNode != null)
        {
            Node moveToNode = CanMove(d);
            if(moveToNode!=null)
            {
                direction = d;
                targetNode = moveToNode;
                previousNode = currentNode;
                currentNode = null;
            }
        }
    }
	void Move () 
    {
        if(nextDirection == direction * -1)
        {
            direction *= -1;
            Node tempNode = targetNode;
            targetNode = previousNode;
            previousNode = tempNode;
        }

        if (targetNode!= currentNode && targetNode!=null)
        {
            if (OverShotTarget())
            {
                currentNode = targetNode;
                transform.localPosition = currentNode.transform.position;
                GameObject otherPortal = GetPortal(currentNode.transform.position);
                if(otherPortal != null)
                {
                    transform.localPosition = otherPortal.transform.position;

                    currentNode = otherPortal.GetComponent<Node>();
                }
                Node moveToNode = CanMove(nextDirection);

                if (moveToNode != null)
                    direction = nextDirection;

                if (moveToNode == null)
                    moveToNode = CanMove(direction);

                if (moveToNode != null)
                {
                    targetNode = moveToNode;
                    previousNode = currentNode;
                    currentNode = null;
                }
                else
                {
                    direction = Vector2.zero;
                }
            }
            else
            {
                transform.localPosition += (Vector3)(direction * speed) * Time.deltaTime;
            }
        }

		
	}

    void MoveToNode(Vector2 d)
    {
        Node moveToNode = CanMove(d);
        if(moveToNode!=null)
        {
            transform.localPosition = moveToNode.transform.position;
            currentNode = moveToNode;
        }
    }

	void UpdateOrientation () 
    {

		if (direction == Vector2.left) 
        {
            orientation = Vector2.left;
			transform.localScale = new Vector3 (-1, 1, 1);
			transform.localRotation = Quaternion.Euler (0, 0, 0);

		}
        else if (direction == Vector2.right) 
        {
            orientation = Vector2.right;
            transform.localScale = new Vector3 (1, 1, 1);
			transform.localRotation = Quaternion.Euler (0, 0, 0);

		} 
        else if (direction == Vector2.up) 
        {
            orientation = Vector2.up;
            transform.localScale = new Vector3 (1, 1, 1);
			transform.localRotation = Quaternion.Euler (0, 0, 90);

		} 
        else if (direction == Vector2.down) 
        {
            orientation = Vector2.down;
            transform.localScale = new Vector3 (1, 1, 1);
			transform.localRotation = Quaternion.Euler (0, 0, 270);
		}
	}

    Node GetNodeAtPosition(Vector2 pos)
    {
        GameObject tile = GameObject.Find("Game").GetComponent<GameBoard>().board[(int)pos.x, (int)pos.y];
        if(tile!= null)
        {
            return tile.GetComponent<Node>();
        }
        return null;
    }

    bool OverShotTarget()
    {
        float nodeToTarget = LengthFromNode(targetNode.transform.position);
        float nodeToSelf = LengthFromNode(transform.localPosition);
        return nodeToSelf > nodeToTarget;  
    }
    float LengthFromNode(Vector2 targetPosition)
    {
        Vector2 vec = targetPosition - (Vector2)previousNode.transform.position;
        return vec.sqrMagnitude;
    }

    GameObject GetPortal(Vector2 pos)
    {
        GameObject tile = GameObject.Find("Game").GetComponent<GameBoard>().board[(int)pos.x, (int)pos.y];
        if(tile!= null)
        {
            if (tile.GetComponent<Tile>() != null)
            {
                if (tile.GetComponent<Tile>().portal)
                {
                    GameObject otherPortal = tile.GetComponent<Tile>().portalReciever;
                    return otherPortal;
                }
            }
        }
        return null;
    }

    GameObject GetTileAtPosition(Vector2 pos)
    {
        int tileX = Mathf.RoundToInt(pos.x);
        int tileY = Mathf.RoundToInt(pos.y);
        GameObject tile = GameObject.Find("Game").GetComponent<GameBoard>().board[tileX, tileY];

        if(tile!=null)
            return tile;
        
        return null;
    }
}
