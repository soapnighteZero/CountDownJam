using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
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
    private const string PhaseOneInstruction =
        "<size=30><b>USE ALL 4 HITS TO LEAVE ONE GREEN DIGIT</b></size>\n" +
        "<size=18>CLICK A SEGMENT = REMOVE ONE LAYER   |   RED > YELLOW > GREEN > OFF   |   DOTS = LAYERS LEFT</size>";
    private const string PhaseTwoInstruction = "";
    private const string SuccessReport =
        "FINAL EQUATION ENTRY UI CLEANUP BUILT\n\n" +
        "Equation hierarchy refined\n" +
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

        ConfigureSupportText(
            context.EntryProgressText,
            new Vector2(-690f, 85f),
            new Vector2(500f, 36f),
            20f);
        ConfigureSupportText(
            context.CurrentValuesText,
            new Vector2(-690f, 45f),
            new Vector2(540f, 36f),
            19f);
        ConfigureSupportText(
            context.AcceptedDigitsText,
            new Vector2(-690f, 5f),
            new Vector2(500f, 36f),
            19f);
        ConfigureSupportText(
            context.FeedbackText,
            new Vector2(-690f, -38f),
            new Vector2(550f, 48f),
            18f);

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
            plusObject,
            readyObject,
            bufferFeedbackObject,
            slotsRoot,
            slot01,
            slot02);
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
            new Vector2(-690f, 85f),
            new Vector2(500f, 36f),
            20f,
            errors);
        ValidateSupportTextLayout(
            context.CurrentValuesText,
            new Vector2(-690f, 45f),
            new Vector2(540f, 36f),
            19f,
            errors);
        ValidateSupportTextLayout(
            context.AcceptedDigitsText,
            new Vector2(-690f, 5f),
            new Vector2(500f, 36f),
            19f,
            errors);
        ValidateSupportTextLayout(
            context.FeedbackText,
            new Vector2(-690f, -38f),
            new Vector2(550f, 48f),
            18f,
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

    private static void ValidateSupportTextLayout(
        TMP_Text text,
        Vector2 position,
        Vector2 size,
        float fontSize,
        List<string> errors)
    {
        RectTransform rectTransform =
            text.GetComponent<RectTransform>();

        if (rectTransform.anchoredPosition != position ||
            rectTransform.sizeDelta != size ||
            rectTransform.anchorMin != new Vector2(0.5f, 0.5f) ||
            rectTransform.anchorMax != new Vector2(0.5f, 0.5f) ||
            rectTransform.pivot != new Vector2(0.5f, 0.5f) ||
            rectTransform.localScale != Vector3.one ||
            rectTransform.localRotation != Quaternion.identity ||
            !Mathf.Approximately(text.fontSize, fontSize) ||
            text.horizontalAlignment != HorizontalAlignmentOptions.Left ||
            text.verticalAlignment != VerticalAlignmentOptions.Middle ||
            text.enableAutoSizing ||
            text.raycastTarget ||
            text.overflowMode != TextOverflowModes.Overflow)
        {
            errors.Add(
                $"{text.gameObject.name} support-text layout is invalid.");
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

    private static void ConfigureSupportText(
        TMP_Text text,
        Vector2 position,
        Vector2 size,
        float fontSize)
    {
        ConfigureText(
            text.GetComponent<RectTransform>(),
            text,
            position,
            size,
            fontSize,
            HorizontalAlignmentOptions.Left,
            setVerticalMiddle: true,
            content: text.text,
            setOverflow: true);
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

    private sealed class TutorialContext
    {
        public Scene TargetScene;
        public GameObject HudCanvas;
        public GameObject EquationRoot;
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
        public TMP_Text PuzzleProgressText;
        public TMP_Text HitsLeftText;
        public TMP_Text PuzzleInstructionText;
        public TMP_Text PuzzleFeedbackText;
        public GameObject EquationPlusText;
        public GameObject EquationReadyText;
        public GameObject BufferFeedbackText;
        public GameObject BufferCapacitySlotsRoot;
        public GameObject BufferSlotVisual01;
        public GameObject BufferSlotVisual02;
        public GameObject EquationALabelText;
        public GameObject EquationBLabelText;
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
}
