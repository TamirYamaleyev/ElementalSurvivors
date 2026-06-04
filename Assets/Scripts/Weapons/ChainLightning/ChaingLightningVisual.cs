using UnityEngine;

public class ChaingLightningVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float offset = 0.4f;
    [SerializeField] private float thickness = 1f;

    public void Initialize(Vector2 start, Vector2 end, Sprite sprite, float lifetime)
    {
        Vector2 dir = (end - start).normalized;

        start += dir * offset;
        end -= dir * offset;

        Vector2 mid = (start +  end) * 0.5f;

        transform.position = mid;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        float distance = Vector2.Distance(start, end);
        float spriteHeight = sr.sprite.bounds.size.y;

        //transform.right = dir.normalized;

        // Stretch between enemies
        transform.localScale = new Vector3(thickness, distance / spriteHeight, 1f);

        sr.sprite = sprite;

        Destroy(gameObject, lifetime);
    }
}
