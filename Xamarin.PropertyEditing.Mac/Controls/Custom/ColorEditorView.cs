using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class NotifyingView<TViewModel> : NSView, INotifyingListner<TViewModel> where TViewModel : NotifyingObject
	{
		internal TViewModel ViewModel
		{
			get => Adaptor.ViewModel;
			set => Adaptor.ViewModel = value;
		}

		public NotifyingView ()
		{
			Adaptor = new NotifyingViewAdaptor<TViewModel> (this);
		}

		public NotifyingView (CGRect frame) : base (frame)
		{
			Adaptor = new NotifyingViewAdaptor<TViewModel> (this);
		}

		public NotifyingView (IntPtr handle) : base (handle)
		{
		}

		[Export ("initWithCoder:")]
		public NotifyingView (NSCoder coder) : base (coder)
		{
		}

		protected NotifyingViewAdaptor<TViewModel> Adaptor { get; }

		public virtual void OnViewModelChanged (TViewModel oldModel)
		{
		}

		public virtual void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
		}
	}

	internal abstract class ColorEditorView : NotifyingView<SolidBrushViewModel>
	{
		protected const float Padding = 3;

		public ColorEditorView (IntPtr handle) : base (handle)
		{
		}


		public ColorEditorView (NSCoder coder) : base (coder)
		{
		}

		public ColorEditorView (CGRect frame) : base (frame)
		{
		}

		public ColorEditorView ()
		{
		}

		public new SolidBrushViewModel ViewModel {
			get => Adaptor.ViewModel;
			set => Adaptor.ViewModel = value;
		}

		public override void OnViewModelChanged (SolidBrushViewModel oldModel)
		{
			OnPropertyChanged (ViewModel, new PropertyChangedEventArgs (nameof (SolidBrushViewModel.Color)));
		}

		public override void MouseDragged (NSEvent theEvent)
		{
			//base.MouseDragged (theEvent);
			UpdateFromEvent (theEvent);
		}

		public override void MouseDown (NSEvent theEvent)
		{
			//base.MouseDown (theEvent);
			UpdateFromEvent (theEvent);
		}

		public virtual void UpdateFromEvent (NSEvent theEvent)
		{
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);

			if (!disposing)
				return;

			Adaptor.Dispose ();
		}
	}
}
