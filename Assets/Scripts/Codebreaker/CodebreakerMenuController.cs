using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CodebreakerMenuController : MonoBehaviour
{
    [Header("Game")]
    [SerializeField] private CodebreakerGameController gameController;
    [SerializeField] private GlobalBombTimer globalTimer;
    [SerializeField]
    private CodebreakerEquationInteractionController equationInteraction;
    [SerializeField] private GameObject gameplayHudRoot;

    [Header("Menus")]
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject pauseMenuRoot;

    [Header("Selection")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainQuitButton;
    [SerializeField] private Button pauseQuitButton;

    private bool timerWasRunningBeforePause;
    private bool equationInteractionWasEnabledBeforePause;
    private bool referencesValid;

    public bool HasGameStarted { get; private set; }
    public bool IsPaused { get; private set; }

    private void Awake()
    {
        Time.timeScale = 1f;
        referencesValid = ValidateReferences();

        if (!referencesValid)
        {
            enabled = false;
            return;
        }

        HasGameStarted = false;
        IsPaused = false;
        gameController.PrepareForMainMenu();
        equationInteraction.SetInteractionEnabled(false);
        gameplayHudRoot.SetActive(false);
        mainMenuRoot.SetActive(true);
        pauseMenuRoot.SetActive(false);
        SelectButton(playButton);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
        {
            return;
        }

        if (gameController == null || gameController.IsTerminalState)
        {
            return;
        }

        if (mainMenuRoot.activeSelf || !HasGameStarted)
        {
            return;
        }

        if (IsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    public void PlayGame()
    {
        if (!referencesValid)
        {
            return;
        }

        Time.timeScale = 1f;
        mainMenuRoot.SetActive(false);
        pauseMenuRoot.SetActive(false);
        gameplayHudRoot.SetActive(true);
        HasGameStarted = true;
        IsPaused = false;
        ClearRememberedPauseState();
        gameController.StartLevel();
        ClearSelection();
    }

    public void PauseGame()
    {
        if (!referencesValid ||
            !HasGameStarted ||
            mainMenuRoot.activeSelf ||
            gameController.IsTerminalState ||
            IsPaused)
        {
            return;
        }

        timerWasRunningBeforePause = globalTimer.IsRunning;
        equationInteractionWasEnabledBeforePause =
            equationInteraction.InteractionEnabled;
        gameController.SetGameplayPaused(true);

        if (timerWasRunningBeforePause)
        {
            globalTimer.PauseTimer();
        }

        equationInteraction.SetInteractionEnabled(false);
        Time.timeScale = 0f;
        pauseMenuRoot.SetActive(true);
        IsPaused = true;
        SelectButton(resumeButton);
    }

    public void ResumeGame()
    {
        if (!referencesValid || !IsPaused)
        {
            return;
        }

        pauseMenuRoot.SetActive(false);
        Time.timeScale = 1f;
        gameController.SetGameplayPaused(false);

        if (timerWasRunningBeforePause)
        {
            globalTimer.ResumeTimer();
        }

        if (equationInteractionWasEnabledBeforePause &&
            gameController.CurrentPhase ==
                CodebreakerPhase.EquationEntry &&
            !gameController.IsTerminalState)
        {
            equationInteraction.SetInteractionEnabled(true);
        }

        ClearRememberedPauseState();
        IsPaused = false;
        ClearSelection();
    }

    public void RetryGame()
    {
        if (!referencesValid)
        {
            return;
        }

        mainMenuRoot.SetActive(false);
        pauseMenuRoot.SetActive(false);
        gameplayHudRoot.SetActive(true);
        Time.timeScale = 1f;
        HasGameStarted = true;
        IsPaused = false;
        gameController.SetGameplayPaused(false);
        equationInteraction.SetInteractionEnabled(false);
        ClearRememberedPauseState();
        gameController.RestartLevel();
        ClearSelection();
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public bool ValidateReferences()
    {
        bool isValid = true;
        isValid &= ValidateAssigned(gameController, nameof(gameController));
        isValid &= ValidateAssigned(globalTimer, nameof(globalTimer));
        isValid &= ValidateAssigned(
            equationInteraction,
            nameof(equationInteraction));
        isValid &= ValidateAssigned(gameplayHudRoot, nameof(gameplayHudRoot));
        isValid &= ValidateAssigned(mainMenuRoot, nameof(mainMenuRoot));
        isValid &= ValidateAssigned(pauseMenuRoot, nameof(pauseMenuRoot));
        isValid &= ValidateAssigned(playButton, nameof(playButton));
        isValid &= ValidateAssigned(resumeButton, nameof(resumeButton));
        isValid &= ValidateAssigned(retryButton, nameof(retryButton));
        isValid &= ValidateAssigned(mainQuitButton, nameof(mainQuitButton));
        isValid &= ValidateAssigned(pauseQuitButton, nameof(pauseQuitButton));

        if (mainMenuRoot != null &&
            pauseMenuRoot != null &&
            mainMenuRoot == pauseMenuRoot)
        {
            LogReferenceError(
                "mainMenuRoot and pauseMenuRoot must be different objects.");
            isValid = false;
        }

        isValid &= ValidateButtonParent(
            playButton,
            mainMenuRoot,
            nameof(playButton),
            nameof(mainMenuRoot));
        isValid &= ValidateButtonParent(
            mainQuitButton,
            mainMenuRoot,
            nameof(mainQuitButton),
            nameof(mainMenuRoot));
        isValid &= ValidateButtonParent(
            resumeButton,
            pauseMenuRoot,
            nameof(resumeButton),
            nameof(pauseMenuRoot));
        isValid &= ValidateButtonParent(
            retryButton,
            pauseMenuRoot,
            nameof(retryButton),
            nameof(pauseMenuRoot));
        isValid &= ValidateButtonParent(
            pauseQuitButton,
            pauseMenuRoot,
            nameof(pauseQuitButton),
            nameof(pauseMenuRoot));

        if (gameplayHudRoot != null &&
            ((mainMenuRoot != null &&
              (gameplayHudRoot == mainMenuRoot ||
               gameplayHudRoot.transform.IsChildOf(
                   mainMenuRoot.transform))) ||
             (pauseMenuRoot != null &&
              (gameplayHudRoot == pauseMenuRoot ||
               gameplayHudRoot.transform.IsChildOf(
                   pauseMenuRoot.transform)))))
        {
            LogReferenceError(
                "gameplayHudRoot must not be a descendant of a menu root.");
            isValid = false;
        }

        if (gameController != null &&
            globalTimer != null &&
            gameController.GlobalTimer != globalTimer)
        {
            LogReferenceError(
                "gameController and globalTimer must reference the same " +
                "GlobalBombTimer.");
            isValid = false;
        }

        referencesValid = isValid;
        return isValid;
    }

    private bool ValidateAssigned(Object reference, string fieldName)
    {
        if (reference != null)
        {
            return true;
        }

        LogReferenceError($"Missing field: {fieldName}.");
        return false;
    }

    private bool ValidateButtonParent(
        Button button,
        GameObject expectedRoot,
        string buttonField,
        string rootField)
    {
        if (button == null || expectedRoot == null)
        {
            return true;
        }

        if (button.transform.IsChildOf(expectedRoot.transform))
        {
            return true;
        }

        LogReferenceError(
            $"{buttonField} must belong to {rootField}.");
        return false;
    }

    private void LogReferenceError(string message)
    {
        Debug.LogError(
            $"CodebreakerMenuController reference validation: {message}",
            this);
    }

    private void ClearRememberedPauseState()
    {
        timerWasRunningBeforePause = false;
        equationInteractionWasEnabledBeforePause = false;
    }

    private static void SelectButton(Button button)
    {
        if (EventSystem.current != null && button != null)
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }

    private static void ClearSelection()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
