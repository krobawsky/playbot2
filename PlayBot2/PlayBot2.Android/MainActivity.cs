using System;

using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using Android.Content;
using Android;
using Android.Support.V4.Content;
using Android.Support.V4.App;
using Android.Support.V7.App;
using V7Toolbar = Android.Support.V7.Widget.Toolbar;

namespace PlayBot2.Droid
{
    [Activity(Label = "PlayBot", Icon = "@drawable/ic_consola", Theme = "@style/NoActionBarStyle", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : AppCompatActivity
    {
        
        private BluetoothAdapter btAdapter = null;
        static ArrayAdapter<string> newDevicesArrayAdapter;
        static ArrayAdapter<string> pairedDevicesArrayAdapter;
        private DeviceDiscoveredReceiver receiver;

        private Button devButton;
        private Button scanButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RequestWindowFeature(WindowFeatures.IndeterminateProgress);
            SetContentView(Resource.Layout.main);

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = GetString(Resource.String.dev_pair);

            permissions();
            CheckBt();

            SetResult(Result.Canceled);

            newDevicesArrayAdapter = new ArrayAdapter<string>(this, Resource.Layout.device_name);
            pairedDevicesArrayAdapter = new ArrayAdapter<string>(this, Resource.Layout.device_name);
            
            var DevicesListView = FindViewById<ListView>(Resource.Id.list_devices);
            DevicesListView.ItemClick += DeviceListView_ItemClick;

            devButton = FindViewById<Button>(Resource.Id.btn_dev);
            devButton.Click += (sender, e) =>
            {
                SupportActionBar.Title = GetString(Resource.String.dev_pair);
                DevicesListView.Adapter = pairedDevicesArrayAdapter;
                pairedDevicesArrayAdapter.Clear();

                DevicePair();
                (sender as View).Visibility = ViewStates.Gone;
                scanButton.Visibility = ViewStates.Visible;
            };


            scanButton = FindViewById<Button>(Resource.Id.btn_scan);
            scanButton.Click += (sender, e) =>
            {
                SupportActionBar.Title = GetString(Resource.String.dev_search);
                DevicesListView.Adapter = newDevicesArrayAdapter;
                newDevicesArrayAdapter.Clear();

                DoDiscovery();
                (sender as View).Visibility = ViewStates.Gone;
            };

            DevicesListView.Adapter = pairedDevicesArrayAdapter;
            DevicePair();

            receiver = new DeviceDiscoveredReceiver(this, devButton);

            var filter = new IntentFilter(BluetoothDevice.ActionFound);
            RegisterReceiver(receiver, filter);

            filter = new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished);
            RegisterReceiver(receiver, filter);

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (btAdapter != null)
            {
                btAdapter.CancelDiscovery();
            }
            UnregisterReceiver(receiver);
        }

        private void CheckBt()
        {
            btAdapter = BluetoothAdapter.DefaultAdapter;

            if (!btAdapter.Enable())
            {
                Toast.MakeText(this, "Bluetooth Desactivado", ToastLength.Short).Show();
            }
            if (btAdapter == null)
            {
                Toast.MakeText(this, "Bluetooth No Existe o esta Ocupado", ToastLength.Short).Show();
            }
        }

        void DevicePair()
        {
            if (btAdapter.IsDiscovering)
            {
                btAdapter.CancelDiscovery();
            }

            var pairedDevices = btAdapter.BondedDevices;

            if (pairedDevices.Count > 0)
            {
                foreach (var dev in pairedDevices)
                {
                    pairedDevicesArrayAdapter.Add(dev.Name + "\n" + dev.Address);
                }
            }
        }

        void DoDiscovery()
        {
            if (btAdapter.IsDiscovering)
            {
                btAdapter.CancelDiscovery();
            }
            btAdapter.StartDiscovery();
            //progress(true);
        }

        void DeviceListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //btAdapter.CancelDiscovery();
            var info = ((TextView)e.View).Text;
            var address = info.Substring(info.Length - 17);
            var name = info.Substring(0, info.Length - 17);

            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
            Android.App.AlertDialog alert = dialog.Create();
            alert.SetTitle("PlayBot");
            alert.SetMessage("Desea conectarse con el dispositivo " + name);
            alert.SetButton("OK", (c, ev) =>
            {
                var activity = new Intent(this, typeof(GilActivity));
                activity.PutExtra("name", name);
                activity.PutExtra("mac", address);
                StartActivity(activity);
            });
            alert.Show();
            
        }

        public class DeviceDiscoveredReceiver : BroadcastReceiver
        {
            Activity activity;
            Button devButton;

            public DeviceDiscoveredReceiver(Activity activity, Button button)
            {
                this.activity = activity;
                devButton = button;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.Action;
                
                if (action == BluetoothDevice.ActionFound)
                {
                    BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                    if (device.BondState != Bond.Bonded)
                    {
                        newDevicesArrayAdapter.Add(device.Name + "\n" + device.Address);
                    }
                }
                else if (action == BluetoothAdapter.ActionDiscoveryFinished)
                {
                    Console.WriteLine("Finish");
                    Toast.MakeText(activity, "Busqueda finalizada", ToastLength.Short).Show();
                    devButton.Visibility = ViewStates.Visible;
                }
            }
        }

        public void progress(bool x)
        {
            var progress = new ProgressDialog(this);
            if (x)
            {
                progress.Indeterminate = true;
                progress.SetProgressStyle(ProgressDialogStyle.Spinner);
                progress.SetMessage("Loading is Progress...");
                progress.SetCancelable(false);
                progress.Show();
            }
            else
            {
                progress.Dismiss();
            }
            
        }

        public void permissions()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation,
                    Manifest.Permission.Bluetooth, Manifest.Permission.BluetoothAdmin}, 0);
            }
            else
            {
                Console.WriteLine("Permission Granted!!!");
            }
        }
    }
}