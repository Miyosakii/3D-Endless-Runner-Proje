using UnityEngine;

public class GoldRotate : MonoBehaviour
{
    private float rotationSpeed = 100f;

    void Update()
    {
        // Dünya ekseninde (Y ekseni etrafýnda) döndürmek, 
        // dinamik oluþturulan objelerde daha stabil sonuç verir.
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}