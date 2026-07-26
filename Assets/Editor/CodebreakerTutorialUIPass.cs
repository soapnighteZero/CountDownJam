using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
    private const string PhaseOneInstruction =
        "<size=30><b>USE ALL 4 HITS TO LEAVE ONE GREEN DIGIT</b></size>\n" +
        "<size=18>CLICK A SEGMENT = REMOVE ONE LAYER   |   RED > YELLOW > GREEN > OFF   |   DOTS = LAYERS LEFT</size>";
    private const string PhaseTwoInstruction =
        "MOVE SEGMENTS BETWEEN THE TWO DISPLAYS AND BUFFER\n" +
        "MAKE THE EQUATION TRUE, THEN PRESS SPACE";
    private const string SuccessReport =
        "CODEBREAKER USABILITY FEEDBACK PASS BUILT\n\n" +
        "Two Buffer slots added\n" +
        "Central equation-ready feedback wired\n" +
        "Buffer-full feedback moved near Buffer\n" +
        "Phase 1 objective hierarchy improved\n" +
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
            new Vector2(0f, -80f),
            new Vector2(130f, 140f),
            96f);
    private static readonly StaticLabelLayout ReadyTextLayout =
        new StaticLabelLayout(
            "EquationReadyText",
            string.Empty,
            new Vector2(500f, -160f),
            new Vector2(420f, 46f),
            23f);
    private static readonly StaticLabelLayout BufferFeedbackLayout =
        new StaticLabelLayout(
            "BufferFeedbackText",
            string.Empty,
            new Vector2(0f, -355f),
            new Vector2(900f, 42f),
            20f);

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
            else if (context.TokenVisualObject
                .GetComponentsInChildren<SpriteRenderer>(true).Length == 0)
            {
                errors.Add(
                    "SegmentInventoryTray tokenTemplate.VisualObject must " +
                    "contain at least one SpriteRenderer.");
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

        if (context.EquationPlusText != null)
        {
            RequireComponent<RectTransform>(
                context.EquationPlusText,
                PlusLabelLayout.Name,
                errors);
            RequireComponent<TMP_Text>(
                context.EquationPlusText,
                PlusLabelLayout.Name,
                errors);
            RequireComponent<CanvasRenderer>(
                context.EquationPlusText,
                PlusLabelLayout.Name,
                errors);
        }

        context.EquationReadyText = FindOptionalUniqueNamed(
            targetScene,
            ReadyTextLayout.Name,
            errors);
        ValidateOptionalTextObject(
            context.EquationReadyText,
            context.EquationRoot,
            ReadyTextLayout.Name,
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

        ConfigureText(
            plusObject.GetComponent<RectTransform>(),
            plusObject.GetComponent<TMP_Text>(),
            PlusLabelLayout.Position,
            PlusLabelLayout.Size,
            PlusLabelLayout.FontSize,
            HorizontalAlignmentOptions.Center,
            setVerticalMiddle: true,
            content: PlusLabelLayout.Text,
            setOverflow: true);
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
            new Vector2(500f, -80f),
            new Vector2(280f, 140f),
            92f,
            HorizontalAlignmentOptions.Center,
            setVerticalMiddle: true,
            content: "= 5",
            setOverflow: true);

        ConfigureSupportText(
            context.EntryProgressText,
            new Vector2(-650f, 90f),
            new Vector2(520f, 40f),
            22f);
        ConfigureSupportText(
            context.CurrentValuesText,
            new Vector2(-650f, 48f),
            new Vector2(570f, 40f),
            23f);
        ConfigureSupportText(
            context.AcceptedDigitsText,
            new Vector2(-650f, 6f),
            new Vector2(520f, 40f),
            23f);
        ConfigureSupportText(
            context.FeedbackText,
            new Vector2(-650f, -42f),
            new Vector2(590f, 56f),
            21f);

        ConfigureText(
            context.EquationInstructionText.GetComponent<RectTransform>(),
            context.EquationInstructionText,
            new Vector2(0f, -290f),
            new Vector2(1250f, 60f),
            18f,
            HorizontalAlignmentOptions.Center,
            setVerticalMiddle: true,
            content: PhaseTwoInstruction,
            setOverflow: true);

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
        ConfigureAsNonInteractive(context.CountText);

        ConfigureTrayLayout(context.InventoryTray);
        GameObject slotsRoot = CreateOrRepairSlotsRoot(context);
        GameObject slot01 = CreateOrRepairBufferSlot(
            context.BufferSlotVisual01,
            context.TokenVisualObject,
            slotsRoot.transform,
            "BufferSlotVisual_01",
            new Vector3(-0.75f, 0.15f, 0f));
        GameObject slot02 = CreateOrRepairBufferSlot(
            context.BufferSlotVisual02,
            context.TokenVisualObject,
            slotsRoot.transform,
            "BufferSlotVisual_02",
            new Vector3(0.75f, 0.15f, 0f));

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
                new Vector2(-0.75f, 0.15f);
        RequireSerializedProperty(
            serializedTray,
            "tokenSpacing").vector2Value =
                new Vector2(1.5f, 0f);
        serializedTray.ApplyModifiedProperties();
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
        GameObject sourceVisual,
        Transform parent,
        string objectName,
        Vector3 localPosition)
    {
        GameObject slotObject = existingObject;

        if (slotObject == null)
        {
            slotObject = Object.Instantiate(sourceVisual);
            slotObject.name = objectName;
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
        slotObject.transform.localScale =
            sourceVisual.transform.localScale;

        InventorySegmentToken[] tokenComponents =
            slotObject.GetComponentsInChildren<InventorySegmentToken>(true);

        foreach (InventorySegmentToken component in tokenComponents)
        {
            Undo.DestroyObjectImmediate(component);
        }

        InventoryDropZone[] dropZones =
            slotObject.GetComponentsInChildren<InventoryDropZone>(true);

        foreach (InventoryDropZone component in dropZones)
        {
            Undo.DestroyObjectImmediate(component);
        }

        foreach (Collider2D collider in
            slotObject.GetComponentsInChildren<Collider2D>(true))
        {
            Undo.RecordObject(collider, UndoName);
            collider.enabled = false;
        }

        foreach (Rigidbody2D rigidbody in
            slotObject.GetComponentsInChildren<Rigidbody2D>(true))
        {
            Undo.RecordObject(rigidbody, UndoName);
            rigidbody.simulated = false;
        }

        foreach (MonoBehaviour behaviour in
            slotObject.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour == null)
            {
                continue;
            }

            Undo.RecordObject(behaviour, UndoName);
            behaviour.enabled = false;
        }

        SpriteRenderer[] sourceRenderers =
            sourceVisual.GetComponentsInChildren<SpriteRenderer>(true);
        SpriteRenderer[] slotRenderers =
            slotObject.GetComponentsInChildren<SpriteRenderer>(true);

        if (sourceRenderers.Length != slotRenderers.Length)
        {
            throw new InvalidOperationException(
                $"{objectName} SpriteRenderer structure does not match " +
                "tokenTemplate.VisualObject.");
        }

        for (int i = 0; i < slotRenderers.Length; i++)
        {
            SpriteRenderer sourceRenderer = sourceRenderers[i];
            SpriteRenderer slotRenderer = slotRenderers[i];
            Color sourceColor = sourceRenderer.color;

            Undo.RecordObject(slotRenderer, UndoName);
            slotRenderer.sprite = sourceRenderer.sprite;
            slotRenderer.sharedMaterial = sourceRenderer.sharedMaterial;
            slotRenderer.sortingLayerID =
                sourceRenderer.sortingLayerID;
            slotRenderer.enabled = true;
            slotRenderer.color = new Color(
                sourceColor.r,
                sourceColor.g,
                sourceColor.b,
                0.18f);
            slotRenderer.sortingOrder =
                sourceRenderer.sortingOrder - 1;
        }

        SetActiveWithUndo(slotObject, true);
        return slotObject;
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
            context.EquationInstructionText,
            new StaticLabelLayout(
                context.EquationInstructionText.gameObject.name,
                PhaseTwoInstruction,
                new Vector2(0f, -290f),
                new Vector2(1250f, 60f),
                18f),
            PhaseTwoInstruction,
            context.EquationInstructionText.gameObject.activeSelf,
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

        ValidateTrayLayout(context.InventoryTray, errors);
        ValidateSlotVisual(
            slot01,
            context.TokenVisualObject,
            "BufferSlotVisual_01",
            errors);
        ValidateSlotVisual(
            slot02,
            context.TokenVisualObject,
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
            context.TokenVisualObject,
            "BufferSlotVisual_01",
            errors);
        ValidateOptionalSlot(
            context.BufferSlotVisual02,
            context.BufferCapacitySlotsRoot,
            context.TokenVisualObject,
            "BufferSlotVisual_02",
            errors);
    }

    private static void ValidateOptionalSlot(
        GameObject slotObject,
        GameObject slotsRoot,
        GameObject sourceVisual,
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

        ValidateSlotRendererStructure(
            slotObject,
            sourceVisual,
            objectName,
            errors);
    }

    private static void ValidateSlotRendererStructure(
        GameObject slotObject,
        GameObject sourceVisual,
        string objectName,
        List<string> errors)
    {
        if (slotObject == null || sourceVisual == null)
        {
            return;
        }

        int sourceRendererCount =
            sourceVisual.GetComponentsInChildren<SpriteRenderer>(true).Length;
        int slotRendererCount =
            slotObject.GetComponentsInChildren<SpriteRenderer>(true).Length;

        if (slotRendererCount != sourceRendererCount)
        {
            errors.Add(
                $"{objectName} must contain the same SpriteRenderer " +
                "structure as tokenTemplate.VisualObject.");
        }
    }

    private static void ValidateSlotVisual(
        GameObject slotObject,
        GameObject sourceVisual,
        string objectName,
        List<string> errors)
    {
        ValidateSlotRendererStructure(
            slotObject,
            sourceVisual,
            objectName,
            errors);

        Vector3 expectedPosition =
            objectName == "BufferSlotVisual_01"
                ? new Vector3(-0.75f, 0.15f, 0f)
                : new Vector3(0.75f, 0.15f, 0f);

        if (slotObject.transform.localPosition != expectedPosition ||
            slotObject.transform.localRotation != Quaternion.identity ||
            slotObject.transform.localScale !=
                sourceVisual.transform.localScale ||
            !slotObject.activeSelf)
        {
            errors.Add($"{objectName} transform or active state is invalid.");
        }

        if (slotObject
                .GetComponentsInChildren<InventorySegmentToken>(true)
                .Length > 0 ||
            slotObject
                .GetComponentsInChildren<InventoryDropZone>(true)
                .Length > 0)
        {
            errors.Add(
                $"{objectName} contains a prohibited gameplay component.");
        }

        foreach (Collider2D collider in
            slotObject.GetComponentsInChildren<Collider2D>(true))
        {
            if (collider.enabled)
            {
                errors.Add(
                    $"{objectName} contains an enabled Collider2D.");
            }
        }

        foreach (Rigidbody2D rigidbody in
            slotObject.GetComponentsInChildren<Rigidbody2D>(true))
        {
            if (rigidbody.simulated)
            {
                errors.Add(
                    $"{objectName} contains a simulated Rigidbody2D.");
            }
        }

        foreach (MonoBehaviour behaviour in
            slotObject.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour != null && behaviour.enabled)
            {
                errors.Add(
                    $"{objectName} contains an enabled gameplay " +
                    "MonoBehaviour.");
            }
        }

        SpriteRenderer[] sourceRenderers =
            sourceVisual.GetComponentsInChildren<SpriteRenderer>(true);
        SpriteRenderer[] slotRenderers =
            slotObject.GetComponentsInChildren<SpriteRenderer>(true);
        int rendererCount =
            Mathf.Min(sourceRenderers.Length, slotRenderers.Length);

        for (int i = 0; i < rendererCount; i++)
        {
            Color sourceColor = sourceRenderers[i].color;
            Color slotColor = slotRenderers[i].color;

            if (slotRenderers[i].sprite != sourceRenderers[i].sprite ||
                slotRenderers[i].sharedMaterial !=
                    sourceRenderers[i].sharedMaterial ||
                slotRenderers[i].sortingLayerID !=
                    sourceRenderers[i].sortingLayerID ||
                !slotRenderers[i].enabled ||
                !Mathf.Approximately(slotColor.r, sourceColor.r) ||
                !Mathf.Approximately(slotColor.g, sourceColor.g) ||
                !Mathf.Approximately(slotColor.b, sourceColor.b) ||
                !Mathf.Approximately(slotColor.a, 0.18f) ||
                slotRenderers[i].sortingOrder >
                    sourceRenderers[i].sortingOrder - 1)
            {
                errors.Add(
                    $"{objectName} SpriteRenderer {i} is not a valid " +
                    "capacity silhouette.");
            }
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
                new Vector2(-0.75f, 0.15f) ||
            RequireSerializedProperty(
                serializedTray,
                "tokenSpacing").vector2Value !=
                new Vector2(1.5f, 0f))
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

        if (!child.transform.IsChildOf(parent.transform))
        {
            errors.Add(
                $"{childLabel} must be a descendant of {parentLabel}; found " +
                $"at {GetHierarchyPath(child)}.");
        }
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
        public SharedSegmentInventory Inventory;
        public InventorySegmentToken TokenTemplate;
        public Transform TokenContainer;
        public TMP_Text CountText;
        public GameObject TokenVisualObject;
        public TMP_Text EntryProgressText;
        public TMP_Text TargetEquationText;
        public TMP_Text CurrentValuesText;
        public TMP_Text AcceptedDigitsText;
        public TMP_Text FeedbackText;
        public TMP_Text EquationInstructionText;
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
