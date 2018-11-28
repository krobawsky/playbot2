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
using V7Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Bluetooth;
using System.IO;
using Java.Util;
using System.Threading.Tasks;
using Java.Lang;

namespace PlayBot2.Droid
{
    [Activity(Label = "PlayBot", Theme = "@style/MainTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    class GilActivity : AppCompatActivity
    {

        private Java.Lang.String dataToSend;

        private BluetoothAdapter btAdapter = null;
        private BluetoothSocket btSocket = null;

        private Stream outStream = null;
        private Stream inStream = null;

        private static readonly UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RequestWindowFeature(WindowFeatures.IndeterminateProgress);
            SetContentView(Resource.Layout.control);

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            string address = Intent.GetStringExtra("mac");
            string name = Intent.GetStringExtra("name");

            SupportActionBar.Title = name;

            Toast.MakeText(this, address, ToastLength.Short).Show();

            CheckBt();
            Connect(address);
            
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