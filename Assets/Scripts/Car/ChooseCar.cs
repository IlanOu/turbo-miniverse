using System.Collections.Generic;
using UnityEngine;

namespace Car
{
    public class ChooseCar : MonoBehaviour
    {
        public List<GameObject> cars = new List<GameObject>();
        
        public void Start()
        {
            ChangeCar(1);
        }
        
        public void ChangeCar(int carIndex)
        {
            if (carIndex < 0 || carIndex >= cars.Count)
            {
                Debug.LogError("Index de voiture invalide: " + carIndex);
                return;
            }
            
            foreach (GameObject car in cars)
            {
                car.SetActive(false);
            }
            
            cars[carIndex].SetActive(true);
        }
    }
}