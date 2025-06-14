using System.Collections.Generic;
using Menu;
using UnityEngine;

namespace Car
{
    public class ChooseCar : MonoBehaviour
    {
        public List<GameObject> cars = new List<GameObject>();
        public SpeedDisplay speedDisplay;
        
        public void Start()
        {
            ChangeCar(0);
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
            cars[carIndex].GetComponentInChildren<CarController>().speedDisplay = speedDisplay;
            speedDisplay.Initialize(cars[carIndex].GetComponentInChildren<Rigidbody>());
        }
    }
}