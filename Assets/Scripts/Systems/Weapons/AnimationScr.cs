using UnityEngine;

public class AnimationScr : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private float animationFPS = 12f;
    [SerializeField] private SpriteRenderer sr;

    private float animationTimer;
    private int currentFrame;

    public void SwapSprites(Sprite[] newSprites)
    {
        sprites = newSprites;
    }

    void Update()
    {
        Animate();
    }

    private void Animate()
    {
        if (sprites.Length == 0)
            return;

        animationTimer += Time.deltaTime;

        float frameDuration = 1f / animationFPS;

        if (animationTimer >= frameDuration)
        {
            animationTimer -= frameDuration;

            currentFrame++;
            if (currentFrame >= sprites.Length)
                currentFrame = 0;

            sr.sprite = sprites[currentFrame];
        }
    }
}
