using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    Vector2Int playerChunk;
    Vector3Int playerCoords;
    Text debugText;

    public Transform player;

    // Start is called before the first frame update
    void Start()
    {
        debugText = GetComponentInChildren<Text>();
        if (debugText == null)
            enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        playerCoords = new Vector3Int((int)player.transform.position.x, (int)player.transform.position.y, (int)player.transform.position.z);
        playerChunk = new Vector2Int(playerCoords.x / GeneralSettings.chunkWidth, playerCoords.z / GeneralSettings.chunkWidth);

        debugText.text = "Debug Screen";
        debugText.text += "\nX Y Z: " + playerCoords.x + " " + playerCoords.y + " " + playerCoords.z+"\n";
        debugText.text += "Chunk X Z: " + playerChunk.x + " " + playerChunk.y+ "\n";
    }
}
