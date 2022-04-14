using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Inspector;

namespace BoardGame
{
    public enum TileStatus
    {
        Idle,
        Active,
        Selected,
        Burning,
        Healing
    }

    public class TileBehaviour : MonoBehaviour
    {
        [Header("Alternative Sprites")]
        public Sprite spriteIdle;
        public Sprite spriteActive;
        public Sprite spriteSelected;
        public Sprite spriteBurningIdle;
        public Sprite spriteBurningActive;
        public Sprite spriteBurningSelected;

        private SpriteRenderer spriteRenderer;
        [Header("Tile Information")]
        [SerializeField]
        [ReadOnly] private GameObject objectOnTile;
        [ReadOnly] public int xCoordinate;
        [ReadOnly] public int yCoordinate;

        [SerializeField]
        [ReadOnly] private List<TileStatus> statusList = new List<TileStatus>();

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            AddStatus(TileStatus.Idle);
        }

        public void AddStatus(TileStatus tileStatus)
        {
            if (!HasStatus(tileStatus))
            {
                statusList.Add(tileStatus);
            }
            UpdateTileColor();
        }

        public void RemoveStatus(TileStatus tileStatus)
        {
            if (statusList.Contains(tileStatus))
            {
                statusList.Remove(tileStatus);
            }

            UpdateTileColor();
        }

        private void UpdateTileColor()
        {
            if (statusList.Contains(TileStatus.Selected))
            {
                if (statusList.Contains(TileStatus.Burning))
                {
                    spriteRenderer.sprite = spriteBurningSelected;
                }
                else
                {
                    spriteRenderer.sprite = spriteSelected;
                }
            }
            else if (statusList.Contains(TileStatus.Active))
            {
                if (statusList.Contains(TileStatus.Burning))
                {
                    spriteRenderer.sprite = spriteBurningActive;
                }
                else
                {
                    spriteRenderer.sprite = spriteActive;
                }
            }
            else if (statusList.Contains(TileStatus.Burning))
            {
                spriteRenderer.sprite = spriteBurningIdle;
            }
            else
            {
                spriteRenderer.sprite = spriteIdle;
            }
        }

        public bool HasStatus(TileStatus tileStatus)
        {
            return statusList.Contains(tileStatus);
        }

        public GameObject GetObjectOnTile()
        {
            if (objectOnTile) return objectOnTile;

            return null;
        }

        public void SetObjectOnTile(GameObject objectNew)
        {
            objectOnTile = objectNew;
        }

        public string Print()
        {
            return "Tile(" + xCoordinate + ", " + yCoordinate + ")";
        }

        public void ResetTile()
        {
            statusList = new List<TileStatus>();
            objectOnTile = null;
            spriteRenderer.sprite = spriteIdle;
        }
    }
}

