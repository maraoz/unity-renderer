using DCL.Controllers;
using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using UnityEngine;

public interface IBIWModeController : IBIWController
{
    event Action<BIWModeController.EditModeState, BIWModeController.EditModeState> OnChangedEditModeState;
    event Action OnInputDone;
    Vector3 GetCurrentEditionPosition();
    void CreatedEntity(BIWEntity entity);

    float GetMaxDistanceToSelectEntities() ;

    void EntityDoubleClick(BIWEntity entity);

    Vector3 GetMousePosition();

    Vector3 GetModeCreationEntryPoint();
    bool IsGodModeActive();
    void UndoEditionGOLastStep();
    void StartMultiSelection();

    void EndMultiSelection();
    void CheckInput();

    void CheckInputSelectedEntities();

    bool ShouldCancelUndoAction();
    void MouseClickDetected();
    void TakeSceneScreenshotForExit();
    void ActivateCamera(ParcelScene sceneToEdit);
    void OpenNewProjectDetails();
}

public class BIWModeController : BIWController, IBIWModeController
{
    public enum EditModeState
    {
        Inactive = 0,
        FirstPerson = 1,
        GodMode = 2
    }

    private GameObject cursorGO;
    private GameObject cameraParentGO;

    private IBIWActionController actionController;
    private IBIWEntityHandler entityHandler;

    private BIWFirstPersonMode firstPersonMode;
    public BIWGodMode godMode { get; set; }

    private InputAction_Trigger toggleSnapModeInputAction;

    public event Action OnInputDone;
    public event Action<EditModeState, EditModeState> OnChangedEditModeState;

    private EditModeState currentEditModeState = EditModeState.Inactive;

    private BIWMode currentActiveMode;

    private bool isSnapActive = false;

    private InputAction_Trigger.Triggered snapModeDelegate;

    private GameObject editionGO;
    private GameObject undoGO;
    private GameObject snapGO;
    private GameObject freeMovementGO;

    public override void Initialize(Context context)
    {
        base.Initialize(context);

        cursorGO = context.sceneReferences.cursorCanvas;
        cameraParentGO = context.sceneReferences.cameraParent;
        InitGameObjects();

        firstPersonMode = new BIWFirstPersonMode();
        godMode = new BIWGodMode();

        firstPersonMode.Init(context);
        godMode.Init(context);

        firstPersonMode.OnInputDone += InputDone;
        godMode.OnInputDone += InputDone;

        if ( context.editorContext.editorHUD != null)
        {
            context.editorContext.editorHUD.OnChangeModeAction += ChangeAdvanceMode;
            context.editorContext.editorHUD.OnResetAction += ResetScaleAndRotation;
            context.editorContext.editorHUD.OnChangeSnapModeAction += ChangeSnapMode;
        }

        actionController = context.editorContext.actionController;
        entityHandler = context.editorContext.entityHandler;
        toggleSnapModeInputAction = context.inputsReferencesAsset.toggleSnapModeInputAction;

        snapModeDelegate = (action) => ChangeSnapMode();
        toggleSnapModeInputAction.OnTriggered += snapModeDelegate;

        firstPersonMode.OnActionGenerated += actionController.AddAction;
        godMode.OnActionGenerated += actionController.AddAction;

        SetEditorGameObjects();
    }

    public override void Dispose()
    {
        base.Dispose();

        toggleSnapModeInputAction.OnTriggered -= snapModeDelegate;

        firstPersonMode.OnInputDone -= InputDone;
        godMode.OnInputDone -= InputDone;

        firstPersonMode.OnActionGenerated -= actionController.AddAction;
        godMode.OnActionGenerated -= actionController.AddAction;

        firstPersonMode.Dispose();
        godMode.Dispose();

        if ( context.editorContext.editorHUD != null)
        {
            context.editorContext.editorHUD.OnChangeModeAction -= ChangeAdvanceMode;
            context.editorContext.editorHUD.OnResetAction -= ResetScaleAndRotation;
        }

        GameObject.Destroy(undoGO);
        GameObject.Destroy(snapGO);
        GameObject.Destroy(editionGO);
        GameObject.Destroy(freeMovementGO);
    }

    public void ActivateCamera(ParcelScene sceneToLook) { godMode.ActivateCamera(sceneToLook); }

    public void TakeSceneScreenshotForExit() { godMode.TakeSceneScreenshotForExit(); }

    public void OpenNewProjectDetails() { godMode.OpenNewProjectDetails(); }

    private void SetEditorGameObjects()
    {
        godMode.SetEditorReferences(editionGO, undoGO, snapGO, freeMovementGO, entityHandler.GetSelectedEntityList());
        firstPersonMode.SetEditorReferences(editionGO, undoGO, snapGO, freeMovementGO, entityHandler.GetSelectedEntityList());
    }

    private void InitGameObjects()
    {
        if (snapGO == null)
            snapGO = new GameObject("SnapGameObject");

        if (freeMovementGO == null)
            freeMovementGO = new GameObject("FreeMovementGO");

        freeMovementGO.transform.SetParent(cameraParentGO.transform);

        if (editionGO == null)
            editionGO = new GameObject("EditionGO");

        editionGO.transform.SetParent(cameraParentGO.transform);

        if (undoGO == null)
            undoGO = new GameObject("UndoGameObject");
    }

    public bool IsGodModeActive() { return currentEditModeState == EditModeState.GodMode; }

    public Vector3 GetCurrentEditionPosition()
    {
        if (editionGO != null)
            return editionGO.transform.position;
        else
            return Vector3.zero;
    }

    public void UndoEditionGOLastStep()
    {
        if (undoGO == null || editionGO == null)
            return;

        BIWUtils.CopyGameObjectStatus(undoGO, editionGO, false, false);
    }

    public override void OnGUI()
    {
        base.OnGUI();
        godMode.OnGUI();
    }

    public override void Update()
    {
        base.Update();
        godMode.Update();
        firstPersonMode.Update();
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
        firstPersonMode.LateUpdate();
    }

    public override void EnterEditMode(IParcelScene scene)
    {
        base.EnterEditMode(scene);
        if (currentActiveMode == null)
            SetBuildMode(EditModeState.GodMode);
    }

    public override void ExitEditMode()
    {
        base.ExitEditMode();
        SetBuildMode(EditModeState.Inactive);
        snapGO.transform.SetParent(null);
    }

    public BIWMode GetCurrentMode() => currentActiveMode;

    public EditModeState GetCurrentStateMode() => currentEditModeState;

    private void InputDone() { OnInputDone?.Invoke(); }

    public void StartMultiSelection() { currentActiveMode.StartMultiSelection(); }

    public void EndMultiSelection() { currentActiveMode.EndMultiSelection(); }

    public void ResetScaleAndRotation() { currentActiveMode.ResetScaleAndRotation(); }

    public void CheckInput() { currentActiveMode?.CheckInput(); }

    public void CheckInputSelectedEntities() { currentActiveMode.CheckInputSelectedEntities(); }

    public bool ShouldCancelUndoAction() { return currentActiveMode.ShouldCancelUndoAction(); }

    public void CreatedEntity(BIWEntity entity) { currentActiveMode?.CreatedEntity(entity); }

    public float GetMaxDistanceToSelectEntities() { return currentActiveMode.maxDistanceToSelectEntities; }

    public void EntityDoubleClick(BIWEntity entity) {  currentActiveMode.EntityDoubleClick(entity); }

    public Vector3 GetMousePosition() { return currentActiveMode.GetPointerPosition(); }

    public Vector3 GetModeCreationEntryPoint()
    {
        if (currentActiveMode != null)
            return currentActiveMode.GetCreatedEntityPoint();
        return Vector3.zero;
    }

    public void MouseClickDetected() { currentActiveMode?.MouseClickDetected(); }

    private void ChangeSnapMode()
    {
        SetSnapActive(!isSnapActive);
        InputDone();

        if (isSnapActive)
            AudioScriptableObjects.enable.Play();
        else
            AudioScriptableObjects.disable.Play();
    }

    public void SetSnapActive(bool isActive)
    {
        isSnapActive = isActive;
        currentActiveMode.SetSnapActive(isActive);
    }

    public void ChangeAdvanceMode()
    {
        if (currentEditModeState == EditModeState.GodMode)
        {
            SetBuildMode(EditModeState.FirstPerson);
        }
        else
        {
            SetBuildMode(EditModeState.GodMode);
        }

        InputDone();
    }

    public void SetBuildMode(EditModeState state)
    {
        EditModeState previousState = currentEditModeState;

        if (currentActiveMode != null)
            currentActiveMode.Deactivate();

        currentActiveMode = null;
        switch (state)
        {
            case EditModeState.Inactive:
                break;
            case EditModeState.FirstPerson:
                currentActiveMode = firstPersonMode;

                if (cursorGO != null)
                    cursorGO.SetActive(true);
                break;
            case EditModeState.GodMode:
                if (cursorGO != null)
                    cursorGO.SetActive(false);
                currentActiveMode = godMode;
                break;
        }

        currentEditModeState = state;

        if (currentActiveMode != null)
        {
            currentActiveMode.Activate(sceneToEdit);
            currentActiveMode.SetSnapActive(isSnapActive);
            entityHandler.SetActiveMode(currentActiveMode);
        }

        OnChangedEditModeState?.Invoke(previousState, state);
    }
}