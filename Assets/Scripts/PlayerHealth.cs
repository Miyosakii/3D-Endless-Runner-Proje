using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [SerializeField] private float invincibilityDuration = 1f;
    private float invincibilityTimer = 0f;
    private bool isInvincible = false;

    [SerializeField] private Renderer playerRenderer; // görsel geri bildirim için
    [SerializeField] private Color damageColor = Color.red;

    private void Start()
    {
        currentHealth = maxHealth;
        GameEvents.TriggerPlayerDamaged(currentHealth);
    }

    private void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                if (playerRenderer != null)
                    playerRenderer.material.color = Color.white;
            }
        }
    }

    public void TakeDamage(int damageAmount = 1)
    {
        if (isInvincible) return;
        if (GameManager.Instance.CurrentState == GameState.GameOver) return;

        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        if (playerRenderer != null)
            playerRenderer.material.color = damageColor;

        GameEvents.TriggerPlayerDamaged(currentHealth);

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        if (currentHealth <= 0)
        {
            GameManager.Instance.GameOver();
        }
    }

    // Tek bir OnTriggerEnter metodu – tag ile ayýrým yapýyoruz
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectible"))
        {
            Collectible collectible = other.GetComponent<Collectible>();
            if (collectible != null)
            {
                GameEvents.TriggerCollectibleCollected(collectible.value);
                other.gameObject.SetActive(false); // Toplanan objeyi devre dýþý býrak
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("TrackBlock"))
        {
            isInvincible = false; // Zeminle temas ettiðinde hasar alabilir hale gelir
            if (playerRenderer != null)
                playerRenderer.material.color = Color.white; // Rengi eski haline getir
        }

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            TakeDamage(1);
            // other.gameObject.SetActive(false); // Havuza geri göndermek isterseniz
        }
    }
}