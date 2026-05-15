using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private float activeTime = 0.15f;
    [SerializeField] private float radius = 1.5f;

    [Header("Refs")]
    [SerializeField] private Transform visual;

    private float timer;
    private Vector2 direction = Vector2.right;

    public void SetDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude > 0.0001f)
            direction = dir.normalized;
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= cooldown)
        {
            timer = 0f;
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        visual.localPosition = direction * radius;
        visual.localRotation = Quaternion.Euler(0, 0, angle);

        visual.gameObject.SetActive(true);

        yield return new WaitForSeconds(activeTime);

        visual.gameObject.SetActive(false);
    }
}
