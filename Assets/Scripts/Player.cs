using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float walkSpeed, mouseSensitivityX, mouseSensitivityY, gravity, charHalfWidth, charHalfHeight, jumpHeight, charMass;
    public World world;
    public int interactDistance;
    public GameObject waterEffect, breakBlockUIObject;
    public RectTransform inventoryHighlightor;
    public InventorySlot[] Inventory = new InventorySlot [8];

    byte selectedInventorySlot;
    Vector3 inventoryOffsetPos;
    private float verMove, horMove, camX, camY;
    private Vector3 velocity = new Vector3(0f, 0f, 0f);
    private Camera cam;
    private float currRot = 0f, virtualVelocityY = 0f, acceleration = 0f;
    private bool toJump = false, inWater = false;
    Vector3Int viewChunk = new Vector3Int(0, 0, 0), placeViewChunk = Vector3Int.zero;
    public Vector3Int focusBlockPos, placeBlockPos;
    private byte focusBlockType = 0;

    // Start is called before the first frame update
    private void Start()
    {
        transform.position = new Vector3(Random.Range(0, 80), 130, Random.Range(0, 80));
        selectedInventorySlot = Inventory[0].blockType;
        inventoryOffsetPos = inventoryHighlightor.position;
        cam = GetComponentInChildren<Camera>();
        if (cam == null)
            Debug.LogError("No Camera attached to Player");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    private void Update()
    {
        GetFocusBlock();
        TakeInputs();
        ApplyWaterEffects();
        ApplyGravity();
        TweakVelocity();
        Move();
        RotateCamera();
    }

    private void TweakVelocity()
    {
        if ((GetBlock(new Vector3(-charHalfWidth + velocity.x * Time.deltaTime, -charHalfHeight + 0.1f, -charHalfWidth) + transform.position) ||
            GetBlock(new Vector3(-charHalfWidth + velocity.x * Time.deltaTime, -charHalfHeight + 0.1f, charHalfWidth) + transform.position)) &&
            velocity.x < 0f)
            velocity.x = 0f;
        else if ((GetBlock(new Vector3(charHalfWidth + velocity.x * Time.deltaTime, -charHalfHeight + 0.1f, -charHalfWidth) + transform.position) ||
            GetBlock(new Vector3(charHalfWidth + velocity.x * Time.deltaTime, -charHalfHeight + 0.1f, charHalfWidth) + transform.position)) &&
            velocity.x > 0f)
            velocity.x = 0f;
        if ((GetBlock(new Vector3(-charHalfWidth, -charHalfHeight + 0.1f, charHalfWidth + velocity.z * Time.deltaTime) + transform.position) ||
            GetBlock(new Vector3(charHalfWidth, -charHalfHeight + 0.1f, charHalfWidth + velocity.z * Time.deltaTime) + transform.position)) &&
            velocity.z > 0f)
            velocity.z = 0f;
        else if ((GetBlock(new Vector3(-charHalfWidth, -charHalfHeight + 0.1f, -charHalfWidth + velocity.z * Time.deltaTime) + transform.position) ||
            GetBlock(new Vector3(charHalfWidth, -charHalfHeight + 0.1f, -charHalfWidth + velocity.z * Time.deltaTime) + transform.position)) &&
            velocity.z < 0f)
            velocity.z = 0f;
        if ((GetBlock(new Vector3(-charHalfWidth + velocity.x * Time.deltaTime, charHalfHeight - 0.1f, -charHalfWidth) + transform.position) ||
            GetBlock(new Vector3(-charHalfWidth + velocity.x * Time.deltaTime, charHalfHeight - 0.1f, charHalfWidth) + transform.position)) &&
            velocity.x < 0f)
            velocity.x = 0f;
        else if ((GetBlock(new Vector3(charHalfWidth + velocity.x * Time.deltaTime, charHalfHeight - 0.1f, -charHalfWidth) + transform.position) ||
            GetBlock(new Vector3(charHalfWidth + velocity.x * Time.deltaTime, charHalfHeight - 0.1f, charHalfWidth) + transform.position)) &&
            velocity.x > 0f)
            velocity.x = 0f;
        if ((GetBlock(new Vector3(-charHalfWidth, charHalfHeight - 0.1f, charHalfWidth + velocity.z * Time.deltaTime) + transform.position) ||
            GetBlock(new Vector3(charHalfWidth, charHalfHeight - 0.1f, charHalfWidth + velocity.z * Time.deltaTime) + transform.position)) &&
            velocity.z > 0f)
            velocity.z = 0f;
        else if ((GetBlock(new Vector3(-charHalfWidth, charHalfHeight - 0.1f, -charHalfWidth + velocity.z * Time.deltaTime) + transform.position) ||
            GetBlock(new Vector3(charHalfWidth, charHalfHeight - 0.1f, -charHalfWidth + velocity.z * Time.deltaTime) + transform.position)) &&
            velocity.z < 0f)
            velocity.z = 0f;
        if (GetBlock(new Vector3(-charHalfWidth, charHalfHeight, -charHalfWidth) + transform.position) ||
            GetBlock(new Vector3(-charHalfWidth, charHalfHeight, charHalfWidth) + transform.position) ||
            GetBlock(new Vector3(charHalfWidth, charHalfHeight, -charHalfWidth) + transform.position) ||
            GetBlock(new Vector3(charHalfWidth, charHalfHeight, charHalfWidth) + transform.position) ||
            virtualVelocityY > 0f
            )
        {
            if (GetBlock(new Vector3(-charHalfWidth, charHalfHeight + virtualVelocityY * Time.deltaTime + 0.5f * acceleration * Time.deltaTime * Time.deltaTime, -charHalfWidth) + transform.position) ||
            GetBlock(new Vector3(-charHalfWidth, charHalfHeight + virtualVelocityY * Time.deltaTime + 0.5f * acceleration * Time.deltaTime * Time.deltaTime, charHalfWidth) + transform.position) ||
            GetBlock(new Vector3(charHalfWidth, charHalfHeight + virtualVelocityY * Time.deltaTime + 0.5f * acceleration * Time.deltaTime * Time.deltaTime, -charHalfWidth) + transform.position) ||
            GetBlock(new Vector3(charHalfWidth, charHalfHeight + virtualVelocityY * Time.deltaTime + 0.5f * acceleration * Time.deltaTime * Time.deltaTime, charHalfWidth) + transform.position))
                virtualVelocityY = 0f;
        }
    }

    void GetFocusBlock()
    {
        Vector3 pos;
        for (float i=0; i <= interactDistance; i+=0.05f)
        {
            pos = cam.transform.position + cam.transform.forward * i;
            pos = new Vector3((int)pos.x, (int)pos.y, (int)pos.z);
            int temp = GeneralSettings.chunkWidth * GeneralSettings.worldSizeInChunks;
            if (!(pos.x >= 0 && pos.x < temp && pos.z >= 0 && pos.z < temp && pos.y >= 0 && pos.y < GeneralSettings.chunkHeight))
            {
                focusBlockPos = new Vector3Int(-1, 0, -1);
                focusBlockType = 0;
                breakBlockUIObject.transform.position = new Vector3(0.5f, 0.5f, 0.5f);
                placeBlockPos = new Vector3Int(-1, 0, -1);
                continue;
            }
            viewChunk = new Vector3Int((int)(pos.x / GeneralSettings.chunkWidth), 0, (int)(pos.z / GeneralSettings.chunkWidth));
            Vector3Int posRC = new Vector3Int((int)pos.x - viewChunk.x * GeneralSettings.chunkWidth, (int)pos.y, (int)pos.z - viewChunk.z * GeneralSettings.chunkWidth);
            if (world.IsPRCInChunk(posRC))
            {
                if (world.chunks[viewChunk.x, viewChunk.z].myBlocks[posRC.x, posRC.y, posRC.z].id != 0 && world.chunks[viewChunk.x, viewChunk.z].myBlocks[posRC.x, posRC.y, posRC.z].id != 5)
                {
                    focusBlockPos = posRC;
                    focusBlockType = world.chunks[viewChunk.x, viewChunk.z].myBlocks[posRC.x, posRC.y, posRC.z].id;
                    breakBlockUIObject.transform.position = pos + new Vector3(0.5f, 0.5f, 0.5f);

                    //Get Previous Block Position
                    pos = cam.transform.position + cam.transform.forward * (i-0.01f);
                    pos = new Vector3((int)pos.x, (int)pos.y, (int)pos.z);
                    temp = GeneralSettings.chunkWidth * GeneralSettings.worldSizeInChunks;
                    placeViewChunk = new Vector3Int((int)pos.x / GeneralSettings.chunkWidth, 0, (int)pos.z / GeneralSettings.chunkWidth);
                    posRC = new Vector3Int((int)pos.x - placeViewChunk.x * GeneralSettings.chunkWidth, (int)pos.y, (int)pos.z - placeViewChunk.z * GeneralSettings.chunkWidth);
                    if (world.chunks[placeViewChunk.x, placeViewChunk.z].myBlocks[posRC.x, posRC.y, posRC.z].id == 0)
                    {
                        placeBlockPos = posRC;
                        return;
                    }
                    return;
                }
            }
            else
                Debug.LogError("FATAL ERROR");
        }
        focusBlockPos = new Vector3Int(-1, 0, -1);
        focusBlockType = 0;
        placeBlockPos = new Vector3Int (-1, 0, -1);
        breakBlockUIObject.transform.position = new Vector3(0.5f, 0.5f, 0.5f);
    }

    private void ApplyWaterEffects()
    {
        byte blockID = GetBlockID(new Vector3(-charHalfWidth + 0.05f, charHalfHeight - 0.1f, -charHalfWidth + 0.05f) + transform.position);
        if (blockID == 5)
            waterEffect.SetActive(true);
        else
            waterEffect.SetActive(false);
            blockID = GetBlockID(new Vector3(-charHalfWidth + 0.05f, -charHalfHeight / 2f, -charHalfWidth + 0.05f) + transform.position);
        if (blockID == 5)
            inWater = true;
        else
            inWater = false;
    }

    private void ApplyGravity()
    {
        if (!GetBlock(new Vector3(-charHalfWidth + 0.05f, -charHalfHeight, -charHalfWidth + 0.05f) + transform.position) &&
            !GetBlock(new Vector3(-charHalfWidth + 0.05f, -charHalfHeight, charHalfWidth - 0.05f) + transform.position) &&
            !GetBlock(new Vector3(charHalfWidth - 0.05f, -charHalfHeight, -charHalfWidth + 0.05f) + transform.position) &&
            !GetBlock(new Vector3(charHalfWidth - 0.05f, -charHalfHeight, charHalfWidth - 0.05f) + transform.position)
            )
        {
            if (!GetBlock(new Vector3(-charHalfWidth + 0.05f, -charHalfHeight + virtualVelocityY * Time.deltaTime + 0.5f * acceleration * Time.deltaTime * Time.deltaTime, -charHalfWidth + 0.05f) + transform.position) &&
                !GetBlock(new Vector3(-charHalfWidth + 0.05f, -charHalfHeight + virtualVelocityY * Time.deltaTime + 0.5f * acceleration * Time.deltaTime * Time.deltaTime, charHalfWidth - 0.05f) + transform.position) &&
                !GetBlock(new Vector3(charHalfWidth - 0.05f, -charHalfHeight + virtualVelocityY * Time.deltaTime + 0.5f * acceleration * Time.deltaTime * Time.deltaTime, -charHalfWidth + 0.05f) + transform.position) &&
                !GetBlock(new Vector3(charHalfWidth - 0.05f, -charHalfHeight + virtualVelocityY * Time.deltaTime + 0.5f * acceleration * Time.deltaTime * Time.deltaTime, charHalfWidth - 0.05f) + transform.position)
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

    private void TakeInputs()
    {
        verMove = Input.GetAxisRaw("Vertical");
        horMove = Input.GetAxisRaw("Horizontal");

        camX = Input.GetAxisRaw("Mouse X");
        camY = -Input.GetAxisRaw("Mouse Y");

        //if (acceleration == 0f)
        {
            velocity = transform.forward * verMove + transform.right * horMove;
            velocity = velocity.normalized * walkSpeed;
        }

        if (Input.GetButtonDown("Jump") && acceleration == 0f)
            toJump = true;
        else if (Input.GetButton("Jump") && inWater)
            toJump = true;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            inventoryHighlightor.position = new Vector3 (0, 0, 0) + inventoryOffsetPos;
            selectedInventorySlot = Inventory[0].blockType;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            inventoryHighlightor.position = new Vector3(inventoryHighlightor.rect.size.x, 0, 0) +inventoryOffsetPos;
            selectedInventorySlot = Inventory[1].blockType;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            inventoryHighlightor.position = new Vector3(inventoryHighlightor.rect.size.x*2, 0, 0)+inventoryOffsetPos;
            selectedInventorySlot = Inventory[2].blockType;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            inventoryHighlightor.position  = new Vector3(inventoryHighlightor.rect.size.x * 3 , 0, 0) +inventoryOffsetPos;
            selectedInventorySlot = Inventory[3].blockType;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            inventoryHighlightor.position = new Vector3(inventoryHighlightor.rect.size.x * 4, 0, 0) + inventoryOffsetPos;
            selectedInventorySlot = Inventory[4].blockType;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            inventoryHighlightor.position = new Vector3(inventoryHighlightor.rect.size.x * 5, 0, 0) + inventoryOffsetPos;
            selectedInventorySlot = Inventory[5].blockType;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            inventoryHighlightor.position = new Vector3(inventoryHighlightor.rect.size.x * 6, 0, 0) + inventoryOffsetPos;
            selectedInventorySlot = Inventory[6].blockType;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            inventoryHighlightor.position = new Vector3(inventoryHighlightor.rect.size.x * 7, 0, 0) + inventoryOffsetPos;
            selectedInventorySlot = Inventory[7].blockType;
        }
        
        if (Input.GetKeyDown(KeyCode.Mouse0) && focusBlockType != 0)
        {
            world.chunks[viewChunk.x, viewChunk.z].myBlocks[focusBlockPos.x, focusBlockPos.y, focusBlockPos.z].id = 0;
            world.chunks[viewChunk.x, viewChunk.z].ClearChunkData();
            world.chunks[viewChunk.x, viewChunk.z].CreateChunk();
            UpdateSurroundingChunksForFocusBlock();
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1) && selectedInventorySlot != 0 && placeBlockPos != new Vector3Int (-1, 0, -1))
        {
            world.chunks[placeViewChunk.x, placeViewChunk.z].myBlocks[placeBlockPos.x, placeBlockPos.y, placeBlockPos.z].id = selectedInventorySlot;
            world.chunks[placeViewChunk.x, placeViewChunk.z].ClearChunkData();
            world.chunks[placeViewChunk.x, placeViewChunk.z].CreateChunk();
            UpdateSurroundingChunksForPlaceBlock();
        }
    }

    void UpdateSurroundingChunksForFocusBlock()
    {
        if (focusBlockPos.x == 0 && viewChunk.x >= 1)
        {
            Chunk chunk = world.chunks[viewChunk.x - 1, viewChunk.z];
            if (chunk != null)
            {
                chunk.ClearChunkData();
                chunk.CreateChunk();
            }
        }
        if (focusBlockPos.z == 0 && viewChunk.z >= 1)
        {
            Chunk chunk = world.chunks[viewChunk.x, viewChunk.z - 1];
            if (chunk != null)
            {
                chunk.ClearChunkData();
                chunk.CreateChunk();
            }
        }
        if (focusBlockPos.x == GeneralSettings.chunkWidth - 1 && viewChunk.x <= GeneralSettings.worldSizeInChunks - 2)
        {
            Chunk chunk = world.chunks[viewChunk.x + 1, viewChunk.z];
            if (chunk != null)
            {
                chunk.ClearChunkData();
                chunk.CreateChunk();
            }
        }
        if (focusBlockPos.z == GeneralSettings.chunkWidth - 1 && viewChunk.z <= GeneralSettings.worldSizeInChunks - 2)
        {
            Chunk chunk = world.chunks[viewChunk.x, viewChunk.z + 1];
            if (chunk != null)
            {
                chunk.ClearChunkData();
                chunk.CreateChunk();
            }
        }
    }

    void UpdateSurroundingChunksForPlaceBlock()
    {
        if (placeBlockPos.x == 0 && placeViewChunk.x >= 1)
        {
            Chunk chunk = world.chunks[placeViewChunk.x - 1, placeViewChunk.z];
            if (chunk != null)
            {
                chunk.ClearChunkData();
                chunk.CreateChunk();
            }
        }
        if (placeBlockPos.z == 0 && placeViewChunk.z >= 1)
        {
            Chunk chunk = world.chunks[placeViewChunk.x, placeViewChunk.z - 1];
            if (chunk != null)
            {
                chunk.ClearChunkData();
                chunk.CreateChunk();
            }
        }
        if (placeBlockPos.x == GeneralSettings.chunkWidth - 1 && placeViewChunk.x <= GeneralSettings.worldSizeInChunks - 2)
        {
            Chunk chunk = world.chunks[placeViewChunk.x + 1, placeViewChunk.z];
            if (chunk != null)
            {
                chunk.ClearChunkData();
                chunk.CreateChunk();
            }
        }
        if (placeBlockPos.z == GeneralSettings.chunkWidth - 1 && placeViewChunk.z <= GeneralSettings.worldSizeInChunks - 2)
        {
            Chunk chunk = world.chunks[placeViewChunk.x, placeViewChunk.z + 1];
            if (chunk != null)
            {
                chunk.ClearChunkData();
                chunk.CreateChunk();
            }
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
                acceleration = (charHalfHeight * charHalfWidth * charHalfWidth * 8f * (-gravity) / charMass + gravity);
                virtualVelocityY = Mathf.Sqrt(-2f * acceleration * jumpHeight / 3f);
            }

            toJump = false;
        }

        transform.position += (velocity + new Vector3(0f, virtualVelocityY, 0f)) * Time.deltaTime + 0.5f * new Vector3(0f, acceleration, 0f) * Time.deltaTime * Time.deltaTime;
        virtualVelocityY += acceleration * Time.deltaTime;
    }

    private void RotateCamera()
    {
        transform.Rotate(new Vector3(0f, camX, 0f) * Time.deltaTime * mouseSensitivityX);
        currRot += camY * Time.deltaTime * mouseSensitivityY;
        if (currRot <= 90f && currRot >= -90f)
            cam.transform.Rotate(new Vector3(camY, 0f, 0f) * Time.deltaTime * mouseSensitivityY);
        else
            currRot -= camY * Time.deltaTime * mouseSensitivityY;
    }

    private bool GetBlock(Vector3 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        int playerChunkX = x / GeneralSettings.chunkWidth;
        int playerChunkZ = z / GeneralSettings.chunkWidth;

        x -= playerChunkX * GeneralSettings.chunkWidth;
        z -= playerChunkZ * GeneralSettings.chunkWidth;

        if (!world.IsPRCInChunk(new Vector3(x, y, z)))
            return false;

        return world.blockTypes[world.chunks[playerChunkX, playerChunkZ].myBlocks[x, y, z].id].isSolid;
    }

    private byte GetBlockID(Vector3 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        int playerChunkX = x / GeneralSettings.chunkWidth;
        int playerChunkZ = z / GeneralSettings.chunkWidth;

        x -= playerChunkX * GeneralSettings.chunkWidth;
        z -= playerChunkZ * GeneralSettings.chunkWidth;

        if (!world.IsPRCInChunk(new Vector3(x, y, z)))
            return 0;

        return world.chunks[playerChunkX, playerChunkZ].myBlocks[x, y, z].id;
    }
}