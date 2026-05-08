using UnityEngine;

namespace Leap.Unity.Attachments;

public class AttachmentHandEnableDisable : MonoBehaviour
{
	public AttachmentHand attachmentHand;

	private void Update()
	{
		if (!attachmentHand.isTracked && attachmentHand.gameObject.activeSelf)
		{
			attachmentHand.gameObject.SetActive(value: false);
		}
		if (attachmentHand.isTracked && !attachmentHand.gameObject.activeSelf)
		{
			attachmentHand.gameObject.SetActive(value: true);
		}
	}
}
