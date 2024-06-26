using UnityEngine;
using System.Collections;
using System;

public class BatManager : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform launcherPoint;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform batHandleCenter;
    [SerializeField] private Animation batHandleAnimation;
    [SerializeField] private float launchVelocity = 15;
    private Vector3 mousePos;
    private const float animDuration = 0.1f;
    private bool isBallShot = false;
    private bool isMenuOn;

    private void Awake()
    {
        UiLevelManagerCanvas.OnHideBat += OnHideBat;
        UiLevelClearedCanvas.OnLevelClearedContinueButtonPressed += OnLevelClearedContinueButtonPressed;
        UiLevelManagerCanvas.OnShootBall += OnShootBall;
    }

    private void OnDestroy()
    {
        UiLevelManagerCanvas.OnHideBat -= OnHideBat;
        UiLevelClearedCanvas.OnLevelClearedContinueButtonPressed -= OnLevelClearedContinueButtonPressed;
        UiLevelManagerCanvas.OnShootBall -= OnShootBall;
    }

    private void OnHideBat()
    {
        isMenuOn = true;
        batHandleCenter.gameObject.SetActive(false);
    }

    private void OnLevelClearedContinueButtonPressed()
    {
        isMenuOn = false;
        batHandleCenter.gameObject.SetActive(true);
    }

    private void OnShootBall()
    {
        if (isBallShot || isMenuOn)
        {
            return;
        }
        mousePos = Input.mousePosition;
        mousePos.z = 10;
        target.position = Camera.main.ScreenToWorldPoint(mousePos);
        transform.LookAt(target);
        StartCoroutine(SwingBatAndShoot());
    }

    private IEnumerator SwingBatAndShoot()
    {
        isBallShot = true;
        yield return new WaitForEndOfFrame();
        batHandleAnimation.Play();
        yield return new WaitForSeconds(animDuration);
        if (Player.IsVibrateEnabled)
        {
            Vibration.Vibrate(50);
        }
        GameObject ball = Instantiate(ballPrefab, launcherPoint.position, launcherPoint.rotation);
        ball.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, launchVelocity * 100, 0));
        isBallShot = false;
    }
}