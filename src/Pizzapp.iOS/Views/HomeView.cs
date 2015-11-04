using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Touch.Views;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;
using CoreLocation;
using MapKit;
using System;
using System.Linq;
using AddressBookUI;
using Pizzapp.Core;

namespace Pizzapp
{
	[Register("HomeView")]
    public class HomeView : MvxViewController<HomeViewModel>
    {
        public HomeView(IntPtr handle)
            :base(handle)
        {
            new LoggingViewControllerAdapter (this);    
        }

        public HomeView ()
        {
            new LoggingViewControllerAdapter (this);    
        }

        CLGeocoder _geocoder;
        MKMapView _mapView;
        AddressBarView _addressBar;
        ConfirmationView _confirmation;
        StatusView _status;
        UIViewController _currentPresentedStep;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
         

            _mapView = new MapKit.MKMapView (View.Bounds);
            View.Add (_mapView);
            new GeocoderViewControllerAdapter (this, _mapView, ViewModel.NotifyAddressChanged);

            _addressBar = new AddressBarView ();
            AddChildViewController (_addressBar);
            View.Add (_addressBar.View);
            _addressBar.DidMoveToParentViewController (this);
            _currentPresentedStep = _addressBar;

            _confirmation = new ConfirmationView ();
            AddChildViewController (_confirmation);
            _confirmation.DidMoveToParentViewController (this);

            _status = new StatusView ();
            AddChildViewController (_status);
            _status.DidMoveToParentViewController (this);

			var set = this.CreateBindingSet<HomeView, Core.HomeViewModel>();

            set.Bind (_addressBar).For (v => v.DataContext).To (vm => vm.AddressBar);
            set.Bind (_confirmation).For (v => v.DataContext).To (vm => vm.Confirmation);
            set.Bind (_status).For (v => v.DataContext).To (vm => vm.Status);
            set.Bind ().For (v => v.Step).To (vm => vm.Step);
            set.Apply();
        }

        public override void ViewDidLayoutSubviews ()
        {
            base.ViewDidLayoutSubviews ();
            _addressBar.View.Frame = new CGRect (0, TopLayoutGuide.Length, View.Bounds.Width, _addressBar.View.Bounds.Height);

        }


        private OrderStep _step;
        public OrderStep Step
        {
            get
            {
                return _step;
            }
            set
            {
                var oldStep = _step;
                _step = value;
                HandleStepChanged (oldStep, value);
            }

        }

        private async void HandleStepChanged(OrderStep oldStep, OrderStep newStep)
        {
            var fromController = _currentPresentedStep;
            var toController = default(UIViewController);

            if (newStep == OrderStep.EnterDeliveryAddress)
            {
                toController = _addressBar;
            }
            else if (newStep == OrderStep.ConfirmDelivery)
            {
                toController = _confirmation;

            }
            else if (newStep == OrderStep.AwaitingDelivery)
            {
                toController = _status;
            }
            else
            {
                throw new InvalidOperationException ();
            }   

            if (toController == fromController)
            {
                return;
            }

            toController.View.Frame = new CGRect (0, TopLayoutGuide.Length, View.Bounds.Width, toController.View.Bounds.Height);
            await this.TransitionAsync (fromController, toController, .6, UIViewAnimationOptions.TransitionCrossDissolve, () => {

            });

            _currentPresentedStep = toController;
        }


    }
}