using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int damage = 10;
    private float speed = 20f;
    
    public void SetParameters(int newDamage, float newSpeed)
    {
        damage = newDamage;
        speed = newSpeed;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Vérifier si l'objet a un composant de santé pour appliquer des dégâts
        // Health health = collision.gameObject.GetComponent<Health>();
        // if (health != null)
        // {
        //     health.TakeDamage(damage);
        // }
        
        // Détruire la balle à l'impact
        // Destroy(gameObject);
    }
}