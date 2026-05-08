using UnityEngine.UI;

public class FreeControllerV3UI : UIProvider
{
	public Button selectRootButton;

	public Text UIDText;

	public ToggleGroupValue positionToggleGroup;

	public ToggleGroupValue rotationToggleGroup;

	public Slider holdPositionSpringSlider;

	public Slider holdPositionDamperSlider;

	public Slider holdPositionMaxForceSlider;

	public Slider holdRotationSpringSlider;

	public Slider holdRotationDamperSlider;

	public Slider holdRotationMaxForceSlider;

	public Slider complyPositionSpringSlider;

	public Slider complyPositionDamperSlider;

	public Slider complyRotationSpringSlider;

	public Slider complyRotationDamperSlider;

	public Slider complyJointRotationDriveSpringSlider;

	public Slider complyPositionThresholdSlider;

	public Slider complyRotationThresholdSlider;

	public Slider complySpeedSlider;

	public Slider linkPositionSpringSlider;

	public Slider linkPositionDamperSlider;

	public Slider linkPositionMaxForceSlider;

	public Slider linkRotationSpringSlider;

	public Slider linkRotationDamperSlider;

	public Slider linkRotationMaxForceSlider;

	public UIPopup linkToSelectionPopup;

	public UIPopup linkToAtomSelectionPopup;

	public Slider jointRotationDriveSpringSlider;

	public Slider jointRotationDriveDamperSlider;

	public Slider jointRotationDriveMaxForceSlider;

	public Slider jointRotationDriveXTargetSlider;

	public Slider jointRotationDriveYTargetSlider;

	public Slider jointRotationDriveZTargetSlider;

	public Button selectLinkToFromSceneButton;

	public Button selectAlignToFromSceneButton;

	public Toggle onToggle;

	public Slider massSlider;

	public Slider dragSlider;

	public Slider angularDragSlider;

	public Toggle physicsEnabledToggle;

	public Toggle collisionEnabledToggle;

	public Toggle useGravityWhenOffToggle;

	public Toggle interactableInPlayModeToggle;

	public Toggle possessableToggle;

	public Toggle canGrabPositionToggle;

	public Toggle canGrabRotationToggle;

	public SetTextFromFloat xPositionText;

	public Button xPositionMinus1Button;

	public Button xPositionMinusPoint1Button;

	public Button xPositionMinusPoint01Button;

	public Button xPosition0Button;

	public Button xPositionPlusPoint01Button;

	public Button xPositionPlusPoint1Button;

	public Button xPositionPlus1Button;

	public Button xPositionSnapPoint1Button;

	public Toggle xPositionLockToggle;

	public Toggle xPositionLocalLockToggle;

	public SetTextFromFloat yPositionText;

	public Button yPositionMinus1Button;

	public Button yPositionMinusPoint1Button;

	public Button yPositionMinusPoint01Button;

	public Button yPosition0Button;

	public Button yPositionPlusPoint01Button;

	public Button yPositionPlusPoint1Button;

	public Button yPositionPlus1Button;

	public Button yPositionSnapPoint1Button;

	public Toggle yPositionLockToggle;

	public Toggle yPositionLocalLockToggle;

	public SetTextFromFloat zPositionText;

	public Button zPositionMinus1Button;

	public Button zPositionMinusPoint1Button;

	public Button zPositionMinusPoint01Button;

	public Button zPosition0Button;

	public Button zPositionPlusPoint01Button;

	public Button zPositionPlusPoint1Button;

	public Button zPositionPlus1Button;

	public Button zPositionSnapPoint1Button;

	public Toggle zPositionLockToggle;

	public Toggle zPositionLocalLockToggle;

	public SetTextFromFloat xRotationText;

	public Button xRotationMinus45Button;

	public Button xRotationMinus5Button;

	public Button xRotationMinusPoint5Button;

	public Button xRotation0Button;

	public Button xRotationPlusPoint5Button;

	public Button xRotationPlus5Button;

	public Button xRotationPlus45Button;

	public Button xRotationSnap1Button;

	public Toggle xRotationLockToggle;

	public SetTextFromFloat yRotationText;

	public Button yRotationMinus45Button;

	public Button yRotationMinus5Button;

	public Button yRotationMinusPoint5Button;

	public Button yRotation0Button;

	public Button yRotationPlusPoint5Button;

	public Button yRotationPlus5Button;

	public Button yRotationPlus45Button;

	public Button yRotationSnap1Button;

	public Toggle yRotationLockToggle;

	public SetTextFromFloat zRotationText;

	public Button zRotationMinus45Button;

	public Button zRotationMinus5Button;

	public Button zRotationMinusPoint5Button;

	public Button zRotation0Button;

	public Button zRotationPlusPoint5Button;

	public Button zRotationPlus5Button;

	public Button zRotationPlus45Button;

	public Button zRotationSnap1Button;

	public Toggle zRotationLockToggle;

	public UIPopup positionGridModePopup;

	public Slider positionGridSlider;

	public UIPopup rotationGridModePopup;

	public Slider rotationGridSlider;
}
