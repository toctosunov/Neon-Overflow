using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class BlitzTimer : MonoBehaviour
{
    public static Action OnTimeEnd;

    [SerializeField] private Image timerBar; // Перетащи сюда свой объект Image
    [SerializeField] private float duration = 20f; // Время в секундах
    [SerializeField] private float _bonusSeconds;

    private float currentTime;
    private bool isGameOver = false;
    private bool isPanicModeActive = false;

    private void OnEnable()
    {
        Ball.OnDestroyed += AddBonusTime;
    }

    private void OnDisable()
    {
        Ball.OnDestroyed -= AddBonusTime;
    }

    void Start()
    {
        currentTime = duration;
        timerBar.fillAmount = 1f;
    }

    void Update()
    {
        if (isGameOver) return;

        currentTime -= Time.deltaTime;
        timerBar.fillAmount = currentTime / duration;

        // Если осталось меньше 10 секунд — включаем режим паники
        if (currentTime <= 10f && !isPanicModeActive)
        {
            TriggerPanicMode();
        }

        if (currentTime <= 0)
        {
            GameOver();
        }
    }

    private void TriggerPanicMode()
    {
        if (timerBar.color != Color.red)
        {
            timerBar.DOColor(Color.red, 0.5f);
            timerBar.transform.DOScaleY(2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void AddBonusTime()
    {
        // Добавляем время
        currentTime += _bonusSeconds;

        // Ограничиваем, чтобы время не превышало максимальное значение
        if (currentTime > duration) currentTime = duration;

        // Если время стало больше 10 секунд, а режим паники был включен — выключаем его
        if (currentTime > 10f && isPanicModeActive)
        {
            StopPanicMode();
        }
    }

    private void StopPanicMode()
    {
        isPanicModeActive = false;

        // Останавливаем все анимации DOTween, чтобы они не конфликтовали
        timerBar.transform.DOKill();

        // Возвращаем нормальный цвет и масштаб
        timerBar.DOColor(Color.white, 0.5f); // Замени Color.white на свой стандартный цвет полоски
        timerBar.transform.DOScaleY(1f, 0.3f);
    }

    private void GameOver()
    {
        isGameOver = true;
        OnTimeEnd?.Invoke();
    }
}