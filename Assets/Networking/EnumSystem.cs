using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnumSystem
{
    public enum TranmissionType
    {
        realtime,
        cache,
    }

    public enum LineState
    {
        Active, // Line is visible
        Hidden  // Line is hidden
    }
    public enum Action
    {
        LocalCreation = 0,
        LocalDeletion = 1,
        LocalModification = 2,
        InteractiveDeletion = 3,
        InteractiveModification = 4
    }
    public enum CreationType
    {
        SelfCreation = 0,
        Received = 1,
        Topdown = 2,
    }

    public enum ContainerType
    {
        SelfCreationContainer = 0,
        ReceivedContainer = 1,
        TopdownContainer = 2,
    }
    public enum MessageType
    {
        myMessage = 0,
        otherMessage = 1,
        systemMessage = 2,
        pluginMessage = 3,
        RedoUndoMessage = 4,
    }
    public enum PenType
    {
        MaliangPen,
        ControllerPen
    }

    public enum SketchMode
    {
        UnableSketch,
        Sketching_in_2D,
        Sketching_in_3D,
        Sketching_in_3D_Maliang,
    }

    public enum SketchCollider
    {
        In,
        Out,
    }
    public enum HeightAdjustmentMode
    {
        RelativeAdjustment,
        AbsoluteAdjustment,
        //AbsoluteAdjustment3D,
    }

    public enum HeightAdjustmentType
    {
        ToLowest,
        ToHighest,
        ToSelected,
        ToNext,
    }
    public enum LineType
    {
        freeSketch = 0,
        area = 1
    }
    public enum AreaSketchType
    {
        Fire = 0,
        Damage = 1,
        None = 2
    }
    public enum HeightAdjustmentHigherAndLowerType
    {
        AllDontMove,
        OneDontMove,
    }

    public enum ElevatorType
    {
        Up,
        Down,
    }

    public enum RenderLayerCameraMode
    {
        AllLayer,
        LastActivate2DLayer,
    }

    public enum SnapControlPointMode
    {
        All,
        End,
    }
}
