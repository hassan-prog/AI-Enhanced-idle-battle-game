using BattleGame.Scripts;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 300f;

    [Header("Shield Settings")]
    public GameObject shieldVisual;
    public float shieldDuration = 5f;
    public float shieldCooldown = 10f;

    [Header("Spawn Point")]
    public Transform bulletSpawnPoint;

    [Header("Enemy References")]
    public Transform SlimeEnemy;
    public Transform batEnemy;

    private float nextFireTime;
    private float nextShieldTime;
    private bool isShieldActive;
    private Transform currentTarget;
    private float rotationOffset = -90f;

    private void Start()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
        }

        // Set up bullet spawn point
        if (bulletSpawnPoint == null)
        {
            GameObject spawnPoint = new GameObject("BulletSpawnPoint");
            bulletSpawnPoint = spawnPoint.transform;
            bulletSpawnPoint.SetParent(transform);
            bulletSpawnPoint.localPosition = Vector3.zero;
        }

        // Ensure shield is initially disabled and has proper setup
        if (shieldVisual != null)
        {
            shieldVisual.tag = "Shield";
            // Add collider to shield if it doesn't have one
            BoxCollider2D shieldCollider = shieldVisual.GetComponent<BoxCollider2D>();
            if (shieldCollider == null)
            {
                shieldCollider = shieldVisual.AddComponent<BoxCollider2D>();
                shieldCollider.isTrigger = true;
            }
            shieldVisual.SetActive(false);
        }

        // Set the player tag
        gameObject.tag = "Player";
    }

    private void Update()
    {
        if (currentTarget != null)
        {
            // Continuously update rotation to track the target
            Vector2 direction = EnemyFunctions.GetDirectionToEnemy(bulletPrefab.transform, currentTarget);
            float angle = EnemyFunctions.GetRotationAngle(direction);
            bulletPrefab.transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        // Update shield status
        if (isShieldActive && Time.time >= nextShieldTime)
        {
            DeactivateShield();
        }
    }

    public void ActivateShield()
    {
        if (Time.time >= nextShieldTime && !isShieldActive)
        {
            isShieldActive = true;
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(true);
            }
            nextShieldTime = Time.time + shieldDuration;
        }
    }

    private void DeactivateShield()
    {
        isShieldActive = false;
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
        nextShieldTime = Time.time + shieldCooldown;
    }

    public bool IsShieldAvailable()
    {
        return Time.time >= nextShieldTime && !isShieldActive;
    }

    public bool IsShieldActive()
    {
        return isShieldActive;
    }

    private void FireBullet()
    {
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.transform.rotation);
            Debug.Log("ay 7aga");
            bullet.transform.SetParent(transform.parent);

            if (currentTarget != null)
            {
                //direction = EnemyFunctions.GetDirectionToEnemy(bulletPrefab.transform, currentTarget);
                bullet.GetComponent<Bullet>().SetDirection(currentTarget.position);
            }

            Destroy(bullet, 3f);
        }
    }

    public void TargetEnemy(string enemyType)
    {
        currentTarget = null;
        switch (enemyType.ToLower())
        {
            case "slime":
                currentTarget = SlimeEnemy;
                break;
            case "bat":
                currentTarget = batEnemy;
                break;
        }

        if (currentTarget != null)
        {
            // Initial rotation towards target
            Vector2 direction = EnemyFunctions.GetDirectionToEnemy(bulletSpawnPoint.transform, currentTarget);
            float angle = EnemyFunctions.GetRotationAngle(direction);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void Fire()
    {
        FireBullet();
    }
}
