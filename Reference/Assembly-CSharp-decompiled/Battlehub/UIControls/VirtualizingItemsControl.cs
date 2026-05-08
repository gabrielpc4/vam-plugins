using System;

namespace Battlehub.UIControls;

public class VirtualizingItemsControl<TDataBindingArgs> : VirtualizingItemsControl where TDataBindingArgs : ItemDataBindingArgs, new()
{
	public event EventHandler<TDataBindingArgs> ItemDataBinding;

	public event EventHandler<TDataBindingArgs> ItemBeginEdit;

	public event EventHandler<TDataBindingArgs> ItemEndEdit;

	protected override void OnItemBeginEdit(object sender, EventArgs e)
	{
		if (CanHandleEvent(sender))
		{
			VirtualizingItemContainer virtualizingItemContainer = (VirtualizingItemContainer)sender;
			if (this.ItemBeginEdit != null)
			{
				TDataBindingArgs e2 = new TDataBindingArgs
				{
					Item = virtualizingItemContainer.Item,
					ItemPresenter = ((!(virtualizingItemContainer.ItemPresenter == null)) ? virtualizingItemContainer.ItemPresenter : base.gameObject),
					EditorPresenter = ((!(virtualizingItemContainer.EditorPresenter == null)) ? virtualizingItemContainer.EditorPresenter : base.gameObject)
				};
				this.ItemBeginEdit(this, e2);
			}
		}
	}

	protected override void OnItemEndEdit(object sender, EventArgs e)
	{
		if (CanHandleEvent(sender))
		{
			VirtualizingItemContainer virtualizingItemContainer = (VirtualizingItemContainer)sender;
			if (this.ItemBeginEdit != null)
			{
				TDataBindingArgs e2 = new TDataBindingArgs
				{
					Item = virtualizingItemContainer.Item,
					ItemPresenter = ((!(virtualizingItemContainer.ItemPresenter == null)) ? virtualizingItemContainer.ItemPresenter : base.gameObject),
					EditorPresenter = ((!(virtualizingItemContainer.EditorPresenter == null)) ? virtualizingItemContainer.EditorPresenter : base.gameObject)
				};
				this.ItemEndEdit(this, e2);
			}
		}
	}

	public override void DataBindItem(object item, ItemContainerData itemContainerData, VirtualizingItemContainer itemContainer)
	{
		TDataBindingArgs args = new TDataBindingArgs
		{
			Item = item,
			ItemPresenter = ((!(itemContainer.ItemPresenter == null)) ? itemContainer.ItemPresenter : base.gameObject),
			EditorPresenter = ((!(itemContainer.EditorPresenter == null)) ? itemContainer.EditorPresenter : base.gameObject)
		};
		itemContainer.Clear();
		RaiseItemDataBinding(args);
		itemContainer.CanEdit = args.CanEdit;
		itemContainer.CanDrag = args.CanDrag;
		itemContainer.CanDrop = args.CanDrop;
	}

	protected void RaiseItemDataBinding(TDataBindingArgs args)
	{
		if (this.ItemDataBinding != null)
		{
			this.ItemDataBinding(this, args);
		}
	}
}
