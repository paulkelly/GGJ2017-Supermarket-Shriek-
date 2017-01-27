﻿using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreenUI : MonoBehaviour
{
    public CanvasGroup TitleCanvas;
    public Animator GameUIAnimator;
    public CanvasGroup GameUICanvas;
    public Camera TitleScreenCamera;
    public Animator TitleAnimator;

    public CanvasGroup GameBlack;

    private bool _started;
    private bool _finished;
    private bool _win;

    private float _blackTime = 0.4f;
    private float _blackTimer = 0;

    private float _endTimer = 0;
    private float _endWaitTime = 1f;

    private bool _finishedAndWaitingForRestart = false;
    private bool _restarting = false;

    private void Awake()
    {
    }

    private void Start()
    {
        StartCoroutine(WaitForIntro());
    }

    private GameState _lastGameState = GameState.Intro;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GlobalMics.Instance.State = GameState.Win;
        }

        if(GlobalMics.Instance.State != _lastGameState)
        {
            if (GlobalMics.Instance.State == GameState.Starting)
            {
                GameStarting();
            }
            else if (GlobalMics.Instance.State == GameState.Started)
            {
                GameStarted();
            }
            else if (GlobalMics.Instance.State == GameState.Finished)
            {
                GameFinished();
            }
            if (GlobalMics.Instance.State == GameState.Win)
            {
                GameWon();
            }
        }
        _lastGameState = GlobalMics.Instance.State;

        if (GlobalMics.Instance.State == GameState.NotStarted)
        {
            bool pausePressed = false;
            foreach (Player player in ReInput.players.AllPlayers)
            {
                pausePressed |= player.GetButtonDown(RewiredConsts.Action.Pause);
            }

            pausePressed |= ReInput.players.SystemPlayer.GetButtonDown(RewiredConsts.Action.Pause);

            if (pausePressed)
            {
                if (Options)
                {
                    HideOptions();
                }
                else
                {
                    ShowOptions();
                }
            }
        }

        if (_finishedAndWaitingForRestart && !_restarting)
        {
            if (GlobalMics.Instance.Player1Volume > 0.8f || GlobalMics.Instance.Player2Volume > 0.8f)
            {
                StartCoroutine(RestartGame());
            }
        }
    }

    private bool Options
    {
        get { return TitleAnimator.GetBool("Options"); }

        set { TitleAnimator.SetBool("Options", value);}
    }

    public void ShowOptions()
    {
        Options = true;
    }

    public void HideOptions()
    {
        Options = false;
    }

    private void GameStarting()
    {
        TitleAnimator.SetTrigger("Start");
        StartCoroutine(WaitForOutAnimation());
    }

    private void GameStarted()
    {
        TitleCanvas.alpha = 0;
        GameUICanvas.alpha = 1;
        GameUIAnimator.SetTrigger("Go");
        TitleScreenCamera.gameObject.SetActive(false);
    }

    private void GameFinished()
    {
        _finished = true;
        StartCoroutine(RestartGame());
    }

    private void GameWon()
    {
        _finished = true;
        GameUIAnimator.SetTrigger("Win");
        StartCoroutine(WaitForWinAnimation());
    }

    public void Restart()
    {
        StartCoroutine(RestartGame());
    }

    private IEnumerator WaitForIntro()
    {
        while (!TitleAnimator.GetCurrentAnimatorStateInfo(0).IsName("In"))
        {
            yield return null;
        }

        while (TitleAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.7f)
        {
            yield return null;
        }

        GlobalMics.Instance.State = GameState.NotStarted;
    }

    private IEnumerator WaitForOutAnimation()
    {
        while (!TitleAnimator.GetCurrentAnimatorStateInfo(0).IsName("Out"))
        {
            yield return null;
        }

        while (TitleAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.95f)
        {
            yield return null;
        }

        GlobalMics.Instance.State = GameState.Started;
    }

    private IEnumerator WaitForWinAnimation()
    {
        while (!TitleAnimator.GetCurrentAnimatorStateInfo(0).IsName("Winner"))
        {
            yield return null;
        }

        while (TitleAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.9f)
        {
            yield return null;
        }

        _finishedAndWaitingForRestart = true;
    }

    private float _fadeOutTimer = 0;
    private float _fadeOutTime = 2;
    private IEnumerator RestartGame()
    {
        _restarting = true;
        Image blackImage = GameBlack.GetComponent<Image>();
        blackImage.color = new Color(blackImage.color.r, blackImage.color.g, blackImage.color.b, 1);
        GameBlack.alpha = 0;
        while (_fadeOutTimer < _fadeOutTime)
        {
            _fadeOutTimer += Time.deltaTime;
            GameBlack.alpha = Mathf.Lerp(0, 1, _fadeOutTimer / _fadeOutTime);
            yield return null;
        }
        GameBlack.alpha = 1;

        yield return new WaitForSeconds(0.5f);
        GlobalMics.Instance.State = GameState.NotStarted;
        SceneManager.LoadScene(0);
    }
}
