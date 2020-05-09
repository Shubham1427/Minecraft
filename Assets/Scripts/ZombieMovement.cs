using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieMovement : MonoBehaviour
{
    public float walkSpeed, gravity, charWidth, charHeight, jumpHeight, charMass, offset, idleTimeMax, playerChaseDistance;

    World world;
    private float verMove, horMove, rotMove, rotLerpParameter, idleTimeCounter;
    Vector3 velocity = new Vector3(0f, 0f, 0f);
    float virtualVelocityY = 0f, acceleration = 0f;
    public bool toJump = false, inWater = false, reachedTarget = true, pathFound = false, idle = false, playerVisible = false, checkingPlayerVisible = false;
    Vector3Int viewChunk = new Vector3Int(0, 0, 0), target = new Vector3Int (0, 0, 0);
    Quaternion finalRot = Quaternion.identity;
    Animator anim;
    public int pathIndex = -1;
    public List<Vector3Int> path;
    Transform player;
    public Vector3 TMP;

    // Start is called before the first frame update
    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        anim = GetComponent<Animator>();
        path = new List<Vector3Int>();
        player = GameObject.Find("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
    private void Update()
    {
        CalculateInputs();
        ApplyWaterEffects();
        TMP = myPosition;
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        TweakVelocity();
        if (velocity == Vector3.zero)
            anim.SetBool("goIdle", true);
        else
            anim.SetBool("goIdle", false);
        Move();
        Rotate(finalRot);
    }

    private void TweakVelocity()
    {
        bool upBlocked = false;
        if ((IsBlockSolid(new Vector3(offset + velocity.x * Time.fixedDeltaTime, charHeight, offset) + myPosition) ||
            IsBlockSolid(new Vector3(offset + velocity.x * Time.fixedDeltaTime, charHeight, charWidth) + myPosition)) &&
            velocity.x < 0f)
        {
            upBlocked = true;
            velocity.x = 0f;
        }
        else if ((IsBlockSolid(new Vector3(charWidth + velocity.x * Time.fixedDeltaTime, charHeight, offset) + myPosition) ||
            IsBlockSolid(new Vector3(charWidth + velocity.x * Time.fixedDeltaTime, charHeight, charWidth) + myPosition)) &&
            velocity.x > 0f)
        {
            upBlocked = true;
            velocity.x = 0f;
        }
        if ((IsBlockSolid(new Vector3(offset, charHeight, charWidth + velocity.z * Time.fixedDeltaTime) + myPosition) ||
            IsBlockSolid(new Vector3(charWidth, charHeight, charWidth + velocity.z * Time.fixedDeltaTime) + myPosition)) &&
            velocity.z > 0f)
        {
            upBlocked = true;
            velocity.z = 0f;
        }
        else if ((IsBlockSolid(new Vector3(offset, charHeight, offset + velocity.z * Time.fixedDeltaTime) + myPosition) ||
            IsBlockSolid(new Vector3(charWidth, charHeight, offset + velocity.z * Time.fixedDeltaTime) + myPosition)) &&
            velocity.z < 0f)
        {
            upBlocked = true;
            velocity.z = 0f;
        }
        if (IsBlockSolid(new Vector3(offset, charHeight + 0.1f, offset) + myPosition) ||
            IsBlockSolid(new Vector3(offset, charHeight + 0.1f, charWidth) + myPosition) ||
            IsBlockSolid(new Vector3(charWidth, charHeight + 0.1f, offset) + myPosition) ||
            IsBlockSolid(new Vector3(charWidth, charHeight + 0.1f, charWidth) + myPosition) ||
            virtualVelocityY > 0f
            )
        {
            upBlocked = true;
            if (IsBlockSolid(new Vector3(offset, charHeight + virtualVelocityY * Time.fixedDeltaTime + 0.5f * acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime, offset) + myPosition) ||
            IsBlockSolid(new Vector3(offset, charHeight + virtualVelocityY * Time.fixedDeltaTime + 0.5f * acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime, charWidth) + myPosition) ||
            IsBlockSolid(new Vector3(charWidth, charHeight + virtualVelocityY * Time.fixedDeltaTime + 0.5f * acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime, offset) + myPosition) ||
            IsBlockSolid(new Vector3(charWidth, charHeight + virtualVelocityY * Time.fixedDeltaTime + 0.5f * acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime, charWidth) + myPosition))
                virtualVelocityY = 0f;
        }
        bool downBlocked = false;
        if ((IsBlockSolid(new Vector3(offset + velocity.x * Time.fixedDeltaTime, 0.05f, offset) + myPosition) ||
            IsBlockSolid(new Vector3(offset + velocity.x * Time.fixedDeltaTime, 0.05f, charWidth) + myPosition)) &&
            velocity.x < 0f)
        {
            downBlocked = true;
            if (upBlocked)
                velocity.x = 0f;
        }
        else if ((IsBlockSolid(new Vector3(charWidth + velocity.x * Time.fixedDeltaTime, 0.05f, offset) + myPosition) ||
            IsBlockSolid(new Vector3(charWidth + velocity.x * Time.fixedDeltaTime, 0.05f, charWidth) + myPosition)) &&
            velocity.x > 0f)
        {
            downBlocked = true;
            if (upBlocked)
                velocity.x = 0f;
        }
        if ((IsBlockSolid(new Vector3(offset, 0.05f, charWidth + velocity.z * Time.fixedDeltaTime) + myPosition) ||
            IsBlockSolid(new Vector3(charWidth, 0.05f, charWidth + velocity.z * Time.fixedDeltaTime) + myPosition)) &&
            velocity.z > 0f)
        {
            downBlocked = true;
            if (upBlocked)
                velocity.z = 0f;
        }
        else if ((IsBlockSolid(new Vector3(offset, 0.05f, offset + velocity.z * Time.fixedDeltaTime) + myPosition) ||
            IsBlockSolid(new Vector3(charWidth, 0.05f, offset + velocity.z * Time.fixedDeltaTime) + myPosition)) &&
            velocity.z < 0f)
        {
            downBlocked = true;
            if (upBlocked)
                velocity.z = 0f;
        }
        if (acceleration == 0f && !upBlocked && downBlocked)
            toJump = true;
    }

    private void ApplyWaterEffects()
    {
        byte blockID;
        if (GetBlock(new Vector3(offset, 0.05f, offset) + myPosition) == null)
            blockID = 0;
        else
            blockID = GetBlock(new Vector3(offset, 0.05f, offset) + myPosition).id;
        if (blockID == 5)
            inWater = true;
        else
            inWater = false;
    }

    private void ApplyGravity()
    {
        if (!IsBlockSolid(new Vector3(offset, -offset, offset) + myPosition) &&
            !IsBlockSolid(new Vector3(charWidth, -offset, offset) + myPosition) &&
            !IsBlockSolid(new Vector3(offset, -offset, charWidth) + myPosition) &&
            !IsBlockSolid(new Vector3(charWidth, -offset, charWidth) + myPosition)
            )
        {
            if (!IsBlockSolid(new Vector3(offset, virtualVelocityY * Time.fixedDeltaTime + 0.5f * acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime, offset) + myPosition) &&
                !IsBlockSolid(new Vector3(charWidth, virtualVelocityY * Time.fixedDeltaTime + 0.5f * acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime, offset) + myPosition) &&
                !IsBlockSolid(new Vector3(offset, virtualVelocityY * Time.fixedDeltaTime + 0.5f * acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime, charWidth) + myPosition) &&
                !IsBlockSolid(new Vector3(charWidth, virtualVelocityY * Time.fixedDeltaTime + 0.5f * acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime, charWidth) + myPosition)
                )
            {
                acceleration = gravity;
                if (inWater)
                    acceleration = acceleration / 2f;
            }
            else
            {
                virtualVelocityY = 0f;
                if (acceleration < 0f)
                    acceleration = 0f;
            }
        }
        else
        {
            virtualVelocityY = 0f;
            if (acceleration < 0f)
                acceleration = 0f;
        }
    }

    Vector3 myPosition
    {
        get
        {
            return new Vector3(transform.position.x - 0.5f, transform.position.y - 1f, transform.position.z - 0.5f);
        }
    }

    IEnumerator CheckPlayerVisible()
    {
        checkingPlayerVisible = true;
        Vector3 dir = new Vector3(player.position.x, 0f, player.position.z) - new Vector3(myPosition.x + 0.5f, 0f, myPosition.z + 0.5f);
        if (playerVisible)
        {
            if (dir.magnitude > playerChaseDistance)
                playerVisible = false;
            checkingPlayerVisible = false;
            yield break;
        }
        if (Vector3.Angle(dir.normalized, transform.forward) > 70f)
        {
            playerVisible = false;
            checkingPlayerVisible = false;
            yield break;
        }
        dir = new Vector3(player.position.x, player.position.y + 1f, player.position.z) - new Vector3(myPosition.x + 0.5f, myPosition.y + 2f, myPosition.z + 0.5f);
        if (dir.magnitude > playerChaseDistance)
        {
            playerVisible = false;
            checkingPlayerVisible = false;
            yield break;
        }
        for (int i=1; i<= playerChaseDistance; i++)
        {
            Vector3 pos = dir * (i/playerChaseDistance) + new Vector3(myPosition.x + 0.5f, myPosition.y + 2f, myPosition.z + 0.5f);
            if (IsBlockSolid(pos))
            {
                playerVisible = false;
                checkingPlayerVisible = false;
                yield break;
            }
            yield return null;
        }
        playerVisible = true;
        checkingPlayerVisible = false;
    }

    private void CalculateInputs()
    {
        if (!checkingPlayerVisible)
            StartCoroutine(CheckPlayerVisible());
        if (idle)
        {
            idleTimeCounter += Time.deltaTime;
            if (idleTimeCounter > idleTimeMax)
            {
                idleTimeCounter = 0f;
                idle = false;
            }
        }
        Vector3Int playerPosition = new Vector3Int((int)player.position.x, 0, (int)player.position.z);
        if ((!pathFound && pathIndex == -1 && !idle) || (playerVisible && target != playerPosition && pathFound))
        {
            if (!playerVisible)
            {
                do
                {
                    target.x = (int)myPosition.x + Random.Range(-9, 10);
                    target.z = (int)myPosition.z + Random.Range(-9, 10);
                }
                while (target.x < 0 || target.x >= GeneralSettings.chunkWidth * GeneralSettings.worldSizeInChunks || target.z < 0 || target.z >= GeneralSettings.chunkWidth * GeneralSettings.worldSizeInChunks);
            }
            else
                target = playerPosition;
            if ((new Vector3Int(target.x, 0, target.z) - new Vector3(myPosition.x, 0, myPosition.z)).magnitude < 0.1f)
            {
                velocity = Vector3.forward * verMove + Vector3.right * horMove;
                velocity = velocity.normalized * walkSpeed;
                if (inWater)
                    toJump = true;
                return;
            }
            pathFound = false;
            pathIndex = 0;
            int yPos;
            for (yPos = (int)myPosition.y; yPos >= 0; yPos--)
            {
                if (world.blockTypes[GetBlock(new Vector3Int((int)Mathf.Round(myPosition.x), yPos, (int)Mathf.Round(myPosition.z))).id].isVisible)
                    break;
            }
            StartCoroutine(PathFind(new Vector3Int((int)Mathf.Round(myPosition.x), yPos, (int)Mathf.Round(myPosition.z)), target));
            reachedTarget = false;
        }
        if (!reachedTarget && pathFound && pathIndex >= 0 && pathIndex < path.Count)
        {
            if ((path[pathIndex] - new Vector3(myPosition.x, 0, myPosition.z)).magnitude < 0.1f)
            {
                if (pathIndex >= path.Count - 1)
                {
                    reachedTarget = true;
                    pathFound = false;
                    if (!playerVisible)
                        idle = true;
                    pathIndex = -1;
                    horMove = 0;
                    verMove = 0;
                    velocity = Vector3.forward * verMove + Vector3.right * horMove;
                    velocity = velocity.normalized * walkSpeed;
                    if (inWater)
                        toJump = true;
                    idleTimeCounter = 0f;
                    return;
                }
                pathIndex++;
            }
            if (pathIndex >= 0 && pathIndex < path.Count)
            {
                Vector3 dir = (path[pathIndex] - new Vector3(myPosition.x, 0f, myPosition.z)).normalized;
                horMove = dir.x;
                verMove = dir.z;
                float angle = Vector3.Angle(transform.forward, path[pathIndex] - new Vector3(myPosition.x, 0f, myPosition.z));
                if (Vector3.Cross(transform.forward, path[pathIndex] - new Vector3(myPosition.x, 0f, myPosition.z)).y < 0f)
                    angle = -angle;
                finalRot = Quaternion.Euler(new Vector3(0f, angle, 0f)) * transform.rotation;
            }
        }

        velocity = Vector3.forward * verMove + Vector3.right * horMove;
        velocity = velocity.normalized * walkSpeed;

        if (inWater)
            toJump = true;
    }

    int CalculateHCost(Vector3Int a, Vector3Int b)
    {
        int x = Mathf.Abs(a.x - b.x);
        int z = Mathf.Abs(a.z - b.z);
        if (x < z)
            return 14 * x + (z - x)*10;
        else
            return 14 * z + (x - z)*10;
    }

    IEnumerator PathFind(Vector3Int start, Vector3Int dest)
    {
        int fCost, gCost=0, hCost=CalculateHCost(start, dest);
        fCost = hCost + gCost;
        List<Vector3Int> localPath = new List<Vector3Int>();
        List<PathNode> opened = new List<PathNode>();
        List<PathNode> closed = new List<PathNode>();
        opened.Add(new PathNode(fCost, gCost, hCost, start, new Vector3Int (-1, -1, -1)));
        PathNode current;
        int count = 0;
        while(opened.Count > 0 && count++<1000)
        {
            current = opened[opened.Count - 1];
            foreach (PathNode x in opened)
            {
                if (x.fCost < current.fCost || (x.fCost == current.fCost && x.hCost < current.hCost))
                    current = x;
            }
            opened.Remove(current);
            closed.Add(current);
            if (current.blockPos.x == dest.x && current.blockPos.z == dest.z)
            {
                opened.Clear();
                closed.Add(current);
                Vector3Int pos = current.blockPos;
                int u = 0;
                while (pos != start && u++ < 300)
                {
                    localPath.Add(new Vector3Int (pos.x, 0, pos.z));
                    foreach (PathNode y in closed)
                    {
                        if (y.blockPos == pos)
                        {
                            pos = y.parent;
                            closed.Remove(y);
                            break;
                        }
                    }
                }
                localPath.Reverse();
                path.Clear();
                for (int i = 0; i < localPath.Count; i++)
                {
                    path.Add(localPath[i]);
                }
                localPath.Clear();
                closed.Clear();
                pathFound = true;
                yield break;
            }
            BlockData currentBlock = GetBlock(current.blockPos);
            for (int i = 0; i < 8; i++)
            {
                if (currentBlock.neighbors[i] == false)
                    continue;
                Vector3Int neighbourPos = current.blockPos + BlockMeshData.neighbourIndex[i];
                for (int j = 1; j >= -3; j--)
                {
                    if (neighbourPos.y + j >= 0 && neighbourPos.y + j < GeneralSettings.chunkHeight)
                    {
                        if (world.blockTypes[GetBlock(neighbourPos + new Vector3Int(0, j, 0)).id].isVisible)
                        {
                            neighbourPos += new Vector3Int(0, j, 0);
                            break;
                        }
                    }
                }
                bool flag = false;
                foreach (PathNode x in closed)
                {
                    if (x.blockPos == neighbourPos)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                    continue;
                gCost = current.gCost + BlockMeshData.neighbourDistance[i];
                hCost = CalculateHCost(neighbourPos, dest);
                fCost = hCost + gCost;
                bool exists = false;
                foreach (PathNode x in opened)
                {
                    if (x.blockPos == neighbourPos)
                    {
                        if (x.gCost > gCost)
                        {
                            x.gCost = gCost;
                            x.fCost = x.gCost + x.hCost;
                            x.parent = current.blockPos;
                        }
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                {
                    PathNode x = new PathNode(fCost, gCost, hCost, neighbourPos, current.blockPos);
                    opened.Add(x);
                }
            }
            yield return null;
        }
        pathIndex = -1;
        horMove = 0;
        verMove = 0;
        opened.Clear();
        closed.Clear();
        localPath.Clear();
        pathFound = false;
        yield break;
    }

    class PathNode
    {
        public int fCost, gCost, hCost;
        public Vector3Int blockPos;
        public Vector3Int parent;

        public PathNode (int f, int g, int h, Vector3Int x, Vector3Int p)
        {
            fCost = f;
            gCost = g;
            hCost = h;
            blockPos = x;
            parent = p;
        }
    }

    private void Move()
    {
        if (toJump)
        {
            if (!inWater)
            {
                acceleration = gravity;
                virtualVelocityY = Mathf.Sqrt(-2f * acceleration * jumpHeight);
            }
            else
            {
                acceleration = (charHeight * charWidth * charWidth * (-gravity) / charMass + gravity);
                virtualVelocityY = Mathf.Sqrt(-2f * acceleration * jumpHeight / 3f);
            }
            toJump = false;
        }
        transform.position += (velocity + new Vector3(0f, virtualVelocityY, 0f)) * Time.fixedDeltaTime + 0.5f * new Vector3(0f, acceleration, 0f) * Time.fixedDeltaTime * Time.fixedDeltaTime;
        virtualVelocityY += acceleration * Time.fixedDeltaTime;
    }

    private void Rotate(Quaternion final)
    {
        if (final == transform.rotation)
        {
            rotLerpParameter = 0f;
            return;
        }
        rotLerpParameter += Time.fixedDeltaTime;
        transform.rotation = Quaternion.Lerp(transform.rotation, final, rotLerpParameter);
    }

    private bool IsBlockSolid(Vector3 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        int zombieChunkX = x / GeneralSettings.chunkWidth;
        int zombieChunkZ = z / GeneralSettings.chunkWidth;

        if (zombieChunkX < 0 || zombieChunkX >= GeneralSettings.worldSizeInChunks || zombieChunkZ < 0 || zombieChunkZ >= GeneralSettings.worldSizeInChunks)
            return true;

        x -= zombieChunkX * GeneralSettings.chunkWidth;
        z -= zombieChunkZ * GeneralSettings.chunkWidth;


        if (!world.IsPRCInChunk(new Vector3(x, y, z)))
            return false;

        return world.blockTypes[world.chunks[zombieChunkX, zombieChunkZ].myBlocks[x, y, z].id].isSolid;
    }

    private BlockData GetBlock(Vector3 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        int zombieChunkX = x / GeneralSettings.chunkWidth;
        int zombieChunkZ = z / GeneralSettings.chunkWidth;

        x -= zombieChunkX * GeneralSettings.chunkWidth;
        z -= zombieChunkZ * GeneralSettings.chunkWidth;

        if (!world.IsPRCInChunk(new Vector3(x, y, z)))
            return null;

        return world.chunks[zombieChunkX, zombieChunkZ].myBlocks[x, y, z];
    }
}