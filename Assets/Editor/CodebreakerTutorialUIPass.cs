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
    private const string PhaseOneInstruction =
        "CLICK A SEGMENT TO REMOVE ONE LAYER\n" +
        "USE ALL 4 HITS - LEAVE ONE VALID GREEN DIGIT\n" +
        "RED > YELLOW > GREEN > OFF   |   DOTS = LAYERS LEFT";
    private const string PhaseTwoInstruction =
        "MOVE SEGMENTS BETWEEN THE TWO DISPLAYS AND BUFFER\n" +
        "MAKE THE EQUATION TRUE, THEN PRESS SPACE";
    private const string SuccessReport =
        "CODEBREAKER TUTORIAL UI LAYOUT REFINED\n\n" +
        "Redundant A and B labels removed\n" +
        "Large equation vertically aligned\n" +
        "Equals target resized and repositioned\n" +
        "Support text moved down\n" +
        "Instruction moved above buffer";

    private static readonly StaticLabelLayout PlusLabelLayout =
        new StaticLabelLayout(
            "EquationPlusText",
            "+",
            new Vector2(0f, -80f),
            new Vector2(130f, 140f),
            96f);

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

        GameObject plusObject = context.EquationPlusText;

        if (plusObject == null)
        {
            plusObject = Object.Instantiate(
                context.TargetEquationText.gameObject);
            plusObject.name = PlusLabelLayout.Name;
            Undo.RegisterCreatedObjectUndo(
                plusObject,
                $"Create {PlusLabelLayout.Name}");
        }

        if (plusObject.transform.parent != context.EquationRoot.transform)
        {
            Undo.SetTransformParent(
                plusObject.transform,
                context.EquationRoot.transform,
                $"Parent {PlusLabelLayout.Name}");
        }

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
            new Vector2(0f, -325f),
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
