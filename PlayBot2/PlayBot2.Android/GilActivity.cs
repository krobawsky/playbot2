using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Bluetooth;
using System.IO;
using Java.Util;
using System.Threading.Tasks;
using Java.Lang;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using PlayBot2.Droid.Fragments;

namespace PlayBot2.Droid
{
    [Activity(Label = "", Icon = "@drawable/ic_consola", Theme = "@style/NoActionBarStyle", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    class GilActivity : AppCompatActivity
    {

        private DrawerLayout drawerLayout;
        private TextView mTitle;
        private ImageView mLogo;

        private Java.Lang.String dataToSend;

        private BluetoothAdapter btAdapter = null;
        private BluetoothSocket btSocket = null;

        private Stream outStream = null;
        private Stream inStream = null;

        string name;

        private static readonly UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RequestWindowFeature(WindowFeatures.NoTitle);
            RequestWindowFeature(WindowFeatures.IndeterminateProgress);

            SetContentView(Resource.Layout.gil);

            var toolBar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolBar);
            mTitle = (TextView)toolBar.FindViewById(Resource.Id.toolbar_title);
            mLogo = (ImageView)toolBar.FindViewById(Resource.Id.toolbar_logo);

            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.NavigationItemSelected += NavigationView_NavigationItemSelected;

            var drawerToggle = new Android.Support.V7.App.ActionBarDrawerToggle(this, drawerLayout, toolBar, Resource.String.open_drawer, Resource.String.close_drawer);
            drawerToggle.SyncState();

            string address = Intent.GetStringExtra("mac");
            name = Intent.GetStringExtra("name");

            Bundle args = new Bundle();
            args.PutString("address", address);

            var transaction = FragmentManager.BeginTransaction();
            FragmentControl fragment = new FragmentControl();
            fragment.Arguments = args;

            transaction.Replace(Resource.Id.viewpager, fragment).AddToBackStack(null).Commit();
            mTitle.SetText(Resource.String.nav_control);
            mLogo.SetImageResource(Resource.Drawable.ic_consola);

            Toast.MakeText(this, address, ToastLength.Short).Show();

            CheckBt();
            Connect(address);
            
        }

        private void NavigationView_NavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            var transaction = FragmentManager.BeginTransaction();
            var item = e.MenuItem.ItemId;

            if (item == Resource.Id.nav_control)
            {
                FragmentControl fragment = new FragmentControl();
                Bundle bd = new Bundle();
                var message = e.MenuItem.TitleFormatted.ToString();
                bd.PutString("address", message);
                fragment.Arguments = bd;
                transaction.Replace(Resource.Id.viewpager, fragment).AddToBackStack(null).Commit();

                mTitle.SetText(Resource.String.nav_control);
                mLogo.SetImageResource(Resource.Drawable.ic_consola);
            }
            else if (item == Resource.Id.nav_sensor)
            {
                FragmentSensores fragment = new FragmentSensores();
                Bundle bd = new Bundle();
                var message = e.MenuItem.TitleFormatted.ToString();
                bd.PutString("address", message);
                fragment.Arguments = bd;
                transaction.Replace(Resource.Id.viewpager, fragment).AddToBackStack(null).Commit();

                mTitle.SetText(Resource.String.nav_sensor);
                mLogo.SetImageResource(Resource.Drawable.ic_sensor);
            }
            else if (item == Resource.Id.nav_web)
            {
                FragmentSensores fragment = new FragmentSensores();
                Bundle bd = new Bundle();
                var message = e.MenuItem.TitleFormatted.ToString();
                bd.PutString("address", message);
                fragment.Arguments = bd;
                transaction.Replace(Resource.Id.viewpager, fragment).AddToBackStack(null).Commit();

                mTitle.SetText(Resource.String.nav_web);
                mLogo.SetImageResource(Resource.Drawable.ic_codigo);
            }

            drawerLayout.CloseDrawers();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    drawerLayout.OpenDrawer(Android.Support.V4.View.GravityCompat.Start);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
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

        public void Connect(string address)
        {

            if (address.Equals(""))
            {
                Toast.MakeText(this, "Direccion mac no encontrada, intentelo de nuevo.", ToastLength.Short).Show();
                StartActivity(typeof(MainActivity));
                Finish();
            }

            BluetoothDevice device = null;
            try
            {
                device = btAdapter.GetRemoteDevice(address);
                Console.WriteLine("Conexion en curso" + device);
            }
            catch (IllegalArgumentException e)
            {
                Console.WriteLine(e);
                Toast.MakeText(this, "La direccion mac " + address + " es incorrecta, intentelo de nuevo.", ToastLength.Short).Show();
                StartActivity(typeof(MainActivity));
                Finish();
            }

            // btAdapter.CancelDiscovery();
            try
            {
                btSocket = device.CreateRfcommSocketToServiceRecord(MY_UUID);
                btSocket.Connect();
                Toast.MakeText(this, "Conexion Correcta", ToastLength.Short).Show();

                dataToSend = new Java.Lang.String("Hola " + name);
                writeData(dataToSend);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
                Toast.MakeText(this, "Error en la conexion", ToastLength.Short).Show();

                StartActivity(typeof(MainActivity));
                Finish();

                try
                {
                    btSocket.Close();
                }
                catch (System.Exception)
                {
                    Console.WriteLine("Imposible Conectar");
                }
                Console.WriteLine("Socket Creado");
            }
        }

        public void beginListenForData()
        {
            try
            {
                inStream = btSocket.InputStream;
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }

            Task.Factory.StartNew(() => {

                byte[] buffer = new byte[1024];
                int bytes;
                while (true)
                {
                    try
                    {
                        bytes = inStream.Read(buffer, 0, buffer.Length);
                        if (bytes > 0)
                        {
                            RunOnUiThread(() => {
                                string valor = Encoding.ASCII.GetString(buffer);
                                Console.WriteLine("Datos recividos: " + valor);
                                Toast.MakeText(this, valor, ToastLength.Short).Show();
                            });
                        }
                    }
                    catch (Java.IO.IOException)
                    {
                        RunOnUiThread(() => {
                            Console.WriteLine("Error al recivir datos");
                        });
                        break;
                    }
                }
            });
        }

        private void writeData(Java.Lang.String data)
        {
            try
            {
                outStream = btSocket.OutputStream;
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Error al enviar" + e.Message);
            }

            Java.Lang.String message = data;
            byte[] msgBuffer = message.GetBytes();

            try
            {
                outStream.Write(msgBuffer, 0, msgBuffer.Length);
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Error al enviar" + e.Message);
            }
        }
    }
}