using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class CodebreakerTutorialUIPass
{
    private const string MenuPath =
        "Tools/Codebreaker/Build Tutorial UI Pass";
    private const string TargetScenePath =
        "Assets/Scenes/CodebreakerPrototypeScene.unity";
    private const string DialogTitle = "Codebreaker Tutorial UI Pass";
    private const string UndoName = "Build Codebreaker Tutorial UI Pass";
    private const string FiveDigitWiringMenuPath =
        "Tools/Codebreaker/Wire Five-Digit Puzzle Configs";
    private const string FiveDigitWiringDialogTitle =
        "Codebreaker Five-Digit Wiring";
    private const string FiveDigitWiringUndoName =
        "Wire Five-Digit Puzzle Configs";
    private const string BombBackgroundMenuPath =
        "Tools/Codebreaker/Install Bomb Background";
    private const string BombBackgroundAssetPath =
        "Assets/Art/CodebreakerBombBackground.png";
    private const string BombBackgroundObjectName =
        "CodebreakerBombBackground";
    private const string BombBackgroundDialogTitle =
        "Codebreaker Bomb Background";
    private const string BombBackgroundUndoName =
        "Install Codebreaker Bomb Background";
    private const string ReleaseMenuPath =
        "Tools/Codebreaker/Build Release Menus";
    private const string ReleaseMenuDialogTitle =
        "Codebreaker Release Menus";
    private const string ReleaseMenuUndoName =
        "Build Codebreaker Release Menus";
    private const string ReleaseMenuCanvasName =
        "ReleaseMenuCanvas";
    private const string ReleaseMenuSuccessReport =
        "CODEBREAKER RELEASE MENUS BUILT\n\n" +
        "Main menu: PLAY / QUIT\n" +
        "Pause menu: RESUME / RETRY / QUIT\n" +
        "Escape toggle: enabled\n" +
        "Timer pause: enabled\n" +
        "New Input System: preserved";
    private const string EquationStatusPanelName =
        "EquationStatusPanel";
    private const string PhaseOneInstruction =
        "<size=30><b>USE ALL 4 HITS TO LEAVE ONE GREEN DIGIT</b></size>\n" +
        "<size=18>CLICK A SEGMENT = REMOVE ONE LAYER   |   RED > YELLOW > GREEN > OFF   |   DOTS = LAYERS LEFT</size>";
    private const string PhaseTwoInstruction = "";
    private const string SuccessReport =
        "FINAL EQUATION ENTRY UI CLEANUP BUILT\n\n" +
        "Equation hierarchy refined\n" +
        "Equation status module polished\n" +
        "Buffer backdrop and token backgrounds hidden\n" +
        "Two segment-shaped Buffer slots rebuilt\n" +
        "Buffer count label clarified\n" +
        "Gameplay rules preserved";
    private const string FiveDigitWiringSuccessReport =
        "FIVE-DIGIT PUZZLE CONFIGS WIRED\n\n" +
        "Puzzle 1 = 5\n" +
        "Puzzle 2 = 6\n" +
        "Puzzle 3 = 9\n" +
        "Puzzle 4 = 8\n" +
        "Puzzle 5 = 7";

    private static readonly string[] FiveDigitPuzzleAssetPaths =
    {
        "Assets/Configs/Codebreaker/PrototypeLayeredDigit5.asset",
        "Assets/Configs/Codebreaker/PrototypeLayeredDigit3.asset",
        "Assets/Configs/Codebreaker/PrototypeLayeredDigit0.asset",
        "Assets/Configs/Codebreaker/PrototypeLayeredDigit8.asset",
        "Assets/Configs/Codebreaker/PrototypeLayeredDigit7.asset"
    };

    private static readonly int[] FiveDigitTargetCodeIndices =
    {
        0,
        1,
        2,
        3,
        4
    };

    private static readonly int[] FiveDigitExpectedDigits =
    {
        5,
        6,
        9,
        8,
        7
    };

    private static readonly StaticLabelLayout PlusLabelLayout =
        new StaticLabelLayout(
            "EquationPlusText",
            "+",
            new Vector2(0f, -100f),
            new Vector2(260f, 220f),
            175f);
    private static readonly StaticLabelLayout ReadyTextLayout =
        new StaticLabelLayout(
            "EquationReadyText",
            string.Empty,
            new Vector2(0f, -225f),
            new Vector2(700f, 90f),
            28f);
    private static readonly StaticLabelLayout BufferFeedbackLayout =
        new StaticLabelLayout(
            "BufferFeedbackText",
            string.Empty,
            new Vector2(0f, -365f),
            new Vector2(900f, 42f),
            18f);
    private static readonly Vector2 EquationStatusPanelPosition =
        new Vector2(-610f, 10f);
    private static readonly Vector2 EquationStatusPanelSize =
        new Vector2(520f, 210f);
    private static readonly Color EquationStatusPanelColor =
        new Color(0.015f, 0.035f, 0.055f, 0.76f);
    private static readonly Color EntryProgressColor =
        new Color(0.30f, 0.88f, 1f, 1f);
    private static readonly Color CurrentValuesColor =
        new Color(0.92f, 0.97f, 1f, 1f);
    private static readonly Color AcceptedDigitsColor =
        new Color(0.55f, 0.68f, 0.76f, 1f);
    private static readonly Color OrdinaryFeedbackColor =
        new Color(1f, 0.68f, 0.28f, 1f);
    private static readonly StatusTextLayout EntryProgressLayout =
        new StatusTextLayout(
            "entryProgressText",
            "DIGIT 1 / 5",
            new Vector2(-610f, 70f),
            new Vector2(450f, 34f),
            22f,
            FontStyles.Bold,
            EntryProgressColor);
    private static readonly StatusTextLayout CurrentValuesLayout =
        new StatusTextLayout(
            "currentValuesText",
            "A 3  +  B 8  =  11",
            new Vector2(-610f, 18f),
            new Vector2(450f, 46f),
            28f,
            FontStyles.Bold,
            CurrentValuesColor);
    private static readonly StatusTextLayout AcceptedDigitsLayout =
        new StatusTextLayout(
            "acceptedDigitsText",
            "ENTERED  -  -  -  -  -",
            new Vector2(-610f, -34f),
            new Vector2(450f, 34f),
            20f,
            FontStyles.Normal,
            AcceptedDigitsColor);
    private static readonly StatusTextLayout FeedbackLayout =
        new StatusTextLayout(
            "feedbackText",
            string.Empty,
            new Vector2(-610f, -76f),
            new Vector2(450f, 34f),
            18f,
            FontStyles.Normal,
            OrdinaryFeedbackColor);

    [MenuItem(MenuPath)]
    private static void BuildTutorialUiPass()
    {
        if (!TryGetRunnableTargetScene(
                out Scene targetScene,
                out string refusal))
        {
            ReportRefusal(refusal);
            return;
        }

        List<string> errors = new List<string>();
        TutorialContext context = ValidateScene(targetScene, errors);

        if (errors.Count > 0)
        {
            ReportValidationFailures(errors);
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(UndoName);
        bool mutationStarted = false;
        bool sceneSaved = false;

        try
        {
            mutationStarted = true;
            ApplyTutorialUi(context);
            EditorSceneManager.MarkSceneDirty(targetScene);

            if (!EditorSceneManager.SaveScene(
                targetScene,
                TargetScenePath,
                false))
            {
                throw new InvalidOperationException(
                    $"Unity could not save {TargetScenePath}.");
            }

            sceneSaved = true;
            Undo.CollapseUndoOperations(undoGroup);
            Debug.Log(SuccessReport);
            EditorUtility.DisplayDialog(
                DialogTitle,
                SuccessReport,
                "OK");
        }
        catch (Exception exception)
        {
            if (mutationStarted && !sceneSaved)
            {
                Undo.RevertAllDownToGroup(undoGroup);
            }

            Debug.LogException(exception);
            Debug.LogError(
                $"Codebreaker tutorial UI pass was not saved: " +
                $"{exception.Message}");
            EditorUtility.DisplayDialog(
                DialogTitle,
                "CODEBREAKER TUTORIAL UI PASS FAILED\n\n" +
                exception.Message,
                "OK");
        }
    }

    [MenuItem(ReleaseMenuPath)]
    private static void BuildReleaseMenus()
    {
        if (!TryGetReleaseMenuTargetScene(
                out Scene targetScene,
                out string refusal))
        {
            ReportReleaseMenuFailure(refusal);
            return;
        }

        List<string> errors = new List<string>();
        ReleaseMenuContext context =
            ValidateReleaseMenuScene(targetScene, errors);

        if (errors.Count > 0)
        {
            ReportReleaseMenuFailures(errors);
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(ReleaseMenuUndoName);
        bool mutationStarted = false;
        bool sceneSaved = false;

        try
        {
            mutationStarted = true;
            ApplyReleaseMenus(context);
            List<string> appliedStateErrors = new List<string>();
            ValidateReleaseMenuAppliedState(
                context,
                appliedStateErrors);

            if (appliedStateErrors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Final applied-state validation failed:\n- " +
                    string.Join("\n- ", appliedStateErrors));
            }

            EditorSceneManager.MarkSceneDirty(targetScene);

            if (!EditorSceneManager.SaveScene(
                    targetScene,
                    TargetScenePath,
                    false))
            {
                throw new InvalidOperationException(
                    $"Unity could not save {TargetScenePath}.");
            }

            sceneSaved = true;
            Undo.CollapseUndoOperations(undoGroup);
            Debug.Log(ReleaseMenuSuccessReport);
            EditorUtility.DisplayDialog(
                ReleaseMenuDialogTitle,
                ReleaseMenuSuccessReport,
                "OK");
        }
        catch (Exception exception)
        {
            if (mutationStarted && !sceneSaved)
            {
                Undo.RevertAllDownToGroup(undoGroup);
            }

            Debug.LogException(exception);
            ReportReleaseMenuFailure(
                "CODEBREAKER RELEASE MENU BUILD FAILED\n\n" +
                exception.Message);
        }
    }

    [MenuItem(BombBackgroundMenuPath)]
    private static void InstallBombBackground()
    {
        if (!TryLoadBombBackgroundSprite(out Sprite sprite))
        {
            return;
        }

        if (!TryGetBombBackgroundTargetScene(
                out Scene targetScene,
                out string refusal))
        {
            ReportBombBackgroundRefusal(refusal);
            return;
        }

        List<string> errors = new List<string>();
        Camera targetCamera =
            RequireUniqueComponent<Camera>(targetScene, errors);
        GameObject backgroundObject = FindBombBackgroundObject(
            targetScene,
            errors);
        float uniformScale = 0f;

        ValidateBombBackgroundCamera(
            targetScene,
            targetCamera,
            errors);

        if (targetCamera != null)
        {
            uniformScale = CalculateBombBackgroundScale(
                targetCamera,
                sprite,
                errors);
        }

        ValidateBombBackgroundBeforeRepair(
            backgroundObject,
            errors);

        if (errors.Count > 0)
        {
            ReportBombBackgroundFailures(
                "BOMB BACKGROUND VALIDATION FAILED",
                errors);
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(BombBackgroundUndoName);
        bool mutationStarted = false;
        bool sceneSaved = false;

        try
        {
            mutationStarted = true;
            SpriteRenderer spriteRenderer = ApplyBombBackground(
                targetScene,
                targetCamera,
                backgroundObject,
                sprite,
                uniformScale);

            List<string> appliedStateErrors = new List<string>();
            ValidateBombBackgroundAppliedState(
                targetScene,
                targetCamera,
                sprite,
                uniformScale,
                spriteRenderer,
                appliedStateErrors);

            if (appliedStateErrors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Final applied-state validation failed:\n- " +
                    string.Join("\n- ", appliedStateErrors));
            }

            EditorSceneManager.MarkSceneDirty(targetScene);

            if (!EditorSceneManager.SaveScene(
                    targetScene,
                    TargetScenePath,
                    false))
            {
                throw new InvalidOperationException(
                    $"Unity could not save {TargetScenePath}.");
            }

            sceneSaved = true;
            Undo.CollapseUndoOperations(undoGroup);

            string successReport =
                "CODEBREAKER BOMB BACKGROUND INSTALLED\n\n" +
                $"Asset: {BombBackgroundAssetPath}\n" +
                $"Camera: {GetHierarchyPath(targetCamera.gameObject)}\n" +
                $"Object: {GetHierarchyPath(spriteRenderer.gameObject)}\n" +
                $"Uniform cover scale: {uniformScale}\n" +
                "Sorting: Default / -1000\n" +
                "Input: no physics, UI, or behaviour components";
            Debug.Log(successReport);
            EditorUtility.DisplayDialog(
                BombBackgroundDialogTitle,
                successReport,
                "OK");
        }
        catch (Exception exception)
        {
            if (mutationStarted && !sceneSaved)
            {
                Undo.RevertAllDownToGroup(undoGroup);
            }

            Debug.LogException(exception);
            Debug.LogError(
                "Codebreaker bomb background was not saved: " +
                exception.Message);
            EditorUtility.DisplayDialog(
                BombBackgroundDialogTitle,
                "CODEBREAKER BOMB BACKGROUND INSTALL FAILED\n\n" +
                exception.Message,
                "OK");
        }
    }

    [MenuItem(FiveDigitWiringMenuPath)]
    private static void WireFiveDigitPuzzleConfigs()
    {
        if (!TryGetFiveDigitWiringTargetScene(
                out Scene targetScene,
                out string refusal))
        {
            ReportFiveDigitWiringRefusal(refusal);
            return;
        }

        List<string> errors = new List<string>();
        LayeredDigitPuzzleController puzzleController =
            RequireUniqueComponent<LayeredDigitPuzzleController>(
                targetScene,
                errors);
        LayeredDigitPuzzleConfig[] puzzleConfigs =
            LoadFiveDigitPuzzleConfigs(errors);
        SerializedObject serializedController = null;

        if (puzzleController != null)
        {
            serializedController = new SerializedObject(puzzleController);
            serializedController.Update();
            SerializedProperty puzzleConfigsProperty =
                serializedController.FindProperty("puzzleConfigs");

            if (puzzleConfigsProperty == null)
            {
                errors.Add(
                    "LayeredDigitPuzzleController has no serialized " +
                    "puzzleConfigs property.");
            }
            else if (!puzzleConfigsProperty.isArray)
            {
                errors.Add(
                    "LayeredDigitPuzzleController.puzzleConfigs must be " +
                    "an array.");
            }
        }

        if (errors.Count > 0)
        {
            ReportFiveDigitWiringFailures(
                "FIVE-DIGIT PUZZLE CONFIG VALIDATION FAILED",
                errors);
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(FiveDigitWiringUndoName);
        bool mutationStarted = false;
        bool sceneSaved = false;

        try
        {
            Undo.RecordObject(
                puzzleController,
                FiveDigitWiringUndoName);
            mutationStarted = true;

            serializedController.Update();
            SerializedProperty puzzleConfigsProperty =
                serializedController.FindProperty("puzzleConfigs");
            puzzleConfigsProperty.arraySize = 5;

            for (int i = 0; i < puzzleConfigs.Length; i++)
            {
                puzzleConfigsProperty
                    .GetArrayElementAtIndex(i)
                    .objectReferenceValue = puzzleConfigs[i];
            }

            serializedController.ApplyModifiedProperties();

            List<string> assignmentErrors = new List<string>();
            ValidateFiveDigitPuzzleAssignment(
                serializedController,
                puzzleConfigs,
                assignmentErrors);

            if (assignmentErrors.Count > 0)
            {
                Undo.RevertAllDownToGroup(undoGroup);
                mutationStarted = false;
                ReportFiveDigitWiringFailures(
                    "FIVE-DIGIT PUZZLE CONFIG ASSIGNMENT FAILED",
                    assignmentErrors);
                return;
            }

            EditorSceneManager.MarkSceneDirty(targetScene);

            if (!EditorSceneManager.SaveScene(
                    targetScene,
                    TargetScenePath,
                    false))
            {
                throw new InvalidOperationException(
                    $"Unity could not save {TargetScenePath}.");
            }

            sceneSaved = true;
            Undo.CollapseUndoOperations(undoGroup);
            Debug.Log(FiveDigitWiringSuccessReport);
            EditorUtility.DisplayDialog(
                FiveDigitWiringDialogTitle,
                FiveDigitWiringSuccessReport,
                "OK");
        }
        catch (Exception exception)
        {
            if (mutationStarted && !sceneSaved)
            {
                Undo.RevertAllDownToGroup(undoGroup);
            }

            Debug.LogException(exception);
            Debug.LogError(
                "Five-digit puzzle config wiring was not saved: " +
                exception.Message);
            EditorUtility.DisplayDialog(
                FiveDigitWiringDialogTitle,
                "FIVE-DIGIT PUZZLE CONFIG WIRING FAILED\n\n" +
                exception.Message,
                "OK");
        }
    }

    private static bool TryLoadBombBackgroundSprite(out Sprite sprite)
    {
        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            BombBackgroundAssetPath);

        if (sprite == null)
        {
            string message =
                $"Could not load a Sprite at {BombBackgroundAssetPath}.\n\n" +
                "Select the texture and set Texture Type to " +
                "Sprite (2D and UI), then apply the import settings.";
            Debug.LogError(message);
            EditorUtility.DisplayDialog(
                BombBackgroundDialogTitle,
                message,
                "OK");
            return false;
        }

        List<string> errors = new List<string>();

        if (sprite.texture == null)
        {
            errors.Add("The loaded Sprite has no texture.");
        }
        else if (sprite.texture.width <= 0 ||
            sprite.texture.height <= 0)
        {
            errors.Add(
                "The loaded Sprite texture must have positive width and " +
                "height.");
        }

        if (!(sprite.bounds.size.x > 0f) ||
            !(sprite.bounds.size.y > 0f))
        {
            errors.Add(
                "The loaded Sprite bounds must have positive width and " +
                "height.");
        }

        if (errors.Count > 0)
        {
            ReportBombBackgroundFailures(
                "BOMB BACKGROUND ASSET VALIDATION FAILED",
                errors);
            sprite = null;
            return false;
        }

        return true;
    }

    private static bool TryGetBombBackgroundTargetScene(
        out Scene targetScene,
        out string refusal)
    {
        targetScene = SceneManager.GetSceneByPath(TargetScenePath);

        if (EditorApplication.isPlaying)
        {
            refusal =
                "The bomb background cannot be installed while Unity is " +
                "in Play Mode.";
            return false;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            refusal =
                "The bomb background cannot be installed while Unity is " +
                "entering Play Mode.";
            return false;
        }

        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            refusal =
                $"Load {TargetScenePath} before installing the bomb " +
                "background.";
            return false;
        }

        if (SceneManager.GetActiveScene() != targetScene)
        {
            refusal =
                $"{TargetScenePath} must be the active scene before " +
                "installing the bomb background.";
            return false;
        }

        if (targetScene.isDirty)
        {
            refusal =
                "The target scene has unsaved changes. Save or discard " +
                "them before installing the bomb background.";
            return false;
        }

        refusal = null;
        return true;
    }

    private static GameObject FindBombBackgroundObject(
        Scene targetScene,
        List<string> errors)
    {
        List<GameObject> matches = FindAllNamed(
            targetScene,
            BombBackgroundObjectName);

        if (matches.Count > 1)
        {
            errors.Add(
                $"{targetScene.path} contains {matches.Count} objects " +
                $"named {BombBackgroundObjectName}; expected at most one " +
                "before repair.");
            return null;
        }

        return matches.Count == 1 ? matches[0] : null;
    }

    private static void ValidateBombBackgroundCamera(
        Scene targetScene,
        Camera targetCamera,
        List<string> errors)
    {
        if (targetCamera == null)
        {
            return;
        }

        if (targetCamera.gameObject.scene != targetScene)
        {
            errors.Add(
                "The unique Camera does not belong to the active target " +
                "scene.");
        }

        if (!targetCamera.enabled)
        {
            errors.Add("The unique Camera must be enabled.");
        }

        if (!targetCamera.orthographic)
        {
            errors.Add("The unique Camera must use orthographic projection.");
        }
    }

    private static float CalculateBombBackgroundScale(
        Camera targetCamera,
        Sprite sprite,
        List<string> errors)
    {
        float worldHeight = targetCamera.orthographicSize * 2f;
        float worldWidth = worldHeight * targetCamera.aspect;
        Vector2 spriteWorldSize = sprite.bounds.size;
        float uniformScale = Mathf.Max(
            worldWidth / spriteWorldSize.x,
            worldHeight / spriteWorldSize.y);

        if (!IsFinitePositive(worldHeight))
        {
            errors.Add(
                "The Camera orthographic size does not produce a positive " +
                "finite viewport height.");
        }

        if (!IsFinitePositive(worldWidth))
        {
            errors.Add(
                "The Camera aspect does not produce a positive finite " +
                "viewport width.");
        }

        if (!IsFinitePositive(uniformScale))
        {
            errors.Add(
                "The calculated uniform cover scale must be positive and " +
                "finite.");
        }

        return uniformScale;
    }

    private static void ValidateBombBackgroundBeforeRepair(
        GameObject backgroundObject,
        List<string> errors)
    {
        if (backgroundObject == null)
        {
            return;
        }

        int rendererCount =
            backgroundObject.GetComponents<SpriteRenderer>().Length;

        if (rendererCount > 1)
        {
            errors.Add(
                $"{BombBackgroundObjectName} contains {rendererCount} " +
                "SpriteRenderer components; expected at most one before " +
                "repair.");
        }
    }

    private static SpriteRenderer ApplyBombBackground(
        Scene targetScene,
        Camera targetCamera,
        GameObject backgroundObject,
        Sprite sprite,
        float uniformScale)
    {
        if (backgroundObject == null)
        {
            backgroundObject = new GameObject(
                BombBackgroundObjectName);
            Undo.RegisterCreatedObjectUndo(
                backgroundObject,
                BombBackgroundUndoName);

            if (backgroundObject.scene != targetScene)
            {
                throw new InvalidOperationException(
                    $"New {BombBackgroundObjectName} was not created in " +
                    $"{targetScene.path}.");
            }
        }

        RemoveProhibitedBombBackgroundComponents(backgroundObject);

        SpriteRenderer spriteRenderer =
            backgroundObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = Undo.AddComponent<SpriteRenderer>(
                backgroundObject);
        }

        if (!backgroundObject.activeSelf)
        {
            Undo.RecordObject(
                backgroundObject,
                BombBackgroundUndoName);
            backgroundObject.SetActive(true);
        }

        Transform backgroundTransform = backgroundObject.transform;

        if (backgroundTransform.parent != targetCamera.transform)
        {
            Undo.SetTransformParent(
                backgroundTransform,
                targetCamera.transform,
                BombBackgroundUndoName);
        }

        Undo.RecordObject(
            backgroundTransform,
            BombBackgroundUndoName);
        backgroundTransform.localPosition = new Vector3(0f, 0f, 50f);
        backgroundTransform.localRotation = Quaternion.identity;
        backgroundTransform.localScale = new Vector3(
            uniformScale,
            uniformScale,
            1f);

        if (backgroundTransform.GetSiblingIndex() != 0)
        {
            backgroundTransform.SetSiblingIndex(0);
        }

        Undo.RecordObject(spriteRenderer, BombBackgroundUndoName);
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = Color.white;
        spriteRenderer.enabled = true;
        spriteRenderer.flipX = false;
        spriteRenderer.flipY = false;
        spriteRenderer.drawMode = SpriteDrawMode.Simple;
        spriteRenderer.sortingLayerName = "Default";
        spriteRenderer.sortingOrder = -1000;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;

        return spriteRenderer;
    }

    private static void RemoveProhibitedBombBackgroundComponents(
        GameObject backgroundObject)
    {
        HashSet<Component> prohibitedComponents =
            new HashSet<Component>();

        AddComponents(
            prohibitedComponents,
            backgroundObject.GetComponents<Collider2D>());
        AddComponents(
            prohibitedComponents,
            backgroundObject.GetComponents<Rigidbody2D>());
        AddComponents(
            prohibitedComponents,
            backgroundObject.GetComponents<MonoBehaviour>());
        AddComponents(
            prohibitedComponents,
            backgroundObject.GetComponents<Canvas>());
        AddComponents(
            prohibitedComponents,
            backgroundObject.GetComponents<CanvasRenderer>());
        AddComponents(
            prohibitedComponents,
            backgroundObject.GetComponents<UnityEngine.UI.Graphic>());

        foreach (Component component in prohibitedComponents)
        {
            if (component != null)
            {
                Undo.DestroyObjectImmediate(component);
            }
        }
    }

    private static void AddComponents<T>(
        HashSet<Component> destination,
        T[] components)
        where T : Component
    {
        foreach (T component in components)
        {
            if (component != null)
            {
                destination.Add(component);
            }
        }
    }

    private static void ValidateBombBackgroundAppliedState(
        Scene targetScene,
        Camera targetCamera,
        Sprite sprite,
        float uniformScale,
        SpriteRenderer appliedRenderer,
        List<string> errors)
    {
        List<GameObject> matches = FindAllNamed(
            targetScene,
            BombBackgroundObjectName);

        if (matches.Count != 1)
        {
            errors.Add(
                $"{targetScene.path} contains {matches.Count} objects " +
                $"named {BombBackgroundObjectName}; expected exactly one.");
            return;
        }

        GameObject backgroundObject = matches[0];
        Transform backgroundTransform = backgroundObject.transform;

        if (backgroundObject.scene != targetScene)
        {
            errors.Add(
                $"{BombBackgroundObjectName} does not belong to the active " +
                "target scene.");
        }

        if (!backgroundObject.activeSelf)
        {
            errors.Add($"{BombBackgroundObjectName} is not active.");
        }

        if (backgroundTransform.parent != targetCamera.transform)
        {
            errors.Add(
                $"{BombBackgroundObjectName} is not a direct child of the " +
                "unique Camera.");
        }

        if (backgroundTransform.GetSiblingIndex() != 0)
        {
            errors.Add(
                $"{BombBackgroundObjectName} is not the Camera's first " +
                "child.");
        }

        if (backgroundTransform.localPosition !=
            new Vector3(0f, 0f, 50f))
        {
            errors.Add(
                $"{BombBackgroundObjectName} local position is invalid.");
        }

        if (backgroundTransform.localRotation != Quaternion.identity)
        {
            errors.Add(
                $"{BombBackgroundObjectName} local rotation is invalid.");
        }

        Vector3 expectedScale = new Vector3(
            uniformScale,
            uniformScale,
            1f);

        if (backgroundTransform.localScale != expectedScale ||
            !IsFinitePositive(backgroundTransform.localScale.x) ||
            backgroundTransform.localScale.x !=
                backgroundTransform.localScale.y)
        {
            errors.Add(
                $"{BombBackgroundObjectName} local scale is not the " +
                "expected positive finite uniform cover scale.");
        }

        SpriteRenderer[] renderers =
            backgroundObject.GetComponents<SpriteRenderer>();

        if (renderers.Length != 1)
        {
            errors.Add(
                $"{BombBackgroundObjectName} contains {renderers.Length} " +
                "SpriteRenderer components; expected exactly one.");
        }
        else
        {
            SpriteRenderer renderer = renderers[0];

            if (renderer != appliedRenderer)
            {
                errors.Add(
                    "The applied SpriteRenderer is not the object's single " +
                    "SpriteRenderer.");
            }

            if (renderer.sprite != sprite)
            {
                errors.Add(
                    "The SpriteRenderer does not reference the exact loaded " +
                    "background Sprite.");
            }

            if (renderer.color != Color.white)
            {
                errors.Add("The SpriteRenderer color is not white.");
            }

            if (!renderer.enabled)
            {
                errors.Add("The SpriteRenderer is not enabled.");
            }

            if (renderer.flipX || renderer.flipY)
            {
                errors.Add("The SpriteRenderer must not be flipped.");
            }

            if (renderer.drawMode != SpriteDrawMode.Simple)
            {
                errors.Add(
                    "The SpriteRenderer draw mode is not Simple.");
            }

            if (renderer.sortingLayerName != "Default" ||
                renderer.sortingOrder != -1000)
            {
                errors.Add(
                    "The SpriteRenderer sorting is not Default / -1000.");
            }

            if (renderer.maskInteraction !=
                SpriteMaskInteraction.None)
            {
                errors.Add(
                    "The SpriteRenderer mask interaction is not None.");
            }
        }

        ValidateNoBombBackgroundComponents<Collider2D>(
            backgroundObject,
            errors);
        ValidateNoBombBackgroundComponents<Rigidbody2D>(
            backgroundObject,
            errors);
        ValidateNoBombBackgroundComponents<MonoBehaviour>(
            backgroundObject,
            errors);
        ValidateNoBombBackgroundComponents<Canvas>(
            backgroundObject,
            errors);
        ValidateNoBombBackgroundComponents<CanvasRenderer>(
            backgroundObject,
            errors);
        ValidateNoBombBackgroundComponents<UnityEngine.UI.Graphic>(
            backgroundObject,
            errors);
    }

    private static void ValidateNoBombBackgroundComponents<T>(
        GameObject backgroundObject,
        List<string> errors)
        where T : Component
    {
        int componentCount =
            backgroundObject.GetComponents<T>().Length;

        if (componentCount > 0)
        {
            errors.Add(
                $"{BombBackgroundObjectName} contains {componentCount} " +
                $"{typeof(T).Name} component(s); expected none.");
        }
    }

    private static bool IsFinitePositive(float value)
    {
        return value > 0f &&
            !float.IsNaN(value) &&
            !float.IsInfinity(value);
    }

    private static void ReportBombBackgroundRefusal(string message)
    {
        Debug.LogError(message);
        EditorUtility.DisplayDialog(
            BombBackgroundDialogTitle,
            message,
            "OK");
    }

    private static void ReportBombBackgroundFailures(
        string heading,
        List<string> errors)
    {
        foreach (string error in errors)
        {
            Debug.LogError(error);
        }

        string report =
            heading + "\n\n- " + string.Join("\n- ", errors);
        Debug.LogError(report);
        EditorUtility.DisplayDialog(
            BombBackgroundDialogTitle,
            report,
            "OK");
    }

    private static bool TryGetFiveDigitWiringTargetScene(
        out Scene targetScene,
        out string refusal)
    {
        targetScene = SceneManager.GetSceneByPath(TargetScenePath);

        if (EditorApplication.isPlaying)
        {
            refusal =
                "Five-digit puzzle wiring cannot run while Unity is in " +
                "Play Mode.";
            return false;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            refusal =
                "Five-digit puzzle wiring cannot run while Unity is " +
                "entering Play Mode.";
            return false;
        }

        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            refusal =
                $"Load {TargetScenePath} before wiring the five-digit " +
                "puzzle configs.";
            return false;
        }

        if (SceneManager.GetActiveScene() != targetScene)
        {
            refusal =
                $"{TargetScenePath} must be the active scene before " +
                "wiring the five-digit puzzle configs.";
            return false;
        }

        if (targetScene.isDirty)
        {
            refusal =
                "The target scene has unsaved changes. Save or discard " +
                "them before wiring the five-digit puzzle configs.";
            return false;
        }

        refusal = null;
        return true;
    }

    private static LayeredDigitPuzzleConfig[]
        LoadFiveDigitPuzzleConfigs(List<string> errors)
    {
        LayeredDigitPuzzleConfig[] puzzleConfigs =
            new LayeredDigitPuzzleConfig[
                FiveDigitPuzzleAssetPaths.Length];

        for (int i = 0; i < FiveDigitPuzzleAssetPaths.Length; i++)
        {
            string assetPath = FiveDigitPuzzleAssetPaths[i];
            LayeredDigitPuzzleConfig puzzleConfig =
                AssetDatabase.LoadAssetAtPath<LayeredDigitPuzzleConfig>(
                    assetPath);
            puzzleConfigs[i] = puzzleConfig;

            if (puzzleConfig == null)
            {
                errors.Add(
                    $"Could not load LayeredDigitPuzzleConfig at " +
                    $"{assetPath}.");
                continue;
            }

            if (puzzleConfig.TargetCodeIndex !=
                FiveDigitTargetCodeIndices[i])
            {
                errors.Add(
                    $"{assetPath} has TargetCodeIndex " +
                    $"{puzzleConfig.TargetCodeIndex}; expected " +
                    $"{FiveDigitTargetCodeIndices[i]}.");
            }

            if (puzzleConfig.ExpectedDigit != FiveDigitExpectedDigits[i])
            {
                errors.Add(
                    $"{assetPath} has ExpectedDigit " +
                    $"{puzzleConfig.ExpectedDigit}; expected " +
                    $"{FiveDigitExpectedDigits[i]}.");
            }
        }

        return puzzleConfigs;
    }

    private static void ValidateFiveDigitPuzzleAssignment(
        SerializedObject serializedController,
        LayeredDigitPuzzleConfig[] expectedConfigs,
        List<string> errors)
    {
        serializedController.Update();
        SerializedProperty puzzleConfigsProperty =
            serializedController.FindProperty("puzzleConfigs");

        if (puzzleConfigsProperty == null)
        {
            errors.Add(
                "LayeredDigitPuzzleController has no serialized " +
                "puzzleConfigs property after assignment.");
            return;
        }

        if (!puzzleConfigsProperty.isArray)
        {
            errors.Add(
                "LayeredDigitPuzzleController.puzzleConfigs is not an " +
                "array after assignment.");
            return;
        }

        if (puzzleConfigsProperty.arraySize != expectedConfigs.Length)
        {
            errors.Add(
                "LayeredDigitPuzzleController.puzzleConfigs has size " +
                $"{puzzleConfigsProperty.arraySize}; expected " +
                $"{expectedConfigs.Length}.");
        }

        int comparableCount = Math.Min(
            puzzleConfigsProperty.arraySize,
            expectedConfigs.Length);

        for (int i = 0; i < comparableCount; i++)
        {
            SerializedProperty element =
                puzzleConfigsProperty.GetArrayElementAtIndex(i);

            if (element.propertyType !=
                SerializedPropertyType.ObjectReference)
            {
                errors.Add(
                    $"LayeredDigitPuzzleController.puzzleConfigs[{i}] " +
                    "is not an object reference.");
                continue;
            }

            if (element.objectReferenceValue != expectedConfigs[i])
            {
                errors.Add(
                    $"LayeredDigitPuzzleController.puzzleConfigs[{i}] " +
                    $"does not reference " +
                    $"{FiveDigitPuzzleAssetPaths[i]}.");
            }
        }
    }

    private static void ReportFiveDigitWiringRefusal(string message)
    {
        Debug.LogError(message);
        EditorUtility.DisplayDialog(
            FiveDigitWiringDialogTitle,
            message,
            "OK");
    }

    private static void ReportFiveDigitWiringFailures(
        string heading,
        List<string> errors)
    {
        foreach (string error in errors)
        {
            Debug.LogError(error);
        }

        string report =
            heading + "\n\n- " + string.Join("\n- ", errors);
        EditorUtility.DisplayDialog(
            FiveDigitWiringDialogTitle,
            report,
            "OK");
    }

    private static bool TryGetRunnableTargetScene(
        out Scene targetScene,
        out string refusal)
    {
        targetScene = SceneManager.GetSceneByPath(TargetScenePath);

        if (EditorApplication.isPlaying)
        {
            refusal =
                "The tutorial UI pass cannot run while Unity is in Play Mode.";
            return false;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            refusal =
                "The tutorial UI pass cannot run while Unity is entering " +
                "Play Mode.";
            return false;
        }

        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            refusal =
                $"Load {TargetScenePath} before running the tutorial UI pass.";
            return false;
        }

        if (SceneManager.GetActiveScene() != targetScene)
        {
            refusal =
                $"{TargetScenePath} must be the active scene before running " +
                "the tutorial UI pass.";
            return false;
        }

        if (targetScene.isDirty)
        {
            refusal =
                "The target scene has unsaved changes. Save or discard them " +
                "before running the tutorial UI pass.";
            return false;
        }

        refusal = null;
        return true;
    }

    private static TutorialContext ValidateScene(
        Scene targetScene,
        List<string> errors)
    {
        TutorialContext context = new TutorialContext
        {
            TargetScene = targetScene,
            HudCanvas = FindUniqueNamed(
                targetScene,
                "CodebreakerHUDCanvas",
                errors),
            EquationRoot = FindUniqueNamed(
                targetScene,
                "EquationEntryRoot",
                errors),
            DisplayA = FindUniqueNamed(
                targetScene,
                "Display_A",
                errors),
            DisplayB = FindUniqueNamed(
                targetScene,
                "Display_B",
                errors),
            BombBackground = FindUniqueNamed(
                targetScene,
                BombBackgroundObjectName,
                errors),
            EquationHud = RequireUniqueComponent<CodebreakerEquationHUD>(
                targetScene,
                errors),
            PuzzleController =
                RequireUniqueComponent<LayeredDigitPuzzleController>(
                    targetScene,
                    errors),
            InventoryTray =
                RequireUniqueComponent<SegmentInventoryTray>(
                    targetScene,
                    errors),
            InventoryDropZone =
                RequireUniqueComponent<InventoryDropZone>(
                    targetScene,
                    errors)
        };

        RequireComponent<Canvas>(
            context.HudCanvas,
            "CodebreakerHUDCanvas",
            errors);
        ValidateDescendant(
            context.EquationRoot,
            context.HudCanvas,
            "EquationEntryRoot",
            "CodebreakerHUDCanvas",
            errors);
        ValidateTrayDropZonePlacement(
            context.InventoryDropZone,
            context.InventoryTray,
            errors);

        if (context.InventoryDropZone != null)
        {
            context.DropZoneColliders =
                context.InventoryDropZone
                    .GetComponentsInChildren<Collider2D>(true);

            if (context.DropZoneColliders.Length == 0)
            {
                errors.Add(
                    "InventoryDropZone must retain at least one Collider2D.");
            }
            else
            {
                context.DropZoneColliderEnabledStates =
                    new bool[context.DropZoneColliders.Length];

                for (int i = 0;
                    i < context.DropZoneColliders.Length;
                    i++)
                {
                    context.DropZoneColliderEnabledStates[i] =
                        context.DropZoneColliders[i].enabled;
                }
            }
        }

        if (context.EquationHud != null)
        {
            context.EntryProgressText = ReadTextReference(
                context.EquationHud,
                "entryProgressText",
                errors);
            context.TargetEquationText = ReadTextReference(
                context.EquationHud,
                "targetEquationText",
                errors);
            context.CurrentValuesText = ReadTextReference(
                context.EquationHud,
                "currentValuesText",
                errors);
            context.AcceptedDigitsText = ReadTextReference(
                context.EquationHud,
                "acceptedDigitsText",
                errors);
            context.FeedbackText = ReadTextReference(
                context.EquationHud,
                "feedbackText",
                errors);
            context.EquationInstructionText = ReadTextReference(
                context.EquationHud,
                "instructionText",
                errors);
            context.SerializedEquationOperatorText = ReadTextReference(
                context.EquationHud,
                "equationOperatorText",
                errors);
            context.SerializedEquationReadyText = ReadTextReference(
                context.EquationHud,
                "equationReadyText",
                errors);
            context.SerializedBufferFeedbackText = ReadTextReference(
                context.EquationHud,
                "bufferFeedbackText",
                errors);
            context.EquationReadyColor = ReadColorValue(
                context.EquationHud,
                "equationReadyColor",
                errors);
        }

        TMP_Text[] equationTexts =
        {
            context.EntryProgressText,
            context.TargetEquationText,
            context.CurrentValuesText,
            context.AcceptedDigitsText,
            context.FeedbackText,
            context.EquationInstructionText
        };

        foreach (TMP_Text text in equationTexts)
        {
            if (text != null)
            {
                ValidateDescendant(
                    text.gameObject,
                    context.EquationRoot,
                    text.gameObject.name,
                    "CodebreakerHUDCanvas/EquationEntryRoot",
                    errors);
            }
        }

        if (context.PuzzleController != null)
        {
            context.PuzzleProgressText = ReadTextReference(
                context.PuzzleController,
                "puzzleProgressText",
                errors);
            context.HitsLeftText = ReadTextReference(
                context.PuzzleController,
                "hitsLeftText",
                errors);
            context.PuzzleInstructionText = ReadTextReference(
                context.PuzzleController,
                "puzzleInstructionText",
                errors);
            context.PuzzleFeedbackText = ReadTextReference(
                context.PuzzleController,
                "puzzleFeedbackText",
                errors);
        }

        if (context.InventoryTray != null)
        {
            context.Inventory = ReadObjectReference<SharedSegmentInventory>(
                context.InventoryTray,
                "inventory",
                errors);
            context.TokenTemplate =
                ReadObjectReference<InventorySegmentToken>(
                    context.InventoryTray,
                    "tokenTemplate",
                    errors);
            context.TokenContainer = ReadObjectReference<Transform>(
                context.InventoryTray,
                "tokenContainer",
                errors);
            context.CountText = ReadTextReference(
                context.InventoryTray,
                "countText",
                errors);

            if (context.CountText != null)
            {
                context.CountFontAsset = context.CountText.font;
                context.CountSharedMaterial =
                    context.CountText.fontSharedMaterial;
            }
        }

        if (context.TokenTemplate != null)
        {
            context.TokenVisualObject =
                context.TokenTemplate.VisualObject;

            if (context.TokenVisualObject == null)
            {
                errors.Add(
                    "SegmentInventoryTray tokenTemplate.VisualObject must " +
                    "be assigned.");
            }
            else
            {
                SpriteRenderer[] visualRenderers =
                    context.TokenVisualObject
                        .GetComponentsInChildren<SpriteRenderer>(true);

                if (visualRenderers.Length == 0)
                {
                    errors.Add(
                        "SegmentInventoryTray tokenTemplate.VisualObject " +
                        "must contain at least one SpriteRenderer.");
                }
                else if (visualRenderers.Length == 1)
                {
                    context.PrimarySegmentRenderer = visualRenderers[0];
                }
                else
                {
                    int highestSortingOrder = int.MinValue;

                    foreach (SpriteRenderer renderer in visualRenderers)
                    {
                        highestSortingOrder = Mathf.Max(
                            highestSortingOrder,
                            renderer.sortingOrder);
                    }

                    List<SpriteRenderer> highestRenderers =
                        new List<SpriteRenderer>();

                    foreach (SpriteRenderer renderer in visualRenderers)
                    {
                        if (renderer.sortingOrder == highestSortingOrder)
                        {
                            highestRenderers.Add(renderer);
                        }
                    }

                    if (highestRenderers.Count != 1)
                    {
                        errors.Add(
                            "tokenTemplate.VisualObject has multiple " +
                            "SpriteRenderers tied for the highest " +
                            $"sortingOrder ({highestSortingOrder}); exactly " +
                            "one primary renderer is required.");
                    }
                    else
                    {
                        context.PrimarySegmentRenderer =
                            highestRenderers[0];
                    }
                }

                context.TokenTemplateRenderers =
                    context.TokenTemplate.gameObject
                        .GetComponentsInChildren<SpriteRenderer>(true);

                if (context.PrimarySegmentRenderer != null &&
                    Array.IndexOf(
                        context.TokenTemplateRenderers,
                        context.PrimarySegmentRenderer) < 0)
                {
                    errors.Add(
                        "The selected primary segment renderer must be on " +
                        "the tokenTemplate GameObject or inside its " +
                        "hierarchy.");
                }
                else if (context.PrimarySegmentRenderer != null &&
                    !context.PrimarySegmentRenderer.enabled)
                {
                    errors.Add(
                        "The selected primary segment renderer must be " +
                        "enabled.");
                }
                else if (context.PrimarySegmentRenderer != null)
                {
                    CapturePrimaryRendererState(context);
                }
            }
        }

        if (context.InventoryTray != null &&
            context.TokenContainer != null)
        {
            List<SpriteRenderer> backdropRenderers =
                new List<SpriteRenderer>();

            foreach (SpriteRenderer renderer in
                context.InventoryTray
                    .GetComponentsInChildren<SpriteRenderer>(true))
            {
                bool insideTokenContainer =
                    renderer.transform == context.TokenContainer ||
                    renderer.transform.IsChildOf(context.TokenContainer);
                bool insideTokenTemplate =
                    context.TokenTemplate != null &&
                    (renderer.transform ==
                        context.TokenTemplate.transform ||
                     renderer.transform.IsChildOf(
                        context.TokenTemplate.transform));

                if (!insideTokenContainer && !insideTokenTemplate)
                {
                    backdropRenderers.Add(renderer);
                }
            }

            context.TrayBackdropRenderers = backdropRenderers.ToArray();

            if (context.TrayBackdropRenderers.Length == 0)
            {
                errors.Add(
                    "SegmentInventoryTray must contain at least one backdrop " +
                    "SpriteRenderer outside tokenContainer.");
            }
        }

        ValidateTextRect(
            context.EntryProgressText,
            "entryProgressText",
            errors);
        ValidateTextRect(
            context.TargetEquationText,
            "targetEquationText",
            errors);
        ValidateTextRect(
            context.CurrentValuesText,
            "currentValuesText",
            errors);
        ValidateTextRect(
            context.AcceptedDigitsText,
            "acceptedDigitsText",
            errors);
        ValidateTextRect(context.FeedbackText, "feedbackText", errors);
        ValidateTextRect(
            context.EquationInstructionText,
            "instructionText",
            errors);
        ValidateTextRect(
            context.PuzzleProgressText,
            "puzzleProgressText",
            errors);
        ValidateTextRect(
            context.HitsLeftText,
            "hitsLeftText",
            errors);
        ValidateTextRect(
            context.PuzzleInstructionText,
            "puzzleInstructionText",
            errors);
        ValidateTextRect(
            context.PuzzleFeedbackText,
            "puzzleFeedbackText",
            errors);
        ValidateTextRect(context.CountText, "countText", errors);

        if (context.TargetEquationText != null &&
            context.TargetEquationText.GetComponent<CanvasRenderer>() == null)
        {
            errors.Add(
                "targetEquationText requires a CanvasRenderer for cloning.");
        }

        List<GameObject> plusMatches = FindAllNamed(
            targetScene,
            PlusLabelLayout.Name);

        if (plusMatches.Count > 1)
        {
            errors.Add(
                $"{targetScene.path} contains {plusMatches.Count} objects " +
                $"named {PlusLabelLayout.Name}; expected at most one before " +
                "repair.");
        }
        else
        {
            context.EquationPlusText =
                plusMatches.Count == 1 ? plusMatches[0] : null;
        }

        ValidateOptionalTextObject(
            context.EquationPlusText,
            context.EquationRoot,
            PlusLabelLayout.Name,
            errors);

        ValidateSerializedHudReference(
            context.SerializedEquationOperatorText,
            context.EquationPlusText,
            "equationOperatorText",
            errors);

        context.EquationReadyText = FindOptionalUniqueNamed(
            targetScene,
            ReadyTextLayout.Name,
            errors);
        ValidateOptionalTextObject(
            context.EquationReadyText,
            context.EquationRoot,
            ReadyTextLayout.Name,
            errors);
        ValidateSerializedHudReference(
            context.SerializedEquationReadyText,
            context.EquationReadyText,
            "equationReadyText",
            errors);

        context.BufferFeedbackText = FindOptionalUniqueNamed(
            targetScene,
            BufferFeedbackLayout.Name,
            errors);
        ValidateOptionalTextObject(
            context.BufferFeedbackText,
            context.EquationRoot,
            BufferFeedbackLayout.Name,
            errors);
        ValidateSerializedHudReference(
            context.SerializedBufferFeedbackText,
            context.BufferFeedbackText,
            "bufferFeedbackText",
            errors);

        context.EquationStatusPanel = FindOptionalUniqueNamed(
            targetScene,
            EquationStatusPanelName,
            errors);
        ValidateStatusPanelBeforeRepair(
            context.EquationStatusPanel,
            context.EquationRoot,
            targetScene,
            errors);

        context.BufferCapacitySlotsRoot = FindOptionalUniqueNamed(
            targetScene,
            "BufferCapacitySlotsRoot",
            errors);
        context.BufferSlotVisual01 = FindOptionalUniqueNamed(
            targetScene,
            "BufferSlotVisual_01",
            errors);
        context.BufferSlotVisual02 = FindOptionalUniqueNamed(
            targetScene,
            "BufferSlotVisual_02",
            errors);

        ValidateBufferSlotObjects(context, errors);

        if (context.EquationRoot != null)
        {
            context.EquationALabelText = FindRemovableLabel(
                targetScene,
                context.EquationRoot,
                "EquationALabelText",
                errors);
            context.EquationBLabelText = FindRemovableLabel(
                targetScene,
                context.EquationRoot,
                "EquationBLabelText",
                errors);
        }

        if (errors.Count == 0)
        {
            context.BombBackgroundState =
                new PreservedHierarchyState(
                    context.BombBackground,
                    BombBackgroundObjectName);
            context.CentralEquationStates =
                new[]
                {
                    new PreservedHierarchyState(
                        context.DisplayA,
                        "Display_A"),
                    new PreservedHierarchyState(
                        context.DisplayB,
                        "Display_B"),
                    new PreservedHierarchyState(
                        context.TargetEquationText.gameObject,
                        "targetEquationText"),
                    new PreservedHierarchyState(
                        context.EquationPlusText,
                        PlusLabelLayout.Name),
                    new PreservedHierarchyState(
                        context.EquationReadyText,
                        ReadyTextLayout.Name)
                };
            context.BufferPresentationState =
                new PreservedHierarchyState(
                    context.InventoryTray.gameObject,
                    "Buffer presentation");
            context.SupportUiStates =
                new[]
                {
                    new PreservedHierarchyState(
                        context.EquationInstructionText.gameObject,
                        "instructionText"),
                    new PreservedHierarchyState(
                        context.BufferFeedbackText,
                        BufferFeedbackLayout.Name),
                    new PreservedHierarchyState(
                        context.PuzzleInstructionText.gameObject,
                        "puzzleInstructionText")
                };
            context.PuzzleControllerState =
                new PreservedGameObjectState(
                    context.PuzzleController.gameObject);
        }

        return context;
    }

    private static void ApplyTutorialUi(TutorialContext context)
    {
        if (context.EquationALabelText != null)
        {
            Undo.DestroyObjectImmediate(context.EquationALabelText);
        }

        if (context.EquationBLabelText != null)
        {
            Undo.DestroyObjectImmediate(context.EquationBLabelText);
        }

        GameObject plusObject = CreateOrRepairTextObject(
            context.EquationPlusText,
            context.TargetEquationText.gameObject,
            context.EquationRoot.transform,
            PlusLabelLayout);

        TMP_Text plusText = plusObject.GetComponent<TMP_Text>();
        ConfigureText(
            plusObject.GetComponent<RectTransform>(),
            plusText,
            PlusLabelLayout.Position,
            PlusLabelLayout.Size,
            PlusLabelLayout.FontSize,
            HorizontalAlignmentOptions.Center,
            setVerticalMiddle: true,
            content: PlusLabelLayout.Text,
            setOverflow: true);
        Undo.RecordObject(plusText, UndoName);
        plusText.fontStyle = FontStyles.Bold;
        SetActiveWithUndo(plusObject, true);

        GameObject readyObject = CreateOrRepairTextObject(
            context.EquationReadyText,
            context.TargetEquationText.gameObject,
            context.EquationRoot.transform,
            ReadyTextLayout);
        TMP_Text readyText = readyObject.GetComponent<TMP_Text>();
        ConfigureText(
            readyObject.GetComponent<RectTransform>(),
            readyText,
            ReadyTextLayout.Position,
            ReadyTextLayout.Size,
            ReadyTextLayout.FontSize,
            HorizontalAlignmentOptions.Center,
            setVerticalMiddle: true,
            content: ReadyTextLayout.Text,
            setOverflow: true);
        SetActiveWithUndo(readyObject, false);

        GameObject bufferFeedbackObject = CreateOrRepairTextObject(
            context.BufferFeedbackText,
            context.FeedbackText.gameObject,
            context.EquationRoot.transform,
            BufferFeedbackLayout);
        TMP_Text bufferFeedbackText =
            bufferFeedbackObject.GetComponent<TMP_Text>();
        ConfigureText(
            bufferFeedbackObject.GetComponent<RectTransform>(),
            bufferFeedbackText,
            BufferFeedbackLayout.Position,
            BufferFeedbackLayout.Size,
            BufferFeedbackLayout.FontSize,
            HorizontalAlignmentOptions.Center,
            setVerticalMiddle: true,
            content: BufferFeedbackLayout.Text,
            setOverflow: true);
        SetActiveWithUndo(bufferFeedbackObject, false);

        ConfigureText(
            context.TargetEquationText.GetComponent<RectTransform>(),
            context.TargetEquationText,
            new Vector2(590f, -100f),
            new Vector2(360f, 190f),
            104f,
            HorizontalAlignmentOptions.Center,
            setVerticalMiddle: true,
            content: "=  5",
            setOverflow: true);
        Undo.RecordObject(context.TargetEquationText, UndoName);
        context.TargetEquationText.fontStyle = FontStyles.Bold;

        GameObject statusPanel = CreateOrRepairStatusPanel(context);
        ConfigureStatusText(
            context.EntryProgressText,
            EntryProgressLayout);
        ConfigureStatusText(
            context.CurrentValuesText,
            CurrentValuesLayout);
        ConfigureStatusText(
            context.AcceptedDigitsText,
            AcceptedDigitsLayout);
        ConfigureStatusText(
            context.FeedbackText,
            FeedbackLayout);

        ConfigureText(
            context.EquationInstructionText.GetComponent<RectTransform>(),
            context.EquationInstructionText,
            new Vector2(0f, -445f),
            new Vector2(1200f, 40f),
            16f,
            HorizontalAlignmentOptions.Center,
            setVerticalMiddle: true,
            content: PhaseTwoInstruction,
            setOverflow: true);
        SetActiveWithUndo(
            context.EquationInstructionText.gameObject,
            false);

        ConfigureText(
            context.PuzzleInstructionText.GetComponent<RectTransform>(),
            context.PuzzleInstructionText,
            new Vector2(0f, -405f),
            new Vector2(1400f, 110f),
            22f,
            HorizontalAlignmentOptions.Center,
            setVerticalMiddle: true,
            content: PhaseOneInstruction,
            setOverflow: true);
        ConfigureAsNonInteractive(context.PuzzleProgressText);
        ConfigureAsNonInteractive(context.HitsLeftText);
        ConfigureAsNonInteractive(context.PuzzleFeedbackText);

        ConfigureTrayLayout(context.InventoryTray);
        ConfigureCountText(context.CountText);
        DisableTrayBackdropRenderers(context.TrayBackdropRenderers);
        DisableNonPrimaryTokenRenderers(
            context.TokenTemplateRenderers,
            context.PrimarySegmentRenderer);
        GameObject slotsRoot = CreateOrRepairSlotsRoot(context);
        GameObject slot01 = CreateOrRepairBufferSlot(
            context.BufferSlotVisual01,
            context.PrimarySegmentRenderer,
            slotsRoot.transform,
            "BufferSlotVisual_01",
            new Vector3(-0.9f, 0.1f, 0f));
        GameObject slot02 = CreateOrRepairBufferSlot(
            context.BufferSlotVisual02,
            context.PrimarySegmentRenderer,
            slotsRoot.transform,
            "BufferSlotVisual_02",
            new Vector3(0.9f, 0.1f, 0f));

        AssignHudReferences(
            context.EquationHud,
            plusObject.GetComponent<TMP_Text>(),
            readyText,
            bufferFeedbackText);
        ValidateAppliedState(
            context,
            statusPanel,
            plusObject,
            readyObject,
            bufferFeedbackObject,
            slotsRoot,
            slot01,
            slot02);
    }

    private static GameObject CreateOrRepairStatusPanel(
        TutorialContext context)
    {
        GameObject panel = context.EquationStatusPanel;

        if (panel == null)
        {
            panel = new GameObject(
                EquationStatusPanelName,
                typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(panel, UndoName);
        }

        if (panel.transform.parent != context.EquationRoot.transform)
        {
            Undo.SetTransformParent(
                panel.transform,
                context.EquationRoot.transform,
                UndoName);
        }

        CanvasRenderer[] canvasRenderers =
            panel.GetComponents<CanvasRenderer>();

        if (canvasRenderers.Length == 0)
        {
            Undo.AddComponent<CanvasRenderer>(panel);
        }

        Image[] images = panel.GetComponents<Image>();

        if (images.Length == 0)
        {
            Undo.AddComponent<Image>(panel);
        }

        canvasRenderers = panel.GetComponents<CanvasRenderer>();

        for (int i = 1; i < canvasRenderers.Length; i++)
        {
            Undo.DestroyObjectImmediate(canvasRenderers[i]);
        }

        images = panel.GetComponents<Image>();

        for (int i = 1; i < images.Length; i++)
        {
            Undo.DestroyObjectImmediate(images[i]);
        }

        RectTransform rectTransform =
            panel.GetComponent<RectTransform>();
        Undo.RecordObject(rectTransform, UndoName);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition =
            EquationStatusPanelPosition;
        rectTransform.sizeDelta = EquationStatusPanelSize;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
        rectTransform.SetSiblingIndex(0);

        Image image = panel.GetComponent<Image>();
        Undo.RecordObject(image, UndoName);
        image.sprite = null;
        image.material = null;
        image.type = Image.Type.Simple;
        image.color = EquationStatusPanelColor;
        image.raycastTarget = false;
        image.maskable = true;
        image.enabled = true;
        SetActiveWithUndo(panel, true);
        return panel;
    }

    private static void ConfigureAsNonInteractive(TMP_Text text)
    {
        Undo.RecordObject(text, UndoName);
        text.raycastTarget = false;
    }

    private static GameObject CreateOrRepairTextObject(
        GameObject existingObject,
        GameObject template,
        Transform parent,
        StaticLabelLayout layout)
    {
        GameObject textObject = existingObject;

        if (textObject == null)
        {
            textObject = Object.Instantiate(template);
            textObject.name = layout.Name;
            Undo.RegisterCreatedObjectUndo(
                textObject,
                $"Create {layout.Name}");
        }

        if (textObject.transform.parent != parent)
        {
            Undo.SetTransformParent(
                textObject.transform,
                parent,
                $"Parent {layout.Name}");
        }

        return textObject;
    }

    private static void SetActiveWithUndo(
        GameObject gameObject,
        bool active)
    {
        if (gameObject.activeSelf == active)
        {
            return;
        }

        Undo.RecordObject(gameObject, UndoName);
        gameObject.SetActive(active);
    }

    private static void ConfigureTrayLayout(
        SegmentInventoryTray inventoryTray)
    {
        Undo.RecordObject(inventoryTray, UndoName);
        SerializedObject serializedTray =
            new SerializedObject(inventoryTray);
        serializedTray.Update();
        RequireSerializedProperty(
            serializedTray,
            "maximumVisibleTokens").intValue = 2;
        RequireSerializedProperty(
            serializedTray,
            "columns").intValue = 2;
        RequireSerializedProperty(
            serializedTray,
            "firstTokenLocalPosition").vector2Value =
                new Vector2(-0.9f, 0.1f);
        RequireSerializedProperty(
            serializedTray,
            "tokenSpacing").vector2Value =
                new Vector2(1.8f, 0f);
        serializedTray.ApplyModifiedProperties();
    }

    private static void ConfigureCountText(TMP_Text countText)
    {
        RectTransform rectTransform =
            countText.GetComponent<RectTransform>();

        Undo.RecordObject(rectTransform, UndoName);
        rectTransform.localPosition = new Vector3(0f, 0.95f, 0f);
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
        rectTransform.sizeDelta = new Vector2(6f, 0.8f);

        Undo.RecordObject(countText, UndoName);
        countText.text = "BUFFER 0 / 2";
        countText.fontSize = 3.2f;
        countText.horizontalAlignment =
            HorizontalAlignmentOptions.Center;
        countText.verticalAlignment = VerticalAlignmentOptions.Middle;
        countText.enableAutoSizing = false;
        countText.overflowMode = TextOverflowModes.Overflow;
        countText.raycastTarget = false;
        countText.richText = true;
    }

    private static void DisableTrayBackdropRenderers(
        SpriteRenderer[] backdropRenderers)
    {
        foreach (SpriteRenderer renderer in backdropRenderers)
        {
            Undo.RecordObject(renderer, UndoName);
            renderer.enabled = false;
        }
    }

    private static void DisableNonPrimaryTokenRenderers(
        SpriteRenderer[] tokenRenderers,
        SpriteRenderer primaryRenderer)
    {
        foreach (SpriteRenderer renderer in tokenRenderers)
        {
            if (renderer == primaryRenderer)
            {
                continue;
            }

            Undo.RecordObject(renderer, UndoName);
            renderer.enabled = false;
        }
    }

    private static GameObject CreateOrRepairSlotsRoot(
        TutorialContext context)
    {
        GameObject slotsRoot = context.BufferCapacitySlotsRoot;

        if (slotsRoot == null)
        {
            slotsRoot = new GameObject("BufferCapacitySlotsRoot");
            Undo.RegisterCreatedObjectUndo(
                slotsRoot,
                "Create BufferCapacitySlotsRoot");
        }

        if (slotsRoot.transform.parent != context.TokenContainer)
        {
            Undo.SetTransformParent(
                slotsRoot.transform,
                context.TokenContainer,
                "Parent BufferCapacitySlotsRoot");
        }

        Undo.RecordObject(slotsRoot.transform, UndoName);
        slotsRoot.transform.localPosition = Vector3.zero;
        slotsRoot.transform.localRotation = Quaternion.identity;
        slotsRoot.transform.localScale = Vector3.one;
        SetActiveWithUndo(slotsRoot, true);
        return slotsRoot;
    }

    private static GameObject CreateOrRepairBufferSlot(
        GameObject existingObject,
        SpriteRenderer primaryRenderer,
        Transform parent,
        string objectName,
        Vector3 localPosition)
    {
        GameObject slotObject = existingObject;

        if (slotObject == null)
        {
            slotObject = new GameObject(objectName);
            Undo.RegisterCreatedObjectUndo(
                slotObject,
                $"Create {objectName}");
        }

        if (slotObject.transform.parent != parent)
        {
            Undo.SetTransformParent(
                slotObject.transform,
                parent,
                $"Parent {objectName}");
        }

        Undo.RecordObject(slotObject.transform, UndoName);
        slotObject.transform.localPosition = localPosition;
        slotObject.transform.localRotation = Quaternion.identity;
        slotObject.transform.localScale = Vector3.one;

        StripSlotRootComponents(slotObject);

        GameObject slotSegmentVisual =
            FindDirectChild(slotObject, "SlotSegmentVisual");

        if (!CanReuseSlotSegmentVisual(
                slotSegmentVisual,
                primaryRenderer))
        {
            DestroyObsoleteSlotChildren(slotObject, null);
            slotSegmentVisual =
                ClonePrimarySegmentVisual(
                    primaryRenderer,
                    slotObject.transform);
        }
        else
        {
            DestroyObsoleteSlotChildren(
                slotObject,
                slotSegmentVisual);
        }

        ConfigureSlotSegmentVisual(
            slotSegmentVisual,
            primaryRenderer);

        SetActiveWithUndo(slotObject, true);
        return slotObject;
    }

    private static void StripSlotRootComponents(GameObject slotObject)
    {
        foreach (MonoBehaviour behaviour in
            slotObject.GetComponents<MonoBehaviour>())
        {
            if (behaviour != null)
            {
                Undo.DestroyObjectImmediate(behaviour);
            }
        }

        foreach (Collider2D collider in
            slotObject.GetComponents<Collider2D>())
        {
            Undo.DestroyObjectImmediate(collider);
        }

        foreach (Rigidbody2D rigidbody in
            slotObject.GetComponents<Rigidbody2D>())
        {
            Undo.DestroyObjectImmediate(rigidbody);
        }

        foreach (SpriteRenderer renderer in
            slotObject.GetComponents<SpriteRenderer>())
        {
            Undo.DestroyObjectImmediate(renderer);
        }
    }

    private static GameObject FindDirectChild(
        GameObject parent,
        string objectName)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.name == objectName)
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private static bool CanReuseSlotSegmentVisual(
        GameObject slotSegmentVisual,
        SpriteRenderer primaryRenderer)
    {
        if (slotSegmentVisual == null)
        {
            return false;
        }

        SpriteRenderer[] renderers =
            slotSegmentVisual
                .GetComponentsInChildren<SpriteRenderer>(true);

        return renderers.Length == 1 &&
            renderers[0].sprite == primaryRenderer.sprite &&
            renderers[0].sharedMaterial ==
                primaryRenderer.sharedMaterial &&
            renderers[0].sortingLayerID ==
                primaryRenderer.sortingLayerID;
    }

    private static void DestroyObsoleteSlotChildren(
        GameObject slotObject,
        GameObject childToKeep)
    {
        List<GameObject> children = new List<GameObject>();

        foreach (Transform child in slotObject.transform)
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            if (child == null ||
                child.transform.parent != slotObject.transform)
            {
                throw new InvalidOperationException(
                    $"Refusing to remove an obsolete child from " +
                    $"{slotObject.name} because it is not a direct " +
                    "descendant of that slot root.");
            }
        }

        foreach (GameObject child in children)
        {
            if (child != childToKeep)
            {
                Undo.DestroyObjectImmediate(child);
            }
        }
    }

    private static GameObject ClonePrimarySegmentVisual(
        SpriteRenderer primaryRenderer,
        Transform parent)
    {
        SpriteRenderer[] sourceOwnerRenderers =
            primaryRenderer.gameObject.GetComponents<SpriteRenderer>();
        int primaryComponentIndex =
            Array.IndexOf(sourceOwnerRenderers, primaryRenderer);

        if (primaryComponentIndex < 0)
        {
            throw new InvalidOperationException(
                "The validated primary segment renderer could not be " +
                "located on its owning GameObject.");
        }

        GameObject slotSegmentVisual =
            Object.Instantiate(primaryRenderer.gameObject);
        slotSegmentVisual.name = "SlotSegmentVisual";
        Undo.RegisterCreatedObjectUndo(
            slotSegmentVisual,
            "Create SlotSegmentVisual");
        Undo.SetTransformParent(
            slotSegmentVisual.transform,
            parent,
            "Parent SlotSegmentVisual");

        SpriteRenderer[] clonedOwnerRenderers =
            slotSegmentVisual.GetComponents<SpriteRenderer>();

        if (primaryComponentIndex >= clonedOwnerRenderers.Length)
        {
            throw new InvalidOperationException(
                "The cloned SlotSegmentVisual does not contain the " +
                "validated primary renderer component.");
        }

        StripSlotVisualComponents(
            slotSegmentVisual,
            clonedOwnerRenderers[primaryComponentIndex]);
        return slotSegmentVisual;
    }

    private static void ConfigureSlotSegmentVisual(
        GameObject slotSegmentVisual,
        SpriteRenderer primaryRenderer)
    {
        SpriteRenderer[] slotRenderers =
            slotSegmentVisual
                .GetComponentsInChildren<SpriteRenderer>(true);

        if (slotRenderers.Length != 1)
        {
            throw new InvalidOperationException(
                "SlotSegmentVisual must contain exactly one " +
                "SpriteRenderer after cleanup.");
        }

        SpriteRenderer slotRenderer = slotRenderers[0];
        StripSlotVisualComponents(slotSegmentVisual, slotRenderer);

        Undo.RecordObject(slotSegmentVisual.transform, UndoName);
        slotSegmentVisual.transform.localPosition = Vector3.zero;
        slotSegmentVisual.transform.localRotation = Quaternion.identity;
        slotSegmentVisual.transform.localScale =
            new Vector3(0.9f, 0.18f, 1f);

        Undo.RecordObject(slotRenderer, UndoName);
        slotRenderer.sprite = primaryRenderer.sprite;
        slotRenderer.sharedMaterial = primaryRenderer.sharedMaterial;
        slotRenderer.sortingLayerID =
            primaryRenderer.sortingLayerID;
        slotRenderer.sortingOrder =
            primaryRenderer.sortingOrder - 1;
        slotRenderer.color =
            new Color(0.55f, 0.72f, 0.78f, 0.18f);
        slotRenderer.enabled = true;
        SetActiveWithUndo(slotSegmentVisual, true);
    }

    private static void StripSlotVisualComponents(
        GameObject slotSegmentVisual,
        SpriteRenderer rendererToKeep)
    {
        foreach (MonoBehaviour behaviour in
            slotSegmentVisual
                .GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour != null)
            {
                Undo.DestroyObjectImmediate(behaviour);
            }
        }

        foreach (Collider2D collider in
            slotSegmentVisual
                .GetComponentsInChildren<Collider2D>(true))
        {
            Undo.DestroyObjectImmediate(collider);
        }

        foreach (Rigidbody2D rigidbody in
            slotSegmentVisual
                .GetComponentsInChildren<Rigidbody2D>(true))
        {
            Undo.DestroyObjectImmediate(rigidbody);
        }

        foreach (SpriteRenderer renderer in
            slotSegmentVisual
                .GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (renderer != rendererToKeep)
            {
                Undo.DestroyObjectImmediate(renderer);
            }
        }
    }

    private static void AssignHudReferences(
        CodebreakerEquationHUD equationHud,
        TMP_Text operatorText,
        TMP_Text readyText,
        TMP_Text bufferFeedbackText)
    {
        Undo.RecordObject(equationHud, UndoName);
        SerializedObject serializedHud =
            new SerializedObject(equationHud);
        serializedHud.Update();

        SerializedProperty operatorProperty =
            RequireSerializedProperty(
                serializedHud,
                "equationOperatorText");
        SerializedProperty readyProperty =
            RequireSerializedProperty(
                serializedHud,
                "equationReadyText");
        SerializedProperty bufferProperty =
            RequireSerializedProperty(
                serializedHud,
                "bufferFeedbackText");

        operatorProperty.objectReferenceValue = operatorText;
        readyProperty.objectReferenceValue = readyText;
        bufferProperty.objectReferenceValue = bufferFeedbackText;
        serializedHud.ApplyModifiedProperties();
        serializedHud.Update();

        if (operatorProperty.objectReferenceValue != operatorText ||
            readyProperty.objectReferenceValue != readyText ||
            bufferProperty.objectReferenceValue != bufferFeedbackText)
        {
            throw new InvalidOperationException(
                "CodebreakerEquationHUD serialized feedback references " +
                "could not be assigned.");
        }
    }

    private static SerializedProperty RequireSerializedProperty(
        SerializedObject serializedObject,
        string propertyName)
    {
        SerializedProperty property =
            serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            throw new InvalidOperationException(
                $"{serializedObject.targetObject.GetType().Name} has no " +
                $"serialized property {propertyName}.");
        }

        return property;
    }

    private static void ValidateAppliedState(
        TutorialContext context,
        GameObject statusPanel,
        GameObject plusObject,
        GameObject readyObject,
        GameObject bufferFeedbackObject,
        GameObject slotsRoot,
        GameObject slot01,
        GameObject slot02)
    {
        List<string> errors = new List<string>();
        RequireExactNamedCount(
            context.TargetScene,
            "EquationALabelText",
            0,
            errors);
        RequireExactNamedCount(
            context.TargetScene,
            "EquationBLabelText",
            0,
            errors);
        RequireExactNamedCount(
            context.TargetScene,
            EquationStatusPanelName,
            1,
            errors);
        RequireExactNamedCount(
            context.TargetScene,
            PlusLabelLayout.Name,
            1,
            errors);
        RequireExactNamedCount(
            context.TargetScene,
            ReadyTextLayout.Name,
            1,
            errors);
        RequireExactNamedCount(
            context.TargetScene,
            BufferFeedbackLayout.Name,
            1,
            errors);
        RequireExactNamedCount(
            context.TargetScene,
            "BufferCapacitySlotsRoot",
            1,
            errors);
        RequireExactNamedCount(
            context.TargetScene,
            "BufferSlotVisual_01",
            1,
            errors);
        RequireExactNamedCount(
            context.TargetScene,
            "BufferSlotVisual_02",
            1,
            errors);
        RequireExactNamedCount(
            context.TargetScene,
            "SlotSegmentVisual",
            2,
            errors);

        if (plusObject.transform.parent !=
                context.EquationRoot.transform ||
            readyObject.transform.parent !=
                context.EquationRoot.transform ||
            bufferFeedbackObject.transform.parent !=
                context.EquationRoot.transform)
        {
            errors.Add(
                "Equation feedback text objects must be direct children of " +
                "EquationEntryRoot.");
        }

        if (slotsRoot.transform.parent != context.TokenContainer ||
            slot01.transform.parent != slotsRoot.transform ||
            slot02.transform.parent != slotsRoot.transform)
        {
            errors.Add(
                "Buffer capacity slot hierarchy is incorrect.");
        }

        if (slotsRoot.transform.localPosition != Vector3.zero ||
            slotsRoot.transform.localRotation != Quaternion.identity ||
            slotsRoot.transform.localScale != Vector3.one ||
            !slotsRoot.activeSelf)
        {
            errors.Add(
                "BufferCapacitySlotsRoot transform or active state is " +
                "invalid.");
        }

        ValidateTextLayout(
            plusObject.GetComponent<TMP_Text>(),
            PlusLabelLayout,
            PlusLabelLayout.Text,
            expectedActive: true,
            errors: errors);
        if (plusObject.GetComponent<TMP_Text>().fontStyle !=
            FontStyles.Bold)
        {
            errors.Add("EquationPlusText must use bold font styling.");
        }
        ValidateTextLayout(
            readyObject.GetComponent<TMP_Text>(),
            ReadyTextLayout,
            string.Empty,
            expectedActive: false,
            errors: errors);
        ValidateTextLayout(
            bufferFeedbackObject.GetComponent<TMP_Text>(),
            BufferFeedbackLayout,
            string.Empty,
            expectedActive: false,
            errors: errors);
        ValidateTextLayout(
            context.TargetEquationText,
            new StaticLabelLayout(
                context.TargetEquationText.gameObject.name,
                "=  5",
                new Vector2(590f, -100f),
                new Vector2(360f, 190f),
                104f),
            "=  5",
            context.TargetEquationText.gameObject.activeSelf,
            errors);
        if (context.TargetEquationText.fontStyle != FontStyles.Bold)
        {
            errors.Add(
                "targetEquationText must use bold font styling.");
        }
        ValidateTextLayout(
            context.EquationInstructionText,
            new StaticLabelLayout(
                context.EquationInstructionText.gameObject.name,
                PhaseTwoInstruction,
                new Vector2(0f, -445f),
                new Vector2(1200f, 40f),
                16f),
            PhaseTwoInstruction,
            expectedActive: false,
            errors);
        ValidateTextLayout(
            context.PuzzleInstructionText,
            new StaticLabelLayout(
                context.PuzzleInstructionText.gameObject.name,
                PhaseOneInstruction,
                new Vector2(0f, -405f),
                new Vector2(1400f, 110f),
                22f),
            PhaseOneInstruction,
            context.PuzzleInstructionText.gameObject.activeSelf,
            errors);

        ValidateSupportTextLayout(
            context.EntryProgressText,
            EntryProgressLayout,
            errors);
        ValidateSupportTextLayout(
            context.CurrentValuesText,
            CurrentValuesLayout,
            errors);
        ValidateSupportTextLayout(
            context.AcceptedDigitsText,
            AcceptedDigitsLayout,
            errors);
        ValidateSupportTextLayout(
            context.FeedbackText,
            FeedbackLayout,
            errors);
        ValidateStatusPanelAppliedState(
            statusPanel,
            context.EquationRoot,
            errors);
        ValidateStatusDrawOrder(
            statusPanel,
            context,
            errors);
        ValidateStatusColors(
            context,
            errors);
        ValidateTrayLayout(context.InventoryTray, errors);
        ValidateCountTextLayout(context, errors);
        ValidateBackdropCleanup(context, errors);
        ValidateTokenVisualCleanup(context, errors);
        ValidateSlotVisual(
            slot01,
            context.PrimarySegmentRenderer,
            "BufferSlotVisual_01",
            errors);
        ValidateSlotVisual(
            slot02,
            context.PrimarySegmentRenderer,
            "BufferSlotVisual_02",
            errors);

        SerializedObject serializedHud =
            new SerializedObject(context.EquationHud);
        serializedHud.Update();
        ValidateObjectReference(
            serializedHud,
            "entryProgressText",
            context.EntryProgressText,
            errors);
        ValidateObjectReference(
            serializedHud,
            "targetEquationText",
            context.TargetEquationText,
            errors);
        ValidateObjectReference(
            serializedHud,
            "currentValuesText",
            context.CurrentValuesText,
            errors);
        ValidateObjectReference(
            serializedHud,
            "acceptedDigitsText",
            context.AcceptedDigitsText,
            errors);
        ValidateObjectReference(
            serializedHud,
            "feedbackText",
            context.FeedbackText,
            errors);
        ValidateObjectReference(
            serializedHud,
            "instructionText",
            context.EquationInstructionText,
            errors);
        ValidateObjectReference(
            serializedHud,
            "equationOperatorText",
            plusObject.GetComponent<TMP_Text>(),
            errors);
        ValidateObjectReference(
            serializedHud,
            "equationReadyText",
            readyObject.GetComponent<TMP_Text>(),
            errors);
        ValidateObjectReference(
            serializedHud,
            "bufferFeedbackText",
            bufferFeedbackObject.GetComponent<TMP_Text>(),
            errors);

        context.BombBackgroundState.Validate(errors);

        foreach (PreservedHierarchyState state in
            context.CentralEquationStates)
        {
            state.Validate(errors);
        }

        context.BufferPresentationState.Validate(errors);

        foreach (PreservedHierarchyState state in
            context.SupportUiStates)
        {
            state.Validate(errors);
        }

        context.PuzzleControllerState.Validate(
            "Gameplay controller",
            errors);

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Post-build validation failed:\n- " +
                string.Join("\n- ", errors));
        }
    }

    private static void ValidateTextLayout(
        TMP_Text text,
        StaticLabelLayout layout,
        string expectedText,
        bool expectedActive,
        List<string> errors)
    {
        RectTransform rectTransform =
            text.GetComponent<RectTransform>();

        if (rectTransform.anchoredPosition != layout.Position ||
            rectTransform.sizeDelta != layout.Size ||
            rectTransform.anchorMin != new Vector2(0.5f, 0.5f) ||
            rectTransform.anchorMax != new Vector2(0.5f, 0.5f) ||
            rectTransform.pivot != new Vector2(0.5f, 0.5f) ||
            rectTransform.localScale != Vector3.one ||
            rectTransform.localRotation != Quaternion.identity ||
            !Mathf.Approximately(text.fontSize, layout.FontSize) ||
            text.horizontalAlignment !=
                HorizontalAlignmentOptions.Center ||
            text.verticalAlignment != VerticalAlignmentOptions.Middle ||
            text.enableAutoSizing ||
            text.raycastTarget ||
            text.overflowMode != TextOverflowModes.Overflow ||
            !text.richText ||
            text.text != expectedText ||
            text.gameObject.activeSelf != expectedActive)
        {
            errors.Add($"{layout.Name} layout or text settings are invalid.");
        }
    }

    private static void ValidateStatusPanelAppliedState(
        GameObject panel,
        GameObject equationRoot,
        List<string> errors)
    {
        if (panel == null)
        {
            errors.Add($"{EquationStatusPanelName} is missing.");
            return;
        }

        RectTransform[] rectTransforms =
            panel.GetComponents<RectTransform>();
        CanvasRenderer[] canvasRenderers =
            panel.GetComponents<CanvasRenderer>();
        Image[] images = panel.GetComponents<Image>();

        if (panel.transform.parent != equationRoot.transform ||
            !panel.activeSelf ||
            panel.transform.GetSiblingIndex() != 0)
        {
            errors.Add(
                $"{EquationStatusPanelName} hierarchy, active state, or " +
                "draw order is invalid.");
        }

        if (rectTransforms.Length != 1)
        {
            errors.Add(
                $"{EquationStatusPanelName} must have exactly one " +
                "RectTransform.");
        }
        else
        {
            RectTransform rectTransform = rectTransforms[0];

            if (rectTransform.anchorMin !=
                    new Vector2(0.5f, 0.5f) ||
                rectTransform.anchorMax !=
                    new Vector2(0.5f, 0.5f) ||
                rectTransform.pivot != new Vector2(0.5f, 0.5f) ||
                rectTransform.anchoredPosition !=
                    EquationStatusPanelPosition ||
                rectTransform.sizeDelta != EquationStatusPanelSize ||
                rectTransform.localRotation != Quaternion.identity ||
                rectTransform.localScale != Vector3.one)
            {
                errors.Add(
                    $"{EquationStatusPanelName} RectTransform is invalid.");
            }
        }

        if (canvasRenderers.Length != 1)
        {
            errors.Add(
                $"{EquationStatusPanelName} must have exactly one " +
                "CanvasRenderer.");
        }

        if (images.Length != 1)
        {
            errors.Add(
                $"{EquationStatusPanelName} must have exactly one Image.");
        }
        else
        {
            Image image = images[0];
            SerializedObject serializedImage =
                new SerializedObject(image);
            serializedImage.Update();
            SerializedProperty materialProperty =
                serializedImage.FindProperty("m_Material");

            if (image.sprite != null ||
                materialProperty == null ||
                materialProperty.objectReferenceValue != null ||
                image.type != Image.Type.Simple ||
                image.color != EquationStatusPanelColor ||
                image.raycastTarget ||
                !image.maskable ||
                !image.enabled)
            {
                errors.Add(
                    $"{EquationStatusPanelName} Image settings are invalid.");
            }
        }

        if (panel.GetComponents<TMP_Text>().Length > 0 ||
            panel.GetComponents<Button>().Length > 0 ||
            panel.GetComponents<Selectable>().Length > 0 ||
            panel
                .GetComponents<UnityEngine.EventSystems.EventTrigger>()
                .Length > 0 ||
            panel.GetComponents<Collider2D>().Length > 0 ||
            panel.GetComponents<Rigidbody2D>().Length > 0)
        {
            errors.Add(
                $"{EquationStatusPanelName} contains a prohibited " +
                "interaction, text, or physics component.");
        }

        foreach (MonoBehaviour behaviour in
            panel.GetComponents<MonoBehaviour>())
        {
            if (behaviour == null || !(behaviour is Image))
            {
                errors.Add(
                    $"{EquationStatusPanelName} contains a prohibited " +
                    "MonoBehaviour.");
                break;
            }
        }

        foreach (Component component in panel.GetComponents<Component>())
        {
            if (component == null ||
                (!(component is RectTransform) &&
                 !(component is CanvasRenderer) &&
                 !(component is Image)))
            {
                errors.Add(
                    $"{EquationStatusPanelName} contains an unsupported " +
                    "component.");
                break;
            }
        }
    }

    private static void ValidateStatusDrawOrder(
        GameObject panel,
        TutorialContext context,
        List<string> errors)
    {
        TMP_Text[] statusTexts =
        {
            context.EntryProgressText,
            context.CurrentValuesText,
            context.AcceptedDigitsText,
            context.FeedbackText
        };

        foreach (TMP_Text text in statusTexts)
        {
            Transform directChild =
                GetDirectChildOfRoot(
                    text.transform,
                    context.EquationRoot.transform);

            if (directChild == null ||
                directChild == panel.transform ||
                directChild.GetSiblingIndex() <=
                    panel.transform.GetSiblingIndex())
            {
                errors.Add(
                    $"{text.gameObject.name} must render after " +
                    $"{EquationStatusPanelName}.");
            }
        }
    }

    private static Transform GetDirectChildOfRoot(
        Transform descendant,
        Transform root)
    {
        Transform current = descendant;

        while (current != null && current.parent != root)
        {
            current = current.parent;
        }

        return current != null && current.parent == root
            ? current
            : null;
    }

    private static void ValidateStatusColors(
        TutorialContext context,
        List<string> errors)
    {
        TMP_Text[] statusTexts =
        {
            context.EntryProgressText,
            context.CurrentValuesText,
            context.AcceptedDigitsText,
            context.FeedbackText
        };

        foreach (TMP_Text text in statusTexts)
        {
            if (text.color == context.EquationReadyColor)
            {
                errors.Add(
                    $"{text.gameObject.name} must not use " +
                    "equationReadyColor for static status information.");
            }
        }
    }

    private static void ValidateSupportTextLayout(
        TMP_Text text,
        StatusTextLayout layout,
        List<string> errors)
    {
        RectTransform rectTransform =
            text.GetComponent<RectTransform>();

        if (rectTransform.anchoredPosition != layout.Position ||
            rectTransform.sizeDelta != layout.Size ||
            rectTransform.anchorMin != new Vector2(0.5f, 0.5f) ||
            rectTransform.anchorMax != new Vector2(0.5f, 0.5f) ||
            rectTransform.pivot != new Vector2(0.5f, 0.5f) ||
            rectTransform.localScale != Vector3.one ||
            rectTransform.localRotation != Quaternion.identity ||
            !Mathf.Approximately(text.fontSize, layout.FontSize) ||
            text.horizontalAlignment != HorizontalAlignmentOptions.Left ||
            text.verticalAlignment != VerticalAlignmentOptions.Middle ||
            text.enableAutoSizing ||
            text.raycastTarget ||
            text.overflowMode != TextOverflowModes.Overflow ||
            text.fontStyle != layout.FontStyle ||
            text.color != layout.Color ||
            text.text != layout.Text ||
            !text.gameObject.activeSelf)
        {
            errors.Add(
                $"{layout.Name} support-text layout is invalid.");
        }
    }

    private static void ValidateCountTextLayout(
        TutorialContext context,
        List<string> errors)
    {
        TMP_Text countText = context.CountText;
        RectTransform rectTransform =
            countText.GetComponent<RectTransform>();

        if (rectTransform.localPosition !=
                new Vector3(0f, 0.95f, 0f) ||
            rectTransform.localRotation != Quaternion.identity ||
            rectTransform.localScale != Vector3.one ||
            rectTransform.sizeDelta != new Vector2(6f, 0.8f) ||
            !Mathf.Approximately(countText.fontSize, 3.2f) ||
            countText.horizontalAlignment !=
                HorizontalAlignmentOptions.Center ||
            countText.verticalAlignment !=
                VerticalAlignmentOptions.Middle ||
            countText.enableAutoSizing ||
            countText.overflowMode != TextOverflowModes.Overflow ||
            countText.raycastTarget ||
            !countText.richText ||
            countText.text != "BUFFER 0 / 2" ||
            countText.font != context.CountFontAsset ||
            countText.fontSharedMaterial !=
                context.CountSharedMaterial)
        {
            errors.Add("SegmentInventoryTray countText layout is invalid.");
        }
    }

    private static void ValidateBackdropCleanup(
        TutorialContext context,
        List<string> errors)
    {
        SegmentInventoryTray appliedTray =
            RequireUniqueComponent<SegmentInventoryTray>(
                context.TargetScene,
                errors);
        InventoryDropZone appliedDropZone =
            RequireUniqueComponent<InventoryDropZone>(
                context.TargetScene,
                errors);

        if (appliedTray != context.InventoryTray ||
            appliedDropZone != context.InventoryDropZone)
        {
            errors.Add(
                "SegmentInventoryTray or InventoryDropZone identity " +
                "changed.");
        }

        foreach (SpriteRenderer renderer in
            context.InventoryTray
                .GetComponentsInChildren<SpriteRenderer>(true))
        {
            bool insideTokenContainer =
                renderer.transform == context.TokenContainer ||
                renderer.transform.IsChildOf(context.TokenContainer);
            bool insideTokenTemplate =
                renderer.transform == context.TokenTemplate.transform ||
                renderer.transform.IsChildOf(
                    context.TokenTemplate.transform);

            if (!insideTokenContainer &&
                !insideTokenTemplate &&
                renderer.enabled)
            {
                errors.Add(
                    "A SegmentInventoryTray backdrop SpriteRenderer remains " +
                    "visible.");
            }
        }

        if (context.InventoryDropZone == null ||
            context.InventoryTray == null ||
            (context.InventoryDropZone.transform !=
                context.InventoryTray.transform &&
             !context.InventoryDropZone.transform.IsChildOf(
                 context.InventoryTray.transform)))
        {
            errors.Add(
                "InventoryDropZone is no longer on the tray GameObject or " +
                "inside the SegmentInventoryTray hierarchy.");
        }

        Collider2D[] currentColliders =
            context.InventoryDropZone != null
                ? context.InventoryDropZone
                    .GetComponentsInChildren<Collider2D>(true)
                : Array.Empty<Collider2D>();

        if (currentColliders.Length !=
            context.DropZoneColliders.Length)
        {
            errors.Add(
                "InventoryDropZone Collider2D components were not " +
                "preserved.");
            return;
        }

        for (int i = 0; i < currentColliders.Length; i++)
        {
            if (currentColliders[i] != context.DropZoneColliders[i] ||
                currentColliders[i].enabled !=
                    context.DropZoneColliderEnabledStates[i])
            {
                errors.Add(
                    "InventoryDropZone Collider2D state changed.");
            }
        }
    }

    private static void ValidateTokenVisualCleanup(
        TutorialContext context,
        List<string> errors)
    {
        SpriteRenderer primary = context.PrimarySegmentRenderer;

        if (primary == null ||
            primary.sprite != context.PrimarySprite ||
            primary.sharedMaterial != context.PrimarySharedMaterial ||
            primary.sortingLayerID != context.PrimarySortingLayerId ||
            primary.sortingOrder != context.PrimarySortingOrder ||
            primary.color != context.PrimaryColor ||
            !primary.enabled ||
            primary.enabled != context.PrimaryEnabled ||
            primary.transform.localPosition !=
                context.PrimaryLocalPosition ||
            primary.transform.localRotation !=
                context.PrimaryLocalRotation ||
            primary.transform.localScale != context.PrimaryLocalScale)
        {
            errors.Add(
                "The primary token segment renderer was not preserved.");
        }

        foreach (SpriteRenderer renderer in
            context.TokenTemplateRenderers)
        {
            if (renderer != primary && renderer.enabled)
            {
                errors.Add(
                    "A non-primary token template SpriteRenderer remains " +
                    "visible.");
            }
        }
    }

    private static void ConfigureStatusText(
        TMP_Text text,
        StatusTextLayout layout)
    {
        ConfigureText(
            text.GetComponent<RectTransform>(),
            text,
            layout.Position,
            layout.Size,
            layout.FontSize,
            HorizontalAlignmentOptions.Left,
            setVerticalMiddle: true,
            content: layout.Text,
            setOverflow: true);
        Undo.RecordObject(text, UndoName);
        text.fontStyle = layout.FontStyle;
        text.color = layout.Color;
        text.raycastTarget = false;
        SetActiveWithUndo(text.gameObject, true);
    }

    private static void ConfigureText(
        RectTransform rectTransform,
        TMP_Text text,
        Vector2 position,
        Vector2 size,
        float fontSize,
        HorizontalAlignmentOptions horizontalAlignment,
        bool setVerticalMiddle,
        string content,
        bool setOverflow)
    {
        Undo.RecordObject(rectTransform, UndoName);
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        Undo.RecordObject(text, UndoName);
        text.fontSize = fontSize;
        text.horizontalAlignment = horizontalAlignment;

        if (setVerticalMiddle)
        {
            text.verticalAlignment = VerticalAlignmentOptions.Middle;
        }

        text.raycastTarget = false;
        text.enableAutoSizing = false;
        text.richText = true;

        if (setOverflow)
        {
            text.overflowMode = TextOverflowModes.Overflow;
        }

        text.text = content;
    }

    private static void CapturePrimaryRendererState(
        TutorialContext context)
    {
        SpriteRenderer primary = context.PrimarySegmentRenderer;
        context.PrimarySprite = primary.sprite;
        context.PrimarySharedMaterial = primary.sharedMaterial;
        context.PrimaryColor = primary.color;
        context.PrimarySortingLayerId = primary.sortingLayerID;
        context.PrimarySortingOrder = primary.sortingOrder;
        context.PrimaryEnabled = primary.enabled;
        context.PrimaryLocalPosition =
            primary.transform.localPosition;
        context.PrimaryLocalRotation =
            primary.transform.localRotation;
        context.PrimaryLocalScale = primary.transform.localScale;
    }

    private static void ValidateSerializedHudReference(
        TMP_Text serializedReference,
        GameObject namedObject,
        string propertyName,
        List<string> errors)
    {
        if (serializedReference == null ||
            namedObject == null ||
            serializedReference.gameObject != namedObject)
        {
            errors.Add(
                $"CodebreakerEquationHUD.{propertyName} must reference the " +
                $"unique {namedObject?.name ?? "required scene object"}.");
        }
    }

    private static TMP_Text ReadTextReference(
        Object owner,
        string propertyName,
        List<string> errors)
    {
        SerializedObject serializedObject = new SerializedObject(owner);
        serializedObject.Update();
        SerializedProperty property =
            serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            errors.Add(
                $"{owner.GetType().Name} has no serialized property " +
                $"{propertyName}.");
            return null;
        }

        if (property.propertyType !=
            SerializedPropertyType.ObjectReference)
        {
            errors.Add(
                $"{owner.GetType().Name}.{propertyName} is not an object " +
                "reference.");
            return null;
        }

        TMP_Text text = property.objectReferenceValue as TMP_Text;

        if (text == null)
        {
            errors.Add(
                $"{owner.GetType().Name}.{propertyName} must reference a " +
                "TMP_Text.");
        }

        return text;
    }

    private static Color ReadColorValue(
        Object owner,
        string propertyName,
        List<string> errors)
    {
        SerializedObject serializedObject = new SerializedObject(owner);
        serializedObject.Update();
        SerializedProperty property =
            serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            errors.Add(
                $"{owner.GetType().Name} has no serialized property " +
                $"{propertyName}.");
            return default;
        }

        if (property.propertyType != SerializedPropertyType.Color)
        {
            errors.Add(
                $"{owner.GetType().Name}.{propertyName} is not a color.");
            return default;
        }

        return property.colorValue;
    }

    private static T ReadObjectReference<T>(
        Object owner,
        string propertyName,
        List<string> errors)
        where T : Object
    {
        SerializedObject serializedObject = new SerializedObject(owner);
        serializedObject.Update();
        SerializedProperty property =
            serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            errors.Add(
                $"{owner.GetType().Name} has no serialized property " +
                $"{propertyName}.");
            return null;
        }

        if (property.propertyType !=
            SerializedPropertyType.ObjectReference)
        {
            errors.Add(
                $"{owner.GetType().Name}.{propertyName} is not an object " +
                "reference.");
            return null;
        }

        T reference = property.objectReferenceValue as T;

        if (reference == null)
        {
            errors.Add(
                $"{owner.GetType().Name}.{propertyName} must reference a " +
                $"{typeof(T).Name}.");
        }

        return reference;
    }

    private static void ValidateTextRect(
        TMP_Text text,
        string label,
        List<string> errors)
    {
        if (text != null && text.GetComponent<RectTransform>() == null)
        {
            errors.Add($"{label} is missing a RectTransform.");
        }
    }

    private static void ValidateOptionalTextObject(
        GameObject textObject,
        GameObject equationRoot,
        string objectName,
        List<string> errors)
    {
        if (textObject == null)
        {
            return;
        }

        RequireComponent<RectTransform>(
            textObject,
            objectName,
            errors);
        RequireComponent<TMP_Text>(
            textObject,
            objectName,
            errors);
        RequireComponent<CanvasRenderer>(
            textObject,
            objectName,
            errors);

        if (equationRoot != null &&
            textObject.transform.parent != equationRoot.transform)
        {
            errors.Add(
                $"{objectName} must be a direct child of " +
                "EquationEntryRoot.");
        }
    }

    private static void ValidateStatusPanelBeforeRepair(
        GameObject panel,
        GameObject equationRoot,
        Scene targetScene,
        List<string> errors)
    {
        if (panel == null)
        {
            return;
        }

        if (panel.scene != targetScene)
        {
            errors.Add(
                $"{EquationStatusPanelName} must belong to " +
                $"{TargetScenePath}.");
        }

        if (equationRoot != null &&
            (panel.transform == equationRoot.transform ||
             !panel.transform.IsChildOf(equationRoot.transform)))
        {
            errors.Add(
                $"{EquationStatusPanelName} must belong to " +
                "EquationEntryRoot.");
        }

        if (panel.GetComponents<RectTransform>().Length != 1)
        {
            errors.Add(
                $"{EquationStatusPanelName} must have exactly one " +
                "RectTransform before repair.");
        }

        if (panel.GetComponents<TMP_Text>().Length > 0)
        {
            errors.Add(
                $"{EquationStatusPanelName} must not contain TMP_Text.");
        }

        if (panel.GetComponents<Button>().Length > 0 ||
            panel.GetComponents<Selectable>().Length > 0)
        {
            errors.Add(
                $"{EquationStatusPanelName} must not contain a Button or " +
                "Selectable.");
        }

        if (panel
                .GetComponents<UnityEngine.EventSystems.EventTrigger>()
                .Length > 0)
        {
            errors.Add(
                $"{EquationStatusPanelName} must not contain an " +
                "EventTrigger.");
        }

        if (panel.GetComponents<Collider2D>().Length > 0 ||
            panel.GetComponents<Rigidbody2D>().Length > 0)
        {
            errors.Add(
                $"{EquationStatusPanelName} must not contain 2D physics " +
                "components.");
        }

        foreach (MonoBehaviour behaviour in
            panel.GetComponents<MonoBehaviour>())
        {
            if (behaviour == null || !(behaviour is Image))
            {
                errors.Add(
                    $"{EquationStatusPanelName} must not contain custom " +
                    "MonoBehaviours.");
                break;
            }
        }

        foreach (Component component in panel.GetComponents<Component>())
        {
            if (component == null ||
                (!(component is RectTransform) &&
                 !(component is CanvasRenderer) &&
                 !(component is Image)))
            {
                errors.Add(
                    $"{EquationStatusPanelName} must contain only its " +
                    "RectTransform, CanvasRenderer, and Image.");
                break;
            }
        }
    }

    private static void ValidateBufferSlotObjects(
        TutorialContext context,
        List<string> errors)
    {
        List<GameObject> slotSegmentVisuals = new List<GameObject>();

        foreach (GameObject gameObject in
            GetSceneGameObjects(context.TargetScene))
        {
            if (gameObject.name.StartsWith(
                    "BufferSlotVisual_",
                    StringComparison.Ordinal) &&
                gameObject.name != "BufferSlotVisual_01" &&
                gameObject.name != "BufferSlotVisual_02")
            {
                errors.Add(
                    $"Unexpected Buffer slot visual named " +
                    $"{gameObject.name} exists at " +
                    $"{GetHierarchyPath(gameObject)}.");
            }

            if (gameObject.name == "SlotSegmentVisual")
            {
                slotSegmentVisuals.Add(gameObject);
            }
        }

        if (context.BufferCapacitySlotsRoot != null &&
            context.TokenContainer != null &&
            context.BufferCapacitySlotsRoot.transform.parent !=
                context.TokenContainer)
        {
            errors.Add(
                "BufferCapacitySlotsRoot must be a direct child of the " +
                "SegmentInventoryTray tokenContainer.");
        }

        ValidateOptionalSlot(
            context.BufferSlotVisual01,
            context.BufferCapacitySlotsRoot,
            "BufferSlotVisual_01",
            errors);
        ValidateOptionalSlot(
            context.BufferSlotVisual02,
            context.BufferCapacitySlotsRoot,
            "BufferSlotVisual_02",
            errors);

        foreach (GameObject slotSegmentVisual in slotSegmentVisuals)
        {
            Transform parent = slotSegmentVisual.transform.parent;

            if (parent == null ||
                (parent.gameObject != context.BufferSlotVisual01 &&
                 parent.gameObject != context.BufferSlotVisual02))
            {
                errors.Add(
                    "SlotSegmentVisual must be a direct child of " +
                    "BufferSlotVisual_01 or BufferSlotVisual_02; found at " +
                    $"{GetHierarchyPath(slotSegmentVisual)}.");
            }
        }
    }

    private static void ValidateOptionalSlot(
        GameObject slotObject,
        GameObject slotsRoot,
        string objectName,
        List<string> errors)
    {
        if (slotObject == null)
        {
            return;
        }

        if (slotsRoot == null ||
            slotObject.transform.parent != slotsRoot.transform)
        {
            errors.Add(
                $"{objectName} must be a direct child of " +
                "BufferCapacitySlotsRoot.");
        }

        int slotSegmentVisualCount = 0;

        foreach (Transform child in slotObject.transform)
        {
            if (child.name == "SlotSegmentVisual")
            {
                slotSegmentVisualCount++;
            }
        }

        if (slotSegmentVisualCount > 1)
        {
            errors.Add(
                $"{objectName} contains {slotSegmentVisualCount} direct " +
                "children named SlotSegmentVisual; expected at most one " +
                "before repair.");
        }
    }

    private static void ValidateSlotVisual(
        GameObject slotObject,
        SpriteRenderer primaryRenderer,
        string objectName,
        List<string> errors)
    {
        Vector3 expectedPosition =
            objectName == "BufferSlotVisual_01"
                ? new Vector3(-0.9f, 0.1f, 0f)
                : new Vector3(0.9f, 0.1f, 0f);

        if (slotObject.transform.localPosition != expectedPosition ||
            slotObject.transform.localRotation != Quaternion.identity ||
            slotObject.transform.localScale != Vector3.one ||
            !slotObject.activeSelf)
        {
            errors.Add($"{objectName} transform or active state is invalid.");
        }

        Transform slotSegmentVisual = null;
        int directChildCount = 0;

        foreach (Transform child in slotObject.transform)
        {
            directChildCount++;

            if (child.name == "SlotSegmentVisual")
            {
                slotSegmentVisual = child;
            }
        }

        if (directChildCount != 1 || slotSegmentVisual == null)
        {
            errors.Add(
                $"{objectName} must contain exactly one direct visual child " +
                "named SlotSegmentVisual.");
            return;
        }

        if (slotObject
                .GetComponentsInChildren<MonoBehaviour>(true).Length > 0 ||
            slotObject
                .GetComponentsInChildren<Collider2D>(true).Length > 0 ||
            slotObject
                .GetComponentsInChildren<Rigidbody2D>(true).Length > 0)
        {
            errors.Add(
                $"{objectName} contains a prohibited behaviour or physics " +
                "component.");
        }

        SpriteRenderer[] slotRenderers =
            slotObject.GetComponentsInChildren<SpriteRenderer>(true);

        if (slotRenderers.Length != 1)
        {
            errors.Add(
                $"{objectName} must contain exactly one SpriteRenderer.");
            return;
        }

        SpriteRenderer slotRenderer = slotRenderers[0];
        Color expectedColor =
            new Color(0.55f, 0.72f, 0.78f, 0.18f);

        if (slotSegmentVisual.localPosition != Vector3.zero ||
            slotSegmentVisual.localRotation != Quaternion.identity ||
            slotSegmentVisual.localScale !=
                new Vector3(0.9f, 0.18f, 1f) ||
            slotRenderer.sprite != primaryRenderer.sprite ||
            slotRenderer.sharedMaterial !=
                primaryRenderer.sharedMaterial ||
            slotRenderer.sortingLayerID !=
                primaryRenderer.sortingLayerID ||
            slotRenderer.sortingOrder !=
                primaryRenderer.sortingOrder - 1 ||
            slotRenderer.color != expectedColor ||
            !slotRenderer.enabled ||
            !slotSegmentVisual.gameObject.activeSelf)
        {
            errors.Add(
                $"{objectName} SlotSegmentVisual is not a valid neutral " +
                "capacity silhouette.");
        }
    }

    private static void ValidateTrayLayout(
        SegmentInventoryTray inventoryTray,
        List<string> errors)
    {
        SerializedObject serializedTray =
            new SerializedObject(inventoryTray);
        serializedTray.Update();

        if (RequireSerializedProperty(
                serializedTray,
                "maximumVisibleTokens").intValue != 2 ||
            RequireSerializedProperty(
                serializedTray,
                "columns").intValue != 2 ||
            RequireSerializedProperty(
                serializedTray,
                "firstTokenLocalPosition").vector2Value !=
                new Vector2(-0.9f, 0.1f) ||
            RequireSerializedProperty(
                serializedTray,
                "tokenSpacing").vector2Value !=
                new Vector2(1.8f, 0f))
        {
            errors.Add(
                "SegmentInventoryTray tutorial token layout is invalid.");
        }
    }

    private static void ValidateObjectReference(
        SerializedObject serializedObject,
        string propertyName,
        Object expectedReference,
        List<string> errors)
    {
        SerializedProperty property =
            serializedObject.FindProperty(propertyName);

        if (property == null ||
            property.objectReferenceValue != expectedReference)
        {
            errors.Add(
                $"{serializedObject.targetObject.GetType().Name}." +
                $"{propertyName} does not reference the expected object.");
        }
    }

    private static void RequireExactNamedCount(
        Scene scene,
        string objectName,
        int expectedCount,
        List<string> errors)
    {
        int actualCount = FindAllNamed(scene, objectName).Count;

        if (actualCount != expectedCount)
        {
            errors.Add(
                $"{scene.path} contains {actualCount} objects named " +
                $"{objectName}; expected {expectedCount}.");
        }
    }

    private static GameObject FindOptionalUniqueNamed(
        Scene scene,
        string objectName,
        List<string> errors)
    {
        List<GameObject> matches = FindAllNamed(scene, objectName);

        if (matches.Count > 1)
        {
            errors.Add(
                $"{scene.path} contains {matches.Count} objects named " +
                $"{objectName}; expected at most one before repair.");
            return null;
        }

        return matches.Count == 1 ? matches[0] : null;
    }

    private static GameObject FindUniqueNamed(
        Scene scene,
        string objectName,
        List<string> errors)
    {
        List<GameObject> matches = FindAllNamed(scene, objectName);

        if (matches.Count != 1)
        {
            errors.Add(
                $"{scene.path} must contain exactly one {objectName}; " +
                $"found {matches.Count}.");
            return null;
        }

        return matches[0];
    }

    private static List<GameObject> FindAllNamed(
        Scene scene,
        string objectName)
    {
        List<GameObject> matches = new List<GameObject>();

        foreach (GameObject gameObject in GetSceneGameObjects(scene))
        {
            if (gameObject.name == objectName)
            {
                matches.Add(gameObject);
            }
        }

        return matches;
    }

    private static GameObject FindRemovableLabel(
        Scene scene,
        GameObject equationRoot,
        string objectName,
        List<string> errors)
    {
        List<GameObject> matches = FindAllNamed(scene, objectName);

        if (matches.Count > 1)
        {
            errors.Add(
                $"{scene.path} contains {matches.Count} objects named " +
                $"{objectName}; expected at most one before removal.");
            return null;
        }

        if (matches.Count == 0)
        {
            return null;
        }

        GameObject match = matches[0];

        if (!match.transform.IsChildOf(equationRoot.transform))
        {
            errors.Add(
                $"{objectName} must be inside EquationEntryRoot before it " +
                $"can be removed safely; found at {GetHierarchyPath(match)}.");
            return null;
        }

        return match;
    }

    private static List<GameObject> GetSceneGameObjects(Scene scene)
    {
        List<GameObject> gameObjects = new List<GameObject>();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform transform in
                root.GetComponentsInChildren<Transform>(true))
            {
                gameObjects.Add(transform.gameObject);
            }
        }

        return gameObjects;
    }

    private static T RequireUniqueComponent<T>(
        Scene scene,
        List<string> errors)
        where T : Component
    {
        List<T> components = new List<T>();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            components.AddRange(root.GetComponentsInChildren<T>(true));
        }

        if (components.Count != 1)
        {
            errors.Add(
                $"{scene.path} must contain exactly one {typeof(T).Name}; " +
                $"found {components.Count}.");
            return null;
        }

        return components[0];
    }

    private static T RequireComponent<T>(
        GameObject gameObject,
        string objectLabel,
        List<string> errors)
        where T : Component
    {
        if (gameObject == null)
        {
            return null;
        }

        T component = gameObject.GetComponent<T>();

        if (component == null)
        {
            errors.Add($"{objectLabel} is missing {typeof(T).Name}.");
        }

        return component;
    }

    private static void ValidateDescendant(
        GameObject child,
        GameObject parent,
        string childLabel,
        string parentLabel,
        List<string> errors)
    {
        if (child == null || parent == null)
        {
            return;
        }

        if (child.transform == parent.transform ||
            !child.transform.IsChildOf(parent.transform))
        {
            errors.Add(
                $"{childLabel} must be a descendant of {parentLabel}; found " +
                $"at {GetHierarchyPath(child)}.");
        }
    }

    private static void ValidateTrayDropZonePlacement(
        InventoryDropZone inventoryDropZone,
        SegmentInventoryTray inventoryTray,
        List<string> errors)
    {
        if (inventoryDropZone == null || inventoryTray == null)
        {
            return;
        }

        Transform dropZoneTransform = inventoryDropZone.transform;
        Transform trayTransform = inventoryTray.transform;

        if (dropZoneTransform == trayTransform ||
            dropZoneTransform.IsChildOf(trayTransform))
        {
            return;
        }

        errors.Add(
            "InventoryDropZone must be on the SegmentInventoryTray " +
            "GameObject or inside its hierarchy; found at " +
            $"{GetHierarchyPath(inventoryDropZone.gameObject)}.");
    }

    private static string GetHierarchyPath(GameObject gameObject)
    {
        string path = gameObject.name;
        Transform parent = gameObject.transform.parent;

        while (parent != null)
        {
            path = $"{parent.name}/{path}";
            parent = parent.parent;
        }

        return path;
    }

    private static bool TryGetReleaseMenuTargetScene(
        out Scene targetScene,
        out string refusal)
    {
        targetScene = SceneManager.GetSceneByPath(TargetScenePath);

        if (EditorApplication.isPlaying)
        {
            refusal =
                "Release menus cannot be built while Unity is in Play Mode.";
            return false;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            refusal =
                "Release menus cannot be built while Unity is entering " +
                "Play Mode.";
            return false;
        }

        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            refusal =
                $"Load {TargetScenePath} before building release menus.";
            return false;
        }

        if (SceneManager.GetActiveScene() != targetScene)
        {
            refusal =
                $"{TargetScenePath} must be the active scene before " +
                "building release menus.";
            return false;
        }

        if (targetScene.isDirty)
        {
            refusal =
                "The target scene has unsaved changes. Save or discard " +
                "them before building release menus.";
            return false;
        }

        refusal = null;
        return true;
    }

    private static ReleaseMenuContext ValidateReleaseMenuScene(
        Scene targetScene,
        List<string> errors)
    {
        ReleaseMenuContext context = new ReleaseMenuContext
        {
            TargetScene = targetScene,
            GameController =
                RequireUniqueComponent<CodebreakerGameController>(
                    targetScene,
                    errors),
            GlobalTimer = RequireUniqueComponent<GlobalBombTimer>(
                targetScene,
                errors),
            EquationInteraction =
                RequireUniqueComponent<
                    CodebreakerEquationInteractionController>(
                        targetScene,
                        errors),
            Hud = RequireUniqueComponent<CodebreakerHUD>(
                targetScene,
                errors),
            EventSystem = RequireUniqueComponent<EventSystem>(
                targetScene,
                errors),
            HudCanvas = FindUniqueNamed(
                targetScene,
                "CodebreakerHUDCanvas",
                errors),
            BombBackground = FindUniqueNamed(
                targetScene,
                BombBackgroundObjectName,
                errors),
            ReleaseCanvas = FindOptionalUniqueNamed(
                targetScene,
                ReleaseMenuCanvasName,
                errors)
        };

        Camera mainCamera = RequireUniqueComponent<Camera>(
            targetScene,
            errors);
        GameObject equationWorldRoot = FindOptionalUniqueNamed(
            targetScene,
            "EquationEntryWorldRoot",
            errors);
        GameObject equationStatusPanel = FindOptionalUniqueNamed(
            targetScene,
            EquationStatusPanelName,
            errors);
        GameObject displayA = FindOptionalUniqueNamed(
            targetScene,
            "Display_A",
            errors);
        GameObject displayB = FindOptionalUniqueNamed(
            targetScene,
            "Display_B",
            errors);
        GameObject codeDiscoveryRoot = ReadGameObjectReference(
            context.GameController,
            "codeDiscoveryRoot",
            errors);
        GameObject equationEntryRoot = ReadGameObjectReference(
            context.GameController,
            "equationEntryRoot",
            errors);
        List<SegmentInventoryTray> inventoryTrays =
            GetSceneComponents<SegmentInventoryTray>(targetScene);

        if (inventoryTrays.Count > 1)
        {
            errors.Add(
                $"{targetScene.path} contains {inventoryTrays.Count} " +
                "SegmentInventoryTray components; Buffer preservation " +
                "requires at most one.");
        }

        ValidateReleaseMenuEventSystem(context, errors);
        ValidateReleaseMenuFontSource(context, errors);
        ValidateExistingReleaseMenuObjects(context, errors);

        if (mainCamera != null &&
            mainCamera.gameObject.name != "Main Camera")
        {
            errors.Add(
                "The unique scene Camera must be named Main Camera.");
        }

        if (context.GameController != null &&
            context.GlobalTimer != null &&
            context.GameController.GlobalTimer != context.GlobalTimer)
        {
            errors.Add(
                "CodebreakerGameController and GlobalBombTimer do not " +
                "reference the same timer.");
        }

        if (context.HudCanvas != null &&
            context.HudCanvas.GetComponent<Canvas>() == null)
        {
            errors.Add("CodebreakerHUDCanvas is missing Canvas.");
        }

        if (context.ReleaseCanvas != null &&
            context.ReleaseCanvas.transform.parent != null)
        {
            errors.Add(
                "ReleaseMenuCanvas must be a root object before repair.");
        }

        if (errors.Count == 0)
        {
            context.PreservedStates.Add(
                new PreservedHierarchyState(
                    context.BombBackground,
                    "CodebreakerBombBackground"));
            context.PreservedStates.Add(
                new PreservedHierarchyState(
                    context.HudCanvas,
                    "CodebreakerHUDCanvas hierarchy"));
            context.PreservedStates.Add(
                new PreservedHierarchyState(
                    mainCamera.gameObject,
                    "Main Camera"));
            context.PreservedStates.Add(
                new PreservedHierarchyState(
                    context.EventSystem.gameObject,
                    "EventSystem and InputSystemUIInputModule"));

            if (equationWorldRoot != null &&
                equationWorldRoot != context.HudCanvas)
            {
                context.PreservedStates.Add(
                    new PreservedHierarchyState(
                        equationWorldRoot,
                        "EquationEntryWorldRoot"));
            }

            if (equationStatusPanel != null &&
                equationStatusPanel != context.HudCanvas)
            {
                context.PreservedStates.Add(
                    new PreservedHierarchyState(
                        equationStatusPanel,
                        EquationStatusPanelName));
            }

            AddPreservedState(
                context,
                codeDiscoveryRoot,
                "Code discovery hierarchy");
            AddPreservedState(
                context,
                equationEntryRoot,
                "Equation entry hierarchy");
            AddPreservedState(
                context,
                displayA,
                "central Equation display A");
            AddPreservedState(
                context,
                displayB,
                "central Equation display B");

            if (inventoryTrays.Count == 1)
            {
                AddPreservedState(
                    context,
                    inventoryTrays[0].gameObject,
                    "Buffer presentation");
            }
        }

        return context;
    }

    private static GameObject ReadGameObjectReference(
        Component component,
        string propertyName,
        List<string> errors)
    {
        if (component == null)
        {
            return null;
        }

        SerializedObject serializedObject =
            new SerializedObject(component);
        serializedObject.Update();
        SerializedProperty property =
            serializedObject.FindProperty(propertyName);
        GameObject value =
            property?.objectReferenceValue as GameObject;

        if (property == null || value == null)
        {
            errors.Add(
                $"{component.GetType().Name}.{propertyName} must reference " +
                "a scene GameObject.");
        }

        return value;
    }

    private static void AddPreservedState(
        ReleaseMenuContext context,
        GameObject gameObject,
        string label)
    {
        if (gameObject != null)
        {
            context.PreservedStates.Add(
                new PreservedHierarchyState(gameObject, label));
        }
    }

    private static void ValidateReleaseMenuEventSystem(
        ReleaseMenuContext context,
        List<string> errors)
    {
        if (context.EventSystem == null)
        {
            return;
        }

        InputSystemUIInputModule[] inputModules =
            context.EventSystem.GetComponents<InputSystemUIInputModule>();

        if (inputModules.Length != 1)
        {
            errors.Add(
                "The existing EventSystem must contain exactly one " +
                "InputSystemUIInputModule.");
        }

        if (GetSceneComponents<StandaloneInputModule>(
                context.TargetScene).Count > 0)
        {
            errors.Add(
                "The existing EventSystem contains a prohibited legacy " +
                "StandaloneInputModule.");
        }
    }

    private static void ValidateReleaseMenuFontSource(
        ReleaseMenuContext context,
        List<string> errors)
    {
        if (context.Hud == null)
        {
            return;
        }

        SerializedObject serializedHud = new SerializedObject(context.Hud);
        serializedHud.Update();
        SerializedProperty timerProperty =
            serializedHud.FindProperty("timerText");
        SerializedProperty phaseProperty =
            serializedHud.FindProperty("phaseText");
        TMP_Text sourceText =
            timerProperty?.objectReferenceValue as TMP_Text ??
            phaseProperty?.objectReferenceValue as TMP_Text;

        if (sourceText == null)
        {
            errors.Add(
                "CodebreakerHUD timerText or phaseText must provide the " +
                "release menu font.");
            return;
        }

        if (sourceText.font == null)
        {
            errors.Add(
                "The release menu source TMP text has no font asset.");
        }

        if (sourceText.fontSharedMaterial == null)
        {
            errors.Add(
                "The release menu source TMP text has no shared material.");
        }

        context.FontAsset = sourceText.font;
        context.FontMaterial = sourceText.fontSharedMaterial;
    }

    private static void ValidateExistingReleaseMenuObjects(
        ReleaseMenuContext context,
        List<string> errors)
    {
        string[] uniqueNames =
        {
            ReleaseMenuCanvasName,
            "MainMenuRoot",
            "MainMenuDim",
            "MainMenuPanel",
            "MainTitleText",
            "MainSubtitleText",
            "MainTaglineText",
            "PlayButton",
            "MainQuitButton",
            "PauseMenuRoot",
            "PauseMenuDim",
            "PauseMenuPanel",
            "PauseTitleText",
            "PauseHintText",
            "ResumeButton",
            "RetryButton",
            "PauseQuitButton"
        };

        foreach (string objectName in uniqueNames)
        {
            List<GameObject> matches =
                FindAllNamed(context.TargetScene, objectName);

            if (matches.Count > 1)
            {
                errors.Add(
                    $"{context.TargetScene.path} contains {matches.Count} " +
                    $"objects named {objectName}; duplicate release menu " +
                    "objects cannot be repaired safely.");
                continue;
            }

            if (matches.Count == 1 &&
                !(matches[0].transform is RectTransform))
            {
                errors.Add(
                    $"{GetHierarchyPath(matches[0])} must use a " +
                    "RectTransform.");
            }

            if (matches.Count == 1)
            {
                foreach (Component component in
                    matches[0].GetComponents<Component>())
                {
                    if (component == null)
                    {
                        errors.Add(
                            $"{GetHierarchyPath(matches[0])} contains a " +
                            "missing script component.");
                        break;
                    }
                }
            }
        }

        List<CodebreakerMenuController> controllers =
            GetSceneComponents<CodebreakerMenuController>(
                context.TargetScene);

        if (controllers.Count > 1)
        {
            errors.Add(
                $"{context.TargetScene.path} contains {controllers.Count} " +
                "CodebreakerMenuController components; expected at most " +
                "one before repair.");
        }
        else if (controllers.Count == 1 &&
                 (context.ReleaseCanvas == null ||
                  controllers[0].gameObject != context.ReleaseCanvas))
        {
            errors.Add(
                "The existing CodebreakerMenuController must belong to " +
                "ReleaseMenuCanvas.");
        }

        string[] buttonNames =
        {
            "PlayButton",
            "MainQuitButton",
            "ResumeButton",
            "RetryButton",
            "PauseQuitButton"
        };

        foreach (string buttonName in buttonNames)
        {
            List<GameObject> matches =
                FindAllNamed(context.TargetScene, buttonName);

            if (matches.Count != 1)
            {
                continue;
            }

            int labelCount = 0;

            foreach (Transform child in matches[0].transform)
            {
                if (child.name == "Label")
                {
                    labelCount++;
                }
            }

            if (labelCount > 1)
            {
                errors.Add(
                    $"{GetHierarchyPath(matches[0])} contains duplicate " +
                    "direct Label children.");
            }
        }
    }

    private static void ApplyReleaseMenus(ReleaseMenuContext context)
    {
        GameObject canvasObject = GetOrCreateReleaseMenuObject(
            context.TargetScene,
            ReleaseMenuCanvasName,
            null);
        context.ReleaseCanvas = canvasObject;
        EnsureExactComponents(
            canvasObject,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(CodebreakerMenuController));
        ConfigureReleaseCanvas(canvasObject);

        GameObject mainRoot = GetOrCreateReleaseMenuObject(
            context.TargetScene,
            "MainMenuRoot",
            canvasObject.transform);
        GameObject pauseRoot = GetOrCreateReleaseMenuObject(
            context.TargetScene,
            "PauseMenuRoot",
            canvasObject.transform);
        EnsureExactComponents(mainRoot, typeof(RectTransform));
        EnsureExactComponents(pauseRoot, typeof(RectTransform));
        ConfigureFullStretch(mainRoot.GetComponent<RectTransform>());
        ConfigureFullStretch(pauseRoot.GetComponent<RectTransform>());

        GameObject mainDim = CreateOrRepairReleaseImage(
            context,
            "MainMenuDim",
            mainRoot.transform,
            new Color(0.005f, 0.012f, 0.02f, 0.78f),
            true,
            true,
            Vector2.zero,
            Vector2.zero);
        GameObject mainPanel = CreateOrRepairReleaseImage(
            context,
            "MainMenuPanel",
            mainRoot.transform,
            new Color(0.012f, 0.035f, 0.055f, 0.94f),
            true,
            false,
            Vector2.zero,
            new Vector2(720f, 580f));

        TMP_Text mainTitle = CreateOrRepairReleaseText(
            context,
            "MainTitleText",
            mainPanel.transform,
            "COUNT DOWN",
            new Vector2(0f, 190f),
            new Vector2(620f, 100f),
            72f,
            FontStyles.Bold,
            new Color(1f, 0.78f, 0.20f, 1f));
        TMP_Text mainSubtitle = CreateOrRepairReleaseText(
            context,
            "MainSubtitleText",
            mainPanel.transform,
            "CODEBREAKER PROTOCOL",
            new Vector2(0f, 105f),
            new Vector2(620f, 54f),
            30f,
            FontStyles.Bold,
            new Color(0.30f, 0.88f, 1f, 1f));
        TMP_Text mainTagline = CreateOrRepairReleaseText(
            context,
            "MainTaglineText",
            mainPanel.transform,
            "RECOVER THE CODE. DEFUSE THE DEVICE.",
            new Vector2(0f, 52f),
            new Vector2(620f, 38f),
            18f,
            FontStyles.Normal,
            new Color(0.65f, 0.76f, 0.82f, 1f));

        context.PlayButton = CreateOrRepairReleaseButton(
            context,
            "PlayButton",
            mainPanel.transform,
            "PLAY",
            new Vector2(0f, -80f),
            new Vector2(360f, 76f),
            new Color(0.04f, 0.32f, 0.38f, 0.98f));
        context.MainQuitButton = CreateOrRepairReleaseButton(
            context,
            "MainQuitButton",
            mainPanel.transform,
            "QUIT",
            new Vector2(0f, -180f),
            new Vector2(360f, 76f),
            new Color(0.38f, 0.08f, 0.07f, 0.98f));

        GameObject pauseDim = CreateOrRepairReleaseImage(
            context,
            "PauseMenuDim",
            pauseRoot.transform,
            new Color(0.005f, 0.012f, 0.02f, 0.72f),
            true,
            true,
            Vector2.zero,
            Vector2.zero);
        GameObject pausePanel = CreateOrRepairReleaseImage(
            context,
            "PauseMenuPanel",
            pauseRoot.transform,
            new Color(0.012f, 0.035f, 0.055f, 0.96f),
            true,
            false,
            Vector2.zero,
            new Vector2(640f, 580f));

        TMP_Text pauseTitle = CreateOrRepairReleaseText(
            context,
            "PauseTitleText",
            pausePanel.transform,
            "PAUSED",
            new Vector2(0f, 190f),
            new Vector2(560f, 90f),
            62f,
            FontStyles.Bold,
            new Color(1f, 0.78f, 0.20f, 1f));
        TMP_Text pauseHint = CreateOrRepairReleaseText(
            context,
            "PauseHintText",
            pausePanel.transform,
            "PRESS ESC TO RESUME",
            new Vector2(0f, 125f),
            new Vector2(560f, 38f),
            18f,
            FontStyles.Normal,
            new Color(0.65f, 0.76f, 0.82f, 1f));

        context.ResumeButton = CreateOrRepairReleaseButton(
            context,
            "ResumeButton",
            pausePanel.transform,
            "RESUME",
            new Vector2(0f, 35f),
            new Vector2(360f, 72f),
            new Color(0.04f, 0.32f, 0.38f, 0.98f));
        context.RetryButton = CreateOrRepairReleaseButton(
            context,
            "RetryButton",
            pausePanel.transform,
            "RETRY",
            new Vector2(0f, -65f),
            new Vector2(360f, 72f),
            new Color(0.42f, 0.29f, 0.06f, 0.98f));
        context.PauseQuitButton = CreateOrRepairReleaseButton(
            context,
            "PauseQuitButton",
            pausePanel.transform,
            "QUIT",
            new Vector2(0f, -165f),
            new Vector2(360f, 72f),
            new Color(0.38f, 0.08f, 0.07f, 0.98f));

        RemoveUnexpectedDirectChildren(
            canvasObject,
            mainRoot,
            pauseRoot);
        RemoveUnexpectedDirectChildren(mainRoot, mainDim, mainPanel);
        RemoveUnexpectedDirectChildren(
            mainPanel,
            mainTitle.gameObject,
            mainSubtitle.gameObject,
            mainTagline.gameObject,
            context.PlayButton.gameObject,
            context.MainQuitButton.gameObject);
        RemoveUnexpectedDirectChildren(pauseRoot, pauseDim, pausePanel);
        RemoveUnexpectedDirectChildren(
            pausePanel,
            pauseTitle.gameObject,
            pauseHint.gameObject,
            context.ResumeButton.gameObject,
            context.RetryButton.gameObject,
            context.PauseQuitButton.gameObject);
        SetReleaseSiblingOrder(canvasObject, mainRoot, pauseRoot);
        SetReleaseSiblingOrder(mainRoot, mainDim, mainPanel);
        SetReleaseSiblingOrder(
            mainPanel,
            mainTitle.gameObject,
            mainSubtitle.gameObject,
            mainTagline.gameObject,
            context.PlayButton.gameObject,
            context.MainQuitButton.gameObject);
        SetReleaseSiblingOrder(pauseRoot, pauseDim, pausePanel);
        SetReleaseSiblingOrder(
            pausePanel,
            pauseTitle.gameObject,
            pauseHint.gameObject,
            context.ResumeButton.gameObject,
            context.RetryButton.gameObject,
            context.PauseQuitButton.gameObject);

        context.MenuController =
            canvasObject.GetComponent<CodebreakerMenuController>();
        AssignReleaseMenuController(
            context,
            mainRoot,
            pauseRoot);
        WireReleaseMenuButtons(context);

        SetReleaseActiveWithUndo(canvasObject, true);
        SetReleaseActiveWithUndo(mainRoot, true);
        SetReleaseActiveWithUndo(pauseRoot, false);
    }

    private static GameObject GetOrCreateReleaseMenuObject(
        Scene scene,
        string objectName,
        Transform parent)
    {
        List<GameObject> matches = FindAllNamed(scene, objectName);
        GameObject gameObject;

        if (matches.Count == 0)
        {
            gameObject = new GameObject(
                objectName,
                typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(
                gameObject,
                ReleaseMenuUndoName);
            SceneManager.MoveGameObjectToScene(gameObject, scene);
        }
        else
        {
            gameObject = matches[0];
        }

        if (gameObject.transform.parent != parent)
        {
            Undo.SetTransformParent(
                gameObject.transform,
                parent,
                ReleaseMenuUndoName);
        }

        Undo.RecordObject(gameObject.transform, ReleaseMenuUndoName);
        gameObject.transform.localScale = Vector3.one;
        gameObject.transform.localRotation = Quaternion.identity;
        return gameObject;
    }

    private static void EnsureExactComponents(
        GameObject gameObject,
        params Type[] requiredTypes)
    {
        foreach (Type requiredType in requiredTypes)
        {
            if (gameObject.GetComponent(requiredType) == null)
            {
                Undo.AddComponent(gameObject, requiredType);
            }
        }

        Component[] components = gameObject.GetComponents<Component>();

        foreach (Component component in components)
        {
            bool allowed = false;

            foreach (Type requiredType in requiredTypes)
            {
                if (component != null &&
                    requiredType.IsInstanceOfType(component))
                {
                    allowed = true;
                    break;
                }
            }

            if (!allowed && component != null)
            {
                Undo.DestroyObjectImmediate(component);
            }
        }
    }

    private static void ConfigureReleaseCanvas(GameObject canvasObject)
    {
        RectTransform rectTransform =
            canvasObject.GetComponent<RectTransform>();
        ConfigureFullStretch(rectTransform);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        Undo.RecordObject(canvas, ReleaseMenuUndoName);
        canvas.enabled = true;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;
        canvas.overrideSorting = false;
        canvas.sortingOrder = 1000;
        canvas.targetDisplay = 0;
        canvas.worldCamera = null;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        Undo.RecordObject(scaler, ReleaseMenuUndoName);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = 100f;

        GraphicRaycaster raycaster =
            canvasObject.GetComponent<GraphicRaycaster>();
        Undo.RecordObject(raycaster, ReleaseMenuUndoName);
        raycaster.enabled = true;
        raycaster.ignoreReversedGraphics = true;
        raycaster.blockingObjects =
            GraphicRaycaster.BlockingObjects.None;
    }

    private static GameObject CreateOrRepairReleaseImage(
        ReleaseMenuContext context,
        string objectName,
        Transform parent,
        Color color,
        bool raycastTarget,
        bool fullStretch,
        Vector2 position,
        Vector2 size)
    {
        GameObject gameObject = GetOrCreateReleaseMenuObject(
            context.TargetScene,
            objectName,
            parent);
        EnsureExactComponents(
            gameObject,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        RectTransform rectTransform =
            gameObject.GetComponent<RectTransform>();

        if (fullStretch)
        {
            ConfigureFullStretch(rectTransform);
        }
        else
        {
            ConfigureCenteredRect(rectTransform, position, size);
        }

        Image image = gameObject.GetComponent<Image>();
        Undo.RecordObject(image, ReleaseMenuUndoName);
        image.enabled = true;
        image.color = color;
        image.raycastTarget = raycastTarget;
        image.sprite = null;
        image.overrideSprite = null;
        image.material = null;
        image.type = Image.Type.Simple;
        image.maskable = true;
        return gameObject;
    }

    private static TMP_Text CreateOrRepairReleaseText(
        ReleaseMenuContext context,
        string objectName,
        Transform parent,
        string content,
        Vector2 position,
        Vector2 size,
        float fontSize,
        FontStyles fontStyle,
        Color color)
    {
        GameObject gameObject = GetOrCreateReleaseMenuObject(
            context.TargetScene,
            objectName,
            parent);
        EnsureExactComponents(
            gameObject,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        ConfigureCenteredRect(
            gameObject.GetComponent<RectTransform>(),
            position,
            size);

        TMP_Text text = gameObject.GetComponent<TMP_Text>();
        Undo.RecordObject(text, ReleaseMenuUndoName);
        text.text = content;
        text.font = context.FontAsset;
        text.fontSharedMaterial = context.FontMaterial;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateOrRepairReleaseButton(
        ReleaseMenuContext context,
        string objectName,
        Transform parent,
        string labelContent,
        Vector2 position,
        Vector2 size,
        Color normalColor)
    {
        GameObject gameObject = GetOrCreateReleaseMenuObject(
            context.TargetScene,
            objectName,
            parent);
        EnsureExactComponents(
            gameObject,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));
        ConfigureCenteredRect(
            gameObject.GetComponent<RectTransform>(),
            position,
            size);

        Image image = gameObject.GetComponent<Image>();
        Undo.RecordObject(image, ReleaseMenuUndoName);
        image.enabled = true;
        image.sprite = null;
        image.overrideSprite = null;
        image.material = null;
        image.raycastTarget = true;
        image.type = Image.Type.Simple;
        image.maskable = true;
        image.color = Color.white;

        Button button = gameObject.GetComponent<Button>();
        Undo.RecordObject(button, ReleaseMenuUndoName);
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        Color highlightedColor =
            MultiplyRgbPreserveAlpha(normalColor, 1.18f);
        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = highlightedColor;
        colors.pressedColor =
            MultiplyRgbPreserveAlpha(normalColor, 0.78f);
        colors.selectedColor = highlightedColor;
        colors.disabledColor =
            new Color(0.16f, 0.18f, 0.20f, 0.75f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.Automatic;
        button.navigation = navigation;

        GameObject labelObject = GetOrCreateButtonLabel(
            context.TargetScene,
            gameObject);
        EnsureExactComponents(
            labelObject,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        ConfigureFullStretch(
            labelObject.GetComponent<RectTransform>());
        TMP_Text label = labelObject.GetComponent<TMP_Text>();
        Undo.RecordObject(label, ReleaseMenuUndoName);
        label.text = labelContent;
        label.font = context.FontAsset;
        label.fontSharedMaterial = context.FontMaterial;
        label.fontSize = 28f;
        label.fontStyle = FontStyles.Bold;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        RemoveUnexpectedDirectChildren(gameObject, labelObject);
        return button;
    }

    private static GameObject GetOrCreateButtonLabel(
        Scene scene,
        GameObject buttonObject)
    {
        GameObject labelObject = null;

        foreach (Transform child in buttonObject.transform)
        {
            if (child.name == "Label")
            {
                labelObject = child.gameObject;
                break;
            }
        }

        if (labelObject == null)
        {
            labelObject = new GameObject(
                "Label",
                typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(
                labelObject,
                ReleaseMenuUndoName);
            SceneManager.MoveGameObjectToScene(labelObject, scene);
            Undo.SetTransformParent(
                labelObject.transform,
                buttonObject.transform,
                ReleaseMenuUndoName);
        }

        return labelObject;
    }

    private static void ConfigureFullStretch(
        RectTransform rectTransform)
    {
        Undo.RecordObject(rectTransform, ReleaseMenuUndoName);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition3D = Vector3.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
    }

    private static void SetReleaseActiveWithUndo(
        GameObject gameObject,
        bool active)
    {
        if (gameObject.activeSelf == active)
        {
            return;
        }

        Undo.RecordObject(gameObject, ReleaseMenuUndoName);
        gameObject.SetActive(active);
    }

    private static void ConfigureCenteredRect(
        RectTransform rectTransform,
        Vector2 position,
        Vector2 size)
    {
        Undo.RecordObject(rectTransform, ReleaseMenuUndoName);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
    }

    private static Color MultiplyRgbPreserveAlpha(
        Color color,
        float multiplier)
    {
        return new Color(
            color.r * multiplier,
            color.g * multiplier,
            color.b * multiplier,
            color.a);
    }

    private static void RemoveUnexpectedDirectChildren(
        GameObject parent,
        params GameObject[] expectedChildren)
    {
        List<GameObject> unexpected = new List<GameObject>();

        foreach (Transform child in parent.transform)
        {
            bool expected = false;

            foreach (GameObject expectedChild in expectedChildren)
            {
                if (child.gameObject == expectedChild)
                {
                    expected = true;
                    break;
                }
            }

            if (!expected)
            {
                unexpected.Add(child.gameObject);
            }
        }

        foreach (GameObject child in unexpected)
        {
            Undo.DestroyObjectImmediate(child);
        }
    }

    private static void SetReleaseSiblingOrder(
        GameObject parent,
        params GameObject[] children)
    {
        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i].transform;

            if (child.GetSiblingIndex() != i)
            {
                Undo.RecordObject(child, ReleaseMenuUndoName);
                child.SetSiblingIndex(i);
            }
        }
    }

    private static void AssignReleaseMenuController(
        ReleaseMenuContext context,
        GameObject mainRoot,
        GameObject pauseRoot)
    {
        Undo.RecordObject(context.MenuController, ReleaseMenuUndoName);
        SerializedObject serializedController =
            new SerializedObject(context.MenuController);
        serializedController.Update();
        SetObjectReference(
            serializedController,
            "gameController",
            context.GameController);
        SetObjectReference(
            serializedController,
            "globalTimer",
            context.GlobalTimer);
        SetObjectReference(
            serializedController,
            "equationInteraction",
            context.EquationInteraction);
        SetObjectReference(
            serializedController,
            "gameplayHudRoot",
            context.HudCanvas);
        SetObjectReference(
            serializedController,
            "mainMenuRoot",
            mainRoot);
        SetObjectReference(
            serializedController,
            "pauseMenuRoot",
            pauseRoot);
        SetObjectReference(
            serializedController,
            "playButton",
            context.PlayButton);
        SetObjectReference(
            serializedController,
            "resumeButton",
            context.ResumeButton);
        SetObjectReference(
            serializedController,
            "retryButton",
            context.RetryButton);
        SetObjectReference(
            serializedController,
            "mainQuitButton",
            context.MainQuitButton);
        SetObjectReference(
            serializedController,
            "pauseQuitButton",
            context.PauseQuitButton);
        serializedController.ApplyModifiedProperties();
    }

    private static void SetObjectReference(
        SerializedObject serializedObject,
        string propertyName,
        Object value)
    {
        SerializedProperty property =
            serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            throw new InvalidOperationException(
                $"{serializedObject.targetObject.GetType().Name} is " +
                $"missing serialized field {propertyName}.");
        }

        property.objectReferenceValue = value;
    }

    private static void WireReleaseMenuButtons(
        ReleaseMenuContext context)
    {
        ClearPersistentListeners(context.PlayButton);
        ClearPersistentListeners(context.MainQuitButton);
        ClearPersistentListeners(context.ResumeButton);
        ClearPersistentListeners(context.RetryButton);
        ClearPersistentListeners(context.PauseQuitButton);
        UnityEventTools.AddPersistentListener(
            context.PlayButton.onClick,
            context.MenuController.PlayGame);
        UnityEventTools.AddPersistentListener(
            context.MainQuitButton.onClick,
            context.MenuController.QuitGame);
        UnityEventTools.AddPersistentListener(
            context.ResumeButton.onClick,
            context.MenuController.ResumeGame);
        UnityEventTools.AddPersistentListener(
            context.RetryButton.onClick,
            context.MenuController.RetryGame);
        UnityEventTools.AddPersistentListener(
            context.PauseQuitButton.onClick,
            context.MenuController.QuitGame);
    }

    private static void ClearPersistentListeners(Button button)
    {
        Undo.RecordObject(button, ReleaseMenuUndoName);

        for (int i = button.onClick.GetPersistentEventCount() - 1;
            i >= 0;
            i--)
        {
            UnityEventTools.RemovePersistentListener(button.onClick, i);
        }
    }

    private static void ValidateReleaseMenuAppliedState(
        ReleaseMenuContext context,
        List<string> errors)
    {
        foreach (PreservedHierarchyState state in context.PreservedStates)
        {
            state.Validate(errors);
        }

        ValidateReleaseCanvasAppliedState(context, errors);
        ValidateReleaseMenuHierarchy(context, errors);
        ValidateReleaseMenuControllerWiring(context, errors);
        ValidateReleaseButtonCallback(
            context.PlayButton,
            context.MenuController,
            nameof(CodebreakerMenuController.PlayGame),
            errors);
        ValidateReleaseButtonCallback(
            context.MainQuitButton,
            context.MenuController,
            nameof(CodebreakerMenuController.QuitGame),
            errors);
        ValidateReleaseButtonCallback(
            context.ResumeButton,
            context.MenuController,
            nameof(CodebreakerMenuController.ResumeGame),
            errors);
        ValidateReleaseButtonCallback(
            context.RetryButton,
            context.MenuController,
            nameof(CodebreakerMenuController.RetryGame),
            errors);
        ValidateReleaseButtonCallback(
            context.PauseQuitButton,
            context.MenuController,
            nameof(CodebreakerMenuController.QuitGame),
            errors);

        if (!context.MenuController.ValidateReferences())
        {
            errors.Add(
                "CodebreakerMenuController.ValidateReferences failed " +
                "after wiring.");
        }
    }

    private static void ValidateReleaseCanvasAppliedState(
        ReleaseMenuContext context,
        List<string> errors)
    {
        List<GameObject> roots =
            FindAllNamed(context.TargetScene, ReleaseMenuCanvasName);

        if (roots.Count != 1)
        {
            errors.Add(
                $"Expected one {ReleaseMenuCanvasName}; found " +
                $"{roots.Count}.");
            return;
        }

        GameObject root = roots[0];
        int controllerCount =
            GetSceneComponents<CodebreakerMenuController>(
                context.TargetScene).Count;

        if (root.transform.parent != null)
        {
            errors.Add(
                "ReleaseMenuCanvas transform.parent expected null but was " +
                $"{GetHierarchyPath(root.transform.parent.gameObject)}.");
        }

        if (root.scene != context.TargetScene)
        {
            errors.Add(
                "ReleaseMenuCanvas scene expected " +
                $"{context.TargetScene.path} but was {root.scene.path}.");
        }

        if (!root.activeSelf)
        {
            errors.Add(
                "ReleaseMenuCanvas activeSelf expected true but was false.");
        }

        if (controllerCount != 1)
        {
            errors.Add(
                "CodebreakerMenuController component count expected 1 but " +
                $"was {controllerCount}.");
        }

        Type[] expectedTypes =
        {
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(CodebreakerMenuController)
        };
        ValidateExactComponents(root, expectedTypes, errors);

        Canvas[] canvases = root.GetComponents<Canvas>();
        Canvas canvas = canvases.Length == 1 ? canvases[0] : null;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        GraphicRaycaster raycaster =
            root.GetComponent<GraphicRaycaster>();

        if (canvases.Length != 1)
        {
            errors.Add(
                "ReleaseMenuCanvas Canvas component count expected 1 but " +
                $"was {canvases.Length}.");
        }

        if (canvas != null)
        {
            if (!canvas.enabled)
            {
                errors.Add(
                    "ReleaseMenuCanvas Canvas enabled expected true but " +
                    "was false.");
            }

            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                errors.Add(
                    "ReleaseMenuCanvas Canvas renderMode expected " +
                    $"{RenderMode.ScreenSpaceOverlay} but was " +
                    $"{canvas.renderMode}.");
            }

            if (canvas.pixelPerfect)
            {
                errors.Add(
                    "ReleaseMenuCanvas Canvas pixelPerfect expected false " +
                    "but was true.");
            }

            if (canvas.overrideSorting)
            {
                errors.Add(
                    "ReleaseMenuCanvas Canvas overrideSorting expected " +
                    "false but was true.");
            }

            if (canvas.sortingOrder != 1000)
            {
                errors.Add(
                    "ReleaseMenuCanvas Canvas sortingOrder expected 1000 " +
                    $"but was {canvas.sortingOrder}.");
            }

            if (canvas.targetDisplay != 0)
            {
                errors.Add(
                    "ReleaseMenuCanvas Canvas targetDisplay expected 0 but " +
                    $"was {canvas.targetDisplay}.");
            }

            if (canvas.worldCamera != null)
            {
                errors.Add(
                    "ReleaseMenuCanvas Canvas worldCamera expected null but " +
                    $"was {canvas.worldCamera.name}.");
            }

            if (!canvas.isRootCanvas)
            {
                errors.Add(
                    "ReleaseMenuCanvas Canvas isRootCanvas expected true " +
                    "but was false.");
            }
        }

        if (scaler == null)
        {
            errors.Add(
                "ReleaseMenuCanvas CanvasScaler component expected one but " +
                "was missing.");
        }
        else
        {
            if (scaler.uiScaleMode !=
                CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                errors.Add(
                    "ReleaseMenuCanvas CanvasScaler uiScaleMode expected " +
                    $"{CanvasScaler.ScaleMode.ScaleWithScreenSize} but was " +
                    $"{scaler.uiScaleMode}.");
            }

            Vector2 resolution = scaler.referenceResolution;

            if (!Mathf.Approximately(resolution.x, 1920f))
            {
                errors.Add(
                    "ReleaseMenuCanvas CanvasScaler " +
                    "referenceResolution.x expected 1920 but was " +
                    $"{resolution.x}.");
            }

            if (!Mathf.Approximately(resolution.y, 1080f))
            {
                errors.Add(
                    "ReleaseMenuCanvas CanvasScaler " +
                    "referenceResolution.y expected 1080 but was " +
                    $"{resolution.y}.");
            }

            if (scaler.screenMatchMode !=
                CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
            {
                errors.Add(
                    "ReleaseMenuCanvas CanvasScaler screenMatchMode " +
                    $"expected " +
                    $"{CanvasScaler.ScreenMatchMode.MatchWidthOrHeight} " +
                    $"but was {scaler.screenMatchMode}.");
            }

            if (!Mathf.Approximately(
                    scaler.matchWidthOrHeight,
                    0.5f))
            {
                errors.Add(
                    "ReleaseMenuCanvas CanvasScaler matchWidthOrHeight " +
                    $"expected 0.5 but was {scaler.matchWidthOrHeight}.");
            }

            if (!Mathf.Approximately(
                    scaler.referencePixelsPerUnit,
                    100f))
            {
                errors.Add(
                    "ReleaseMenuCanvas CanvasScaler " +
                    "referencePixelsPerUnit expected 100 but was " +
                    $"{scaler.referencePixelsPerUnit}.");
            }
        }

        if (raycaster == null)
        {
            errors.Add(
                "ReleaseMenuCanvas GraphicRaycaster component expected one " +
                "but was missing.");
        }
        else
        {
            if (!raycaster.enabled)
            {
                errors.Add(
                    "ReleaseMenuCanvas GraphicRaycaster enabled expected " +
                    "true but was false.");
            }

            if (!raycaster.ignoreReversedGraphics)
            {
                errors.Add(
                    "ReleaseMenuCanvas GraphicRaycaster " +
                    "ignoreReversedGraphics expected true but was false.");
            }

            if (raycaster.blockingObjects !=
                GraphicRaycaster.BlockingObjects.None)
            {
                errors.Add(
                    "ReleaseMenuCanvas GraphicRaycaster blockingObjects " +
                    $"expected {GraphicRaycaster.BlockingObjects.None} but " +
                    $"was {raycaster.blockingObjects}.");
            }
        }

        if (root.GetComponentsInChildren<Canvas>(true).Length != 1 ||
            root.GetComponentInChildren<EventSystem>(true) != null ||
            root.GetComponentInChildren<StandaloneInputModule>(true) !=
                null ||
            root.GetComponentInChildren<Collider2D>(true) != null ||
            root.GetComponentInChildren<Rigidbody2D>(true) != null)
        {
            errors.Add(
                "ReleaseMenuCanvas contains a prohibited component.");
        }
    }

    private static void ValidateReleaseMenuHierarchy(
        ReleaseMenuContext context,
        List<string> errors)
    {
        string[] uniqueNames =
        {
            "MainMenuRoot",
            "MainMenuDim",
            "MainMenuPanel",
            "MainTitleText",
            "MainSubtitleText",
            "MainTaglineText",
            "PlayButton",
            "MainQuitButton",
            "PauseMenuRoot",
            "PauseMenuDim",
            "PauseMenuPanel",
            "PauseTitleText",
            "PauseHintText",
            "ResumeButton",
            "RetryButton",
            "PauseQuitButton"
        };

        foreach (string objectName in uniqueNames)
        {
            RequireExactNamedCount(
                context.TargetScene,
                objectName,
                1,
                errors);
        }

        GameObject mainRoot = FindSingleNamedUnchecked(
            context.TargetScene,
            "MainMenuRoot");
        GameObject pauseRoot = FindSingleNamedUnchecked(
            context.TargetScene,
            "PauseMenuRoot");
        GameObject mainPanel = FindSingleNamedUnchecked(
            context.TargetScene,
            "MainMenuPanel");
        GameObject pausePanel = FindSingleNamedUnchecked(
            context.TargetScene,
            "PauseMenuPanel");

        ValidateDirectChildren(
            context.ReleaseCanvas,
            errors,
            mainRoot,
            pauseRoot);
        ValidateDirectChildren(
            mainRoot,
            errors,
            FindSingleNamedUnchecked(context.TargetScene, "MainMenuDim"),
            mainPanel);
        ValidateDirectChildren(
            mainPanel,
            errors,
            FindSingleNamedUnchecked(context.TargetScene, "MainTitleText"),
            FindSingleNamedUnchecked(
                context.TargetScene,
                "MainSubtitleText"),
            FindSingleNamedUnchecked(
                context.TargetScene,
                "MainTaglineText"),
            context.PlayButton.gameObject,
            context.MainQuitButton.gameObject);
        ValidateDirectChildren(
            pauseRoot,
            errors,
            FindSingleNamedUnchecked(context.TargetScene, "PauseMenuDim"),
            pausePanel);
        ValidateDirectChildren(
            pausePanel,
            errors,
            FindSingleNamedUnchecked(context.TargetScene, "PauseTitleText"),
            FindSingleNamedUnchecked(context.TargetScene, "PauseHintText"),
            context.ResumeButton.gameObject,
            context.RetryButton.gameObject,
            context.PauseQuitButton.gameObject);

        if (mainRoot == null || !mainRoot.activeSelf ||
            pauseRoot == null || pauseRoot.activeSelf)
        {
            errors.Add(
                "MainMenuRoot must be active and PauseMenuRoot inactive " +
                "after construction.");
        }

        ValidateExactComponents(
            mainRoot,
            new[] { typeof(RectTransform) },
            errors);
        ValidateExactComponents(
            pauseRoot,
            new[] { typeof(RectTransform) },
            errors);
        ValidateFullStretchRect(mainRoot, errors);
        ValidateFullStretchRect(pauseRoot, errors);
        ValidateReleaseImage(
            context,
            "MainMenuDim",
            new Color(0.005f, 0.012f, 0.02f, 0.78f),
            true,
            true,
            Vector2.zero,
            Vector2.zero,
            errors);
        ValidateReleaseImage(
            context,
            "MainMenuPanel",
            new Color(0.012f, 0.035f, 0.055f, 0.94f),
            true,
            false,
            Vector2.zero,
            new Vector2(720f, 580f),
            errors);
        ValidateReleaseText(
            context,
            "MainTitleText",
            "COUNT DOWN",
            new Vector2(0f, 190f),
            new Vector2(620f, 100f),
            72f,
            FontStyles.Bold,
            new Color(1f, 0.78f, 0.20f, 1f),
            errors);
        ValidateReleaseText(
            context,
            "MainSubtitleText",
            "CODEBREAKER PROTOCOL",
            new Vector2(0f, 105f),
            new Vector2(620f, 54f),
            30f,
            FontStyles.Bold,
            new Color(0.30f, 0.88f, 1f, 1f),
            errors);
        ValidateReleaseText(
            context,
            "MainTaglineText",
            "RECOVER THE CODE. DEFUSE THE DEVICE.",
            new Vector2(0f, 52f),
            new Vector2(620f, 38f),
            18f,
            FontStyles.Normal,
            new Color(0.65f, 0.76f, 0.82f, 1f),
            errors);
        ValidateReleaseImage(
            context,
            "PauseMenuDim",
            new Color(0.005f, 0.012f, 0.02f, 0.72f),
            true,
            true,
            Vector2.zero,
            Vector2.zero,
            errors);
        ValidateReleaseImage(
            context,
            "PauseMenuPanel",
            new Color(0.012f, 0.035f, 0.055f, 0.96f),
            true,
            false,
            Vector2.zero,
            new Vector2(640f, 580f),
            errors);
        ValidateReleaseText(
            context,
            "PauseTitleText",
            "PAUSED",
            new Vector2(0f, 190f),
            new Vector2(560f, 90f),
            62f,
            FontStyles.Bold,
            new Color(1f, 0.78f, 0.20f, 1f),
            errors);
        ValidateReleaseText(
            context,
            "PauseHintText",
            "PRESS ESC TO RESUME",
            new Vector2(0f, 125f),
            new Vector2(560f, 38f),
            18f,
            FontStyles.Normal,
            new Color(0.65f, 0.76f, 0.82f, 1f),
            errors);

        ValidateReleaseButton(
            context,
            context.PlayButton,
            "PLAY",
            new Vector2(0f, -80f),
            new Vector2(360f, 76f),
            new Color(0.04f, 0.32f, 0.38f, 0.98f),
            errors);
        ValidateReleaseButton(
            context,
            context.MainQuitButton,
            "QUIT",
            new Vector2(0f, -180f),
            new Vector2(360f, 76f),
            new Color(0.38f, 0.08f, 0.07f, 0.98f),
            errors);
        ValidateReleaseButton(
            context,
            context.ResumeButton,
            "RESUME",
            new Vector2(0f, 35f),
            new Vector2(360f, 72f),
            new Color(0.04f, 0.32f, 0.38f, 0.98f),
            errors);
        ValidateReleaseButton(
            context,
            context.RetryButton,
            "RETRY",
            new Vector2(0f, -65f),
            new Vector2(360f, 72f),
            new Color(0.42f, 0.29f, 0.06f, 0.98f),
            errors);
        ValidateReleaseButton(
            context,
            context.PauseQuitButton,
            "QUIT",
            new Vector2(0f, -165f),
            new Vector2(360f, 72f),
            new Color(0.38f, 0.08f, 0.07f, 0.98f),
            errors);
    }

    private static void ValidateReleaseImage(
        ReleaseMenuContext context,
        string objectName,
        Color expectedColor,
        bool expectedRaycastTarget,
        bool fullStretch,
        Vector2 expectedPosition,
        Vector2 expectedSize,
        List<string> errors)
    {
        GameObject gameObject = FindSingleNamedUnchecked(
            context.TargetScene,
            objectName);

        if (gameObject == null)
        {
            return;
        }

        ValidateExactComponents(
            gameObject,
            new[]
            {
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            },
            errors);
        Image[] images = gameObject.GetComponents<Image>();

        if (images.Length != 1)
        {
            errors.Add(
                $"{objectName} Image component count expected 1 but was " +
                $"{images.Length}.");
        }

        Image image = images.Length == 1 ? images[0] : null;

        if (image != null)
        {
            if (!image.enabled)
            {
                errors.Add(
                    $"{objectName} Image enabled expected true but was " +
                    "false.");
            }

            if (image.sprite != null)
            {
                errors.Add(
                    $"{objectName} Image sprite expected null but was " +
                    $"{image.sprite.name}.");
            }

            if (image.overrideSprite != null)
            {
                errors.Add(
                    $"{objectName} Image overrideSprite expected null but " +
                    $"was {image.overrideSprite.name}.");
            }

            if (!HasNoSerializedCustomMaterial(image))
            {
                errors.Add(
                    $"{objectName} Image serialized custom material " +
                    "expected null but was " +
                    $"{DescribeSerializedCustomMaterial(image)}.");
            }

            if (image.type != Image.Type.Simple)
            {
                errors.Add(
                    $"{objectName} Image type expected {Image.Type.Simple} " +
                    $"but was {image.type}.");
            }

            if (image.raycastTarget != expectedRaycastTarget)
            {
                errors.Add(
                    $"{objectName} Image raycastTarget expected " +
                    $"{FormatBool(expectedRaycastTarget)} but was " +
                    $"{FormatBool(image.raycastTarget)}.");
            }

            if (!image.maskable)
            {
                errors.Add(
                    $"{objectName} Image maskable expected true but was " +
                    "false.");
            }

            if (!ApproximatelyColor(image.color, expectedColor))
            {
                errors.Add(
                    $"{objectName} Image color expected " +
                    $"{FormatColor(expectedColor)} but was " +
                    $"{FormatColor(image.color)}.");
            }
        }

        if (fullStretch)
        {
            ValidateFullStretchRect(gameObject, errors);
        }
        else
        {
            ValidateCenteredRect(
                gameObject,
                expectedPosition,
                expectedSize,
                errors);
        }
    }

    private static bool HasNoSerializedCustomMaterial(Graphic graphic)
    {
        if (graphic == null)
        {
            return false;
        }

        SerializedObject serializedGraphic =
            new SerializedObject(graphic);
        serializedGraphic.Update();
        SerializedProperty materialProperty =
            serializedGraphic.FindProperty("m_Material");
        return materialProperty != null &&
            materialProperty.objectReferenceValue == null;
    }

    private static string DescribeSerializedCustomMaterial(
        Graphic graphic)
    {
        if (graphic == null)
        {
            return "<null Graphic>";
        }

        SerializedObject serializedGraphic =
            new SerializedObject(graphic);
        serializedGraphic.Update();
        SerializedProperty materialProperty =
            serializedGraphic.FindProperty("m_Material");

        if (materialProperty == null)
        {
            return "<missing m_Material property>";
        }

        Object material = materialProperty.objectReferenceValue;
        return material == null ? "null" : material.name;
    }

    private static bool ApproximatelyColor(
        Color actual,
        Color expected)
    {
        return Mathf.Approximately(actual.r, expected.r) &&
            Mathf.Approximately(actual.g, expected.g) &&
            Mathf.Approximately(actual.b, expected.b) &&
            Mathf.Approximately(actual.a, expected.a);
    }

    private static string FormatColor(Color color)
    {
        return
            $"({color.r:0.000}, {color.g:0.000}, " +
            $"{color.b:0.000}, {color.a:0.000})";
    }

    private static string FormatBool(bool value)
    {
        return value ? "true" : "false";
    }

    private static void ValidateReleaseText(
        ReleaseMenuContext context,
        string objectName,
        string expectedContent,
        Vector2 expectedPosition,
        Vector2 expectedSize,
        float expectedFontSize,
        FontStyles expectedFontStyle,
        Color expectedColor,
        List<string> errors)
    {
        GameObject gameObject = FindSingleNamedUnchecked(
            context.TargetScene,
            objectName);

        if (gameObject == null)
        {
            return;
        }

        ValidateExactComponents(
            gameObject,
            new[]
            {
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI)
            },
            errors);
        TMP_Text text = gameObject.GetComponent<TMP_Text>();

        if (text == null ||
            text.text != expectedContent ||
            text.font != context.FontAsset ||
            text.fontSharedMaterial != context.FontMaterial ||
            text.fontSize != expectedFontSize ||
            text.fontStyle != expectedFontStyle ||
            text.color != expectedColor ||
            text.alignment != TextAlignmentOptions.Center ||
            text.raycastTarget)
        {
            errors.Add($"{objectName} TMP settings are invalid.");
        }

        ValidateCenteredRect(
            gameObject,
            expectedPosition,
            expectedSize,
            errors);
    }

    private static void ValidateFullStretchRect(
        GameObject gameObject,
        List<string> errors)
    {
        if (gameObject == null)
        {
            return;
        }

        RectTransform rectTransform =
            gameObject.GetComponent<RectTransform>();

        if (rectTransform == null ||
            rectTransform.anchorMin != Vector2.zero ||
            rectTransform.anchorMax != Vector2.one ||
            rectTransform.pivot != new Vector2(0.5f, 0.5f) ||
            rectTransform.anchoredPosition3D != Vector3.zero ||
            rectTransform.sizeDelta != Vector2.zero ||
            rectTransform.offsetMin != Vector2.zero ||
            rectTransform.offsetMax != Vector2.zero ||
            rectTransform.localScale != Vector3.one ||
            rectTransform.localRotation != Quaternion.identity)
        {
            errors.Add(
                $"{GetHierarchyPath(gameObject)} full-stretch " +
                "RectTransform is invalid.");
        }
    }

    private static void ValidateCenteredRect(
        GameObject gameObject,
        Vector2 expectedPosition,
        Vector2 expectedSize,
        List<string> errors)
    {
        if (gameObject == null)
        {
            return;
        }

        RectTransform rectTransform =
            gameObject.GetComponent<RectTransform>();
        Vector2 centered = new Vector2(0.5f, 0.5f);

        if (rectTransform == null ||
            rectTransform.anchorMin != centered ||
            rectTransform.anchorMax != centered ||
            rectTransform.pivot != centered ||
            rectTransform.anchoredPosition != expectedPosition ||
            rectTransform.sizeDelta != expectedSize ||
            rectTransform.localScale != Vector3.one ||
            rectTransform.localRotation != Quaternion.identity)
        {
            errors.Add(
                $"{GetHierarchyPath(gameObject)} centered " +
                "RectTransform is invalid.");
        }
    }

    private static void ValidateReleaseButton(
        ReleaseMenuContext context,
        Button button,
        string expectedLabel,
        Vector2 expectedPosition,
        Vector2 expectedSize,
        Color normalColor,
        List<string> errors)
    {
        if (button == null)
        {
            errors.Add($"Release menu button {expectedLabel} is missing.");
            return;
        }

        ValidateExactComponents(
            button.gameObject,
            new[]
            {
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button)
            },
            errors);
        ValidateCenteredRect(
            button.gameObject,
            expectedPosition,
            expectedSize,
            errors);

        if (button.transform.childCount != 1 ||
            button.transform.GetChild(0).name != "Label")
        {
            errors.Add(
                $"{button.name} must contain exactly one direct Label.");
            return;
        }

        TMP_Text label = button.transform.GetChild(0)
            .GetComponent<TMP_Text>();
        ValidateExactComponents(
            button.transform.GetChild(0).gameObject,
            new[]
            {
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI)
            },
            errors);
        ValidateFullStretchRect(
            button.transform.GetChild(0).gameObject,
            errors);
        Image[] buttonImages = button.GetComponents<Image>();
        Image image =
            buttonImages.Length == 1 ? buttonImages[0] : null;
        ColorBlock colors = button.colors;

        if (label == null ||
            label.text != expectedLabel ||
            label.font != context.FontAsset ||
            label.fontSharedMaterial != context.FontMaterial ||
            label.fontSize != 28f ||
            label.fontStyle != FontStyles.Bold ||
            label.color != Color.white ||
            label.raycastTarget)
        {
            errors.Add($"{button.name} Label settings are invalid.");
        }

        if (buttonImages.Length != 1)
        {
            errors.Add(
                $"{button.name} Image component count expected 1 but was " +
                $"{buttonImages.Length}.");
        }

        if (image != null)
        {
            if (!image.enabled)
            {
                errors.Add(
                    $"{button.name} Image enabled expected true but was " +
                    "false.");
            }

            if (image.sprite != null)
            {
                errors.Add(
                    $"{button.name} Image sprite expected null but was " +
                    $"{image.sprite.name}.");
            }

            if (image.overrideSprite != null)
            {
                errors.Add(
                    $"{button.name} Image overrideSprite expected null but " +
                    $"was {image.overrideSprite.name}.");
            }

            if (!HasNoSerializedCustomMaterial(image))
            {
                errors.Add(
                    $"{button.name} Image serialized custom material " +
                    "expected null but was " +
                    $"{DescribeSerializedCustomMaterial(image)}.");
            }

            if (image.type != Image.Type.Simple)
            {
                errors.Add(
                    $"{button.name} Image type expected " +
                    $"{Image.Type.Simple} but was {image.type}.");
            }

            if (!image.raycastTarget)
            {
                errors.Add(
                    $"{button.name} Image raycastTarget expected true but " +
                    "was false.");
            }

            if (!image.maskable)
            {
                errors.Add(
                    $"{button.name} Image maskable expected true but was " +
                    "false.");
            }

            if (!ApproximatelyColor(image.color, Color.white))
            {
                errors.Add(
                    $"{button.name} Image color expected " +
                    $"{FormatColor(Color.white)} but was " +
                    $"{FormatColor(image.color)}.");
            }

            if (button.targetGraphic != image)
            {
                string actualTarget = button.targetGraphic == null
                    ? "null"
                    : button.targetGraphic.name;
                errors.Add(
                    $"{button.name} Button targetGraphic expected its " +
                    $"Image but was {actualTarget}.");
            }
        }

        if (button.transition != Selectable.Transition.ColorTint)
        {
            errors.Add(
                $"{button.name} Button transition expected " +
                $"{Selectable.Transition.ColorTint} but was " +
                $"{button.transition}.");
        }

        if (button.navigation.mode != Navigation.Mode.Automatic)
        {
            errors.Add(
                $"{button.name} Button navigation.mode expected " +
                $"{Navigation.Mode.Automatic} but was " +
                $"{button.navigation.mode}.");
        }

        Color expectedHighlighted =
            MultiplyRgbPreserveAlpha(normalColor, 1.18f);
        Color expectedPressed =
            MultiplyRgbPreserveAlpha(normalColor, 0.78f);
        Color expectedDisabled =
            new Color(0.16f, 0.18f, 0.20f, 0.75f);

        if (!ApproximatelyColor(colors.normalColor, normalColor))
        {
            errors.Add(
                $"{button.name} Button colors.normalColor expected " +
                $"{FormatColor(normalColor)} but was " +
                $"{FormatColor(colors.normalColor)}.");
        }

        if (!ApproximatelyColor(
                colors.highlightedColor,
                expectedHighlighted))
        {
            errors.Add(
                $"{button.name} Button colors.highlightedColor expected " +
                $"{FormatColor(expectedHighlighted)} but was " +
                $"{FormatColor(colors.highlightedColor)}.");
        }

        if (!ApproximatelyColor(colors.pressedColor, expectedPressed))
        {
            errors.Add(
                $"{button.name} Button colors.pressedColor expected " +
                $"{FormatColor(expectedPressed)} but was " +
                $"{FormatColor(colors.pressedColor)}.");
        }

        if (!ApproximatelyColor(
                colors.selectedColor,
                expectedHighlighted))
        {
            errors.Add(
                $"{button.name} Button colors.selectedColor expected " +
                $"{FormatColor(expectedHighlighted)} but was " +
                $"{FormatColor(colors.selectedColor)}.");
        }

        if (!ApproximatelyColor(colors.disabledColor, expectedDisabled))
        {
            errors.Add(
                $"{button.name} Button colors.disabledColor expected " +
                $"{FormatColor(expectedDisabled)} but was " +
                $"{FormatColor(colors.disabledColor)}.");
        }

        if (!Mathf.Approximately(colors.colorMultiplier, 1f))
        {
            errors.Add(
                $"{button.name} Button colors.colorMultiplier expected 1 " +
                $"but was {colors.colorMultiplier}.");
        }

        if (!Mathf.Approximately(colors.fadeDuration, 0.08f))
        {
            errors.Add(
                $"{button.name} Button colors.fadeDuration expected 0.08 " +
                $"but was {colors.fadeDuration}.");
        }
    }

    private static void ValidateReleaseMenuControllerWiring(
        ReleaseMenuContext context,
        List<string> errors)
    {
        if (context.MenuController == null)
        {
            errors.Add("CodebreakerMenuController is missing.");
            return;
        }

        SerializedObject serializedController =
            new SerializedObject(context.MenuController);
        serializedController.Update();
        ValidateObjectReference(
            serializedController,
            "gameController",
            context.GameController,
            errors);
        ValidateObjectReference(
            serializedController,
            "globalTimer",
            context.GlobalTimer,
            errors);
        ValidateObjectReference(
            serializedController,
            "equationInteraction",
            context.EquationInteraction,
            errors);
        ValidateObjectReference(
            serializedController,
            "gameplayHudRoot",
            context.HudCanvas,
            errors);
        ValidateObjectReference(
            serializedController,
            "mainMenuRoot",
            FindSingleNamedUnchecked(
                context.TargetScene,
                "MainMenuRoot"),
            errors);
        ValidateObjectReference(
            serializedController,
            "pauseMenuRoot",
            FindSingleNamedUnchecked(
                context.TargetScene,
                "PauseMenuRoot"),
            errors);
        ValidateObjectReference(
            serializedController,
            "playButton",
            context.PlayButton,
            errors);
        ValidateObjectReference(
            serializedController,
            "resumeButton",
            context.ResumeButton,
            errors);
        ValidateObjectReference(
            serializedController,
            "retryButton",
            context.RetryButton,
            errors);
        ValidateObjectReference(
            serializedController,
            "mainQuitButton",
            context.MainQuitButton,
            errors);
        ValidateObjectReference(
            serializedController,
            "pauseQuitButton",
            context.PauseQuitButton,
            errors);
    }

    private static void ValidateReleaseButtonCallback(
        Button button,
        CodebreakerMenuController target,
        string methodName,
        List<string> errors)
    {
        if (button == null)
        {
            return;
        }

        if (button.onClick.GetPersistentEventCount() != 1 ||
            button.onClick.GetPersistentTarget(0) != target ||
            button.onClick.GetPersistentMethodName(0) != methodName)
        {
            errors.Add(
                $"{button.name} must have exactly one persistent callback " +
                $"to CodebreakerMenuController.{methodName}.");
        }
    }

    private static void ValidateExactComponents(
        GameObject gameObject,
        Type[] expectedTypes,
        List<string> errors)
    {
        if (gameObject == null)
        {
            return;
        }

        Component[] components = gameObject.GetComponents<Component>();

        if (components.Length != expectedTypes.Length)
        {
            errors.Add(
                $"{GetHierarchyPath(gameObject)} has " +
                $"{components.Length} components; expected " +
                $"{expectedTypes.Length}.");
            return;
        }

        foreach (Type expectedType in expectedTypes)
        {
            int count = 0;

            foreach (Component component in components)
            {
                if (component != null &&
                    expectedType.IsInstanceOfType(component))
                {
                    count++;
                }
            }

            if (count != 1)
            {
                errors.Add(
                    $"{GetHierarchyPath(gameObject)} must contain exactly " +
                    $"one {expectedType.Name}; found {count}.");
            }
        }
    }

    private static void ValidateDirectChildren(
        GameObject parent,
        List<string> errors,
        params GameObject[] expectedChildren)
    {
        if (parent == null)
        {
            return;
        }

        if (parent.transform.childCount != expectedChildren.Length)
        {
            errors.Add(
                $"{GetHierarchyPath(parent)} has " +
                $"{parent.transform.childCount} direct children; expected " +
                $"{expectedChildren.Length}.");
        }

        foreach (GameObject child in expectedChildren)
        {
            int expectedIndex = Array.IndexOf(expectedChildren, child);

            if (child == null ||
                child.transform.parent != parent.transform ||
                child.transform.GetSiblingIndex() != expectedIndex)
            {
                errors.Add(
                    $"{GetHierarchyPath(parent)} has an invalid child " +
                    "hierarchy.");
                return;
            }
        }
    }

    private static GameObject FindSingleNamedUnchecked(
        Scene scene,
        string objectName)
    {
        List<GameObject> matches = FindAllNamed(scene, objectName);
        return matches.Count == 1 ? matches[0] : null;
    }

    private static List<T> GetSceneComponents<T>(Scene scene)
        where T : Component
    {
        List<T> components = new List<T>();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            components.AddRange(root.GetComponentsInChildren<T>(true));
        }

        return components;
    }

    private static void ReportReleaseMenuFailure(string message)
    {
        Debug.LogError(message);
        EditorUtility.DisplayDialog(
            ReleaseMenuDialogTitle,
            message,
            "OK");
    }

    private static void ReportReleaseMenuFailures(List<string> errors)
    {
        foreach (string error in errors)
        {
            Debug.LogError(error);
        }

        ReportReleaseMenuFailure(
            "CODEBREAKER RELEASE MENU VALIDATION FAILED\n\n- " +
            string.Join("\n- ", errors));
    }

    private static void ReportRefusal(string message)
    {
        Debug.LogError(message);
        EditorUtility.DisplayDialog(DialogTitle, message, "OK");
    }

    private static void ReportValidationFailures(List<string> errors)
    {
        foreach (string error in errors)
        {
            Debug.LogError(error);
        }

        string report =
            "CODEBREAKER TUTORIAL UI VALIDATION FAILED\n\n- " +
            string.Join("\n- ", errors);
        Debug.LogError(report);
        EditorUtility.DisplayDialog(DialogTitle, report, "OK");
    }

    private sealed class ReleaseMenuContext
    {
        public Scene TargetScene;
        public CodebreakerGameController GameController;
        public GlobalBombTimer GlobalTimer;
        public CodebreakerEquationInteractionController EquationInteraction;
        public CodebreakerHUD Hud;
        public EventSystem EventSystem;
        public GameObject HudCanvas;
        public GameObject BombBackground;
        public GameObject ReleaseCanvas;
        public TMP_FontAsset FontAsset;
        public Material FontMaterial;
        public CodebreakerMenuController MenuController;
        public Button PlayButton;
        public Button ResumeButton;
        public Button RetryButton;
        public Button MainQuitButton;
        public Button PauseQuitButton;
        public readonly List<PreservedHierarchyState> PreservedStates =
            new List<PreservedHierarchyState>();
    }

    private sealed class TutorialContext
    {
        public Scene TargetScene;
        public GameObject HudCanvas;
        public GameObject EquationRoot;
        public GameObject DisplayA;
        public GameObject DisplayB;
        public GameObject BombBackground;
        public PreservedHierarchyState BombBackgroundState;
        public PreservedHierarchyState[] CentralEquationStates;
        public PreservedHierarchyState BufferPresentationState;
        public PreservedHierarchyState[] SupportUiStates;
        public PreservedGameObjectState PuzzleControllerState;
        public CodebreakerEquationHUD EquationHud;
        public LayeredDigitPuzzleController PuzzleController;
        public SegmentInventoryTray InventoryTray;
        public InventoryDropZone InventoryDropZone;
        public Collider2D[] DropZoneColliders;
        public bool[] DropZoneColliderEnabledStates;
        public SharedSegmentInventory Inventory;
        public InventorySegmentToken TokenTemplate;
        public Transform TokenContainer;
        public TMP_Text CountText;
        public TMP_FontAsset CountFontAsset;
        public Material CountSharedMaterial;
        public GameObject TokenVisualObject;
        public SpriteRenderer[] TokenTemplateRenderers;
        public SpriteRenderer PrimarySegmentRenderer;
        public SpriteRenderer[] TrayBackdropRenderers;
        public Sprite PrimarySprite;
        public Material PrimarySharedMaterial;
        public Color PrimaryColor;
        public int PrimarySortingLayerId;
        public int PrimarySortingOrder;
        public bool PrimaryEnabled;
        public Vector3 PrimaryLocalPosition;
        public Quaternion PrimaryLocalRotation;
        public Vector3 PrimaryLocalScale;
        public TMP_Text EntryProgressText;
        public TMP_Text TargetEquationText;
        public TMP_Text CurrentValuesText;
        public TMP_Text AcceptedDigitsText;
        public TMP_Text FeedbackText;
        public TMP_Text EquationInstructionText;
        public TMP_Text SerializedEquationOperatorText;
        public TMP_Text SerializedEquationReadyText;
        public TMP_Text SerializedBufferFeedbackText;
        public Color EquationReadyColor;
        public TMP_Text PuzzleProgressText;
        public TMP_Text HitsLeftText;
        public TMP_Text PuzzleInstructionText;
        public TMP_Text PuzzleFeedbackText;
        public GameObject EquationPlusText;
        public GameObject EquationReadyText;
        public GameObject BufferFeedbackText;
        public GameObject EquationStatusPanel;
        public GameObject BufferCapacitySlotsRoot;
        public GameObject BufferSlotVisual01;
        public GameObject BufferSlotVisual02;
        public GameObject EquationALabelText;
        public GameObject EquationBLabelText;
    }

    private sealed class PreservedHierarchyState
    {
        private readonly GameObject root;
        private readonly string label;
        private readonly PreservedGameObjectState[] objectStates;

        public PreservedHierarchyState(
            GameObject hierarchyRoot,
            string stateLabel)
        {
            root = hierarchyRoot;
            label = stateLabel;
            Transform[] transforms =
                hierarchyRoot.GetComponentsInChildren<Transform>(true);
            objectStates =
                new PreservedGameObjectState[transforms.Length];

            for (int i = 0; i < transforms.Length; i++)
            {
                objectStates[i] =
                    new PreservedGameObjectState(
                        transforms[i].gameObject);
            }
        }

        public void Validate(List<string> errors)
        {
            if (root == null)
            {
                errors.Add(
                    $"{label} was removed during the tutorial UI pass.");
                return;
            }

            Transform[] currentTransforms =
                root.GetComponentsInChildren<Transform>(true);

            if (currentTransforms.Length != objectStates.Length)
            {
                errors.Add(
                    $"{label} hierarchy changed during the tutorial UI " +
                    "pass.");
            }

            foreach (PreservedGameObjectState state in objectStates)
            {
                state.Validate(label, errors);
            }
        }
    }

    private sealed class PreservedGameObjectState
    {
        private readonly GameObject gameObject;
        private readonly string hierarchyPath;
        private readonly string objectName;
        private readonly string tag;
        private readonly int layer;
        private readonly bool activeSelf;
        private readonly bool isStatic;
        private readonly HideFlags hideFlags;
        private readonly Transform parent;
        private readonly Vector3 localPosition;
        private readonly Quaternion localRotation;
        private readonly Vector3 localScale;
        private readonly bool isRectTransform;
        private readonly Vector2 anchorMin;
        private readonly Vector2 anchorMax;
        private readonly Vector2 pivot;
        private readonly Vector3 anchoredPosition3D;
        private readonly Vector2 sizeDelta;
        private readonly Vector2 offsetMin;
        private readonly Vector2 offsetMax;
        private readonly Component[] components;
        private readonly string[] serializedComponents;

        public PreservedGameObjectState(GameObject target)
        {
            gameObject = target;
            hierarchyPath = GetHierarchyPath(target);
            objectName = target.name;
            tag = target.tag;
            layer = target.layer;
            activeSelf = target.activeSelf;
            isStatic = target.isStatic;
            hideFlags = target.hideFlags;

            Transform targetTransform = target.transform;
            parent = targetTransform.parent;
            localPosition = targetTransform.localPosition;
            localRotation = targetTransform.localRotation;
            localScale = targetTransform.localScale;
            RectTransform rectTransform =
                targetTransform as RectTransform;
            isRectTransform = rectTransform != null;

            if (rectTransform != null)
            {
                anchorMin = rectTransform.anchorMin;
                anchorMax = rectTransform.anchorMax;
                pivot = rectTransform.pivot;
                anchoredPosition3D =
                    rectTransform.anchoredPosition3D;
                sizeDelta = rectTransform.sizeDelta;
                offsetMin = rectTransform.offsetMin;
                offsetMax = rectTransform.offsetMax;
            }

            components = target.GetComponents<Component>();
            serializedComponents = new string[components.Length];

            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];

                if (component != null &&
                    !(component is Transform))
                {
                    serializedComponents[i] =
                        EditorJsonUtility.ToJson(component);
                }
            }
        }

        public void Validate(
            string stateLabel,
            List<string> errors)
        {
            if (gameObject == null)
            {
                errors.Add(
                    $"{stateLabel} object {hierarchyPath} was removed.");
                return;
            }

            Transform currentTransform = gameObject.transform;
            RectTransform currentRect =
                currentTransform as RectTransform;

            if (gameObject.name != objectName ||
                gameObject.tag != tag ||
                gameObject.layer != layer ||
                gameObject.activeSelf != activeSelf ||
                gameObject.isStatic != isStatic ||
                gameObject.hideFlags != hideFlags ||
                currentTransform.parent != parent ||
                currentTransform.localPosition != localPosition ||
                currentTransform.localRotation != localRotation ||
                currentTransform.localScale != localScale ||
                (currentRect != null) != isRectTransform)
            {
                errors.Add(
                    $"{stateLabel} object {hierarchyPath} changed.");
            }

            if (isRectTransform &&
                (currentRect.anchorMin != anchorMin ||
                 currentRect.anchorMax != anchorMax ||
                 currentRect.pivot != pivot ||
                 currentRect.anchoredPosition3D !=
                    anchoredPosition3D ||
                 currentRect.sizeDelta != sizeDelta ||
                 currentRect.offsetMin != offsetMin ||
                 currentRect.offsetMax != offsetMax))
            {
                errors.Add(
                    $"{stateLabel} RectTransform {hierarchyPath} changed.");
            }

            Component[] currentComponents =
                gameObject.GetComponents<Component>();

            if (currentComponents.Length != components.Length)
            {
                errors.Add(
                    $"{stateLabel} component structure at " +
                    $"{hierarchyPath} changed.");
                return;
            }

            for (int i = 0; i < components.Length; i++)
            {
                if (currentComponents[i] != components[i])
                {
                    errors.Add(
                        $"{stateLabel} component identity at " +
                        $"{hierarchyPath} changed.");
                    continue;
                }

                Component component = currentComponents[i];

                if (component != null &&
                    !(component is Transform) &&
                    EditorJsonUtility.ToJson(component) !=
                        serializedComponents[i])
                {
                    errors.Add(
                        $"{stateLabel} component " +
                        $"{component.GetType().Name} at " +
                        $"{hierarchyPath} changed.");
                }
            }
        }
    }

    private struct StaticLabelLayout
    {
        public string Name { get; }
        public string Text { get; }
        public Vector2 Position { get; }
        public Vector2 Size { get; }
        public float FontSize { get; }

        public StaticLabelLayout(
            string name,
            string text,
            Vector2 position,
            Vector2 size,
            float fontSize)
        {
            Name = name;
            Text = text;
            Position = position;
            Size = size;
            FontSize = fontSize;
        }
    }

    private struct StatusTextLayout
    {
        public string Name { get; }
        public string Text { get; }
        public Vector2 Position { get; }
        public Vector2 Size { get; }
        public float FontSize { get; }
        public FontStyles FontStyle { get; }
        public Color Color { get; }

        public StatusTextLayout(
            string name,
            string text,
            Vector2 position,
            Vector2 size,
            float fontSize,
            FontStyles fontStyle,
            Color color)
        {
            Name = name;
            Text = text;
            Position = position;
            Size = size;
            FontSize = fontSize;
            FontStyle = fontStyle;
            Color = color;
        }
    }
}
