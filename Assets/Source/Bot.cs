using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot : MonoBehaviour
{
    public float speed;// Множитель скорости
    public float rotation;// Множитель поворота
    public LayerMask raycastMask;// Маска для сенсоров

    private float[] input = new float[5];// Входные данные для нейронной сети
    public NeuralNetwork network;

    public int position;// Номер чекпоинта на трассе
    public bool collided;// Показывает, столкнулась ли машина с препятствием

    void FixedUpdate()// FixedUpdate вызывается с постоянным интервалом
    {
        if (!collided)// Если машина не столкнулась со стеной, она использует нейронную сеть для получения выходных данных
        {
            for (int i = 0; i < 5; i++)// Рисует пять отладочных лучей как входные данные
            {
                Vector3 newVector = Quaternion.AngleAxis(i * 45 - 90, new Vector3(0, 1, 0)) * transform.right;// Вычисляет угол луча
                RaycastHit hit;
                Ray Ray = new Ray(transform.position, newVector);

                if (Physics.Raycast(Ray, out hit, 10, raycastMask))
                {
                    input[i] = (10 - hit.distance) / 10;// Возвращает расстояние, где 1 - близко
                }
                else
                {
                    input[i] = 0;// Если ничего не обнаружено, возвращает 0 в сеть
                }
            }

            float[] output = network.FeedForward(input);// Вызов сети для прямого распространения

            transform.Rotate(0, output[0] * rotation, 0, Space.World);// Управляет движением машины
            transform.position += this.transform.right * output[1] * speed;// Управляет поворотом машины
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.gameObject.layer == LayerMask.NameToLayer("CheckPoint"))// Проверяет, прошла ли машина ворота
        {
            GameObject[] checkPoints = GameObject.FindGameObjectsWithTag("CheckPoint");
            for (int i=0; i < checkPoints.Length; i++)
            {
                if(collision.collider.gameObject == checkPoints[i] && i == (position + 1 + checkPoints.Length) % checkPoints.Length)
                {
                    position++;// Если ворота впереди, увеличивает позицию, которая используется для оценки эффективности сети
                    break;
                }
            }
        }
        else if(collision.collider.gameObject.layer != LayerMask.NameToLayer("Learner"))
        {
            collided = true;// Остановить работу, если машина столкнулась
        }
    }

    public void UpdateFitness()
    {
        network.fitness = position;// Обновляет эффективность сети для сортировки
    }
}
