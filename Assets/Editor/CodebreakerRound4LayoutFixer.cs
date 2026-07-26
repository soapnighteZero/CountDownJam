using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CodebreakerRound4LayoutFixer
{
    private const string MenuPath =
        "Tools/Codebreaker/Fix Round 4 Prototype Layout";
    private const string TargetScenePath =
        "Assets/Scenes/CodebreakerPrototypeScene.unity";
    private const string DialogTitle = "Round 4 Prototype Layout Fixer";
    private const string UndoName = "Fix Round 4 Prototype Layout";
    private const string SuccessReport =
        "ROUND 4 PROTOTYPE LAYOUT FIXED\n\n" +
        "2 displays repositioned\n" +
        "1 tray repositioned\n" +
        "6 HUD labels normalized";

    private static readonly HudLayout[] HudLayouts =
    {
        new HudLayout(
            "EntryProgressText",
            new Vector2(-610f, 150f),
            new Vector2(520f, 42f),
            23f,
            HorizontalAlignmentOptions.Left),
        new HudLayout(
            "TargetEquationText",
            new Vector2(-610f, 105f),
            new Vector2(520f, 50f),
            31f,
            HorizontalAlignmentOptions.Left),
        new HudLayout(
            "CurrentValuesText",
            new Vector2(-610f, 60f),
            new Vector2(520f, 42f),
            25f,
            HorizontalAlignmentOptions.Left),
        new HudLayout(
            "AcceptedDigitsText",
            new Vector2(-610f, 15f),
            new Vector2(520f, 42f),
            25f,
            HorizontalAlignmentOptions.Left),
        new HudLayout(
            "EquationFeedbackText",
            new Vector2(-610f, -35f),
            new Vector2(560f, 50f),
            22f,
            HorizontalAlignmentOptions.Left),
        new HudLayout(
            "EquationInstructionText",
            new Vector2(0f, -445f),
            new Vector2(1200f, 72f),
            19f,
            HorizontalAlignmentOptions.Center)
    };

    [MenuItem(MenuPath)]
    private static void FixRound4PrototypeLayout()
    {
        Scene targetScene;

        if (!TryGetRunnableTargetScene(out targetScene, out string refusal))
        {
            ReportRefusal(refusal);
            return;
        }

        List<string> errors = new List<string>();
        LayoutContext context = ValidateScene(targetScene, errors);

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
            ApplyLayout(context);
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
                $"Round 4 prototype layout was not saved: " +
                $"{exception.Message}");
            EditorUtility.DisplayDialog(
                DialogTitle,
                "ROUND 4 PROTOTYPE LAYOUT FIX FAILED\n\n" +
                exception.Message,
                "OK");
        }
    }

    [MenuItem(MenuPath, true)]
    private static bool ValidateFixRound4PrototypeLayout()
    {
        if (EditorApplication.isPlaying ||
            EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return false;
        }

        Scene targetScene = SceneManager.GetSceneByPath(TargetScenePath);
        return targetScene.IsValid() && targetScene.isLoaded;
    }

    private static bool TryGetRunnableTargetScene(
        out Scene targetScene,
        out string refusal)
    {
        targetScene = SceneManager.GetSceneByPath(TargetScenePath);

        if (EditorApplication.isPlaying)
        {
            refusal = "The layout fixer cannot run while Unity is in Play Mode.";
            return false;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            refusal =
                "The layout fixer cannot run while Unity is entering Play Mode.";
            return false;
        }

        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            refusal =
                $"Load {TargetScenePath} before running the layout fixer.";
            return false;
        }

        if (SceneManager.GetActiveScene() != targetScene)
        {
            refusal =
                $"{TargetScenePath} must be the active scene before running " +
                "the layout fixer.";
            return false;
        }

        if (targetScene.isDirty)
        {
            refusal =
                "The target scene has unsaved changes. Save or discard them " +
                "before running the layout fixer.";
            return false;
        }

        refusal = null;
        return true;
    }

    private static LayoutContext ValidateScene(
        Scene targetScene,
        List<string> errors)
    {
        LayoutContext context = new LayoutContext
        {
            TargetScene = targetScene,
            WorldRoot = FindUniqueNamed(
                targetScene,
                "EquationEntryWorldRoot",
                errors),
            DisplayA = FindUniqueNamed(targetScene, "Display_A", errors),
            DisplayB = FindUniqueNamed(targetScene, "Display_B", errors),
            Tray = FindUniqueNamed(
                targetScene,
                "SegmentInventoryTray",
                errors),
            HudCanvas = FindUniqueNamed(
                targetScene,
                "CodebreakerHUDCanvas",
                errors),
            HudRoot = FindUniqueNamed(
                targetScene,
                "EquationEntryRoot",
                errors),
            MainCamera = FindUniqueNamed(
                targetScene,
                "Main Camera",
                errors)
        };

        ValidateDescendant(
            context.DisplayA,
            context.WorldRoot,
            "Display_A",
            "EquationEntryWorldRoot",
            errors);
        ValidateDescendant(
            context.DisplayB,
            context.WorldRoot,
            "Display_B",
            "EquationEntryWorldRoot",
            errors);
        ValidateDescendant(
            context.Tray,
            context.WorldRoot,
            "SegmentInventoryTray",
            "EquationEntryWorldRoot",
            errors);
        ValidateDescendant(
            context.HudRoot,
            context.HudCanvas,
            "EquationEntryRoot",
            "CodebreakerHUDCanvas",
            errors);

        RequireComponent<SevenSegmentDisplay>(
            context.DisplayA,
            "Display_A",
            errors);
        RequireComponent<SevenSegmentDisplay>(
            context.DisplayB,
            "Display_B",
            errors);
        RequireComponent<SegmentInventoryTray>(
            context.Tray,
            "SegmentInventoryTray",
            errors);
        RequireComponent<Canvas>(
            context.HudCanvas,
            "CodebreakerHUDCanvas",
            errors);
        RequireComponent<Camera>(
            context.MainCamera,
            "Main Camera",
            errors);

        RequireUniqueComponent<CodebreakerEquationHUD>(
            targetScene,
            errors);
        RequireUniqueComponent<CodebreakerEquationEntryController>(
            targetScene,
            errors);
        RequireUniqueComponent<CodebreakerEquationInteractionController>(
            targetScene,
            errors);
        RequireUniqueComponent<SharedSegmentInventory>(
            targetScene,
            errors);

        foreach (HudLayout layout in HudLayouts)
        {
            GameObject hudObject = FindUniqueNamed(
                targetScene,
                layout.Name,
                errors);

            if (hudObject == null)
            {
                continue;
            }

            ValidateDescendant(
                hudObject,
                context.HudRoot,
                layout.Name,
                "CodebreakerHUDCanvas/EquationEntryRoot",
                errors);

            RectTransform rectTransform = RequireComponent<RectTransform>(
                hudObject,
                layout.Name,
                errors);
            TMP_Text text = RequireComponent<TMP_Text>(
                hudObject,
                layout.Name,
                errors);

            if (rectTransform != null && text != null)
            {
                context.HudObjects.Add(
                    new HudTarget(layout, rectTransform, text));
            }
        }

        return context;
    }

    private static void ApplyLayout(LayoutContext context)
    {
        ConfigureWorldTransform(
            context.DisplayA.transform,
            new Vector3(-3f, -1.05f, 0f),
            new Vector3(0.62f, 0.62f, 1f));
        ConfigureWorldTransform(
            context.DisplayB.transform,
            new Vector3(3f, -1.05f, 0f),
            new Vector3(0.62f, 0.62f, 1f));
        ConfigureWorldTransform(
            context.Tray.transform,
            new Vector3(0f, -3.75f, 0f),
            new Vector3(0.78f, 0.78f, 1f));

        foreach (HudTarget target in context.HudObjects)
        {
            ConfigureHud(target);
        }
    }

    private static void ConfigureWorldTransform(
        Transform transform,
        Vector3 localPosition,
        Vector3 localScale)
    {
        Undo.RecordObject(transform, UndoName);
        transform.localPosition = localPosition;
        transform.localRotation = Quaternion.identity;
        transform.localScale = localScale;
    }

    private static void ConfigureHud(HudTarget target)
    {
        Undo.RecordObject(target.RectTransform, UndoName);
        target.RectTransform.localRotation = Quaternion.identity;
        target.RectTransform.localScale = Vector3.one;
        target.RectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        target.RectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        target.RectTransform.pivot = new Vector2(0.5f, 0.5f);
        target.RectTransform.anchoredPosition = target.Layout.Position;
        target.RectTransform.sizeDelta = target.Layout.Size;

        Undo.RecordObject(target.Text, UndoName);
        target.Text.fontSize = target.Layout.FontSize;
        target.Text.horizontalAlignment = target.Layout.HorizontalAlignment;
        target.Text.verticalAlignment = VerticalAlignmentOptions.Middle;
        target.Text.raycastTarget = false;
        target.Text.enableAutoSizing = false;
        target.Text.overflowMode = TextOverflowModes.Overflow;
    }

    private static GameObject FindUniqueNamed(
        Scene scene,
        string objectName,
        List<string> errors)
    {
        List<GameObject> matches = new List<GameObject>();

        foreach (GameObject gameObject in GetSceneGameObjects(scene))
        {
            if (gameObject.name == objectName)
            {
                matches.Add(gameObject);
            }
        }

        if (matches.Count != 1)
        {
            errors.Add(
                $"{scene.path} must contain exactly one {objectName}; " +
                $"found {matches.Count}.");
            return null;
        }

        return matches[0];
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
            "ROUND 4 PROTOTYPE LAYOUT VALIDATION FAILED\n\n- " +
            string.Join("\n- ", errors);
        Debug.LogError(report);
        EditorUtility.DisplayDialog(DialogTitle, report, "OK");
    }

    private sealed class LayoutContext
    {
        public Scene TargetScene;
        public GameObject WorldRoot;
        public GameObject DisplayA;
        public GameObject DisplayB;
        public GameObject Tray;
        public GameObject HudCanvas;
        public GameObject HudRoot;
        public GameObject MainCamera;
        public readonly List<HudTarget> HudObjects =
            new List<HudTarget>();
    }

    private sealed class HudTarget
    {
        public HudLayout Layout { get; }
        public RectTransform RectTransform { get; }
        public TMP_Text Text { get; }

        public HudTarget(
            HudLayout layout,
            RectTransform rectTransform,
            TMP_Text text)
        {
            Layout = layout;
            RectTransform = rectTransform;
            Text = text;
        }
    }

    private struct HudLayout
    {
        public string Name { get; }
        public Vector2 Position { get; }
        public Vector2 Size { get; }
        public float FontSize { get; }
        public HorizontalAlignmentOptions HorizontalAlignment { get; }

        public HudLayout(
            string name,
            Vector2 position,
            Vector2 size,
            float fontSize,
            HorizontalAlignmentOptions horizontalAlignment)
        {
            Name = name;
            Position = position;
            Size = size;
            FontSize = fontSize;
            HorizontalAlignment = horizontalAlignment;
        }
    }
}
