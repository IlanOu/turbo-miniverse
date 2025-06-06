using System.Collections.Generic;
using UnityEngine;

namespace Car
{
    public class ChooseCar : MonoBehaviour
    {
        public GameObject car1;
        public GameObject car1WindGameObject;

        public GameObject car2;
        public GameObject car2WindGameObject;

        public GameObject car3;
        public GameObject car3WindGameObject;

        public SmoothCamera smoothCamera;
        public DynamicFOVController dynamicFOVController;
        public SpeedParticleController speedParticleController;

        public void Start()
        {
            ChangeCar(1);
        }
        
        public void ChangeCar(int carNumber)
        {
            if (carNumber == 1)
            {
                smoothCamera.target = car1.transform;
                dynamicFOVController.target = car1.transform;
                speedParticleController.target = car1.transform;
                speedParticleController.speedEffects =
                    new List<ParticleSystem>(car1WindGameObject.GetComponentsInChildren<ParticleSystem>());
                car1.SetActive(true);
                car2.SetActive(false);
                car3.SetActive(false);
            }
            else if (carNumber == 2)
            {
                smoothCamera.target = car2.transform;
                dynamicFOVController.target = car2.transform;
                speedParticleController.target = car2.transform;
                speedParticleController.speedEffects =
                    new List<ParticleSystem>(car2WindGameObject.GetComponentsInChildren<ParticleSystem>());

                car1.SetActive(false);
                car2.SetActive(true);
                car3.SetActive(false);
            }
            else if (carNumber == 3)
            {
                smoothCamera.target = car3.transform;
                dynamicFOVController.target = car3.transform;
                speedParticleController.target = car3.transform;
                speedParticleController.speedEffects =
                    new List<ParticleSystem>(car3WindGameObject.GetComponentsInChildren<ParticleSystem>());

                car1.SetActive(false);
                car2.SetActive(false);
                car3.SetActive(true);
            }
        }
    }
}