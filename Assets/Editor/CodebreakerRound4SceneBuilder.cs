using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class CodebreakerRound4SceneBuilder
{
    private const string MenuPath =
        "Tools/Codebreaker/Build Round 4 Equation Scene";
    private const string TargetScenePath =
        "Assets/Scenes/CodebreakerPrototypeScene.unity";
    private const string SourceScenePath =
        "Assets/Scenes/EquationPrototypeScene.unity";
    private const string DialogTitle = "Round 4 Equation Scene Builder";
    private const string AlreadyBuiltMessage =
        "ROUND 4 EQUATION SCENE ALREADY EXISTS OR IS PARTIALLY BUILT";

    private static readonly string[] IdempotencyNames =
    {
        "EquationEntryWorldRoot",
        "EntryProgressText",
        "TargetEquationText",
        "CurrentValuesText",
        "AcceptedDigitsText",
        "EquationFeedbackText",
        "EquationInstructionText"
    };

    [MenuItem(MenuPath)]
    private static void BuildRound4EquationScene()
    {
        Scene sourceScene = default;
        bool sourceOpened = false;
        bool targetMutated = false;
        bool saved = false;
        int undoGroup = -1;
        string successReport = null;

        try
        {
            Scene targetScene = SceneManager.GetSceneByPath(TargetScenePath);
            List<string> errors = new List<string>();

            if (!targetScene.IsValid() || !targetScene.isLoaded)
            {
                errors.Add(
                    $"Open {TargetScenePath} before running the builder.");
            }
            else if (targetScene.isDirty)
            {
                errors.Add(
                    "The target scene has unsaved changes. Save or discard " +
                    "them before running the builder.");
            }

            if (SceneManager.GetSceneByPath(SourceScenePath).isLoaded)
            {
                errors.Add(
                    $"{SourceScenePath} is already open. Close it before " +
                    "running the builder.");
            }

            if (errors.Count > 0)
            {
                throw new BuildException("Preflight validation failed.", errors);
            }

            sourceScene = EditorSceneManager.OpenScene(
                SourceScenePath,
                OpenSceneMode.Additive);
            sourceOpened = true;

            BuildContext context = ValidateBeforeMutation(
                targetScene,
                sourceScene);

            Undo.IncrementCurrentGroup();
            undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Build Round 4 Equation Scene");
            targetMutated = true;

            BuildTarget(context);
            context.WorldRoot.SetActive(false);

            List<string> finalErrors = ValidateBuiltScene(
                targetScene,
                context.WorldRoot);

            if (finalErrors.Count > 0)
            {
                throw new BuildException(
                    "Post-build scene validation failed.",
                    finalErrors);
            }

            EditorSceneManager.MarkSceneDirty(targetScene);

            if (!EditorSceneManager.SaveScene(
                targetScene,
                TargetScenePath,
                false))
            {
                throw new BuildException(
                    "Unity could not save the target scene.",
                    new List<string>
                    {
                        $"SaveScene returned false for {TargetScenePath}."
                    });
            }

            Undo.CollapseUndoOperations(undoGroup);
            saved = true;
            successReport =
                "Built Round 4 Equation Entry: 2 displays, 14 segments, " +
                "1 shared tray, 6 HUD labels, and 3 controller components.";
        }
        catch (BuildException exception)
        {
            if (targetMutated && !saved && undoGroup >= 0)
            {
                Undo.RevertAllDownToGroup(undoGroup);
            }

            ReportFailure(exception.Message, exception.Errors);
        }
        catch (Exception exception)
        {
            if (targetMutated && !saved && undoGroup >= 0)
            {
                Undo.RevertAllDownToGroup(undoGroup);
            }

            Debug.LogException(exception);
            EditorUtility.DisplayDialog(
                DialogTitle,
                $"Build failed: {exception.Message}",
                "OK");
        }
        finally
        {
            if (sourceOpened && sourceScene.IsValid() && sourceScene.isLoaded)
            {
                EditorSceneManager.CloseScene(sourceScene, true);
            }
        }

        if (saved)
        {
            Debug.Log(successReport);
            EditorUtility.DisplayDialog(DialogTitle, successReport, "OK");
        }
    }

    private static BuildContext ValidateBeforeMutation(
        Scene targetScene,
        Scene sourceScene)
    {
        List<string> errors = new List<string>();
        BuildContext context = new BuildContext
        {
            TargetScene = targetScene,
            SourceScene = sourceScene
        };

        ValidateRuntimeTypes(errors);
        ValidateIdempotency(targetScene, errors);

        context.SourceDisplayA = FindUnique(
            sourceScene,
            "Display_A",
            errors);
        context.SourceDisplayB = FindUnique(
            sourceScene,
            "Display_B",
            errors);
        context.SourceTray = FindUnique(
            sourceScene,
            "SegmentInventoryTray",
            errors);

        ValidateSourceDisplay(context.SourceDisplayA, "Display_A", errors);
        ValidateSourceDisplay(context.SourceDisplayB, "Display_B", errors);
        ValidateSourceTray(context.SourceTray, errors);

        context.MainCameraObject = FindUnique(
            targetScene,
            "Main Camera",
            errors);
        context.GameManager = FindUnique(
            targetScene,
            "CodebreakerGameManager",
            errors);
        context.HudCanvas = FindUnique(
            targetScene,
            "CodebreakerHUDCanvas",
            errors);
        context.UiEquationRoot = FindUnique(
            targetScene,
            "EquationEntryRoot",
            errors);
        context.Placeholder = FindUnique(
            targetScene,
            "EquationPlaceholder",
            errors);
        context.EventSystemObject = FindUnique(
            targetScene,
            "EventSystem",
            errors);

        context.MainCamera = RequireComponent<Camera>(
            context.MainCameraObject,
            "Main Camera",
            errors);
        context.GameController =
            RequireUniqueComponent<CodebreakerGameController>(
                targetScene,
                errors);
        ValidateGameControllerHierarchy(
            context.GameManager,
            context.GameController,
            errors);
        context.CodeDisplay = RequireUniqueComponent<CodeSequenceDisplay>(
            targetScene,
            errors);
        RequireUniqueComponent<GlobalBombTimer>(targetScene, errors);
        RequireComponent<Canvas>(
            context.HudCanvas,
            "CodebreakerHUDCanvas",
            errors);
        RequireComponent<EventSystem>(
            context.EventSystemObject,
            "EventSystem",
            errors);
        ValidateTargetBaseline(targetScene, errors);

        if (context.GameManager != null && !context.GameManager.activeSelf)
        {
            errors.Add("CodebreakerGameManager must be active.");
        }

        if (context.HudCanvas != null &&
            context.UiEquationRoot != null &&
            !context.UiEquationRoot.transform.IsChildOf(
                context.HudCanvas.transform))
        {
            errors.Add(
                "EquationEntryRoot must be beneath CodebreakerHUDCanvas.");
        }

        if (context.UiEquationRoot != null &&
            context.Placeholder != null &&
            !context.Placeholder.transform.IsChildOf(
                context.UiEquationRoot.transform))
        {
            errors.Add(
                "EquationPlaceholder must be beneath EquationEntryRoot.");
        }

        context.PlaceholderText = RequireComponent<TMP_Text>(
            context.Placeholder,
            "EquationPlaceholder",
            errors);

        if (errors.Count > 0)
        {
            string message = errors.Exists(
                error => error.StartsWith(AlreadyBuiltMessage))
                ? AlreadyBuiltMessage
                : "Required source or target validation failed.";
            throw new BuildException(message, errors);
        }

        return context;
    }

    private static void ValidateGameControllerHierarchy(
        GameObject gameManager,
        CodebreakerGameController gameController,
        List<string> errors)
    {
        if (gameManager == null || gameController == null)
        {
            return;
        }

        Transform controllerTransform = gameController.transform;

        if (gameController.gameObject != gameManager &&
            !controllerTransform.IsChildOf(gameManager.transform))
        {
            errors.Add(
                "CodebreakerGameController must be on " +
                "CodebreakerGameManager or one of its descendants; found at " +
                $"{GetHierarchyPath(gameController.gameObject)}.");
        }
    }

    private static void ValidateTargetBaseline(
        Scene targetScene,
        List<string> errors)
    {
        RequireCount<Camera>(targetScene, 1, errors);
        RequireCount<Canvas>(targetScene, 1, errors);
        RequireCount<GraphicRaycaster>(targetScene, 1, errors);
        RequireCount<EventSystem>(targetScene, 1, errors);
        RequireCount<InputSystemUIInputModule>(targetScene, 1, errors);
        RequireCount<StandaloneInputModule>(targetScene, 0, errors);
        RequireCount<EquationBombController>(targetScene, 0, errors);
        RequireCount<EquationBombHUD>(targetScene, 0, errors);
        RequireCount<CodeModuleController>(targetScene, 0, errors);
        ValidateMissingScripts(targetScene, errors);
        ValidateCrossSceneReferences(targetScene, errors);
        ValidateTemporaryReferences(targetScene, errors);
    }

    private static void ValidateRuntimeTypes(List<string> errors)
    {
        Type[] requiredTypes =
        {
            typeof(CodebreakerEquationEntryController),
            typeof(CodebreakerEquationInteractionController),
            typeof(CodebreakerEquationHUD),
            typeof(CodebreakerEquationMathUtility),
            typeof(SharedSegmentInventory)
        };

        foreach (Type type in requiredTypes)
        {
            if (type == null)
            {
                errors.Add("A required Round 4 runtime type is unavailable.");
            }
        }
    }

    private static void ValidateIdempotency(
        Scene targetScene,
        List<string> errors)
    {
        foreach (string objectName in IdempotencyNames)
        {
            if (FindAllNamed(targetScene, objectName).Count > 0)
            {
                errors.Add(
                    $"{AlreadyBuiltMessage}: found {objectName}.");
            }
        }

        Type[] componentTypes =
        {
            typeof(CodebreakerEquationEntryController),
            typeof(CodebreakerEquationInteractionController),
            typeof(CodebreakerEquationHUD),
            typeof(SharedSegmentInventory)
        };

        foreach (Type componentType in componentTypes)
        {
            if (GetSceneComponents(targetScene, componentType).Count > 0)
            {
                errors.Add(
                    $"{AlreadyBuiltMessage}: found {componentType.Name}.");
            }
        }
    }

    private static void ValidateSourceDisplay(
        GameObject displayObject,
        string label,
        List<string> errors)
    {
        if (displayObject == null)
        {
            return;
        }

        RequireComponent<SevenSegmentDisplay>(
            displayObject,
            label,
            errors);
        SevenSegmentPiece[] pieces =
            displayObject.GetComponentsInChildren<SevenSegmentPiece>(true);

        if (pieces.Length != 7)
        {
            errors.Add(
                $"{label} must contain exactly seven SevenSegmentPiece " +
                $"components; found {pieces.Length}.");
        }

        foreach (SevenSegmentPiece piece in pieces)
        {
            if (piece.GetComponentInChildren<Collider2D>(true) == null)
            {
                errors.Add(
                    $"{label}/{piece.gameObject.name} has no Collider2D.");
            }
        }

        ValidateExcludedComponents(displayObject, label, errors);
    }

    private static void ValidateSourceTray(
        GameObject trayObject,
        List<string> errors)
    {
        if (trayObject == null)
        {
            return;
        }

        RequireComponent<SegmentInventoryTray>(
            trayObject,
            "SegmentInventoryTray",
            errors);

        if (trayObject.GetComponentInChildren<InventoryDropZone>(true) == null)
        {
            errors.Add(
                "SegmentInventoryTray has no InventoryDropZone descendant.");
        }

        if (trayObject.GetComponentInChildren<InventorySegmentToken>(true) ==
            null)
        {
            errors.Add(
                "SegmentInventoryTray has no InventorySegmentToken template.");
        }

        ValidateExcludedComponents(trayObject, "SegmentInventoryTray", errors);
    }

    private static void ValidateExcludedComponents(
        GameObject root,
        string label,
        List<string> errors)
    {
        if (root.GetComponentInChildren<EquationBombController>(true) != null ||
            root.GetComponentInChildren<EquationBombHUD>(true) != null ||
            root.GetComponentInChildren<CodeModuleController>(true) != null)
        {
            errors.Add(
                $"{label} contains an explicitly excluded legacy component.");
        }
    }

    private static void BuildTarget(BuildContext context)
    {
        context.WorldRoot = new GameObject("EquationEntryWorldRoot");
        Undo.RegisterCreatedObjectUndo(
            context.WorldRoot,
            "Create Equation Entry World Root");
        SceneManager.MoveGameObjectToScene(
            context.WorldRoot,
            context.TargetScene);

        GameObject displayAObject = CloneIntoTarget(
            context.SourceDisplayA,
            "Display_A",
            context.WorldRoot.transform,
            context.TargetScene);
        GameObject displayBObject = CloneIntoTarget(
            context.SourceDisplayB,
            "Display_B",
            context.WorldRoot.transform,
            context.TargetScene);
        GameObject trayObject = CloneIntoTarget(
            context.SourceTray,
            "SegmentInventoryTray",
            context.WorldRoot.transform,
            context.TargetScene);

        SevenSegmentDisplay displayA =
            displayAObject.GetComponent<SevenSegmentDisplay>();
        SevenSegmentDisplay displayB =
            displayBObject.GetComponent<SevenSegmentDisplay>();
        SegmentInventoryTray tray =
            trayObject.GetComponent<SegmentInventoryTray>();

        SharedSegmentInventory inventory =
            Undo.AddComponent<SharedSegmentInventory>(context.GameManager);
        CodebreakerEquationInteractionController interaction =
            Undo.AddComponent<CodebreakerEquationInteractionController>(
                context.GameManager);
        CodebreakerEquationEntryController entry =
            Undo.AddComponent<CodebreakerEquationEntryController>(
                context.GameManager);
        CodebreakerEquationHUD hud =
            Undo.AddComponent<CodebreakerEquationHUD>(context.UiEquationRoot);

        AssignObject(tray, "inventory", inventory);
        AssignObject(interaction, "worldCamera", context.MainCamera);
        AssignObject(interaction, "inventory", inventory);

        TMP_Text[] hudTexts =
        {
            CreateTextClone(
                context,
                "EntryProgressText",
                100f,
                "ENTRY DIGIT 1 OF 3"),
            CreateTextClone(
                context,
                "TargetEquationText",
                55f,
                "A + B = 5"),
            CreateTextClone(
                context,
                "CurrentValuesText",
                15f,
                "A 3 + B 8 = 11"),
            CreateTextClone(
                context,
                "AcceptedDigitsText",
                -25f,
                "ENTERED  _ _ _"),
            CreateTextClone(
                context,
                "EquationFeedbackText",
                -65f,
                string.Empty),
            CreateTextClone(
                context,
                "EquationInstructionText",
                -115f,
                "DRAG SEGMENTS BETWEEN A, B, AND THE TRAY\n" +
                "PRESS SPACE TO SUBMIT")
        };

        AssignObject(hud, "entryProgressText", hudTexts[0]);
        AssignObject(hud, "targetEquationText", hudTexts[1]);
        AssignObject(hud, "currentValuesText", hudTexts[2]);
        AssignObject(hud, "acceptedDigitsText", hudTexts[3]);
        AssignObject(hud, "feedbackText", hudTexts[4]);
        AssignObject(hud, "instructionText", hudTexts[5]);

        AssignObject(entry, "gameController", context.GameController);
        AssignObject(entry, "codeSequenceDisplay", context.CodeDisplay);
        AssignObject(entry, "displayA", displayA);
        AssignObject(entry, "displayB", displayB);
        AssignObject(entry, "sharedInventory", inventory);
        AssignObject(entry, "interactionController", interaction);
        AssignObject(entry, "equationHUD", hud);
        AssignObject(entry, "equationWorldRoot", context.WorldRoot);
        AssignInteger(entry, "startingDigitA", 3);
        AssignInteger(entry, "startingDigitB", 8);
        AssignInteger(entry, "totalPhysicalSegments", 12);
        AssignFloat(entry, "successAdvanceDelaySeconds", 0.6f);

        Undo.DestroyObjectImmediate(context.Placeholder);
    }

    private static GameObject CloneIntoTarget(
        GameObject source,
        string cloneName,
        Transform parent,
        Scene targetScene)
    {
        GameObject clone = Object.Instantiate(source);
        clone.name = cloneName;
        Undo.RegisterCreatedObjectUndo(clone, $"Clone {cloneName}");
        Undo.SetTransformParent(clone.transform, null, $"Unparent {cloneName}");
        SceneManager.MoveGameObjectToScene(clone, targetScene);
        Undo.SetTransformParent(clone.transform, parent, "Parent Equation Object");
        return clone;
    }

    private static TMP_Text CreateTextClone(
        BuildContext context,
        string objectName,
        float anchoredY,
        string initialText)
    {
        GameObject clone = Object.Instantiate(context.Placeholder);
        clone.name = objectName;
        Undo.RegisterCreatedObjectUndo(clone, $"Create {objectName}");
        Undo.SetTransformParent(clone.transform, null, $"Unparent {objectName}");
        SceneManager.MoveGameObjectToScene(clone, context.TargetScene);
        Undo.SetTransformParent(
            clone.transform,
            context.UiEquationRoot.transform,
            $"Parent {objectName}");

        RectTransform rectTransform = clone.GetComponent<RectTransform>();
        TMP_Text text = clone.GetComponent<TMP_Text>();

        if (rectTransform == null || text == null)
        {
            throw new BuildException(
                $"Could not create {objectName}.",
                new List<string>
                {
                    "EquationPlaceholder clone lacks RectTransform or TMP_Text."
                });
        }

        Undo.RecordObject(rectTransform, $"Position {objectName}");
        rectTransform.anchoredPosition = new Vector2(
            rectTransform.anchoredPosition.x,
            anchoredY);
        Undo.RecordObject(text, $"Configure {objectName}");
        text.text = initialText;
        text.raycastTarget = false;
        return text;
    }

    private static List<string> ValidateBuiltScene(
        Scene targetScene,
        GameObject worldRoot)
    {
        List<string> errors = new List<string>();
        RequireCount<Camera>(targetScene, 1, errors);
        RequireCount<Canvas>(targetScene, 1, errors);
        RequireCount<GraphicRaycaster>(targetScene, 1, errors);
        RequireCount<EventSystem>(targetScene, 1, errors);
        RequireCount<InputSystemUIInputModule>(targetScene, 1, errors);
        RequireCount<StandaloneInputModule>(targetScene, 0, errors);
        RequireCount<SevenSegmentDisplay>(targetScene, 2, errors);
        RequireCount<SevenSegmentPiece>(targetScene, 14, errors);
        RequireCount<SharedSegmentInventory>(targetScene, 1, errors);
        RequireCount<CodebreakerEquationEntryController>(
            targetScene,
            1,
            errors);
        RequireCount<CodebreakerEquationInteractionController>(
            targetScene,
            1,
            errors);
        RequireCount<CodebreakerEquationHUD>(targetScene, 1, errors);
        RequireCount<EquationBombController>(targetScene, 0, errors);
        RequireCount<EquationBombHUD>(targetScene, 0, errors);
        RequireCount<CodeModuleController>(targetScene, 0, errors);

        if (FindAllNamed(targetScene, "Main Camera").Count != 1)
        {
            errors.Add("Target scene must contain exactly one Main Camera.");
        }

        ValidateMissingScripts(targetScene, errors);
        ValidateCrossSceneReferences(targetScene, errors);
        ValidateTemporaryReferences(targetScene, errors);

        Canvas canvas = RequireUniqueComponent<Canvas>(targetScene, errors);

        if (worldRoot == null)
        {
            errors.Add("EquationEntryWorldRoot was not created.");
        }
        else
        {
            if (worldRoot.activeSelf)
            {
                errors.Add("EquationEntryWorldRoot must be inactive.");
            }

            if (canvas != null &&
                worldRoot.transform.IsChildOf(canvas.transform))
            {
                errors.Add(
                    "EquationEntryWorldRoot must remain outside the Canvas.");
            }
        }

        foreach (CodebreakerEquationEntryController controller in
            GetSceneComponents<CodebreakerEquationEntryController>(targetScene))
        {
            if (!controller.gameObject.activeInHierarchy)
            {
                errors.Add(
                    "CodebreakerEquationEntryController must remain active.");
            }
        }

        foreach (CodebreakerEquationInteractionController controller in
            GetSceneComponents<CodebreakerEquationInteractionController>(
                targetScene))
        {
            if (!controller.gameObject.activeInHierarchy)
            {
                errors.Add(
                    "CodebreakerEquationInteractionController must remain active.");
            }
        }

        return errors;
    }

    private static void ValidateMissingScripts(
        Scene scene,
        List<string> errors)
    {
        foreach (GameObject gameObject in GetSceneGameObjects(scene))
        {
            foreach (Component component in gameObject.GetComponents<Component>())
            {
                if (component == null)
                {
                    errors.Add(
                        $"Missing Script found on {GetHierarchyPath(gameObject)}.");
                }
            }
        }
    }

    private static void ValidateCrossSceneReferences(
        Scene targetScene,
        List<string> errors)
    {
        foreach (GameObject gameObject in GetSceneGameObjects(targetScene))
        {
            foreach (Component component in gameObject.GetComponents<Component>())
            {
                if (component == null)
                {
                    continue;
                }

                SerializedObject serializedObject = new SerializedObject(
                    component);
                SerializedProperty property =
                    serializedObject.GetIterator();

                while (property.NextVisible(true))
                {
                    if (property.propertyType !=
                        SerializedPropertyType.ObjectReference)
                    {
                        continue;
                    }

                    Object reference = property.objectReferenceValue;

                    if (reference == null || EditorUtility.IsPersistent(reference))
                    {
                        continue;
                    }

                    GameObject referencedObject = reference as GameObject;
                    Component referencedComponent = reference as Component;
                    Scene referencedScene = referencedObject != null
                        ? referencedObject.scene
                        : referencedComponent != null
                            ? referencedComponent.gameObject.scene
                            : default;

                    if (referencedScene.IsValid() &&
                        referencedScene != targetScene)
                    {
                        errors.Add(
                            $"{GetHierarchyPath(gameObject)}.{property.propertyPath} " +
                            $"references scene {referencedScene.path}.");
                    }
                }
            }
        }
    }

    private static void ValidateTemporaryReferences(
        Scene targetScene,
        List<string> errors)
    {
        foreach (GameObject gameObject in GetSceneGameObjects(targetScene))
        {
            foreach (Component component in gameObject.GetComponents<Component>())
            {
                if (component == null)
                {
                    continue;
                }

                SerializedObject serializedObject = new SerializedObject(
                    component);
                SerializedProperty property =
                    serializedObject.GetIterator();

                while (property.NextVisible(true))
                {
                    string value = null;

                    if (property.propertyType ==
                        SerializedPropertyType.String)
                    {
                        value = property.stringValue;
                    }
                    else if (property.propertyType ==
                        SerializedPropertyType.ObjectReference &&
                        property.objectReferenceValue != null)
                    {
                        value = AssetDatabase.GetAssetPath(
                            property.objectReferenceValue);
                    }

                    if (!string.IsNullOrEmpty(value) &&
                        value.IndexOf(".round", StringComparison.OrdinalIgnoreCase)
                        >= 0 &&
                        value.IndexOf("temp", StringComparison.OrdinalIgnoreCase)
                        >= 0)
                    {
                        errors.Add(
                            $"{GetHierarchyPath(gameObject)}.{property.propertyPath} " +
                            $"contains a temporary path: {value}");
                    }
                }
            }
        }
    }

    private static void AssignObject(
        Object target,
        string propertyName,
        Object value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property =
            RequireProperty(serializedObject, propertyName);
        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedProperties();
    }

    private static void AssignInteger(
        Object target,
        string propertyName,
        int value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property =
            RequireProperty(serializedObject, propertyName);
        property.intValue = value;
        serializedObject.ApplyModifiedProperties();
    }

    private static void AssignFloat(
        Object target,
        string propertyName,
        float value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property =
            RequireProperty(serializedObject, propertyName);
        property.floatValue = value;
        serializedObject.ApplyModifiedProperties();
    }

    private static SerializedProperty RequireProperty(
        SerializedObject serializedObject,
        string propertyName)
    {
        serializedObject.Update();
        SerializedProperty property =
            serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            throw new BuildException(
                "Serialized wiring failed.",
                new List<string>
                {
                    $"{serializedObject.targetObject.GetType().Name} has no " +
                    $"serialized property named {propertyName}."
                });
        }

        return property;
    }

    private static GameObject FindUnique(
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

    private static List<Component> GetSceneComponents(
        Scene scene,
        Type componentType)
    {
        List<Component> components = new List<Component>();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            components.AddRange(
                root.GetComponentsInChildren(componentType, true));
        }

        return components;
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

    private static T RequireUniqueComponent<T>(
        Scene scene,
        List<string> errors)
        where T : Component
    {
        List<T> components = GetSceneComponents<T>(scene);

        if (components.Count != 1)
        {
            errors.Add(
                $"{scene.path} must contain exactly one {typeof(T).Name}; " +
                $"found {components.Count}.");
            return null;
        }

        return components[0];
    }

    private static void RequireCount<T>(
        Scene scene,
        int expected,
        List<string> errors)
        where T : Component
    {
        int actual = GetSceneComponents<T>(scene).Count;

        if (actual != expected)
        {
            errors.Add(
                $"Expected {expected} {typeof(T).Name} component(s); " +
                $"found {actual}.");
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

    private static void ReportFailure(
        string summary,
        List<string> errors)
    {
        string details = errors == null || errors.Count == 0
            ? summary
            : $"{summary}\n\n- {string.Join("\n- ", errors)}";
        Debug.LogError(details);
        EditorUtility.DisplayDialog(DialogTitle, details, "OK");
    }

    private sealed class BuildContext
    {
        public Scene TargetScene;
        public Scene SourceScene;
        public GameObject SourceDisplayA;
        public GameObject SourceDisplayB;
        public GameObject SourceTray;
        public GameObject MainCameraObject;
        public GameObject GameManager;
        public GameObject HudCanvas;
        public GameObject UiEquationRoot;
        public GameObject Placeholder;
        public GameObject EventSystemObject;
        public Camera MainCamera;
        public CodebreakerGameController GameController;
        public CodeSequenceDisplay CodeDisplay;
        public TMP_Text PlaceholderText;
        public GameObject WorldRoot;
    }

    private sealed class BuildException : Exception
    {
        public List<string> Errors { get; }

        public BuildException(string message, List<string> errors)
            : base(message)
        {
            Errors = errors;
        }
    }
}
