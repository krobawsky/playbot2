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
    public class FragmentSensores : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.FragmentSensores, container, false);

            String address = Arguments.GetString("address");
            SetUpFragment(view, address);

            return view;
        }

        private void SetUpFragment(View view, String address)
        {

        }
    }
}