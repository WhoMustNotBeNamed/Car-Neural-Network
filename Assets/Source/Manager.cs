using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public float timeframe; // Время для кадра
    public int populationSize;// Создает размер популяции
    public GameObject prefab;// Содержит префаб бота

    public int[] layers = new int[3] { 5, 3, 2 };// Инициализация сети нужного размера

    [Range(0.0001f, 1f)] public float MutationChance = 0.01f;// Шанс мутации

    [Range(0f, 1f)] public float MutationStrength = 0.5f;// Сила мутации

    [Range(0.1f, 10f)] public float Gamespeed = 1f;// Скорость игры

    public List<NeuralNetwork> networks;
    private List<Bot> cars;

    void Start()// Start вызывается перед первым обновлением кадра
    {
        if (populationSize % 2 != 0)
            populationSize = 50;// Если размер популяции нечетный, устанавливает его равным пятидесяти

        InitNetworks();
        InvokeRepeating("CreateBots", 0.1f, timeframe);// Повторяющаяся функция
    }

    public void InitNetworks()
    {
        networks = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Load("Assets/ModelSave.txt");// При запуске загружает сохранение сети
            networks.Add(net);
        }
    }

    public void CreateBots()
    {
        Time.timeScale = Gamespeed;// Устанавливает скорость игры, которая будет увеличиваться для ускорения обучения
        if (cars != null)
        {
            for (int i = 0; i < cars.Count; i++)
            {
                GameObject.Destroy(cars[i].gameObject);// Если в сцене есть префабы, они будут уничтожены
            }

            SortNetworks();// Сортирует сети и мутирует их
        }

        cars = new List<Bot>();
        for (int i = 0; i < populationSize; i++)
        {
            Bot car = (Instantiate(prefab, new Vector3(0, 1.6f, -16), new Quaternion(0, 0, 1, 0))).GetComponent<Bot>();// Создает ботов
            car.network = networks[i];// Передает сеть каждому обучающемуся
            cars.Add(car);
        }
        
    }

    public void SortNetworks()
    {
        for (int i = 0; i < populationSize; i++)
        {
            cars[i].UpdateFitness();// Заставляет ботов установить соответствующую эффективность их сетей
        }
        networks.Sort();
        networks[populationSize - 1].Save("Assets/ModelSave.txt");// Сохраняет веса и смещения сетей в файле для сохранения производительности сети
        for (int i = 0; i < populationSize / 2; i++)
        {
            networks[i] = networks[i + populationSize / 2].copy(new NeuralNetwork(layers));
            networks[i].Mutate((int)(1/MutationChance), MutationStrength);
        }
    }
}
