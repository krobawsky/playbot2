using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace PlayBot2.Droid.Fragments
{
    public class FragmentControl : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.FragmentControl, container, false);

            String address = Arguments.GetString("address");
            SetUpFragment(view, address);

            return view;
        }

        private void SetUpFragment(View view, String address)
        {

            ImageButton btnArriba = view.FindViewById<ImageButton>(Resource.Id.img_arriba);
            ImageButton btnAbajo = view.FindViewById<ImageButton>(Resource.Id.img_abajo);
            ImageButton btnIzquierda = view.FindViewById<ImageButton>(Resource.Id.img_izquierda);
            ImageButton btnDerecha = view.FindViewById<ImageButton>(Resource.Id.img_derecha);

            ImageButton btnMicrofono = view.FindViewById<ImageButton>(Resource.Id.img_microfono);
            ImageButton btnPantera = view.FindViewById<ImageButton>(Resource.Id.img_pantera);
            ImageButton btnChavo = view.FindViewById<ImageButton>(Resource.Id.img_chavo);

            btnArriba.Click += (sender, e) =>
            {
                Toast.MakeText(view.Context, "Arriba", ToastLength.Short).Show();
            };

            btnAbajo.Click += (sender, e) =>
            {
                Toast.MakeText(view.Context, "Abajo", ToastLength.Short).Show();
            };

            btnIzquierda.Click += (sender, e) =>
            {
                Toast.MakeText(view.Context, "Izquierda", ToastLength.Short).Show();
            };

            btnDerecha.Click += (sender, e) =>
            {
                Toast.MakeText(view.Context, "Derecha", ToastLength.Short).Show();
            };

            btnMicrofono.Click += (sender, e) =>
            {
                Toast.MakeText(view.Context, address, ToastLength.Short).Show();
            };

            btnPantera.Click += (sender, e) =>
            {
                Toast.MakeText(view.Context, "Pantera rosa", ToastLength.Short).Show();
            };

            btnChavo.Click += (sender, e) =>
            {
                Toast.MakeText(view.Context, "Chavo", ToastLength.Short).Show();
            };

        }
    }
}