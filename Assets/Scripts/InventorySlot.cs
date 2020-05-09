using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Texture2D icon;
    public int texture;
    public byte blockType;
    Image image;

    private void Start()
    {
        image = GetComponent<Image>();
        AddTexture(texture);
    }

    private void AddTexture(int ID)
    {
        if (ID < 0)
        {
            image.color = new Color (1, 1, 1, 0);
            return;
        }
        int y = ID / GeneralSettings.TextureSize;
        int x = ID - y * 16;

        image.sprite = Sprite.Create(icon, new Rect(x*GeneralSettings.TextureSize, GeneralSettings.TextureSize * (15 - y), 16, 16), new Vector2(0, 0));
    }
}
