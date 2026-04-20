using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDefaultWeapon : MonoBehaviour
{
    [SerializeField] private Transform spear;
    [SerializeField] private Camera cam;

    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackRadius = 2f;
    [SerializeField] private float activeTime = 0.12f;

    private float timer;

    private Vector2 lastDirection = Vector2.right;

    void Update()
    {
        UpdateDirection();

        timer += Time.deltaTime;
        
        if (timer >= attackCooldown)
        {
            timer = 0f;
            StartCoroutine(Attack());
        }
    }

    private void UpdateDirection()
    {
        if (Mouse.current == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector2 dir = mouseWorldPos - transform.position;

        if (dir.sqrMagnitude > 0.0001f)
            lastDirection = dir.normalized;
    }

    private IEnumerator Attack()
    {
        Vector2 dir = lastDirection;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        spear.localPosition = dir * attackRadius;
        spear.localRotation = Quaternion.Euler(0, 0, angle);

        spear.gameObject.SetActive(true);

        yield return new WaitForSeconds(activeTime);

        spear.gameObject.SetActive(false);
    }
}
