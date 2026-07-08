using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 5f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float speedIncreaseRate = 0.1f;
    [SerializeField] private float laneWidth = 2.5f;
    [SerializeField] private float laneChangeSpeed = 10f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float crouchHeight = 0.5f; // eðilme yüksekliði

    private Vector3 targetPosition;
    private int currentLane = 1; // 0 = sol, 1 = orta, 2 = sað
    private float originalHeight;
    private bool isCrouching = false;
    private bool isGrounded = true;
    private Rigidbody rb;

    // Lane deðiþimi tamamlanana kadar yeni giriþleri engelle
    private bool isChangingLane = false;
    private const float laneSnapThreshold = 0.01f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalHeight = transform.localScale.y;
        targetPosition = transform.position;
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing)
            return;

        // Hýzlanma
        if (forwardSpeed < maxSpeed)
            forwardSpeed += speedIncreaseRate * Time.deltaTime;

        // Ýleri hareket (Z ekseninde)
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);

        // Giriþler: sadece deðiþim yapýlabiliyorsa yeni lane kabul et
        if (!isChangingLane)
        {
            // Kesin tuþ basýmlarý (A/D, sol/sað oklarý)
            if ((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) && currentLane > 0)
            {
                currentLane--;
                UpdateTargetPosition();
                isChangingLane = true;
            }
            else if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) && currentLane < 2)
            {
                currentLane++;
                UpdateTargetPosition();
                isChangingLane = true;
            }
            else
            {
                // Analog çubuk veya joystick için eþik kontrollü tek seferlik hareket
                float horizontalInput = Input.GetAxisRaw("Horizontal");
                if (horizontalInput <= -0.5f && currentLane > 0)
                {
                    currentLane--;
                    UpdateTargetPosition();
                    isChangingLane = true;
                }
                else if (horizontalInput >= 0.5f && currentLane < 2)
                {
                    currentLane++;
                    UpdateTargetPosition();
                    isChangingLane = true;
                }
            }
        }

        // Zýplama
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        // Eðilme
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            isCrouching = true;
            transform.localScale = new Vector3(transform.localScale.x, crouchHeight, transform.localScale.z);
        }
        if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
        {
            isCrouching = false;
            transform.localScale = new Vector3(transform.localScale.x, originalHeight, transform.localScale.z);
        }

        // Lane geçiþini yumuþat
        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(newPos.x, targetPosition.x, laneChangeSpeed * Time.deltaTime);
        transform.position = newPos;

        // Geçiþ tamamlandýðýnda kilidi kaldýr (hassas eþik ile snap)
        if (isChangingLane && Mathf.Abs(transform.position.x - targetPosition.x) <= laneSnapThreshold)
        {
            var p = transform.position;
            p.x = targetPosition.x;
            transform.position = p;
            isChangingLane = false;
        }
    }

    private void UpdateTargetPosition()
    {
        float xPos = (currentLane - 1) * laneWidth;
        targetPosition = new Vector3(xPos, transform.position.y, transform.position.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("TrackBlock"))
            isGrounded = true;
    }

    // Trigger ile toplama veya engel çarpýþmasý PlayerHealth'de yapýlacak.
}