using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ghost : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float normalMoveSpeed = 5.0f;
    public float frightenedModeMoveSpeed = 3.0f;
    public float portalMoveSpeed = 3.0f;
    public float consumedMoveSpeed = 15.0f;
    private float previousMoveSpeed;
    public Node startingPosition;
    public bool canMove = true;

    public Node homeNode;
    public Node ghostHouse;

    public int frightenedModeDuration = 10;
    public int startBlinkingAt = 7;

    private float frightenedModeTimer = 0;
    private float blinkTimer = 0;

    private bool frightenedModeIsWhite = false;

    private AudioSource backgroundAudio;
    public int scatterModeTimer1 = 7;
    public int chaseModeTimer1 = 20;
    public int scatterModeTimer2 = 7;
    public int chaseModeTimer2 = 20;
    public int scatterModeTimer3 = 5;
    public int chaseModeTimer3 = 20;
    public int scatterModeTimer4 = 5;

    public Sprite eyesUp;
    public Sprite eyesDown;
    public Sprite eyesLeft;
    public Sprite eyesRight;

    private int modeChangeIteration = 1;
    public float modeChangeTimer = 0;

    public float ghostReleaseTimer = 0;
    public int pinkyReleaseTimer = 5;
    public int inkyReleaseTimer = 7;
    public int clydeReleaseTimer = 10;

    public bool isInGhostHouse = false;

    public RuntimeAnimatorController ghostUp;
    public RuntimeAnimatorController ghostDown;
    public RuntimeAnimatorController ghostLeft;
    public RuntimeAnimatorController ghostRight;
    public RuntimeAnimatorController ghostWhite;
    public RuntimeAnimatorController ghostBlue;

    public int consumedGhostScore = 200;
    public enum Mode
    {
        chase,
        scatter,
        frightened,
        consumed
    }

    Mode currentMode = Mode.scatter;
    Mode previousMode;
    public enum GhostType
    {
        Red,
        Pink,
        Blue,
        Orange
    }

    public GhostType ghostType = GhostType.Red;

    private GameObject pacMan;
    private Node currentNode, targetNode, previousNode;
    private Vector2 direction, nextDirection;
    // Start is called before the first frame update

    public void MoveToStartingPosition()
    {
        if (transform.name != "Ghost_Blinky")
            isInGhostHouse = true;

        transform.position = startingPosition.transform.position;
        if (isInGhostHouse)
        {
            direction = Vector2.up;
        }
        else
        {
            direction = Vector2.left;
        }
        UpdateAnimatorController();
    }
    public void Restart()
    {
        canMove = true;
        //transform.GetComponent<SpriteRenderer>().enabled = true;

        currentMode = Mode.scatter;

        moveSpeed = normalMoveSpeed;
        previousMoveSpeed = 0;


        ghostReleaseTimer = 0;
        modeChangeIteration = 1;
        modeChangeTimer = 0;


        currentNode = startingPosition;
        if (isInGhostHouse)
        {
            direction = Vector2.up;
            targetNode = currentNode.neighbors[0];
        }
        else
        {
            direction = Vector2.left;
            targetNode = ChooseNextNode();
        }
        previousNode = currentNode;

    }
    void Start()
    {
        backgroundAudio = GameObject.Find("Game").transform.GetComponent<AudioSource>();

        pacMan = GameObject.FindGameObjectWithTag("PacMan");

        Node node = GetNodeAtPosition(transform.localPosition);

        if (node != null)
        {
            currentNode = node;
        }

        if (isInGhostHouse)
        {
            direction = Vector2.up;
            targetNode = currentNode.neighbors[0];
        }
        else
        {
            direction = Vector2.left;
            targetNode = ChooseNextNode();
        }
        previousNode = currentNode;
        UpdateAnimatorController();
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            ModeUpdate();

            Move();

            ReleaseGhosts();

            CheckCollision();

            CheckIsInGhostHouse();
        }
    }

    void CheckIsInGhostHouse()
    {
        if (currentMode == Mode.consumed)
        {
            moveSpeed = consumedMoveSpeed;
            GameObject tile = GetTileAtPosition(transform.position);
            if (tile != null)
            {
                if (tile.transform.GetComponent<Tile>() != null)
                {
                    if (tile.transform.GetComponent<Tile>().isGhostHouse)
                    {
                        moveSpeed = normalMoveSpeed;
                        Node node = GetNodeAtPosition(transform.position);
                        if (node != null)
                        {
                            currentNode = node;
                            direction = Vector2.up;
                            targetNode = currentNode.neighbors[0];

                            previousNode = currentNode;

                            currentMode = Mode.chase;
                            UpdateAnimatorController();
                        }
                    }
                }
            }
        }
    }

    void CheckCollision()
    {
        Rect ghostRect = new Rect(transform.position, transform.GetComponent<SpriteRenderer>().sprite.bounds.size / 4);
        Rect pacManRect = new Rect(pacMan.transform.position, pacMan.transform.GetComponent<SpriteRenderer>().sprite.bounds.size / 4);

        if (ghostRect.Overlaps(pacManRect))
        {
            if (currentMode == Mode.frightened)
            {
                //Pacman should consume the ghosts
                Consumed();
            }
            else
            if (currentMode == Mode.scatter || currentMode == Mode.chase)
            {
                //Pacman Should Die
                GameObject.Find("Game").transform.GetComponent<GameBoard>().StartDeath();
            }
        }
    }

    void Consumed()
    {
        GameBoard.player1Score += GameBoard.ghostConsumedRunningScore;
        currentMode = Mode.consumed;
        previousMoveSpeed = moveSpeed;
        moveSpeed = consumedMoveSpeed;
        UpdateAnimatorController();

        GameObject.Find("Game").transform.GetComponent<GameBoard>().StartConsumed(this.GetComponent<ghost>());

        GameBoard.ghostConsumedRunningScore = GameBoard.ghostConsumedRunningScore * 2;

    }

    void UpdateAnimatorController()
    {
        if (currentMode != Mode.frightened && currentMode != Mode.consumed)
        {
            if (direction == Vector2.up)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostUp;
            }
            else
            if (direction == Vector2.down)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostDown;
            }
            else
            if (direction == Vector2.left)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostLeft;
            }
            else
            if (direction == Vector2.down)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostRight;
            }
            else
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostLeft;
            }
        }
        else
        if (currentMode == Mode.frightened)
        {
            transform.GetComponent<Animator>().runtimeAnimatorController = ghostBlue;
        }
        else
        if (currentMode == Mode.consumed)
        {
            transform.GetComponent<Animator>().runtimeAnimatorController = null;
            if (direction == Vector2.up)
            {
                transform.GetComponent<SpriteRenderer>().sprite = eyesUp;
            }
            else
            if (direction == Vector2.down)
            {
                transform.GetComponent<SpriteRenderer>().sprite = eyesDown;
            }
            else
            if (direction == Vector2.left)
            {
                transform.GetComponent<SpriteRenderer>().sprite = eyesLeft;
            }
            else
            if (direction == Vector2.right)
            {
                transform.GetComponent<SpriteRenderer>().sprite = eyesRight;
            }
        }
    }

    void Move()
    {

        if (targetNode != currentNode && targetNode != null && !isInGhostHouse)
        {
           
            if (OverShotTarget())
            {
                currentNode = targetNode;
                transform.localPosition = currentNode.transform.position;
                PortalSlower();
                
                GameObject otherPortal = GetPortal(currentNode.transform.position);

                if (otherPortal != null)
                {
                    transform.localPosition = otherPortal.transform.position;
                    currentNode = otherPortal.GetComponent<Node>();
                }

                targetNode = ChooseNextNode();
                previousNode = currentNode;
                currentNode = null;
                UpdateAnimatorController();
            }
            else
            {
                transform.localPosition += (Vector3)direction * moveSpeed * Time.deltaTime;
            }
            
        }
    }

    void PortalSlower()
    {
        GameObject tile = GetTileAtPosition(currentNode.transform.position);
        if (tile.transform.GetComponent<Tile>().portalSlower == true)
        {
            if(currentMode != Mode.frightened || currentMode!=Mode.consumed)
            {
                previousMoveSpeed = moveSpeed;
                moveSpeed = portalMoveSpeed;

            } 
        }
        else
        {
            if(currentMode != Mode.frightened || currentMode!=Mode.consumed)
            {
                moveSpeed = normalMoveSpeed;
            }  
        }
    }
    void ModeUpdate()
    {
        if(currentMode != Mode.frightened)
        {
            modeChangeTimer += Time.deltaTime;
            
            if(modeChangeIteration == 1)
            {
                if(currentMode == Mode.scatter && modeChangeTimer > scatterModeTimer1)
                {
                    ChangeMode(Mode.chase);
                    modeChangeTimer = 0;
                }

                if(currentMode == Mode.chase && modeChangeTimer > chaseModeTimer1)
                {
                    modeChangeIteration = 2;
                    ChangeMode(Mode.scatter);
                    modeChangeTimer = 0;
                }
            }
            else
            if(modeChangeIteration == 2)
            {
                if(currentMode == Mode.scatter && modeChangeTimer >scatterModeTimer2)
                {
                    ChangeMode(Mode.chase);
                    modeChangeTimer = 0;
                }

                if(currentMode == Mode.chase && modeChangeTimer >chaseModeTimer2)
                {
                    modeChangeIteration = 3;
                    ChangeMode(Mode.scatter);
                    modeChangeTimer = 0;
                }

            }
            else
            if(modeChangeIteration ==3)
            {
                if (currentMode == Mode.scatter && modeChangeTimer > scatterModeTimer3)
                {
                    ChangeMode(Mode.chase);
                    modeChangeTimer = 0;
                }

                if (currentMode == Mode.chase && modeChangeTimer > chaseModeTimer3)
                {
                    modeChangeIteration = 4;
                    ChangeMode(Mode.scatter);
                    modeChangeTimer = 0;
                }
            }
            else
            if (modeChangeIteration == 4)
            {
                if (currentMode == Mode.scatter && modeChangeTimer > scatterModeTimer4)
                {
                    ChangeMode(Mode.chase);
                    modeChangeTimer = 0;
                }
            }
        } 
        else
        if(currentMode == Mode.frightened)
        {
            moveSpeed = frightenedModeMoveSpeed;
            frightenedModeTimer += Time.deltaTime;
            if(frightenedModeTimer >= frightenedModeDuration)
            {
                backgroundAudio.clip = GameObject.Find("Game").transform.GetComponent<GameBoard>().backgroundAudioNormal;
                backgroundAudio.Play();
                frightenedModeTimer = 0;
                ChangeMode(previousMode);
            }

            if(frightenedModeTimer >= startBlinkingAt)
            {
                blinkTimer += Time.deltaTime;
                if(blinkTimer >= 0.1f)
                {
                    blinkTimer = 0;
                    if(frightenedModeIsWhite)
                    {
                        transform.GetComponent<Animator>().runtimeAnimatorController = ghostBlue;
                        frightenedModeIsWhite = false;
                    }
                    else
                    {
                        transform.GetComponent<Animator>().runtimeAnimatorController = ghostWhite;
                        frightenedModeIsWhite = true;
                    }
                }
            }
        }
    }

   public void ChangeMode(Mode m)
    {
        if(currentMode == Mode.frightened)
        {
            moveSpeed = previousMoveSpeed;
        }
        
        if(m == Mode.frightened)
        {
            previousMoveSpeed = moveSpeed;
            moveSpeed = frightenedModeMoveSpeed;
        }
        if(currentMode!= m)
        {
            previousMode = currentMode;
            currentMode = m;
        }
        UpdateAnimatorController();
    }

    public void StartFrightenedMode()
    {
        GameBoard.ghostConsumedRunningScore = 200;
        if(!isInGhostHouse)
        {
         ReverseGhostDirection();
        }
        
        if(currentMode != Mode.consumed)
        {
            frightenedModeTimer = 0;
            backgroundAudio.clip = GameObject.Find("Game").transform.GetComponent<GameBoard>().backgroundAudioFrightened;
            backgroundAudio.Play();
            ChangeMode(Mode.frightened);
        }

    }

    Vector2 GetRedGhostTargetTile()
    {
        Vector2 pacManPosition = pacMan.transform.localPosition;
        Vector2 targetTile = new Vector2(Mathf.RoundToInt(pacManPosition.x), Mathf.RoundToInt(pacManPosition.y));
        return targetTile;
    }

    Vector2 GetPinkGhostTargetTile()
    {
        // four tiles ahead of the PacMan
        // Taking account the orientation and Position of the PacMan
        Vector2 pacManPosition = pacMan.transform.localPosition;
        Vector2 pacManOrientation = pacMan.GetComponent<PacMan>().orientation;

        int pacManPositionX = Mathf.RoundToInt(pacManPosition.x);
        int pacManPositionY = Mathf.RoundToInt(pacManPosition.y);

        Vector2 pacManTile = new Vector2(pacManPositionX, pacManPositionY);
        Vector2 targetTile = pacManTile + (4 * pacManOrientation);

        return targetTile;
    }

    Vector2 GetBlueGhostTargetTile()
    {
        //select the position 2 tiles infront of pacman
        //Draw a vector from blinky to that position
        //double the length of the vector
        Vector2 pacManPosition = pacMan.transform.localPosition;
        Vector2 pacManOrientation = pacMan.GetComponent<PacMan>().orientation;

        int pacManPositionX = Mathf.RoundToInt(pacManPosition.x);
        int pacManPositionY = Mathf.RoundToInt(pacManPosition.y);

        Vector2 pacManTile = new Vector2(pacManPositionX, pacManPositionY);
        Vector2 targetTile = pacManTile +(2*pacManOrientation);
        //Temporary Blinky Position
        Vector2 tempBlinkyPosition = GameObject.Find("Ghost_Blinky").transform.localPosition;

        int blinkyPositionX = Mathf.RoundToInt(tempBlinkyPosition.x);
        int blinkyPositionY = Mathf.RoundToInt(tempBlinkyPosition.y);

        tempBlinkyPosition = new Vector2(blinkyPositionX, blinkyPositionY);
        float distance = GetDistance(tempBlinkyPosition, targetTile);
        distance *= 2;
        targetTile = new Vector2(tempBlinkyPosition.x + distance, tempBlinkyPosition.y + distance);
        return targetTile;
    }

    Vector2 GetOrangeGhostTargetTile()
    {
        //calculate the distance from Pac-Man
        // if the distance is greater than eight tiles targeting  is the same as Blinky
        // if the distance is less than eight tiles, then target is his Home Node, i-e Same as Scatter Mode
        Vector2 pacManPosition = pacMan.transform.localPosition;
        float distance = GetDistance(transform.localPosition, pacManPosition);
        Vector2 targetTile = Vector2.zero;

        if(distance > 8)
        {
            targetTile = new Vector2(Mathf.RoundToInt(pacManPosition.x), Mathf.RoundToInt(pacManPosition.y));
        }
        else
        if(distance < 8)
        {
            targetTile = homeNode.transform.position;
        }
        return targetTile;
    }

    Vector2 GetTargetTile()
    {
        Vector2 targetTile = Vector2.zero;

        if(ghostType == GhostType.Red)
        {
            targetTile = GetRedGhostTargetTile();
        }
        else
        if(ghostType == GhostType.Pink)
        {
            targetTile = GetPinkGhostTargetTile();
        }
        else
        if(ghostType == GhostType.Blue)
        {
            targetTile = GetBlueGhostTargetTile();  
        }
        else
        if(ghostType == GhostType.Orange)
        {
            targetTile = GetOrangeGhostTargetTile();
        }
        return targetTile;
    }

    Vector2 GetRandomTile()
    {
        int x = Random.Range(0, 28);
        int y = Random.Range(0, 36);
        return new Vector2(x, y);
    }

    void ReleasePinkGhost()
    {
        if(ghostType == GhostType.Pink && isInGhostHouse)
        {
            isInGhostHouse = false;
        }
    }

    void ReleaseBlueGhost()
    {
        if(ghostType == GhostType.Blue && isInGhostHouse)
        {
            isInGhostHouse = false;
        }
    }

    void ReleaseOrangeGhost()
    {
        if(ghostType == GhostType.Orange && isInGhostHouse)
        {
            isInGhostHouse = false;
        }
    }

    void ReleaseGhosts()
    {
        ghostReleaseTimer += Time.deltaTime;
        if(ghostReleaseTimer > pinkyReleaseTimer)
        {
            ReleasePinkGhost();
        }
        if (ghostReleaseTimer > inkyReleaseTimer)
        {
            ReleaseBlueGhost();
        }
        if(ghostReleaseTimer >clydeReleaseTimer)
        {
            ReleaseOrangeGhost();
        }

    }

    Node ChooseNextNode()
    {
        Vector2 targetTile = Vector2.zero;

        if (currentMode == Mode.chase)
        {
            targetTile = GetTargetTile();
        }
        else
        if (currentMode == Mode.scatter)
        {
            targetTile = homeNode.transform.position;
        }
        else
        if (currentMode == Mode.frightened)
        {
            targetTile = GetRandomTile();
        }
        else
        if (currentMode == Mode.consumed)
        {
            targetTile = ghostHouse.transform.position;
        }

        Node moveToNode = null;

        Node[] foundNodes = new Node[4];
        Vector2[] foundNodesDirection = new Vector2[4];

        int nodeCounter = 0;

        for(int i =0; i< currentNode.neighbors.Length; i++)
        {
            if(currentNode.validDirections [i] != direction * -1)
            {
                if(currentMode!= Mode.consumed)
                {
                    GameObject tile = GetTileAtPosition(currentNode.transform.position);
                    if(tile.transform.GetComponent<Tile>().isGhostHouseEntrance == true)
                    {
                        //found a ghost house, don't want to allow movement
                        if(currentNode.validDirections[i] != Vector2.down)
                        {
                            foundNodes[nodeCounter] = currentNode.neighbors[i];
                            foundNodesDirection[nodeCounter] = currentNode.validDirections[i];
                            nodeCounter++;
                        }
                    }
                    else
                    {
                        foundNodes[nodeCounter] = currentNode.neighbors[i];
                        foundNodesDirection[nodeCounter] = currentNode.validDirections[i];
                        nodeCounter++;
                    }
                }

                else
                {
                    foundNodes[nodeCounter] = currentNode.neighbors[i];
                    foundNodesDirection[nodeCounter] = currentNode.validDirections[i];
                    nodeCounter++;
                }
            }
        }

        if(foundNodes.Length == 1)
        {
            moveToNode = foundNodes[0];
            direction = foundNodesDirection[0];
        }

        if(foundNodes.Length > 1)
        {
            float leastDistance = 100000f;
            for(int i =0; i< foundNodes.Length; i++)
            {
                if(foundNodesDirection[i] != Vector2.zero)
                {
                    float distance = GetDistance(foundNodes[i].transform.position, targetTile);

                    if(distance < leastDistance)
                    {
                        leastDistance = distance;
                        moveToNode = foundNodes[i];
                        direction = foundNodesDirection[i];
                    }
                }
            }
        }
        return moveToNode;
    }
    Node GetNodeAtPosition(Vector2 Pos)
    {
        GameObject tile = GameObject.Find("Game").GetComponent<GameBoard>().board[(int)Pos.x, (int)Pos.y];
        if(tile!=null)
        {
            if(tile.GetComponent<Node>() !=null)
            {
                return tile.GetComponent<Node>();
            }
        }
        return null;
    }

    GameObject GetPortal(Vector2 pos)
    {
        GameObject tile = GameObject.Find("Game").GetComponent<GameBoard>().board[(int)pos.x, (int)pos.y];
        if(tile!=null)
        {
            if(tile.GetComponent<Tile>().portal)
            {
                GameObject otherPortal = tile.GetComponent<Tile>().portalReciever;
                return otherPortal;
            }
        }
        return null;
    }

    GameObject GetTileAtPosition(Vector2 pos)
    {
        int tileX = Mathf.RoundToInt(pos.x);
        int tileY = Mathf.RoundToInt(pos.y);
        GameObject tile = GameObject.Find("Game").transform.GetComponent<GameBoard>().board[tileX, tileY];
        if(tile != null)
        {
            return tile;
        }
        return null;
    }

  float LengthFromNode(Vector2 targetPosition)
    {
        Vector2 vec = targetPosition - (Vector2)previousNode.transform.position;
        return vec.sqrMagnitude;
    }

    bool OverShotTarget()
    {
        float nodeToTarget = LengthFromNode(targetNode.transform.position);
        float nodeToSelf = LengthFromNode(transform.localPosition);

        return nodeToSelf > nodeToTarget;
    }

    float GetDistance(Vector2 posA, Vector2 posB)
    {
        float dx = posA.x - posB.x;
        float dy = posA.y - posB.y;

        float distance = Mathf.Sqrt(dx * dx + dy * dy);
        return distance;
    }

    public void ReverseGhostDirection()
    {
        direction *= -1;
        Node tempNode = targetNode;
        targetNode = previousNode;
        previousNode = tempNode;
    }

}
